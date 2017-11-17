using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Chat;
using UBeat.Crm.CoreApi.DomainModel.Utility;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Utility;

namespace UBeat.Crm.CoreApi.Repository.Repository.Chat
{
    public class ChatRepository : IChatRepository
    {

        public OperateResult AddGroup(GroupInsert data)
        {
            var executeSql = "select * from crm_func_chat_group_add(@chatgroupname,@pinyin,@grouptype,@entityid,@businessid,@groupicon,@members,@userno)";
            var args = new
            {
                chatgroupname = data.GroupName,
                pinyin = data.Pinyin,
                grouptype = data.GroupType,
                entityid = data.EntityId,
                businessid = data.BusinessId,
                groupicon = data.GroupIcon,
                members = data.MemberIds,
                userno = data.UserNo
            };
            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }

        public OperateResult UpdateGroup(GroupUpdate data)
        {
            var executeSql = "select * from crm_func_chat_group_update(@groupid,@groupName,@pinyin,@groupicon,@userno)";
            var args = new
            {
                groupid = data.GroupId,
                groupName = data.GroupName,
                pinyin = data.Pinyin,
                groupicon = data.GroupIcon,
                userno = data.UserNo
            };
            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }

        #region --讨论群的成员设置--
        public OperateResult AddMembers(GroupMemberAdd data)
        {
            var executeSql = "select * from crm_func_chatgroup_member_insert(@groupid,@members,@userno)";
            var args = new
            {
                groupid = data.GroupId,
                members = data.Members,
                userno = data.UserNo
            };
            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }

        /// <summary>
        /// 设置成员：设置管理员，取消管理员，设置屏蔽群，取消屏蔽群--
        /// </summary>
        public OperateResult SetMembers(GroupMemberSet data)
        {
            var executeSql = "select * from crm_func_chatgroup_member_update(@groupid,@memberid,@operatetype,@userno)";
            var args = new
            {
                groupid = data.GroupId,
                memberid = data.Memberid,
                operatetype = data.OperateType,
                userno = data.UserNo
            };
            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }

        public dynamic GetMembers(GroupMemberSelect data)
        {
            return null;
        }
        #endregion

        public OperateResult DeleteGroup(GroupDelete data)
        {
            var executeSql = "select * from crm_func_chat_group_delete(@groupid,@opertatetype,@userno)";
            var args = new
            {
                groupid = data.GroupId,
                opertatetype = data.OperateType,
                userno = data.UserNo
            };
            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }

        public dynamic Grouplist(GroupSelect data)
        {
            var executeSql = "select * from crm_func_chat_group_select(@grouptype,@entityid,@businessid,@recversion,@userno)";

            var args = new DynamicParameters();
            args.Add("grouptype", data.GroupType);
            args.Add("entityid", data.EntityId);
            args.Add("businessid", data.BusinessId);
            args.Add("recversion", data.MaxRecVersion);
            args.Add("userno", data.UserNo);
            var dataResult = DataBaseHelper.QueryStoredProcCursor(executeSql, args, CommandType.Text);
            foreach (var m in dataResult)
            {
                m["members"] = JsonHelper.ToObject<List<IDictionary<string, object>>>(m["members"].ToString());
            }
            return dataResult;
        }

        public ChatGroupModel GetGroupInfo(Guid GroupId)
        {
            var executeSql = "select * from crm_sys_chat_group where chatgroupid=@_chatgroupid";

            var args = new DynamicParameters();
            args.Add("_chatgroupid", GroupId);
           
            var dataResult = DataBaseHelper.QuerySingle<ChatGroupModel>(executeSql, args, CommandType.Text);
           
            return dataResult;
        }

        public OperateResult InsertChatMessage(ChatInsert data)
        {
            var executeSql = "select * from crm_func_chat_message_insert(@groupid,@chattype,@msgtype,@chatcon,@contype,@receivers,@userno)";
            var args = new
            {
                groupid = data.GroupId,
                chattype = data.ChatType,
                msgtype = data.MsgType,
                chatcon = data.ChatContent,
                contype = data.ContentType,
                receivers = data.FriendId,
                userno = data.UserNo
            };
            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }

        public dynamic ChatList(ChatSelect data)
        {
            var executeSql = "select * from crm_func_chat_message_list(@groupid,@friendid,@recversion,@ishistory,@userno)";

            var args = new DynamicParameters();
            args.Add("groupid", data.GroupId);
            args.Add("friendid", data.FriendId);
            args.Add("recversion", data.RecVersion);
            args.Add("ishistory", data.IsHistory);
            args.Add("userno", data.UserNo);

            return DataBaseHelper.QueryStoredProcCursor(executeSql, args, CommandType.Text);
        }
        public dynamic ChatUnreadList(int userId, long recversion =0)
        {
            var executeSql = "select * from crm_func_chat_message_unreadlist(@userno,@recversion)";

            var args = new DynamicParameters();
            args.Add("userno", userId);
            args.Add("recversion", recversion);

            return DataBaseHelper.QueryStoredProcCursor(executeSql, args, CommandType.Text);
        }

        public OperateResult ReadedCallback(List<Guid> recids, int userId)
        {
            var executeSql = "select * from crm_func_chat_message_callback(@recids,@userno)";
            var args = new
            {
                recids = recids,
                userno = userId
            };
            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }
    }
}
