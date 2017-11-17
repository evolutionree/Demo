using Dapper;
using Newtonsoft.Json.Linq;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.BasicData;
using UBeat.Crm.CoreApi.DomainModel.Notify;
using UBeat.Crm.CoreApi.DomainModel.Utility;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Utility;

namespace UBeat.Crm.CoreApi.Repository.Repository.Notify
{
    public class NotifyRepository :RepositoryBase, INotifyRepository
    {
        
        public Dictionary<string, List<IDictionary<string, object>>> FetchMessage(NotifyFetchMessageMapper versionMapper, int userNumber)
        {
            var procName =
                "SELECT crm_func_notify_message_list(@recversion, @userno)";

            var dataNames = new List<string> { "notifymsg", "version" };

            var param = new
            {
                RecVersion = versionMapper.RecVersion,
                UserNo = userNumber
            };

            var resultTrd = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);

            return resultTrd;
        }

        public OperateResult WriteReadStatus(string msgIds, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_notify_message_readstatus_fetch(@msgIds, @userNo)
            ";

            var param = new
            {
                MsgIds = msgIds,
                UserNo = userNumber
            };
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public Dictionary<string, object> GetMessageList(DbTransaction tran, PageParam pageParam,int msgType, int userNumber)
        {
            var result = new Dictionary<string, object>();
            var sql = "SELECT crm_func_notify_message_list(@userno, @pageindex,@pagesize,@msgType)";
            var param = new DbParameter[]
                     {
                        new NpgsqlParameter("userno", userNumber),
                        new NpgsqlParameter("pageindex", pageParam.PageIndex),
                        new NpgsqlParameter("pagesize", pageParam.PageSize),
                        new NpgsqlParameter("msgType", msgType),
                     };
            var tempResult= DBHelper.ExecuteQueryRefCursor(tran, sql, param);
            result.Add("pagedata", tempResult["data"]);
            var page = tempResult["page"].FirstOrDefault();
            page.Add("unreadcount", tempResult["unreadcount"].FirstOrDefault()["unreadcount"]);
            result.Add("page", page);
            return result;

        }


        public OperateResult WriteMessage(NotifyEntity readModel, bool isAutoSend, int userNumber)
        {
            OperateResult resutl = new OperateResult();
            using (var conn = DBHelper.GetDbConnect())
            {
                conn.Open();
                var tran = conn.BeginTransaction();
                try
                {
                    //写离线消息
                    var sql = @"SELECT crm_func_notify_message_insert_2(
								@_msggroupid, @_msgdataid, @_entityid, @_msgtype, @_msgtitle, @_msgcontent, @_msgstatus,
								@_msgparam, @_sendtime, @_receiver, @_userno) ";

                    var param = new DbParameter[]
                    {
                        new NpgsqlParameter("_msggroupid", readModel.msggroupid),
                        new NpgsqlParameter("_msgdataid", readModel.msgdataid),
                        new NpgsqlParameter("_entityid", readModel.entityid),
                        new NpgsqlParameter("_msgtype", readModel.msgtype),
                        new NpgsqlParameter("_msgtitle", readModel.msgtitle),
                        new NpgsqlParameter("_msgcontent", readModel.msgcontent),
                        new NpgsqlParameter("_msgstatus", readModel.msgstatus ?? ""),
                        new NpgsqlParameter("_msgparam", readModel.msgparam),
                        new NpgsqlParameter("_sendtime", readModel.sendtime),
                        new NpgsqlParameter("_receiver", readModel.receiver),
                        new NpgsqlParameter("_userno", userNumber),
                    };


                    var notify_msgid = DBHelper.ExecuteScalar(tran, sql, param);
                    if (notify_msgid != null)
                    {
                        if (isAutoSend && int.Parse(notify_msgid.ToString()) > 0)
                        {
                            var notify_sync_sql = @"INSERT INTO crm_sys_notify_sync (notifyid, synctype, syncparam, reccreator) VALUES (
                                                    @_notifyid, @_synctype, @_syncparam, @_reccreator);";

                            int _synctype = 0;
                            var notify_sync_param = new DbParameter[]
                            {
                                new NpgsqlParameter("_notifyid",  int.Parse(notify_msgid.ToString())),
                                new NpgsqlParameter("_synctype", _synctype),
                                new NpgsqlParameter("_syncparam", "{}"){NpgsqlDbType = NpgsqlDbType.Jsonb },
                                new NpgsqlParameter("_reccreator", userNumber),
                            };

                            var result = DBHelper.ExecuteNonQuery(tran, notify_sync_sql, notify_sync_param);
                        }
                        tran.Commit();
                        resutl.Id = notify_msgid.ToString();
                        resutl.Flag = 1;
                    }
                    else
                    {
                        throw new Exception("写入离线消息失败，获取不到正确的msgid");
                    }
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    resutl.Flag = 0;
                    resutl.Msg = ex.Message;
                    resutl.Stacks = ex.StackTrace;

                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
                return resutl;
            }
        }

    }
}
