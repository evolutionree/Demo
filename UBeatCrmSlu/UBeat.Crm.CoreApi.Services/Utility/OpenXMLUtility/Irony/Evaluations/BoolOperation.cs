using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Utility.OpenXMLUtility.Irony.Evaluations
{
    internal enum BoolOperation
    {
        /// <summary>
        /// 大于
        /// </summary>
        GreaterThan,
        /// <summary>
        /// 大于等于
        /// </summary>
        GreaterThanEqual,
        /// <summary>
        /// 小于
        /// </summary>
        LessThan,
        /// <summary>
        /// 小于等于
        /// </summary>
        LessThanEqual,
        /// <summary>
        /// 等于
        /// </summary>
        Equal,
        /// <summary>
        /// 等于
        /// </summary>
        NotEqual,

        And,

        OR
    }
}
