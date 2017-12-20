using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.DbManage.WorkFlow
{
    public class CrmSysWorkflowFuncEvent
    {
        public Guid FuncEventId { set; get; }

        public Guid FlowId { set; get; }

        public string FuncName { set; get; }
        /// <summary>
        /// 0为caseitemadd执行 1为caseitemaudit执行
        /// </summary>
        public int StepType { set; get; }
        /// <summary>
        /// 固定流程的节点id，若为自由流程，则uuid值为0作为流程起点，值为1作为流程终点
        /// </summary>
        public Guid NodeId { set; get; }
    }
}
