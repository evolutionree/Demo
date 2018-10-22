using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using UBeat.Crm.CoreApi.DingTalk.Models;
using UBeat.Crm.CoreApi.DingTalk.Repository;
using UBeat.Crm.CoreApi.DingTalk.Utils;
using UBeat.Crm.CoreApi.DomainModel.Account;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.CoreApi.Utility;
using NLog;
using System.Linq;


namespace UBeat.Crm.CoreApi.DingTalk.Services
{
    public class DingTalkAdressBookServices : EntityBaseServices
    {
        private readonly IDingTalkRepository _dingTalk;
        public DingTalkAdressBookServices(IDingTalkRepository dingTalk)
        {
            _dingTalk = dingTalk;
        }
        public List<DingTalkDeptInfo> GetContactList()
        {
            string Access_token = DingTalkTokenUtils.GetAccessToken();
            string url = string.Format("{0}?access_token={1}&fetch_child=true", DingTalkUrlUtils.ListDeptsUrl(), Access_token);
            string response = DingTalkHttpUtils.HttpGet(url);
            Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);
            if (result.ContainsKey("department") && result["department"] != null)
            {
                return JsonConvert.DeserializeObject<List<DingTalkDeptInfo>>(JsonConvert.SerializeObject(result["department"]));
            }
            return null;
        }
        /// <summary>
        /// 获取部门下的用户
        /// </summary>
        /// <param name="deptid"></param>
        /// <returns></returns>
        public DingTalkListUsersByDeptIdInfo GetUsersByDept(long deptid)
        {
            string Access_token = DingTalkTokenUtils.GetAccessToken();
            string url = string.Format("{0}?access_token={1}&deptId={2}", DingTalkUrlUtils.ListUsersByDeptId(), Access_token, deptid);
            string response = DingTalkHttpUtils.HttpGet(url);
            return JsonConvert.DeserializeObject<DingTalkListUsersByDeptIdInfo>(response);
        }
        public GetUserInfoResponseInfo GetUserInfo(string userid)
        {
            string Access_token = DingTalkTokenUtils.GetAccessToken();
            string url = string.Format("{0}?access_token={1}&userid={2}", DingTalkUrlUtils.GetUserInfo_Url(), Access_token, userid);
            string response = DingTalkHttpUtils.HttpGet(url);
            return JsonConvert.DeserializeObject<GetUserInfoResponseInfo>(response);
        }

        public AccountUserMapper LoginWithDingTalkCode(string code)
        {
            string Access_token = DingTalkTokenUtils.GetAccessToken();
            string ddUserUrl = DingTalkUrlUtils.GetUserId() + "?access_token=" + Access_token + "&code=" + code;
            string response = DingTalkHttpUtils.HttpGet(ddUserUrl);
            var ddUserInfo = JsonConvert.DeserializeObject<DingTalkUserInfo>(response);
            var userData = _dingTalk.GetUserInfoforDingding(ddUserInfo.userid);
            return userData;
        }

        public OutputResult<object> GetEntranceList()
        {
            var rusult = _dingTalk.GetEntranceList();
            return new OutputResult<object>(rusult);
        }


        public AccountUserMapper SSOLoginWithDingTalkCode(string code)
        {
            try
            {
                string Access_token = DingTalkTokenUtils.GetSSOAccessToken();
                string ddUserUrl = DingTalkUrlUtils.GetSSOUserId() + "?access_token=" + Access_token + "&code=" + code;
                string response = DingTalkHttpUtils.HttpGet(ddUserUrl);
                var ddUserInfo = JsonConvert.DeserializeObject<DingTalkSSOUserInfo>(response);
                var userData = _dingTalk.GetUserInfoforDingding(ddUserInfo.user_info.userid);
                return userData;

            }
            catch (Exception ex)
            {

                return new AccountUserMapper();
            }


        }



        private static ILogger logger = LogManager.GetCurrentClassLogger();

        public AccountUserMapper ScanLoginWithDingTalkCode(string tmpAuthcode)
        {

            //获取钉钉开放应用的ACCESS_TOKEN
            string accessToken = DingTalkTokenUtils.GetSnsAccessToken();

            //获取用户授权的持久授权码
            PersistentResponse persistentCodeResponse = DingTalkTokenUtils.GetPersistentCode(accessToken, tmpAuthcode);

            //获取用户授权的SNS_TOKEN
            string snsToken = DingTalkTokenUtils.GetAuthSnsToken(accessToken, persistentCodeResponse.openid, persistentCodeResponse.persistent_code);

            //获取用户授权的个人信息
            var userinfo = DingTalkTokenUtils.GetSnsUserInfo(snsToken);

            //获取crm用户信息
            if (userinfo != null && userinfo.user_info != null && userinfo.user_info.nick != null)
            {
                var userData = _dingTalk.GetUserInfoforDingdingByNick(userinfo.user_info.nick);
                return userData;
            }
            else
            {
                return new AccountUserMapper();

            }

        }
        public void SynchDeptWithDingtalk()
        {
            List<DingTalkDeptInfo> ListOfDepts = this.GetContactList();
            if (ListOfDepts == null || ListOfDepts.Count == 0) return;
            DingTalkDeptInfo RootDept = null;
            Dictionary<long, DingTalkDeptInfo> allDept = new Dictionary<long, DingTalkDeptInfo>();
            foreach (DingTalkDeptInfo dept in ListOfDepts)
            {
                allDept.Add(dept.Id, dept);
            }
            foreach (DingTalkDeptInfo dept in ListOfDepts)
            {
                if (dept.Id == 1)
                {
                    RootDept = dept;
                }
                if (dept.ParentId > 0)
                {
                    if (allDept.ContainsKey(dept.ParentId))
                    {
                        allDept[dept.ParentId].SubDepts.Add(dept);
                    }
                }
            }


            if (RootDept == null || RootDept.SubDepts == null || RootDept.SubDepts.Count == 0) return;
            Guid rootDeptId = new Guid("7f74192d-b937-403f-ac2a-8be34714278b");
            SynchDepts(rootDeptId, RootDept.SubDepts);
            RootDept.CRMRecId = rootDeptId;
            foreach (DingTalkDeptInfo dept in ListOfDepts)
            {
                SynchUserInDept(dept);
            }
        }

        private void SynchDepts(Guid parentId, List<DingTalkDeptInfo> depts)
        {
            foreach (DingTalkDeptInfo dept in depts)
            {
                SynchOneDept(parentId, dept);
            }

        }
        private void SynchOneDept(Guid parentId, DingTalkDeptInfo deptInfo)
        {
            Guid thisDeptId = Guid.Empty;
            deptInfo.CRMRecId = thisDeptId;
            //检查重复的问题
            if (deptInfo.SubDepts != null && deptInfo.SubDepts.Count > 0)
                SynchDepts(thisDeptId, deptInfo.SubDepts);

        }
        private void SynchUserInDept(DingTalkDeptInfo dept)
        {

        }
        //同步一个用户
        private void SynchOneUser()
        {

        }

        //

        #region  同步部门和用户数据


        public GetDepartmentUserResponse GetUserBYDepartmentId(long departmentId)
        {
            string accessToken = DingTalkTokenUtils.GetAccessToken();
            return DingTalkTokenUtils.GetUserListByDepartmentId(accessToken, departmentId);
        }

        public void SynDingTalkDepartment()
        {
            List<DingTalkDeptInfo> dingTalkDepartment = GetContactList();
            var rootDepartment = dingTalkDepartment.Where(x => x.ParentId == 0).FirstOrDefault();
            rootDepartment.CRMRecId = new Guid("7f74192d-b937-403f-ac2a-8be34714278b");

            SynDingTalkDepartmentRecursive(dingTalkDepartment, rootDepartment.Id);
        }



        public void SynDingTalkDepartmentRecursive(List<DingTalkDeptInfo> departmentList, long departmentId)
        {
            var subDepartments = departmentList.Where(x => x.ParentId == departmentId);
            var parentDepartment = departmentList.Where(x => x.Id == departmentId).FirstOrDefault();
            foreach (var item in subDepartments)
            {
                SynDepartment(item, parentDepartment.CRMRecId);
                SynDingTalkDepartmentRecursive(departmentList, item.Id);
            }
        }

        public void SynDepartment(DingTalkDeptInfo subItem, Guid topCrmDepartmentId)
        {
            int administartorUserId = 1;
            int userNumber = 1;

            bool isDepartmetnExist = _dingTalk.IsDepartmentExist(subItem.Name, subItem.Id);
            var departmentUsers = GetUserBYDepartmentId(subItem.Id);
            if (!isDepartmetnExist)
            {
                _dingTalk.DepartmentAdd(topCrmDepartmentId, subItem.Name, 0, administartorUserId, subItem.Id, subItem.ParentId);
                Guid departmentId = _dingTalk.GetDepartmentId(subItem.Name, subItem.Id);
                subItem.CRMRecId = departmentId;

                if (departmentUsers != null && departmentUsers.userlist != null && departmentUsers.userlist.Count > 0)
                {
                    foreach (var userItem in departmentUsers.userlist)
                    {
                        SynUser(userItem, departmentId, userNumber);
                    }
                }
            }
            else
            {
                //get department id
                Guid _departmentId = _dingTalk.GetDepartmentId(subItem.Name, subItem.Id);
                subItem.CRMRecId = _departmentId;
                foreach (var userItem in departmentUsers.userlist)
                {
                    SynUser(userItem, _departmentId, userNumber);
                }
            }
        }


        public void SynUser(DingTalkUserinfo userItem, Guid departmentId, int userNumber)
        {
            bool isUserExist = _dingTalk.IsUserExist(userItem.mobile);
            if (!isUserExist)
            {
                AccountUserRegistMapper _account = new AccountUserRegistMapper()
                {
                    AccountName = userItem.mobile,
                    AccountPwd = string.Empty,
                    AccessType = "00",
                    UserName = userItem.name,
                    UserIcon = "",
                    UserPhone = userItem.mobile,
                    UserJob = string.Empty,
                    DeptId = departmentId,
                    Email = userItem.email,
                    BirthDay = DateTime.Now,
                    JoinedDate = DateTime.Now,
                    Remark = string.Empty,
                    Sex = 1,
                    Tel = userItem.tel,
                    Status = 1,
                    WorkCode = string.Empty,
                    NextMustChangePwd = 1
                };

                _dingTalk.UserAdd(_account, userItem.userid, userItem.name, userNumber);

            }
        }



        #endregion




    }
}
