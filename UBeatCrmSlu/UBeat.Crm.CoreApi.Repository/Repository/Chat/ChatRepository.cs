using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
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
            Guid messid = Guid.Empty;
            if (data.MessageId == null || data.MessageId.Equals(Guid.Empty) == false)
            {
                messid = Guid.NewGuid();
            }
            else {
                messid = (Guid)data.MessageId;
            }
            var executeSql = "select * from crm_func_chat_message_insert(@messageid,@groupid,@chattype,@msgtype,@chatcon,@contype,@receivers,@userno)";
            var args = new
            {
                messageid = messid,
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

        public List<IDictionary<string, object>> GetRecentChatList(DbTransaction tran, int userId)
        {
            try {
                string strSQL = @"select * 
from 
	(
		select msg.groupid::text  chatid ,chatgroup.chatgroupname   chatname ,1 chattype , '' chaticon ,max(msg.reccreated) recentlydate
		from crm_sys_chat_message  msg
				inner join crm_sys_chat_group chatgroup on msg.groupid = chatgroup.chatgroupid 
		where  msg.chattype = 1 
				and msg.groupid in(
								select  chatgroupid  
								from crm_sys_chat_group_members 
								where memberid = @userid
							)
		group by  msg.groupid,chatgroup.chatgroupname
	union all
		select chat.chatid::text  ,userinfo.username chatname ,0 chattype,userinfo.usericon chaticon ,max(recentlydate) recentlydate 
		from (
			select chatdetail.chatid,max(chatdetail.recentlydate) recentlydate
			from (
					select receivers chatid ,max(reccreated) recentlydate
					from crm_sys_chat_message 
					where reccreator = @userid and chattype = 0 
					group by receivers
				union all 
					select reccreator  chatid ,max(reccreated) recentlydate
					from crm_sys_chat_message
					where receivers = @userid and chattype = 0 
					group by reccreator
				) chatdetail 
				group by chatdetail.chatid 
			) chat 
				inner join crm_sys_userinfo userinfo on chat.chatid = userinfo.userid 
		group by chat.chatid ,userinfo.username,userinfo.usericon 
	) total 
order by total.recentlydate desc  
limit 50";
                //DbParameter[] p = new DbParameter[] {
                //    new Npgsql.NpgsqlParameter("@userid",userId)
                //};
                var p = new
                {
                    userid = userId
                };
                return DataBaseHelper.Query(strSQL, p);
            }
            catch(Exception ex) {
                return new List<IDictionary<string, object>>();
            }
        }

        public List<IDictionary<string, object>> GetRecentMsgByGroupChatIds(DbTransaction tran, List<Guid> groupchatids, int userId)
        {
            try
            {
                string strSQL = @"select s.*  
from ( 
    select a.*,b.username reccreator_name, row_number() over (partition by a.groupid order by a.reccreated desc ) as group_idx  
    from crm_sys_chat_message a
                inner join crm_sys_userinfo b on a.reccreator = b.userid
            
		where a.groupid =any(@groupids)
) s
where s.group_idx <=2
order by s.groupid ,s.reccreated desc ";
                //DbParameter[] p = new DbParameter[] {
                //    new Npgsql.NpgsqlParameter("@groupids",groupchatids.ToArray())
                //};
                var p = new
                {
                    groupids = groupchatids.ToArray()
                };
                return DataBaseHelper.Query(strSQL, p);
            }
            catch (Exception ex)
            {
                return new List<IDictionary<string, object>>();
            }
        }

        public List<IDictionary<string, object>> GetRecentMsgByPersonalChatIds(DbTransaction tran, List<int> singlechatids, int userId)
        {
            try
            {
                string strSQL = @"select * FROM(
select *, row_number() over (partition by a.relateuser order by a.reccreated desc ) as group_idx  
from (
select a.receivers relateuser ,b.username reccreator_name,a.*
from crm_sys_chat_message a 
        inner join crm_sys_userinfo b on a.reccreator = b.userid 
where a.reccreator = @userid
		and a.receivers =any(@relateusers)
	and a.chattype =0
union all 
select a.reccreator relateuser , ,b.username reccreator_name, *
from crm_sys_chat_message a
    inner join crm_sys_userinfo b on a.reccreator = b.userid 
where a.receivers = @userid
	and a.reccreator =any(@relateusers)
	and a.chattype =0
) a
)s
where s.group_idx <=2
order by s.group_idx,s.reccreated desc ";
                //DbParameter[] p = new DbParameter[] {
                //    new Npgsql.NpgsqlParameter("@relateusers",singlechatids.ToArray()),
                //    new Npgsql.NpgsqlParameter("@userid",userId)
                //};
                var p = new
                {
                    relateusers = singlechatids.ToArray(),
                    userid = userId
                };
                return DataBaseHelper.Query(strSQL, p);
            }
            catch (Exception ex)
            {
                return new List<IDictionary<string, object>>();
            }
        }
    }
}
