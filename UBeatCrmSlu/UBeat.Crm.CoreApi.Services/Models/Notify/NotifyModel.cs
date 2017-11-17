using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.Notify
{
    public class NotifyFetchModel
    {
        public Int64 RecVersion { get; set; }
    }

    public class NotifyReadModel
    {
        public string MsgIds { get; set; }
    }


    public class AddAnalyseModel
    {
        public string AnafuncName { get; set; }
        public int MoreFlag { get; set; }
        public string CountFunc { get; set; }
        public string MoreFunc { get; set; }
    }
    public class EditAnalyseModel
    {
        public string AnafuncId { get; set; }
        public string AnafuncName { get; set; }
        public int MoreFlag { get; set; }
        public string CountFunc { get; set; }
        public string MoreFunc { get; set; }
        
    }
    public class DisabledOrOderbyAnalyseModel
    {
        public string AnafuncIds { get; set; }
    }

    public class AnalyseListModel
    {
        public string AnafuncName { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }

    public class PageParamModel
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }

        public int MsgType { set; get; }
    }

}
