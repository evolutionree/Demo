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
        private ILogger _logger = NLog.LogManager.GetLogger("UBeat.Crm.CoreApi.DingTalk.Services.DingTalkAdressBookServices");
        public DingTalkAdressBookServices(IDingTalkRepository dingTalk)
        {
            _dingTalk = dingTalk;
        }
        public List<DingTalkDeptInfo> ListDingTalkDepartments(string parentid)
        {

            string Access_token = DingTalkTokenUtils.GetAccessToken();
            string url = string.Format("{0}?access_token={1}&fetch_child=false&id={2}", DingTalkUrlUtils.ListDeptsUrl(), Access_token, parentid);
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
            _logger.Log(LogLevel.Fatal, JsonConvert.SerializeObject(response));
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
            List<DingTalkDeptInfo> ListOfDepts = this.ListDingTalkDepartments(DingTalkConfig.getInstance().RootDeptId);
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
            foreach (var tmp in DingTalkConfig.getInstance().mapperDeptIds)
            {
                List<DingTalkDeptInfo> dingTalkDepartment = ListDingTalkDepartments(tmp.DingDingDeptId);
                DingTalkDeptInfo rootDepartment = dingTalkDepartment.Where(x => x.Id.ToString() == tmp.DingDingDeptId).FirstOrDefault();
                if (rootDepartment == null)
                {
                    rootDepartment = new DingTalkDeptInfo()
                    {
                        Id = int.Parse(tmp.DingDingDeptId),
                        Name = "金雅福集团"
                    };
                    dingTalkDepartment.Add(rootDepartment);
                }
                rootDepartment.CRMRecId = new Guid(tmp.CrmDeptId);

                SynDingTalkDepartmentRecursive(dingTalkDepartment, rootDepartment.CRMRecId);
            }
        }



        public void SynDingTalkDepartmentRecursive(List<DingTalkDeptInfo> subDepartments, Guid parentId)
        {
            foreach (var item in subDepartments)
            {
                Guid deptid = SynDepartment(item, parentId);
                List<DingTalkDeptInfo> sub = this.ListDingTalkDepartments(item.Id.ToString());
                SynDingTalkDepartmentRecursive(sub, deptid);
            }
        }

        public Guid SynDepartment(DingTalkDeptInfo subItem, Guid topCrmDepartmentId)
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
                return departmentId;
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
                return _departmentId;
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

        public bool GetRoleList(int userId)
        {
            string accessToken = DingTalkTokenUtils.GetAccessToken();
            var collection = DingTalkTokenUtils.GetRoleList(accessToken);
            foreach (var tmp in collection.RoleRelations)
            {
                Guid groupId = _dingTalk.AddGroup(tmp.Group.GroupName, tmp.Group.GroupId, userId);
                foreach (var tmp1 in tmp.RoleList.RoleList)
                {
                    Guid roleId = _dingTalk.AddRole(tmp1.RoleName, tmp1.Id, userId);
                    _dingTalk.AddRoleGroup(groupId, roleId, tmp.Group.GroupId, tmp1.Id, userId);
                }
            }
            return true;
        }


    }
}
