﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.Controllers;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.Models;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.Account;
using UBeat.Crm.CoreApi.Services.Services;

namespace UBeat.Crm.CoreApi.DevAssist.Controllers
{
    [Route("api/[controller]")]
    public class SystemStatisticController: BaseController
    {
        private readonly AccountServices _accountServices;
        public SystemStatisticController(AccountServices accountServices) : base(accountServices) {
            this._accountServices = accountServices;

        }
        [UKWebApiAttribute("获取用户的登陆", Description = "获取所有用户的登陆情况")]
        [HttpPost("listalluser")]
        public OutputResult<object> ListAllUsers()
        {
            Dictionary<string,LoginSessionModel> mobileLogin = CacheService.Repository.GetAllMobileLoginSession<LoginSessionModel>();
            Dictionary<string,LoginSessionModel> webLogin = CacheService.Repository.GetAllWebLoginSession<LoginSessionModel>();
            AccountQueryModel model = new AccountQueryModel() {
                PageIndex = 1, PageSize = 100000,RecStatus = 1 ,UserName="",UserPhone = ""
            };
            OutputResult<object> userResponse =_accountServices.GetUserList(model, UserId);
            Dictionary<string, List<IDictionary<string, object>>> userData = (Dictionary<string, List<IDictionary<string, object>>>)userResponse.DataBody;
            List<IDictionary<string, object>> userList = (List<IDictionary<string, object>>)userData["PageData"];
            List<Dictionary<string, object>> retUserList = new List<Dictionary<string, object>>();
            Dictionary<string, object> newUserInfo = new Dictionary<string, object>();
            foreach (IDictionary<string, object> user in userList) {
                bool HasData = false;
                if (mobileLogin.ContainsKey(user["userid"].ToString())) {
                    HasData = true;
                    if (!newUserInfo.ContainsKey("sessions")) {
                        newUserInfo["sessions"] = new List<Dictionary<string, object>>();
                    }
                    foreach (string key  in  ((LoginSessionModel)mobileLogin[user["userid"].ToString()]).Sessions.Keys) {
                        if (((LoginSessionModel)mobileLogin[user["userid"].ToString()]).Sessions[key].Expiration > System.DateTime.Now) continue;
                        Dictionary<string, object> item = new Dictionary<string, object>();
                        item["deviceid"] = ((LoginSessionModel)mobileLogin[user["userid"].ToString()]).Sessions[key].DeviceId;
                        item["devicetype"] = ((LoginSessionModel)mobileLogin[user["userid"].ToString()]).Sessions[key].DeviceType;
                        item["sysmark"] = ((LoginSessionModel)mobileLogin[user["userid"].ToString()]).Sessions[key].SysMark;
                        item["expiration"] = ((LoginSessionModel)mobileLogin[user["userid"].ToString()]).Sessions[key].Expiration;
                        item["requesttimestamp"] = ((LoginSessionModel)mobileLogin[user["userid"].ToString()]).Sessions[key].RequestTimeStamp;
                        item["lastrequesttimestamp"] = ((LoginSessionModel)mobileLogin[user["userid"].ToString()]).Sessions[key].LastRequestTime;
                        ((List<Dictionary<string, object>>)newUserInfo["sessions"]).Add(item);
                    }
                    
                }
                if (webLogin.ContainsKey(user["userid"].ToString()))
                {
                    HasData = true;
                    if (!newUserInfo.ContainsKey("sessions"))
                    {
                        newUserInfo["sessions"] = new List<Dictionary<string, object>>();
                    }
                    foreach (string key in ((LoginSessionModel)webLogin[user["userid"].ToString()]).Sessions.Keys)
                    {
                        if (((LoginSessionModel)webLogin[user["userid"].ToString()]).Sessions[key].Expiration < System.DateTime.Now) continue;
                        Dictionary<string, object> item = new Dictionary<string, object>();
                        item["deviceid"] = ((LoginSessionModel)webLogin[user["userid"].ToString()]).Sessions[key].DeviceId;
                        item["devicetype"] = ((LoginSessionModel)webLogin[user["userid"].ToString()]).Sessions[key].DeviceType;
                        item["sysmark"] = ((LoginSessionModel)webLogin[user["userid"].ToString()]).Sessions[key].SysMark;
                        item["expiration"] = ((LoginSessionModel)webLogin[user["userid"].ToString()]).Sessions[key].Expiration;
                        item["requesttimestamp"] = ((LoginSessionModel)webLogin[user["userid"].ToString()]).Sessions[key].RequestTimeStamp;
                        item["lastrequesttimestamp"] = ((LoginSessionModel)webLogin[user["userid"].ToString()]).Sessions[key].LastRequestTime;
                        ((List<Dictionary<string, object>>)newUserInfo["sessions"]).Add(item);
                    }

                }
                if (HasData)
                {
                    newUserInfo["userid"] = user["userid"];
                    newUserInfo["username"] = user["username"];
                    newUserInfo["accountname"] = user["accountname"];
                    newUserInfo["deptid"] = user["deptid"];
                    newUserInfo["deptname"] = user["deptname"];
                    retUserList.Add(newUserInfo);
                }
            }
            return new OutputResult<object>(retUserList);
        }

        [UKWebApiAttribute("获取某个用户的登录信息", Description = "获取某个用户的登录信息")]
        [HttpPost("userlogininfo")]
        public OutputResult<object> ListLoginUserInfo([FromBody]UserLoginParamInfo userInfo = null)
        {
            if (userInfo == null || userInfo.UserId <= 0) {
                return ResponseError<object>("参数异常");
            }
            var loginSession_web = CacheService.Repository.Get<LoginSessionModel>("WebLoginSession_" + userInfo.UserId);
            var loginSession_mobile = CacheService.Repository.Get<LoginSessionModel>("MobileLoginSession_" + userInfo.UserId);
            List<Dictionary<string, object>> retList = new List<Dictionary<string, object>>();
            if (loginSession_web != null) {
                foreach (var tokenInfo in loginSession_web.Sessions.Values) {
                    Dictionary<string, object> item = new Dictionary<string, object>();
                    item["deviceid"] = tokenInfo.DeviceId;
                    item["devicetype"] = tokenInfo.DeviceType;
                    item["sysmark"] = tokenInfo.SysMark;
                    item["expiration"] = tokenInfo.Expiration;
                    item["requesttimestamp"] = tokenInfo.RequestTimeStamp;
                    item["lastrequesttimestamp"] = tokenInfo.LastRequestTime;
                    retList.Add(item);
                }
            }
            if (loginSession_mobile != null)
            {
                foreach (var tokenInfo in loginSession_mobile.Sessions.Values)
                {
                    Dictionary<string, object> item = new Dictionary<string, object>();
                    item["deviceid"] = tokenInfo.DeviceId;
                    item["devicetype"] = tokenInfo.DeviceType;
                    item["sysmark"] = tokenInfo.SysMark;
                    item["expiration"] = tokenInfo.Expiration;
                    item["requesttimestamp"] = tokenInfo.RequestTimeStamp;
                    item["lastrequesttimestamp"] = tokenInfo.LastRequestTime;
                    retList.Add(item);
                }
            }
            return new OutputResult<object>(retList);
        }
    }
    public class UserLoginParamInfo {
        public int UserId { get; set; }
    }
}
