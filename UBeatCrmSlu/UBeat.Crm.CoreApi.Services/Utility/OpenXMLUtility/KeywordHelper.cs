using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace UBeat.Crm.CoreApi.Services.Utility.OpenXMLUtility
{
    /// <summary>
    /// 关键字解析帮助类
    /// </summary>
    public class KeywordHelper
    {
        #region ---常量定义---

        public const string CurUser = "【#CurUser#】";
        public const string CurUserName = "【#CurUser_Name#】";
        public const string CurUser_Chinese = "【#当前用户#】";
        public const string CurUserName_Chinese = "【#当前用户_Name#】";

        public const string CurUserId = "【#CurUser_Id#】";
        public const string CurUserId_Chinese = "【#当前用户_Id#】";

        public const string CurDate = "【#CurDate#】";
        public const string CurDate_Chinese = "【#当前日期#】";

        public const string CurTime = "【#CurTime#】";
        public const string CurTime_Chinese = "【#当前时间#】";

        public const string CurDept = "【#CurDept#】";
        public const string CurDept_Chinese = "【#当前部门#】";
        public const string CurDept_Name = "【#CurDept_Name#】";
        public const string CurDept_Name_Chinese = "【#当前部门_Name#】";
        public const string CurDeptId = "【#CurDept_Id#】";
        public const string CurDeptId_Chinese = "【#当前部门_Id#】";

        public const string Ent = "【#Ent#】";//当前授权企业的企业名称
        public const string Ent_Name = "【#Ent_Name#】";//当前授权企业的企业名称
        public const string Ent_Chinese = "【#企业名称#】";//当前授权企业的企业名称
        public const string Ent_Name_Chinese = "【#企业名称_Name#】";//当前授权企业的企业名称


        public const string Key_IF = @"^=IF\(\S+\)$";
        public const string Key_ElseIF = @"^=ElseIF\(\S+\)$";
        public const string Key_EndIF = "=EndIF()";
        public const string Key_Loop = @"^=Loop\(\S+\)$";
        public const string Key_EndLoop = "=EndLoop()";

        public const string Key_Sum = @"^=sum\(\S+\)$";
        public const string Key_Concat = @"^=Concat\(\S+\)$";
        public const string Key_Count = @"^=count\(\S+\)$";

        #endregion

        #region ---private Methor---
        private static bool IsEquals(string input, string constVariable)
        {
            return input.Trim().ToLower().Equals(constVariable.Trim().ToLower());
        }
        #endregion

        #region ---public Methor（固定变量）---
        /// <summary>
        /// 是否当前用户名称变量
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsCurUserName(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;
            return IsEquals(input, CurUser) || IsEquals(input, CurUserName) || IsEquals(input, CurUser_Chinese) || IsEquals(input, CurUserName_Chinese);
        }
        /// <summary>
        /// 是否当前用户ID变量
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsCurUserId(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;
            return IsEquals(input, CurUserId) || IsEquals(input, CurUserId_Chinese);
        }
        /// <summary>
        /// 是否当前日期变量
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsCurDate(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;
            return IsEquals(input, CurDate) || IsEquals(input, CurDate_Chinese);
        }
        /// <summary>
        /// 是否当前时间变量
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsCurTime(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;
            return IsEquals(input, CurTime) || IsEquals(input, CurTime_Chinese);
        }
        /// <summary>
        /// 是否当前部门名称变量
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsCurDeptName(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;
            return IsEquals(input, CurDept) || IsEquals(input, CurDept_Name) || IsEquals(input, CurDept_Chinese) || IsEquals(input, CurDept_Name_Chinese);
        }
        /// <summary>
        /// 是否当前部门id变量
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsCurDeptId(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;
            return IsEquals(input, CurDeptId) || IsEquals(input, CurDeptId_Chinese);
        }
        /// <summary>
        /// 是否企业名称变量
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsEnterpriseName(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;
            return IsEquals(input, Ent) || IsEquals(input, Ent_Name) || IsEquals(input, Ent_Chinese) || IsEquals(input, Ent_Name_Chinese);
        }
        #endregion

        #region ---public Methor（控制函数）---
        /// <summary>
        /// 是否是IF关键字
        /// </summary>
        /// <param name="input"></param>
        /// <param name="formula">返回IF内的公式内容</param>
        /// <returns></returns>
        public static bool IsKey_IF(string input, out string formula)
        {
            formula = null;
            if (string.IsNullOrEmpty(input))
                return false;
            formula = input.ToLower().Replace("=if(","").Replace(")","").Trim();
            return Regex.IsMatch(input, Key_IF);
        }
        /// <summary>
        /// 是否是ElseIF关键字
        /// </summary>
        /// <param name="input"></param>
        /// <param name="formula">返回ElseIF内的公式内容</param>
        /// <returns></returns>
        public static bool IsKey_ElseIF(string input, out string formula)
        {
            formula = null;
            if (string.IsNullOrEmpty(input))
                return false;
            formula = input.ToLower().Replace("=elseif(", "").Replace(")", "").Trim();
            return Regex.IsMatch(input, Key_ElseIF);
        }
        /// <summary>
        /// 是否是EndIF关键字
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsKey_EndIF(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;
            return IsEquals(input, Key_EndIF);
        }

        /// <summary>
        /// 是否是Loop关键字
        /// </summary>
        /// <param name="input"></param>
        /// <param name="formula">返回Loop内的公式内容</param>
        /// <returns></returns>
        public static bool IsKey_Loop(string input, out string formula)
        {
            formula = null;
            if (string.IsNullOrEmpty(input))
                return false;
            formula = input.ToLower().Replace("=loop(", "").Replace(")", "").Trim();
            return Regex.IsMatch(input, Key_Loop);
       
        }
        /// <summary>
        /// 是否是EndLoop关键字
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsKey_EndLoop(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;
            return IsEquals(input, Key_EndLoop);
        }
        #endregion

        #region ---public Methor（值函数）---
        /// <summary>
        /// 是否是Sum关键字
        /// </summary>
        /// <param name="input"></param>
        /// <param name="formula">返回Sum内的公式内容</param>
        /// <returns></returns>
        public static bool IsKey_Sum(string input, out string formula)
        {
            formula = null;
            if (string.IsNullOrEmpty(input))
                return false;
            formula = input.ToLower().Replace("=sum(", "").Replace(")", "").Trim();
            return Regex.IsMatch(input, Key_Sum);
        }
        /// <summary>
        /// 是否是concat关键字
        /// </summary>
        /// <param name="input"></param>
        /// <param name="formula">返回concat内的公式内容</param>
        /// <returns></returns>
        public static bool IsKey_Concat(string input, out string formula)
        {
            formula = null;
            if (string.IsNullOrEmpty(input))
                return false;
            formula = input.ToLower().Replace("=concat(", "").Replace(")", "").Trim();
            return Regex.IsMatch(input, Key_Concat);
        }
        /// <summary>
        /// 是否是count关键字
        /// </summary>
        /// <param name="input"></param>
        /// <param name="formula">返回count内的公式内容</param>
        /// <returns></returns>
        public static bool IsKey_Count(string input, out string formula)
        {
            formula = null;
            if (string.IsNullOrEmpty(input))
                return false;
            formula = input.ToLower().Replace("=count(", "").Replace(")", "").Trim();
            return Regex.IsMatch(input, Key_Count);
        }
        #endregion


        
    }
}
