using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.DbManage.WorkFlow
{
    public class CrmSysWorkflowFuncEvent
    {
        [JsonProperty("funceventid")]
        public Guid FuncEventId { set; get; }

        [JsonProperty("flowid")]
        public Guid FlowId { set; get; }

        [JsonProperty("funcname")]
        public string FuncName { set; get; }
        /// <summary>
        /// 0为caseitemadd执行 1为caseitemaudit执行
        /// </summary>
        [JsonProperty("steptype")]
        public int StepType { set; get; }
        /// <summary>
        /// 固定流程的节点id，若为自由流程，则uuid值为0作为流程起点，值为1作为流程终点
        /// </summary>
        [JsonProperty("nodeid")]
        public Guid NodeId { set; get; }
    }
}
