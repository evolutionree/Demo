using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.DbManage.WorkFlow
{
    public class CrmSysWorkflow
    {
        public Guid FlowId { set; get; }
        /// <summary>
        /// 流程名称
        /// </summary>
        public string FlowName { set; get; }
        /// <summary>
        /// 流程类型 0自由流程 1固定流程
        /// </summary>
        public int FlowType { set; get; }
        /// <summary>
        /// 退回标志 1为有退回，0为没退回''
        /// </summary>
        public int BackFlag { set; get; }
        /// <summary>
        /// 流程中止后是否可返回标志 1为可返回 0为不可
        /// </summary>
        public int ResetFlag { set; get; }
        /// <summary>
        /// 过期自动中止 0为不限制 超过0的为限制几天后中止审批
        /// </summary>
        public int ExpireDay { set; get; }
        /// <summary>
        /// 流程说明
        /// </summary>
        public string Remark { set; get; }
        /// <summary>
        /// 实体ID
        /// </summary>
        public Guid EntityId { set; get; }
        /// <summary>
        /// 流程版本号
        /// </summary>
        public int VerNum { set; get; }
        /// <summary>
        /// 是否跳过
        /// </summary>
        public int SkipFlag { set; get; }

    }
}
