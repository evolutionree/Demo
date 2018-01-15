using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.Services.Models;
using System.Data.Common;
using UBeat.Crm.CoreApi.IRepository;

namespace UBeat.Crm.CoreApi.Services.Utility
{
    public class ReportFilterDefaultSchemeParseUtil
    {
        public static List<Dictionary<string,object>>  parseMultiScheme(string scheme, UserData userdata) {
            if (scheme == "#CurrentUser#") {
                List<Dictionary<string, object>> ret = new List<Dictionary<string, object>>();
                Dictionary<string, object> item = new Dictionary<string, object>();
                item.Add("id", userdata.UserId);
                item.Add("name", userdata.AccountUserInfo.UserName);
                ret.Add(item);
                return ret;
            }
            return null;
        }
        public static  string parseScheme(string scheme,int userid, IReportEngineRepository reportEngineRepository = null) {
            if (scheme.StartsWith("#CurYear#"))
            {
                return parseCurYear(scheme);
            }
            else if (scheme.StartsWith("#CurMonth#"))
            {
                return parseCurMonth(scheme);
            }
            else if (scheme.StartsWith("#CurDate#"))
            {
                return parseCurDate(scheme);
            }
            else if (scheme.StartsWith("#CurUserId#"))
            {
                return userid.ToString();
            }
            else if (scheme.StartsWith("#MyDept#"))
            {
                return "";
            }
            else if (scheme.StartsWith("#adjustrangetype#"))
            {
                return parseAdjustRangeType(scheme, userid, reportEngineRepository);
            }
            else if (scheme.StartsWith("#adjustrange#"))
            {
                return parseAdjustRange(scheme, userid, reportEngineRepository);
            }
            return "";
        }
        /// <summary>
        /// 计算报表默认范围类型的默认计算规则
        /// 如果是部门领导，则返回1，否则返回2
        /// </summary>
        /// <param name="scheme"></param>
        /// <param name="userid"></param>
        /// <param name="reportEngineRepository"></param>
        /// <returns></returns>
        private static string parseAdjustRangeType(string scheme, int userid, IReportEngineRepository reportEngineRepository = null) {
            if (reportEngineRepository == null) return "";
            Dictionary<string, object> detail = reportEngineRepository.getMyRangeWithType(userid,null);
            if (detail == null) return "";
            if (detail["isleader"].ToString() == "1")
            {
                return "1";
            }
            else {
                return "2";
            }
        }

        private static string parseAdjustRange(string scheme, int userid, IReportEngineRepository reportEngineRepository) {
            if (reportEngineRepository == null) return "";
            Dictionary<string, object> detail = reportEngineRepository.getMyRangeWithType(userid, null);
            if (detail == null) return "";
            if (detail["isleader"].ToString() == "1")
            {
                return (string)detail["deptid"];
            }
            else
            {
                return userid.ToString();
            }
        }
        private static string parseCurYear(string scheme) {
            scheme = scheme.Substring("#CurYear#".Length);
            int curYear = System.DateTime.Now.Year;
            if (scheme.Length > 1)
            {
                if (scheme.StartsWith("+"))
                {
                    string tmp = scheme.Substring(1);
                    int val = 0;
                    if (int.TryParse(tmp, out val))
                    {
                        curYear = curYear + val;
                    }
                    return curYear.ToString();
                }
                else if (scheme.StartsWith("-"))
                {
                    string tmp = scheme.Substring(1);
                    int val = 0;
                    if (int.TryParse(tmp, out val))
                    {
                        curYear = curYear + val;
                    }
                    return curYear.ToString();
                }
                else
                {
                    return curYear.ToString() + scheme;
                }
            }
            else
            {
                return curYear.ToString();
            }
        }
        private static string parseCurMonth(string scheme) {
            scheme = scheme.Substring("#CurMonth#".Length);
            int curMonth = System.DateTime.Now.Month;
            if (scheme.Length > 1)
            {
                if (scheme.StartsWith("+"))
                {
                    string tmp = scheme.Substring(1);
                    int val = 0;
                    if (int.TryParse(tmp, out val))
                    {
                        curMonth = curMonth + val;
                    }
                    return curMonth.ToString();
                }
                else if (scheme.StartsWith("-"))
                {
                    string tmp = scheme.Substring(1);
                    int val = 0;
                    if (int.TryParse(tmp, out val))
                    {
                        curMonth = curMonth + val;
                    }
                    return curMonth.ToString();
                }
                else
                {
                    return curMonth.ToString() + scheme;
                }
            }
            else
            {
                return curMonth.ToString();
            }
        }
        private static string parseCurDate(string scheme) {
            System.DateTime dt = System.DateTime.Now;
            scheme = scheme.Substring("#CurDate#".Length);
            if (scheme.Length > 1)
            {
                int action = 0;
                if (scheme.StartsWith("+"))
                {
                    scheme = scheme.Substring(1);
                    action = 1;
                }
                else if (scheme.StartsWith("-"))
                {
                    scheme = scheme.Substring(1);
                    action = -1;
                }
                int actiontype = 0;
                if (action != 0)
                {
                    if (scheme.EndsWith("d"))
                    {
                        scheme =scheme.Substring(0, scheme.Length - 1);
                        actiontype = 1;
                    }
                    else if (scheme.EndsWith("m"))
                    {
                        scheme = scheme.Substring(0, scheme.Length - 1);
                        actiontype = 2;
                    }
                    else if (scheme.EndsWith("y"))
                    {
                        scheme = scheme.Substring(0, scheme.Length - 1);
                        actiontype = 3;
                    }
                    int count = 0;
                    if (int.TryParse(scheme, out count))
                    {

                        if (actiontype == 1)
                        {
                            dt = dt.AddDays(action * count);
                        }
                        else if (actiontype == 2)
                        {
                            dt = dt.AddMonths(action * count);
                        }
                        else if (actiontype == 3) {
                            dt = dt.AddYears(action * count);
                        }
                        return dt.ToString("yyyy-MM-dd");
                    }
                    else {
                        return dt.ToString("yyyy-MM-dd");
                    }
                }
                else {
                    return dt.ToString("yyyy-MM-dd") + scheme;
                }
                
            }
            else
            {
                return dt.ToString("yyyy-MM-dd");
            }
        }
    }
}
