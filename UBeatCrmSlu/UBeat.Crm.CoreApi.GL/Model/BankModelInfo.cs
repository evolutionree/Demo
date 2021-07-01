using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.ZGQY.Model
{
   public class BankModelInfo
    {
        public BankModelInfo()
        {
        }
    }


    public class SapBankModelResult
    {
        public string TYPE { get; set; }
        public string MESSAGE { get; set; }
        public List<Dictionary<string,object>> DATA { get; set; }

    }

}
