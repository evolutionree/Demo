using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DingTalk.Utils
{
    public  class H3ConfigInfo
    {
        public string AccessUrl { get; set; }
        public string Secret { get; set; }
        public string EngineCode { get; set; }
        public string DingDingCode { get; set; }
        public H3ConfigInfo() {
            AccessUrl = "https://www.h3yun.com/Webservices/BizObjectService.asmx";
            Secret = "GtVRpNOdBc+7GToXnTDEzPH9pb+5TJwJVpe+oes+DzD3bSIUr5OG8A==";
            EngineCode = "m7n9zbh3iwmj3ysnnbikkc9o3";
            DingDingCode = "dingc35025f3feb25c1335c2f4657eb6378f";
        }
    }
}
