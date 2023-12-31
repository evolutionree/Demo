﻿using System;
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

        PageDataInfo<MailBodyMapper> InnerToAndFroListMail(InnerToAndFroMailMapper entity, int userId);
        /// <summary>
        /// 对邮件打标记或者取消标记
        /// </summary>
        /// <param name="mailids"></param>
        /// <param name="userNum"></param>
        /// <param name="tran"></param>
        /// <returns></returns>
        OperateResult TagMails(string mailids, MailTagActionType actionType, int userNum);
        List<MailBoxMapper> GetIsWhiteList(int isWhiteLst, int userId);
        /// <summary>
        /// 我负责的客户的联系人和内部人员
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        List<InnerUserMailMapper> GetUserMailList(int userId);
        bool IsHasSubUserAuth(int leaderUserId, int userId);

        PageDataInfo<TransferRecordMapper> GetInnerTransferRecord(TransferRecordParamMapper entity, int userId);

        OperateResult DeleteMails(DeleteMailMapper entity, int userId,DbTransaction _dbTrans);

        OperateResult ReConverMails(ReConverMailMapper entity, int userId, DbTransaction dbTrans = null);

        OperateResult ReadMail(ReadOrUnReadMailMapper entity, int userId);

        Dictionary<string, object> MailDetail(MailDetailMapper entity, int userId);

        IList<MailAttachmentMapper> MailAttachment(List<Guid> mailIds);
        OperateResult InnerTransferMail(TransferMailDataMapper entity, int userId, DbTransaction tran = null);

        OperateResult MoveMail(MoveMailMapper entity, int userId, DbTransaction tran = null);

        List<MailUserMapper> GetContactByKeyword(string keyword, int count, int userId);

        List<OrgAndStaffMapper> GetInnerContact(string deptId, int userId);

        PageDataInfo<OrgAndStaffMapper> GetInnerPersonContact(string keyword, int pageIndex, int pageSize, int userId);

        PageDataInfo<OrgAndStaffMapper> TransferInnerPersonContact(string keyword, int pageIndex, int pageSize, int userId);

        List<OrgAndStaffMapper> TransferInnerContact(string deptId, int userId);

        PageDataInfo<MailUserMapper> GetCustomerContact(string keyword,int pageIndex, int pageSize, int userId);

        PageDataInfo<MailBodyMapper> GetInnerToAndFroMail(ToAndFroMapper entity, int userId);

        PageDataInfo<MailUserMapper> GetRecentContact(int pageIndex, int pageSize, int userId);

        PageDataInfo<ToAndFroFileMapper> GetInnerToAndFroAttachment(ToAndFroMapper entity, int userId);

        List<OrgAndStaffTree> GetInnerToAndFroUser(string keyword, int userId);

        PageDataInfo<AttachmentChooseListMapper> GetLocalFileFromCrm(AttachmentListMapper entity, string ruleSql, int userId);

        PageDataInfo<MailBox> GetMailBoxList(int pageIndex, int pageSize, int userId);

        bool IsDraftMail(Guid mailId, int userId);
        ReceiveMailRelatedMapper GetUserReceiveMailTime(string mailAddress, int userId);

        List<ReceiveMailRelatedMapper> GetReceiveMailRelated(int userId);

        MailBodyMapper GetMailInfo(List<Guid> mailIds, int userId);

        OperateResult MirrorWritingMailStatus(Guid mailId, int mailStatus, int userId,DbTransaction dbTrans = null);

        PageDataInfo<MailTruncateLstMapper> GetReconvertMailList(ReconvertMailMapper entity, int userId);

        UserMailInfo GetEnablePassword(Guid mailBoxId, int userId);

        OperateResult DeleteMailDraft(Guid mailId, DbTransaction dbTrans, int userId);
    }
}
