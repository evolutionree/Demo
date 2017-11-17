using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.EntityPro
{
    public class ServicesJsonInfo
    {
        public List<NoticeServiceModel> NoticeService { set; get; }
        public List<CallBackServiceModel> CallBackServices { get; set; }
        public EntryPageModel EntryPages { get; set; }
    }

    public enum CallBackServiceModel_ServiceType {
        ServiceType_InnerService = 1,
        ServiceType_CommonMethod = 2
    }
    public class CallBackServiceModel {
        public OperatType OperatType { get; set; }
        public CallBackServiceModel_ServiceType ServiceType { get; set; }
        /// <summary>
        /// 函数参数必须遵循以下参数
        /// Transaction ---事务
        /// List<Dict<string,object>> 调用堆栈（每次至少压入recid和entityid，functioncode）
        /// 
        /// </summary>
        public string MethodFullName { get; set; }
    }
    public class NoticeServiceModel
    {
        /// <summary>
        /// 操作类型 0为insert，1为update，2为delete
        /// </summary>
        public OperatType OperatType { set; get; }

        /// <summary>
        /// 需要触发消息的数据源字段名称
        /// </summary>
        public string FieldName { set; get; }
        /// <summary>
        /// 数据源字段关联的实体id
        /// </summary>
        public Guid EntityId { set; get; }
       
        /// <summary>
        /// Message配置的funccode
        /// </summary>
        public string MsgConfigFuncCode { set; get; }

       
        public bool IsValidData()
        {
            return !string.IsNullOrEmpty(FieldName) && EntityId != Guid.Empty && !string.IsNullOrEmpty(MsgConfigFuncCode) ;
        }

    }
    /// <summary>
    /// 定义每个实体的入口信息，目前仅有WEB生效
    /// </summary>
    public class EntryPageModel {
        public string WebListPage { get; set; }
        public string WebEditPage { get; set; }
        public string WebViewPage { get; set; }
        public string WebIndexPage { get; set; }
        public string AndroidListPage { get; set; }
        public string AndroidEditPage { get; set; }
        public string AndroidViewPage { get; set; }
        public string AndroidIndexPage { get; set; }
        public string IOSListPage {get;set;}
        public string IOSEditPage { get; set; }
        public string IOSViewPage { get; set; }
        public string IOSIndexpage { get; set; }
    }

    public enum OperatType
    {
        Insert=0,
        Update=1,
        Delete=2,

    }
}
