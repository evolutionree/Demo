using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.Notice
{
    public class NoticeListModel
    {
        public string NoticeId { get; set; }
        public int NoticeType { get; set; }

        public string KeyWord { get; set; }
        public string NoticeTitle { get; set; }
        public string HeadImg { get; set; }
        public string HeadRemark { get; set; }
        public string MsgContent { get; set; }
        public string NoticeUrl { get; set; }
        public int RecStatus { get; set; }
        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public int RecVersion { get; set; }
        public int NoticeSendStatus { get; set; }
    }
    public class NoticeModel
    {
        public string NoticeId { get; set; }
        public int NoticeType { get; set; }
        public string NoticeTitle { get; set; }
        public string HeadImg { get; set; }
        public string HeadRemark { get; set; }
        public string MsgContent { get; set; }
        public string NoticeUrl { get; set; }
        public int RecStatus { get; set; }
        public int RecVersion { get; set; }
        public int NoticeSendStatus { get; set; }
    }

    public class NoticeSendRecordModel
    {
        public Guid NoticeId { get; set; }
        public string KeyWord { get; set; }
        public int ReadFlag { get; set; }
        public Guid DeptId { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }

    public class NoticeReceiverModel
    {
        public string NoticeId { get; set; }
        public string UserIds { get; set; }
        public ICollection<NoticeReceiverDeptModel> DeptIds { get; set; }
        public int IspopUp { get; set; }

    }

    public class NoticeReceiverDeptModel
    {
        public string DeptId { get; set; }

        public ICollection<string> RoleIds { get; set; }
    }

    public class NoticeDisabledModel
    {
        public string NoticeIds { get; set; }
    }

    public class NoticeReadFlagModel
    {
        public string NoticeId { get; set; }
        public int UserId { get; set; }
    }
}
