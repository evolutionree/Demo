using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Notice;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface INoticeRepository
    {
        Dictionary<string, List<IDictionary<string, object>>> NoticeQuery(NoticeListMapper notice, int userNumber);

        Dictionary<string, List<IDictionary<string, object>>> NoticeMobQuery(NoticeListMapper notice, int userNumber);

        Dictionary<string, List<IDictionary<string, object>>> NoticeSendRecordQuery(NoticeSendRecordMapper notice, int userNumber);

        Dictionary<string, List<IDictionary<string, object>>> NoticeVersionHistoryQuery(NoticeListMapper notice, int userNumber);
        IDictionary<string, object> NoticeInfoQuery(NoticeListMapper notice, int userNumber);
        OperateResult InsertNotice(DbTransaction transaction,NoticeMapper notice, int userNumber);
        OperateResult UpdateNotice(DbTransaction transaction, NoticeMapper notice, int userNumber);

        OperateResult DisabledNotice(DbTransaction transaction,NoticeDisabledMapper notice, int userNumber);
        OperateResult UpdateNoticeReadFlag(NoticeReadFlagMapper notice, int userNumber);
        OperateResult SendNoticeToUser(DbTransaction transaction, NoticeReceiverMapper noticeReceiver, int userNumber);

        /// <summary>
        /// 获取通知的接收人列表
        /// </summary>
        /// <param name="noticeid"></param>
        /// <returns></returns>
        List<NoticeReceiverInfo> GetNoticeReceivers(Guid noticeid);

    }
}
