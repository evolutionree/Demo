using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.Rule;

namespace UBeat.Crm.CoreApi.DomainModel.Vocation
{
    /// <summary>
    /// 功能信息
    /// </summary>
    public class FunctionInfo
    {
        public FunctionInfo()
        {

        }
        public FunctionInfo(Guid id,Guid parentId,string funcname,string funccode,Guid entityid,int devicetype,FunctionType recType,int isLastChild,string relationValue,string routePath, RuleInfo rule=null)
        {
            FuncId = id;
            ParentId = parentId;
            FuncName = funcname;
            Funccode = funccode;
            EntityId = entityid;
            DeviceType = devicetype;
            RecType = recType;
            IsLastChild = isLastChild;
            RelationValue = relationValue;
            RoutePath = routePath;
            Rule = rule;
        }

        //public Guid VocationId { set; get; }


        public Guid FuncId { set; get; }
        /// <summary>
        /// 功能名称
        /// </summary>
        public string FuncName { set; get; }

        public string Funccode { set; get; }

        /// <summary>
        /// 父级节点ID
        /// </summary>
        public Guid ParentId { set; get; }
        /// <summary>
        /// 实体ID
        /// </summary>
        public Guid EntityId { set; get; }

        /// <summary>
        /// 设备类型,0:web,1:mobile
        /// </summary>
        public int DeviceType { set; get; }

        /// <summary>
        /// 功能类型
        /// </summary>
        public FunctionType RecType { set; get; }
        //已废弃
        [JsonIgnore]
        public int ChildType { get; set; }
        /// <summary>
        /// -1表示可以配置数据权限，其他值不可配置
        /// </summary>
        public int IsLastChild { set; get; }

        public string RecTypeName {
            get
            {
                string name = null;
                switch(RecType)
                {
                    case FunctionType.Entity:
                        name = "实体";
                        break;
                    case FunctionType.EntityMenu:
                        name = "列表菜单";
                        break;
                    case FunctionType.EntityFunc:
                        name = "列表功能";
                        break;
                    case FunctionType.EntityTab:
                        name = "主页tab";
                        break;
                    case FunctionType.EntityDynamicTab:
                        name = "主页动态";
                        break;
                    case FunctionType.Function:
                        name = "功能节点";
                        break;                   
                }
                return name;
            }
        }

        public string RelationValue { set; get; }

        /// <summary>
        /// api 路由path
        /// </summary>
        public string RoutePath { set; get; }



        public RuleInfo Rule { set; get; }


        public RuleInfo BasicRule{ get; set; }
    }

    /// <summary>
    /// 功能类型
    /// </summary>
    public enum FunctionType
    {
        Function = 0,//功能节点
        Entity = 1,//实体
        EntityMenu = 2,//实体菜单
        EntityFunc = 3,//实体功能
        EntityTab = 4,//主页Tab
        EntityDynamicTab = 5,//主页动态Tab
        DocumentList=9,//文档
    }

}
