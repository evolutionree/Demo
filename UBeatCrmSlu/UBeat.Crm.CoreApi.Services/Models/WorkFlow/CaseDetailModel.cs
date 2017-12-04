using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.WorkFlow;

namespace UBeat.Crm.CoreApi.Services.Models.WorkFlow
{
    public class CaseDetailModel
    {
        public Guid CaseId { set; get; }
    }


    public class CaseDetailDataModel
    {
        public WorkFlowCaseInfoExt CaseDetail { set; get; }

        public CaseItemAuditInfo CaseItem { set; get; }

        public IDictionary<string, object> EntityDetail { set; get; }

        public IDictionary<string,object> RelateDetail { set; get; }
    }

    public class WorkFlowCaseInfoExt
    {
        /// <summary>
        /// 流程记录ID
        /// </summary>
        public Guid CaseId { set; get; }

        /// <summary>
        /// 流程配置ID
        /// </summary>
        public Guid FlowId { set; get; }


        /// <summary>
        /// 数据记录ID
        /// </summary>
        public Guid RecId { set; get; }

        /// <summary>
        /// 实体id
        /// </summary>
        public Guid EntityId { set; get; }

        /// <summary>
        /// 关联实体id
        /// </summary>
        public Guid RelEntityId { set; get; }

        /// <summary>
        /// 关联实体数据ID
        /// </summary>
        public Guid RelRecId { set; get; }

        /// <summary>
        /// 审批状态 0审批中 1通过 2不通过 3发起审批
        /// </summary>
        public AuditStatusType AuditStatus { set; get; }

        /// <summary>
        /// 流程流水号
        /// </summary>
        public string RecCode { set; get; }
        /// <summary>
        /// 当前节点数
        /// 发起时为0   中间过程加1    结束为-1
        /// </summary>
        public int NodeNum { set; get; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime RecCreated { set; get; }
        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime RecUpdated { set; get; }
        /// <summary>
        /// 创建人
        /// </summary>
        public int RecCreator { set; get; }
        /// <summary>
        /// 修改人
        /// </summary>
        public int RecUpdator { set; get; }
        
        public int Recstatus { set; get; }

        public string FlowName { set; get; }
        /// <summary>
        /// 退回标志 1为有退回，0为没退回
        /// </summary>
        public int BackFlag { set; get; }

        public string RecCreator_Name { set; get; }

    }

    public class CaseItemAuditInfo
    {
        public Guid NodeId { set; get; }

        public string NodeName { set; get; }

        public object ColumnConfig { set; get; }

        /// <summary>
        /// 是否允许拒绝
        /// </summary>
        public int IsCanReject { set; get; }
        /// <summary>
        /// 是否允许退回
        /// </summary>
        public int IsCanReback { set; get; }
        /// <summary>
        /// 是否允许同意
        /// </summary>
        public int IsCanAllow { set; get; }

        /// <summary>
        /// 是否允许中止
        /// </summary>
        public int IsCanTerminate { set; get; }
        /// <summary>
        /// 是否允许编辑
        /// </summary>
        public int IsCanEdit { set; get; }


    }
}
