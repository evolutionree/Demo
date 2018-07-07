using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.CoreApi.DomainModel.PushService.XGPush;
using Newtonsoft.Json;
using UBeat.Crm.CoreApi.Services.Models.PushService;
using Microsoft.Extensions.Configuration;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.DomainModel.PushService;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class PushServiceController : BaseController
    {
        private readonly ILogger<PushServiceController> _logger;

        private readonly PushServices _pushService;

        public PushServiceController(ILogger<PushServiceController> logger, PushServices pushServices) : base(pushServices)
        {
            _logger = logger;
            _pushService = pushServices;
            _pushService.Logger = logger;
        }

       

        #region --推送接口--

        /// <summary>
        /// 帐号推送
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("accountspush")]
        public OutputResult<object> AccountsPush([FromBody]AccountsPushModel body)
        {
            if (body == null)
                return ResponseError<object>("参数错误");
            var responsedata = _pushService.PushMessage(body.Accounts, body.Title, body.Message, body.CustomContent, 0, body.SendTime);

            //var accounts = body.Accounts.Split(',');
            //string androidmes = _pushService.GetSimpleMessage(DeviceType.Android,  body.Title, body.Message, body.CustomContent,0);
            //string iosmes = _pushService.GetSimpleMessage(DeviceType.IOS,  null, body.Message, body.CustomContent,0);
            //Dictionary<string, HttpResponse<object>> responsedata = new Dictionary<string, HttpResponse<object>>();

            //if (accounts.Length == 0)
            //    return ResponseError<object>("accounts不可为空");
            //else if (accounts.Length == 1)
            //{
            //    responsedata.Add("android", _pushService.PushSingleAccount(accounts[0], androidmes, 1, body.SendTime, DeviceType.Android));
            //    responsedata.Add("ios", _pushService.PushSingleAccount(accounts[0], iosmes, 0, body.SendTime, DeviceType.IOS));
            //}
            //else if (accounts.Length < 80)
            //{
            //    responsedata.Add("android", _pushService.PushAccountList(accounts.ToList(), androidmes, 1, DeviceType.Android));
            //    responsedata.Add("ios", _pushService.PushAccountList(accounts.ToList(), iosmes, 0, DeviceType.IOS));
            //}
            //else
            //{
            //    responsedata.Add("android", _pushService.PushMultiAccounts(accounts.ToList(), androidmes, 1, DeviceType.Android));
            //    responsedata.Add("ios", _pushService.PushMultiAccounts(accounts.ToList(), iosmes, 0, DeviceType.IOS));
            //}
            return new OutputResult<object>(responsedata);
        }

        

        #endregion
    }
}
