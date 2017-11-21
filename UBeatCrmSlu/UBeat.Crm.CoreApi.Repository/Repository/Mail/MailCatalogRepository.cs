using System;
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
            var sql = "select (COALESCE(max(recorder),0)+1) recorder from crm_sys_mail_catalog where pid=@catalogpid LIMIT 1";
            var param = new
            {
                CatalogPId = entity.CatalogPId
            };
            var recOrder = DataBaseHelper.QuerySingle<int>(sql, param, CommandType.Text);
            sql = "INSERT INTO public.crm_sys_mail_catalog (recname, userid, viewuserid, ctype, pid, vpid, recorder,isdynamic) VALUES (@catalogname,@userid,@userid,@ctype,@catalogpid,@catalogpid,@recorder,1)";
            var args = new
            {
                CatalogName = entity.CatalogName,
                UserId = userId,
                Ctype = entity.Ctype,
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
            string sql = "UPDATE crm_sys_mail_catalog SET recname=@catalogname WHERE recid=@catalogid and viewuserid=@userid;";
            var args = new
            {
                CatalogId = entity.CatalogId,
                CatalogName = entity.CatalogName,
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
            var sql = "select isdynamic from crm_sys_mail_catalog where  recid=@catalogid and viewuserid=@userid and recstatus=1 limit 1;";
            var args = new
            {
                CatalogId = entity.CatalogId,
                UserId = userId
            };
            var isdynamic = DataBaseHelper.QuerySingle<int?>(sql, args, CommandType.Text);
            if (!isdynamic.HasValue)
            {
                return new OperateResult()
                {
                    Flag = 0,
                    Msg = "该文件夹不存在"
                };
            }
            if (isdynamic == 0)
            {
                return new OperateResult()
                {
                    Flag = 0,
                    Msg = "该文件夹是系统文件夹,不允许删除"
                };
            }
            sql = "update  crm_sys_mail_catalog set recstatus=0 where   recid=@catalogid and viewuserid=@userid;";
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

        public OperateResult ToOrderCatalog(Guid recId,int doType) {
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
            for (int i=0;i< catalogResult.Count;i++) {
                if (catalogResult[i].RecId == recId)
                {
                    //找到当前移动对象
                    currCatalog = catalogResult[i];
                    index = i;
                }
            }
            if (doType == 0) {
                //目标目录
                int tarIndex = index + 1;
                if (catalogResult.Count >= tarIndex)
                {
                    tarCatalog = catalogResult[tarIndex];
                }
                else {
                    return new OperateResult()
                    {
                        Flag = 0,
                        Msg = "无法下移目录"
                    };
                }
            }
            else{
                if (index > 0)
                {
                    int tarIndex = index - 1;
                    tarCatalog = catalogResult[tarIndex];
                }
                else {
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
                "on a.recid = x.defaultid where a.recstatus = 1";
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
        public List<OrgAndStaffTree> GetOrgAndStaffTreeByLevel(int userId, string deptId,string keyword)
        {
            List<OrgAndStaffTree> resultList = new List<OrgAndStaffTree>();
            //判断是否领导用户
            string isLeaderSql = "select a.deptid from crm_sys_account_userinfo_relate a " +
                "inner join crm_sys_userinfo b on a.userid = b.userid " +
                "where b.isleader = 1 and b.recstatus = 1 and b.userid = @userid";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("UserId", userId),
            };
            var result = ExecuteQuery(isLeaderSql, param);
            //该用户是领导岗，获取下属邮件逻辑
            if (result.Count > 0)
            {
                string getOrgTreeSql = "select * from (select * from (select deptid::text treeid,deptname treename,''::text deptname,''::text userjob,0 nodetype,0 unreadcount from crm_sys_department a " +
                    "where a.recstatus = 1 and a.pdeptid::text =@deptId order by recorder) t " +
                    "UNION ALL" +
                    " select * from(select b.userid::text treeid, b.username treename,a1.deptname,b.userjob,1 nodetype,0 unreadcount from crm_sys_account_userinfo_relate a " +
                    "inner join crm_sys_userinfo b on a.userid = b.userid left join crm_sys_department a1 on a1.deptid=a.deptid  where(b.isleader is null or b.isleader <> 1) and a.recstatus = 1 " +
                    "and a.deptid::text = @deptId order by b.username) t1 ) x where 1=1 ";
                string searchDept = result[0]["deptid"].ToString();
                if (!string.IsNullOrEmpty(deptId))
                {
                    searchDept = deptId;
                }
                var paramTree = new DbParameter[]
                {
                    new NpgsqlParameter("deptId", searchDept),
                };
                //只返回人员
                if (!string.IsNullOrEmpty(keyword))
                {
                    getOrgTreeSql = string.Format(getOrgTreeSql+ " and x.nodetype=1 and x.treename like '%{0}%' ", keyword);

                }
                resultList = ExecuteQuery<OrgAndStaffTree>(getOrgTreeSql, paramTree);
            }
            return resultList;
        }

        public List<MailCatalogInfo> GetMailCataLog(string catalogType,string keyword, int userId)
        {
            var sql = "WITH RECURSIVE cata as" +
                    "(" +
                    "			SELECT  a.recid,a.vpid,ARRAY[ctype]::text as idpath,a.ctype,a.viewuserid FROM crm_sys_mail_catalog a where vpid::text='00000000-0000-0000-0000-000000000000' " +
                    "			UNION ALL" +
                    "			SELECT b.recid,b.vpid,cata.idpath || b.ctype,b.ctype,b.viewuserid" +
                    "      FROM crm_sys_mail_catalog b INNER JOIN cata  on cata.recid= b.vpid" +
                    ")," +
                    "usercatalog AS(" +
                    "     SELECT recid ,recname,userid,viewuserid,ctype,pid,vpid,recorder FROM crm_sys_mail_catalog WHERE viewuserid=@userid AND recstatus=1  " +
                    ")," +
                    "catalogrelation AS(" +
                    "     SELECT a.catalogid,COUNT(mailid) AS unreadmail FROM crm_sys_mail_catalog_relation a inner join crm_sys_mail_mailbody b on a.mailid = b.recid  where(b.isread is null or b.isread = 0) GROUP BY a.catalogid" +
                    ")" +
                    "SELECT usercatalog.*," +
                    "COALESCE(CASE WHEN usercatalog.CType=1001 THEN (SELECT sum(unreadmail) FROM catalogrelation WHERE catalogrelation.catalogid IN (SELECT cata.recid FROM  cata WHERE POSITION('1001' IN idpath) >0))" +
                    "WHEN usercatalog.CType=1002 THEN (SELECT sum(unreadmail) FROM catalogrelation WHERE catalogrelation.catalogid IN (SELECT cata.recid FROM  cata WHERE POSITION('1002' IN idpath) >0))" +
                    "WHEN usercatalog.CType=2001 THEN (SELECT sum(unreadmail) FROM catalogrelation WHERE catalogrelation.catalogid IN (SELECT cata.recid FROM  cata WHERE POSITION('2001' IN idpath) >0))" +
                    "ELSE catalogrelation.unreadmail END,0) unreadmail,cata.idpath " +
                    "FROM usercatalog LEFT JOIN catalogrelation ON usercatalog.recid=catalogrelation.catalogid " +
                    "left join cata on cata.recid=usercatalog.recid where 1=1 {0} order by vpid,recorder";
            string condition = string.Empty;
            if (!string.IsNullOrEmpty(catalogType))
            {
                condition = string.Format(" and POSITION ('{0}' IN cata.idpath) > 0 ", catalogType);

            }
            if (!string.IsNullOrEmpty(keyword))
            {
                condition = string.Format(condition+" and usercatalog.recname like '%{0}%' ", keyword);

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
            var sql = "select * from crm_sys_mail_catalog where recid=@catalog and userid=@userid";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("catalog",  catalog ),
                new NpgsqlParameter("userid",  userNum )
            };
            var result = ExecuteQuery<MailCatalogInfo>(sql, param, p, CommandType.Text);
            return result.FirstOrDefault();
        }


        public UserMailInfo GetUserMailInfo(string fromAddress, int userId)
        {
            var sql = " SELECT box.*,mailserver.imapaddress,mailserver.imapport,mailserver.smtpaddress,mailserver.smtpport FROM( SELECT accountid, encryptpwd, (mailserver->> 'id')::uuid serverid,mailserver->> 'name' servername,owner FROM crm_sys_mail_mailbox Where recstatus=1) AS box  LEFT JOIN crm_sys_mail_server mailserver ON box.serverid = mailserver.recid Where box.owner=@userid ANd accountid=@fromaddress  ";
            string condition = string.Empty;
            var param = new
            {
                FromAddress = fromAddress,
                UserId = userId.ToString()
            };
            return DataBaseHelper.QuerySingle<UserMailInfo>(string.Format(sql, condition), param, CommandType.Text);
        }
        public IList<UserMailInfo> GetAllUserMail(bool isDevice, int userId)
        {
            var sql = " SELECT box.*,mailserver.imapaddress,mailserver.imapport,mailserver.smtpaddress,mailserver.smtpport FROM( SELECT accountid, encryptpwd, (mailserver->> 'id')::uuid serverid,mailserver->> 'name' servername,owner FROM crm_sys_mail_mailbox  Where  recstatus=1) AS box  LEFT JOIN crm_sys_mail_server mailserver ON box.serverid = mailserver.recid {0}";
            string condition = string.Empty;
            var param = new DynamicParameters();
            if (!isDevice)
            {
                condition = " Where box.owner=@userid";
                param.Add("userid", userId.ToString());
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
            string strSQL = string.Format("select count(*)  totalcount  from crm_sys_mail_catalog_relation where catalogid = '{0}'", recid);
            object obj = ExecuteQuery(strSQL, new DbParameter[] { }, tran).FirstOrDefault()["totalcouont"];
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
            for (int i = 0; i < totalCycle; i++) {
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

        public void MoveCatalog(string recid, string parentid, DbTransaction tran)
        {
            string strSQL = string.Format("update crm_sys_mail_catalog set pid='{0}',vpid='{0}' where recid = '{1}'", parentid, recid);
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
            string strSQL = string.Format("select * from crm_sys_mail_catalog where ctype = {0} and userid = {1} and custcatalog = '{2}'",
                MailCatalogType.Cust, userid, custCatalog.ToString());
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
        /// 指定邮件拥有人
        /// </summary>
        /// <param name="MailBoxs"></param>
        /// <param name="newUserId"></param>
        public void SaveMailOwner(List<Guid> MailBoxs, int newUserId)
        {
            string strSQL = "update crm_sys_mail_mailbox set owner=@userid where recid=@recid";
            List<DbParameter[]> paramList = new List<DbParameter[]>();
            foreach (var recid in MailBoxs) {
                var param = new DbParameter[]
                {
                    new NpgsqlParameter("userid", newUserId),
                    new NpgsqlParameter("recid", recid)
                };
                paramList.Add(param);
            }

            ExecuteNonQueryMultiple(strSQL, paramList);
        }

        /// <summary>
        /// 批量设置白名单
        /// </summary>
        /// <param name="MailBoxs"></param>
        /// <param name="enable"></param>
        public void SaveWhiteList(List<Guid> MailBoxs, int enable)
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
    }
}
