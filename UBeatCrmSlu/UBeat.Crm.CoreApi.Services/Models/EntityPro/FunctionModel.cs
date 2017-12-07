using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.EntityPro
{
    
    public class FunctionModel
    {
        public FunctionModel()
        {

        }
        public FunctionModel(Guid id, Guid parentId, string funcname, string funccode, Guid entityid,  int isLastChild, string relationValue, string routePath)
        {
            FuncId = id;
            ParentId = parentId;
            FuncName = funcname;
            Funccode = funccode;
            EntityId = entityid;
            IsLastChild = isLastChild;
            RelationValue = relationValue;
            RoutePath = routePath;
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
        /// -1表示可以配置数据权限，其他值不可配置
        /// </summary>
        public int IsLastChild { set; get; }

        public string RelationValue { set; get; }

        /// <summary>
        /// api 路由path
        /// </summary>
        public string RoutePath { set; get; }
        
    }
}
