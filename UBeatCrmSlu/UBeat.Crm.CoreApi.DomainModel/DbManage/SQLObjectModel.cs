using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.DbManage
{
    public class SQLObjectModel
    {
        public Guid Id { get; set; }
        public SQLObjectTypeEnum ObjType { get; set; }
        public string SqlPath { get; set; }
        public string LastVersion { get; set; }
        public string ObjName { get; set; }
        public string Remark { get; set; }
        public string RelativeObj { get; set; }
        public string Name { get; set; }
        public void checkEmpty() {
            if (SqlPath == null) SqlPath = "";
            if (LastVersion == null) LastVersion = "";
            if (ObjName == null) ObjName = "";
            if (Remark == null) Remark = "";
            if (RelativeObj == null) RelativeObj = "";
            if (Name == null) Name = "";
        }
    }
    public class SQLTextModel {
        public Guid Id { get; set; }
        public Guid? SqlObjId { get; set; }
        public string Version { get; set; }
        public string Remark { get; set; }
        public string SqlText { get; set; }
        public SqlOrJsonEnum sqlOrJson { get; set; }
        public InitOrUpdate initOrUpdate { get; set; }
        public StructOrData structOrData { get; set; }
        public int isRun { get; set; }
        public void checkEmpty()
        {
            if (SqlObjId == null) SqlObjId = Guid.Empty;
            if (Version == null) Version = "";
            if (Remark == null) Remark = "";
            if (SqlText == null) SqlText = "";
        }


    }
    public class SQLReflectQueryModel {
        public string[] RecIds { get; set; }
    }
    public enum SQLObjectTypeEnum {
        All =0,
        Table=1,
        Func=2
    }
    public enum SqlOrJsonEnum {
        All = 0,
        SQL = 1,
        Json =2
    }
    public enum InitOrUpdate {
        All = 0,
        Init = 1,
        Update = 2
    }
    public enum StructOrData {
        All = 0,
        Struct = 1,
        Data = 2
    }
}
