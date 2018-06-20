using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.QRCode
{
    public class QRCodeEntryResultInfo
    {
        public static QRCodeEntryResultInfo NoActionResultInfo = new QRCodeEntryResultInfo() {
            ActionType = QRCodeActionTypeEnum.NoAction
        };
        public QRCodeActionTypeEnum ActionType { get; set; }
        public string QRCode { get; set; }
        public int CodeType { get; set; }
    }

    public class QRCodeSimpleMsgResultInfo: QRCodeEntryResultInfo {
        public Dictionary<string, object> DetailsInfo { get; set; }
        public int IsSuccess { get; set; }
        public int ButtonCount { get; set; }
        public QRCodeSimpleMsgButtonInfo Button1 { get; set; }
        public QRCodeSimpleMsgButtonInfo Button2 { get; set; }

    }
    public class QRCodeShowEntityUIResultInfo : QRCodeEntryResultInfo
    {
        public Dictionary<string, object> DetailsInfo { get; set; }
        public Guid EntityId { get; set; }
        public QRCodeShowEntityUIViewType ViewType { get; set; }
    }
    public class QRCodeShowCommonUIResult: QRCodeEntryResultInfo
    {
        public Dictionary<string, object> DetailsInfo { get; set; }
        public string AndroidUI { get; set; }
        public string IOSUI { get; set; }

    }
    public class QRCodeShowH5UIResultInfo: QRCodeEntryResultInfo
    {
        public Dictionary<string, object> DetailsInfo { get; set; }
        public string Url { get; set; }
    }
    public enum QRCodeShowEntityUIViewType {
        Add = 1 ,
        Edit =2,
        View = 3,
        List = -1

    }
    public class QRCodeSimpleMsgButtonInfo {
        public string Title { get; set; }
        public QRCodeSimpleMsgButtonActionEnum ActionType { get; set; }
        public string ServiceUrl { get; set; }
    }
    public enum QRCodeSimpleMsgButtonActionEnum {
        CloseAll =1,
        CloseAndKeep = 2,
        CallService = 3
    }
}
