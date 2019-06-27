using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.WorkFlow;

namespace UBeat.Crm.CoreApi.Services.Models.WorkFlow
{
    public class NextNodeDataModel
    {
        /// <summary>
        /// 节点信息
        /// </summary>
        public NextNodeDataInfo NodeInfo { set; get; }
        /// <summary>
        /// 节点的审批人列表
        /// </summary>
        public List<ApproverInfo> Approvers { set; get; }
        public List<ApproverInfo> CPUsers { set; get; }

        public int NotFound { get; set; }
        public bool IsSkipNode { get; set; } = false;
        public bool IsCpShow { get; set; }
    }



}
