using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Notice
{
    public class NoticeReceiverInfo
    {
        /// <summary>
        /// 公告通知ID
        /// </summary>
        public Guid NoticeId { set; get; }
        /// <summary>
        /// 接收人ID
        /// </summary>
        public int UserId { set; get; }
        /// <summary>
        /// 是否弹窗 1弹窗 0 不弹窗
        /// </summary>
        public int IsPopup { set; get; }
        /// <summary>
        /// 0未读1已读
        /// </summary>
        public int ReadFlag { set; get; }
        /// <summary>
        /// 状态 1启用 0停用
        /// </summary>
        public int RecStatus { set; get; }
        /// <summary>
        /// 发送人
        /// </summary>
        public int RecCreator { set; get; }
        /// <summary>
        /// 发送时间
        /// </summary>
        public DateTime RecCreated { set; get; }

    }
}
