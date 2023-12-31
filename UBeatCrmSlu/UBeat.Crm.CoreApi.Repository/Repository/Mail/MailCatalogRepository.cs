﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.EMail;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Utility;
using System.Data;
using Npgsql;
using System.Linq;
using Dapper;

namespace UBeat.Crm.CoreApi.Repository.Repository.Mail
{
    public class MailCatalogRepository : RepositoryBase, IMailCatalogRepository
    {
        public OperateResult InsertCatalog(CUMailCatalogMapper entity, int userId)
        {
            var sql = "select (COALESCE(max(recorder),0)+1) recorder from crm_sys_mail_catalog where vpid=@catalogpid LIMIT 1";
            var param = new
            {
                CatalogPId = entity.CatalogPId,
                CatalogName = entity.CatalogName.Trim(),
            };

            var existSql = @"select count(1) from  crm_sys_mail_catalog where vpid=@catalogpid and recstatus=1 and recname=@catalogname ";
            var existCount = DataBaseHelper.QuerySingle<int>(existSql, param, CommandType.Text);
            if (existCount > 0)
            {
                return new OperateResult()
                {
                    Flag = 0,
                    Msg = "文件夹名称已存在"
                };
            }
            var recOrder = DataBaseHelper.QuerySingle<int>(sql, param, CommandType.Text);
            sql = "INSERT INTO public.crm_sys_mail_catalog (recname, userid, viewuserid, ctype,CustCataLog,CustId, pid, vpid,recstatus,recorder,isdynamic) VALUES (@catalogname,@userid,@userid,@ctype,@CustCataLog,@CustId,@catalogpid,@catalogpid,1,@recorder,1)";
            var args = new
            {
                CatalogName = entity.CatalogName.Trim(),
                UserId = userId,
                Ctype = entity.Ctype,
                CustId = entity.CustId,
                CustCataLog = entity.CustCataLog,
                CatalogPId = entity.CatalogPId,
                Recorder = recOrder
            };

            var result = DataBaseHelper.ExecuteNonQuery(sql, args, CommandType.Text);
            if (result == 1)
            {
                return new OperateResult()
                {
                    Flag = 1,
                    Msg = "新增文件夹成功"
                };
            }
            else
            {
                return new OperateResult()
                {
                    Flag = 0,
                    Msg = "新增文件夹失败"
                };
            }
        }

        public OperateResult EditCatalog(CUMailCatalogMapper entity, int userId)
        {
            //var sql = "WITH RECURSIVE cata as" +
            //                "(" +
            //                "			SELECT  a.recid,a.vpid,a.ctype::TEXT as idpath,a.ctype,a.viewuserid FROM crm_sys_mail_catalog a   WHERE a.recid=@catalogid" +
            //                "			UNION ALL" +
            //                "			SELECT b.recid,b.vpid,cata.idpath||'>'||b.ctype::TEXT as idpath,b.ctype,b.viewuserid" +
            //                "      FROM crm_sys_mail_catalog b INNER JOIN cata  on cata.vpid= b.recid" +
            //                ") " +
            //                "SELECT COUNT(1) FROM cata WHERE ctype=2001 and viewuserid=@userid" +
            //                "UNION ALL" +
            //                "SELECT  count(1) FROM crm_sys_mail_catalog_relation WHERE catalogid IN (SELECT cata.recid FROM cata where viewuserid=@userid)";
            //var param = new
            //{
            //    CatalogId = entity.CatalogId,
            //    UserId = userId
            //};
            //var vaildResult = DataBaseHelper.Query<dynamic>(sql, param, CommandType.Text);
            //if (vaildResult[0].count < 0)
            //{
            //    return new OperateResult()
            //    {
            //        Flag = 0,
            //        Msg = "该文件夹不能移动到客户文件夹中"
            //    };
            //}
            //if (vaildResult[1].count > 0)
            //{
            //    return new OperateResult()
            //    {
            //        Flag = 0,
            //        Msg = "该文件夹下包含邮件,不能删除"
            //    };
            //}
            var param = new
            {
                CatalogPId = entity.CatalogPId,
                CatalogName = entity.CatalogName.Trim(),
            };

            var existSql = @"select count(1) from  crm_sys_mail_catalog where vpid=@catalogpid and recstatus=1 and recname=@catalogname ";
            var existCount = DataBaseHelper.QuerySingle<int>(existSql, param, CommandType.Text);
            if (existCount > 0)
            {
                return new OperateResult()
                {
                    Flag = 0,
                    Msg = "文件夹名称已存在"
                };
            }
            string sql = "UPDATE crm_sys_mail_catalog SET recname=@catalogname WHERE recid=@catalogid and viewuserid=@userid;";
            var args = new
            {
                CatalogId = entity.CatalogId,
                CatalogName = entity.CatalogName.Trim(),
                UserId = userId
            };

            var result = DataBaseHelper.ExecuteNonQuery(sql, args, CommandType.Text);
            if (result == 1)
            {
                return new OperateResult()
                {
                    Flag = 1,
                    Msg = "编辑文件夹成功"
                };
            }
            else
            {
                return new OperateResult()
                {
                    Flag = 0,
                    Msg = "编辑文件夹失败"
                };
            }
        }


        public OperateResult DeleteCatalog(DeleteMailCatalogMapper entity, int userId)
        {
            var sql = @"WITH tarcatalog as (
                select * from crm_sys_mail_catalog 
                where  recid=@catalogid and viewuserid=@userid and recstatus=1 LIMIT 1),
                childcatalog as(
                    select vpid,count(*) catalogCount from crm_sys_mail_catalog where vpid=@catalogid and viewuserid=@userid and recstatus=1 group by vpid
                ),
                childmail as(
                    select catalogid,count(*) mailCount from crm_sys_mail_catalog_relation where catalogid=@catalogid  group by catalogid)
                select COALESCE(tarcatalog.isdynamic,0) isdynamic,COALESCE(childcatalog.catalogCount,0)::int catalogCount,COALESCE(childmail.mailCount,0)::int mailCount from tarcatalog left join childcatalog on tarcatalog.recid=childcatalog.vpid left join childmail on childmail.catalogid=tarcatalog.recid";
            var param = new DbParameter[]
            {
                                new NpgsqlParameter("catalogid", entity.CatalogId),
                                new NpgsqlParameter("userid", userId)
            };
            List<Dictionary<string, object>> catalogList = ExecuteQuery(sql, param);
            Dictionary<string, object> catalogMap = new Dictionary<string, object>();
            if (catalogList.Count > 0)
                catalogMap = catalogList.FirstOrDefault();
            else
                return new OperateResult()
                {
                    Flag = 0,
                    Msg = "目录异常不能删除"
                };
            if ((int)catalogMap["isdynamic"] == 0)
            {
                return new OperateResult()
                {
                    Flag = 0,
                    Msg = "该文件夹是系统文件夹,不允许删除"
                };
            }
            if ((int)catalogMap["catalogcount"] > 0 || (int)catalogMap["mailcount"] > 0)
            {
                return new OperateResult()
                {
                    Flag = 0,
                    Msg = "有子文件夹或者邮件不允许删除"
                };
            }
            sql = "update  crm_sys_mail_catalog set recstatus=0 where   recid=@catalogid and viewuserid=@userid;";
            var args = new
            {
                CatalogId = entity.CatalogId,
                UserId = userId
            };
            var result = DataBaseHelper.ExecuteNonQuery(sql, args, CommandType.Text);
            if (result == 1)
            {
                return new OperateResult()
                {
                    Flag = 1,
                    Msg = "删除文件夹成功"
                };
            }
            else
            {
                return new OperateResult()
                {
                    Flag = 0,
                    Msg = "删除文件夹失败"
                };
            }
        }

        public OperateResult ToOrderCatalog(Guid recId, int doType)
        {
            string catalogSql = "select * from crm_sys_mail_catalog a where a.recstatus=1 " +
                "and a.vpid =(select vpid from crm_sys_mail_catalog where recid =@recId) order by a.recorder ";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("recid", recId)
            };
            List<MailCatalogInfo> catalogResult = ExecuteQuery<MailCatalogInfo>(catalogSql, param);

            MailCatalogInfo tarCatalog = null;
            MailCatalogInfo currCatalog = null;
            int index = 0;
            //doType 0 下移  doType  1 上移
            for (int i = 0; i < catalogResult.Count; i++)
            {
                if (catalogResult[i].RecId == recId)
                {
                    //找到当前移动对象
                    currCatalog = catalogResult[i];
                    index = i;
                }
            }
            if (doType == 0)
            {
                //目标目录
                int tarIndex = index + 1;
                if (catalogResult.Count > tarIndex)
                {
                    tarCatalog = catalogResult[tarIndex];
                }
                else
                {
                    return new OperateResult()
                    {
                        Flag = 0,
                        Msg = "无法下移目录"
                    };
                }
            }
            else
            {
                if (index > 0)
                {
                    int tarIndex = index - 1;
                    tarCatalog = catalogResult[tarIndex];
                }
                else
                {
                    return new OperateResult()
                    {
                        Flag = 0,
                        Msg = "无法上移目录"
                    };
                }
            }

            string newSql = "update crm_sys_mail_catalog set recorder=@recorder where  recid =@recId";
            var newParam = new DbParameter[]
            {
                new NpgsqlParameter("recorder", tarCatalog.RecOrder),
                new NpgsqlParameter("recid", recId)
            };
            ExecuteNonQuery(newSql, newParam);
            var oldParam = new DbParameter[]
            {
                new NpgsqlParameter("recorder", currCatalog.RecOrder),
                new NpgsqlParameter("recid", tarCatalog.RecId)
            };
            ExecuteNonQuery(newSql, oldParam);
            return new OperateResult()
            {
                Flag = 1,
                Msg = "移动成功"
            };
        }
        public OperateResult OrderbyCatalog(DbTransaction trans, OrderByMailCatalogMapper entity, int userId)
        {

            var sql = "select recorder from crm_sys_mail_catalog where recid=@catalogid and userid=@userid" +
                          "UINON ALL" +
                          "select recorder from crm_sys_mail_catalog where recid=@changecatalogid and userid=@userid";
            var param = new DbParameter[]
            {
                    new NpgsqlParameter("catalogid", entity.CatalogId),
                        new NpgsqlParameter("changecatalogid", entity.ChangeCatalogId),
                    new NpgsqlParameter("userid", userId)
            };
            var orderLst = DBHelper.ExecuteQuery<dynamic>(trans, sql, param, CommandType.Text);
            if (orderLst.Count != 2)
            {
                return new OperateResult()
                {
                    Flag = 0,
                    Msg = "文件夹不存在"
                };
            }
            sql = "update  crm_sys_mail_catalog set recorder=@recorder where   recid=@catalogid and viewuserid=@userid";
            foreach (var tmp in orderLst)
            {
                var args = new DbParameter[]
                 {
                        new NpgsqlParameter("catalogid", entity.CatalogId),
                        new NpgsqlParameter("recorder", tmp.recorder)
                 };
                var result = DBHelper.ExecuteQuery<int>(trans, sql, args).FirstOrDefault();
                if (result != 1)
                {
                    return new OperateResult()
                    {
                        Flag = 0,
                        Msg = "移动文件夹失败"
                    };
                }
            }
            return new OperateResult()
            {
                Flag = 1,
                Msg = "移动文件夹成功"
            };

        }

        public MailCatalogInfo GetMailCatalogByCode(int userId, string catalogType)
        {
            string sql = "select * from crm_sys_mail_catalog a where a.userid=@userId and ctype::text=@catalogType";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("userId", userId),
                new NpgsqlParameter("catalogType", catalogType)
            };
            List<MailCatalogInfo> catalogResult = ExecuteQuery<MailCatalogInfo>(sql, param);

            return catalogResult.FirstOrDefault();
        }

        public List<Dictionary<string, object>> GetDefaultCatalog(int userId)
        {
            string sql = "select a.*,x.defaultid,x.recid existid from crm_sys_mail_default_catalog a left join (select * from crm_sys_mail_catalog b where b.userid=@userId) x " +
                "on a.recid = x.defaultid where a.recstatus = 1 order by a.ctype";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("userId", userId)
            };
            List<Dictionary<string, object>> catalogResult = ExecuteQuery(sql, param);

            return catalogResult;
        }

        public int NeedInitCatalog(int userId)
        {
            string sql = "select a.*  from crm_sys_mail_default_catalog a " +
                "left join (select * from crm_sys_mail_catalog b where b.userid=@userId) x on a.recid = x.defaultid where a.recstatus = 1 and x.defaultid is null";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("userId", userId)
            };
            List<Dictionary<string, object>> catalogResult = ExecuteQuery(sql, param);

            return catalogResult.Count;
        }


        public int InitCatalog(Dictionary<string, object> newCatalog, int userId)
        {
            string sql = "insert into crm_sys_mail_catalog(recid,recname,userid,viewuserid,ctype,recstatus,pid,vpid,recorder,isdynamic,defaultid) " +
                "VALUES(@newId, @recname, @userid, @viewuserid, @ctype, 1, @pid, @vpid, @recorder, @isdynamic, @defaultid)";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("newId", newCatalog["recid"]),
                new NpgsqlParameter("recname", newCatalog["recname"]),
                new NpgsqlParameter("userId", userId),
                new NpgsqlParameter("viewuserid", userId),
                new NpgsqlParameter("ctype", newCatalog["ctype"]),
                new NpgsqlParameter("pid", newCatalog["pid"]),
                new NpgsqlParameter("vpid", newCatalog["vpid"]),
                new NpgsqlParameter("recorder", newCatalog["recorder"]),
                new NpgsqlParameter("isdynamic", newCatalog["isdynamic"]),
                new NpgsqlParameter("defaultid", newCatalog["defaultid"])
            };
            int catalogResult = ExecuteNonQuery(sql, param);

            return catalogResult;
        }
        public List<OrgAndStaffTree> GetOrgAndStaffTreeByLevel(int userId, string deptId, string keyword)
        {
            List<OrgAndStaffTree> resultList = new List<OrgAndStaffTree>();
            //判断是否领导用户
            string isLeaderSql = "select a.deptid from crm_sys_account_userinfo_relate a " +
                "inner join crm_sys_userinfo b on a.userid = b.userid " +
                "where a.recstatus=1 and b.isleader = 1 and b.recstatus = 1 and b.userid = @userid";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("UserId", userId),
            };
            var result = ExecuteQuery(isLeaderSql, param);
            //该用户是领导岗，获取下属邮件逻辑
            if (result.Count > 0)
            {
                string getOrgTreeSql = @"select * from (select * from (select deptid::text treeid,deptname treename,''::text deptname,''::text userjob,0 nodetype,0 unreadcount from crm_sys_department a 
	                where a.recstatus = 1 and a.pdeptid =@deptId order by recorder) t 
                        UNION ALL
                    select * from(
                    WITH mails as(select viewuserid,count(1) unreadcount from crm_sys_mail_catalog a inner join crm_sys_mail_catalog_relation b on a.recid=b.catalogid
                    inner join crm_sys_mail_mailbody c on c.recid=b.mailid where c.isread is null or c.isread = 0 group by viewuserid )
                    select b.userid::text treeid, b.username treename,a1.deptname,b.userjob,1 nodetype,COALESCE(mails.unreadcount,0)::int unreadcount from crm_sys_account_userinfo_relate a 
                        inner join crm_sys_userinfo b on a.userid = b.userid left join crm_sys_department a1 on a1.deptid=a.deptid left join mails on mails.viewuserid=b.userid  where 1=1 {0} and a.recstatus = 1 
                            and a.deptid = @deptId  order by b.username) t1 ) x where 1=1";
                string searchDept = result[0]["deptid"].ToString();
                string condition = string.Empty;
                if (string.IsNullOrEmpty(deptId))
                {
                    condition = string.Format(" and (b.isleader is null or b.isleader <> 1) ");
                }
                else {
                    if (deptId == searchDept) {
                        condition = string.Format(" and (b.isleader is null or b.isleader <> 1) ");
                    }
                    searchDept = deptId;
                }
                var paramTree = new DbParameter[]
                {
                    new NpgsqlParameter("deptId", new Guid(searchDept)),
                    new NpgsqlParameter("UserId", userId)
                };
                //只返回人员
                if (!string.IsNullOrEmpty(keyword))
                {
                    getOrgTreeSql = @"WITH mails as(select viewuserid,count(1) unreadcount from crm_sys_mail_catalog a inner join crm_sys_mail_catalog_relation b on a.recid=b.catalogid
                            inner join crm_sys_mail_mailbody c on c.recid=b.mailid where c.isread is null or c.isread = 0 group by viewuserid )
		                            select b.userid::text treeid, b.username treename,a1.deptname,b.userjob,1 nodetype,COALESCE(mails.unreadcount,0)::int unreadcount from crm_sys_account_userinfo_relate a 
	                             inner join crm_sys_userinfo b on a.userid = b.userid left join crm_sys_department a1 on a1.deptid=a.deptid left join mails on mails.viewuserid=b.userid  where 1=1 {0}  and a.recstatus = 1 
		                            and a.deptid in (SELECT deptid FROM crm_func_department_tree_power(@deptId,1,1,@userId)) ";
                    getOrgTreeSql = getOrgTreeSql + " and b.username like '%" + keyword + "%'  order by b.username ";

                }
                string newSql = string.Format(getOrgTreeSql,condition);
                resultList = ExecuteQuery<OrgAndStaffTree>(newSql, paramTree);
            }
            return resultList;
        }

        public List<MailCatalogInfo> GetMailCataLog(string catalogType, string vpid,string keyword, int userId)
        {
            var sql = @"WITH RECURSIVE cata as
            (
			            SELECT  a.recid,a.vpid,ARRAY[ctype]::text as idpath,a.ctype,a.viewuserid FROM crm_sys_mail_catalog a where vpid::text='00000000-0000-0000-0000-000000000000'  and recstatus=1  
			            UNION ALL
			            SELECT b.recid,b.vpid,cata.idpath || b.ctype,b.ctype,b.viewuserid
			            FROM crm_sys_mail_catalog b INNER JOIN cata  on cata.recid= b.vpid where  recstatus=1  
            ),
            usercatalog AS(
		             SELECT recid ,recname,userid,viewuserid,ctype,pid,vpid,recorder FROM crm_sys_mail_catalog WHERE viewuserid=@userId AND recstatus=1  
            ),
            catalogrelation AS(
				SELECT a.catalogid,COUNT(mailid) AS unreadmail,c.vpid FROM crm_sys_mail_catalog_relation a INNER JOIN crm_sys_mail_catalog c on c.recid=a.catalogid inner join crm_sys_mail_mailbody b on a.mailid = b.recid  where b.recstatus=1 and (b.isread is null or b.isread = 0) GROUP BY a.catalogid,c.vpid
            ),
            catalogmailcount AS(
		             SELECT a.catalogid,COUNT(mailid) AS mailcount,SUM(istag) flagstar FROM crm_sys_mail_catalog_relation a inner join crm_sys_mail_mailbody b on a.mailid = b.recid  where b.recstatus!=2  GROUP BY a.catalogid
            )
            SELECT usercatalog.*,
            COALESCE(CASE WHEN usercatalog.CType=1001 THEN (SELECT sum(unreadmail) FROM catalogrelation WHERE catalogrelation.catalogid IN (SELECT cata.recid FROM  cata WHERE POSITION('1001' IN idpath) >0 and viewuserid=@userId ))
            WHEN usercatalog.CType=2001 THEN (SELECT sum(unreadmail) FROM catalogrelation WHERE catalogrelation.catalogid IN (SELECT cata.recid FROM  cata WHERE POSITION('2001' IN idpath) >0 and viewuserid=@userId ))
            WHEN usercatalog.CType=3001 THEN (SELECT sum(unreadmail) FROM catalogrelation WHERE catalogrelation.catalogid IN (SELECT cata.recid FROM  cata WHERE POSITION('3001' IN idpath) >0 and viewuserid=@userId ) and catalogrelation.vpid=usercatalog.recid)
            WHEN usercatalog.CType=4001 THEN (SELECT sum(unreadmail) FROM catalogrelation WHERE catalogrelation.catalogid IN (SELECT cata.recid FROM  cata WHERE POSITION('4001' IN idpath) >0 and viewuserid=@userId ) and catalogrelation.catalogid=usercatalog.recid)
            WHEN usercatalog.CType=2002 THEN (SELECT sum(unreadmail) FROM catalogrelation WHERE catalogrelation.catalogid IN (SELECT cata.recid FROM  cata WHERE POSITION('2002' IN idpath) >0 and viewuserid=@userId ))
            WHEN usercatalog.CType=3002 THEN (SELECT sum(unreadmail) FROM catalogrelation WHERE catalogrelation.catalogid IN (SELECT cata.recid FROM  cata WHERE POSITION('3002' IN idpath) >0 and viewuserid=@userId ) and catalogrelation.catalogid=usercatalog.recid)
            WHEN usercatalog.CType=2003 THEN (SELECT sum(unreadmail) FROM catalogrelation WHERE catalogrelation.catalogid IN (SELECT cata.recid FROM  cata WHERE POSITION('2003' IN idpath) >0 and viewuserid=@userId ))
            WHEN usercatalog.CType=1009 THEN (SELECT sum(unreadmail) FROM catalogrelation WHERE catalogrelation.catalogid IN (SELECT cata.recid FROM  cata WHERE POSITION('1009' IN idpath) >0 and viewuserid=@userId ))
            WHEN usercatalog.CType=1002 THEN (SELECT sum(unreadmail) FROM catalogrelation WHERE catalogrelation.catalogid IN (SELECT cata.recid FROM  cata WHERE (POSITION('1001' IN idpath) >0 or POSITION('1004' IN idpath) >0 ) and viewuserid=@userId ))
						ELSE catalogrelation.unreadmail END,0)::int unreadcount,
						COALESCE(CASE WHEN usercatalog.CType=1008 THEN (SELECT sum(flagstar) FROM catalogmailcount WHERE catalogmailcount.catalogid IN (SELECT cata.recid FROM  cata WHERE (POSITION('1001' IN idpath) >0 or POSITION('1004' IN idpath) >0 ) and viewuserid=@userId ))       
					  WHEN usercatalog.CType=1004 THEN (SELECT sum(mailcount) FROM catalogmailcount WHERE catalogmailcount.catalogid IN (SELECT cata.recid FROM  cata WHERE POSITION('1004' IN idpath) >0 and viewuserid=@userId ))
						ELSE catalogmailcount.mailcount END, 0)::int mailcount,cata.idpath 
            FROM usercatalog LEFT JOIN catalogrelation ON usercatalog.recid=catalogrelation.catalogid 
            left join cata on cata.recid=usercatalog.recid left join catalogmailcount on catalogmailcount.catalogid=usercatalog.recid
             where 1=1 {0} order by vpid,recorder";
            string condition = string.Empty;
            if (!string.IsNullOrEmpty(catalogType))
            {
                condition = string.Format(" and POSITION ('{0}' IN cata.idpath) > 0 ", catalogType);

            }
            if (!string.IsNullOrEmpty(keyword))
            {
                condition = string.Format(condition + " and usercatalog.recname like '%{0}%' ", keyword);

            }
            if (!string.IsNullOrEmpty(vpid))
            {
                condition = string.Format(condition + " and usercatalog.vpid='{0}' ", vpid);

            }
            var param = new DbParameter[]
            {
                new NpgsqlParameter("UserId", userId)
            };
            string newSql = string.Format(sql, condition);
            var result = ExecuteQuery<MailCatalogInfo>(newSql, param);
            return result;
        }

        public MailCatalogInfo GetMailCataLogById(Guid catalog, int userNum, DbTransaction p)
        {
            var sql = "select * from crm_sys_mail_catalog where recid=@catalog ";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("catalog",  catalog )
            };
            var result = ExecuteQuery<MailCatalogInfo>(sql, param, p, CommandType.Text);
            return result.FirstOrDefault();
        }

        public MailCatalogInfo GetMailCataLogByViewUserId(Guid catalog, int userid, DbTransaction p)
        {
            var sql = "select * from crm_sys_mail_catalog where recid=@catalog and viewuserid=@userid";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("catalog",  catalog ),
                new NpgsqlParameter("userid",  userid )
            };
            var result = ExecuteQuery<MailCatalogInfo>(sql, param, p, CommandType.Text);
            return result.FirstOrDefault();
        }

        public MailCatalogInfo GetMailCataLogByCustId(Guid custId, int userid, DbTransaction p)
        {
            var sql = "select * from crm_sys_mail_catalog where ctype=4001 and recstatus=1 and custid=@custId and viewuserid=@userid";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("custId",  custId ),
                new NpgsqlParameter("userid",  userid )
            };
            var result = ExecuteQuery<MailCatalogInfo>(sql, param, p, CommandType.Text);
            return result.FirstOrDefault();
        }

        public List<MailCatalogInfo> GetMailCataLogTreeByUserId(int userid, string catalogType)
        {
            //会对收件箱 进行未读邮件进行统计
            var sql = @"WITH RECURSIVE cata AS ( SELECT a.recname,A.recid,A.vpid,A.ctype,A.viewuserid,ARRAY[ctype]::text as idpath
                FROM crm_sys_mail_catalog A WHERE recstatus = 1 {0}
                 UNION ALL SELECT  b.recname,b.recid,b.vpid,b.ctype,b.viewuserid,cata.idpath || b.ctype
                FROM crm_sys_mail_catalog b INNER JOIN cata ON cata.recid = b.vpid ), 
								catalogrelation AS(
										SELECT a.catalogid,COUNT(mailid) AS unreadmail,c.vpid FROM crm_sys_mail_catalog_relation a 
										INNER JOIN crm_sys_mail_catalog c on c.recid=a.catalogid inner join crm_sys_mail_mailbody b on a.mailid = b.recid  
                    where b.recstatus=1 and (b.isread is null or b.isread = 0) GROUP BY a.catalogid,c.vpid
								)
                       select cata.*,
									COALESCE(CASE WHEN cata.CType=1001 THEN (SELECT sum(unreadmail) FROM catalogrelation WHERE catalogrelation.catalogid IN (SELECT cata.recid FROM  cata WHERE POSITION('1001' IN idpath) >0 and viewuserid=@userid ))
									WHEN cata.CType=2001 THEN (SELECT sum(unreadmail) FROM catalogrelation WHERE catalogrelation.catalogid IN (SELECT cata.recid FROM  cata WHERE POSITION('2001' IN idpath) >0 and viewuserid=@userid ))
									WHEN cata.CType=3001 THEN (SELECT sum(unreadmail) FROM catalogrelation WHERE catalogrelation.catalogid IN (SELECT cata.recid FROM  cata WHERE POSITION('3001' IN idpath) >0 and viewuserid=@userid ) and catalogrelation.vpid=cata.recid)
									WHEN cata.CType=4001 THEN (SELECT sum(unreadmail) FROM catalogrelation WHERE catalogrelation.catalogid IN (SELECT cata.recid FROM  cata WHERE POSITION('4001' IN idpath) >0 and viewuserid=@userid ) and catalogrelation.catalogid=cata.recid)
									WHEN cata.CType=2002 THEN (SELECT sum(unreadmail) FROM catalogrelation WHERE catalogrelation.catalogid IN (SELECT cata.recid FROM  cata WHERE POSITION('2002' IN idpath) >0 and viewuserid=@userid ))
									WHEN cata.CType=3002 THEN (SELECT sum(unreadmail) FROM catalogrelation WHERE catalogrelation.catalogid IN (SELECT cata.recid FROM  cata WHERE POSITION('3002' IN idpath) >0 and viewuserid=@userid ) and catalogrelation.catalogid=cata.recid)
									WHEN cata.CType=2003 THEN (SELECT sum(unreadmail) FROM catalogrelation WHERE catalogrelation.catalogid IN (SELECT cata.recid FROM  cata WHERE POSITION('2003' IN idpath) >0 and viewuserid=@userid ))
									ELSE catalogrelation.unreadmail END,0)::int unreadcount 
								from cata left JOIN  catalogrelation on cata.recid=catalogrelation.catalogid  where cata.VIEWuserid = @userid";
            string condition = string.Empty;
            if (!string.IsNullOrEmpty(catalogType))
            {
                condition = string.Format(" and ctype={0} ", catalogType);

            }
            else
            {
                condition = string.Format(" and vpid :: TEXT = '00000000-0000-0000-0000-000000000000' ");
            }
            string newSql = string.Format(sql, condition);
            var param = new DbParameter[]
            {
                new NpgsqlParameter("userid",  userid )
            };
            return ExecuteQuery<MailCatalogInfo>(newSql, param);
        }

        public List<MailCatalogInfo> GetMailCataLogTreeByKeyword(string keyword, string catalogType, int userid)
        {
            //会对收件箱 进行未读邮件进行统计
            var sql = @"WITH RECURSIVE cata AS ( SELECT a.recname,A.recid,A.vpid,A.ctype,A.viewuserid,ARRAY[ctype]::text as idpath
                FROM crm_sys_mail_catalog A WHERE recstatus = 1 {0}
                 UNION ALL SELECT  b.recname,b.recid,b.vpid,b.ctype,b.viewuserid,cata.idpath || b.ctype
                FROM crm_sys_mail_catalog b INNER JOIN cata ON cata.recid = b.vpid ), 
								catalogrelation AS(
										SELECT a.catalogid,COUNT(mailid) AS unreadmail,c.vpid FROM crm_sys_mail_catalog_relation a 
										INNER JOIN crm_sys_mail_catalog c on c.recid=a.catalogid inner join crm_sys_mail_mailbody b on a.mailid = b.recid  
                    where b.recstatus=1 and (b.isread is null or b.isread = 0) GROUP BY a.catalogid,c.vpid
								)
                       select cata.*,
									COALESCE(CASE WHEN cata.CType=1001 THEN (SELECT sum(unreadmail) FROM catalogrelation WHERE catalogrelation.catalogid IN (SELECT cata.recid FROM  cata WHERE POSITION('1001' IN idpath) >0 and viewuserid=@userid ))
									WHEN cata.CType=2001 THEN (SELECT sum(unreadmail) FROM catalogrelation WHERE catalogrelation.catalogid IN (SELECT cata.recid FROM  cata WHERE POSITION('2001' IN idpath) >0 and viewuserid=@userid ))
									WHEN cata.CType=3001 THEN (SELECT sum(unreadmail) FROM catalogrelation WHERE catalogrelation.catalogid IN (SELECT cata.recid FROM  cata WHERE POSITION('3001' IN idpath) >0 and viewuserid=@userid ) and catalogrelation.vpid=cata.recid)
									WHEN cata.CType=4001 THEN (SELECT sum(unreadmail) FROM catalogrelation WHERE catalogrelation.catalogid IN (SELECT cata.recid FROM  cata WHERE POSITION('4001' IN idpath) >0 and viewuserid=@userid ) and catalogrelation.catalogid=cata.recid)
									WHEN cata.CType=2002 THEN (SELECT sum(unreadmail) FROM catalogrelation WHERE catalogrelation.catalogid IN (SELECT cata.recid FROM  cata WHERE POSITION('2002' IN idpath) >0 and viewuserid=@userid ))
									WHEN cata.CType=3002 THEN (SELECT sum(unreadmail) FROM catalogrelation WHERE catalogrelation.catalogid IN (SELECT cata.recid FROM  cata WHERE POSITION('3002' IN idpath) >0 and viewuserid=@userid ) and catalogrelation.catalogid=cata.recid)
									WHEN cata.CType=2003 THEN (SELECT sum(unreadmail) FROM catalogrelation WHERE catalogrelation.catalogid IN (SELECT cata.recid FROM  cata WHERE POSITION('2003' IN idpath) >0 and viewuserid=@userid ))
									ELSE catalogrelation.unreadmail END,0)::int unreadcount 
								from cata left JOIN  catalogrelation on cata.recid=catalogrelation.catalogid  where cata.VIEWuserid = @userid";
            string condition = string.Empty;
            if (!string.IsNullOrEmpty(catalogType))
            {
                condition = string.Format(" and ctype={0} ", catalogType);

            }
            else
            {
                condition = string.Format(" and vpid :: TEXT = '00000000-0000-0000-0000-000000000000' ");
            }
            sql = string.Format(sql, condition);
            if (!string.IsNullOrEmpty(keyword))
            {
                sql = string.Format(sql + " and recname like '%{0}%' ", keyword);

            }
            if (!string.IsNullOrEmpty(catalogType))
            {
                sql = string.Format(sql + " and ctype!={0} ", catalogType);
            }
            var param = new DbParameter[]
            {
                new NpgsqlParameter("userid",  userid )
            };
            var searchList = ExecuteQuery<MailCatalogInfo>(sql, param);
            var wholeTree = this.GetMailCataLogTreeByUserId(userid, catalogType);
            List<MailCatalogInfo> resultList = new List<MailCatalogInfo>();
            foreach (var catalog in searchList)
            {
                List<MailCatalogInfo> tempList = new List<MailCatalogInfo>();
                foreach (var tempItem in wholeTree)
                {
                    MailCatalogInfo info = new MailCatalogInfo();
                    info.RecName = tempItem.RecName;
                    info.RecId = tempItem.RecId;
                    info.VPId = tempItem.VPId;
                    info.CType = tempItem.CType;
                    info.ViewUserId = tempItem.ViewUserId;
                    info.UnReadCount = tempItem.UnReadCount;
                    tempList.Add(info);
                }
                foreach (var itemTreeOne in tempList)
                {
                    if (catalog.RecId == itemTreeOne.RecId)
                    {
                        resultList.Add(itemTreeOne);
                    }
                    foreach (var item in tempList)
                    {
                        if (item.VPId == itemTreeOne.RecId)
                        {
                            if (itemTreeOne.SubCatalogs == null)
                            {
                                itemTreeOne.SubCatalogs = new List<MailCatalogInfo>();
                            }
                            itemTreeOne.SubCatalogs.Add(item);
                        }
                    }
                }
            }

            return resultList;
        }


        public UserMailInfo GetUserMailInfo(string fromAddress, int userId)
        {
            var sql = " SELECT box.*,mailserver.imapaddress,mailserver.imapport,mailserver.smtpaddress,mailserver.smtpport,mailserver.enablessl,mailserver.wgvhhx,(select username from crm_sys_userinfo where userid=@userid::int4 limit 1) as displayname FROM( SELECT nickname,accountid, encryptpwd, (mailserver->> 'id')::uuid serverid,mailserver->> 'name' servername,owner FROM crm_sys_mail_mailbox Where recstatus=1) AS box  LEFT JOIN crm_sys_mail_server mailserver ON box.serverid = mailserver.recid Where box.owner=@userid ANd accountid=@fromaddress  ";
            string condition = string.Empty;
            var param = new
            {
                FromAddress = fromAddress,
                UserId = userId.ToString()
            };
            return DataBaseHelper.QuerySingle<UserMailInfo>(string.Format(sql, condition), param, CommandType.Text);
        }
        public IList<UserMailInfo> GetAllUserMail(int deviceType, int userId)
        {
            var sql = " SELECT box.*,mailserver.imapaddress,mailserver.imapport,mailserver.smtpaddress,mailserver.smtpport,mailserver.enablessl FROM( SELECT accountid, encryptpwd, (mailserver->> 'id')::uuid serverid,mailserver->> 'name' servername,owner,nickname FROM crm_sys_mail_mailbox  Where  recstatus=1) AS box  INNER JOIN crm_sys_mail_server mailserver ON box.serverid = mailserver.recid AND mailserver.recstatus=1  {0}";

            string condition = string.Empty;
            var param = new DynamicParameters();
            if (deviceType != 3)
            {
                condition = " Where box.owner::int4=@userid";
                param.Add("userid", userId);
            }
            return DataBaseHelper.Query<UserMailInfo>(string.Format(sql, condition), param, CommandType.Text);
        }

        /// <summary>
        /// 计算该目录是否有邮件
        /// </summary>
        /// <param name="recid"></param>
        /// <param name="tran"></param>
        /// <returns></returns>
        public bool checkHasMails(string recid, DbTransaction tran)
        {
            string strSQL = string.Format("select COALESCE(count(*),0)  totalcount  from crm_sys_mail_catalog_relation where catalogid = '{0}'", recid);
            List<Dictionary<string, object>> list = ExecuteQuery(strSQL, new DbParameter[] { }, tran);
            object obj = null;
            if (list.Count > 0)
                obj = list.FirstOrDefault()["totalcount"];
            if (obj == null) return false;
            int totalCount = 0;
            int.TryParse(obj.ToString(), out totalCount);
            if (totalCount > 0) return true;
            return false;
        }
        /// <summary>
        /// 检查两个目录是否会产生循环
        /// </summary>
        /// <param name="parentid"></param>
        /// <param name="recid"></param>
        /// <param name="tran"></param>
        /// <returns></returns>

        public bool checkCycleCatalog(string parentid, string recid, DbTransaction tran)
        {
            if (parentid == recid) return true;
            string thisparentid = parentid;
            int totalCycle = 10;//最大不超过10及
            for (int i = 0; i < totalCycle; i++)
            {
                string strSQL = string.Format("Select pid from crm_sys_mail_catalog where recid = '{0}'", thisparentid);
                object obj = ExecuteQuery(strSQL, new DbParameter[] { }, tran).FirstOrDefault()["pid"];
                if (obj == null) return false;
                thisparentid = obj.ToString();
                if (thisparentid == Guid.Empty.ToString()) return false;

                if (thisparentid == recid) return true;
            }
            return false;
        }
        /// <summary>
        /// 移动目录
        /// </summary>
        /// <param name="recid"></param>
        /// <param name="parentid"></param>
        /// <param name="tran"></param>

        public void MoveCatalog(string recid, string parentid, string recname, DbTransaction tran)
        {
            string strSQL = string.Format("update crm_sys_mail_catalog set recname='{2}',pid='{0}',vpid='{0}' where recid = '{1}'", parentid, recid, recname);
            ExecuteNonQuery(strSQL, new DbParameter[] { }, tran);
        }

        /// <summary>
        /// 根据客户类型获取与这个客户类型相关的邮件目录
        /// </summary>
        /// <param name="custCatalog"></param>
        /// <param name="newUserId"></param>
        /// <param name="tran"></param>
        /// <returns></returns>
        public MailCatalogInfo GetCatalogForCustType(Guid custCatalog, int userid, DbTransaction tran)
        {
            int ctype = (int)MailCatalogType.CustType;
            string strSQL = string.Format("select * from crm_sys_mail_catalog where ctype = {0} and userid = {1} and custcatalog = '{2}'",
                ctype, userid, custCatalog.ToString());
            return ExecuteQuery<MailCatalogInfo>(strSQL, new DbParameter[] { }, tran).FirstOrDefault();
        }


        /// <summary>
        /// 转移邮件目录
        /// </summary>
        /// <param name="recId"></param>
        /// <param name="newUserId"></param>
        /// <param name="newParentCatalogid"></param>
        /// <param name="tran"></param>
        public void TransferCatalog(Guid recId, int newUserId, Guid newParentCatalogid, DbTransaction tran)
        {
            string strSQL = string.Format("update crm_sys_mail_catalog set vpid='{0}',viewuserid={1} where recid ='{2}'", newParentCatalogid, newUserId, recId.ToString());
            ExecuteNonQuery(strSQL, new DbParameter[] { }, tran);
        }

        /// <summary>
        /// 转移邮件到新目录
        /// </summary>
        /// <param name="newCatalogid"></param>
        /// <param name="oldCatalogid"></param>
        public void TransferMailsToNewCatalog(Guid newCatalogid, Guid oldCatalogid, DbTransaction tran)
        {
            string strSQL = @"update crm_sys_mail_catalog_relation set catalogid=@newCatalogid where catalogid = @oldCatalogid";
            var param = new DbParameter[]
            {
                            new NpgsqlParameter("newCatalogid", newCatalogid),
                            new NpgsqlParameter("oldCatalogid", oldCatalogid)
            };
            ExecuteNonQuery(strSQL, param, tran);
        }

        /// <summary>
        /// 根据父级目录批量转移邮件目录
        /// </summary>
        /// <param name="recId"></param>
        /// <param name="newUserId"></param>
        /// <param name="newParentCatalogid"></param>
        /// <param name="tran"></param>
        public void TransferBatcCatalog(int newUserId, int oldUserId, Guid newParentCatalogid, int ctype, DbTransaction tran)
        {
            string strSQL = @"update crm_sys_mail_catalog set viewuserid=@newUserId where recid in (
                WITH RECURSIVE cata AS ( SELECT a.recname,A.recid,A.vpid,A.ctype,A.viewuserid 
                               FROM crm_sys_mail_catalog A WHERE recstatus = 1 and ctype=@ctype
                                 UNION ALL SELECT  b.recname,b.recid,b.vpid,b.ctype,b.viewuserid 
                                FROM crm_sys_mail_catalog b INNER JOIN cata ON cata.recid = b.vpid where b.recstatus=1 ) 
                               select recid from cata where VIEWuserid = @oldUserId and (vpid=@newParentCatalogid or recid=@newParentCatalogid ) )";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("newUserId", newUserId),
                new NpgsqlParameter("oldUserId", oldUserId),
                new NpgsqlParameter("newParentCatalogid", newParentCatalogid),
                new NpgsqlParameter("ctype", ctype)
            };
            ExecuteNonQuery(strSQL, param, tran);
        }

        /// <summary>
        /// 指定邮件拥有人
        /// </summary>
        /// <param name="MailBoxs"></param>
        /// <param name="newUserId"></param>
        public OperateResult SaveMailOwner(List<Guid> MailBoxs, int newUserId, int userId, DbTransaction dbTrans = null)
        {
            string userMailAddressSql = " select accountid as mailaddress,owner::int4 as userid from crm_sys_mail_mailbox where recid IN (SELECT regexp_split_to_table(@recids,',')::uuid)";
            var userMailAddressLst = DataBaseHelper.Query<TransferMailAddressMapper>(userMailAddressSql, new { RecIds = string.Join(",", MailBoxs) });
            var result = TransferMailToOtherMailAddress(userMailAddressLst, newUserId, dbTrans);//邮件转移
            if (result.Flag == 0)
            {
                dbTrans.Rollback();
                return result;
            }

            string strSQL = "update crm_sys_mail_mailbox set owner=@userid where recid=@recid";
            List<DbParameter[]> paramList = new List<DbParameter[]>();
            foreach (var recid in MailBoxs)
            {
                var param = new DbParameter[]
                {
                    new NpgsqlParameter("userid", newUserId),
                    new NpgsqlParameter("recid", recid)
                };
                paramList.Add(param);
            }
            ExecuteNonQueryMultiple(strSQL, paramList, dbTrans);
            return result;
        }

        public OperateResult TransferMailToOtherMailAddress(List<TransferMailAddressMapper> entities, int userId, DbTransaction dbTrans = null)
        {
            var sqlPro = "SELECT * FROM crm_func_mail_cata_related_handle(@mailid, null,@extradata::json, '60ec5c79-dfe2-4c11-aaf8-51177e921c5d',@userid)";

            var sql = "WITH RECURSIVE T1 AS" +
                            "(" +
                            "    SELECT recname,ctype,recid,viewuserid,vpid FROM crm_sys_mail_catalog WHERE viewuserid=@userid AND (ctype=1001 OR ctype=1004 OR ctype=1005 OR ctype=1006)" +
                            "    UNION " +
                            "    SELECT cata.recname,cata.ctype,cata.recid,cata.viewuserid,cata.vpid FROM crm_sys_mail_catalog cata INNER JOIN  T1 ON T1.recid=cata.vpid AND " +
                            "   cata.viewuserid=@userid" +
                            ")" +
                            "SELECT tmp.mailid,'{\"issendoreceive\":'||tmp.mailoperatetype::TEXT||',\"sendrecord\":{\"status\":'||COALESCE(record.status,-1)||'}}' as extradata FROM (SELECT * FROM crm_sys_mail_related related WHERE relatedmailaddress=@mailaddress AND mailid IN " +
                            "(" +
                            " SELECT mailid FROM crm_sys_mail_catalog_relation WHERE catalogid IN (  SELECT recid FROM T1 )" +
                            ") ) AS tmp LEFT JOIN crm_sys_mail_sendrecord record ON record.mailid=tmp.mailid" +
                            "";
            var delMailCataReSql = @"WITH RECURSIVE T1 AS 
				            ( 
						            SELECT recname,ctype,recid,viewuserid,vpid FROM crm_sys_mail_catalog WHERE viewuserid=@userid AND (ctype=1001 OR ctype=1004 OR ctype=1005 OR ctype=1006)   AND recstatus=1
						            UNION  
						            SELECT cata.recname,cata.ctype,cata.recid,cata.viewuserid,cata.vpid FROM crm_sys_mail_catalog cata INNER JOIN  T1 ON T1.recid=cata.vpid AND   cata.viewuserid=@userid    AND recstatus=1
				            )
                            DELETE FROM crm_sys_mail_catalog_relation WHERE mailid IN 
                            (
                            SELECT  mailid FROM crm_sys_mail_related related WHERE relatedmailaddress=@mailaddress
                            )      AND catalogid IN (SELECT recid FROM T1)  ";

            var delMailCataSql = @"WITH RECURSIVE T1 AS 
				            ( 
						            SELECT recname,ctype,recid,viewuserid,vpid FROM crm_sys_mail_catalog WHERE viewuserid=@userid AND (ctype=4001 OR ctype=3002)    AND recstatus=1
						            UNION  
						            SELECT cata.recname,cata.ctype,cata.recid,cata.viewuserid,cata.vpid FROM crm_sys_mail_catalog cata INNER JOIN  T1 ON T1.recid=cata.vpid AND   cata.viewuserid=@userid  AND recstatus=1  WHERE(cata.ctype=4001 OR cata.ctype=3002) 
				            )
                           DELETE FROM crm_sys_mail_catalog WHERE recid IN (SELECT T1.recid FROM T1 LEFT JOIN crm_sys_mail_catalog_relation re ON T1.recid=re.catalogid WHERE re.catalogid IS NULL)";//只删除客户目录  个人目录下面的
            var delCategoryCataLogSql = @"WITH RECURSIVE T1 AS 
				            ( 
						            SELECT recname,ctype,recid,viewuserid,vpid FROM crm_sys_mail_catalog WHERE viewuserid=@userid AND  ctype=3001    AND recstatus=1
						            UNION  
						            SELECT cata.recname,cata.ctype,cata.recid,cata.viewuserid,cata.vpid FROM crm_sys_mail_catalog cata INNER JOIN  T1 ON T1.recid=cata.vpid AND   cata.viewuserid=@userid  AND recstatus=1 WHERE  cata.ctype=3001
				            )
                           DELETE FROM crm_sys_mail_catalog WHERE recid IN (SELECT T1.recid FROM T1 LEFT JOIN crm_sys_mail_catalog cata ON cata.vpid=T1.recid WHERE cata.recid IS NULL)";//删除客户分类
            var mailRecSql = @"UPDATE crm_sys_mail_receivemailrelated SET userid=@newuserid WHERE userid=@userid AND mailaddress=@mailaddress";
            OperateResult result = new OperateResult { Flag = 1 };
            foreach (var entity in entities)
            {
                var param = new
                {
                    MailAddress = entity.MailAddress,
                    UserId = entity.UserId
                };
                var mailsResult = DataBaseHelper.Query<TransferMailAddressInfo>(sql, param);

                foreach (var tmp in mailsResult)
                {
                    var args = new DbParameter[]
                    {
                        new NpgsqlParameter("mailid",tmp.MailId),
                        new NpgsqlParameter("extradata",tmp.ExtraData),
                        new NpgsqlParameter("userid",userId)
                    };
                    result = DBHelper.ExecuteQuery<OperateResult>(dbTrans, sqlPro, args).FirstOrDefault();
                }
                var args1 = new DbParameter[]
                {
                        new NpgsqlParameter("mailaddress",entity.MailAddress),
                        new NpgsqlParameter("userid",entity.UserId)
                };
                DBHelper.ExecuteQuery(dbTrans, delMailCataReSql, args1);
                DBHelper.ExecuteQuery(dbTrans, delMailCataSql, args1);
                DBHelper.ExecuteQuery(dbTrans, delCategoryCataLogSql, args1);
                var args2 = new DbParameter[]
                {
                        new NpgsqlParameter("mailaddress",entity.MailAddress),
                        new NpgsqlParameter("userid",entity.UserId),
                        new NpgsqlParameter("newuserid",userId)
                };
                DBHelper.ExecuteQuery(dbTrans, mailRecSql, args2);
            }

            return result;
        }

        /// <summary>
        /// 批量设置白名单
        /// </summary>
        /// <param name="MailBoxs"></param>
        /// <param name="enable"></param>
        public void SaveWhiteList(List<Guid> MailBoxs, string enable)
        {
            string strSQL = "update crm_sys_mail_mailbox set inwhitelist=@enable where recid=@recid";
            List<DbParameter[]> paramList = new List<DbParameter[]>();
            foreach (var recid in MailBoxs)
            {
                var param = new DbParameter[]
                {
                    new NpgsqlParameter("enable", enable),
                    new NpgsqlParameter("recid", recid)
                };
                paramList.Add(param);
            }

            ExecuteNonQueryMultiple(strSQL, paramList);
        }

        public void MailServerEnable(List<Guid> MailServers)
        {
            string strSQL = "update crm_sys_mail_server set recstatus=1 where recid=@recid";
            List<DbParameter[]> paramList = new List<DbParameter[]>();
            foreach (var recid in MailServers)
            {
                var param = new DbParameter[]
                {
                    new NpgsqlParameter("recid", recid)
                };
                paramList.Add(param);
            }

            ExecuteNonQueryMultiple(strSQL, paramList);
        }

        public void MailServerUnEnable(List<Guid> MailServers)
        {
            string strSQL = "update crm_sys_mail_server set recstatus=0 where recid=@recid";
            List<DbParameter[]> paramList = new List<DbParameter[]>();
            foreach (var recid in MailServers)
            {
                var param = new DbParameter[]
                {
                    new NpgsqlParameter("recid", recid)
                };
                paramList.Add(param);
            }

            ExecuteNonQueryMultiple(strSQL, paramList);
        }
    }
}
