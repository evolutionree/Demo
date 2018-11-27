
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
namespace UBeat.Crm.CoreApi.DingTalk.Utils
{
    public class DingTalkTokenUtils
    {
        /// <summary>
        /// 获取AccessToken
        /// </summary>
        /// <returns></returns>
        public static string GetAccessToken()
        {
            string url = string.Format("{0}?corpid={1}&corpsecret={2}", DingTalkUrlUtils.GetTokenUrl(), DingTalkConfig.getInstance().CorpId, DingTalkConfig.getInstance().CorpSecret);
            string responsestring = DingTalkHttpUtils.HttpGet(url);
            Dictionary<string, string> result = JsonConvert.DeserializeObject<Dictionary<string, string>>(responsestring);
            string errorCode = "";
            string errorMsg = "";
            string Access_Token = "";
            if (result.ContainsKey("errcode") && result["errcode"] != null)
                errorCode = result["errcode"].ToString();
            if (result.ContainsKey("errmsg") && result["errmsg"] != null)
                errorMsg = result["errmsg"].ToString();
            if (result.ContainsKey("access_token") && result["access_token"] != null)
                Access_Token = result["access_token"].ToString();
            return Access_Token;
        }


        public static string GetSSOAccessToken()
        {
            string url = string.Format("{0}?corpid={1}&corpsecret={2}", DingTalkUrlUtils.GetSSOTokenUrl(), DingTalkConfig.getInstance().CorpId, DingTalkConfig.getInstance().SSOCorpSecret);
            string responsestring = DingTalkHttpUtils.HttpGet(url);
            Dictionary<string, string> result = JsonConvert.DeserializeObject<Dictionary<string, string>>(responsestring);
            string errorCode = "";
            string errorMsg = "";
            string Access_Token = "";
            if (result.ContainsKey("errcode") && result["errcode"] != null)
                errorCode = result["errcode"].ToString();
            if (result.ContainsKey("errmsg") && result["errmsg"] != null)
                errorMsg = result["errmsg"].ToString();
            if (result.ContainsKey("access_token") && result["access_token"] != null)
                Access_Token = result["access_token"].ToString();
            return Access_Token;
        }


        /// <summary>
        /// 获取钉钉开放应用的ACCESS_TOKEN
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public static string GetSnsAccessToken()
        {
            string token = string.Empty;

            string requestUrl = DingTalkUrlUtils.GetSnsTokenUrl();
            string appId = DingTalkConfig.getInstance().AppId;
            string appSecrect = DingTalkConfig.getInstance().AppSecret;
            string url = string.Format("{0}?appid={1}&appsecret={2}", requestUrl, appId, appSecrect);


            string responsestring = DingTalkHttpUtils.HttpGet(url);
            var result = JsonConvert.DeserializeObject<TokenResponse>(responsestring);
            if (result != null && result.access_token != null)
            {
                token = result.access_token;
            }
            return token;

        }

        public static PersistentResponse GetPersistentCode(string access_token, string tmpAuthcode)
        {

            string requestUrl = DingTalkUrlUtils.GetPersistentCodeUrl();
            string url = string.Format("{0}?access_token={1}", requestUrl, access_token);

            var postData = new
            {
                tmp_auth_code = tmpAuthcode
            };

            var jsonRequest = JsonConvert.SerializeObject(postData);
            var jsonResult = DingTalkHttpUtils.HttpPost(url, jsonRequest);

            var result = JsonConvert.DeserializeObject<PersistentResponse>(jsonResult);
            return result;

        }

        public static string GetAuthSnsToken(string token, string _openid, string _persistent_code)
        {
            string snsToken = string.Empty;

            string requestUrl = DingTalkUrlUtils.GetAuthSnsTokenUrl();
            string url = string.Format("{0}?access_token={1}", requestUrl, token);

            var postData = new
            {
                openid = _openid,
                persistent_code = _persistent_code
            };
            var jsonRequest = JsonConvert.SerializeObject(postData);

            var jsonResult = DingTalkHttpUtils.HttpPost(url, jsonRequest);
            var result = JsonConvert.DeserializeObject<SnsTokenResponse>(jsonResult);

            if (result != null && result.sns_token != null)
            {
                snsToken = result.sns_token;

            }

            return snsToken;

        }

        public static GetUserResponse GetSnsUserInfo(string sns_token)
        {

            string requestUrl = DingTalkUrlUtils.GetSnsUserInfoUrl();
            string url = string.Format("{0}?sns_token={1}", requestUrl, sns_token);

            string responsestring = DingTalkHttpUtils.HttpGet(url);
            var result = JsonConvert.DeserializeObject<GetUserResponse>(responsestring);
            return result;

        }


        public static GetDepartmentUserResponse GetUserListByDepartmentId(string accessToken, long departmentId)
        {
            string requestUrl = DingTalkUrlUtils.GetUserListByDepartmentId();
            string url = string.Format("{0}?access_token={1}&department_id={2}", requestUrl, accessToken, departmentId);

            string responsestring = DingTalkHttpUtils.HttpGet(url);
            var result = JsonConvert.DeserializeObject<GetDepartmentUserResponse>(responsestring);
            return result;
        }
        public static DingTalkRoleRelations GetRoleList(string accessToken)
        {
            string requestUrl = DingTalkUrlUtils.GetRoleList();
            string url = string.Format("{0}?access_token={1}", requestUrl, accessToken);

            string responsestring = DingTalkHttpUtils.HttpGet(url);
            JObject jo = JObject.Parse(responsestring);
            JToken jt = jo.SelectToken("result").SelectToken("list");
            List<JToken> lstJt = jt.ToList();
            JArray ja = JArray.Parse(jt.ToString());
            DingTalkRoleGroup group = new DingTalkRoleGroup();
            DingTalkRoleRelation relation = new DingTalkRoleRelation();
            DingTalkRole role = new DingTalkRole();
            DingTalkRoleList roleList = new DingTalkRoleList();
            DingTalkRoleRelations dingTalkRoleRelations = new DingTalkRoleRelations();
            foreach (JToken tmp in jt)
            {
                String groupid = JObject.Parse(tmp.ToString())["groupId"].ToString();
                group.GroupId = JObject.Parse(tmp.ToString())["groupId"].ToString();
                group.GroupName = JObject.Parse(tmp.ToString())["name"].ToString();
                JToken jt1 = JObject.Parse(tmp.ToString())["roles"];
                foreach (var tmp1 in jt1)
                {
                    role.Id = JObject.Parse(tmp1.ToString())["id"].ToString();
                    if (roleList.RoleList.Count > 0 && roleList.RoleList.Exists(t => t.Id == role.Id)) continue;
                    role.Id = JObject.Parse(tmp1.ToString())["id"].ToString();
                    role.RoleName = JObject.Parse(tmp1.ToString())["name"].ToString();
                    roleList.RoleList.Add(role);
                    role = new DingTalkRole();
                 
                }
                relation.Group = group;
                relation.RoleList = roleList;
                dingTalkRoleRelations.RoleRelations.Add(relation);
                group = new DingTalkRoleGroup();
                relation = new DingTalkRoleRelation();
                roleList = new DingTalkRoleList();
            }
        //    var result = JsonConvert.DeserializeObject<DingTalkRoleList>(responsestring);
            return dingTalkRoleRelations;
        }
    }



    public class TokenResponse
    {

        public string access_token { get; set; }

        public int errcode { get; set; }
        public string errmsg { get; set; }

    }

    public class PersistentResponse
    {

        public int errcode { get; set; }
        public string errmsg { get; set; }

        public string openid { get; set; }


        public string persistent_code { get; set; }


        public string unionid { get; set; }

    }

    public class SnsTokenResponse
    {

        public int errcode { get; set; }
        public string errmsg { get; set; }


        public int expires_in { get; set; }
        public string sns_token { get; set; }
    }

    public class SnsUserInfo
    {

        public string nick { get; set; }
        public string openid { get; set; }
        public string unionid { get; set; }


    }

    public class GetUserResponse
    {

        public int errcode { get; set; }
        public string errmsg { get; set; }

        public SnsUserInfo user_info { get; set; }
    }

    public class DingTalkUserinfo
    {

        public string mobile { get; set; }

        public string tel { get; set; }

        public string userid { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string dingId { get; set; }

    }


    public class GetDepartmentUserResponse
    {
        public List<DingTalkUserinfo> userlist { get; set; }

    }

    public class DingTalkRoleGroup
    {
        public String GroupId { get; set; }
        public String GroupName { get; set; }
    }
    public class DingTalkRoleRelation
    {
        public DingTalkRoleGroup Group { get; set; }
        public DingTalkRoleList RoleList { get; set; }
    }
    public class DingTalkRoleRelations
    {
        public DingTalkRoleRelations()
        {
            RoleRelations = new List<DingTalkRoleRelation>();
        }
        public List<DingTalkRoleRelation> RoleRelations { get; set; }
    }
    public class DingTalkRoleList
    {
        public DingTalkRoleList() {
            RoleList = new List<DingTalkRole>();
        }
        public List<DingTalkRole> RoleList { get; set; }
    }
    public class DingTalkRole
    {
        public String Id { get; set; }

        public String RoleName { get; set; }
    }

}
