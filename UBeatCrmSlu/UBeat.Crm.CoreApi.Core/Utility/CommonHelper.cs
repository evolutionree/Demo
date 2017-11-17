using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;


namespace UBeat.Crm.CoreApi.Core.Utility
{
    public class CommonHelper
    {
        /// <summary>
        /// 数据验证类使用的正则表述式选项
        /// </summary>
        private const RegexOptions Options = RegexOptions.IgnoreCase | RegexOptions.Compiled;
        /// <summary>
        /// 检测字符串是否为有效的邮件地址捕获正则 new Regex(@"^[A-Za-z0-9_-]+@qq\.com$").Match(str).Success;
        /// </summary>
        private static readonly Regex EmailRegex = new Regex(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$", Options);

        public static bool IsNumeric(string value)
        {
            return Regex.IsMatch(value, @"^[+-]?\d*[.]?\d*$");
        }
        public static bool IsInt(string value)
        {
            return Regex.IsMatch(value, @"^[+-]?\d*$");
        }

        public static bool IsMatchNumber(string value)
        {
            return IsNumeric(value) || IsInt(value) || IsPlaceHolder(value);
        }
        public static bool IsPlaceHolder(string value)
        {
            return Regex.IsMatch(value, @"[{\w}]");
        }
        /// <summary>
        /// 是否为日期型字符串
        /// </summary>
        /// <param name="StrSource">日期字符串(2008-05-08)</param>
        /// <returns></returns>
        public static bool IsDate(string value)
        {
            return Regex.IsMatch(value, @"^((((1[6-9]|[2-9]\d)\d{2})-(0?[13578]|1[02])-(0?[1-9]|[12]\d|3[01]))|(((1[6-9]|[2-9]\d)\d{2})-(0?[13456789]|1[012])-(0?[1-9]|[12]\d|30))|(((1[6-9]|[2-9]\d)\d{2})-0?2-(0?[1-9]|1\d|2[0-9]))|(((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))-0?2-29-))$");
        }

        /// <summary>
        /// 是否为时间型字符串
        /// </summary>
        /// <param name="source">时间字符串(15:00:00)</param>
        /// <returns></returns>
        public static bool IsTime(string value)
        {
            return Regex.IsMatch(value, @"^((20|21|22|23|[0-1]?\d):[0-5]?\d:[0-5]?\d)$");
        }

        /// <summary>
        /// 是否为日期+时间型字符串
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsDateTime(string value)
        {
            return Regex.IsMatch(value, @"^(((((1[6-9]|[2-9]\d)\d{2})-(0?[13578]|1[02])-(0?[1-9]|[12]\d|3[01]))|(((1[6-9]|[2-9]\d)\d{2})-(0?[13456789]|1[012])-(0?[1-9]|[12]\d|30))|(((1[6-9]|[2-9]\d)\d{2})-0?2-(0?[1-9]|1\d|2[0-8]))|(((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))-0?2-29-)) (20|21|22|23|[0-1]?\d):[0-5]?\d:[0-5]?\d)$ ");
        }

        public static bool IsMatchDateTime(string value)
        {
            return IsDateTime(value) || IsDate(value) || IsTime(value) || IsPlaceHolder(value); ;
        }

        public static bool IsMatchGuid(string value)
        {
            Guid val;
            return Guid.TryParse(value, out val);
        }

        public static string GetEmailType(string emailAddress)
        {
            Regex regex = new Regex(@"^[A-Za-z0-9_-]+@qq\.com$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            if (regex.IsMatch(emailAddress))
            {
                return "QQ";
            }
            else
            {
                return string.Empty;
            }
        }

        private static object lockThis = new object(); //原子锁


        public static String GetEncryptDataFileContent()
        {
            string path = Directory.GetCurrentDirectory();
            if (string.IsNullOrEmpty(path))
            {
                throw new Exception("应用主程序目录不存在");
            }
            path = path + "//encryptdata.dat";
            lock (lockThis)
            {
                FileStream fs = new FileStream(path, FileMode.Open);
                StreamReader sr = new StreamReader(fs);
                try
                {
                    String line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        return line.ToString();
                    }
                    throw new Exception();
                }
                catch
                {
                    throw new Exception("读取文件异常");
                }
                finally
                {
                    fs.Dispose();
                    sr.Dispose();
                }
            }
        }

        /// <summary>
        /// 字典类型转化为对象
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        public T DicToObject<T>(Dictionary<string, object> dic) where T : new()
        {
            var md = new T();
            //if(CultureInfo.CurrentCulture==null)
            //     CultureInfo.CurrentCulture= new CultureInfo("zh-CN");

            foreach (var d in dic)
            {
                var filed = d.Key;
                var value = d.Value;
                md.GetType().GetProperty(filed).SetValue(md, value);
            }
            return md;
        }


        ///   <summary>
        ///   去除HTML标记
        ///   </summary>
        ///   <param   name=”NoHTML”>包括HTML的源码   </param>
        ///   <returns>已经去除后的文字</returns>
        public static string NoHTML(string Htmlstring)
        {
            //删除脚本
            Htmlstring = Regex.Replace(Htmlstring, @"(\<script(.+?)\</script\>)|(\<style(.+?)\</style\>)", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            //删除HTML 
            Htmlstring = Regex.Replace(Htmlstring, @"<(.[^>]*)>", "",
            RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"([\r\n])[\s]+", "",
            RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"–>", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"<!–.*", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(quot|#34);", "\"",
            RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(amp|#38);", "&",
            RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(lt|#60);", "<",
            RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(gt|#62);", ">",
            RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(nbsp|#160);", "   ",
            RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(iexcl|#161);", "\xa1", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(cent|#162);", "\xa2", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(pound|#163);", "\xa3", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(copy|#169);", "\xa9", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&#(\d+);", "", RegexOptions.IgnoreCase);
            Htmlstring.Replace("<", "");
            Htmlstring.Replace(">", "");
            Htmlstring.Replace("\r\n", "");
            return Htmlstring;
        }
    }
}
