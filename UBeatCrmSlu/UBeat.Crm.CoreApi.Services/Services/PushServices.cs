using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.DomainModel.PushService;
using UBeat.Crm.CoreApi.DomainModel.PushService.XGPush;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Repository.PushService;
using UBeat.Crm.CoreApi.Services.Models.PushService;
using UBeat.Crm.CoreApi.Services.Utility.MsgForPug_inUtility;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class PushServices : BaseServices
    {
        PushApiConfig _config = null;
        ILogger _logger = null;
        string accountPrefix = "UK100";

        public ILogger Logger { get => _logger; set => _logger = value; }

        public PushServices()
        {
           
            IConfigurationRoot config = ServiceLocator.Current.GetInstance<IConfigurationRoot>();
            _config = config.GetSection("XGApiSetting").Get<XGApiConfig>();
        }

        private string GenerateAccount(string accountid)
        {
            return accountPrefix + accountid;
        }
        private List<string> GenerateAccount(List<string> accountids)
        {
            List<string> accounts = new List<string>();
            foreach(var m in accountids)
            {
                accounts.Add(accountPrefix + m);
            }
            return accounts;
        }


        private IPushServiceRepository GetRepository(PushDeviceType deviceType)
        {
            if (_config == null)
                throw new Exception("API configuration has not been configured");
            IPushServiceRepository apiRepository = null;
            switch (deviceType)
            {
                case PushDeviceType.Android:
                    if (_config is XGApiConfig)
                    {
                        var configTemp = _config as XGApiConfig;
                        apiRepository = new XGPushRepository(Logger, _config.BaseUrl, configTemp.AndroidAccessId, configTemp.AndroidSecretKey);
                    }
                    break;
                case PushDeviceType.IOS:
                    if (_config is XGApiConfig)
                    {
                        var configTemp = _config as XGApiConfig;
                        apiRepository = new XGPushRepository(Logger, _config.BaseUrl, configTemp.IOSAccessId, configTemp.IOSSecretKey);
                    }
                    break;
            }
            return apiRepository;
        }

        #region --生成信鸽的消息内容--
        private string GetXGSimpleMessage(PushDeviceType deviceType, string title, string message, Dictionary<string, object> customContent, int messageType)
        {
            XGMessage mes = null;
            switch (deviceType)
            {
                case PushDeviceType.Android:
                    mes = new AndroidMessage()
                    {
                        Title = title,
                        Content = message,
                        AcceptTimes = new List<AcceptTime>() { new AcceptTime() }
                    };
                    break;
                case PushDeviceType.IOS:
                    mes = new IOSMessage()
                    {

                        APS = new APS()
                        {
                            Alert = new AlertInfo(message),
                            Badge = 1
                        },
                        AcceptTimes = new List<AcceptTime>() { new AcceptTime() }
                    };

                    break;
            }
            var dic = new Dictionary<string, object>();
            dic.Add("mtype", messageType); //消息类型，0为通知推送，1为聊天短消息，2为聊天长消息
            dic.Add("data", customContent);
            mes.CustomContent = dic;
            //mes.MessageType = messageType;
            return JsonConvert.SerializeObject(mes);
        }
        #endregion

        #region --推送接口--
        /// <summary>
        /// 生产简单的消息信息字符串
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="businessType"></param>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="customContent"></param>
        /// <param name="messageType">消息类型，0为通知推送，1为聊天短消息，2为聊天长消息</param>
        /// <returns></returns>
        public string GetSimpleMessage(PushDeviceType deviceType, string title, string message, Dictionary<string, object> customContent, int messageType)
        {
            return GetXGSimpleMessage(deviceType, title, message, customContent, messageType);
        }

        /// <summary>
        /// 单个帐号
        /// </summary>
        /// <param name="account">账号</param>
        /// <param name="message">消息内容，可以调用GetSimpleMessage生成消息，或者自己生成与api所需格式一样的消息内容</param>
        /// <param name="message_type">消息类型：1：通知 2：透传消息。iOS平台请填0</param>
        /// <param name="deviceType">0:IOs,1:Android</param>
        /// <returns></returns>
        public dynamic PushSingleAccount(string account, string message, int message_type, string send_time, PushDeviceType deviceType)
        {
            account = GenerateAccount(account);
            return GetRepository(deviceType).PushSingleAccount(account, message, message_type, send_time, deviceType == PushDeviceType.Android ? 0 : _config.EnvironmentType);
        }

        /// <summary>
        /// 批量帐号,单次发送account不超过100个
        /// </summary>
        /// <param name="account_list">每个元素是一个account，string类型，单次发送account不超过100个</param>
        /// <param name="message">消息内容，可以调用GetSimpleMessage生成消息，或者自己生成与api所需格式一样的消息内容</param>
        /// <param name="message_type">消息类型：1：通知 2：透传消息。iOS平台请填0</param>
        /// <param name="deviceType">0:IOs,1:Android</param>
        /// <returns></returns>
        public dynamic PushAccountList(List<string> account_list, string message, int message_type, PushDeviceType deviceType)
        {
            account_list = GenerateAccount(account_list);
            return GetRepository(deviceType).PushAccountList(account_list, message, message_type, deviceType == PushDeviceType.Android ? 0 : _config.EnvironmentType);
        }

        /// <summary>
        /// 批量帐号，大量账号时使用
        /// </summary>
        /// <param name="account_list"></param>
        /// <param name="message">消息内容，可以调用GetSimpleMessage生成消息，或者自己生成与api所需格式一样的消息内容</param>
        /// <param name="message_type">消息类型：1：通知 2：透传消息。iOS平台请填0</param>
        /// <param name="deviceType">0:IOs,1:Android</param>
        /// <returns></returns>
        public dynamic PushMultiAccounts(List<string> account_list, string message, int message_type, PushDeviceType deviceType)
        {
            account_list = GenerateAccount(account_list);
            return GetRepository(deviceType).PushMultiAccounts(account_list, message, message_type, deviceType == PushDeviceType.Android ? 0 : _config.EnvironmentType);
        }


        public Dictionary<string, HttpResponse<object>> PushMessage(string accountsStr, string title, string message, Dictionary<string, object> customContent, int messageType, string sendTime)
        {
            var accounts = accountsStr.Split(',');
            string androidmes = GetSimpleMessage(PushDeviceType.Android, title, message, customContent, messageType);
            string iosmes = GetSimpleMessage(PushDeviceType.IOS, null, message, customContent, messageType);
   
            Dictionary<string, HttpResponse<object>> responsedata = new Dictionary<string, HttpResponse<object>>();

            if (accounts.Length == 0)
                return null;
            else if (accounts.Length == 1)
            {
                responsedata.Add("android", PushSingleAccount(accounts[0], androidmes, 1, sendTime, PushDeviceType.Android));
                responsedata.Add("ios", PushSingleAccount(accounts[0], iosmes, 0, sendTime, PushDeviceType.IOS));
            }
            else if (accounts.Length < 80)
            {
                responsedata.Add("android", PushAccountList(accounts.ToList(), androidmes, 1, PushDeviceType.Android));
                responsedata.Add("ios", PushAccountList(accounts.ToList(), iosmes, 0, PushDeviceType.IOS));
            }
            else
            {
                responsedata.Add("android", PushMultiAccounts(accounts.ToList(), androidmes, 1, PushDeviceType.Android));
                responsedata.Add("ios", PushMultiAccounts(accounts.ToList(), iosmes, 0, PushDeviceType.IOS));
            }
            return responsedata;
        }
        public Dictionary<string, HttpResponse<object>> PushMessage(List<string> account_list, string title, string message, Dictionary<string, object> customContent, int messageType, string sendTime)
        {
            //测试代码---推送消息到企业微信
            Pug_inMsg msg = new Pug_inMsg();
            msg.content = message;
            msg.title = title;
            msg.recevier.Add("HuangGuoChen");
            MsgForPug_inHelper.SendMessage(MSGServiceType.WeChat, MSGType.TextCard, msg);

            //var accounts = accountsStr.Split(',');
            string androidmes = GetSimpleMessage(PushDeviceType.Android, title, message, customContent, messageType);
            string iosmes = GetSimpleMessage(PushDeviceType.IOS, null, message, customContent, messageType);

            Dictionary<string, HttpResponse<object>> responsedata = new Dictionary<string, HttpResponse<object>>();

            if (account_list==null||account_list.Count == 0)
                return null;
            else if (account_list.Count == 1)
            {
                responsedata.Add("android", PushSingleAccount(account_list[0], androidmes, 1, sendTime, PushDeviceType.Android));
                responsedata.Add("ios", PushSingleAccount(account_list[0], iosmes, 0, sendTime, PushDeviceType.IOS));
            }
            else if (account_list.Count < 80)
            {
                responsedata.Add("android", PushAccountList(account_list, androidmes, 1, PushDeviceType.Android));
                responsedata.Add("ios", PushAccountList(account_list, iosmes, 0, PushDeviceType.IOS));
            }
            else
            {
                responsedata.Add("android", PushMultiAccounts(account_list, androidmes, 1, PushDeviceType.Android));
                responsedata.Add("ios", PushMultiAccounts(account_list, iosmes, 0, PushDeviceType.IOS));
            }
            return responsedata;
        }
        #endregion


    }
}
