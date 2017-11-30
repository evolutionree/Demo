using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.IRepository;
using System.Linq;
using UBeat.Crm.CoreApi.DomainModel.DbManage;

namespace UBeat.Crm.CoreApi.Repository.Repository.DbManage
{
    public class DbManageRepository : RepositoryBase, IDbManageRepository
    {
        public void deleteSQLObject(string id, int userid, DbTransaction tran)
        {
            try
            {
                string strSQL = string.Format("delete from crm_sys_dbmgr_object where id='{0}'", id);
                ExecuteNonQuery(strSQL, new DbParameter[] { }, tran);
            }
            catch (Exception ex) {

            }
        }

        public void deleteSQLText(string id, int userid, DbTransaction tran)
        {
            try
            {
                string strSQL = string.Format("delete from crm_sys_dbmgr_sql where id='{0}'", id);
                ExecuteNonQuery(strSQL, new DbParameter[] { }, tran);
            }
            catch (Exception ex)
            {

            }
        }

        public void deleteSQLTextByObjId(string objid, InitOrUpdate initorupdate, StructOrData structordata, int userid, DbTransaction tran)
        {
            try
            {
                string strSQL = string.Format(@"delete from crm_sys_dbmgr_sql where sqlobjid='{0}' and (initorupdate = {1} or {1}=0)
                                                and(structordata ={2} or {2} = 0 )", objid,(int)initorupdate, (int)structordata);
                ExecuteNonQuery(strSQL, new DbParameter[] { }, tran);
            }
            catch (Exception ex)
            {

            }
        }

        public List<Dictionary<string, object>> getConstraints(string tablename, int userid, DbTransaction tran)
        {
            try
            {
                string strSQL = string.Format(@"select  pg_get_constraintdef(a.oid) definedtext,a.*
                                    from pg_constraint a 
		                                    inner join pg_class b on a.conrelid = b.oid 
                                    where b.relname= '{0}'", tablename);
                return ExecuteQuery(strSQL, new DbParameter[] { }, tran);
            }
            catch (Exception ex) {
                return null;
            }
        }

        public List<Dictionary<string, object>> getFieldList(string tablename, int userid, DbTransaction tran)
        {
            try
            {
                string strSQL = string.Format(@"SELECT A.attnum,
	                                            col_description (A .attrelid, A .attnum) AS COMMENT,
	                                            format_type (A .atttypid, A .atttypmod) AS TYPE,
	                                            A .attname AS NAME,
	                                            A .attnotnull AS NOTNULL,de.adsrc,attlen
                                            FROM
	                                            pg_class AS C inner join  pg_attribute AS A on A .attrelid = C .oid
	                                            left outer join pg_attrdef as de  on de.adrelid = a.attrelid  and de.adnum = a.attnum
                                            WHERE
	                                            C .relname = '{0}'
                                            AND A .attnum > 0", tablename);
                return ExecuteQuery(strSQL, new DbParameter[] { }, tran);
            }
            catch (Exception ex) {
                return null;
            }
        }

        public List<Dictionary<string, object>> getIndexes(string tablename, int userid, DbTransaction tran)
        {
            try
            {
                string strSQL = string.Format(@"select b.relname ,c.relname,pg_get_indexdef(a.indexrelid) sqltext,a.*
                                            from pg_index a 
		                                            inner join pg_class b on a.indrelid = b.oid 
		                                            inner join pg_class c on a.indexrelid = c.oid
                                            where b.relname = '{0}' and a.indisprimary = 'f'", tablename);
                return ExecuteQuery(strSQL, new DbParameter[] { }, tran);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public Dictionary<string, object> getProcInfo(string procname, int userid, DbTransaction tran)
        {
            try
            {
                string strSQL = string.Format(@"select pg_get_functiondef(a.proname::regproc) textsql
                                        from    pg_proc  a
                                        where a.proname ='{0}' ", procname);
                return ExecuteQuery(strSQL, new DbParameter[] { }, tran).FirstOrDefault();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<SQLObjectModel> getSQLObjects(string searchKey, int userid, DbTransaction tran)
        {
            try
            {
                string strSQL = string.Format(@"select * 
                                        from crm_sys_dbmgr_object 
                                        where objname like '%{0}%' ", searchKey.Replace("'","''"));
                return ExecuteQuery<SQLObjectModel>(strSQL, new DbParameter[] { }, tran);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<SQLTextModel> getSQLTextList(string objid, InitOrUpdate initorupdate, StructOrData structordata, int userid, DbTransaction tran)
        {
            try
            {
                string strSQL = string.Format(@"select * 
                                        from crm_sys_dbmgr_sql 
                                        where sqlobjid='{0}'
	                                        and (initorupdate = {1} or {1}=0)
                                        and (structordata ={2} or {2} = 0 )
                                ", objid,initorupdate,structordata);
                return ExecuteQuery<SQLTextModel>(strSQL, new DbParameter[] { }, tran);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public Dictionary<string, object> getTableInfo(string tablename, int userid, DbTransaction tran)
        {
            try
            {
                string strSQL = string.Format(@"SELECT
	                                                relname AS tabname,
	                                                CAST (
		                                                obj_description (relfilenode, 'pg_class') AS VARCHAR
	                                                ) AS COMMENT
                                                FROM
	                                                pg_class C
                                                WHERE
	                                                relkind = 'r'
                                                AND relname NOT LIKE 'pg_%'
                                                AND relname NOT LIKE 'sql_%'
                                                and relname = '{0}'
                                                ORDER BY
	                                                relname", tablename);
                return ExecuteQuery(strSQL, new DbParameter[] { }, tran).FirstOrDefault();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<Dictionary<string, object>> getTriggers(string tablename, int userid, DbTransaction tran)
        {
            try
            {
                string strSQL = string.Format(@"select b.relname,c.proname ,pg_get_triggerdef(a.oid ) sqltext,a.* 
                                    from pg_trigger a 
                                    inner join pg_class b on a.tgrelid = b.oid 
                                    left outer  join pg_proc c on c.oid = a.tgfoid 
                                    where a.tgisinternal = 'f'
	                                    and b.relname = '{0}'
                                    ", tablename);
                return ExecuteQuery(strSQL, new DbParameter[] { }, tran);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public SQLObjectModel querySQLObject(string id, int userid, DbTransaction tran)
        {
            try
            {
                string strSQL = string.Format(@"select * 
                                        from crm_sys_dbmgr_object 
                                        where id ='{0}'::uuid
                                    ", id);
                return ExecuteQuery<SQLObjectModel>(strSQL, new DbParameter[] { }, tran).FirstOrDefault();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public void saveSQLObject(SQLObjectModel model, int userid, DbTransaction tran)
        {
            if (model.Id == null || model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();
                model.checkEmpty();
                string strSQL = string.Format(@"insert into crm_sys_dbmgr_object (
                            id,objtype,sqlpath,lastversion,objname,
                            remark,relativeobj,name)
                            VALUES
	                            (		'{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}');",
                                model.Id, (int)model.ObjType, model.SqlPath.Replace("'", "''"),
                                model.LastVersion.Replace("'", "''"),
                                model.ObjName.Replace("'", "''"),
                                model.Remark.Replace("'", "''"),
                                model.RelativeObj.Replace("'", "''"),
                                model.Name.Replace("'", "''"));
                ExecuteNonQuery(strSQL, new DbParameter[] { }, tran);
            }
            else
            {
                model.checkEmpty();
                string strSQL = string.Format(@"update crm_sys_dbmgr_object set objtype={1}," +
                    "sqlpath='{2}' ,lastversion='{3}' , objname='{4}'," +
                    "remark='{5}',relativeobj='{6}',name='{7}' where id='{0}'", 
                                model.Id, (int)model.ObjType, model.SqlPath.Replace("'", "''"),
                                model.LastVersion.Replace("'", "''"),
                                model.ObjName.Replace("'", "''"),
                                model.Remark.Replace("'", "''"),
                                model.RelativeObj.Replace("'", "''"),
                                model.Name.Replace("'", "''"));
                ExecuteNonQuery(strSQL, new DbParameter[] { }, tran);
            }
        }

        public void saveSQLText(SQLTextModel model, int userid, DbTransaction tran)
        {
            if (model.Id == null || model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();
                model.checkEmpty();
                string strSQL = string.Format(@"INSERT INTO crm_sys_dbmgr_sql (
	                                        id,sqlobjid,version,remark,sqltext,
	                                        sqlorjson,initorupdate,isrun,structordata
                                        )
                                        VALUES
	                                        ('{0}','{1}', '{2}','{3}','{4}', '{5}','{6}',
		                                        '{7}', '{8}');", model.Id,
                                         model.SqlObjId, model.Version.Replace("'", "''"),
                                         model.Remark.Replace("'", "''"),
                                         model.SqlText.Replace("'", "''"),
                                          (int)model.sqlOrJson, (int)model.initOrUpdate, model.isRun, (int)model.structOrData);
                ExecuteNonQuery(strSQL, new DbParameter[] { }, tran);
            }
            else
            {
                model.checkEmpty();
                string strSQL = string.Format(@"update crm_sys_dbmgr_sql set sqlobjid='{1}',
                                        version='{2}',remark='{3}',sqltext='{4}',
	                                        sqlorjson={5},initorupdate={6},isrun={7},structordata={8}
                                        where id ='{0}'", model.Id,
                                         model.SqlObjId, model.Version.Replace("'", "''"),
                                         model.Remark.Replace("'", "''"),
                                         model.SqlText.Replace("'", "''"),
                                          (int)model.sqlOrJson, (int)model.initOrUpdate, model.isRun, (int)model.structOrData);
                ExecuteNonQuery(strSQL, new DbParameter[] { }, tran);
            }
        }
    }
}
