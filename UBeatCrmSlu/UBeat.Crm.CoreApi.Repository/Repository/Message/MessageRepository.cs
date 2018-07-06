using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Message;
using UBeat.Crm.CoreApi.IRepository;

namespace UBeat.Crm.CoreApi.Repository.Repository.Message
{
    public class MessageRepository : RepositoryBase, IMessageRepository
    {

        public List<MessageConfigInfo> GetMessageConfigInfo()
        {
            var executeSql = @"SELECT * FROM crm_sys_message_config;";
            return ExecuteQuery<MessageConfigInfo>(executeSql, null);
        }

        #region --写入消息--
        public bool WriteMessage(DbTransaction trans, MessageInsertInfo msgInfo, int userNumber)
        {
            if (msgInfo == null)
                return false;
            if (msgInfo.ReceiverIds == null || msgInfo.ReceiverIds.Count == 0)
            {
                return true;
            }
            Guid msgid = Guid.NewGuid();
            DateTime sendtime = DateTime.Now;
            //插入消息信息数据
            var executeSql = @"INSERT INTO crm_sys_message (msgid,entityid,businessid,msggroupid,msgstyletype,msgtitle,msgtitletip,msgcontent,msgparam,sendtime,recstatus,reccreator,recupdator,reccreated,recupdated)
                               VALUES(@msgid,@entityid,@businessid,@msggroupid,@msgstyletype,@msgtitle,@msgtitletip,@msgcontent,@msgparam,@sendtime,1,@usernum,@usernum,@sendtime,@sendtime	);";

            var param = new DbParameter[]
             {
                    new NpgsqlParameter("msgid", msgid),
                    new NpgsqlParameter("entityid", msgInfo.EntityId),
                    new NpgsqlParameter("businessid", msgInfo.BusinessId),
                    new NpgsqlParameter("msggroupid",(int)msgInfo.MsgGroupId),
                    new NpgsqlParameter("msgstyletype", (int)msgInfo.MsgStyleType),
                    new NpgsqlParameter("msgtitle", msgInfo.MsgTitle??""),
                    new NpgsqlParameter("msgtitletip", msgInfo.MsgTitleTip??""),
                    new NpgsqlParameter("msgcontent", msgInfo.MsgContent??""),
                    new NpgsqlParameter("msgparam", msgInfo.MsgpParam){  NpgsqlDbType= NpgsqlDbType.Jsonb},
                    new NpgsqlParameter("sendtime", sendtime),
                    new NpgsqlParameter("usernum", userNumber)
             };
            int result = ExecuteNonQuery(executeSql, param, trans);
            if (result <= 0)
                throw new Exception("插入消息失败");
            //插入消息接收人数据
            var executeReciverSql = @"INSERT INTO crm_sys_message_receiver (msgid, userid, readstatus) VALUES (@msgid, @userid, 0);";
            List<DbParameter[]> cmdParms = new List<DbParameter[]>();

            var userids = msgInfo.ReceiverIds.Distinct();
            foreach (var userid in userids)
            {
                if (userid > 0)
                {
                    cmdParms.Add(new DbParameter[]
                    {
                        new NpgsqlParameter("msgid", msgid),
                        new NpgsqlParameter("userid", userid)
                    });
                }
            }

            ExecuteNonQueryMultiple(executeReciverSql, cmdParms, trans);
            return result > 0;

        }
        #endregion


        /// <summary>
        /// 获取消息接收人列表
        /// </summary>
        /// <param name="entityid"></param>
        /// <param name="businessid"></param>
        /// <returns></returns>
        public List<MessageReceiverInfo> GetMessageRecevers(Guid entityid, Guid businessid)
        {
            List<DbParameter> dbParams = new List<DbParameter>();

            var msgGroupIdSql = string.Empty;

           
            var executeSql = @"SELECT mr.* FROM crm_sys_message_receiver AS mr
                                INNER JOIN crm_sys_message AS m ON m.msgid= mr.msgid
                                WHERE m.entityid=@entityid AND m.businessid=@businessid;";

            dbParams.Add(new NpgsqlParameter("entityid", entityid));
            dbParams.Add(new NpgsqlParameter("businessid", businessid));
            var resutl = ExecuteQuery<MessageReceiverInfo>(executeSql, dbParams.ToArray());
            //var resutl1 = resutl.
            return resutl;
        }

        #region --统计未读数据--
        public List<UnreadMessageInfo> StatisticUnreadMessage(List<MessageGroupType> msgGroupIds,int userNumber)
        {
            List<DbParameter> dbParams = new List<DbParameter>();

            var msgGroupIdSql = string.Empty;
            
            if (msgGroupIds != null && msgGroupIds.Count > 0)
            {
                msgGroupIdSql = " AND m.msggroupid = ANY(@msggroupids)";
                dbParams.Add(new NpgsqlParameter("msggroupids", msgGroupIds.Cast<int>().ToArray()));
            }
            
            var executeSql = string.Format(@"SELECT m.msggroupid, COUNT(1) AS Count FROM crm_sys_message_receiver AS mr
                                LEFT JOIN crm_sys_message AS m ON m.msgid= mr.msgid
                                WHERE (mr.userid=@userid) AND mr.readstatus=0 {0} 
                                GROUP BY m.msggroupid;", msgGroupIdSql);

            dbParams.Add(new NpgsqlParameter("userid", userNumber));
            var resutl = ExecuteQuery<UnreadMessageInfo>(executeSql, dbParams.ToArray());
            //var resutl1 = resutl.
            return resutl;
        }

        #endregion

        #region --增量获取消息列表--
        public IncrementPageDataInfo<MessageInfo> GetMessageList(Guid entityId, Guid businessId, List<MessageGroupType> msgGroupIds,List<MessageStyleType> msgStyleType, IncrementPageParameter incrementPage, int userNumber)
        {
            List<DbParameter> dbParams = new List<DbParameter>();

            var versionSql = string.Empty;
            var limitSql = string.Empty;
            var orderby = "DESC";
            var entityIdSql = string.Empty;
            var businessIdSql = string.Empty;
            var msgGroupIdSql = string.Empty;
            var msgStyleTypeSql = string.Empty;

            //处理增量参数
            if (incrementPage == null)
                throw new Exception("增量参数不可为空");
            else
            {
                //判断增量方向,如果版本号为小于等于0的数据，则获取最新N条数据
                if (incrementPage.Direction != IncrementDirection.None && incrementPage.RecVersion > 0)
                {
                    versionSql = string.Format(@" AND m.recversion{0}@recversion", incrementPage.Direction == IncrementDirection.Forward ? "<" : ">");
                    orderby = incrementPage.Direction == IncrementDirection.Forward ? "DESC" : "ASC";
                    dbParams.Add(new NpgsqlParameter("recversion", incrementPage.RecVersion));
                }
                //设置增量页大小
                if (incrementPage.PageSize > 0)
                {
                    limitSql = @"LIMIT @limitcount";
                    dbParams.Add(new NpgsqlParameter("limitcount", incrementPage.PageSize));
                }

            }
            //处理业务条件
            if (entityId != Guid.Empty)
            {
                entityIdSql = " AND m.entityid=@entityid";
                dbParams.Add(new NpgsqlParameter("entityid", entityId));
            }
            if (businessId != Guid.Empty)
            {
                businessIdSql = " AND m.businessid=@businessid";
                dbParams.Add(new NpgsqlParameter("businessid", businessId));
            }
            if (msgGroupIds != null && msgGroupIds.Count > 0)
            {
                msgGroupIdSql = " AND m.msggroupid = ANY(@msggroupids)";
                dbParams.Add(new NpgsqlParameter("msggroupids", msgGroupIds.Cast<int>().ToArray()));
            }
            //msgStyleType
            if (msgStyleType != null && msgStyleType.Count > 0)
            {
                msgStyleTypeSql = " AND m.msgstyletype = ANY(@msgstyletypes)";
                dbParams.Add(new NpgsqlParameter("msgstyletypes", msgStyleType.Cast<int>().ToArray()));
            }


            var executeSql = string.Format(@"SELECT m.*,mr.readstatus,mr.userid,e.entityname AS EntityName ,e.modeltype AS EntityModel,u.username AS RecCreatorName,u.usericon AS RecCreatorIcon 
                                FROM crm_sys_message_receiver AS mr
                                LEFT JOIN crm_sys_message AS m ON m.msgid= mr.msgid
                                LEFT JOIN crm_sys_userinfo AS u ON m.reccreator = u.userid
                                LEFT JOIN crm_sys_entity AS e ON e.entityid = m.entityid
                                WHERE (mr.userid=@userid and mr.readstatus != 3) {0} {1} {2} {3} {4}
                                ORDER BY m.recversion {5}
                                {6};", versionSql, entityIdSql, businessIdSql, msgGroupIdSql, msgStyleTypeSql, orderby, limitSql);

            dbParams.Add(new NpgsqlParameter("userid", userNumber));

            var result = new IncrementPageDataInfo<MessageInfo>();
            result.DataList = ExecuteQuery<MessageInfo>(executeSql, dbParams.ToArray());
            if (result.DataList.Count > 0)
            {
                //result.DataList = result.DataList.OrderByDescending(m => m.RecCreated).ToList();
                var firstRowVersion = result.DataList.FirstOrDefault().RecVersion;
                var lastRowVersion = result.DataList.LastOrDefault().RecVersion;
                result.PageMaxVersion = Math.Max(firstRowVersion, lastRowVersion);
                result.PageMinVersion = Math.Min(firstRowVersion, lastRowVersion);
            }
            return result;
        }
        #endregion

        #region --分页获取消息列表--
        public PageDataInfo<MessageInfo> GetMessageList(Guid entityId, Guid businessId, List<MessageGroupType> msgGroupIds, List<MessageStyleType> msgStyleType, int pageIndex, int pageSize, int userNumber)
        {
            List<DbParameter> dbParams = new List<DbParameter>();

            var entityIdSql = string.Empty;
            var businessIdSql = string.Empty;
            var msgGroupIdSql = string.Empty;
            var msgStyleTypeSql = string.Empty;

            //处理业务条件
            if (entityId != Guid.Empty)
            {
                entityIdSql = " AND m.entityid=@entityid";
                dbParams.Add(new NpgsqlParameter("entityid", entityId));
            }
            if (businessId != Guid.Empty)
            {
                businessIdSql = " AND m.businessid=@businessid";
                dbParams.Add(new NpgsqlParameter("businessid", businessId));
            }
            if (msgGroupIds != null && msgGroupIds.Count > 0)
            {
                msgGroupIdSql = " AND m.msggroupid = ANY(@msggroupids)";
                dbParams.Add(new NpgsqlParameter("msggroupids", msgGroupIds.Cast<int>().ToArray()));
            }
            if (msgStyleType != null && msgStyleType.Count > 0)
            {
                msgStyleTypeSql = " AND m.msgstyletype = ANY(@msgstyletypes)";
                dbParams.Add(new NpgsqlParameter("msgstyletypes", msgStyleType.Cast<int>().ToArray()));
            }

            var executeSql = string.Format(@"SELECT m.*,mr.readstatus,mr.userid,e.entityname AS EntityName ,e.modeltype AS EntityModel,u.username AS RecCreatorName,u.usericon AS RecCreatorIcon FROM crm_sys_message_receiver AS mr
                                LEFT JOIN crm_sys_message AS m ON m.msgid= mr.msgid
                                LEFT JOIN crm_sys_userinfo AS u ON m.reccreator = u.userid
                                LEFT JOIN crm_sys_entity AS e ON e.entityid = m.entityid
                                WHERE (mr.userid=@userid) {0} {1} {2} {3}
                                ORDER BY m.recversion ASC ", entityIdSql, businessIdSql, msgGroupIdSql, msgStyleTypeSql);

            dbParams.Add(new NpgsqlParameter("userid", userNumber));
            var result= ExecuteQueryByPaging<MessageInfo>(executeSql, dbParams.ToArray(), pageSize, pageIndex);
            result.DataList = result.DataList.OrderByDescending(m => m.RecCreated).ToList();
            return result;
        }
        #endregion

        public void UpdateMessageStatus(List<MsgWriteBackInfo> messages, int userNumber)
        {
            if (messages == null)
                return;
            Guid msgid = Guid.NewGuid();
            DateTime sendtime = DateTime.Now;
            //插入消息信息数据
            var executeSql = @"UPDATE crm_sys_message_receiver SET readstatus=@readstatus WHERE msgid=@msgid AND userid=@userid;";
            var paramList = new List<DbParameter[]>();
            foreach(var m in messages)
            {
                if (m == null)
                    continue;
                 var param = new DbParameter[]
                 {
                     new NpgsqlParameter("readstatus", m.MsgStatus),
                     new NpgsqlParameter("userid", userNumber),
                     new NpgsqlParameter("msgid", m.MsgId),
                 };
                paramList.Add(param);
            }

            ExecuteNonQueryMultiple(executeSql, paramList);
        }

        public void MessageWriteBack(List<Guid> messageids, int userNumber)
        {
            if (messageids == null)
                return;
            Guid msgid = Guid.NewGuid();
            DateTime sendtime = DateTime.Now;
            //插入消息信息数据
            var executeSql = @"UPDATE crm_sys_message_receiver SET readstatus=2 WHERE msgid=ANY(@messageids) AND userid=@userid;";

            var param = new DbParameter[]
             {
                    new NpgsqlParameter("userid", userNumber),
                    new NpgsqlParameter("messageids", messageids.ToArray()),
             };
            ExecuteNonQuery(executeSql, param);

        }
        /// <summary>
        /// 修改消息状态
        /// </summary>
        /// <param name="msgGroupIds"></param>
        /// <param name="readstatus">消息状态，0未读 1已查 2已读 3删除</param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public void UpdateMessageStatus(DbTransaction tran, int msgGroupId, List<Guid> msgIds, int readstatus, int userNumber)
        {
            List<DbParameter> parm = new List<DbParameter>();
            string sql = @"Update crm_sys_message_receiver set readstatus = @readstatus 
where userid = @userid and msgid IN (SELECT msgid from crm_sys_message where msggroupid = @msggroupids) and readstatus != 3 and readstatus != @readstatus "; // readstatus != 3 不对已有删除标识的数据做处理
            parm.Add(new NpgsqlParameter("readstatus", readstatus));
            parm.Add(new NpgsqlParameter("userid", userNumber));
            parm.Add(new NpgsqlParameter("msggroupids", msgGroupId));
            if (msgIds != null && msgIds.Count() > 0)
            {
                sql += " and msgid = ANY(@msgid)";
                parm.Add(new NpgsqlParameter("msgid", msgIds.ToArray()));
            }
            ExecuteNonQuery(sql, parm.ToArray(), tran);
        }

        public void UpdateMessageBizStatus(DbTransaction tran, List<MsgWriteBackBizStatusInfo> messages, int userNumber)
        {
            Guid msgid = Guid.NewGuid();
            DateTime sendtime = DateTime.Now;
            //插入消息信息数据
            var executeSql = @"UPDATE crm_sys_message_receiver SET bizstatus=@bizstatus  WHERE msgid=@msgid and userid=@userid";
            foreach (MsgWriteBackBizStatusInfo msg in messages) {

                var param = new DbParameter[]
                 {
                    new NpgsqlParameter("bizstatus", msg.BizStatus),
                    new NpgsqlParameter("msgid", msg.MsgId),
                    new NpgsqlParameter("userid",msg.ReceiverId)
                 };
                ExecuteNonQuery(executeSql, param);
            }
        }

        public List<MsgWriteBackBizStatusInfo> GetWorkflowMsgList(DbTransaction tran,Guid bizId, Guid caseId,int stepnum, int handlerUserId)
        {
            try {
                string sql = @"select a.msgid,b.userid ReceiverId,b.bizstatus 
from crm_sys_message a 
	inner join crm_sys_message_receiver b on a.msgid = b.msgid 
where a.msgstyletype =5 and a.businessid = @businessid 
     and  ((msgparam->>'Data')::jsonb->>'caseid')::uuid = @caseid 
		and ((msgparam->>'Data')::jsonb->>'stepnum') = @stepnum::text";
                var param = new DbParameter[]
                 {
                    new NpgsqlParameter("caseid",caseId),
                    new NpgsqlParameter("stepnum",stepnum),
                    new NpgsqlParameter("businessid",bizId)
                 };
                return ExecuteQuery<MsgWriteBackBizStatusInfo>(sql, param, tran);
            }
            catch(Exception e)
            {
                return new List<MsgWriteBackBizStatusInfo>();
            }
        }
    }
}
