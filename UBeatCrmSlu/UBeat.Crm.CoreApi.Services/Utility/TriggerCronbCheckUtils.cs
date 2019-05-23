using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Utility
{
    public class TriggerCronbCheckUtils
    {
        private static string[] MonthForEnglish = new string[] {
            "没有定义","JAN","FEB","MAR","APR","MAY","JUN",
            "JUL","AUG","SEP","OCT","NOV","DEC"
        };
        private static string[] WeekForEnglish = new string[] {
            "NO","SUN","MON","TUE","WEN","THU","FRI","SAT"
        };
        public static bool Match(DateTime dt, string cronbstring) {
            if (cronbstring == null || cronbstring.Length == 0) return false;
            cronbstring = cronbstring.Trim();
            string[] subitems = cronbstring.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (subitems.Length != 6 && subitems.Length != 7) return false;
            if (MatchSecond(dt, subitems[0]) == false) return false;
            if (MatchMinute(dt, subitems[1]) == false) return false;
            if (MatchHour(dt, subitems[2]) == false) return false;
            if (MatchDayOfMonth(dt, subitems[3]) == false) return false;
            if (MatchMonth(dt, subitems[4]) == false) return false;
            if (MatchDayOfWeek(dt, subitems[5]) == false) return false;
            if (subitems.Length == 7 && MatchYear(dt, subitems[6]) == false) return false;
            return true;
        }
        private static bool CheckMatch1(int curItem, string item) {
            if (item[0] == '*') return true;
            int count = 0;
            if (item.IndexOf(',') >= 0) count++;
            if (item.IndexOf('/') >= 0) count++;
            if (item.IndexOf('-') >= 0) count++;
            if (count == 0)
            {
                int result = -1;
                if (Int32.TryParse(item, out result) == false) return false;
                if (curItem == result) return true;
                else return false;
            }
            else if (count == 1)
            {
                if (item.IndexOf(',') >= 0)
                {
                    //这个可能有多个情况
                    string[] tmp = item.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    foreach (string tmpItem in tmp)
                    {
                        int i = -1;
                        if (Int32.TryParse(tmpItem, out i))
                        {
                            if (i == curItem) return true;
                        }
                    }
                    return false;
                }
                else if (item.IndexOf('-') >= 0)
                {
                    string[] tmp = item.Split("-".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (tmp.Length != 2) return false;
                    int from = -1, to = -1;
                    if (Int32.TryParse(tmp[0], out from) == false || Int32.TryParse(tmp[1], out to) == false) return false;
                    if (from <= curItem && curItem <= to) return true;
                    return false;
                }
                else
                {
                    string[] tmp = item.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (tmp.Length != 2) return false;
                    int from = -1, step = -1;
                    if (Int32.TryParse(tmp[0], out from) == false || Int32.TryParse(tmp[1], out step) == false) return false;
                    if (step < 0)
                    {
                        return false;
                    }
                    else if (step == 0)
                    {
                        if (curItem == from) return true;
                        else return false;
                    }
                    else
                    {
                        if (((curItem + from) % step) == 0) return true;
                        else return false;
                    }
                }
            }
            else
            {
                return false;//错误的数据，直接返回false
            }
        }
        /// <summary>
        /// 匹配秒定义
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private static bool MatchSecond(DateTime dt, string item) {
            int curItem = dt.Second;
            return CheckMatch1(curItem, item);
        }
        /// <summary>
        /// 匹配分钟
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private static bool MatchMinute(DateTime dt, string item) {
            int curItem = dt.Minute;
            return CheckMatch1(curItem, item);
        }
        /// <summary>
        /// 匹配小时
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private static bool MatchHour(DateTime dt, string item) {
            int curItem = dt.Hour;
            return CheckMatch1(curItem, item);
        }
        /// <summary>
        /// 匹配日
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private static bool MatchDayOfMonth(DateTime dt, string item) {
            int curItem = dt.Day;
            if (item.IndexOf('?') >= 0 ) return true;
            item = item.ToUpper();
            if (item.IndexOf('W') < 0 && item.IndexOf('L') < 0) return CheckMatch1(curItem, item);
            if (item.IndexOf("LW") >= 0 || item.IndexOf("WL") >= 0) {
                if ((int)dt.DayOfWeek >= 1 && (int)dt.DayOfWeek <= 5) {
                    if (dt.Month != dt.AddDays(1).Month) {
                        return true;//当前是工作日，且是本月的最后一天
                    }
                    if (dt.DayOfWeek == DayOfWeek.Friday
                        && (dt.Month != dt.AddDays(1).Month || dt.Month != dt.AddDays(2).Month
                                || dt.Month != dt.AddDays(3).Month)) {
                        return true;//
                    }
                }
                return false;
            }
            if (item.IndexOf("W") >= 0) {
                string tmp = item.Replace("W", "");
                if (tmp.Length == 0)
                {
                    return false;
                }
                else {
                    int needDay = -1;
                    if (Int32.TryParse(tmp, out needDay) == false) return false;
                    int dif = needDay - dt.Day;
                    DateTime dt2 = dt.AddDays(dif);
                    if (dt2.Month != dt.Month) return false;//本月没有这个日期
                    if (dt2.DayOfWeek == DayOfWeek.Sunday)
                    {
                        //周日的处理方法,优先找周一，如果周一超月，则找上周五
                        if (dt2.AddDays(1).Month == dt2.Month)
                        {
                            if (dt2.AddDays(1).Day == dt.Day) return true;
                            else return false;
                        }
                        else {
                            if (dt2.AddDays(-2).Day == dt.Day) return true;
                            else return false;
                        }
                    }
                    else if (dt2.DayOfWeek == DayOfWeek.Saturday)
                    {
                        //周六的处理方法 ,优先找上周五，如果是上个月，则找下周一
                        if (dt2.AddDays(-1).Month == dt2.Month)
                        {
                            if (dt2.AddDays(-1).Day == dt.Day) return true;
                            else return false;
                        }
                        else {
                            if (dt2.AddDays(2).Day == dt.Day) return true;
                            else return false;
                        }
                    }
                    else {
                        if (dif == 0) return true;
                        else return false;
                    }
                }
            }
            if (item.IndexOf("L") >= 0) {
                if (dt.AddDays(1).Month == dt.Month) return true;
                else return false;
            }
            return false;
        }
        private static bool MatchMonth(DateTime dt, string item) {
            if (item == null || item.Length == 0) return false;
            item = item.ToUpper();
            for (int i = 1; i < MonthForEnglish.Length; i++)
            {
                item = item.Replace(MonthForEnglish[i], i.ToString());
            }
            int curItem = dt.Month;
            return CheckMatch1(curItem, item);
        }
        private static bool MatchDayOfWeek(DateTime dt, string item) {
            if (item == null || item.Length == 0) return false;
			if (item.IndexOf('?') >= 0) return true;

			item = item.ToUpper();
            int curItem = ((int)dt.DayOfWeek) + 1;
            if (item.IndexOf("L") < 0 && item.IndexOf('#') < 0) return CheckMatch1(curItem, item);
            if (item.IndexOf("L") >= 0) {
                if (item.IndexOf("L") == 0)
                {
                    if (dt.DayOfWeek == DayOfWeek.Saturday) return true;
                    else return false;
                }
                else {
                    string tmpItem = item.Replace("L", "");
                    int tmpI = -1;
                    if (Int32.TryParse(tmpItem, out tmpI) == false) return false;
                    if (((int)dt.DayOfWeek)+1 != tmpI) return false;
                    if (dt.AddDays(7).Month == dt.Month) return true;
                    else return false;
                }
            }
            if (item.IndexOf("#") >= 0) {
                string[] tmpItem = item.Split("#".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (tmpItem.Length != 2) return false;
                int DayInWeek = -1;
                int WeekInMonth = -1;
                if (Int32.TryParse(tmpItem[0], out DayInWeek) == false || Int32.TryParse(tmpItem[1], out WeekInMonth) == false) return false;
                if (((int)dt.DayOfWeek) != DayInWeek - 1) return false;
                int day = dt.Day - (int)dt.DayOfWeek;
                if ((WeekInMonth - 2) * 7 + 2 <= day
                        && day <= (WeekInMonth - 1) * 7 + 1)
                {
                    return true;
                }
                else {
                    return false;
                }
            }
            return false;
        }
        private static bool MatchYear(DateTime dt, string item) {
            int curItem = dt.Year;
            return CheckMatch1(curItem, item);
            return false;
        }
    }
}
