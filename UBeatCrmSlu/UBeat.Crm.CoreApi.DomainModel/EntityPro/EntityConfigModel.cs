using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.EntityPro
{
    public class EntityConfigModel
    {
        
    }

    public class FuncEvent
    {
        public Guid FuncEventId { get; set; }
        public Guid TypeId { get; set; }
        public int OperateType { get; set; }
        public string FuncName { get; set; }

    }

    public class ActionExtConfig
    {
        public Guid RecId { get; set; }
        public string RoutePath { get; set; }
        public int ImplementType { get; set; }
        public string AssemblyName { get; set; }
        public string ClassTypeName { get; set; }
        public string FuncName { get; set; }
        public int OperateType { get; set; }
        public int ResultType { get; set; }
        public int RecStatus { get; set; }
        public Guid EntityId { get; set; }
    }

    public class ExtFunction
    {
        public Guid DbFuncId { get; set; }
        public string FuncTionName { get; set; }
        public Guid EntityId { get; set; }
        public string Parameters { get; set; }
        public int RecOrder { get; set; }
        public int ReturnType { get; set; }
        public int RecStatus { get; set; }
        public string Remark { get; set; }
    }
}
