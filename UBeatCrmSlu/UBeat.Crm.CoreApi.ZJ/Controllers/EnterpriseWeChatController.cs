using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UBeat.Crm.CoreApi.Controllers;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.ZJ.Services;
using UBeat.Crm.CoreApi.ZJ.WJXModel;

namespace UBeat.Crm.CoreApi.ZJ.Controllers
{
    [Route("api/[controller]")]
    public class EnterpriseWeChatController : BaseController
    {
        private readonly EnterpriseWeChatServices _enterpriseWeChatServices;
        public EnterpriseWeChatController(EnterpriseWeChatServices enterpriseWeChatServices)
        {
            _enterpriseWeChatServices = enterpriseWeChatServices;
        }
        [HttpGet]
        [Route("getssocode")]
        [AllowAnonymous]
        public OutputResult<object> GetMessageSSOCode()
        {
            Stream stream = Request.Body;
            Byte[] byteData = new Byte[stream.Length];
            stream.Read(byteData, 0, (Int32)stream.Length);
            string userId = Request.Query["userid"];
            string username = Request.Query["username"];
            string caseid = Request.Query["caseid"];
            string action = Request.Query["action"];
            string code = Request.Query["code"];
            string urltype = Request.Query["urltype"];
            EnterpriseWeChatModel enterpriseWeChat = new EnterpriseWeChatModel();
            enterpriseWeChat.Code = code;
            enterpriseWeChat.Data = new Dictionary<string, object>();
            if (urltype == "1")
            {
                enterpriseWeChat.UrlType = UrlTypeEnum.WorkFlow;
                enterpriseWeChat.Data.Add("caseid", caseid);
                enterpriseWeChat.Data.Add("userid", userId);
                enterpriseWeChat.Data.Add("username", username);
            }
            else
            {
                enterpriseWeChat.UrlType = UrlTypeEnum.SmartReminder;
            }

            var result = _enterpriseWeChatServices.GetSSOCode(enterpriseWeChat);
            if (result.Status == 1) return result;
            this.HttpContext.Response.Cookies.Append("token", result.DataBody.ToString().Substring(result.DataBody.ToString().LastIndexOf("?") + 1), new CookieOptions
            {
                Expires = DateTime.Now.AddMinutes(120),
            });
            this.HttpContext.Response.Redirect("http://183.63.72.242:45290/dashboard");
            return result;
        }

        [HttpGet]
        [Route("getsso")]
        [AllowAnonymous]
        public OutputResult<object> GetSSO()
        {
            Stream stream = Request.Body;
            Byte[] byteData = new Byte[stream.Length];
            stream.Read(byteData, 0, (Int32)stream.Length);
            string code = Request.Query["code"];

            EnterpriseWeChatModel enterpriseWeChat = new EnterpriseWeChatModel();
            enterpriseWeChat.Code = code;
            enterpriseWeChat.Data = new Dictionary<string, object>();
            enterpriseWeChat.UrlType = UrlTypeEnum.SSO;

            var result = _enterpriseWeChatServices.GetSSOCode(enterpriseWeChat);
            if (result.Status == 1) return result;
            HttpContext.Response.Cookies.Append("token", result.DataBody.ToString().Substring(result.DataBody.ToString().LastIndexOf("?") + 1), new CookieOptions
            {
                Expires = DateTime.Now.AddMinutes(120)
            });

            return result;
        }
        [HttpGet]
        [Route("geturl")]
        [AllowAnonymous]
        public string GetAbc()
        {
            Stream stream = Request.Body;
            Byte[] byteData = new Byte[stream.Length];
            stream.Read(byteData, 0, (Int32)stream.Length);
            string code = Request.Query["code"];
            return code;
        }

    }
}
