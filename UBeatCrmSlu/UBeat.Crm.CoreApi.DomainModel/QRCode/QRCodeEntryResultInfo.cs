using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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
        public List<QRCodeSimpleMsg_FieldInfo> DetailsInfo { get; set; }
        public int IsSuccess { get; set; }
        public int ButtonCount { get; set; }
        public QRCodeSimpleMsgButtonInfo Button1 { get; set; }
        public QRCodeSimpleMsgButtonInfo Button2 { get; set; }

    }
    public class QRCodeSimpleMsg_FieldInfo {
        public string FieldName { get; set; }
        public string Title { get; set; }
        public string Value { get; set; }
        public int IsNeedEdit { get; set; }
        public int IsDisplay { get; set; }
        public QRCodeSimpleFieldTypeEnum FieldType { get; set; }
        public List<QRCodeSimpleMsg_SelectItemInfo> SelectionList { get; set; }
    }
    public class QRCodeSimpleMsg_SelectItemInfo {
        public string FieldKey { get; set; }
        public string FieldValue { get; set; }
    }
    public enum QRCodeSimpleFieldTypeEnum: Int32
    {
        Label = 1,
        TextEdit = 2,
        Selection = 3
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
    public enum QRCodeShowEntityUIViewType : Int32
    {
        Add = 1 ,
        Edit =2,
        View = 3,
        List = -1

    }
    public class QRCodeSimpleMsgButtonInfo {
        public QRCodeButtonStyleEnum ButtonStyle { get; set; }
        public string Title { get; set; }
        public QRCodeSimpleMsgButtonActionEnum ActionType { get; set; }
        public string ServiceUrl { get; set; }
    }
    public enum QRCodeButtonStyleEnum : Int32 {
        Info_Button = 1,
        Warning_Button = 2,
        Error_Button=3,
        Success_Button=4
    }
    public enum QRCodeSimpleMsgButtonActionEnum : Int32
    {
        CloseAll =1,
        CloseAndKeep = 2,
        CallService = 3
    }
}
