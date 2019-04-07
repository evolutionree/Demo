using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DevAssist.Models
{
    public class UKWebApiInfo
    {
        /// <summary>
        /// Api的全路径
        /// </summary>
        public string FullPath { get; set; }
        public string DllName { get; set; }
        public string ClassName { get; set; }
        public string MethodName { get; set; }
        public bool NeedAuth { get; set; }
        public  List<UKApiRequestParameterInfo> Parameters { get; set; }
        public string ApiName { get; set; }
        public string MoreName { get; set; }
        public string SelfDescription { get; set; }
        public string MoreDescription { get; set; }
        public string RequestSample { get; set; }
        public string ResponseSample { get; set; }
        public UKWebApiInfo() {
            NeedAuth = true;
            Parameters = new List<UKApiRequestParameterInfo>();
        }
    }
    public class UKApiRequestParameterInfo {
        public string ParameterName { get; set; }
        public string ParameterType { get; set; }
        public UKApiRequestParamFromTypeEnum FromType { get; set; }
        public string ParameterCNName { get; set; }
        public string Description { get; set; }

        public List<UKApiRequestParameterInfo> SubParameters { get; set; }


    }

    public enum UKApiRequestParamFromTypeEnum {
        None = 0 , 
        FromBody =1 ,
        FromQuery =2 
    }
}
