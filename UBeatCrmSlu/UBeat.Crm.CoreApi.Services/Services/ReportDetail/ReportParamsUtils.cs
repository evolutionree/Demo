using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Services.ReportDetail
{
    public class ReportParamsUtils
    {
        public static string parseString(Dictionary<string, object> param, string keyname) {
            if (param == null) return null;
            if (param.ContainsKey("@" + keyname) && param["@" + keyname] != null)
            {
                return param["@" + keyname].ToString();
            }
            else if (param.ContainsKey(keyname)) {
                return param[keyname].ToString();
            }
            return null;
        }

        public static DateTime parseDateTime(Dictionary<string, object> param, string keyname) {
            if (param == null) return System.DateTime.MinValue;
            string tmp = null;
            if (param.ContainsKey("@" + keyname) && param["@" + keyname] != null)
            {
                tmp = param["@" + keyname].ToString();
            }
            else if (param.ContainsKey(keyname) && param[keyname] != null) {
                tmp = param[keyname].ToString();
            }

            try
            {
                return DateTime.Parse(tmp);
            }
            catch (Exception ex) {
                return DateTime.MinValue;
            }
        }
        public static int parseInt(Dictionary<string, object> param, string keyname) {
            if (param == null) return 0;
            int ret = 0;
            if (param.ContainsKey("@" + keyname) && param["@" + keyname] != null)
            {
                int.TryParse(param["@" + keyname].ToString(), out ret);
                return ret;
            }
            else if (param.ContainsKey(keyname))
            {
                int.TryParse(param[keyname].ToString(), out ret);
                return ret;
            }
            return 0;
        }
    }
}
