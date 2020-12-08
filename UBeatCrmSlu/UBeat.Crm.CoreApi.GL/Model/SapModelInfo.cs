using System;
using System.Collections.Generic;

namespace UBeat.Crm.CoreApi.GL.Model
{
    public class SapModelInfo
    {
        public SapModelInfo()
        {
        }
    }

    public class SynSapModel
    {
        public SynSapModel()
        {
            type = -1;
        }
        public int type { get; set; }
        public Guid EntityId { get; set; }
        public List<Guid> RecIds { get; set; }
        public Dictionary<string, object> OtherParams { get; set; }
    }

    public enum BizSynEnum
    {
        None = 0,
        验证业务 = 1,
    }

    public class SynResultModel
    {
        public SynResultModel()
        {
            Result = false;
            Message = string.Empty;
        }
        public bool Result { get; set; }
        public string Message { get; set; }
    }

    public enum DataSourceType
    {
        SAP = 1,
        CRM = 2,
    }

    public class AutoSynSapModel
    {
        public Guid EntityId { get; set; }
        public Guid RecId { get; set; }
    }
}
