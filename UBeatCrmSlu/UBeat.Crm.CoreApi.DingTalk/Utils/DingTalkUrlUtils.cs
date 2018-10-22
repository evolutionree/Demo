using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DingTalk.Utils
{
    public class DingTalkUrlUtils
    {
        private static string _baseUrl = "https://oapi.dingtalk.com/";
        private static string _getToken = "gettoken";
        public static string GetTokenUrl()
        {
            return _baseUrl + _getToken;
        }
        private static string _getssoToken = "sso/gettoken";
        public static string GetSSOTokenUrl()
        {
            return _baseUrl + _getssoToken;
        }


        private static string jsapi_ticket = "get_jsapi_ticket";
        public static string Get_JSApi_Ticket_Url()
        {
            return _baseUrl + jsapi_ticket;
        }

        private static string getUserId = "user/getuserinfo";
        public static string GetUserId()
        {
            return _baseUrl + getUserId;
        }


        #region 部门&用户
        private static string _listdepts = "department/list";
        public static string ListDeptsUrl()
        {
            return _baseUrl + _listdepts;

        }
        private static string _listusersbydeptid = "user/getDeptMember";
        public static string ListUsersByDeptId()
        {
            return _baseUrl + _listusersbydeptid;
        }
        private static string _getuserinfo_url = "user/get";
        public static string GetUserInfo_Url()
        {
            return _baseUrl + _getuserinfo_url;
        }
        #endregion



        private static string ssoGetUserId = @"sso/getuserinfo";
        public static string GetSSOUserId()
        {
            return _baseUrl + ssoGetUserId;

        }


        private static string _snsToken = @"sns/gettoken";
        public static string GetSnsTokenUrl()
        {

            return _baseUrl + _snsToken;
        }


        private static string _persistentCode = @"sns/get_persistent_code";
        public static string GetPersistentCodeUrl()
        {
            return _baseUrl + _persistentCode;

        }

        private static string _authSnsToken = @"sns/get_sns_token";
        public static string GetAuthSnsTokenUrl()
        {
            return _baseUrl + _authSnsToken;
        }


        private static string _snsUserinfo = @"sns/getuserinfo";
        public static string GetSnsUserInfoUrl()
        {
            return _baseUrl + _snsUserinfo;

        }



        public static string GetUserListByDepartmentId()
        {
            return _baseUrl + "user/list";
        }


    }
}
