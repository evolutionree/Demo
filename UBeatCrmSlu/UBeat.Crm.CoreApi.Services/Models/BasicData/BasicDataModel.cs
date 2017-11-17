using System;
using System.Collections.Generic;

namespace UBeat.Crm.CoreApi.Services.Models.BasicData
{
    public class BasicDataMessageModel
    {
        public Int64 RecVersion { get; set; }
        public int LoginType { get; set; }
    }

    public class BasicDataSyncModel
    {
        public Dictionary<string,Int64> VersionKey { get; set; }
    }

    public class BasicDataDeptModel
    {
        public Guid DeptId { get; set; }

        public int Status { get; set; }
        public int Direction { get; set; }
    }

    public class BasicDataUserContactListModel
    {
        public Int64 RecVersion { get; set; }
        public string SearchName { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }

    public class BasicDataFuncCountListModel
    {
        public Guid AnaFuncId { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }
}
