using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.IRepository;
using System.Linq;
using UBeat.Crm.CoreApi.DomainModel.DbManage;
using Npgsql;

namespace UBeat.Crm.CoreApi.Repository.Repository.DbManage
{
    public class DbManageRepository : RepositoryBase, IDbManageRepository
    {
        public bool checkHasPreProName(string proname, int userid, DbTransaction tran)
        {
            try
            {
                string strSQL = string.Format(@"select *from crm_sys_dbmgr_object where objtype =2 and objname ='{0}' ", proname.Replace("'","''"));
                if (ExecuteQuery(strSQL, new DbParameter[] { }, tran).FirstOrDefault() == null)
                    return false;
                return true;
            }
            catch (Exception ex) {
            }
            return false;
        }

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

        public Dictionary<string, object> getProcInfo(string procname,string param, int userid, DbTransaction tran)
        {
            try
            {
                string strSQL = "";
                if (param == null || param == "")
                {
                    strSQL = string.Format(@"select pg_get_functiondef(a.oid::regproc) textsql
                                        from    pg_proc  a
                                        where a.proname ='{0}'  and proargnames is null ", procname);
                }
                else {
                    strSQL = string.Format(@"select pg_get_functiondef(a.oid::regproc) textsql
                                        from    pg_proc  a
                                        where a.proname ='{0}'  and proargnames ='{1}' ", procname, param);
                }
                
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
        public Dictionary<string, object> getTypeInfo(string typename, int userid, DbTransaction tran)
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
	                                                relkind = 'c'
                                                AND relname NOT LIKE 'pg_%'
                                                AND relname NOT LIKE 'sql_%'
                                                and relname = '{0}'
                                                ORDER BY
	                                                relname", typename);
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

        public List<SQLTextModel> ListInitSQLForFunc(SQLObjectBelongSysEnum exportSys, StructOrData isStruct, int userId,DbTransaction tran)
        {
            try
            {
                string strSQL = @"select b.*
                                from crm_sys_dbmgr_object a inner join crm_sys_dbmgr_sql b on a.""id"" = b.sqlobjid 
                                where b.initorupdate = 1 and   a.objtype =2";
                if (exportSys != SQLObjectBelongSysEnum.All) {
                    strSQL = strSQL + " And a.belongto=" + ((int)exportSys).ToString();
                }
                if (isStruct != StructOrData.All) {
                    strSQL = strSQL + " And b.structordata=" + ((int)isStruct).ToString();
                }

                strSQL = strSQL + " order by a.recorder,a.objname ";
                return ExecuteQuery<SQLTextModel>(strSQL, new DbParameter[] { }, tran);

            }
            catch (Exception ex) {
                return null;
            }
        }
        public List<SQLTextModel> ListInitSQLForTable(SQLObjectBelongSysEnum exportSys, StructOrData isStruct, int userId,DbTransaction tran)
        {
            try
            {
                string strSQL = @"select b.*
                                from crm_sys_dbmgr_object a inner join crm_sys_dbmgr_sql b on a.""id"" = b.sqlobjid 
                                where b.initorupdate = 1 and   a.objtype =1";
                if (exportSys != SQLObjectBelongSysEnum.All)
                {
                    strSQL = strSQL + " And a.belongto=" + ((int)exportSys).ToString();
                }
                if (isStruct != StructOrData.All)
                {
                    strSQL = strSQL + " And b.structordata=" + ((int)isStruct).ToString();
                }

                strSQL = strSQL + " order by a.recorder,a.objname ";
                return ExecuteQuery<SQLTextModel>(strSQL, new DbParameter[] { }, tran);

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
                string strSQL = string.Format(@"
insert into crm_sys_dbmgr_object (
    id,objtype,sqlpath,lastversion,objname,
    remark,relativeobj,name,recstatus,belongto,
    needinitsql,procparam,recorder)
VALUES(
    @id,@objtype,@sqlpath,@lastversion,@objname,
    @remark,@relativeobj,@name,@recstatus,@belongto,
    @needinitsql,@procparam,@recorder);");
                DbParameter[] p = new DbParameter[] {
                     new Npgsql.NpgsqlParameter("@id", model.Id),
                     new Npgsql.NpgsqlParameter("@objtype",  (int)model.ObjType),
                     new Npgsql.NpgsqlParameter("@sqlpath",  model.SqlPath),
                     new Npgsql.NpgsqlParameter("@lastversion",  model.LastVersion),
                     new Npgsql.NpgsqlParameter("@objname",  model.ObjName),
                     new Npgsql.NpgsqlParameter("@remark",  model.Remark),
                     new Npgsql.NpgsqlParameter("@relativeobj",  model.RelativeObj),
                     new Npgsql.NpgsqlParameter("@name",  model.Name),
                     new Npgsql.NpgsqlParameter("@recstatus",  model.RecStatus),
                     new Npgsql.NpgsqlParameter("@belongto", (int)model.belongTo),
                     new Npgsql.NpgsqlParameter("@needinitsql", model.NeedInitSQL),
                     new Npgsql.NpgsqlParameter("@procparam", model.ProcParam),
                     new Npgsql.NpgsqlParameter("@recorder", model.RecOrder)
                };
                ExecuteNonQuery(strSQL, p, tran);
            }
            else
            {
                model.checkEmpty();
                string strSQL = string.Format(@"update crm_sys_dbmgr_object set objtype=@objtype," +
                    "sqlpath=@sqlpath ,lastversion=@lastversion , objname=@objname," +
                    "remark=@remark,relativeobj=@relativeobj,name=@name,recstatus=@recstatus," +
                    "belongto=@belongto,needinitsql=@needinitsql,procparam=@procparam,recorder=@recorder where id=@id");
                DbParameter[] p = new DbParameter[] {
                     new Npgsql.NpgsqlParameter("@id", model.Id),
                     new Npgsql.NpgsqlParameter("@objtype",  (int)model.ObjType),
                     new Npgsql.NpgsqlParameter("@sqlpath",  model.SqlPath),
                     new Npgsql.NpgsqlParameter("@lastversion",  model.LastVersion),
                     new Npgsql.NpgsqlParameter("@objname",  model.ObjName),
                     new Npgsql.NpgsqlParameter("@remark",  model.Remark),
                     new Npgsql.NpgsqlParameter("@relativeobj",  model.RelativeObj),
                     new Npgsql.NpgsqlParameter("@name",  model.Name),
                     new Npgsql.NpgsqlParameter("@recstatus",  model.RecStatus),
                     new Npgsql.NpgsqlParameter("@belongto", (int)model.belongTo),
                     new Npgsql.NpgsqlParameter("@needinitsql", model.NeedInitSQL),
                     new Npgsql.NpgsqlParameter("@procparam", model.ProcParam),
                     new Npgsql.NpgsqlParameter("@recorder", model.RecOrder)
                };
                ExecuteNonQuery(strSQL, p, tran);
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
	                                        (@id,@sqlobjid,@version,@remark,@sqltext,
	                                        @sqlorjson,@initorupdate,@isrun,@structordata);");
                DbParameter[] p = new DbParameter[] {
                    new Npgsql.NpgsqlParameter("@id", model.Id),
                    new Npgsql.NpgsqlParameter("@sqlobjid", model.SqlObjId),
                    new Npgsql.NpgsqlParameter("@version", model.Version),
                    new Npgsql.NpgsqlParameter("@remark", model.Remark),
                    new Npgsql.NpgsqlParameter("@sqltext", model.SqlText),
                    new Npgsql.NpgsqlParameter("@sqlorjson",(int)model.sqlOrJson),
                    new Npgsql.NpgsqlParameter("@initorupdate",(int)model.initOrUpdate),
                    new Npgsql.NpgsqlParameter("@isrun", model.isRun),
                    new Npgsql.NpgsqlParameter("@structordata",(int)model.structOrData)
                };
                ExecuteNonQuery(strSQL, p, tran);
            }
            else
            {
                model.checkEmpty();
                string strSQL = string.Format(@"update crm_sys_dbmgr_sql set sqlobjid=@sqlobjid,
                                        version=@version,remark=@remark,sqltext=@sqltext,
	                                        sqlorjson=@sqlorjson,initorupdate=@initorupdate,isrun=@isrun,structordata=@structordata
                                        where id =@id");
                DbParameter[] p = new DbParameter[] {
                    new Npgsql.NpgsqlParameter("@id", model.Id),
                    new Npgsql.NpgsqlParameter("@sqlobjid", model.SqlObjId),
                    new Npgsql.NpgsqlParameter("@version", model.Version),
                    new Npgsql.NpgsqlParameter("@remark", model.Remark),
                    new Npgsql.NpgsqlParameter("@sqltext", model.SqlText),
                    new Npgsql.NpgsqlParameter("@sqlorjson",(int)model.sqlOrJson),
                    new Npgsql.NpgsqlParameter("@initorupdate",(int)model.initOrUpdate),
                    new Npgsql.NpgsqlParameter("@isrun", model.isRun),
                    new Npgsql.NpgsqlParameter("@structordata",(int)model.structOrData)
                };
                ExecuteNonQuery(strSQL,p, tran);
            }
        }

        public List<SQLTextModel> ListInitSQLForType(SQLObjectBelongSysEnum exportSys, StructOrData isStruct, int userId, DbTransaction tran)
        {
            try
            {
                string strSQL = @"select b.*
                                from crm_sys_dbmgr_object a inner join crm_sys_dbmgr_sql b on a.""id"" = b.sqlobjid 
                                where b.initorupdate = 1 and   a.objtype =3 ";
                if (exportSys != SQLObjectBelongSysEnum.All)
                {
                    strSQL = strSQL + " And a.belongto=" + ((int)exportSys).ToString();
                }
                if (isStruct != StructOrData.All)
                {
                    strSQL = strSQL + " And b.structordata=" + ((int)isStruct).ToString();
                }
                strSQL = strSQL + " order by a.recorder,a.objname ";
                return ExecuteQuery<SQLTextModel>(strSQL, new DbParameter[] { }, tran);

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<string> ListAllDirs(int userId, DbTransaction tran)
        {
            try
            {
                string sql = "Select distinct  sqlpath from crm_sys_dbmgr_object order by sqlpath";
                List<Dictionary<string, object>> l = ExecuteQuery(sql, new DbParameter[] { }, tran);
                List<string> ret = new List<string>();
                foreach (Dictionary<string, object> item in l) {
                    ret.Add((string)item["sqlpath"]);
                }
                return ret;
            }
            catch (Exception ex) {

            }return null;
        }

        public List<SQLObjectModel> SearchSQLObjects(DbListObjectsParamInfo paramInfo, int userId, DbTransaction tran)
        {
            try
            {
                string strSQL = "";
                strSQL = "Select * from  crm_sys_dbmgr_object where 1=1 and  sqlpath = @sqlpath and (objname like @objname or name like @objname) ";
                if (paramInfo.ObjectType != SQLObjectTypeEnum.All) {
                    strSQL = strSQL + " And objtype=" + ((int)paramInfo.ObjectType).ToString();
                }
                DbParameter [] p = new DbParameter[] {
                    new NpgsqlParameter("@sqlpath",paramInfo.FullPath),
                    new NpgsqlParameter("@objname","%"+paramInfo.SearchKey+"%")
                };
                return ExecuteQuery<SQLObjectModel>(strSQL, p, tran);
            }
            catch (Exception ex) {
            }
            return null;
        }
    }
}
