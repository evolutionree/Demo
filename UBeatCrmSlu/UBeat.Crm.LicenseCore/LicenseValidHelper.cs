using MessagePack;
using MessagePack.Resolvers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UBeat.Crm.LicenseCore
{
    public class LicenseValidHelper
    {

        private static readonly string _sign = "2e8c6f747312a7d52aba7aa193969d3c";

        private static readonly string _appKey = "26537";
        private static DateTime lastDateTime;
        private static bool isOK = false;

        public static DateTime OnLineTime()
        {
            if (isOK) {
                return lastDateTime;
            }
            WebRequest wrt = null;
            Task<WebResponse> wrp = null;
            try
            {
                return DateTime.Now;
            }
            catch (Exception)
            {
                isOK = true;
                lastDateTime = DateTime.Now;
                return lastDateTime;
            }
            finally
            {
                if (wrp != null)
                    wrp.Result.Dispose();
                if (wrt != null)
                    wrt.Abort();
            }
        }
    }

    [MessagePackObject]
    public class LicenseConfig
    {
        [Key("company")]
        public string Company { get; set; }
        [Key("begindate")]
        public string BeginTime { get; set; }
        [Key("enddate")]
        public string EndTime { get; set; }
        [Key("limitperson")]
        public int LimitPersonNum { get; set; }
    }

    public static class LicenseInstance
    {
        public static LicenseConfig Instance { get; set; }
    }

}
