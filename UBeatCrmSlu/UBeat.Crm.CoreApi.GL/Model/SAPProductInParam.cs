using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.GL.Model
{
    public class SAPProductInParam
    {
        public int TYPE { get; set; }
        public SAPProduct DATA { get; set; }
    }
    public class SAPProduct
    {
        public String GUID { get; set; }
        public String MODE { get; set; }
        public List<Dictionary<String, String>> MARA { get; set; }
        public List<Dictionary<String, String>> MAKT { get; set; }
        public List<Dictionary<String, String>> MARM { get; set; }
        public List<Dictionary<String, String>> MARC { get; set; }
        public List<Dictionary<String, String>> MBEW { get; set; }
        public List<Dictionary<String, String>> QMAT { get; set; }
        public List<Dictionary<String, String>> MVKE { get; set; }
        public List<Dictionary<String, String>> MLAN { get; set; }
    }
}
