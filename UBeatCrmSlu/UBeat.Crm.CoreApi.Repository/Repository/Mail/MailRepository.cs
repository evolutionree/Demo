﻿using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.EMail;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Utility;
using System.Linq;
using Dapper;

namespace UBeat.Crm.CoreApi.Repository.Repository.Mail
{
    public class MailRepository : RepositoryBase, IMailRepository
    {
        /// <summary>
        /// 根据条件返回邮件列表
        /// </summary>
        /// <param name="paramInfo"></param>
        /// <param name="orderbyfield"></param>
        /// <param name="userId"></param>
        /// <param name="tran"></param>
        /// <returns></returns>
        public PageDataInfo<MailBodyMapper> ListMail(MailListActionParamInfo paramInfo, string orderbyfield, string keyWord, int userId, DbTransaction tran = null)
        {
            string strSQL = @"SELECT " +
                                        "body.recid mailid," +
                                        "(SELECT row_to_json(t) FROM (SELECT mailaddress,displayname FROM crm_sys_mail_senderreceivers WHERE ctype=1 AND mailid=body.recid LIMIT 1) t)::jsonb sender," +
                                        "(SELECT array_to_json(array_agg(row_to_json(t))) FROM (SELECT mailaddress,displayname FROM crm_sys_mail_senderreceivers WHERE ctype=2 AND mailid=body.recid ) t)::jsonb receivers," +
                                        "(SELECT array_to_json(array_agg(row_to_json(t))) FROM (SELECT mailaddress,displayname FROM crm_sys_mail_senderreceivers WHERE ctype=3 AND mailid=body.recid ) t)::jsonb ccers," +
                                        "(SELECT array_to_json(array_agg(row_to_json(t))) FROM (SELECT mailaddress,displayname FROM crm_sys_mail_senderreceivers WHERE ctype=4 AND mailid=body.recid ) t)::jsonb bccers," +
                                        "body.title," +
                                        "body.mailbody," +
                                        "body.senttime," +
                                        "body.receivedtime," +
                                        "body.istag," +
                                        "body.isread," +
                                        "(SELECT COUNT(1) FROM crm_sys_mail_attach WHERE mailid=body.recid) attachcount" +
                                        " FROM crm_sys_mail_mailbody body Where body.recstatus=1 AND body.recid IN (SELECT mailid FROM crm_sys_mail_catalog_relation WHERE catalogid=@catalogid)  {0} {1} ";
            object[] sqlWhere = new object[] { };
            string sqlCondition = string.Empty;
            if (!string.IsNullOrEmpty(keyWord))
            {
                sqlCondition = string.Format(" AND ((body.sender ILIKE '%' || @{0} || '%' ESCAPE '`') OR (body.title ILIKE '%' || @{0} || '%' ESCAPE '`') OR (body.receivers ILIKE @{0} || '%' ESCAPE '`'))", keyWord);
                sqlWhere.Concat(new object[] { sqlCondition });
            }

            sqlCondition = string.Join(" AND ", sqlWhere);


            if (paramInfo.pageSize <= 0)
            {
                paramInfo.pageSize = 1000000;
            }
            if (paramInfo.PageIndex <= 0)
            {
                paramInfo.PageIndex = 1;
            }
            orderbyfield = string.Format(@"order by {0} desc", orderbyfield);
            strSQL = string.Format(strSQL, sqlCondition, orderbyfield);
            var arg = new
            {
                catalogid = paramInfo.Catalog
            };
            return ExecuteQueryByPaging<MailBodyMapper>(strSQL, new DbParameter[] { new NpgsqlParameter("catalogid", paramInfo.Catalog), new NpgsqlParameter("keyword", keyWord) }, paramInfo.pageSize, (paramInfo.PageIndex - 1) * paramInfo.pageSize);
        }

        public OperateResult TagMails(string mailids, MailTagActionType actionType, int userId)
        {
            var preSql = "SELECT count(1) hashdata FROM crm_sys_mail_catalog WHERE recid IN(SELECT catalogid FROM crm_sys_mail_catalog_relation  WHERE mailid =@mailid ) AND viewuserid = @userid ";
            foreach (var tmp in mailids.Split(','))
            {
                var args = new
                {
                    MailId = Guid.Parse(tmp),
                    UserId = userId
                };
                var result = DataBaseHelper.QuerySingle<int>(preSql, args, CommandType.Text);
                if (result == 0)
                {
                    return new OperateResult
                    {
                        Flag = 1,
                        Msg = "正在被标记的邮件中有不属于你的邮件,不允许标记"
                    };
                }
            }
            var sql = "update crm_sys_mail_mailbody set istag=@istag where recid IN (select regexp_split_to_table(@mailids,',')::uuid);";
            var param = new
            {
                IsTag = (int)actionType,
                MailIds = mailids
            };
            try
            {
                var result = DataBaseHelper.ExecuteNonQuery(sql, param, CommandType.Text);
                if (result > 0)
                {
                    return new OperateResult
                    {
                        Flag = 1,
                        Msg = "标记邮件成功"
                    };
                }
                return new OperateResult
                {
                    Flag = 0,
                    Msg = "标记邮件失败"
                };
            }
            catch (Exception ex)
            {
                return new OperateResult
                {
                    Flag = 0,
                    Msg = "标记邮件异常"
                };
            }
        }

        public OperateResult DeleteMails(DeleteMailMapper entity, int userId)
        {
            var preSql = "SELECT count(1) hashdata FROM crm_sys_mail_catalog WHERE recid IN(SELECT catalogid FROM crm_sys_mail_catalog_relation  WHERE mailid =@mailid ) AND viewuserid = @userid ";
            foreach (var tmp in entity.MailIds.Split(','))
            {
                var args = new
                {
                    MailId = Guid.Parse(tmp),
                    UserId = userId
                };
                var result = DataBaseHelper.QuerySingle<int>(preSql, args, CommandType.Text);
                if (result == 0)
                {
                    return new OperateResult
                    {
                        Flag = 1,
                        Msg = "正在被删除的邮件中有不属于你的邮件,不允许删除"
                    };
                }
            }
            string sql = string.Empty;

            if (entity.IsTruncate)
                sql = "Update  crm_sys_mail_mailbody set recstatus=2 where recid IN (select regexp_split_to_table(@mailids,',')::uuid);" +
                   "Update  crm_sys_mail_attach set recstatus=2  where mailid IN (select regexp_split_to_table(@mailids,',')::uuid);";
            else
                sql = "update crm_sys_mail_mailbody set recstatus=0 where recid IN (select regexp_split_to_table(@mailids,',')::uuid);" +
                    "update crm_sys_mail_catalog_relation set catalogid=(SELECT recid FROM crm_sys_mail_catalog WHERE viewuserid = @userid AND ctype = 1006 LIMIT 1) where recid IN (select regexp_split_to_table(@mailids,',')::uuid);";

            try
            {
                var param = new
                {
                    MailIds = entity.MailIds,
                    UserId = userId
                };
                var result = DataBaseHelper.ExecuteNonQuery(sql, param, CommandType.Text);
                if (result > 0)
                {
                    return new OperateResult
                    {
                        Flag = 1,
                        Msg = "删除邮件成功"
                    };
                }
                return new OperateResult
                {
                    Flag = 0,
                    Msg = "删除邮件失败"
                };
            }
            catch (Exception ex)
            {
                return new OperateResult
                {
                    Flag = 0,
                    Msg = "删除邮件异常"
                };
            }
        }

        public OperateResult ReConverMails(ReConverMailMapper entity, int userId)
        {
            string sql = "update crm_sys_mail_mailbody set recstatus=@recstatus where recid IN (select regexp_split_to_table(@mailids,',')::uuid);";

            try
            {
                var param = new
                {
                    MailIds = entity.MailIds,
                    RecStatus = entity.RecStatus
                };
                var result = DataBaseHelper.ExecuteNonQuery(sql, param, CommandType.Text);
                if (result > 0)
                {
                    return new OperateResult
                    {
                        Flag = 1,
                        Msg = "恢复邮件成功"
                    };
                }
                return new OperateResult
                {
                    Flag = 0,
                    Msg = "恢复邮件失败"
                };
            }
            catch (Exception ex)
            {
                return new OperateResult
                {
                    Flag = 0,
                    Msg = "恢复邮件异常"
                };
            }
        }

        public OperateResult ReadMail(ReadOrUnReadMailMapper entity, int userId)
        {
            var sql = "update crm_sys_mail_mailbody set isread=@isread where recid  IN (select regexp_split_to_table(@mailids,',')::uuid);";
            try
            {
                var param = new
                {
                    MailIds = entity.MailIds,
                    IsRead = entity.IsRead
                };
                var result = DataBaseHelper.ExecuteNonQuery(sql, param, CommandType.Text);
                if (result > 0)
                {
                    return new OperateResult
                    {
                        Flag = 1,
                        Msg = ""
                    };
                }
                return new OperateResult
                {
                    Flag = 0,
                    Msg = ""
                };
            }
            catch (Exception ex)
            {
                return new OperateResult
                {
                    Flag = 0,
                    Msg = "读取邮件异常"
                };
            }
        }

        public Dictionary<string, object> MailDetail(MailDetailMapper entity, int userId)
        {
            var sql = @"SELECT " +
                                        "body.recid mailid," +
                                        "(SELECT row_to_json(t) FROM (SELECT mailaddress,displayname FROM crm_sys_mail_senderreceivers WHERE ctype=1 AND mailid=body.recid LIMIT 1) t)::jsonb sender," +
                                        "(SELECT array_to_json(array_agg(row_to_json(t))) FROM (SELECT mailaddress,displayname FROM crm_sys_mail_senderreceivers WHERE ctype=2 AND mailid=body.recid ) t)::jsonb receiversjson," +
                                        "(SELECT array_to_json(array_agg(row_to_json(t))) FROM (SELECT mailaddress,displayname FROM crm_sys_mail_senderreceivers WHERE ctype=3 AND mailid=body.recid ) t)::jsonb ccersjson," +
                                        "(SELECT array_to_json(array_agg(row_to_json(t))) FROM (SELECT mailaddress,displayname FROM crm_sys_mail_senderreceivers WHERE ctype=4 AND mailid=body.recid ) t)::jsonb bccersjson," +
                                        "body.title," +
                                        "body.mailbody," +
                                        "body.senttime," +
                                        "body.receivedtime," +
                                        "body.istag," +
                                        "body.isread," +
                                        "(SELECT COUNT(1) FROM crm_sys_mail_attach WHERE mailid=body.recid) attachcount," +
                                        "(SELECT array_to_json(array_agg(row_to_json(t))) FROM (SELECT filename,mongoid AS fileid FROM crm_sys_mail_attach WHERE  mailid=body.recid ) t)::jsonb attachinfojson" +
                                        " FROM crm_sys_mail_mailbody body Where body.recid=@mailid ";
            var senderSql = @"SELECT recname,phone,email,headicon FROM crm_sys_contact WHERE email=(Select mailaddress From crm_sys_mail_senderreceivers Where mailid=@mailid And ctype=1)  AND recstatus=1 UNION ALL SELECT username,usertel,useremail,usericon FROM crm_sys_userinfo WHERE useremail=(Select mailaddress From crm_sys_mail_senderreceivers Where mailid=@mailid And ctype=1)   AND recstatus=1;";
            var isCustExistsSql = @"Select count(1) From crm_sys_customer Where recid=(SELECT belcust->>'id' FROM crm_sys_contact WHERE email=(Select mailaddress From crm_sys_mail_senderreceivers Where mailid=@mailid And ctype=1)  AND recstatus=1 LIMIT 1)::uuid";
            var custSql = @"SELECT crm_func_entity_protocol_data_detail('f9db9d79-e94b-4678-a5cc-aa6e281c1246',
(SELECT belcust->>'id' FROM crm_sys_contact WHERE email=(Select mailaddress From crm_sys_mail_senderreceivers Where mailid=@mailid And ctype=1)  AND recstatus=1 LIMIT 1)::uuid,0,@userid);";
            var isConExistsSql = @"Select count(1) From crm_sys_contact Where recid=(SELECT belcust->>'id' FROM crm_sys_contact WHERE email=(Select mailaddress From crm_sys_mail_senderreceivers Where mailid=@mailid And ctype=1)  AND recstatus=1 LIMIT 1)::uuid";
            var contactsSql = @"SELECT * FROM crm_func_entity_protocol_data_list
('e450bfd7-ff17-4b29-a2db-7ddaf1e79342','75ce6617-2016-46f0-8cb4-8467b77ef468',' and t.recid IN (SELECT recid FROM crm_sys_contact WHERE belcust->>'id' IN (
SELECT belcust->>'id' FROM crm_sys_contact WHERE email=(Select mailaddress From crm_sys_mail_senderreceivers Where mailid=@mailid And ctype=1)  AND recstatus=1))','',0,NULL,1,@maxpagesize,0,@userid)
";
            var param = new DynamicParameters();
            param.Add("mailid", entity.MailId);
            param.Add("userid", userId);
            var mailDetail = DataBaseHelper.QuerySingle<MailBodyDetailMapper>(sql, param, CommandType.Text);
            var senderResult = DataBaseHelper.Query<dynamic>(senderSql, param, CommandType.Text);
            var countRecord = DataBaseHelper.QuerySingle<int>(isCustExistsSql, param, CommandType.Text);
            List<dynamic> custResult = new List<dynamic>();
            if (countRecord == 1)
                custResult = DataBaseHelper.QueryStoredProcCursor<dynamic>(custSql, param, CommandType.Text);
            countRecord = DataBaseHelper.QuerySingle<int>(isConExistsSql, param, CommandType.Text);
            List<dynamic> contactsResult = new List<dynamic>();
            if (countRecord == 1)
                contactsResult = DataBaseHelper.QueryStoredProcCursor<dynamic>(contactsSql, param, CommandType.Text);
            Dictionary<string, object> dicResult = new Dictionary<string, object>();
            dicResult.Add("maildetail", mailDetail);
            dicResult.Add("sender", senderResult);
            dicResult.Add("custinfo", custResult);
            dicResult.Add("contacts", contactsResult);
            return dicResult;
        }

        public IList<MailAttachmentMapper> MailAttachment(List<Guid> mailIds)
        {
            var sql = @"SELECT  filename,filetype,filesize,mongoid	,mailid FROM crm_sys_mail_attach Where mailid IN (select regexp_split_to_table(@mailids,',')::uuid);";

            var param = new
            {
                MailIds = string.Join(",", mailIds.Select(t => t.ToString()))
            };
            return DataBaseHelper.Query<MailAttachmentMapper>(sql, param, CommandType.Text);
        }

        public OperateResult InnerTransferMail(TransferMailDataMapper entity, int userId, DbTransaction tran = null)
        {
            var mailBodySql = @"	INSERT INTO crm_sys_mail_mailbody
	                                        (recname, reccode, recaudits, recstatus, reccreator
	                                        , recupdator, recmanager, relativemailbox, headerinfo, title
	                                        , mailbody, sender, receivers, ccers, bccers
	                                        , attachcount, urgency, mongoid, isread, istag
	                                        , senttime, receivedtime)
                                        SELECT recname, reccode, recaudits, recstatus, @userid
	                                        , @userid, recmanager, relativemailbox, headerinfo, title
	                                        , mailbody, sender, receivers, ccers, bccers
	                                        , attachcount, urgency, mongoid, isread, istag
	                                        , senttime, receivedtime FROM crm_sys_mail_mailbody
                                        WHERE recid = @mailid
                                        RETURNING recid";
            var mailAttachSql = @"INSERT INTO crm_sys_mail_attach(filename,filetype,filesize,mongoid,mailid) Select @filename,@filetype,@filesize,@mongoid,@newmailid";

            var mailSenderreceiversSql = @"INSERT INTO crm_sys_mail_senderreceivers (mailid,ctype,biztype,mailaddress,displayname,ismailgroup,	relativetocotract,relativetouser,relativetodept,relativemailbox)
         SELECT @newmailid::uuid,ctype,biztype,mailaddress,displayname,ismailgroup,relativetocotract,relativetouser,relativetodept	,relativemailbox FROM crm_sys_mail_senderreceivers WHERE mailid=@mailid";

            var mailInnerTransferRecord = @"INSERT INTO crm_sys_mail_intransferrecord  ( reccreator, recupdator, recmanager, userid, transferuserid, fromuser,mailid,newmailid) Select @userid,@userid,@userid,@userid,@transferuserid,@userid,@mailid,@newmailid";

            var subDeptUserSql = @"SELECT userid FROM crm_sys_account_userinfo_relate WHERE recstatus = 1 AND deptid IN 
                                           (SELECT deptid FROM crm_func_department_tree(@deptid,1)) AND userid!=@userid";

            var emailUserSql = @" SELECT Distinct owner::int4 FROM crm_sys_mail_mailbox WHERE owner::int4 IN (@userids)  ";

            var cataHandleSql = "Select * From  crm_func_mail_cata_related_handle(@mailid,null,'{\"issendoreceive\":\"1\"}',null,@userid);";

            try
            {
                List<int> userCollection = new List<int>();
                foreach (var deptId in entity.DeptIds)
                {
                    var subDeptUser = DataBaseHelper.Query<int>(subDeptUserSql, new { UserId = userId, DeptId = deptId });
                    userCollection = userCollection.Concat(subDeptUser).ToList();
                }
                entity.TransferUserIds = entity.TransferUserIds.Concat(userCollection).Distinct().Where(t => t != userId).ToList();
                entity.TransferUserIds = DataBaseHelper.Query<int>(emailUserSql, new { UserIds = string.Join(",", entity.TransferUserIds) });
                foreach (var mailId in entity.MailIds)
                {
                    foreach (var user in entity.TransferUserIds)
                    {
                        var param = new DbParameter[]
                        {
                            new NpgsqlParameter("mailid",mailId),
                            new NpgsqlParameter("userid",userId)
                        };
                        var result = DBHelper.ExecuteScalar(tran, mailBodySql, param, CommandType.Text);
                        param = new DbParameter[]
                        {
                            new NpgsqlParameter("mailid",mailId),
                           new NpgsqlParameter("newmailid",result)
                        };
                        DBHelper.ExecuteNonQuery(tran, mailSenderreceiversSql, param, CommandType.Text);
                        foreach (var att in entity.Attachment)
                        {
                            param = new DbParameter[]
                           {
                                new NpgsqlParameter("filename",att.FileName),
                                 new NpgsqlParameter("filetype",att.FileType),
                                new NpgsqlParameter("filesize",att.FileSize),
                                new NpgsqlParameter("mongoid",att.MongoId),
                                new NpgsqlParameter("newmailid",result)
                           };
                            DBHelper.ExecuteNonQuery(tran, mailAttachSql, param, CommandType.Text);
                        }
                        param = new DbParameter[]
                        {
                            new NpgsqlParameter("userid",userId),
                            new NpgsqlParameter("transferuserid", user),
                            new NpgsqlParameter("mailid",mailId),
                           new NpgsqlParameter("newmailid",Guid.Parse(result.ToString()))
                        };
                        DBHelper.ExecuteNonQuery(tran, mailInnerTransferRecord, param, CommandType.Text);
                        param = new DbParameter[]
                        {
                            new NpgsqlParameter("mailid",mailId),
                            new NpgsqlParameter("userid", userId),
                        };
                        var operateResult = DBHelper.ExecuteQuery<OperateResult>(tran, cataHandleSql, param).FirstOrDefault();
                        if (operateResult.Flag == 0)
                        {
                            continue;
                        }
                    }
                }
                return new OperateResult
                {
                    Flag = 1,
                    Msg = "内部转发成功"
                };
            }
            catch (Exception ex)
            {
                return new OperateResult
                {
                    Flag = 1,
                    Msg = "内部转发异常",
                    Stacks = ex.Message
                };
            }
        }

        public PageDataInfo<TransferRecordMapper> GetInnerTransferRecord(TransferRecordParamMapper entity, int userId)
        {
            var sql = @"SELECT u1.userid,u1.username,u.username as fromuser ,transfer.reccreated as transfertime FROM  crm_sys_mail_intransferrecord transfer
                                LEFT JOIN crm_sys_userinfo u ON transfer.fromuser=u.userid
                                LEFT JOIN crm_sys_userinfo u1 ON transfer.transferuserid=u1.userid WHERE mailid=@mailid";

            var param = new DbParameter[]
            {
                   new NpgsqlParameter("mailid",entity.MailId.ToString())
            };
            return ExecuteQueryByPaging<TransferRecordMapper>(sql, param, entity.PageSize, (entity.PageIndex - 1) * entity.PageIndex);
        }

        public OperateResult MoveMail(MoveMailMapper entity, int userId, DbTransaction tran = null)
        {
            try
            {
                var sql = @"select count(1) from crm_sys_mail_catalog where recstatus=1 and recid=@catalogid";
                var param = new DynamicParameters();
                param.Add("catalogid", entity.CatalogId);
                var result = DataBaseHelper.QuerySingle<int>(sql, param, CommandType.Text);
                if (result > 0)
                {
                    foreach (var tmp in entity.MailIds.Split(','))
                    {
                        Guid uuid = Guid.Parse(tmp);
                        sql = @"select count(1) from   crm_sys_mail_catalog_relation  where mailid=@mailid ";
                        param = new DynamicParameters();
                        param.Add("mailid", uuid);
                        result = DataBaseHelper.QuerySingle<int>(sql, param, CommandType.Text);
                        if (result > 0)
                        {

                            sql = @"Update crm_sys_mail_catalog_relation Set catalogid=@catalogid Where mailid=@mailid";
                            var args = new DbParameter[]
                            {
                                    new NpgsqlParameter("mailid",uuid),
                                    new NpgsqlParameter("catalogid",entity.CatalogId)
                            };
                            DBHelper.ExecuteNonQuery(tran, sql, args, CommandType.Text);
                        }
                        else
                        {
                            return new OperateResult
                            {
                                Flag = 1,
                                Msg = "移动邮件信息失败"
                            };
                        }
                    }
                    return new OperateResult
                    {
                        Flag = 1,
                        Msg = "移动邮件成功"
                    };
                }
                else
                {
                    return new OperateResult
                    {
                        Flag = 1,
                        Msg = "该目录不存在"
                    };
                }
            }
            catch (Exception ex)
            {
                return new OperateResult
                {
                    Flag = 1,
                    Msg = "移动邮件信息异常"
                };
            }
        }
        public PageDataInfo<MailBodyMapper> GetInnerToAndFroMail(ToAndFroMapper entity, int userId)
        {

            string sql = @" Select                               
                                        body.recid mailid, 
                                        (SELECT row_to_json(t) FROM (SELECT mailaddress,displayname FROM crm_sys_mail_senderreceivers WHERE ctype=1 AND mailid=body.recid LIMIT 1) t)::jsonb sender, 
                                        (SELECT array_to_json(array_agg(row_to_json(t))) FROM (SELECT mailaddress,displayname FROM crm_sys_mail_senderreceivers WHERE ctype=2 AND mailid=body.recid ) t)::jsonb receivers, 
                                        (SELECT array_to_json(array_agg(row_to_json(t))) FROM (SELECT mailaddress,displayname FROM crm_sys_mail_senderreceivers WHERE ctype=3 AND mailid=body.recid ) t)::jsonb ccers, 
                                        (SELECT array_to_json(array_agg(row_to_json(t))) FROM (SELECT mailaddress,displayname FROM crm_sys_mail_senderreceivers WHERE ctype=4 AND mailid=body.recid ) t)::jsonb bccers, 
                                        body.title, 
                                        body.mailbody as summary, 
                                        body.senttime, 
                                        body.receivedtime  FROM crm_sys_mail_mailbody body {0}";
            var param = new DbParameter[] { new NpgsqlParameter("mailid", entity.MailId), new NpgsqlParameter("userid", userId.ToString()) };
            string whereSql = string.Empty;
            //与自己往来+收到和发出的邮件
            if (entity.relatedMySelf == 0 && entity.relatedSendOrReceive == 0)
            {
                whereSql = @"Where recstatus=1 and recid IN (
                                        SELECT mailid FROM crm_sys_mail_senderreceivers WHERE mailaddress IN (
                                        SELECT tmp.email FROM (
                                        SELECT regexp_split_to_table((belcust->>'id'),',') AS custid,email FROM crm_sys_contact WHERE recstatus=1 ) AS tmp 
                                        WHERE tmp.custid IN (
                                        SELECT regexp_split_to_table((belcust->>'id'),',')  AS custid FROM crm_sys_contact WHERE email IN(
                                        SELECT mailaddress FROM crm_sys_mail_senderreceivers WHERE mailid=@mailid AND ctype=1 ))  
                                        )) AND recid IN (Select mailid From crm_sys_mail_senderreceivers Where mailaddress IN (SELECT accountid FROM crm_sys_mail_mailbox WHERE recstatus=1 AND owner=@userid))";
            }
            else if (entity.relatedMySelf == 0 && entity.relatedSendOrReceive == 1)            //与自己往来+收到的邮件
            {
                whereSql = @" Where recstatus=1 and  recid IN (
                                        SELECT mailid FROM crm_sys_mail_senderreceivers WHERE mailaddress IN (
                                        SELECT tmp.email FROM (
                                        SELECT regexp_split_to_table((belcust->>'id'),',') AS custid,email FROM crm_sys_contact WHERE recstatus=1 ) AS tmp 
                                        WHERE tmp.custid IN (
                                        SELECT regexp_split_to_table((belcust->>'id'),',')  AS custid FROM crm_sys_contact WHERE email IN(
                                        SELECT mailaddress FROM crm_sys_mail_senderreceivers WHERE mailid=@mailid AND ctype=1 ))  
                                        )) AND recid IN (Select mailid From crm_sys_mail_senderreceivers Where mailaddress IN (SELECT accountid FROM crm_sys_mail_mailbox WHERE recstatus=1 AND owner=@userid) AND ctype!=1)";
            }
            else if (entity.relatedMySelf == 0 && entity.relatedSendOrReceive == 2)         //与自己往来+发出的邮件
            {
                whereSql = @" Where recstatus=1 and  recid IN (
                                        SELECT mailid FROM crm_sys_mail_senderreceivers WHERE mailaddress IN (
                                        SELECT tmp.email FROM (
                                        SELECT regexp_split_to_table((belcust->>'id'),',') AS custid,email FROM crm_sys_contact WHERE recstatus=1 ) AS tmp 
                                        WHERE tmp.custid IN (
                                        SELECT regexp_split_to_table((belcust->>'id'),',')  AS custid FROM crm_sys_contact WHERE email IN(
                                        SELECT mailaddress FROM crm_sys_mail_senderreceivers WHERE mailid=@mailid AND ctype=1 ))  
                                        ))  AND recid IN (Select mailid From crm_sys_mail_senderreceivers Where mailaddress IN (SELECT accountid FROM crm_sys_mail_mailbox WHERE recstatus=1 AND owner=@userid) AND ctype=1)";
            }
            else if (entity.relatedMySelf == 1 && entity.relatedSendOrReceive == 0)       //与所有用户往来+收到和发出的邮件
            {
                whereSql = @"  Where recstatus=1 and  recid IN (
                                        SELECT mailid FROM crm_sys_mail_senderreceivers WHERE mailaddress IN (
                                        SELECT tmp.email FROM (
                                        SELECT regexp_split_to_table((belcust->>'id'),',') AS custid,email FROM crm_sys_contact WHERE recstatus=1 ) AS tmp 
                                        WHERE tmp.custid IN (
                                        SELECT regexp_split_to_table((belcust->>'id'),',')  AS custid FROM crm_sys_contact WHERE email IN(
                                        SELECT mailaddress FROM crm_sys_mail_senderreceivers WHERE mailid=@mailid AND ctype=1 ))  
                                        )) AND recid IN (Select mailid From crm_sys_mail_senderreceivers Where mailaddress IN (SELECT accountid FROM crm_sys_mail_mailbox Where recstatus=1))";

            }
            else if (entity.relatedMySelf == 1 && entity.relatedSendOrReceive == 1)//与所有用户往来+收出的邮件
            {
                whereSql = @"  Where recstatus=1 and  recid IN (
                                        SELECT mailid FROM crm_sys_mail_senderreceivers WHERE mailaddress IN (
                                        SELECT tmp.email FROM (
                                        SELECT regexp_split_to_table((belcust->>'id'),',') AS custid,email FROM crm_sys_contact WHERE recstatus=1 ) AS tmp 
                                        WHERE tmp.custid IN (
                                        SELECT regexp_split_to_table((belcust->>'id'),',')  AS custid FROM crm_sys_contact WHERE email IN(
                                        SELECT mailaddress FROM crm_sys_mail_senderreceivers WHERE mailid=@mailid AND ctype=1 ))  
                                        )) AND recid IN (Select mailid From crm_sys_mail_senderreceivers Where mailaddress IN (SELECT accountid FROM crm_sys_mail_mailbox WHERE  recstatus=1) AND ctype!=1)";

            }

            else if (entity.relatedMySelf == 1 && entity.relatedSendOrReceive == 2)//与所有用户往来+发出的邮件
            {
                whereSql = @"  Where recstatus=1 and  recid IN (
                                        SELECT mailid FROM crm_sys_mail_senderreceivers WHERE mailaddress IN (
                                        SELECT tmp.email FROM (
                                        SELECT regexp_split_to_table((belcust->>'id'),',') AS custid,email FROM crm_sys_contact WHERE recstatus=1 ) AS tmp 
                                        WHERE tmp.custid IN (
                                        SELECT regexp_split_to_table((belcust->>'id'),',')  AS custid FROM crm_sys_contact WHERE email IN(
                                        SELECT mailaddress FROM crm_sys_mail_senderreceivers WHERE mailid=@mailid AND ctype=1 ))  
                                        )) AND recid IN (Select mailid From crm_sys_mail_senderreceivers Where mailaddress IN (SELECT accountid FROM crm_sys_mail_mailbox WHERE recstatus=1) AND ctype=1)";

            }

            return ExecuteQueryByPaging<MailBodyMapper>(string.Format(sql, whereSql), param, entity.PageSize, (entity.PageIndex - 1) * entity.PageIndex);
        }


        public PageDataInfo<ToAndFroFileMapper> GetInnerToAndFroAttachment(ToAndFroMapper entity, int userId)
        {

            string sql = @" SELECT filename,filetype,filesize,mongoid,mailid FROM crm_sys_mail_attach  {0}";
            var param = new DbParameter[] { new NpgsqlParameter("mailid", entity.MailId), new NpgsqlParameter("userid", userId.ToString()) };
            string whereSql = string.Empty;
            //与自己往来+收到和发出的邮件
            if (entity.relatedMySelf == 0 && entity.relatedSendOrReceive == 0)
            {
                whereSql = @"Where recstatus=1 and mailid IN (
                                        SELECT mailid FROM crm_sys_mail_senderreceivers WHERE mailaddress IN (
                                        SELECT tmp.email FROM (
                                        SELECT regexp_split_to_table((belcust->>'id'),',') AS custid,email FROM crm_sys_contact WHERE recstatus=1 ) AS tmp 
                                        WHERE tmp.custid IN (
                                        SELECT regexp_split_to_table((belcust->>'id'),',')  AS custid FROM crm_sys_contact WHERE email IN(
                                        SELECT mailaddress FROM crm_sys_mail_senderreceivers WHERE mailid=@mailid AND ctype=1 ))  
                                        )) AND mailid IN (Select mailid From crm_sys_mail_senderreceivers Where mailaddress IN (SELECT accountid FROM crm_sys_mail_mailbox WHERE recstatus=1 AND owner=@userid))";
            }
            else if (entity.relatedMySelf == 0 && entity.relatedSendOrReceive == 1)            //与自己往来+收到的邮件
            {
                whereSql = @" Where recstatus=1 and  mailid IN (
                                        SELECT mailid FROM crm_sys_mail_senderreceivers WHERE mailaddress IN (
                                        SELECT tmp.email FROM (
                                        SELECT regexp_split_to_table((belcust->>'id'),',') AS custid,email FROM crm_sys_contact WHERE recstatus=1 ) AS tmp 
                                        WHERE tmp.custid IN (
                                        SELECT regexp_split_to_table((belcust->>'id'),',')  AS custid FROM crm_sys_contact WHERE email IN(
                                        SELECT mailaddress FROM crm_sys_mail_senderreceivers WHERE mailid=@mailid AND ctype=1 ))  
                                        )) AND recid IN (Select mailid From crm_sys_mail_senderreceivers Where mailaddress IN (SELECT accountid FROM crm_sys_mail_mailbox WHERE recstatus=1 AND owner=@userid) AND ctype!=1)";
            }
            else if (entity.relatedMySelf == 0 && entity.relatedSendOrReceive == 2)         //与自己往来+发出的邮件
            {
                whereSql = @" Where recstatus=1 and  mailid IN (
                                        SELECT mailid FROM crm_sys_mail_senderreceivers WHERE mailaddress IN (
                                        SELECT tmp.email FROM (
                                        SELECT regexp_split_to_table((belcust->>'id'),',') AS custid,email FROM crm_sys_contact WHERE recstatus=1 ) AS tmp 
                                        WHERE tmp.custid IN (
                                        SELECT regexp_split_to_table((belcust->>'id'),',')  AS custid FROM crm_sys_contact WHERE email IN(
                                        SELECT mailaddress FROM crm_sys_mail_senderreceivers WHERE mailid=@mailid AND ctype=1 ))  
                                        ))  AND mailid IN (Select mailid From crm_sys_mail_senderreceivers Where mailaddress IN (SELECT accountid FROM crm_sys_mail_mailbox WHERE recstatus=1 AND owner=@userid) AND ctype=1)";
            }
            else if (entity.relatedMySelf == 1 && entity.relatedSendOrReceive == 0)       //与所有用户往来+收到和发出的邮件
            {
                whereSql = @"  Where recstatus=1 and  mailid IN (
                                        SELECT mailid FROM crm_sys_mail_senderreceivers WHERE mailaddress IN (
                                        SELECT tmp.email FROM (
                                        SELECT regexp_split_to_table((belcust->>'id'),',') AS custid,email FROM crm_sys_contact WHERE recstatus=1 ) AS tmp 
                                        WHERE tmp.custid IN (
                                        SELECT regexp_split_to_table((belcust->>'id'),',')  AS custid FROM crm_sys_contact WHERE email IN(
                                        SELECT mailaddress FROM crm_sys_mail_senderreceivers WHERE mailid=@mailid AND ctype=1 ))  
                                        )) AND mailid IN (Select mailid From crm_sys_mail_senderreceivers Where mailaddress IN (SELECT accountid FROM crm_sys_mail_mailbox Where recstatus=1))";

            }
            else if (entity.relatedMySelf == 1 && entity.relatedSendOrReceive == 1)//与所有用户往来+收出的邮件
            {
                whereSql = @"  Where recstatus=1 and  mailid IN (
                                        SELECT mailid FROM crm_sys_mail_senderreceivers WHERE mailaddress IN (
                                        SELECT tmp.email FROM (
                                        SELECT regexp_split_to_table((belcust->>'id'),',') AS custid,email FROM crm_sys_contact WHERE recstatus=1 ) AS tmp 
                                        WHERE tmp.custid IN (
                                        SELECT regexp_split_to_table((belcust->>'id'),',')  AS custid FROM crm_sys_contact WHERE email IN(
                                        SELECT mailaddress FROM crm_sys_mail_senderreceivers WHERE mailid=@mailid AND ctype=1 ))  
                                        )) AND mailid IN (Select mailid From crm_sys_mail_senderreceivers Where mailaddress IN (SELECT accountid FROM crm_sys_mail_mailbox WHERE  recstatus=1) AND ctype!=1)";

            }

            else if (entity.relatedMySelf == 1 && entity.relatedSendOrReceive == 2)//与所有用户往来+发出的邮件
            {
                whereSql = @"  Where recstatus=1 and  mailid IN (
                                        SELECT mailid FROM crm_sys_mail_senderreceivers WHERE mailaddress IN (
                                        SELECT tmp.email FROM (
                                        SELECT regexp_split_to_table((belcust->>'id'),',') AS custid,email FROM crm_sys_contact WHERE recstatus=1 ) AS tmp 
                                        WHERE tmp.custid IN (
                                        SELECT regexp_split_to_table((belcust->>'id'),',')  AS custid FROM crm_sys_contact WHERE email IN(
                                        SELECT mailaddress FROM crm_sys_mail_senderreceivers WHERE mailid=@mailid AND ctype=1 ))  
                                        )) AND mailid IN (Select mailid From crm_sys_mail_senderreceivers Where mailaddress IN (SELECT accountid FROM crm_sys_mail_mailbox WHERE recstatus=1) AND ctype=1)";

            }

            return ExecuteQueryByPaging<ToAndFroFileMapper>(string.Format(sql, whereSql), param, entity.PageSize, (entity.PageIndex - 1) * entity.PageIndex);
        }

        public PageDataInfo<AttachmentChooseListMapper> GetLocalFileFromCrm(AttachmentListMapper entity, string ruleSql, int userId)
        {
            var sql = @"
                 Select * From (
                            SELECT fileid,filename
				            FROM public.crm_sys_documents AS d Where entityid='a3500e78-fe1c-11e6-aee4-005056ae7f49'  AND recstatus=1  {0}   UNION ALL 
                             SELECT  fileid,filename 
                             FROM crm_sys_documents  AS d   WHERE entityid IN (
                            SELECT entityid FROM crm_sys_entity WHERE modeltype=0 AND recstatus=1 )  AND recstatus=1 AND reccreator=@userid) as t Where 1=1 {1}";
            string whereSql = string.Empty;
            var param = new List<DbParameter>();
            if (!string.IsNullOrEmpty(entity.KeyWord))
            {
                param.Add(new NpgsqlParameter("filename", entity.KeyWord));
                whereSql = " AND t.filename  ILIKE '%' || @filename || '%' ESCAPE '`'";
            }
            if (!string.IsNullOrEmpty(ruleSql))
            {
                ruleSql = " AND " + ruleSql;
            }
            param.Add(new NpgsqlParameter("userid", userId));
            return ExecuteQueryByPaging<AttachmentChooseListMapper>(string.Format(sql, ruleSql, whereSql), param.ToArray(), entity.PageSize, (entity.PageIndex - 1) * entity.PageIndex); ;
        }

        /// <summary>
        /// 获取我的邮箱列表
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public PageDataInfo<MailBox> GetMailBoxList(int pageIndex, int pageSize, int userId)
        {
            var sql = "select a.recid,a.accountid,a.recname,a.inwhitelist,b.recname mailserver,b.mailprovider,b.imapaddress," +
                "b.refreshinterval,b.servertype,b.smtpaddress	" +
                " from crm_sys_mail_mailbox a " +
                " inner join crm_sys_mail_server b on(a.mailserver->> 'id')::uuid = b.recid " +
                " where a.OWNER= @userid ";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("userid", userId.ToString())
            };
            var result = ExecuteQueryByPaging<MailBox>(sql, param, pageSize, pageIndex);
            return result;
        }

        #region
        /// <summary>
        /// 获取白名单或者黑名单
        /// </summary>
        /// <param name="isWhiteLst"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<MailBoxMapper> GetIsWhiteList(int isWhiteLst, int userId)
        {
            var sql = "SELECT box.accountid,u.userid,u.username FROM crm_sys_mail_mailbox box LEFT JOIN crm_sys_userinfo u ON box.owner::int4 = u.userid and inwhitelist = @iswhitelst ";
            var param = new
            {
                IsWhiteLst = isWhiteLst,
                UserId = userId
            };
            return DataBaseHelper.Query<MailBoxMapper>(sql, param, CommandType.Text);
        }
        #endregion
        #region  模糊查询我的通讯人员
        public List<MailUserMapper> GetContactByKeyword(string keyword, int count, int userId)
        {
            var sql = "select email EmailAddress,name from (SELECT email,recname as name FROM crm_sys_contact " +
                " WHERE email is not null and email!=''  and (belcust->> 'id')::uuid IN (SELECT recid FROM crm_sys_customer " +
                " WHERE recmanager = @userid) UNION ALL SELECT useremail as email,username as name " +
                " FROM crm_sys_userinfo WHERE  useremail is not null and useremail!= '' and recstatus = 1 ) x where 1=1 ";
            if (string.IsNullOrEmpty(keyword))
            {
                if (count == 0)
                {
                    sql = string.Format(sql + "  LIMIT 10 ");
                }
                else
                {
                    sql = string.Format(sql + "  LIMIT {0} ", count);
                }
            }
            else
            {
                keyword = keyword.Replace("@", "_");
                if (count == 0)
                {
                    sql = string.Format(sql + " and name like '%{0}%' or email like '%{1}%'  LIMIT 10 ", keyword, keyword);
                }
                else
                {
                    sql = string.Format(sql + " and name like '%{0}%' or email like '%{1}%'  LIMIT {2} ", keyword, keyword, count);
                }
            }

            var param = new DbParameter[]
            {
                new NpgsqlParameter("userId", userId)
            };
            return ExecuteQuery<MailUserMapper>(sql, param);
        }
        /// <summary>
        /// 获取企业内部通讯录
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>       
        public List<OrgAndStaffMapper> GetInnerContact(string deptId, int userId)
        {
            if (string.IsNullOrEmpty(deptId))
            {
                string rootsql = "select deptid::text from crm_sys_department where pdeptid::text = '00000000-0000-0000-0000-000000000000' and recstatus=1 ";
                deptId = (string)ExecuteScalar(rootsql, new DbParameter[] { });
            }
            var sql = "select * from (select ''::text mail,deptid::text treeid,deptname treename,''::text deptname,0 nodetype from crm_sys_department a  " +
                " where a.recstatus = 1 and a.pdeptid::text =@deptId order by recorder) t " +
                 "UNION ALL " +
                " select* from(select b.useremail mail, b.userid::text treeid, b.username treename, a1.deptname,1 nodetype " +
                " from crm_sys_account_userinfo_relate a inner join crm_sys_userinfo b on a.userid = b.userid " +
                " left join crm_sys_department a1 on a1.deptid = a.deptid " +
                " where a.recstatus = 1 and b.useremail is not null  and useremail!= '' " +
                "and a.deptid::text =@deptId order by b.username) t1";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("deptId", deptId)
            };
            return ExecuteQuery<OrgAndStaffMapper>(sql, param);
        }

        /// <summary>
        /// 最近联系人
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>       
        public PageDataInfo<MailUserMapper> GetRecentContact(int pageIndex, int pageSize, int userId)
        {
            var executeSql = "select x.mailAddress EmailAddress,displayname as name" +
                " from (select a.reccreated,c.* from crm_sys_mail_mailbody a " +
                " inner join crm_sys_mail_sendrecord b on a.recid=b.mailid " +
                " inner join crm_sys_mail_senderreceivers c ON c.mailid = b.mailid where c.ctype = 2 and a.recmanager =@userId ) x " +
                " group by x.mailaddress,displayname order by max(reccreated) DESC ";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("userId", userId)
            };
            return ExecuteQueryByPaging<MailUserMapper>(executeSql, param, pageSize, pageIndex);
        }

        /// <summary>
        /// 获取客户联系人
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>       
        public PageDataInfo<MailUserMapper> GetCustomerContact(int pageIndex, int pageSize, int userId)
        {
            var executeSql = "SELECT a.email EmailAddress,a.recname as name,b.recname customer,COALESCE(a.headicon,'00000000-0000-0000-0000-000000000000')::uuid icon" +
                " FROM crm_sys_contact a inner join crm_sys_customer b on(a.belcust ->> 'id') ::uuid = b.recid " +
                " where a.email is not null and a.email != ''  and a.recstatus = 1 and b.recstatus = 1 and b.recmanager = @userid";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("userId", userId)
            };
            return ExecuteQueryByPaging<MailUserMapper>(executeSql, param, pageSize, pageIndex);
        }

        /// <summary>
        /// 获取内部往来人员列表
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="userId"></param>
        /// <returns></returns>       
        public List<InnerToAndFroUser> GetInnerToAndFroUser(string keyword, int userId)
        {
            var executeSql = "select t.userid,t.username,count(1)-sum(t.isread) unread  " +
                " from (select d.userid,d.username,COALESCE(c.isread, 0) isread " +
                " from(select b.recid from crm_sys_mail_senderreceivers a " +
                " inner join crm_sys_mail_mailbody b on a.mailid= b.recid " +
                " where 1 = 1 and (ctype = 2 or  ctype = 3 or ctype = 4) and relativetouser = @userid) x " +
                " inner join crm_sys_mail_mailbody c on c.recid = x.recid " +
                " inner join crm_sys_userinfo d on d.userid = c.recmanager ) t " +
                "  {0} group by t.userid,t.username";
            string condition = string.Empty;
            if (!string.IsNullOrEmpty(keyword))
            {
                condition = string.Format(" where t.username like '%{0}%'", keyword);

            }
            string newSql = string.Format(executeSql, condition);
            var param = new DbParameter[]
            {
                new NpgsqlParameter("userId", userId)
            };
            return ExecuteQuery<InnerToAndFroUser>(newSql, param);
        }

        #endregion
        #region  我负责的客户的联系人和内部人员
        public List<MailUserMapper> GetManagerContactAndInnerUser(int userId)
        {
            var sql = "SELECT email,recname as name FROM crm_sys_contact WHERE(belcust->> 'id')::uuid IN(SELECT recid FROM crm_sys_customer WHERE recmanager = @userid); " +
                "UNION ALL" +
                "SELECT useremail as email,username as name FROM crm_sys_userinfo WHERE recstatus = 1";
            var param = new
            {
                UserId = userId
            };
            return DataBaseHelper.Query<MailUserMapper>(sql, param, CommandType.Text);
        }
        #endregion

        #region 判断领导是否拥有该下属
        public bool IsHasSubUserAuth(int leaderUserId, int userId)
        {
            var sql = "Select count(1) as ishashauth  From (SELECT userid FROM crm_sys_account_userinfo_relate WHERE recstatus = 1 AND deptid IN(SELECT deptid FROM crm_func_department_tree((SELECT deptid FROM crm_sys_account_userinfo_relate WHERE userid = @leaderuserid AND recstatus = 1 LIMIT 1), 1))) as t Where t.userid=@userid ";

            var param = new
            {
                LeaderUserId = leaderUserId,
                UserId = userId
            };
            var isHasAuth = DataBaseHelper.QuerySingle<int>(sql, param, CommandType.Text);
            if (isHasAuth > 0)
                return true;
            else
                return false;
        }
        #endregion

    }
}
