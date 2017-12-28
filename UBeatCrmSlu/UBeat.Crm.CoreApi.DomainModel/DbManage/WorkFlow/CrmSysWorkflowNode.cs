using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.DbManage.WorkFlow
{
    public class CrmSysWorkflowNode
    {
        public Guid NodeId { set; get; }

        public string NodeName { set; get; }

        public Guid FlowId { set; get; }

        public int AuditNum { set; get; }

        public int NodeType { set; get; }

        public int StepTypeId { set; get; }

        [SqlType(NpgsqlDbType.Jsonb)]
        public object RuleConfig { set; get; }

        [SqlType(NpgsqlDbType.Jsonb)]
        public object ColumnConfig { set; get; }

        public int VerNum { set; get; }

        public int AuditSucc { set; get; }

        [SqlType(NpgsqlDbType.Jsonb)]
        public object NodeConfig { set; get; }

    }
}
