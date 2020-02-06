using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.WorkFlow
{
    public class WorkFlowCaseInfo
    {
        public string RecName { get; set; }
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
        /// 流程配置版本号
        /// </summary>
        public int VerNum { set; get; }

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
        /// 创建人
        /// </summary>
        public string RecCreator_Name { set; get; }
        /// <summary>
        /// 修改人
        /// </summary>
        public int RecUpdator { set; get; }
        /// <summary>
        /// 记录版本,系统自动生成
        /// </summary>
        public long RecVersion { set; get; }

        public int Recstatus { set; get; }
    }
}
