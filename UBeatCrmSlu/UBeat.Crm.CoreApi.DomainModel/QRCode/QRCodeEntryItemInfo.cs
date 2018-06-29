using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.QRCode
{
    public class QRCodeEntryItemInfo
    {
        public Guid RecId { get; set; }
        public QRCodeCheckTypeEnum CheckType { get; set; }
        public string CheckType_Name { get; set; }
        public QRCodeCheckMatchParamInfo CheckParam { get; set; }
        public string CheckRemark { get; set; }
        public QRCodeCheckTypeEnum DealType { get; set; }
        public string DealType_Name { get; set; }
        public QRCodeDealParamInfo DealParam { get; set; }
        public string DealRemark { get; set; }
        public string RecName { get;set; }
        public int RecOrder { get; set; }
        public int RecStatus { get; set; }
        public string Remark { get; set; }
    }
    /// <summary>
    /// 匹配规则参数的总包，其内部包含各种模式下的参数
    /// </summary>
    public class QRCodeCheckMatchParamInfo {
        public QRCodeStringCheckMatchParamInfo StringMatchParam { get; set; }
        public QRCodeRegexCheckMatchParamInfo RegexMatchParam { get; set; }
        public QRCodeEntityCheckMatchParamInfo EntityMatchParam { get; set; }
        public QRCodeInnerServiceCheckMatchParamInfo InnerServiceMatchParam { get; set; }
        public QRCodeSQLScriptCheckMatchParamInfo SQLMatchParam { get; set; }
        public QRCodeUScriptCheckMatchParamInfo UScriptParam { get; set; }
    }
    /// <summary>
    /// 字符串匹配的参数
    /// </summary>
    public class QRCodeStringCheckMatchParamInfo {
        public string CheckItemString { get; set; }
        /// <summary>
        /// 是否区分大小写，0=不区分，1=区分
        /// </summary>
        public int CaseSensitive { get; set; }
        /// <summary>
        /// 是否全文匹配
        /// 1=全文匹配，0=部分匹配
        /// </summary>
        public int MatchWholeWord { get; set; }
    }
    /// <summary>
    /// 正则表达式匹配参数
    /// </summary>
    public class QRCodeRegexCheckMatchParamInfo {
        public string RegexString { get; set; }
        /// <summary>
        /// 是否区分大小写，0=不区分，1=区分
        /// </summary>
        public int CaseSensitive { get; set; }
    }
    /// <summary>
    /// 实体匹配参数
    /// </summary>
    public class QRCodeEntityCheckMatchParamInfo {
        public Guid EntityId { get; set; }
        public string EntityName { get; set; }
        public Guid FieldId { get; set; }
        public string FieldDisplayName { get; set; }
        /// <summary>
        /// 是否区分大小写，0=不区分，1=区分
        /// </summary>
        public int CaseSensitive { get; set; }
        /// <summary>
        /// 是否全文匹配
        /// 1=全文匹配，0=部分匹配
        /// </summary>
        public int MatchWholeWord { get; set; }
        /// <summary>
        /// 其他限定规则ID
        /// </summary>
        public Guid RuleId { get; set; }
    }
    /// <summary>
    /// 内部服务匹配参数
    /// </summary>
    public class QRCodeInnerServiceCheckMatchParamInfo {
        public string ClassFullName { get; set; }
        public string MethodName { get; set; }
        public Dictionary<string, object> OthterParams { get; set; }
    }
    /// <summary>
    /// 数据库脚本匹配和数据库函数匹配共用的参数配置
    /// </summary>
    public class QRCodeSQLScriptCheckMatchParamInfo {
        public string SQLScript { get; set; }
        public int MultiAsSuccess { get; set; }
    }
    /// <summary>
    /// UScript的匹配参数
    /// </summary>
    public class QRCodeUScriptCheckMatchParamInfo {
        public string UScript { get; set; }

    }

    public enum QRCodeDealErrorDefaultActionEnum
    {
        ReturnNoActionResult = 1,
        ReturnErrorMsgResult = 2
    }
    /// <summary>
    /// 各种计算结果的计算参数的配置
    /// </summary>
    public class QRCodeDealParamInfo {
        /// <summary>
        /// 执行失败（就是抛出异常或者没有返回值）时的默认返回方案
        /// </summary>
        public QRCodeDealErrorDefaultActionEnum DefaultAction { get; set; }
        public QRCodeDeal_UScriptParamInfo UScriptParam { get; set; }
        public QRCodeDeal_EntityParamInfo EntityParam { get; set; }
        public QRCodeDeal_SQLParamInfo SQLParam { get; set; }
        public QRCodeDeal_InnerServiceParamInfo InnerServiceParam { get; set; }



    }
    /// <summary>
    /// UScript作为处理规则的相关参数
    /// </summary>
    public class QRCodeDeal_UScriptParamInfo {
        public string UScirpt { get; set; }
    }

    /// <summary>
    /// 实体处理规则参数
    /// </summary>
    public class QRCodeDeal_EntityParamInfo {
        /// <summary>
        /// 要显示的实体类型
        /// </summary>
        public Guid EntityId { get; set; }
        public string EntityName { get; set; }
        public int ViewType { get; set; }
        public Guid TypeId { get; set; }
        public string TypeName { get; set; }
        public Guid FieldId { get; set; }
        public string FieldDisplayName { get; set; }
        public Guid DataIdFetchRuleId { get; set; }
        public Guid FieldDataFetchRuleId { get; set; }

    }

    /// <summary>
    /// SQL脚本和SQL函数的处理参数
    /// </summary>
    public class QRCodeDeal_SQLParamInfo {
        public string SQLFunc { get; set; }
    }

    public class QRCodeDeal_InnerServiceParamInfo {
        public string ClassFullName { get; set; }
        public string MethodName { get; set; }
        public Dictionary<string, object> OtherParams { get; set; }
    }
    public enum QRCodeCheckTypeEnum :Int32
{
        StringSearch = 1 ,
        RegexSearch = 2,
        JSPlugInSearch = 3,
        EntitySearch =4,
        SQLScript =5,
        SQLFunction = 6,
        InnerService =7
    }
    public enum QRCodeActionTypeEnum : Int32
    {
        NoAction = 0,
        SimpleMsg = 1,
        ShowEntityUI = 2,
        ShowCommonUI= 3,
        ShowH5Page = 4
    }
   
    
}
