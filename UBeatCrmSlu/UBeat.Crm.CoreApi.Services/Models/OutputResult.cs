using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace UBeat.Crm.CoreApi.Services.Models
{
    public class Output
    {
        /// <summary>
        /// 业务处理状态码，
        /// 0=成功，
        /// 1=失败，
        /// -25013=多端登录被踢出
        /// -25003=系统管理员修改配置，请重新登录
        /// </summary>
        [JsonProperty("error_code")]
        public int Status { get; set; }
        
        /// <summary>
        /// 业务处理错误提示内容
        /// </summary>
        /// 
        [JsonProperty("error_msg")]
        public string Message { get; set; }

        public Output()
        {
            
        }

        public Output(int status, String message = "")
        {
            Status = status;
            Message = message;
        }
    }

    public class OutputResult<TDataType> : Output
    {
        public OutputResult()
        {

        }

        public OutputResult(TDataType dataBody, String message = "", int status = 0)
            : base(status, message)
        {
            DataBody = dataBody;
        }

        /// <summary>
        /// 响应的业务数据
        /// </summary>
        [JsonProperty("data")]
        public TDataType DataBody
        {
            get;
            private set;
        }

        /// <summary>
        /// 版本数据
        /// </summary>
        [JsonProperty("versions")]
        public VersionData Versions { set; get; }

       
    }

    
}
