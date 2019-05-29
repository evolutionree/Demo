using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.WorkFlow
{
    public class WorkFlowInfo
    {
        public Guid FlowId { set; get; }

        /// <summary>
        /// 实体ID
        /// </summary>
        public Guid Entityid { set; get; }

        /// <summary>
        /// 关联实体id
        /// </summary>
        public Guid RelEntityId { set; get; }

        /// <summary>
        /// 流程名称
        /// </summary>
        public string FlowName { set; get; }

        /// <summary>
        /// 流程类型 0自由流程 1固定流程
        /// </summary>
        public WorkFlowType FlowType { set; get; }

        /// <summary>
        /// 退回标志 1为有退回，0为没退回
        /// </summary>
        public int BackFlag { set; get; }

        /// <summary>
        /// 流程中止后是否可返回标志 1为可返回 0为不可
        /// </summary>
        public int ResetFlag { set; get; }

        /// <summary>
        /// 跳过标志，1为新增直接结束流程 0为正常流程
        /// </summary>
        public int SkipFlag { set; get; }
        /// <summary>
        /// 过期自动中止 0为不限制 超过0的为限制几天后中止审批
        /// </summary>
        public int ExpireDay { set; get; }

        /// <summary>
        /// 流程说明
        /// </summary>
        public string Remark { set; get; }
        /// <summary>
        /// 流程版本号
        /// </summary>
        public int VerNum { set; get; }
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
        public string RecCreator_name { set; get; }

        /// <summary>
        /// 修改人
        /// </summary>
        public int RecUpdator { set; get; }
        /// <summary>
        /// 记录版本,系统自动生成
        /// </summary>
        public long RecVersion { set; get; }

        public int Recstatus { set; get; }
        /// <summary>
        /// 流程定义的标题设置 
        /// </summary>
        public string TitleConfig { get; set; }
    }
    public class LoopInfo
    {
        public Guid NodeId { get; set; }
        /// <summary>
        /// 是否强制跳出递归，还是找到用户自然跳出
        /// </summary>
        public bool IsBreak { get; set; }

        public bool IsSkip { get; set; } = false;
        public NextNodeDataInfo NodeDataInfo { get; set; }
        /// <summary>
        /// 判断是否所有节点都没有审批人
        /// </summary>
        public bool IsNoneApproverUser { get; set; } = false;
    }
}
