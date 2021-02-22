using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.GL.Model
{
    public class SAPDelivnoteInfo
    {
    }

    public class SapDelivnoteDATA
    {
        /// <summary>
        /// 
        /// </summary>
        public List<Dictionary<string, object>> LIKP { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<Dictionary<string, object>> LIPS { get; set; }
    }

    public class SapDelivnoteResult
    {
        /// <summary>
        /// 
        /// </summary>
        public string TYPE { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string MESSAGE { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public SapDelivnoteDATA DATA { get; set; }
    }

    public class SapDeliveryCreateModelResult
    {
        public string TYPE { get; set; }
        public string MESSAGE { get; set; }
        public string JHDH { get; set; }

    }
}
