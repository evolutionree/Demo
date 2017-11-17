using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace UBeat.Crm.CoreApi.DomainModel.Utility
{
    public static class JsonHelper
    {
        private static readonly JsonSerializerSettings JsonSettings;

        static JsonHelper()
        {
            JsonSettings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
                ContractResolver = new LowerCasePropertyNamesContractResolver(),
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
        }

        public static string ToJson(object obj)
        {
            try
            {
                return JsonConvert.SerializeObject(obj, JsonSettings);
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static T ToObject<T>(string json)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(json)) return default(T);
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        public static JArray ToJsonArray(this string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JArray.Parse(json);
        }

        public static Dictionary<string, object> ToJsonDictionary(this string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new Dictionary<string, object>();
            }
            try
            {
                var obj = JToken.Parse(json);
            }
            catch (Exception)
            {
                throw new FormatException("不符合json格式.");
            }

            return JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        }
    }
}
