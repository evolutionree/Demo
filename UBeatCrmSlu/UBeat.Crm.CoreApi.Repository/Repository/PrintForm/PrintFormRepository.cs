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
                        (recid,entityid,templatename,templatetype,datasourcetype,datasourcefunc,extjs,fileid,ruleid,ruledesc,description,reccreated,recupdated,reccreator,recupdator)
                        VALUES(@recid,@entityid,@templatename,@templatetype,@datasourcetype,@datasourcefunc,@extjs,@fileid,@ruleid,@ruledesc,@description,@reccreated,@recupdated,@reccreator,@recupdator)";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("recid", recid),
                new NpgsqlParameter("entityid", model.EntityId),
                new NpgsqlParameter("templatename", model.TemplateName),
                new NpgsqlParameter("templatetype", (int)model.TemplateType),
                new NpgsqlParameter("datasourcetype", (int)model.DataSourceType),
                new NpgsqlParameter("datasourcefunc", model.DataSourceFunc),
                new NpgsqlParameter("extjs", model.ExtJs),
                new NpgsqlParameter("fileid", model.FileId),
                new NpgsqlParameter("ruleid", model.RuleId),
                new NpgsqlParameter("ruledesc", model.RuleDesc),
                new NpgsqlParameter("description", model.Description),
                new NpgsqlParameter("reccreated", model.RecCreated),
                new NpgsqlParameter("recupdated", model.RecUpdated),
                new NpgsqlParameter("reccreator", model.RecCreator),
                new NpgsqlParameter("recupdator", model.RecUpdator),
            };

            var rowscount = ExecuteNonQuery(sql, param, tran);
            if (rowscount != 1)
                throw new Exception("新增模板失败");
            return recid;
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
            }
            else//否则更新模板信息数据
            {
                var sql =@"UPDATE crm_sys_entity_print_template 
                        SET templatename = @templatename, templatetype = @templatetype, datasourcetype = @datasourcetype,
                        datasourcefunc = @datasourcefunc, extjs = @extjs, fileid = @fileid, ruleid = @ruleid, ruledesc = @ruledesc,
                        description = @description, recupdated = @recupdated, recupdator = @recupdator
                        WHERE recid = @recid";

                var param = new DbParameter[]
                {
                    new NpgsqlParameter("templatename", model.TemplateName),
                    new NpgsqlParameter("templatetype", (int)model.TemplateType),
                    new NpgsqlParameter("datasourcetype", (int)model.DataSourceType),
                    new NpgsqlParameter("datasourcefunc", model.DataSourceFunc),
                    new NpgsqlParameter("extjs", model.ExtJs),
                    new NpgsqlParameter("fileid", model.FileId),
                    new NpgsqlParameter("ruleid", model.RuleId),
                    new NpgsqlParameter("ruledesc", model.RuleDesc),
                    new NpgsqlParameter("description", model.Description),
                    new NpgsqlParameter("recupdated", model.RecUpdated),
                    new NpgsqlParameter("recupdator", model.RecUpdator),
                    new NpgsqlParameter("recid", model.RecId),
                };
                var rowscount = ExecuteNonQuery(sql, param, tran);
            }
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
                sql = @" SELECT * FROM crm_sys_entity_print_template WHERE entityid=@entityid AND recstatus=0 ORDER BY recversion DESC; ";
            }
            else if (recstate == 1)
            {
                sql = @" SELECT * FROM crm_sys_entity_print_template WHERE entityid=@entityid AND recstatus=1 ORDER BY recversion DESC; ";
            }
            else sql = @"SELECT * FROM crm_sys_entity_print_template WHERE entityid=@entityid ORDER BY recversion DESC";
            var param = new DbParameter[]
            {
                 new NpgsqlParameter("entityid", entityid),
            };

            return ExecuteQuery<CrmSysEntityPrintTemplate>(sql, param, tran);
        }
    }
}
