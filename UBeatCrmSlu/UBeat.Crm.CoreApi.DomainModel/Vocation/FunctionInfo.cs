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
        public int ChildType { get; set; }
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
                    case FunctionType.Menu:
                        name = "列表菜单";
                        break;
                    case FunctionType.Function:
                        name = "列表功能";
                        break;
                    case FunctionType.Tab:
                        name = "主页tab";
                        break;
                    case FunctionType.Dynamic:
                        name = "主页动态";
                        break;
                    case FunctionType.TabFunction:
                        name = "主页Tab下功能";
                        break;
                    case FunctionType.TabDynamic:
                        name = "动态的tab";
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
    }


    /// <summary>
    /// 功能类型
    /// </summary>
    public enum FunctionType
    {
        Default = 0,
        Entity = 1,//实体
        Menu = 2,//列表菜单
        Function = 3,//列表功能
        Tab = 4,//主页tab
        Dynamic = 5,//主页动态
        TabFunction = 6,//主页Tab下功能
        TabDynamic = 7,// 动态的tab
    }

}
