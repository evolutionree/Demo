﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Message;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IMessageRepository
    {

        List<MessageConfigInfo> GetMessageConfigInfo();

        /// <summary>
        /// 获取消息接收人列表
        /// </summary>
        /// <param name="entityid"></param>
        /// <param name="businessid"></param>
        /// <returns></returns>
        List<MessageReceiverInfo> GetMessageRecevers(Guid entityid, Guid businessid);

        bool WriteMessage(DbTransaction trans ,MessageInsertInfo entityInfo, int userNumber);

        List<UnreadMessageInfo> StatisticUnreadMessage(List<MessageGroupType> msgGroupIds, int userNumber);

        IncrementPageDataInfo<MessageInfo> GetMessageList(Guid entityId, Guid businessId, List<MessageGroupType> msgGroupIds, List<MessageStyleType> msgStyleType, IncrementPageParameter incrementPage, int userNumber);

        PageDataInfo<MessageInfo> GetMessageList(Guid entityId, Guid businessId, List<MessageGroupType> msgGroupIds, List<MessageStyleType> msgStyleType, int pageIndex , int pageSize, int userNumber);

        void MessageWriteBack(List<Guid> messageids, int userNumber);
        /// <summary>
        /// 更新消息状态
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="userNumber"></param>
        void UpdateMessageStatus(List<MsgWriteBackInfo> messages, int userNumber);

        void UpdateMessageStatus(DbTransaction tran, int msgGroupId, List<Guid> msgIds, int readstatus, int userNumber);
        void UpdateMessageBizStatus(DbTransaction tran, List<MsgWriteBackBizStatusInfo> messages,int userNumber);
        List<MsgWriteBackBizStatusInfo> GetWorkflowMsgList(DbTransaction tran, Guid bizId,Guid caseId,int stepnum, int handlerUserId);

        PageDataInfo<Dictionary<string, object>> GetDynamicsUnMsg(UnHandleMsgMapper msg, int userId);
        PageDataInfo<Dictionary<string, object>> GetWorkFlowsMsg(UnHandleMsgMapper msg, int userId);
        List<Dictionary<string, object>> GetMessageCount(DbTransaction tran, int userId);
    
    }
}
