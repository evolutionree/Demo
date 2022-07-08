using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Utility
{
    public class DockingAPIHelper
    {
        ///// <summary>
        ///// 根据企业名称或董监高姓名等关键字获取企业列表
        ///// </summary>
        //public static String ADVSEARCH_API = "http://api.qixin.com/APIService/v2/search/advSearch?keyword={0}&matchType={1}&appkey={2}&skip={3}";
        ///// <summary>
        ///// 根据企业名称或董监高姓名等关键字获取企业列表
        ///// </summary>
        //public static String FORENGIN_ADVSEARCH_API = "http://api.qixin.com/APIService/sax/getOnlineView?countryCode={0}&companyName={1}&appkey={2}";
        ///// <summary>
        ///// 根据企业全名或注册号或统一社会信用代码精确获取企业工商基本信息
        ///// </summary>
        //public static String GETBASICINFO_API = "http://api.qixin.com/APIService/enterprise/getBasicInfo?appkey={0}&keyword={1}";
        ///// <summary>
        ///// 根据企业全名或注册号或统一社会信用代码获取企业联系信息
        ///// </summary>
        //public static String GETCONTACTINFO_API = "http://api.qixin.com/APIService/enterprise/getContactInfo?keyword={0}&appkey={1}";
        ///// <summary>
        ///// 根据企业名称或注册号或统一社会信用代码获取企业Logo
        ///// </summary>
        //public static String GETENTLOGOBYNAME_API = "http://api.qixin.com/APIService/enterprise/getEntLogoByName?name={0}&appkey={1}";
        ///// <summary>
        ///// 按企业全名或注册号或统一社会信用代码返回工商年报信息
        ///// </summary>
        //public static String GETREPORTLISTBYNAME_API = "http://api.qixin.com/APIService/reports/getReportListByName?appkey={0}&keyword={1}";
        ///// <summary>
        ///// 按企业全名或注册号或统一社会信用代码返回裁判文书列表
        ///// </summary>
        //public static string GETLAWSUITLISTBYNAME_API = "http://api.qixin.com/APIService/lawsuit/getLawsuitListByName?appkey={0}&name={1}&skip={2}";
        ///// <summary>
        ///// 按企业全名或注册号或统一社会信用代码返回裁判文书列表
        ///// </summary>
        //public static string GETCASEDETAILLISTBYNAME_API = "http://api.qixin.com/APIService/case/getCaseDetailListByName?appkey={0}&keyword={1}";
        ///// <summary>
        ///// 按企业全名或注册号或统一社会信用代码返回法院公告信息
        ///// </summary>
        //public static string GETNOTICELISTBYNAME_API = "http://api.qixin.com/APIService/notice/getNoticeListByName?appkey={0}&name={1}&skip={2}";
        
        /// <summary>
        /// 根据企业名称或董监高姓名等关键字获取企业列表
        /// </summary>
        public static String ADVSEARCH_API = "http://api.qixin.com/APIService/v2/search/advSearch?keyword={0}&matchType={1}&appkey={2}&skip={3}&secret_key={4}";
        /// <summary>
        /// 根据企业名称或董监高姓名等关键字获取企业列表
        /// </summary>
        public static String FORENGIN_ADVSEARCH_API = "http://api.qixin.com/APIService/sax/getOnlineView?countryCode={0}&companyName={1}&appkey={2}&secret_key={3}";
        /// <summary>
        /// 根据企业全名或注册号或统一社会信用代码精确获取企业工商基本信息
        /// </summary>
        public static String GETBASICINFO_API = "http://api.qixin.com/APIService/enterprise/getBasicInfo?appkey={0}&keyword={1}&secret_key={2}";
        /// <summary>
        /// 根据企业全名或注册号或统一社会信用代码获取企业联系信息
        /// </summary>
        public static String GETCONTACTINFO_API = "http://api.qixin.com/APIService/enterprise/getContactInfo?keyword={0}&appkey={1}&secret_key={2}";
        /// <summary>
        /// 根据企业名称或注册号或统一社会信用代码获取企业Logo
        /// </summary>
        public static String GETENTLOGOBYNAME_API = "http://api.qixin.com/APIService/enterprise/getEntLogoByName?name={0}&appkey={1}&secret_key={2}";
        /// <summary>
        /// 按企业全名或注册号或统一社会信用代码返回工商年报信息
        /// </summary>
        public static String GETREPORTLISTBYNAME_API = "http://api.qixin.com/APIService/reports/getReportListByName?appkey={0}&keyword={1}&secret_key={2}";
        /// <summary>
        /// 按企业全名或注册号或统一社会信用代码返回裁判文书列表
        /// </summary>
        public static string GETLAWSUITLISTBYNAME_API = "http://api.qixin.com/APIService/lawsuit/getLawsuitListByName?appkey={0}&name={1}&skip={2}&secret_key={3}";
        /// <summary>
        /// 按企业全名或注册号或统一社会信用代码返回裁判文书列表
        /// </summary>
        public static string GETCASEDETAILLISTBYNAME_API = "http://api.qixin.com/APIService/case/getCaseDetailListByName?appkey={0}&keyword={1}&secret_key={2}";
        /// <summary>
        /// 按企业全名或注册号或统一社会信用代码返回法院公告信息
        /// </summary>
        public static string GETNOTICELISTBYNAME_API = "http://api.qixin.com/APIService/notice/getNoticeListByName?appkey={0}&name={1}&skip={2}&secret_key={3}";

        /// <summary>
        /// 根据企业名称或董监高姓名等关键字获取企业列表
        /// </summary>
        public static String GETHKCOMPANYBYNAME_API = "http://api.qixin.com/APIService/hkenterprise/getHKDetailByName?appkey={0}&secret_key={1}&keyword={2}";

        /**
         * 企查查请求地址
         */
        //企查查企业工商数据查询
        public static String SEARCHWIDE_API = "http://api.qichacha.com/ECIV4/SearchWide?key={0}&keyword={1}";
        //企查查企业工商详情
        public static String GETDETAILSBYNAME_API = "http://api.qichacha.com/ECIV4/GetBasicDetailsByName?key={0}&keyword={1}";
        //企查查经营信息详情
        public static String GETINFO_API = "http://api.qichacha.com/ECIInfoOverview/GetInfo?key={0}&searchKey={1}";
        
    }
}
