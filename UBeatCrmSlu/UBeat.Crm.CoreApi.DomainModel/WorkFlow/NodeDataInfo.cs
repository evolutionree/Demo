﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.WorkFlow
{
    public class NextNodeDataInfo
    {
        public Guid? NodeId { set; get; }

        public string NodeName { set; get; }
        /// <summary>
        /// 当前节点的CaseData字段
        /// </summary>
        public object NodeData { get; set; }

        /// <summary>
        /// 0普通审批 1会审
        /// </summary>
        public NodeType NodeType { set; get; }

        /// <summary>
        /// 流程类型 0自由流程 1固定流程
        /// </summary>
        public WorkFlowType FlowType { set; get; }

        public int NodeNum { set; get; }
        /// <summary>
        /// 节点状态：-1=流程已结束，0=可进入下一步，1=等待其他人会审,2=到达最后审批节点,3=退回回到第一个节点,4=没有满足下一步审批人条件的选人列表
        /// </summary>
        public int NodeState { set; get; }

        /// <summary>
        /// 当前节点审批人类型
        /// </summary>
        public NodeStepType StepTypeId { set; get; }

        /// <summary>
        /// 当前节点审批人类型
        /// </summary>
        public NodeStepType StepCPTypeId { set; get; }

        public int NotFound { get; set; }

        /// <summary>
        ///  节点审批通过还需多少人同意
        /// </summary>
        public int NeedSuccAuditCount { set; get; } = 1;

        public bool IsSkip { get; set; } = false;
        public bool IsSkipNode { get; set; } = false;
    }


}
