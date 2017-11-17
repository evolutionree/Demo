using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Core.Utility
{
    /// <summary>
    /// 规则类型
    /// </summary>
    public enum RuleType
    {

        /// <summary>
        /// 范围规则
        /// </summary>
        Range = 0,

        /// <summary>
        /// 字段规则
        /// </summary>
        Field = 1,

        /// <summary>
        /// 语句规则
        /// </summary>
        Sql = 2,

    }


    public enum Condition
    {
        IsNull,
        IsNotNull,
        Equal,
        NotEqual,
        Less,
        Greater,
        LessEqual,
        GreaterEqual,
        Between,
        Like,
        In,
        NotIn
    }

    public static class ConditionExtension
    {
        public static string GetSqlOperate(this Condition e)
        {
            var result = string.Empty;
            switch (e)
            {
                case Condition.IsNull:
                    return "IS NULL";
                case Condition.IsNotNull:
                    return "IS NOT NULL";
                case Condition.Equal:
                    return "=";
                case Condition.NotEqual:
                    return "<>";
                case Condition.Less:
                    return "<";
                case Condition.Greater:
                    return ">";
                case Condition.LessEqual:
                    return "<=";
                case Condition.GreaterEqual:
                    return ">=";
                case Condition.Between:
                    return "BETWEEN";
                case Condition.Like:
                    return "ILIKE";
                case Condition.In:
                    return "IN";
                case Condition.NotIn:
                    return "NOT IN";
                default:
                    throw new ArgumentOutOfRangeException("e");
            }
        }
    }
}
