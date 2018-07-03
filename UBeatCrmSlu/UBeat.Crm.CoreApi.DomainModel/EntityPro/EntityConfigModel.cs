using Newtonsoft.Json;
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
        [JsonProperty("funceventid")]
        public Guid FuncEventId { get; set; }
        [JsonProperty("typeid")]
        public Guid TypeId { get; set; }
        [JsonProperty("operatetype")]
        public int OperateType { get; set; }
        [JsonProperty("funcname")]
        public string FuncName { get; set; }

    }

    public class ActionExtConfig
    {
        [JsonProperty("recid")]
        public Guid RecId { get; set; }
        [JsonProperty("routepath")]
        public string RoutePath { get; set; }
        [JsonProperty("implementtype")]
        public int ImplementType { get; set; }
        [JsonProperty("assemblyname")]
        public string AssemblyName { get; set; }
        [JsonProperty("classtypename")]
        public string ClassTypeName { get; set; }
        [JsonProperty("funcname")]
        public string FuncName { get; set; }
        [JsonProperty("operatetype")]
        public int OperateType { get; set; }
        [JsonProperty("resulttype")]
        public int ResultType { get; set; }
        [JsonProperty("recstatus")]
        public int RecStatus { get; set; }
        [JsonProperty("entityid")]
        public Guid EntityId { get; set; }
    }

    public class ExtFunction
    {
        [JsonProperty("dbfuncid")]
        public Guid DbFuncId { get; set; }
        [JsonProperty("functionname")]
        public string FuncTionName { get; set; }
        [JsonProperty("entityid")]
        public Guid EntityId { get; set; }
        [JsonProperty("parameters")]
        public string Parameters { get; set; }
        [JsonProperty("recorder")]
        public int RecOrder { get; set; }
        [JsonProperty("returntype")]
        public int ReturnType { get; set; }
        [JsonProperty("recstatus")]
        public int RecStatus { get; set; }
        [JsonProperty("remark")]
        public string Remark { get; set; }
        [JsonProperty("enginetype")]
        public int EngineType { get; set; }
        [JsonProperty("uscript")]
        public string UScript { get; set; }
    }
}
