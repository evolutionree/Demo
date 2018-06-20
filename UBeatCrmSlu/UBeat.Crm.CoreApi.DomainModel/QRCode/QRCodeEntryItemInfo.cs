using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.QRCode
{
    public class QRCodeEntryItemInfo
    {
        public Guid RecId { get; set; }
        public QRCodeCheckTypeEnum CheckType { get; set; }
        public string CheckDetail { get; set; }
        public int RecOrder { get; set; }
        public int RecStatus { get; set; }
        public QRCodeDealTypeEnum DealType { get; set; }
        public string DealDetail { get; set; }
        public string Remark { get; set; }



    }
    public enum QRCodeCheckTypeEnum {
        StringSearch = 1 ,
        RegexSearch = 2,
        JSPlugInSearch = 3
    }
    public enum QRCodeActionTypeEnum {
        NoAction = 0,
        SimpleMsg = 1,
        ShowEntityUI = 2,
        ShowCommonUI= 3,
        ShowH5Page = 4
    }
    public enum QRCodeDealTypeEnum {
        CallService = 1 ,
        CallDbFunction = 2,
        JSService = 3
    }
}
