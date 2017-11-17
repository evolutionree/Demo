using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.PushService.XGPush
{
    public class HttpResponse<T>
    {
        [JsonProperty("ret_code")]
        public int StatusCode { set; get; }

        [JsonProperty("err_msg")]
        public string ErrorMessage { set; get; }

        [JsonProperty("result")]
        public T Data { set; get; }

        public string GetStatusCodeDescription()
        {
            return StatusCodeDescription.GetDescription(StatusCode);
        }
    }


    public static class StatusCodeDescription
    {
        static Dictionary<int, string> statusCodeMap = new Dictionary<int, string>();
        static StatusCodeDescription()
        {
            AddKeyValue(0, "调用成功");
            AddKeyValue(-1, "参数错误");
            AddKeyValue(-2, "请求时间戳不在有效期内");
            AddKeyValue(-3, "sign校验无效");
            AddKeyValue(2, "参数错误");
            AddKeyValue(14, "收到非法token");
            AddKeyValue(15, "信鸽逻辑服务器繁忙");
            AddKeyValue(19, "操作时序错误。例如进行tag操作前未获取到deviceToken,可能是没有注册信鸽或者苹果推送或者provisioning profile制作不正确");
            AddKeyValue(20, "鉴权错误，可能是由于Access ID和Access Key不匹配");
            AddKeyValue(40, "推送的token没有在信鸽中注册");
            AddKeyValue(48, "推送的账号没有绑定token");
            AddKeyValue(63, "标签系统忙");
            AddKeyValue(71, "APNS服务器繁忙");
            AddKeyValue(72, "消息字符数超限");
            AddKeyValue(76, "请求过于频繁，请稍后再试");
            AddKeyValue(78, "循环任务参数错误");
            AddKeyValue(100, "APNS证书错误。请重新提交正确的证书");
            
        }

       private static void AddKeyValue(int key,string value)
        {
            if (statusCodeMap == null)
                statusCodeMap = new Dictionary<int, string>();
            if (statusCodeMap.ContainsKey(key))
                statusCodeMap[key] = value;
            else statusCodeMap.Add(key,value);
        }

        public static string GetDescription(int statusCode)
        {
            if (statusCodeMap.ContainsKey(statusCode))
                return statusCodeMap[statusCode];
            else return "其他错误";
        }
    }



}
