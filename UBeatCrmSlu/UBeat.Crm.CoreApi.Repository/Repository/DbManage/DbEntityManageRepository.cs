using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.DbManage;
using UBeat.Crm.CoreApi.IRepository;
using System.Linq;
namespace UBeat.Crm.CoreApi.Repository.Repository.DbManage
{
    public class DbEntityManageRepository : RepositoryBase,IDbEntityManageRepository
    {
        public string addEntityInfo(DbEntityInfo entityInfo, int userId, DbTransaction tran)
        {
            try
            {
                string strSQL = @"
INSERT INTO crm_sys_entity (
	entityid,	entityname,	entitytable,modeltype,remark,
	styles,icons,relentityid,relaudit,recorder,
	recstatus,reccreator,recupdator,reccreated,recupdated,
	publishstatus,delflag,servicesjson,functionbuttons,datadelflag,
	newload,editload,checkload
)
VALUES
	(
@entityid,@entityname,@entitytable,@modeltype,@remark,
@styles,@icons,@relentityid,1,@recorder,
1,@userid,@userid,now(),now(),
1,@delflag,@servicesjson,@functionbuttons,@datadelflag,
@newload,@editload,@checkload
	)
";
                string servicejson = null;
                string functionbuttons = null;
                if (entityInfo.ServiceJson != null) {
                    servicejson = Newtonsoft.Json.JsonConvert.SerializeObject(entityInfo.ServiceJson);
                }
                if (entityInfo.FunctionButtons != null) {
                    functionbuttons = Newtonsoft.Json.JsonConvert.SerializeObject(entityInfo.FunctionButtons);
                }
                DbParameter[] param = new DbParameter[] {
                    new Npgsql.NpgsqlParameter("@entityid",entityInfo.EntityId),
                    new Npgsql.NpgsqlParameter("@entityname",entityInfo.EntityName),
                    new Npgsql.NpgsqlParameter("@entitytable",entityInfo.EntityTable),
                    new Npgsql.NpgsqlParameter("@modeltype",(int)entityInfo.ModelType),
                    new Npgsql.NpgsqlParameter("@remark",entityInfo.Remark),
                    new Npgsql.NpgsqlParameter("@styles",entityInfo.Styles),
                    new Npgsql.NpgsqlParameter("@icons",entityInfo.Icons),
                    new Npgsql.NpgsqlParameter("@relentityid",entityInfo.RelEntityId),
                    new Npgsql.NpgsqlParameter("@recorder",1),
                    new Npgsql.NpgsqlParameter("@userid",userId),
                    new Npgsql.NpgsqlParameter("@delflag","0"),
                    new Npgsql.NpgsqlParameter("@servicesjson",servicejson),
                    new Npgsql.NpgsqlParameter("@functionbuttons",functionbuttons),
                    new Npgsql.NpgsqlParameter("@datadelflag",1),
                    new Npgsql.NpgsqlParameter("@newload",entityInfo.NewLoad),
                    new Npgsql.NpgsqlParameter("@editload",entityInfo.NewLoad),
                    new Npgsql.NpgsqlParameter("@checkload",entityInfo.CheckLoad),
                };
                ExecuteNonQuery(strSQL, param, tran);
            }
            catch (Exception ex) {
                return ex.Message;
            }
            return null;
        }

        public string createEmptyTableForEntity(string tablename, int userId, DbTransaction tran)
        {
            try
            {
                string strSQL = "create table " + tablename + " ( recid uuid DEFAULT uuid_generate_v4() NOT NULL) WITH (OIDS=FALSE);";
                ExecuteNonQuery(strSQL, new DbParameter[] { }, tran);
                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// reflect实体分类，以及与实体分类 相关的字段可见信息
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="exportCatelogIds"></param>
        /// <param name="userId"></param>
        /// <param name="tran"></param>
        /// <returns></returns>
        public List<DbEntityCatelogInfo> getCatelogs(string entityId, string[] exportCatelogIds, int userId, DbTransaction tran)
        {
            if (exportCatelogIds == null || exportCatelogIds.Length == 0) {
                exportCatelogIds = new string[] { entityId };
            }
            List<DbEntityCatelogInfo> ret = new List<DbEntityCatelogInfo>();
            foreach (string catelogid in exportCatelogIds) {
                try
                {
                    string strSQL = @"select * from crm_sys_entity_category where categorid =@categorid";
                    DbEntityCatelogInfo catelogInfo = ExecuteQuery<DbEntityCatelogInfo>(strSQL, new DbParameter[] { new Npgsql.NpgsqlParameter("@categorid", catelogid) }, tran).FirstOrDefault();
                    if (catelogInfo == null) continue;
                    strSQL = @"select * from crm_sys_entity_field_rules
where recstatus = 1 
	and typeid = @typeid
order by recorder ";
                    List<DbEntityFieldRuleInfo> fieldRules = ExecuteQuery<DbEntityFieldRuleInfo>(strSQL, new DbParameter[] { new Npgsql.NpgsqlParameter("@typeid", catelogid) }, tran);
                    catelogInfo.FieldRules = fieldRules;
                    ret.Add( catelogInfo);
                }
                catch (Exception ex) {
                }
            }
            return ret;
        }

        /// <summary>
        /// 获取手机列表的按钮控件列表
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="userId"></param>
        /// <param name="tran"></param>
        /// <returns></returns>
        public List<DbEntityComponentConfigInfo> getComponentConfigList(string entityId, int userId, DbTransaction tran)
        {
            List<DbEntityComponentConfigInfo> ret = null;
            try
            {
                string strSQL = @"select * from crm_sys_entity_compoment_config
where recstatus = 1 
	and entityid = @entityid";
                return ExecuteQuery<DbEntityComponentConfigInfo>(strSQL, new DbParameter[] { new Npgsql.NpgsqlParameter("@entityid", entityId) }, tran);
            }
            catch (Exception ex) {

            }
            return null;
        }

        public List<DbEntityFieldInfo> getEntityFields(string entityid, DbEntityReflectConfigParam configParam, int userId, DbTransaction tran)
        {
            try
            {
                string strSQL = "Select * from crm_sys_entity_fields where entityid::text=@entityid ";
                if (configParam.IsReflectDeleted ==false) {
                    strSQL = strSQL + " And RecStatus= 1";
                }
                return ExecuteQuery<DbEntityFieldInfo>(strSQL, new DbParameter[] { new Npgsql.NpgsqlParameter("@entityid", entityid) }, tran);
            }
            catch (Exception ex) {
            }
            return null;
        }

        public DbEntityInfo getEntityInfo(string entityid, int userId, DbTransaction tran)
        {
            try
            {
                string strSQL = "Select * from crm_sys_entity where entityid::text=@entityid";
                return ExecuteQuery<DbEntityInfo>(strSQL, new DbParameter[] { new Npgsql.NpgsqlParameter("@entityid", entityid) }, tran).FirstOrDefault();
            }
            catch (Exception ex)
            {
            }
            return null;
        }

        public DbEntityMobileListConfigInfo getMobileColumnConfig(string entityId, int userId, DbTransaction tran)
        {
            try
            {
                string strSQL = "select * from crm_sys_entity_listview_config where recstatus =1 and   entityid::text=@entityid";
                DbEntityMobileListConfigInfo ret = ExecuteQuery<DbEntityMobileListConfigInfo>(strSQL, new DbParameter[] { new Npgsql.NpgsqlParameter("@entityid", entityId) }, tran).FirstOrDefault();
                if (ret == null) return null;
                strSQL = @"
select * from crm_sys_entity_listview_viewcolumn
where recstatus = 1 and viewtype = 0  and entityid::text =@entityid 
order by recorder ";
                ret.Columns =  ExecuteQuery<DbEntityMobileListColumnInfo>(strSQL, new DbParameter[] { new Npgsql.NpgsqlParameter("@entity", entityId) }, tran);
                return ret;
            }
            catch (Exception ex) {
            }
            return null;
        }

        /// <summary>
        /// 获取web列定义信息
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="userId"></param>
        /// <param name="tran"></param>
        /// <returns></returns>
        public List<DbEntityWebFieldInfo> getWebFieldList(string entityId, int userId, DbTransaction tran)
        {
            try
            {
                string strSQL = @"
select * from crm_sys_entity_listview_viewcolumn
where recstatus = 1 and viewtype = 0  and entityid::text =@entityid 
order by recorder ";
                return ExecuteQuery<DbEntityWebFieldInfo>(strSQL, new DbParameter[] { new Npgsql.NpgsqlParameter("@entity",entityId)}, tran);
            }
            catch (Exception ex) {
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityInfo"></param>
        /// <param name="userId"></param>
        /// <param name="tran"></param>
        public void updateEntityInfo(DbEntityInfo entityInfo, int userId, DbTransaction tran)
        {
            try
            {
                
            }
            catch (Exception ex) {
            }
        }
    }
}
