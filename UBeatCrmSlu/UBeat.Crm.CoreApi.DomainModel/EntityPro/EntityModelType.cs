using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.EntityPro
{
    /// <summary>
    /// 实体模型类型
    /// </summary>
    public enum EntityModelType
    {
       
        /// <summary>
        /// 独立实体
        /// </summary>
        Independent = 0,
        /// <summary>
        /// 嵌套实体
        /// </summary>
        Nested = 1,
        /// <summary>
        /// 简单(应用)实体
        /// </summary>
        Simple = 2,
        /// <summary>
        /// 动态实体
        /// </summary>
        Dynamic = 3,

    }
}
