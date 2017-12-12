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
        public void addEntityInfo(DbEntityInfo entityInfo, int userId, DbTransaction tran)
        {
            try
            {
            }
            catch (Exception ex) {
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
