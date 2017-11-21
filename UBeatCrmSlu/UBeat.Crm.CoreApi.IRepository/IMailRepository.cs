using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.EMail;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IMailRepository : IBaseRepository
    {
        /// <summary>
        /// 列举邮件信息
        /// </summary>
        /// <param name="paramInfo"></param>
        /// <param name="userId"></param>
        /// <param name="tran"></param>
        /// <returns></returns>
        PageDataInfo<MailBodyMapper> ListMail(MailListActionParamInfo paramInfo, string orderbyfield, string keyWord, int userId, DbTransaction tran = null);
        /// <summary>
        /// 对邮件打标记或者取消标记
        /// </summary>
        /// <param name="mailids"></param>
        /// <param name="userNum"></param>
        /// <param name="tran"></param>
        /// <returns></returns>
        OperateResult TagMails(string mailids, MailTagActionType actionType, int userNum);
        List<MailUserMapper> GetManagerContactAndInnerUser(int userId);
        List<MailBoxMapper> GetIsWhiteList(int isWhiteLst, int userId);
        bool IsHasSubUserAuth(int leaderUserId, int userId);

        PageDataInfo<TransferRecordMapper> GetInnerTransferRecord(TransferRecordParamMapper entity, int userId);

        OperateResult DeleteMails(DeleteMailMapper entity, int userId);

        OperateResult ReConverMails(ReConverMailMapper entity, int userId);

        OperateResult ReadMail(ReadOrUnReadMailMapper entity, int userId);

        Dictionary<string, object> MailDetail(MailDetailMapper entity, int userId);

        IList<MailAttachmentMapper> MailAttachment(List<Guid> mailIds);
        OperateResult InnerTransferMail(TransferMailDataMapper entity, int userId, DbTransaction tran = null);

        OperateResult MoveMail(MoveMailMapper entity, int userId, DbTransaction tran = null);

        List<MailUserMapper> GetContactByKeyword(string keyword, int count, int userId);

        List<OrgAndStaffMapper> GetInnerContact(string deptId, int userId);

        PageDataInfo<MailUserMapper> GetCustomerContact(int pageIndex, int pageSize, int userId);

        PageDataInfo<MailBodyMapper> GetInnerToAndFroMail(ToAndFroMapper entity, int userId);

        PageDataInfo<MailUserMapper> GetRecentContact(int pageIndex, int pageSize, int userId);

        PageDataInfo<ToAndFroFileMapper> GetInnerToAndFroAttachment(ToAndFroMapper entity, int userId);

        List<InnerToAndFroUser> GetInnerToAndFroUser(string keyword, int userId);

        PageDataInfo<AttachmentChooseListMapper> GetLocalFileFromCrm(AttachmentListMapper entity, string ruleSql, int userId);

        PageDataInfo<MailBox> GetMailBoxList(int pageIndex, int pageSize, int userId);
    }
}
