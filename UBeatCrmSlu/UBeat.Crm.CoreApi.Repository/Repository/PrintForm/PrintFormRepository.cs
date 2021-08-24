using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.PrintForm;
using UBeat.Crm.CoreApi.IRepository;

namespace UBeat.Crm.CoreApi.Repository.Repository.PrintForm
{
    public class PrintFormRepository : RepositoryBase, IPrintFormRepository
    {
        public Guid InsertTemplate(CrmSysEntityPrintTemplate model, DbTransaction tran = null)
        {
            Guid recid = Guid.NewGuid();
            var sql = @"INSERT INTO crm_sys_entity_print_template 
                        (recid,entityid,templatename,templatetype,datasourcetype,
datasourcefunc,assemblyname,classtypename,extjs,fileid,
ruleid,ruledesc,description,reccreated,recupdated,
reccreator,recupdator,exportconfig)
                        VALUES(@recid,@entityid,@templatename,@templatetype,@datasourcetype,
@datasourcefunc,@assemblyname,@classtypename,@extjs,@fileid,
@ruleid,@ruledesc,@description,@reccreated,@recupdated,
@reccreator,@recupdator,@exportconfig)";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("recid", recid),
                new NpgsqlParameter("entityid", model.EntityId),
                new NpgsqlParameter("templatename", model.TemplateName),
                new NpgsqlParameter("templatetype", (int)model.TemplateType),
                new NpgsqlParameter("datasourcetype", (int)model.DataSourceType),
                new NpgsqlParameter("datasourcefunc", model.DataSourceFunc),
                new NpgsqlParameter("assemblyname", model.AssemblyName),
                new NpgsqlParameter("classtypename", model.ClassTypeName),
                new NpgsqlParameter("extjs", model.ExtJs),
                new NpgsqlParameter("fileid", model.FileId),
                new NpgsqlParameter("ruleid", model.RuleId),
                new NpgsqlParameter("ruledesc", model.RuleDesc),
                new NpgsqlParameter("description", model.Description),
                new NpgsqlParameter("reccreated", model.RecCreated),
                new NpgsqlParameter("recupdated", model.RecUpdated),
                new NpgsqlParameter("reccreator", model.RecCreator),
                new NpgsqlParameter("recupdator", model.RecUpdator),
                new NpgsqlParameter("exportconfig", model.ExportConfig),
            };

            var rowscount = ExecuteNonQuery(sql, param, tran);
            if (rowscount != 1)
                throw new Exception("新增模板失败");
            return recid;
        }

        public void DeleteTemplates(List<Guid> recids, int usernumber, DbTransaction tran = null)
        {
            var sql = "DELETE FROM crm_sys_entity_print_template  WHERE recid= ANY(@recids);";
            var param = new DbParameter[]
            {
                    new NpgsqlParameter("recids", recids.ToArray())
            };
            var rowscount = ExecuteNonQuery(sql, param, tran);
        }

        public void SetTemplatesStatus(List<Guid> recids, int recstatus, int usernumber, DbTransaction tran = null)
        {

            var sql = "UPDATE crm_sys_entity_print_template  SET recstatus=@recstatus,recupdated=@recupdated,recupdator=@recupdator WHERE recid= ANY(@recids);";
            var param = new DbParameter[]
            {
                    new NpgsqlParameter("recids", recids.ToArray()),
                    new NpgsqlParameter("recstatus", recstatus),
                    new NpgsqlParameter("recupdated", DateTime.Now),
                    new NpgsqlParameter("recupdator", usernumber),
            };
            var rowscount = ExecuteNonQuery(sql, param, tran);
        }



        public void UpdateTemplate(CrmSysEntityPrintTemplate model, DbTransaction tran = null)
        {
            var fileidSql = @" SELECT fileid FROM crm_sys_entity_print_template WHERE recid=@recid ";
            var fileidParam = new DbParameter[]
            {
                 new NpgsqlParameter("recid", model.RecId),
            };
            var result = ExecuteScalar(fileidSql, fileidParam, tran);
             //如果模板文件发生改变，则需要新增一条记录，保留旧版本数据
            if (result != null && !result.ToString().Equals(model.FileId.GetValueOrDefault().ToString()))
            {
                InsertTemplate(model, tran);
                var sql = @"update crm_sys_entity_print_template set recstatus=0 where   recid = @recid";
                var param = new DbParameter[]
                {
                    new NpgsqlParameter("recid", model.RecId),
                };
                var rowscount = ExecuteNonQuery(sql, param, tran);
            }
            else//否则更新模板信息数据
            {
                var sql = @"UPDATE crm_sys_entity_print_template 
                        SET templatename = @templatename, templatetype = @templatetype, datasourcetype = @datasourcetype,
                        datasourcefunc = @datasourcefunc, assemblyname = @assemblyname,classtypename = @classtypename,extjs = @extjs, fileid = @fileid, ruleid = @ruleid, ruledesc = @ruledesc,
                        description = @description, recupdated = @recupdated, recupdator = @recupdator, exportconfig=@exportconfig
                        WHERE recid = @recid";

                var param = new DbParameter[]
                {
                    new NpgsqlParameter("templatename", model.TemplateName),
                    new NpgsqlParameter("templatetype", (int)model.TemplateType),
                    new NpgsqlParameter("datasourcetype", (int)model.DataSourceType),
                    new NpgsqlParameter("datasourcefunc", model.DataSourceFunc),
                    new NpgsqlParameter("assemblyname", model.AssemblyName),
                    new NpgsqlParameter("classtypename", model.ClassTypeName),
                    new NpgsqlParameter("extjs", model.ExtJs),
                    new NpgsqlParameter("fileid", model.FileId),
                    new NpgsqlParameter("ruleid", model.RuleId),
                    new NpgsqlParameter("ruledesc", model.RuleDesc),
                    new NpgsqlParameter("description", model.Description),
                    new NpgsqlParameter("recupdated", model.RecUpdated),
                    new NpgsqlParameter("recupdator", model.RecUpdator),
                    new NpgsqlParameter("recid", model.RecId),
                    new NpgsqlParameter("exportconfig", model.ExportConfig),
                };
                var rowscount = ExecuteNonQuery(sql, param, tran);
            }
        }
        /// <summary>
        /// 更新数据源Ucode
        /// </summary>
        /// <param name="recId"></param>
        /// <param name="uCode"></param>
        /// <param name="userId"></param>
        public void SaveUCode(Guid recId, string uCode, int userId)
        {
            var sql = @"UPDATE crm_sys_entity_print_template 
                        SET extjs = @extjs 
                        WHERE recid = @recid";
            var param = new DbParameter[]
               {
                    new NpgsqlParameter("extjs",uCode),
                    new NpgsqlParameter("recid",recId),
               };
            ExecuteNonQuery(sql, param, null);
        }
        public CrmSysEntityPrintTemplate GetTemplateInfo(Guid recid, DbTransaction tran = null)
        {
            string sql = @"SELECT * FROM crm_sys_entity_print_template WHERE recid=@recid ";
            var param = new DbParameter[]
            {
                 new NpgsqlParameter("recid", recid),
            };

            return ExecuteQuery<CrmSysEntityPrintTemplate>(sql, param, tran).FirstOrDefault();
        }

        public List<CrmSysEntityPrintTemplate> GetTemplateList(Guid entityid, int recstate, DbTransaction tran = null)
        {
            string sql = string.Empty;

            if (recstate == 0)
            {
                sql = @"SELECT pt.*,u.username AS reccreator_name,u2.username AS recupdator_name FROM crm_sys_entity_print_template pt
                        LEFT JOIN crm_sys_userinfo u ON u.userid=pt.reccreator
                        LEFT JOIN crm_sys_userinfo u2 on u2.userid=pt.recupdator
                        WHERE pt.entityid=@entityid AND pt.recstatus=0 ORDER BY recversion DESC;  ";
            }
            else if (recstate == 1)
            {
                sql = @"SELECT pt.*,u.username AS reccreator_name,u2.username AS recupdator_name FROM crm_sys_entity_print_template pt
                        LEFT JOIN crm_sys_userinfo u ON u.userid=pt.reccreator
                        LEFT JOIN crm_sys_userinfo u2 on u2.userid=pt.recupdator
                        WHERE pt.entityid=@entityid AND pt.recstatus=1 ORDER BY recversion DESC;  ";
            }
            else sql = @"SELECT pt.*,u.username AS reccreator_name,u2.username AS recupdator_name FROM crm_sys_entity_print_template pt
                        LEFT JOIN crm_sys_userinfo u ON u.userid=pt.reccreator
                        LEFT JOIN crm_sys_userinfo u2 on u2.userid=pt.recupdator
                        WHERE pt.entityid=@entityid ORDER BY recversion DESC;  ";
            var param = new DbParameter[]
            {
                 new NpgsqlParameter("entityid", entityid),
            };

            return ExecuteQuery<CrmSysEntityPrintTemplate>(sql, param, tran);
        }

        /// <summary>
        /// 获取某条记录可以关联的所有模板文件
        /// </summary>
        public List<CrmSysEntityPrintTemplate> GetRecDataTemplateList(Guid entityid,Guid businessid,int userno, DbTransaction tran = null)
        {
            string tableSql = @"SELECT entitytable FROM crm_sys_entity WHERE entityid=@entityid";
            var tableParam = new DbParameter[]
            {
                 new NpgsqlParameter("entityid", entityid),
            };
            object tableobj = ExecuteScalar(tableSql, tableParam, tran);
            if(tableobj==null|| string.IsNullOrEmpty(tableobj.ToString()))
            {
                throw new Exception("找不到实体id对应的表名");
            }
            string entityTableName = tableobj.ToString();

            string sql = string.Format(@" SELECT pt.* FROM crm_sys_entity_print_template pt 
                            INNER JOIN {0} e ON e.recid=@businessid
                            LEFT JOIN crm_sys_rule AS r ON r.ruleid=pt.ruleid 
                            WHERE pt.entityid=@entityid AND pt.recstatus=1 AND  crm_func_printtemplate_rule_check(@entitytablename,e.recid,r.rulesql,@userno)
                            ORDER BY pt.recversion DESC; ", entityTableName);
            
            var param = new DbParameter[]
            {
                 new NpgsqlParameter("entityid", entityid),
                 new NpgsqlParameter("businessid", businessid),
                 new NpgsqlParameter("entitytablename", entityTableName),
                 new NpgsqlParameter("userno", userno),
            };

            return ExecuteQuery<CrmSysEntityPrintTemplate>(sql, param, tran);
        }


        /// <summary>
        /// 获取打印数据源（通过函数处理）
        /// </summary>
        /// <param name="entityId">实体id</param>
        /// <param name="recId">记录id</param>
        /// <param name="dbFunction">函数名称</param>
        /// <param name="usernumber">当前操作人</param>
        /// <returns>返回数据已字典形式，如果不是实体中的字段，字典中的key必须和模板定义的字段匹配上</returns>
        public IDictionary<string, object> GetPrintDetailDataByProc(Guid entityId, Guid recId,string dbFunction, int usernumber)
        {
            string sql = string.Format(@"SELECT * FROM {0}(@entityid,@recId,@usernumber)", dbFunction);
            var param = new DbParameter[]
            {
                 new NpgsqlParameter("entityid", entityId),
                 new NpgsqlParameter("recid", recId),
                 new NpgsqlParameter("usernumber", usernumber),
            };
            var res = ExecuteQueryRefCursor(sql, param);
            if (res.Count != 1)
                throw new Exception("返回结果集格式错误");
            var datalist = res.FirstOrDefault().Value;
            if (datalist == null || datalist.Count == 0)
                throw new Exception("不存在该业务数据记录");

            return datalist.FirstOrDefault();
        }

    }
}
