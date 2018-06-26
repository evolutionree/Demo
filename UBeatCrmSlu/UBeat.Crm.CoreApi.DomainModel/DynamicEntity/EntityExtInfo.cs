using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.DynamicEntity
{
    /// <summary>
    /// UK100实体引擎中关于扩展函数（数据库）的定义
    /// </summary>
    public class EntityExtFunctionInfo
    {
        /// <summary>
        /// 函数的id， 关键字
        /// </summary>
        public Guid DbFuncId { get; set; }
        /// <summary>
        /// 实体ID
        /// </summary>
        public Guid EntityId { get; set; }
        /// <summary>
        /// 实际数据库函数的名字，必须全小写
        /// </summary>
        public string FunctionName { get; set; }
        /// <summary>
        /// 参数列表,以@开头，以,分隔
        /// </summary>
        public string Parameters { get; set; }
        /// <summary>
        /// 显示顺序
        /// </summary>
        public int RecOrder { get; set; }
        /// <summary>
        /// 函数的返回类型
        /// </summary>
        public EntityExtFunctionReturnType ReturnType { get; set; }
        /// <summary>
        /// 函数的可用状态
        /// </summary>
        public int RecStatus { get; set; }

    }
    public enum EntityExtFunctionReturnType {
        /// <summary>
        /// 不返回任何数据
        /// </summary>
        NoReturn=0,
        /// <summary>
        /// 返回单记录表（可能多条记录)
        /// </summary>
        SingleQuery = 1,
        /// <summary>
        /// 返回多记录表
        /// </summary>
        MultiCursor=2,
        SingleRow = 3,
    }

}