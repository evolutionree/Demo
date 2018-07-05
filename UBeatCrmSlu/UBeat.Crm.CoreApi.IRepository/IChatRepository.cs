using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Chat;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IChatRepository
    {
        OperateResult AddGroup(GroupInsert data);

        OperateResult UpdateGroup(GroupUpdate data);

        OperateResult AddMembers(GroupMemberAdd data);

        OperateResult SetMembers(GroupMemberSet data);

        dynamic GetMembers(GroupMemberSelect data);

        OperateResult DeleteGroup(GroupDelete data);

        dynamic Grouplist(GroupSelect data);

        ChatGroupModel GetGroupInfo(Guid GroupId);


        OperateResult InsertChatMessage(ChatInsert data);

        dynamic ChatList(ChatSelect data);

        dynamic ChatUnreadList(int userId, long recversion = 0);

        OperateResult ReadedCallback(List<Guid> recids, int userId);
        List<IDictionary<string, object>> GetRecentChatList(DbTransaction tran, int userId);
        List<IDictionary<string, object>> GetRecentMsgByGroupChatIds(DbTransaction tran, List<Guid> groupchatids, int userId);
        List<IDictionary<string, object>> GetRecentMsgByPersonalChatIds(DbTransaction tran, List<int> singlechatids, int userId);
        void UpdateMembers(GroupMemberAdd crmData, int userId);
    }
}
