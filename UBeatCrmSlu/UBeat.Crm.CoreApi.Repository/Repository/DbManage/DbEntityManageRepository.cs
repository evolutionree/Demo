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

        public void updateEntityInfo(DbEntityInfo entityInfo, int userId, DbTransaction tran)
        {
           
            throw new NotImplementedException();
        }
    }
}
