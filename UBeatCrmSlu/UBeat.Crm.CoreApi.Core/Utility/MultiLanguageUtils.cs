using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Core.Utility
{
    public class MultiLanguageUtils
    {
        public static string CN = "cn";
        public static string EN = "en";
        public static string DefaultLanguage = "cn";
        public static string GetDefaultLanguageValue(Dictionary<string, object> multilanguage)
        {
            if (multilanguage == null) return null;
            foreach (var item in multilanguage)
            {
                if (item.Key.ToLower() == DefaultLanguage)
                {
                    if (item.Value == null) return null;
                    else return item.Value.ToString();
                }
            }
            return null;
        }

        public static  bool IsSupportLanguage(string ln) {
            if (ln == CN) return true;
            if (ln == EN) return true;
            return false;
        }
        public static string GetDefaultLanguageValue(Dictionary<string, string> multilanguage)
        {
            if (multilanguage == null) return null;
            foreach (var item in multilanguage)
            {
                if (item.Key.ToLower() == DefaultLanguage)
                {
                    if (item.Value == null) return null;
                    else return item.Value.ToString();
                }
            }
            return null;
        }

        public static string GetDefaultLanguageValue(string DefaultValue,Dictionary<string, string> multilanguage,out string Result)
        {
            Result = string.Concat(DefaultValue).Trim();
            
            if (multilanguage == null) return Result;
            foreach (var item in multilanguage)
            {
                if (item.Key.ToLower() == DefaultLanguage)
                {
                    if (item.Value == null) return Result;
                    else {
                        Result= string.Concat(item.Value).Trim();
                        return Result;
                    }
                }
            }
            return null;
        }
    }
}
