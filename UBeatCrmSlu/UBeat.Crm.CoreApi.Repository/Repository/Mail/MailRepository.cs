using Npgsql;
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
            string strSQL = @" #recursivesql#
                                        SELECT  * FROM (SELECT " +
                                        "body.recid mailid," +
                                        "(SELECT row_to_json(t) FROM (SELECT mailaddress address,CASE WHEN displayname='' OR displayname IS NULL THEN split_part(mailaddress,'@',1) ELSE displayname END FROM crm_sys_mail_senderreceivers WHERE ctype=1 AND mailid=body.recid LIMIT 1) t)::jsonb sender," +
                                        "(SELECT array_to_json(array_agg(row_to_json(t))) FROM (SELECT mailaddress address,CASE WHEN displayname='' OR displayname IS NULL THEN split_part(mailaddress,'@',1) ELSE displayname END FROM crm_sys_mail_senderreceivers WHERE ctype=2 AND mailid=body.recid ) t)::jsonb receivers," +
                                        "(SELECT array_to_json(array_agg(row_to_json(t))) FROM (SELECT mailaddress address,CASE WHEN displayname='' OR displayname IS NULL THEN split_part(mailaddress,'@',1) ELSE displayname END FROM crm_sys_mail_senderreceivers WHERE ctype=3 AND mailid=body.recid ) t)::jsonb ccers," +
                                        "(SELECT array_to_json(array_agg(row_to_json(t))) FROM (SELECT mailaddress address,CASE WHEN displayname='' OR displayname IS NULL THEN split_part(mailaddress,'@',1) ELSE displayname END FROM crm_sys_mail_senderreceivers WHERE ctype=4 AND mailid=body.recid ) t)::jsonb bccers," +
                                        "body.title," +
                                        "body.mailbody," +
                                        "COALESCE(body.senttime,body.receivedtime)  senttime," +
                                        "COALESCE(body.receivedtime,body.senttime)  receivedtime," +
                                        "body.istag," +
                                        "body.isread," +
                                        "(SELECT COUNT(1) FROM crm_sys_mail_attach WHERE mailid=body.recid AND recstatus=1) attachcount," +
                                        "(SELECT array_to_json(array_agg(row_to_json(t))) FROM (SELECT mailid,recid,mongoid,filename FROM crm_sys_mail_attach WHERE  mailid=body.recid AND recstatus=1 ) t)::jsonb attachinfo" +
                                        " FROM crm_sys_mail_mailbody body Where 1=1  {0}  ) AS tmp  {1} ";
            object[] sqlWhere = new object[] { };
            string sqlCondition = string.Empty;
            string recursiveSql = string.Empty;
            if (!string.IsNullOrEmpty(keyWord))
            {
                sqlCondition = "  ((body.sender ILIKE '%' || @keyword || '%' ESCAPE '`') OR (body.title ILIKE '%' || @keyword || '%' ESCAPE '`') OR (body.receivers ILIKE '%' || @keyword || '%' ESCAPE '`'))";
                sqlWhere = sqlWhere.Concat(new object[] { sqlCondition }).ToArray();
            }
            var catalogSql = @"SELECT ctype FROM crm_sys_mail_catalog WHERE viewuserid = @userid AND  recid=@catalogid LIMIT 1; ";
            var param = new
            {
                UserId = userId,
                CatalogId = paramInfo.Catalog
            };
            int ctype = DataBaseHelper.QuerySingle<int>(catalogSql, param);
            switch (ctype)
            {
                case 1002:
                    recursiveSql = @"
                                          WITH RECURSIVE  T1 AS (
                                           SELECT recid, recname, vpid FROM crm_sys_mail_catalog WHERE (ctype = 1001 OR ctype = 1004) AND viewuserid = @userid AND recstatus = 1
                                           UNION ALL
                                           SELECT cata.recid,cata.recname,cata.vpid FROM crm_sys_mail_catalog cata INNER JOIN T1 ON cata.vpid = T1.recid WHERE cata.viewuserid = @userid
                                        )";
                    sqlCondition = "  body.recid IN (SELECT mailid FROM crm_sys_mail_catalog_relation WHERE catalogid IN (SELECT recid FROM T1)) AND  body.isread=0 ";
                    sqlWhere = sqlWhere.Concat(new object[] { sqlCondition }).ToArray();
                    strSQL = strSQL.Replace("#recursivesql#", recursiveSql);
                    break;
                case 1008:
                    recursiveSql = @"
                                          WITH RECURSIVE  T1 AS (
                                           SELECT recid, recname, vpid FROM crm_sys_mail_catalog WHERE (ctype = 1001 OR ctype = 1004) AND viewuserid = @userid AND recstatus = 1
                                           UNION ALL
                                           SELECT cata.recid,cata.recname,cata.vpid FROM crm_sys_mail_catalog cata INNER JOIN T1 ON cata.vpid = T1.recid WHERE cata.viewuserid = @userid
                                        )";
                    sqlCondition = "  body.recid IN (SELECT mailid FROM crm_sys_mail_catalog_relation WHERE catalogid IN (SELECT recid FROM T1)) AND  body.istag=1 ";
                    sqlWhere = sqlWhere.Concat(new object[] { sqlCondition }).ToArray();
                    strSQL = strSQL.Replace("#recursivesql#", recursiveSql);
                    break;
                case 1006:
                    recursiveSql = @"
                                          WITH RECURSIVE  T1 AS (
                                           SELECT recid, recname, vpid FROM crm_sys_mail_catalog WHERE  viewuserid = @userid AND recstatus = 1 AND recid=@catalogid
                                           UNION ALL
                                           SELECT cata.recid,cata.recname,cata.vpid FROM crm_sys_mail_catalog cata INNER JOIN T1 ON cata.vpid = T1.recid WHERE cata.viewuserid = @userid
                                        )";
                    sqlCondition = "  body.recid IN (SELECT mailid FROM crm_sys_mail_catalog_relation WHERE catalogid IN (SELECT recid FROM T1))   AND body.recstatus=0 ";
                    sqlWhere = sqlWhere.Concat(new object[] { sqlCondition }).ToArray();
                    strSQL = strSQL.Replace("#recursivesql#", recursiveSql);
                    break;
                default:
                    recursiveSql = @"
                                          WITH RECURSIVE  T1 AS (
                                           SELECT recid, recname, vpid FROM crm_sys_mail_catalog WHERE  viewuserid = @userid AND recstatus = 1 AND recid=@catalogid
                                           UNION ALL
                                           SELECT cata.recid,cata.recname,cata.vpid FROM crm_sys_mail_catalog cata INNER JOIN T1 ON cata.vpid = T1.recid WHERE cata.viewuserid = @userid
                                        )";
                    sqlCondition = "  body.recid IN (SELECT mailid FROM crm_sys_mail_catalog_relation WHERE catalogid IN (SELECT recid FROM T1))  AND body.recstatus=1 ";
                    sqlWhere = sqlWhere.Concat(new object[] { sqlCondition }).ToArray();
                    strSQL = strSQL.Replace("#recursivesql#", recursiveSql);
                    break;
            }

            sqlCondition = sqlWhere.Count() == 0 ? string.Empty : " AND " + string.Join(" AND ", sqlWhere);


            if (paramInfo.PageSize <= 0)
            {
                paramInfo.PageSize = 1000000;
            }
            if (paramInfo.PageIndex <= 0)
            {
                paramInfo.PageIndex = 1;
            }

            orderbyfield = @" order by tmp.receivedtime desc ";
            strSQL = string.Format(strSQL, sqlCondition, orderbyfield);
            return ExecuteQueryByPaging<MailBodyMapper>(strSQL, new DbParameter[] { new NpgsqlParameter("catalogid", paramInfo.Catalog), new NpgsqlParameter("ctype", ctype), new NpgsqlParameter("userid", userId), new NpgsqlParameter("keyword", keyWord) }, paramInfo.PageSize, paramInfo.PageIndex);
        }

        /// <summary>
        /// 根据条件返回邮件列表
        /// </summary>
        /// <param name="paramInfo"></param>
        /// <param name="orderbyfield"></param>
        /// <param name="userId"></param>
        /// <param name="tran"></param>
        /// <returns></returns>
        public PageDataInfo<MailBodyMapper> InnerToAndFroListMail(InnerToAndFroMailMapper entity, int userId)
        {
            string strSQL = @" SELECT * FROM ( SELECT " +
                                        "body.recid mailid," +
                                        "(SELECT row_to_json(t) FROM (SELECT mailaddress as address,displayname FROM crm_sys_mail_senderreceivers WHERE ctype=1 AND mailid=body.recid LIMIT 1) t)::jsonb sender," +
                                        "(SELECT array_to_json(array_agg(row_to_json(t))) FROM (SELECT mailaddress  as address,displayname FROM crm_sys_mail_senderreceivers WHERE ctype=2 AND mailid=body.recid ) t)::jsonb receivers," +
                                        "(SELECT array_to_json(array_agg(row_to_json(t))) FROM (SELECT mailaddress  as address,displayname FROM crm_sys_mail_senderreceivers WHERE ctype=3 AND mailid=body.recid ) t)::jsonb ccers," +
                                        "(SELECT array_to_json(array_agg(row_to_json(t))) FROM (SELECT mailaddress  as address,displayname FROM crm_sys_mail_senderreceivers WHERE ctype=4 AND mailid=body.recid ) t)::jsonb bccers," +
                                        "body.title," +
                                        "body.mailbody," +
                                        "COALESCE(body.senttime,body.receivedtime)  senttime," +
                                        "COALESCE(body.receivedtime,body.senttime)  receivedtime," +
                                        "body.istag," +
                                        "body.isread," +
                                        "(SELECT COUNT(1) FROM crm_sys_mail_attach WHERE mailid=body.recid AND recstatus=1) attachcount," +
                                        "(SELECT array_to_json(array_agg(row_to_json(t))) FROM (SELECT mailid,recid,mongoid,filename FROM crm_sys_mail_attach WHERE  mailid=body.recid AND recstatus=1 ) t)::jsonb attachinfo" +
                                        " FROM crm_sys_mail_mailbody body Where body.recid IN (SELECT mailid FROM crm_sys_mail_catalog_relation WHERE catalogid IN(SELECT recid FROM crm_sys_mail_catalog WHERE recstatus = 1 AND viewuserid = @userid AND ctype != 1003) AND body.recstatus=1 AND body.recid IN (" +
                                        "SELECT DISTINCT tmp.mailid FROM (SELECT * FROM crm_sys_mail_senderreceivers where mailid in(\n" +
                                        "SELECT\n" +
                                        " mailid FROM crm_sys_mail_senderreceivers where mailaddress IN (SELECT accountid FROM crm_sys_mail_mailbox  WHERE owner::int4 = @userid) AND ctype=1) ) AS tmp\n" +
                                        "INNER JOIN\n" +
                                        "(SELECT * FROM crm_sys_mail_senderreceivers where mailid in(\n" +
                                        "select\n" +
                                        " mailid from crm_sys_mail_senderreceivers where mailaddress IN (SELECT accountid FROM crm_sys_mail_mailbox  WHERE owner::int4 = @fromuserid) AND (ctype=2 OR ctype=3 OR ctype=4)) ) AS tmp1\n" +
                                        "ON tmp.mailid=tmp1.mailid\n" +
                                        "UNION\n" +
                                        "SELECT DISTINCT tmp.mailid FROM (SELECT * FROM crm_sys_mail_senderreceivers where mailid in(\n" +
                                        "SELECT\n" +
                                        " mailid FROM crm_sys_mail_senderreceivers where mailaddress IN (SELECT accountid FROM crm_sys_mail_mailbox  WHERE owner::int4 = @fromuserid) AND ctype=1) ) AS tmp\n" +
                                        "INNER JOIN\n" +
                                        "(SELECT * FROM crm_sys_mail_senderreceivers where mailid in(\n" +
                                        "select\n" +
                                        " mailid from crm_sys_mail_senderreceivers where mailaddress IN (SELECT accountid FROM crm_sys_mail_mailbox  WHERE owner::int4 = @userid) AND (ctype=2 OR ctype=3 OR ctype=4)) ) AS tmp1\n" +
                                        "ON tmp.mailid=tmp1.mailid" +
                                        ") {0} )) AS tmp  ORDER BY tmp.receivedtime DESC";
            var sqlWhere = new object[] { };
            string sqlCondition = string.Empty;
            if (!string.IsNullOrEmpty(entity.KeyWord))
            {
                sqlCondition = "  ((body.sender ILIKE '%' || @keyword || '%' ESCAPE '`') OR (body.title ILIKE '%' || @keyword || '%' ESCAPE '`') OR (body.receivers ILIKE '%' ||  @keyword || '%' ESCAPE '`'))";
                sqlWhere = sqlWhere.Concat(new object[] { sqlCondition }).ToArray();
            }

            sqlCondition = sqlWhere.Count() == 0 ? string.Empty : " AND " + string.Join(" AND ", sqlWhere);


            if (entity.PageSize <= 0)
            {
                entity.PageSize = 1000000;
            }
            if (entity.PageIndex <= 0)
            {
                entity.PageIndex = 1;
            }

            return ExecuteQueryByPaging<MailBodyMapper>(string.Format(strSQL, sqlCondition), new DbParameter[] { new NpgsqlParameter("fromuserid", entity.FromUserId), new NpgsqlParameter("userid", userId), new NpgsqlParameter("keyword", entity.KeyWord) }, entity.PageSize, entity.PageIndex);
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

        public OperateResult DeleteMails(DeleteMailMapper entity, int userId, DbTransaction dbTrans = null)
        {
            var preSql = "SELECT count(1) hashdata FROM crm_sys_mail_catalog WHERE recid IN(SELECT catalogid FROM crm_sys_mail_catalog_relation  WHERE mailid =@mailid ) AND viewuserid = @userid ";
            var draftMailSql = "DELETE FROM crm_sys_mail_mailbody WHERE recid=@mailid;DELETE FROM crm_sys_mail_catalog_relation WHERE mailid=@mailid;DELETE FROM crm_sys_mail_senderreceivers WHERE mailid=@mailid;DELETE FROM crm_sys_mail_related WHERE mailid=@mailid;DELETE FROM crm_sys_mail_sendrecord WHERE mailid=@mailid;DELETE FROM crm_sys_mail_attach WHERE mailid=@mailid;";
            var catalogSql = "SELECT ctype FROM crm_sys_mail_catalog WHERE viewuserid=@userid AND recid=(SELECT catalogid FROM crm_sys_mail_catalog_relation WHERE mailid=@mailid LIMIT 1);";
            bool isDraftMail = false;
            foreach (var tmp in entity.MailIds.Split(','))
            {
                Guid mailId = Guid.Parse(tmp);
                var args = new
                {
                    MailId = mailId,
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
                var ctype = DataBaseHelper.QuerySingle<int>(catalogSql, args);
                if (ctype == 1005)
                {
                    DBHelper.ExecuteNonQuery(dbTrans, draftMailSql, new DbParameter[] { new NpgsqlParameter("userid", userId), new NpgsqlParameter("mailid", mailId) });
                    isDraftMail = true;
                }
                else
                    isDraftMail = false;
            }
            if (isDraftMail)
                return new OperateResult
                {
                    Flag = 1,
                    Msg = "删除邮件成功"
                };
            string sql = string.Empty;

            if (entity.IsTruncate)
                sql = "Update  crm_sys_mail_mailbody set recstatus=2 where recid IN (select regexp_split_to_table(@mailids,',')::uuid);" +
                   "Update  crm_sys_mail_attach set recstatus=2  where mailid IN (select regexp_split_to_table(@mailids,',')::uuid);" +
                   "Delete From  crm_sys_mail_reconvert  where mailid IN (select regexp_split_to_table(@mailids,',')::uuid);" +
                  "INSERT INTO crm_sys_mail_reconvert ( mailid,srccatalogid,srcuserid) SELECT mailid,catalogid,@userid FROM crm_sys_mail_catalog_relation WHERE mailid IN (select regexp_split_to_table(@mailids,',')::uuid);";
            else
                sql = "update crm_sys_mail_mailbody set recstatus=0 where recid IN (select regexp_split_to_table(@mailids,',')::uuid);" +
                    "update crm_sys_mail_catalog_relation set catalogid=(SELECT recid FROM crm_sys_mail_catalog WHERE viewuserid = @userid AND ctype = 1006 LIMIT 1) where mailid IN (select regexp_split_to_table(@mailids,',')::uuid);";
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

        public OperateResult ReConverMails(ReConverMailMapper entity, int userId, DbTransaction dbTrans = null)
        {
            string validCatalogSql = @"SELECT cata.* FROM crm_sys_mail_reconvert re INNER JOIN crm_sys_mail_catalog cata ON  re.srccatalogid=cata.recid WHERE re.mailid=@mailid AND cata.recstatus=1;";//判断目录是否存在
            string validCatalogRelationSql = @"SELECT re.*,cata.catalogid FROM crm_sys_mail_reconvert re LEFT JOIN crm_sys_mail_catalog_relation cata ON re.mailid=cata.mailid AND re.srccatalogid=cata.catalogid WHERE re.mailid=@mailid;";//判断邮件和目录的关系是否存在
            string cataSql = @"SELECT recid,recname FROM crm_sys_mail_catalog WHERE viewuserid=(SELECT srcuserid FROM crm_sys_mail_reconvert WHERE mailid=@mailid LIMIT 1) AND ctype=2002 LIMIT 1";//个人目录
            string createCatalogSql = @"INSERT INTO public.crm_sys_mail_catalog (recname, userid, viewuserid, ctype, custcatalog, custid, recstatus, pid, vpid, recorder, isdynamic, defaultid) VALUES ('恢复已删除',(SELECT srcuserid FROM crm_sys_mail_reconvert WHERE mailid=@mailid LIMIT 1),(SELECT srcuserid FROM crm_sys_mail_reconvert WHERE mailid=@mailid LIMIT 1),3002,null,null,1,@catalogid,@catalogid,(SELECT COUNT(COALESCE(recorder,1))+1 FROM crm_sys_mail_catalog WHERE viewuserid=(SELECT srcuserid FROM crm_sys_mail_reconvert WHERE mailid=@mailid LIMIT 1) AND ctype=3002 AND vpid=@catalogid),1,null) returning recid;";
            string catalogRelationSql = @"INSERT INTO public.crm_sys_mail_catalog_relation (mailid, catalogid, relativetype) VALUES (@mailid,@catalogid,1);";
            string sql = "update crm_sys_mail_mailbody set recstatus=@recstatus where recid=@mailid ;Delete From crm_sys_mail_reconvert Where mailid=@mailid;";
            try
            {
                dynamic catalog;
                DbParameter[] param;
                string tipMsg = string.Empty;

                int recstatus = -1;
                foreach (var tmp in entity.MailIds.Split(','))
                {
                    Guid mailId = Guid.Parse(tmp);
                    var catalogExist = DataBaseHelper.QuerySingle<dynamic>(validCatalogSql, new { MailId = mailId });//先判断目录是否存在
                    if (catalogExist.ctype == 1006)
                    {
                        recstatus = 0;
                    }
                    else
                    {
                        recstatus = 1;
                    }
                    if (catalogExist == null)
                    {
                        catalog = DataBaseHelper.QuerySingle<dynamic>(cataSql, new { MailId = mailId });

                        if (catalog != null)
                        {
                            param = new DbParameter[]
                            {
                                    new NpgsqlParameter("mailid",mailId),
                                    new NpgsqlParameter("catalogid",catalog.recid),
                            };
                            var resultUUID = DBHelper.ExecuteScalar(dbTrans, createCatalogSql, param, CommandType.Text);
                            param = new DbParameter[]
                            {
                                    new NpgsqlParameter("mailid",mailId),
                                    new NpgsqlParameter("catalogid",resultUUID),
                            };
                            DBHelper.ExecuteNonQuery(dbTrans, catalogRelationSql, param, CommandType.Text);
                            tipMsg = "邮件已经恢复到恢复已删除文件夹中";
                        }
                        else
                        {
                            throw new Exception("缺少个人文件夹");
                        }
                    }
                    else
                    {
                        var catalogRelationExist = DataBaseHelper.QuerySingle<MailReconvert>(validCatalogRelationSql, new { MailId = mailId });//先判断目录关系是否存在
                        if (catalogRelationExist.CatalogId == Guid.Empty)
                        {
                            param = new DbParameter[]
                           {
                                new NpgsqlParameter("mailid",mailId),
                                new NpgsqlParameter("catalogid",catalogRelationExist.SrcCatalogId),
                           };
                            DBHelper.ExecuteNonQuery(dbTrans, catalogRelationSql, param, CommandType.Text);
                        }
                        tipMsg = "邮件已经恢复到" + catalogExist.recname + "文件夹中";
                    }
                    param = new DbParameter[]
                    {
                        new NpgsqlParameter("recstatus",recstatus),
                        new NpgsqlParameter("mailid",mailId)
                    };
                    var result = DBHelper.ExecuteNonQuery(dbTrans, sql, param, CommandType.Text);
                }
                if (entity.MailIds.Split(',').Count() == 1)
                {
                    return new OperateResult
                    {
                        Flag = 1,
                        Msg = tipMsg
                    };
                }
                else
                {
                    return new OperateResult
                    {
                        Flag = 1,
                        Msg = "恢复邮件成功"
                    };
                }
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
                                        "(SELECT row_to_json(t) FROM (SELECT mailaddress address,CASE WHEN displayname='' OR displayname IS NULL THEN split_part(mailaddress,'@',1) ELSE displayname END ,box.nickname  FROM crm_sys_mail_senderreceivers sender LEFT JOIN  crm_sys_mail_mailbox box ON sender.mailaddress = box.accountid  AND  box.recstatus=1  WHERE  ctype=1 AND mailid=body.recid LIMIT 1) t)::jsonb sender," +
                                        "(SELECT array_to_json(array_agg(row_to_json(t))) FROM (SELECT mailaddress address,CASE WHEN displayname='' OR displayname IS NULL THEN split_part(mailaddress,'@',1) ELSE displayname END,box.nickname  FROM crm_sys_mail_senderreceivers sender LEFT JOIN  crm_sys_mail_mailbox box ON sender.mailaddress = box.accountid AND  box.recstatus=1  WHERE ctype=2 AND mailid=body.recid ) t)::jsonb receiversjson," +
                                        "(SELECT array_to_json(array_agg(row_to_json(t))) FROM (SELECT mailaddress address,CASE WHEN displayname='' OR displayname IS NULL THEN split_part(mailaddress,'@',1) ELSE displayname END ,box.nickname  FROM crm_sys_mail_senderreceivers sender LEFT JOIN  crm_sys_mail_mailbox box ON sender.mailaddress = box.accountid AND  box.recstatus=1  WHERE   ctype=3 AND mailid=body.recid ) t)::jsonb ccersjson," +
                                        "(SELECT array_to_json(array_agg(row_to_json(t))) FROM (SELECT mailaddress address,CASE WHEN displayname='' OR displayname IS NULL THEN split_part(mailaddress,'@',1) ELSE displayname END,box.nickname  FROM crm_sys_mail_senderreceivers sender LEFT JOIN  crm_sys_mail_mailbox box ON sender.mailaddress = box.accountid AND  box.recstatus=1  WHERE  ctype=4 AND mailid=body.recid ) t)::jsonb bccersjson," +
                                        "body.ccers ccersstr," +
                                        "body.bccers bccersstr," +
                                        "body.receivers receiversstr," +
                                        "body.sender senderstr," +
                                        "body.title," +
                                        "body.mailbody," +
                                        "COALESCE(body.senttime,body.receivedtime)  senttime," +
                                        "COALESCE(body.receivedtime,body.senttime)  receivedtime," +
                                        "body.istag," +
                                        "body.isread," +
                                        "(SELECT COUNT(1) FROM crm_sys_mail_attach WHERE mailid=body.recid) attachcount," +
                                        "(SELECT mailaddress FROM crm_sys_mail_receivemailrelated WHERE mailid=body.recid LIMIT 1) frommailaddress," +
                                        "(SELECT array_to_json(array_agg(row_to_json(t))) FROM (SELECT filename,mongoid AS fileid,filesize FROM crm_sys_mail_attach WHERE  mailid=body.recid ) t)::jsonb attachinfojson" +
                                        " FROM crm_sys_mail_mailbody body Where body.recid=@mailid ";
            var isConExistsSql = @"Select count(1) From crm_sys_contact Where (belcust->>'id') IN (
                                                SELECT regexp_split_to_table(custid,'id') 
                                                   FROM (SELECT (belcust->>'id') custid FROM crm_sys_contact 
                                                WHERE email=(Select mailaddress From 
                                                   crm_sys_mail_senderreceivers Where
                                                    mailid=@mailid And ctype=1)  AND 
                                                   recstatus=1 LIMIT 1) AS tmp )";
            var senderSql = @" SELECT username as recname,usertel as phone,CASE WHEN useremail IS NULL OR useremail='' THEN (Select mailaddress From
 crm_sys_mail_senderreceivers Where mailid='0e9f4571-fb80-4216-922e-98abdfdaac18' And ctype=1 LIMIT 1) ELSE useremail END  as email,usericon as headicon FROM crm_sys_userinfo WHERE
               userid = (SELECT owner::int4 FROM crm_sys_mail_mailbox  WHERE accountid=(Select mailaddress From crm_sys_mail_senderreceivers Where mailid=@mailid And ctype=1 LIMIT 1) LIMIT 1) AND  recstatus=1;";
            var senderMailSql = @"Select displayname as recname,'' as phone,mailaddress as email,'' as headicon From crm_sys_mail_senderreceivers Where mailid=@mailid And ctype=1 LIMIT 1";
            var conCustSql = @"SELECT recname ,phone ,email ,headicon  FROM crm_sys_contact WHERE email=(Select mailaddress address From crm_sys_mail_senderreceivers Where mailid=@mailid And ctype=1)  AND recstatus=1";

            var isCustExistsSql = @"Select count(1) From crm_sys_customer Where recid=(SELECT belcust->>'id' FROM crm_sys_contact WHERE email=(Select mailaddress From crm_sys_mail_senderreceivers Where mailid=@mailid And ctype=1)  AND recstatus=1 LIMIT 1)::uuid";
            var custConfigSql = @"  SELECT tmp.columnkey||tmp1.extracolumn FROM (  SELECT string_agg(fieldname,',') AS columnkey     
                                                    FROM (
                                                        SELECT f.fieldid,f.fieldname FROM crm_sys_entity_fields AS f
                                                        WHERE f.entityid='349cba2f-42b0-44c2-89f5-207052f50a00' 
                                                        AND controltype  NOT IN(20,1001,1002,1003,1004,1005,1006,1007,1008) AND recstatus=1
                                                    ) AS t ) AS tmp, (select
                                                    crm_func_entity_protocol_extrainfo_fetch AS extracolumn from crm_func_entity_protocol_extrainfo_fetch('349cba2f-42b0-44c2-89f5-207052f50a00',@userid)) AS tmp1";
            var custConSql = @"SELECT  '349cba2f-42b0-44c2-89f5-207052f50a00' as rectype,{0}   from crm_sys_customer e WHERE recid IN (
                                        SELECT regexp_split_to_table(custids,',')::uuid custid FROM (
                                        SELECT (belcust->>'id') AS custids FROM crm_sys_contact WHERE email=
                                        (Select mailaddress From crm_sys_mail_senderreceivers Where mailid=@mailid And ctype=1)  AND recstatus=1) AS tmp ) AND recstatus=1";
            var custMailSuffixSql = @"SELECT '349cba2f-42b0-44c2-89f5-207052f50a00' as rectype,{0}  FROM crm_sys_customer as e WHERE e.recstatus=1 AND e.mailsuffix=(Select substr(mailaddress,position('@' in mailaddress)+1,length(mailaddress)+1) From crm_sys_mail_senderreceivers Where mailid=@mailid And ctype=1)";
            var contactConfigSql = @"select   crm_func_entity_protocol_extrainfo_fetch AS extracolumn from crm_func_entity_protocol_extrainfo_fetch('e450bfd7-ff17-4b29-a2db-7ddaf1e79342',@userid)";

            var contactsSql = @" Select e.* {0} From crm_sys_contact as e Where (belcust->> 'id')   IN (SELECT regexp_split_to_table(custid, ',')  FROM(SELECT(belcust->> 'id') as custid FROM crm_sys_contact WHERE email = (Select mailaddress From  crm_sys_mail_senderreceivers Where mailid =@mailid  And ctype = 1 LIMIT 1)  AND recstatus = 1 LIMIT 1) AS tmp )";
            var param = new DynamicParameters();
            param.Add("mailid", entity.MailId);
            param.Add("userid", userId);
            param.Add("maxpagesize", 99999);
            var mailDetail = DataBaseHelper.QuerySingle<MailBodyDetailMapper>(sql, param, CommandType.Text);
            var custContactCount = DataBaseHelper.QuerySingle<int>(isConExistsSql, param, CommandType.Text);
            List<dynamic> senderResult = new List<dynamic>();
            List<dynamic> contactsResult = new List<dynamic>();
            if (custContactCount > 0)//优先找客户下的联系人 没有找邮箱信息的
            {
                senderResult = DataBaseHelper.Query<dynamic>(conCustSql, param, CommandType.Text);
                contactsSql = string.Format(contactsSql, DataBaseHelper.QuerySingle<string>(contactConfigSql, param));
                contactsResult = DataBaseHelper.Query<dynamic>(contactsSql, param, CommandType.Text);
            }
            else
            {
                senderResult = DataBaseHelper.Query<dynamic>(senderSql, param, CommandType.Text);
            }
            if (senderResult == null || senderResult.Count == 0)
            {
                senderResult = DataBaseHelper.Query<dynamic>(senderMailSql, param, CommandType.Text);
            }
            var countRecord = DataBaseHelper.QuerySingle<int>(isCustExistsSql, param, CommandType.Text);
            List<dynamic> custResult = new List<dynamic>();
            if (countRecord > 0)
            {
                custConSql = string.Format(custConSql, DataBaseHelper.QuerySingle<string>(custConfigSql, param));
                custResult = DataBaseHelper.Query<dynamic>(custConSql, param, CommandType.Text);
            }
            if (custResult == null || custResult.Count == 0)
            {
                custMailSuffixSql = string.Format(custMailSuffixSql, DataBaseHelper.QuerySingle<string>(custConfigSql, param));
                custResult = DataBaseHelper.Query<dynamic>(custMailSuffixSql, param, CommandType.Text);
            }
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
	                                        , attachcount, urgency, mongoid, 0, 0
	                                        , senttime, now() FROM crm_sys_mail_mailbody
                                        WHERE recid = @mailid
                                        RETURNING recid";

            var mailAttachSql = @"INSERT INTO crm_sys_mail_attach(filename,filetype,filesize,mongoid,mailid) Select @filename,@filetype,@filesize,@mongoid,@newmailid";

            var mailSenderreceiversSql = @"INSERT INTO crm_sys_mail_senderreceivers (mailid,ctype,biztype,mailaddress,displayname,ismailgroup,	relativetocotract,relativetouser,relativetodept,relativemailbox)
         SELECT @newmailid::uuid,ctype,biztype,mailaddress,displayname,ismailgroup,relativetocotract,relativetouser,relativetodept	,relativemailbox FROM crm_sys_mail_senderreceivers WHERE mailid=@mailid";
            var mailSendRecordSql = @"INSERT INTO crm_sys_mail_sendrecord (mailid,actiontype,status,message,createdtime, lastupdatetime, completedtime, nexttrytime) SELECT @newmailid,1,@status,@msg,now(),now(),now(),now();";
            var mailInnerTransferRecord = @"INSERT INTO crm_sys_mail_intransferrecord  ( reccreator, recupdator, recmanager, userid, transferuserid, fromuser,mailid,newmailid) Select @userid,@userid,@userid,@userid,@transferuserid,@userid,@mailid,@newmailid";

            var subDeptUserSql = @"SELECT userid FROM crm_sys_account_userinfo_relate WHERE recstatus = 1 AND deptid IN 
                                           (SELECT deptid FROM crm_func_department_tree(@deptid,1)) AND userid!=@userid";

            var emailUserSql = @" SELECT Distinct owner::int4 FROM crm_sys_mail_mailbox WHERE owner::int4 IN (SELECT regexp_split_to_table(@userids,',')::int4)  ";

            var cataHandleSql = "Select * From  crm_func_mail_cata_transfer_related_handle(@newmailid,@transferuserid,@userid);";

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
                           new NpgsqlParameter("newmailid",result)
                       };
                        DBHelper.ExecuteNonQuery(tran, mailInnerTransferRecord, param, CommandType.Text);
                        param = new DbParameter[]
                        {
                            new NpgsqlParameter("newmailid",result),
                           new NpgsqlParameter("transferuserid", user),
                            new NpgsqlParameter("userid", userId),
                        };
                        var operateResult = DBHelper.ExecuteQuery<OperateResult>(tran, cataHandleSql, param).FirstOrDefault();
                        // @newmailid,1,@status,@msg,
                        param = new DbParameter[]
                        {
                            new NpgsqlParameter("newmailid",result),
                            new NpgsqlParameter("status", operateResult.Flag==0?7:6),
                            new NpgsqlParameter("msg", operateResult.Msg),
                        };
                        DBHelper.ExecuteNonQuery(tran, mailSendRecordSql, param, CommandType.Text);
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
            var sql = @"SELECT COALESCE(u1.workcode,'') as workcode,u1.username,u.username as fromuser ,transfer.reccreated as transfertime FROM  crm_sys_mail_intransferrecord transfer
                                LEFT JOIN crm_sys_userinfo u ON transfer.fromuser=u.userid
                                LEFT JOIN crm_sys_userinfo u1 ON transfer.transferuserid=u1.userid WHERE mailid=@mailid or newmailid=@mailid::uuid";

            var param = new DbParameter[]
            {
                   new NpgsqlParameter("mailid",entity.MailId.ToString())
            };
            return ExecuteQueryByPaging<TransferRecordMapper>(sql, param, entity.PageSize, entity.PageIndex);
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

            string sql = @" WITH T1 AS (
                                        Select                               
                                        body.recid mailid, 
                                        (SELECT row_to_json(t) FROM (SELECT mailaddress as address,displayname FROM crm_sys_mail_senderreceivers WHERE ctype=1 AND mailid=body.recid LIMIT 1) t)::jsonb sender,
                                        body.title, 
                                        body.mailbody as summary, 
                                        COALESCE(body.senttime, body.receivedtime)  senttime,
                                        COALESCE(body.receivedtime,body.senttime)  receivedtime,
                                        rl.mailserverid,
                                        rl.userid
                                        FROM crm_sys_mail_mailbody body  LEFT JOIN crm_sys_mail_receivemailrelated rl ON body.recid=rl.mailid {0} 
                                        ),
                                        T2 AS (
                                              SELECT * FROM (SELECT RANK() OVER(PARTITION BY T1.mailserverid  order by T1.userid desc) id,T1.mailserverid,T1.mailid FROM  T1  ) AS t WHERE t.id=1

                                        )
                                        SELECT * FROM T1 WHERE T1.mailid IN (SELECT mailid FROM T2)     ORDER BY T1.receivedtime DESC  LIMIT @pagesize OFFSET @pageindex
                                        ";
            string countSql = @" WITH T1 AS (
                                        Select                               
                                        body.recid mailid, 
                                        (SELECT row_to_json(t) FROM (SELECT mailaddress as address,displayname FROM crm_sys_mail_senderreceivers WHERE ctype=1 AND mailid=body.recid LIMIT 1) t)::jsonb sender, 
                                        body.title, 
                                        body.mailbody as summary, 
                                        COALESCE(body.senttime, body.receivedtime)  senttime,
                                        COALESCE(body.receivedtime,body.senttime)  receivedtime,
										rl.mailserverid,
                                        rl.userid
                                        FROM crm_sys_mail_mailbody body LEFT JOIN crm_sys_mail_receivemailrelated rl ON body.recid=rl.mailid  {0} 
                                        ),
                                        T2 AS (
                                              SELECT * FROM (SELECT RANK() OVER(PARTITION BY T1.mailserverid  order by T1.userid desc) id,T1.mailserverid,T1.mailid FROM  T1  ) AS t WHERE t.id=1

                                        )
                                        SELECT COUNT(1) FROM T1 WHERE T1.mailid IN (SELECT mailid FROM T2) 

                                        ";
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
                                        )) AND recid IN (Select mailid From crm_sys_mail_senderreceivers Where mailaddress IN (SELECT accountid FROM crm_sys_mail_mailbox WHERE recstatus=1 AND owner=@userid))  ";
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
                                        )) AND recid IN (Select mailid From crm_sys_mail_senderreceivers Where mailaddress IN (SELECT accountid FROM crm_sys_mail_mailbox WHERE recstatus=1 AND owner=@userid) AND (ctype=2 or ctype=3 or ctype=4))";
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
                                        )) AND recid IN (Select mailid From crm_sys_mail_senderreceivers Where mailaddress IN (SELECT accountid FROM crm_sys_mail_mailbox WHERE  recstatus=1) AND (ctype=2 or ctype=3 or ctype=4))";

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
            sql = string.Format(sql, whereSql);
            countSql = string.Format(countSql, whereSql);
            if (entity.PageSize <= 0)
            {
                entity.PageSize = 1000000;
            }
            if (entity.PageIndex <= 1)
            {
                entity.PageIndex = entity.PageIndex - 1;
            }
            var param = new
            {
                MailId = entity.MailId,
                UserId = userId.ToString(),
                PageSize = entity.PageSize,
                PageIndex = entity.PageIndex
            };
            PageDataInfo<MailBodyMapper> pageData = new PageDataInfo<MailBodyMapper>();
            pageData.DataList = DataBaseHelper.Query<MailBodyMapper>(sql, param);
            pageData.PageInfo = new PageInfo
            {
                TotalCount = DataBaseHelper.QuerySingle<long>(countSql, param),
                PageSize = entity.PageSize,
            };
            return pageData;
        }


        public PageDataInfo<ToAndFroFileMapper> GetInnerToAndFroAttachment(ToAndFroMapper entity, int userId)
        {

            string sql = @" WITH T1 AS (
                                        Select                               
                                        body.recid mailid,
										rl.mailserverid,
                                        att.filename,att.filetype,att.filesize,att.mongoid, COALESCE(body.senttime, body.receivedtime)  senttime,
                                        COALESCE(body.receivedtime,body.senttime)  receivedtime,rl.userid FROM crm_sys_mail_attach att LEFT JOIN crm_sys_mail_mailbody body ON body.recid=att.mailid
                                         LEFT JOIN crm_sys_mail_receivemailrelated rl ON body.recid=rl.mailid {0} 
                                        ),
                                        T2 AS (
                                              SELECT * FROM (SELECT RANK() OVER(PARTITION BY T1.mailserverid  order by T1.userid desc) id,T1.mailserverid,T1.mailid FROM  T1  ) AS t WHERE t.id=1

                                        )
                                        SELECT * FROM T1 WHERE T1.mailid IN (SELECT mailid FROM T2)     ORDER BY T1.receivedtime DESC  LIMIT @pagesize OFFSET @pageindex

                                        ";
            string countSql = @" WITH T1 AS (
                                        Select                               
                                        body.recid mailid,
										rl.mailserverid,
                                        att.filename,att.filetype,att.filesize,att.mongoid, COALESCE(body.senttime, body.receivedtime)  senttime,
                                        COALESCE(body.receivedtime,body.senttime)  receivedtime,rl.userid FROM crm_sys_mail_attach att LEFT JOIN crm_sys_mail_mailbody body ON body.recid=att.mailid
                                         LEFT JOIN crm_sys_mail_receivemailrelated rl ON body.recid=rl.mailid {0} 
                                        ),
                                        T2 AS (
                                              SELECT * FROM (SELECT RANK() OVER(PARTITION BY T1.mailserverid  order by T1.userid desc) id,T1.mailserverid,T1.mailid FROM  T1  ) AS t WHERE t.id=1
                                        )
                                        SELECT COUNT(1) FROM T1 WHERE T1.mailid IN (SELECT mailid FROM T2)    

                                        ";
            string whereSql = string.Empty;

            if (entity.relatedMySelf == 0 && entity.relatedSendOrReceive == 0)
            {
                whereSql = @"Where att.recstatus=1 and att.mailid=@mailid ";
            }
            else if (entity.relatedMySelf == 0 && entity.relatedSendOrReceive == 1)
            {
                whereSql = @"Where att.recstatus=1 and att.mailid=@mailid  AND att.mailid IN (Select mailid From crm_sys_mail_senderreceivers Where mailaddress IN (SELECT accountid FROM crm_sys_mail_mailbox WHERE  recstatus=1) AND (ctype=2 or ctype=3 or ctype=4))";
            }
            else if (entity.relatedMySelf == 0 && entity.relatedSendOrReceive == 2)
            {
                whereSql = @"Where att.recstatus=1 and att.mailid=@mailid  AND att.mailid IN (Select mailid From crm_sys_mail_senderreceivers Where mailaddress IN (SELECT accountid FROM crm_sys_mail_mailbox WHERE recstatus=1) AND ctype=1)";
            }
            // 与自己往来+收到和发出的邮件
            else if (entity.relatedMySelf == 1 && entity.relatedSendOrReceive == 0)
            {
                whereSql = @"Where  att.recstatus=1 and att.mailid IN (
                                        SELECT mailid FROM crm_sys_mail_senderreceivers WHERE mailaddress IN (
                                        SELECT tmp.email FROM (
                                        SELECT regexp_split_to_table((belcust->>'id'),',') AS custid,email FROM crm_sys_contact WHERE recstatus=1 ) AS tmp 
                                        WHERE tmp.custid IN (
                                        SELECT regexp_split_to_table((belcust->>'id'),',')  AS custid FROM crm_sys_contact WHERE email IN(
                                        SELECT mailaddress FROM crm_sys_mail_senderreceivers WHERE mailid=@mailid AND ctype=1 ))  
                                        )) AND att.mailid IN (Select mailid From crm_sys_mail_senderreceivers Where mailaddress IN (SELECT accountid FROM crm_sys_mail_mailbox WHERE recstatus=1 AND owner=@userid))  ";
            }
            else if (entity.relatedMySelf == 1 && entity.relatedSendOrReceive == 1)            //与自己往来+收到的邮件
            {
                whereSql = @" Where  att.recstatus=1 and  att.mailid IN (
                                        SELECT mailid FROM crm_sys_mail_senderreceivers WHERE mailaddress IN (
                                        SELECT tmp.email FROM (
                                        SELECT regexp_split_to_table((belcust->>'id'),',') AS custid,email FROM crm_sys_contact WHERE recstatus=1 ) AS tmp 
                                        WHERE tmp.custid IN (
                                        SELECT regexp_split_to_table((belcust->>'id'),',')  AS custid FROM crm_sys_contact WHERE email IN(
                                        SELECT mailaddress FROM crm_sys_mail_senderreceivers WHERE mailid=@mailid AND ctype=1 ))  
                                        )) AND att.mailid IN (Select mailid From crm_sys_mail_senderreceivers Where mailaddress IN (SELECT accountid FROM crm_sys_mail_mailbox WHERE recstatus=1 AND owner=@userid) AND (ctype=2 or ctype=3 or ctype=4))";
            }
            else if (entity.relatedMySelf == 1 && entity.relatedSendOrReceive == 2)         //与自己往来+发出的邮件
            {
                whereSql = @" Where att.recstatus=1 and  att.mailid IN (
                                        SELECT mailid FROM crm_sys_mail_senderreceivers WHERE mailaddress IN (
                                        SELECT tmp.email FROM (
                                        SELECT regexp_split_to_table((belcust->>'id'),',') AS custid,email FROM crm_sys_contact WHERE recstatus=1 ) AS tmp 
                                        WHERE tmp.custid IN (
                                        SELECT regexp_split_to_table((belcust->>'id'),',')  AS custid FROM crm_sys_contact WHERE email IN(
                                        SELECT mailaddress FROM crm_sys_mail_senderreceivers WHERE mailid=@mailid AND ctype=1 ))  
                                        ))  AND att.mailid IN (Select mailid From crm_sys_mail_senderreceivers Where mailaddress IN (SELECT accountid FROM crm_sys_mail_mailbox WHERE recstatus=1 AND owner=@userid) AND ctype=1)";
            }
            else if (entity.relatedMySelf == 2 && entity.relatedSendOrReceive == 0)       //与所有用户往来+收到和发出的邮件
            {
                whereSql = @"  Where  att.recstatus=1 and  att.mailid IN (
                                        SELECT mailid FROM crm_sys_mail_senderreceivers WHERE mailaddress IN (
                                        SELECT tmp.email FROM (
                                        SELECT regexp_split_to_table((belcust->>'id'),',') AS custid,email FROM crm_sys_contact WHERE recstatus=1 ) AS tmp 
                                        WHERE tmp.custid IN (
                                        SELECT regexp_split_to_table((belcust->>'id'),',')  AS custid FROM crm_sys_contact WHERE email IN(
                                        SELECT mailaddress FROM crm_sys_mail_senderreceivers WHERE mailid=@mailid AND ctype=1 ))  
                                        )) AND att.mailid IN (Select mailid From crm_sys_mail_senderreceivers Where mailaddress IN (SELECT accountid FROM crm_sys_mail_mailbox Where recstatus=1))";

            }
            else if (entity.relatedMySelf == 2 && entity.relatedSendOrReceive == 1)//与所有用户往来+收出的邮件
            {
                whereSql = @"  Where  att.recstatus=1 and  att.mailid IN (
                                        SELECT mailid FROM crm_sys_mail_senderreceivers WHERE mailaddress IN (
                                        SELECT tmp.email FROM (
                                        SELECT regexp_split_to_table((belcust->>'id'),',') AS custid,email FROM crm_sys_contact WHERE recstatus=1 ) AS tmp 
                                        WHERE tmp.custid IN (
                                        SELECT regexp_split_to_table((belcust->>'id'),',')  AS custid FROM crm_sys_contact WHERE email IN(
                                        SELECT mailaddress FROM crm_sys_mail_senderreceivers WHERE mailid=@mailid AND ctype=1 ))  
                                        )) AND att.mailid IN (Select mailid From crm_sys_mail_senderreceivers Where mailaddress IN (SELECT accountid FROM crm_sys_mail_mailbox WHERE  recstatus=1) AND (ctype=2 or ctype=3 or ctype=4))";

            }

            else if (entity.relatedMySelf == 2 && entity.relatedSendOrReceive == 2)//与所有用户往来+发出的邮件
            {
                whereSql = @"  Where  att.recstatus=1 and  att.mailid IN (
                                        SELECT mailid FROM crm_sys_mail_senderreceivers WHERE mailaddress IN (
                                        SELECT tmp.email FROM (
                                        SELECT regexp_split_to_table((belcust->>'id'),',') AS custid,email FROM crm_sys_contact WHERE recstatus=1 ) AS tmp 
                                        WHERE tmp.custid IN (
                                        SELECT regexp_split_to_table((belcust->>'id'),',')  AS custid FROM crm_sys_contact WHERE email IN(
                                        SELECT mailaddress FROM crm_sys_mail_senderreceivers WHERE mailid=@mailid AND ctype=1 ))  
                                        )) AND att.mailid IN (Select mailid From crm_sys_mail_senderreceivers Where mailaddress IN (SELECT accountid FROM crm_sys_mail_mailbox WHERE recstatus=1) AND ctype=1)";

            }
            sql = string.Format(sql, whereSql);
            countSql = string.Format(countSql, whereSql);
            if (entity.PageSize <= 0)
            {
                entity.PageSize = 1000000;
            }
            if (entity.PageIndex <= 1)
            {
                entity.PageIndex = entity.PageIndex - 1;
            }
            var param = new
            {
                MailId = entity.MailId,
                UserId = userId.ToString(),
                PageSize = entity.PageSize,
                PageIndex = entity.PageIndex
            };
            PageDataInfo<ToAndFroFileMapper> pageData = new PageDataInfo<ToAndFroFileMapper>();
            pageData.DataList = DataBaseHelper.Query<ToAndFroFileMapper>(sql, param);
            pageData.PageInfo = new PageInfo
            {
                TotalCount = DataBaseHelper.QuerySingle<long>(countSql, param),
                PageSize = entity.PageSize,
            };
            return pageData;
        }

        public PageDataInfo<AttachmentChooseListMapper> GetLocalFileFromCrm(AttachmentListMapper entity, string ruleSql, int userId)
        {
            var sql = @"
                 Select * From (
                            SELECT fileid,filename,filelength as filesize
				            FROM public.crm_sys_documents AS d Where entityid='a3500e78-fe1c-11e6-aee4-005056ae7f49'  AND recstatus=1 AND substr(filename,length(filename)-3,4)!='.exe'  {0}   UNION ALL 
                             SELECT  fileid,filename,filelength as filesize
                             FROM crm_sys_documents  AS d   WHERE entityid IN (
                            SELECT entityid FROM crm_sys_entity WHERE modeltype=0 AND recstatus=1 ) AND substr(filename,length(filename)-3,4)!='.exe'   AND recstatus=1 AND reccreator=@userid) as t Where 1=1 {1}";
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

        public ReceiveMailRelatedMapper GetUserReceiveMailTime(string mailAddress, int userId)
        {
            var sql = @"SELECT * FROM crm_sys_mail_receivemailrelated WHERE userid=@userid and mailaddress=@mailaddress ORDER BY  receivetime desc LIMIT 1";
            return DataBaseHelper.QuerySingle<ReceiveMailRelatedMapper>(sql, new { UserId = userId, MailAddress = mailAddress });
        }

        public List<ReceiveMailRelatedMapper> GetReceiveMailRelated(int userId)
        {
            var sql = @"SELECT * FROM crm_sys_mail_receivemailrelated WHERE userid=@userid";
            return DataBaseHelper.Query<ReceiveMailRelatedMapper>(sql, new { UserId = userId });
        }

        /// <summary>
        /// 获取我的邮箱列表
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public PageDataInfo<MailBox> GetMailBoxList(int pageIndex, int pageSize, int userId)
        {
            var sql = "select a.recid,a.accountid,a.recname,a.inwhitelist,b.recname mailserver,b.mailprovider,b.imapaddress," +
                "b.refreshinterval,b.servertype,b.smtpaddress,a.signature	" +
                " from crm_sys_mail_mailbox a " +
                " inner join crm_sys_mail_server b on(a.mailserver->> 'id')::uuid = b.recid " +
                " where a.OWNER= @userid  and a.recstatus=1 ";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("userid", userId.ToString())
            };
            var result = ExecuteQueryByPaging<MailBox>(sql, param, pageSize, pageIndex);
            return result;
        }
        public OperateResult MirrorWritingMailStatus(Guid mailId, int mailStatus, int userId, DbTransaction dbTrans = null)
        {

            if (dbTrans == null)
            {
                DbConnection conn = DBHelper.GetDbConnect();
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }
                dbTrans = conn.BeginTransaction();
            }
            try
            {
                var sql = @" update crm_sys_mail_sendrecord set status=@mailstatus where mailid=@mailid;";
                var mailCatalogSql = @"		SELECT recid  FROM crm_sys_mail_catalog WHERE viewuserid=@userid AND ctype=@ctype";
                var mailCatalogChangeSql = @" update crm_sys_mail_catalog_relation set catalogid=@catalogid where mailid=@mailid;";
                var param = new DbParameter[]
                {
                new NpgsqlParameter("mailstatus",mailStatus),
                new NpgsqlParameter("mailid",mailId)
                };

                int count = DBHelper.ExecuteNonQuery(dbTrans, sql, param, CommandType.Text);
                if (count > 0)
                {
                    var arg = new DynamicParameters();
                    arg.Add("userid", userId);
                    if (mailStatus != 6)
                    {
                        arg.Add("ctype", 1003);
                    }
                    else
                    {
                        arg.Add("ctype", 1004);
                    }
                    var catalogId = DataBaseHelper.QuerySingle<Guid>(mailCatalogSql, arg);
                    if (catalogId == Guid.Empty)
                        throw new Exception("该用户没有发送或已发送目录");
                    param = new DbParameter[]
                    {
                    new NpgsqlParameter("catalogid",catalogId),
                    new NpgsqlParameter("mailid",mailId)
                    };
                    count = DBHelper.ExecuteNonQuery(dbTrans, mailCatalogChangeSql, param, CommandType.Text);
                    if (count > 0)
                    {
                        return new OperateResult
                        {
                            Flag = 1
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                dbTrans.Rollback();
                return new OperateResult
                {
                    Flag = 1
                };
            }
            finally
            {
                dbTrans.Commit();
                dbTrans.Dispose();

            }
            return new OperateResult
            {
                Flag = 0
            };
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
            var sql = "SELECT box.accountid,u.userid,u.username FROM crm_sys_mail_mailbox box LEFT JOIN crm_sys_userinfo u ON box.owner::int4 = u.userid  WHERE box.inwhitelist = @iswhitelst ";
            var param = new
            {
                IsWhiteLst = isWhiteLst,
                UserId = userId
            };
            return DataBaseHelper.Query<MailBoxMapper>(sql, param, CommandType.Text);
        }
        /// <summary>
        /// 内部人员
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<InnerUserMailMapper> GetUserMailList(int userId)
        {
            var sql = @"SELECT userid,useremail FROM crm_sys_userinfo WHERE recstatus=1
                               UNION 
                               SELECT owner::int4,accountid FROM crm_sys_mail_mailbox WHERE recstatus=1 "
            ;
            return DataBaseHelper.Query<InnerUserMailMapper>(sql, CommandType.Text);
        }
        #endregion
        #region  模糊查询我的通讯人员
        public List<MailUserMapper> GetContactByKeyword(string keyword, int count, int userId)
        {
            var sql = @"select email EmailAddress,name from (SELECT email,recname as name FROM crm_sys_contact 
                     WHERE email is not null and email!=''  and (belcust->> 'id')::uuid IN (SELECT recid FROM crm_sys_customer 
                     WHERE recmanager = @userid) 
                UNION ALL 
                select useremail mail,username as name from crm_sys_userinfo a where a.recstatus=1 and a.useremail is not null  and a.useremail!= ''
                UNION ALL
                select a.accountid mail,b.username as name  from crm_sys_mail_mailbox a inner join crm_sys_userinfo b on a.OWNER::integer=b.userid 
                where b.recstatus=1 and a.recstatus=1
                ) x where 1=1 {0}
                group by email,name ";

            string condition = string.Empty;
            if (string.IsNullOrEmpty(keyword))
            {
                sql = string.Format(sql, "");
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
                condition = string.Format(" and name like '%{0}%' or email like '%{1}%' ", keyword, keyword);
                if (count == 0)
                {
                    sql = string.Format(sql + " LIMIT 10", condition);
                }
                else
                {
                    sql = string.Format(sql + " LIMIT {1} ", condition, count);
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
            var sql = @"                   
                select * from (select ''::text mail,deptid::text treeid,deptname treename,''::text deptname,0 nodetype,'00000000-0000-0000-0000-000000000000'::uuid icon from crm_sys_department a 
                            where a.recstatus = 1 and a.pdeptid::text =@deptId order by recorder) t 
                            UNION ALL 
                    select * from (
                    SELECT t2.mail,t2.treeid,t2.treename,t2.deptname,1 nodetype,t2.icon from 
                                    (select regexp_split_to_table(mail,';') mail,x.userid::text treeid,x.username treename,d.deptname,x.icon from (
                                    select useremail mail,userid,username,usericon::uuid icon  from crm_sys_userinfo a where a.recstatus=1 and a.useremail is not null  and a.useremail!= '' 
                                    UNION all 
                                    select a.accountid mail,b.userid,b.username,b.usericon::uuid icon  from crm_sys_mail_mailbox a inner join crm_sys_userinfo b on a.OWNER::integer=b.userid where b.recstatus=1 and a.recstatus=1) x
                                    inner join crm_sys_account_userinfo_relate ur on ur.userid=x.userid
                                    left join crm_sys_department d on d.deptid=ur.deptid
                                        where ur.recstatus = 1 and d.deptid::text=@deptId ) t2 where t2.mail is not null
                                    group by t2.mail,t2.treeid,t2.treename,t2.deptname,t2.icon  order by  t2.treename ) t3 ";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("deptId", deptId)
            };
            return ExecuteQuery<OrgAndStaffMapper>(sql, param);
        }

        /// <summary>
        /// 获取企业内部通讯录_人员查询
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>       
        public PageDataInfo<OrgAndStaffMapper> GetInnerPersonContact(string keyword, int pageIndex, int pageSize, int userId)
        {
            string sql = @"select t2.mail,t2.treeid,t2.treename,t2.deptname,t2.icon, 1 nodetype from (select regexp_split_to_table(mail,';') mail,x.userid::text treeid,x.username treename,d.deptname,x.icon from (select t.mail,t.userid,t.username,t.icon from (
                select useremail mail,userid,username,usericon::uuid icon  from crm_sys_userinfo a where a.recstatus=1 and a.useremail is not null  and a.useremail!= '' 
                UNION all 
                select a.accountid mail,b.userid,b.username,b.usericon::uuid icon  from crm_sys_mail_mailbox a inner join crm_sys_userinfo b on a.OWNER::integer=b.userid where b.recstatus=1  ) t 
                    group by t.mail,t.userid,t.username,t.icon ) x
                inner join crm_sys_account_userinfo_relate ur on ur.userid=x.userid
                left join crm_sys_department d on d.deptid=ur.deptid
                 where ur.recstatus = 1 {0} ) t2 where t2.mail is not null 
								group by t2.mail,t2.treeid,t2.treename,t2.deptname,t2.icon
                order by t2.treename";
            string condition = string.Empty;
            if (!string.IsNullOrEmpty(keyword))
            {
                condition = string.Format(" and x.username like '%{0}%' or mail like '%{1}%' ", keyword, keyword);

            }
            string newSql = string.Format(sql, condition);

            return ExecuteQueryByPaging<OrgAndStaffMapper>(newSql, new DbParameter[] { }, pageSize, pageIndex);
        }

        /// <summary>
        /// 获取内部分发内部通讯录_人员列表
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>       
        public PageDataInfo<OrgAndStaffMapper> TransferInnerPersonContact(string keyword, int pageIndex, int pageSize, int userId)
        {
            string sql = @"select * from (
                select x.userid::text treeid,x.username treename,d.deptname,1 nodetype,x.icon from (
                select a.accountid mail,b.userid,b.username,b.usericon::uuid icon  from crm_sys_mail_mailbox a inner join crm_sys_userinfo b on a.OWNER::integer=b.userid where b.recstatus=1  ) x
                inner join crm_sys_account_userinfo_relate ur on ur.userid=x.userid
                left join crm_sys_department d on d.deptid=ur.deptid
                 where ur.recstatus = 1  {0}
				 ORDER BY  x.userid,x.username,d.deptname,x.icon ) t 
                order by t.deptname";
            string condition = string.Empty;
            if (!string.IsNullOrEmpty(keyword))
            {
                condition = string.Format(" and x.username like '%{0}%' or mail like '%{1}%' ", keyword, keyword);

            }
            string newSql = string.Format(sql, condition);

            return ExecuteQueryByPaging<OrgAndStaffMapper>(newSql, new DbParameter[] { }, pageSize, pageIndex);
        }

        /// <summary>
        /// 获取内部分发通讯录
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>       
        public List<OrgAndStaffMapper> TransferInnerContact(string deptId, int userId)
        {
            if (string.IsNullOrEmpty(deptId))
            {
                string rootsql = "select deptid::text from crm_sys_department where pdeptid::text = '00000000-0000-0000-0000-000000000000' and recstatus=1 ";
                deptId = (string)ExecuteScalar(rootsql, new DbParameter[] { });
            }
            var sql = @"select * from (select ''::text mail,deptid::text treeid,deptname treename,''::text deptname,0 nodetype,'00000000-0000-0000-0000-000000000000'::uuid icon from crm_sys_department a 
                 where a.recstatus = 1 and a.pdeptid::text =@deptId order by recorder) t 
                 UNION ALL 
                SELECT ''::text mail,t2.treeid,t2.treename,t2.deptname,1 nodetype,t2.icon from 
                                (select mail,x.userid::text treeid,x.username treename,d.deptname,x.icon from (
                                select a.accountid mail,b.userid,b.username,b.usericon::uuid icon  from crm_sys_mail_mailbox a inner join crm_sys_userinfo b on a.OWNER::integer=b.userid where b.recstatus=1) x
                                inner join crm_sys_account_userinfo_relate ur on ur.userid=x.userid
                                left join crm_sys_department d on d.deptid=ur.deptid
                                 where ur.recstatus = 1 and d.deptid::text=@deptId ) t2
                                group by t2.treeid,t2.treename,t2.deptname,t2.icon";
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
            var executeSql = @"select * from (select distinct on (x.mailAddress) x.mailAddress EmailAddress,x.reccreated,COALESCE(x.usericon,'00000000-0000-0000-0000-000000000000')::uuid icon,displayname as name
		         from (select a.reccreated,c.*,u.usericon from crm_sys_mail_mailbody a 
		         inner join crm_sys_mail_sendrecord b on a.recid=b.mailid 
		         inner join crm_sys_mail_senderreceivers c ON c.mailid = b.mailid 
		         left join crm_sys_userinfo u on u.userid=c.relativetouser where c.ctype = 2 and c.displayname is not null and c.displayname!='' and a.recmanager =@userId ) x ) t
		         order by t.reccreated DESC";
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
        public PageDataInfo<MailUserMapper> GetCustomerContact(string keyword, int pageIndex, int pageSize, int userId)
        {
            var executeSql = "SELECT a.recid,a.email EmailAddress,a.recname as name,b.recname customer,COALESCE(a.headicon,'00000000-0000-0000-0000-000000000000')::uuid icon" +
                " FROM crm_sys_contact a inner join crm_sys_customer b on(a.belcust ->> 'id') ::uuid = b.recid " +
                " where a.email is not null and a.email != ''  and a.recstatus = 1 and b.recstatus = 1 and b.recmanager = @userid";
            if (!string.IsNullOrEmpty(keyword))
            {
                executeSql = string.Format(executeSql + " and a.recname like '%{0}%' or a.email like '%{1}%' ", keyword, keyword);

            }
            executeSql = string.Format(executeSql + " order by b.recname");
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
        public List<OrgAndStaffTree> GetInnerToAndFroUser(string keyword, int userId)
        {
            var executeSql = @"SELECT 1 nodetype,u.userid::text treeid,u.username treename,(count(1)-sum(tmp.isread))::int unreadcount  FROM ( SELECT 
			    body.recid mailid,
			    body.isread
			     FROM crm_sys_mail_mailbody body Where body.recid IN (SELECT mailid FROM crm_sys_mail_catalog_relation WHERE catalogid IN(SELECT recid FROM crm_sys_mail_catalog WHERE recstatus = 1 
                    AND viewuserid = @userId AND ctype != 1003) AND body.recstatus=1 AND body.recid IN (
			    SELECT DISTINCT tmp.mailid FROM (SELECT * FROM crm_sys_mail_senderreceivers where mailid in(
			    SELECT
			     mailid FROM crm_sys_mail_senderreceivers where mailaddress IN (SELECT accountid FROM crm_sys_mail_mailbox  WHERE owner::int4 = @userId) AND ctype=1) ) AS tmp
			    INNER JOIN
			    (SELECT * FROM crm_sys_mail_senderreceivers where mailid in(
			    select
			     mailid from crm_sys_mail_senderreceivers where mailaddress IN (SELECT accountid FROM crm_sys_mail_mailbox ) AND (ctype=2 OR ctype=3 OR ctype=4)) ) AS tmp1
			    ON tmp.mailid=tmp1.mailid
			    UNION
			    SELECT DISTINCT tmp.mailid FROM (SELECT * FROM crm_sys_mail_senderreceivers where mailid in(
			    SELECT
			     mailid FROM crm_sys_mail_senderreceivers where mailaddress IN (SELECT accountid FROM crm_sys_mail_mailbox  ) AND ctype=1) ) AS tmp
			    INNER JOIN
			    (SELECT * FROM crm_sys_mail_senderreceivers where mailid in(
			    select
			     mailid from crm_sys_mail_senderreceivers where mailaddress IN (SELECT accountid FROM crm_sys_mail_mailbox  WHERE owner::int4 = @userId ) AND (ctype=2 OR ctype=3 OR ctype=4)) ) AS tmp1
			    ON tmp.mailid=tmp1.mailid
			    )  )) AS tmp inner join crm_sys_mail_senderreceivers t on t.mailid=tmp.mailid
           inner join crm_sys_mail_mailbox t2 on t2.accountid=t.mailaddress
           inner join crm_sys_userinfo u on u.userid=t2.owner::INT4
           where 1=1 {0}
           group by u.userid, u.username";
            string condition = string.Empty;
            if (!string.IsNullOrEmpty(keyword))
            {
                condition = string.Format(" and  u.username like '%{0}%'", keyword);

            }
            string newSql = string.Format(executeSql, condition);
            var param = new DbParameter[]
            {
                new NpgsqlParameter("userId", userId)
            };
            return ExecuteQuery<OrgAndStaffTree>(newSql, param);
        }

        public MailBodyMapper GetMailInfo(List<Guid> mailIds, int userId)
        {
            string strSQL = @"SELECT " +
                            "body.recid mailid," +
                            "body.title," +
                            "body.mailbody," +
                            "body.senttime," +
                            "body.receivedtime," +
                            "body.istag," +
                            "body.isread" +
                            " FROM crm_sys_mail_mailbody body Where body.recstatus=1 AND body.recid IN (select regexp_split_to_table(@mailids,',')::uuid)";
            var param = new
            {
                MailIds = string.Join(",", mailIds.ToArray())
            };
            return DataBaseHelper.QuerySingle<MailBodyMapper>(strSQL, param);
        }

        #endregion


        #region 判断领导是否拥有该下属
        public bool IsHasSubUserAuth(int leaderUserId, int userId)
        {
            var sql = "Select count(1) as ishashauth  From (SELECT userid FROM crm_sys_account_userinfo_relate WHERE recstatus = 1 AND deptid IN(SELECT deptid FROM crm_func_department_tree((SELECT deptid FROM crm_sys_account_userinfo_relate WHERE userid = @userid AND recstatus = 1 LIMIT 1), 1))) as t Where t.userid=@leaderuserid ";

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


        public PageDataInfo<MailTruncateLstMapper> GetReconvertMailList(ReconvertMailMapper entity, int userId)
        {
            string strSQL = @" SELECT  tmp.*,re.srcuserid,ur.username,tmp1.receivers,tmp1.displayname FROM (SELECT " +
                              "body.recid mailid," +
                              "(SELECT row_to_json(t) FROM (SELECT mailaddress address,CASE WHEN displayname='' OR displayname IS NULL THEN split_part(mailaddress,'@',1) ELSE displayname END FROM crm_sys_mail_senderreceivers WHERE ctype=1 AND mailid=body.recid LIMIT 1) t)::jsonb sender," +
                              "body.title," +
                               "COALESCE(body.senttime,body.receivedtime)  senttime," +
                               "COALESCE(body.receivedtime,body.senttime)  receivedtime," +
                              "COALESCE(body.receivedtime,body.senttime) mailtime,body.recstatus" +
                              " FROM crm_sys_mail_mailbody body ) AS tmp INNER JOIN (SELECT array_to_json(array_agg(row_to_json(t))) as receivers,array_to_string(ARRAY(SELECT unnest(array_agg(t.displayname))),',') as displayname,mailid" +
                            " FROM(SELECT mailaddress address, mailid, CASE WHEN displayname = '' OR displayname IS NULL THEN split_part(mailaddress, '@', 1)" +
                            " ELSE displayname END FROM crm_sys_mail_senderreceivers WHERE ctype = 2 ) t GROUP BY t.mailid" +
                            " ) as tmp1 ON tmp1.mailid=tmp.mailid INNER JOIN crm_sys_mail_reconvert re ON re.mailid=tmp.mailid LEFT JOIN crm_sys_userinfo ur ON ur.userid = re.srcuserid   Where   tmp.recstatus=2  {0} ";
            object[] sqlWhere = new object[] { };
            string sqlCondition = string.Empty;
            if (!string.IsNullOrEmpty(entity.KeyWord))
            {
                sqlCondition = "  (((tmp.sender->>'displayname') ILIKE '%' || @keyword || '%' ESCAPE '`') OR (tmp.title ILIKE '%' || @keyword || '%' ESCAPE '`') OR (tmp1.displayname ILIKE '%' || @keyword || '%' ESCAPE '`'))";
                sqlWhere = sqlWhere.Concat(new object[] { sqlCondition }).ToArray();
            }
            if (entity.UserId.HasValue)
            {
                sqlCondition = "  tmp.mailid IN (SELECT mailid FROM crm_sys_mail_senderreceivers WHERE   mailaddress IN (SELECT accountid FROM crm_sys_mail_mailbox WHERE owner::INT4=@queryuserid #mailaddress#) #ctype#) ";
                if (!string.IsNullOrEmpty(entity.MailAddress))
                {
                    sqlCondition = sqlCondition.Replace("#mailaddress#", "AND accountid=@mailaddress");
                }
                else
                {
                    sqlCondition = sqlCondition.Replace("#mailaddress#", string.Empty);
                }
                if (entity.Ctype == 0)
                {
                    sqlCondition = sqlCondition.Replace("#ctype#", " AND ctype!=@ctype");
                }
                else
                {
                    sqlCondition = sqlCondition.Replace("#ctype#", " AND ctype=@ctype");
                }
                sqlWhere = sqlWhere.Concat(new object[] { sqlCondition }).ToArray();
            }
            if (entity.DateRange == 0)
            {
                sqlCondition = "  tmp.mailtime>=(now()-interval '2 week') AND tmp.mailtime<=now()  ";
            }
            else if (entity.DateRange == 1)
            {
                sqlCondition = "  tmp.mailtime>=(now()-interval '3 month') AND tmp.mailtime<=now()  ";
            }
            else if (entity.DateRange == 2)
            {
                sqlCondition = "  tmp.mailtime>=(now()-interval '6 month') AND tmp.mailtime<=now()  ";
            }
            else if (entity.DateRange == 3)
            {
                sqlCondition = "  tmp.mailtime>=(now()-interval '1 y') AND tmp.mailtime<=now()  ";
            }
            else if (entity.DateRange == -1)
            {
                if (entity.StartDate.HasValue && entity.EndDate.HasValue)
                    sqlCondition = "  tmp.mailtime>=@startdate AND tmp.mailtime<=@enddate ";
                else if (entity.StartDate.HasValue)
                    sqlCondition = "  tmp.mailtime>=@startdate  ";
                else if (entity.StartDate.HasValue)
                    sqlCondition = "  tmp.mailtime<=@enddate  ";
            }
            sqlWhere = sqlWhere.Concat(new object[] { sqlCondition }).ToArray();
            sqlCondition = sqlWhere.Count() == 0 ? string.Empty : " AND " + string.Join(" AND ", sqlWhere);
            strSQL = string.Format(strSQL, sqlCondition);
            var param = new DbParameter[]
            {
                new NpgsqlParameter("keyword",entity.KeyWord),
                new NpgsqlParameter("mailaddress",entity.MailAddress),
                new NpgsqlParameter("startdate",entity.StartDate),
                new NpgsqlParameter("enddate",entity.EndDate),
                new NpgsqlParameter("queryuserid",entity.UserId.Value),
                new NpgsqlParameter("ctype",entity.Ctype)
            };
            return ExecuteQueryByPaging<MailTruncateLstMapper>(strSQL, param, entity.PageSize, entity.PageIndex);
        }


        public UserMailInfo GetEnablePassword(Guid mailBoxId, int userId)
        {
            string strSQL = @"SELECT * FROM crm_sys_mail_mailbox WHERE recstatus=1 AND recid=@mailboxid";
            var param = new
            {
                mailboxid = mailBoxId
            };
            return DataBaseHelper.QuerySingle<UserMailInfo>(strSQL, param);
        }

        public OperateResult DeleteMailDraft(Guid mailId, DbTransaction dbTrans, int userId)
        {
            var cataRelationSql = @" SELECT COUNT(1) FROM crm_sys_mail_catalog_relation WHERE catalogid IN ( SELECT recid FROM crm_sys_mail_catalog WHERE  ctype=1005 AND viewuserid=@userid) AND mailid=@mailid";
            var senderReceiversSql = @"DELETE FROM crm_sys_mail_senderreceivers WHERE mailid=@mailid";
            var relatedSql = @"DELETE FROM crm_sys_mail_related WHERE mailid=@mailid";
            var sendrecordSql = @"DELETE FROM crm_sys_mail_sendrecord WHERE mailid=@mailid";
            var attachSql = @"DELETE FROM crm_sys_mail_attach WHERE mailid=@mailid";
            var mailBodySql = @"DELETE FROM  crm_sys_mail_mailbody Where recid=@mailid;";
            var param = new
            {
                MailId = mailId,
                UserId = userId
            };
            try
            {
                var result = DataBaseHelper.QuerySingle<int>(cataRelationSql, param);
                if (result > 0)
                {
                    var dbParam = new DbParameter[] {
                    new NpgsqlParameter("mailid",mailId)
                };
                    DBHelper.ExecuteQuery(dbTrans, senderReceiversSql, dbParam);
                    DBHelper.ExecuteQuery(dbTrans, relatedSql, dbParam);
                    DBHelper.ExecuteQuery(dbTrans, sendrecordSql, dbParam);
                    DBHelper.ExecuteQuery(dbTrans, attachSql, dbParam);
                    DBHelper.ExecuteQuery(dbTrans, mailBodySql, dbParam);
                }
            }
            catch (Exception ex)
            {
                return new OperateResult
                {
                    Flag = 0,
                    Msg = "草稿箱邮件异常"
                };
            }
            return new OperateResult
            {
                Flag = 1,
                Msg = ""
            };
        }
    }
}
