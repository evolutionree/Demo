using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.PushService;
using UBeat.Crm.CoreApi.DomainModel.PushService.XGPush;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Utility;


namespace UBeat.Crm.CoreApi.Repository.Repository.PushService
{
    public class XGPushRepository: IPushServiceRepository
    {
        private string baseUrl = "http://openapi.xg.qq.com/v2";
        private string baseHost = null;
        private long access_id;
        private string secret_key = null;
        ILogger _logger;

        public XGPushRepository(ILogger logger,string baseUrl,long access_id,string secret_key)
        {
            _logger = logger;
            BaseUrl = baseUrl;
            Access_id = access_id;
            Secret_key = secret_key;
        }

        #region ---property---
        public string BaseUrl
        {
            get => baseUrl;
            set
            {
                baseUrl = value.TrimEnd('/');
                if (value.StartsWith("http://"))
                {
                    value = value.Replace("http://", "");
                }
                else if (value.StartsWith("https://"))
                {
                    value = value.Replace("https://", "");
                }
                baseHost = value.TrimEnd('/');
            }
        }
        public long Access_id { get => access_id; set => access_id = value; }
        public string Secret_key { get => secret_key; set => secret_key = value; }

        #endregion

        private void WriteLog(string message)
        {
            if (_logger != null)
            {
                _logger.LogTrace(message);
            }
        }

        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public static long GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds);
        }

        #region ---private method---
        private string GetSign(string httpMethod, string host, Dictionary<string, object> paramDic)
        {
            StringBuilder paramsSb = new StringBuilder();
            var dicSort = paramDic.OrderBy(m => m.Key);
            //var dicSort = from objDic in paramDic orderby objDic.Key ascending select objDic;
            foreach (var kvp in dicSort)
            {
                if (kvp.Key.Equals("sign"))
                    continue;
                paramsSb.Append(string.Format("{0}={1}", kvp.Key, kvp.Value));
            }
            string signstring = string.Format("{0}{1}{2}{3}", httpMethod, host, paramsSb.ToString(), Secret_key);
            return PushServiceHelper.GetMD5HashToLower(signstring);

        }

        private string SerializeObject(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        private IDictionary<string, object> GetPostParameters(string httpMethod, string host, BaseRequestModel requestParams)
        {
            if (requestParams == null)
                requestParams = new BaseRequestModel();
            requestParams.access_id = Access_id;
            requestParams.timestamp = GetTimeStamp();
            var requestParamsDic = requestParams.ToDictionary();
            requestParams.sign = GetSign(httpMethod, host, requestParamsDic);
            requestParamsDic["sign"] = requestParams.sign;
            return requestParamsDic;
        }
        private HttpResponse<T> HttpPost<T>(string servicePath, BaseRequestModel requestParams)
        {
            string url = string.Format("{0}/{1}", baseUrl, servicePath);
            string host = string.Format("{0}/{1}", baseHost, servicePath);
            string data = PushServiceHelper.BuildQuery(GetPostParameters("POST", host, requestParams));
            WriteLog(string.Format("XGRestApi Request[POST]:\n url:{0}\n host:{1}\n UrlEncodeData:{2} \n UrlDecodeData:{3}", url, host, data, WebUtility.UrlDecode(data)));
            var result = PushServiceHelper.HttpPost(url, data, host);
            WriteLog(string.Format("XGRestApi Response:\n {0}", result));
            return JsonConvert.DeserializeObject<HttpResponse<T>>(result);
        }
        private HttpResponse<T> HttpGet<T>(string servicePath, BaseRequestModel requestParams)
        {
            string url = string.Format("{0}/{1}", baseUrl, servicePath);
            string host = string.Format("{0}/{1}", baseHost, servicePath);
            string data = PushServiceHelper.BuildQuery(GetPostParameters("GET", host, requestParams));
            WriteLog(string.Format("XGRestApi Request[GET]:\n url:{0}\n host:{1}\n UrlEncodeData:{2} \n UrlDecodeData:{3}", url, host, data, WebUtility.UrlDecode(data)));
            var result = PushServiceHelper.HttpGet(url, data, host);
            WriteLog(string.Format("XGRestApi Response:\n {0}", result));
            return JsonConvert.DeserializeObject<HttpResponse<T>>(result);
        }

        #endregion


        #region --IPushServiceRepository接口实现--
        public dynamic PushSingleAccount(string account, string message, int message_type, string send_time, int environment = 0)
        {
            SingleAccountPushModel requestParams = new SingleAccountPushModel();
            requestParams.account = account;
            requestParams.message_type = message_type;
            requestParams.message = message;
            requestParams.environment = environment;
            requestParams.send_time = send_time;
            return PushSingleAccount(requestParams);
        }

        public dynamic PushAccountList(List<string> account_list, string message, int message_type, int environment = 0)
        {
            MultiAccountPushModel requestParams = new MultiAccountPushModel();

            requestParams.account_list = JsonConvert.SerializeObject(account_list);
            requestParams.message_type = message_type;

            requestParams.message = message;
            requestParams.environment = environment;
            return PushAccountList(requestParams);
        }

        public dynamic PushMultiAccounts(List<string> account_list, string message, int message_type, int environment = 0)
        {
            CreateMultiPushRequest requestParams = new CreateMultiPushRequest();
            requestParams.message_type = message_type;

            requestParams.message = message;
            requestParams.environment = environment;

            return PushMultiAccounts(requestParams, account_list);
        } 
        #endregion


        #region --推送接口--
        /// <summary>
        /// 单个设备推送接口
        /// </summary>
        /// <param name="requestParams"></param>
        /// <returns></returns>
        public HttpResponse<object> PushSingleDevice(SingleDevicePushModel requestParams)
        {
            return HttpPost<object>("push/single_device", requestParams);
        }

        /// <summary>
        /// 批量设备推送接口
        /// </summary>
        /// <param name="requestParams"></param>
        /// <returns></returns>
        public HttpResponse<object> PushMultiDevices(CreateMultiPushRequest requestParams, List<string> device_list)
        {
            var res = HttpPost<DevicesPushResponseData>("push/create_multipush", requestParams);

            MultiDevicesModel req = new MultiDevicesModel()
            {
                device_list = SerializeObject(device_list),
                push_id = res.Data.PushId
            };
            return HttpPost<object>("push/device_list_multiple", req);
        }



        /// <summary>
        /// 全量设备推送接口
        /// 后台对本接口的调用频率有限制，两次调用之间的时间间隔不能小于3秒。
        /// </summary>
        /// <param name="requestParams"></param>
        /// <returns></returns>
        public HttpResponse<DevicesPushResponseData> PushAllDevice(AllDevicePushModel requestParams)
        {
            return HttpPost<DevicesPushResponseData>("push/all_device", requestParams);

        }


        /// <summary>
        /// 标签
        /// 可以针对设置过标签的设备进行推送。如：女、大学生、低消费等任意类型标签。
        /// </summary>
        /// <param name="requestParams"></param>
        /// <returns></returns>
        public HttpResponse<DevicesPushResponseData> TagsDevice(TagsDeviceModel requestParams)
        {
            return HttpPost<DevicesPushResponseData>("push/tags_device", requestParams);

        }



        /// <summary>
        /// 单个帐号推送接口
        /// 设备的账户或别名由终端SDK在调用推送注册接口时设置，详情参考终端SDK文档。
        /// </summary>
        /// <param name="requestParams"></param>
        /// <returns></returns>
        public HttpResponse<object> PushSingleAccount(SingleAccountPushModel requestParams)
        {
            return HttpPost<object>("push/single_account", requestParams);

        }

        /// <summary>
        /// 批量帐号推送接口
        /// 设备的账户或别名由终端SDK在调用推送注册接口时设置，详情参考终端SDK文档。
        /// </summary>
        /// <param name="requestParams"></param>
        /// <returns></returns>
        public HttpResponse<object> PushAccountList(MultiAccountPushModel requestParams)
        {
            return HttpPost<object>("push/account_list", requestParams);

        }

        /// <summary>
        /// 批量帐号推送接口
        /// 如果推送目标帐号数量很大（比如≥10000），推荐使用本接口
        /// </summary>
        /// <param name="requestParams"></param>
        /// <returns></returns>
        public HttpResponse<object> PushMultiAccounts(CreateMultiPushRequest requestParams, List<string> account_list)
        {
            var res = HttpPost<DevicesPushResponseData>("push/create_multipush", requestParams);

            MultiAccountModel req = new MultiAccountModel()
            {
                account_list = SerializeObject(account_list),
                push_id = res.Data.PushId
            };
            return HttpPost<object>("push/account_list_multiple", req);
        }

        #endregion

        #region --标签设置/删除接口--
        /// <summary>
        /// 批量设置标签
        /// </summary>
        /// <param name="requestParams"></param>
        /// <returns></returns>
        public HttpResponse<object> BatchSet(BatchRequestModel requestParams)
        {

            return HttpPost<object>("tags/batch_set", requestParams);
        }
        /// <summary>
        /// 批量删除标签
        /// </summary>
        /// <param name="requestParams"></param>
        /// <returns></returns>
        public HttpResponse<object> BatchDelete(BatchRequestModel requestParams)
        {

            return HttpPost<object>("tags/batch_del", requestParams);
        }

        #endregion


        #region --账号映射删除接口--

        /// <summary>
        /// 单清
        /// 删除应用中某个account映射的某个token
        /// </summary>
        /// <param name="requestParams"></param>
        /// <returns></returns>
        public HttpResponse<DelAppAccountTokensResponseData> DeleteAppAccountTokens(DelAppAccountTokensModel requestParams)
        {

            return HttpPost<DelAppAccountTokensResponseData>("application/del_app_account_tokens", requestParams);
        }

        /// <summary>
        /// 全清
        /// 删除应用中某account映射的所有token
        /// </summary>
        /// <param name="requestParams"></param>
        /// <returns></returns>
        public HttpResponse<object> DelAppAccountAllTokens(DelAppAccountAllTokensModel requestParams)
        {

            return HttpPost<object>("application/del_app_account_all_tokens", requestParams);
        }

        #endregion

        #region --查询接口--

        #region 查询消息/设备/帐号
        /// <summary>
        /// 查询群发消息发送状态
        /// </summary>
        /// <param name="requestParams"></param>
        /// <returns></returns>
        public HttpResponse<GetMessageStatusResponseData> GetMessageStatus(List<string> push_ids)
        {

            List<PushIDModel> list = new List<PushIDModel>();
            foreach (var m in push_ids)
            {
                list.Add(new PushIDModel() { PushId = m });
            }
            GetMessageStatusModel requestParams = new GetMessageStatusModel();
            requestParams.push_ids = SerializeObject(list);
            return HttpPost<GetMessageStatusResponseData>("push/get_msg_status", requestParams);
        }

        /// <summary>
        /// 查询应用覆盖的设备数（token总数）
        /// 若请求应用列表中某个应用信息非法，则不会在result中返回结果
        /// </summary>
        /// <param name="requestParams"></param>
        /// <returns></returns>
        public HttpResponse<GetAppDeviceNumResponseData> GetAppDeviceNum()
        {

            return HttpPost<GetAppDeviceNumResponseData>("application/get_app_device_num", null);
        }

        /// <summary>
        /// 查询应用的某个token的信息（查看是否有效）
        /// </summary>
        /// <param name="requestParams"></param>
        /// <returns></returns>
        public HttpResponse<GetAppTokenInfoResponseData> GetAppTokenInfo(GetAppTokenInfoModel requestParams)
        {

            return HttpPost<GetAppTokenInfoResponseData>("application/get_app_token_info", requestParams);
        }

        /// <summary>
        /// 查询应用某帐号映射的token（查看帐号-token对应关系）
        /// </summary>
        /// <param name="requestParams"></param>
        /// <returns></returns>
        public HttpResponse<GetAppAccountTokensResponseData> GetAppAccountTokens(GetAppAccountTokensModel requestParams)
        {

            return HttpPost<GetAppAccountTokensResponseData>("application/get_app_account_tokens", requestParams);
        }
        #endregion

        #region 查询标签

        /// <summary>
        /// 查询应用设置的标签
        /// </summary>
        /// <param name="requestParams"></param>
        /// <returns></returns>
        public HttpResponse<QueryAppTagsResponseData> QueryAppTags(QueryAppTagsModel requestParams)
        {

            return HttpPost<QueryAppTagsResponseData>("tags/query_app_tags", requestParams);
        }

        /// <summary>
        /// 查询应用的某个设备上设置的标签
        /// </summary>
        /// <param name="requestParams"></param>
        /// <returns></returns>
        public HttpResponse<QueryTokenTagsResponseData> QueryTokenTags(QueryTokenTagsModel requestParams)
        {

            return HttpPost<QueryTokenTagsResponseData>("tags/query_token_tags", requestParams);
        }

        /// <summary>
        /// 查询应用某个标签下关联的设备数
        /// </summary>
        /// <param name="requestParams"></param>
        /// <returns></returns>
        public HttpResponse<QueryTagTokenNumResponseData> QueryTagTokenNum(QueryTagTokenNumModel requestParams)
        {

            return HttpPost<QueryTagTokenNumResponseData>("tags/query_tag_token_num", requestParams);
        }

        #endregion

        #endregion

        #region --任务删除/取消接口--

        /// <summary>
        /// 删除群发推送任务的离线消息（接口对接失败，返回404错误）
        /// 针对有任务ID（push ID），并且已发送任务可以删除离线消息
        /// </summary>
        /// <param name="requestParams"></param>
        /// <returns></returns>
        public HttpResponse<object> DeleteOfflineMsg(DeleteOfflineMsgModel requestParams)
        {

            return HttpGet<object>("push/delete_offline_msg", requestParams);
        }

        /// <summary>
        /// 取消尚未触发的定时群发任务(接口对接失败，报参数错误)
        /// 针对尚未发送的任务，需要任务ID
        /// </summary>
        /// <param name="requestParams"></param>
        /// <returns></returns>
        public HttpResponse<CancelTimingTaskResponseData> CancelTimingTask(DeleteOfflineMsgModel requestParams)
        {

            return HttpPost<CancelTimingTaskResponseData>("push/cancel_timing_task", requestParams);
        }

        #endregion
    }
}

