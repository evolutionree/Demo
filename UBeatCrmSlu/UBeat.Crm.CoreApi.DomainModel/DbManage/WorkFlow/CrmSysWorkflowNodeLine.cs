using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.DbManage.RuleInfo;

namespace UBeat.Crm.CoreApi.DomainModel.DbManage.WorkFlow
{
    public class CrmSysWorkflowNodeLine
    {
        public Guid LineId { set; get; }

        public Guid FlowId { set; get; }

        public Guid RuleId { set; get; }

        public int VerNum { set; get; }

        public Guid FromNodeId { set; get; }

        public Guid ToNodeId { set; get; }

        /// <summary>
        /// 线配置字段，如线的位置等
        /// </summary>
        [SqlType(NpgsqlDbType.Jsonb)]
        public object LineConfig { set; get; }

        

    }
}
