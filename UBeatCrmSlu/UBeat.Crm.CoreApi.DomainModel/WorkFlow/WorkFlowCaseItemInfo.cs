using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.WorkFlow
{
    public class WorkFlowCaseItemInfo
    {
        /// <summary>
        /// 步骤明细ID
        /// </summary>
        public Guid CaseItemId { set; get; }
        /// <summary>
        /// 流程实例ID
        /// </summary>
        public Guid CaseId { set; get; }

        public Guid NodeId { set; get; }
        /// <summary>
        /// 当前审批步骤数，发起时为0   中间过程加1    结束为-1
        /// </summary>
        public int NodeNum { set; get; }
        /// <summary>
        /// 整个流程处理过程中的节点总数
        /// </summary>
        public int StepNum { set; get; }
        /// <summary>
        /// 操作选择 0拒绝 1通过 2退回 3中止 4编辑
        /// </summary>
        public ChoiceStatusType ChoiceStatus { set; get; }
        /// <summary>
        /// 改进建议
        /// </summary>
        public string Suggest { set; get; }
        /// <summary>
        /// 节点审批状态 0:未处理; 1:已读; 2:已处理 3作废
        /// </summary>
        public CaseStatusType CaseStatus { set; get; }
        /// <summary>
        /// 操作数据
        /// </summary>
        public object Casedata { set; get; }
        /// <summary>
        /// 个人备注
        /// </summary>
        public string Remark { set; get; }
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
        /// <summary>
        /// 当前步骤处理人
        /// </summary>
        public int HandleUser { set; get; }

        /// <summary>
        /// 当前步骤处理人
        /// </summary>
        public string HandleUserName { set; get; }

        /// <summary>
        /// 当前步骤抄送人字符串
        /// </summary>
        public string CopyUser { set; get; }

       


    }
}
