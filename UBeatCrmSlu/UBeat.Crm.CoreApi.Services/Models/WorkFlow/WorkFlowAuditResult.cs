using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.WorkFlow;

namespace UBeat.Crm.CoreApi.Services.Models.WorkFlow
{
    /// <summary>
    /// 流程预处理审批结果
    /// </summary>
    public class PretreatAuditResult
    {
        public int AuditStatus { set; get; }
        public NextNodeApproverInfo NextNodeApprovers { set; get; }
    }
}
