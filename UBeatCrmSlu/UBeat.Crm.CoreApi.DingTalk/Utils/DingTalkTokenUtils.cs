
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;

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
    }


    public class GetDepartmentUserResponse
    {
        public List<DingTalkUserinfo> userlist { get; set; }

    }
 


}
