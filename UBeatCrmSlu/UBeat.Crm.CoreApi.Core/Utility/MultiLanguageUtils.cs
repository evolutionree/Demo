﻿using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Core.Utility
{
    public class MultiLanguageUtils
    {
        public static string DefaultLanguage = "CN";
        public static string GetDefaultLanguageValue(Dictionary<string, object> multilanguage)
        {
            if (multilanguage == null) return null;
            foreach (var item in multilanguage)
            {
                if (item.Key.ToUpper() == DefaultLanguage)
                {
                    if (item.Value == null) return null;
                    else return item.Value.ToString();
                }
            }
            return null;
        }

        public static string GetDefaultLanguageValue(Dictionary<string, string> multilanguage)
        {
            if (multilanguage == null) return null;
            foreach (var item in multilanguage)
            {
                if (item.Key.ToUpper() == DefaultLanguage)
                {
                    if (item.Value == null) return null;
                    else return item.Value.ToString();
                }
            }
            return null;
        }

        public static string GetDefaultLanguageValue(string DefaultValue,Dictionary<string, string> multilanguage,out string Result)
        {
            Result = DefaultValue;
            
            if (multilanguage == null) return Result;
            foreach (var item in multilanguage)
            {
                if (item.Key.ToUpper() == DefaultLanguage)
                {
                    if (item.Value == null) return Result;
                    else {
                        Result= item.Value;
                        return Result;
                    }
                }
            }
            return null;
        }
    }
}