using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Notice;
using UBeat.Crm.CoreApi.Repository.Utility;
using System.Linq;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.DomainModel.Utility;
using System.Data.Common;
using Npgsql;

namespace UBeat.Crm.CoreApi.Repository.Repository.Notice
{
    public class NoticeRepository : RepositoryBase, INoticeRepository
    {
        public Dictionary<string, List<IDictionary<string, object>>> NoticeQuery(NoticeListMapper notice, int userNumber)
        {
            var procName =
                "SELECT crm_func_notice_list(@noticetype,@keyword,@noticesendstatus,@pageindex,@pagesize,@userno)";

            var dataNames = new List<string> { "PageData", "PageCount" };
            var param = new DynamicParameters();
            param.Add("noticetype", notice.NoticeType);
            param.Add("keyword", notice.KeyWord);
            param.Add("noticesendstatus", notice.NoticeSendStatus);
            param.Add("pageindex", notice.PageIndex);
            param.Add("pagesize", notice.PageSize);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }

        public Dictionary<string, List<IDictionary<string, object>>> NoticeVersionHistoryQuery(NoticeListMapper notice, int userNumber)
        {
            var procName =
                "SELECT crm_func_notice_history_list(@version,@userno)";

            var dataNames = new List<string> { "NoticeHistoryList" };
            var param = new DynamicParameters();
            param.Add("version", notice.RecVersion);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }

        public Dictionary<string, List<IDictionary<string, object>>> NoticeMobQuery(NoticeListMapper notice, int userNumber)
        {
            var procName =
                "SELECT crm_func_notice_mob_list(@noticetype,@keyword,@noticesendstatus,@pageindex,@pagesize,@userno)";

            var dataNames = new List<string> { "PageData", "PageCount" };
            var param = new DynamicParameters();
            param.Add("noticetype", notice.NoticeType);
            param.Add("keyword", notice.KeyWord);
            param.Add("noticesendstatus", notice.NoticeSendStatus);
            param.Add("pageindex", notice.PageIndex);
            param.Add("pagesize", notice.PageSize);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }
        public Dictionary<string, List<IDictionary<string, object>>> NoticeSendRecordQuery(NoticeSendRecordMapper notice, int userNumber)
        {
            var procName =
                "SELECT crm_func_notice_send_record_list(@noticeid,@keyword,@readstatus,@deptid,@pageindex,@pagesize,@userno)";

            var dataNames = new List<string> { "PageData", "PageCount" };
            var param = new DynamicParameters();
            param.Add("noticeid", notice.NoticeId);
            param.Add("keyword", notice.KeyWord);
            param.Add("readstatus", notice.ReadFlag);
            param.Add("deptid", notice.DeptId);
            param.Add("pageindex", notice.PageIndex);
            param.Add("pagesize", notice.PageSize);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }
        public IDictionary<string, object> NoticeInfoQuery(NoticeListMapper notice, int userNumber)
        {
            var procName =
                "SELECT crm_func_notice_info(@noticeid,@userno)";

            var dataNames = new List<string> { "NoticeInfo" };
            var param = new DynamicParameters();
            param.Add("noticeid", notice.NoticeId);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QueryStoredProcCursor(procName, param, CommandType.Text).FirstOrDefault();
            var dataDic = new Dictionary<string, object>();
            dataDic.Add("NoticeInfo", result);
            return result;
        }
        public OperateResult InsertNotice(DbTransaction transaction, NoticeMapper notice, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_notice_add(@noticetype,@noticetitle, @headimg, @headmark, @msgcontent, @noticeurl, @userno)
            ";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("noticetype",notice.NoticeType),
                new NpgsqlParameter("noticetitle",notice.NoticeTitle),
                new NpgsqlParameter("headimg",notice.HeadImg),
                new NpgsqlParameter("headmark",notice.HeadRemark),
                new NpgsqlParameter("msgcontent",notice.MsgContent),
                new NpgsqlParameter("noticeurl",notice.NoticeUrl),
               new NpgsqlParameter("userno",userNumber),
            };
            var result = DBHelper.ExecuteQuery<OperateResult>(transaction, sql, param);
            return result.FirstOrDefault();
        }

        public OperateResult UpdateNotice(DbTransaction transaction, NoticeMapper notice, int userNumber)
        {
            var sql = @"
                     SELECT * FROM crm_func_notice_edit(@noticeid,@noticetype,@noticetitle, @headimg, @headmark, @msgcontent, @noticeurl, @userno)
            ";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("noticeid",notice.NoticeId),
                new NpgsqlParameter("noticetype",notice.NoticeType),
                new NpgsqlParameter("noticetitle",notice.NoticeTitle),
                new NpgsqlParameter("headimg",notice.HeadImg),
                new NpgsqlParameter("headmark",notice.HeadRemark),
                new NpgsqlParameter("msgcontent",notice.MsgContent),
                new NpgsqlParameter("noticeurl",notice.NoticeUrl),
               new NpgsqlParameter("userno",userNumber),
            };
            var result = DBHelper.ExecuteQuery<OperateResult>(transaction, sql, param);
            return result.FirstOrDefault();
        }

        public OperateResult DisabledNotice(DbTransaction transaction, NoticeDisabledMapper notice, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_notice_disabled(@noticeids,@status, @userno)
            ";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("status",(object)0),
                new NpgsqlParameter("noticeids", notice.NoticeIds),
               new NpgsqlParameter("userno",userNumber),
            };
            var result = DBHelper.ExecuteQuery<OperateResult>(transaction, sql, param);
            return result.FirstOrDefault();

        }
        public OperateResult UpdateNoticeReadFlag(NoticeReadFlagMapper notice, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_notice_readflag(@noticeid,@userid, @userno)
            ";
            var param = new DynamicParameters();
            param.Add("userid", notice.UserId);
            param.Add("noticeid", notice.NoticeId);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }
        public OperateResult SendNoticeToUser(DbTransaction transaction, NoticeReceiverMapper noticeReceiver, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_notice_receiver_add(@noticeids,@ispopup,@userids,@deptjson, @userno)
            ";
            var deptJson = JsonHelper.ToJson(noticeReceiver.deptids);
            var param = new DbParameter[]
           {
                new NpgsqlParameter("userids", noticeReceiver.UserIds),
                new NpgsqlParameter("deptjson", deptJson),
                new NpgsqlParameter("noticeids", noticeReceiver.NoticeId),
               new NpgsqlParameter("ispopup", noticeReceiver.IspopUp),
               new NpgsqlParameter("userno",userNumber),
            };
            var result = DBHelper.ExecuteQuery<OperateResult>(transaction, sql, param);
            return result.FirstOrDefault();
        }


        /// <summary>
        /// 获取通知的接收人列表
        /// </summary>
        /// <param name="noticeid"></param>
        /// <returns></returns>
        public List<NoticeReceiverInfo> GetNoticeReceivers(Guid noticeid)
        {
            var sql = @"
                SELECT * FROM crm_sys_notice_receiver WHERE noticeid=@noticeid
            ";
          
            var param = new DbParameter[]
           {
                new NpgsqlParameter("noticeid", noticeid),
              
            };
            return ExecuteQuery<NoticeReceiverInfo>(sql, param);
            
        }
    }
}
