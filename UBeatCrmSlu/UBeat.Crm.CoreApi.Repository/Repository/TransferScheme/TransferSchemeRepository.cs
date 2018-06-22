using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.TransferScheme;
using UBeat.Crm.CoreApi.IRepository;
using System.Linq;


namespace UBeat.Crm.CoreApi.Repository.Repository.TransferScheme
{
   public class TransferSchemeRepository: RepositoryBase, ITransferSchemeRepository
    {
        /// <summary>
        /// 添加转移方案
        /// </summary>
        /// <param name="data"></param>
        /// <param name="tran"></param>
        /// <returns></returns>
        public bool AddTransferScheme(TransferSchemeModel data, DbTransaction tran)
        {
            string sql = @"insert into crm_sys_transfer_scheme values(@recid,@recname,@entityid,@association::jsonb,@remark,@reccreator,@reccreated,@recstatus,@fieldid) ";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("recid",data.RecId),
                new NpgsqlParameter("recname",data.RecName),
                new NpgsqlParameter("entityid",data.EntityId),
                new NpgsqlParameter("association",data.Association),
                new NpgsqlParameter("remark",data.Remark),
                new NpgsqlParameter("reccreator",data.RecCreator),
                new NpgsqlParameter("reccreated",data.RecCreated),
                new NpgsqlParameter("recstatus",data.RecStatus),
                new NpgsqlParameter("fieldid",data.FieldId)
            };
            return ExecuteNonQuery(sql, param, tran) > 0;
        }
        /// <summary>
        /// 修改转移方案
        /// </summary>
        /// <param name="data"></param>
        /// <param name="tran"></param>
        /// <returns></returns>
        public bool UpdateTransferScheme(TransferSchemeModel data, DbTransaction tran)
        {
            #region sql
            string sql = @"UPDATE crm_sys_transfer_scheme
SET recname =@transschemename, 
entityid =@targettransferid,
association =@associationtransfer::jsonb, 
remark =@remark,
reccreator =@reccreator, 
reccreated =@reccreated, 
recstatus =@recstatus,
fieldid=@fieldid
WHERE	recid =@transschemeid";
            #endregion
            var param = new DbParameter[]
            {
                new NpgsqlParameter("transschemeid",data.RecId),
                new NpgsqlParameter("transschemename",data.RecName),
                new NpgsqlParameter("targettransferid",data.EntityId),
                new NpgsqlParameter("associationtransfer",data.Association),
                new NpgsqlParameter("remark",data.Remark),
                new NpgsqlParameter("reccreator",data.RecCreator),
                new NpgsqlParameter("reccreated",data.RecCreated),
                new NpgsqlParameter("recstatus",data.RecStatus),
                new NpgsqlParameter("fieldid",data.FieldId)
            };
            return ExecuteNonQuery(sql, param, tran) > 0;
        }
        /// <summary>
        /// 获取转移方案
        /// </summary>
        /// <param name="TransSchemeId"></param>
        /// <param name="tran"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public TransferSchemeModel GetTransferScheme(Guid TransSchemeId, DbTransaction tran, int userNumber)
        {
            string sql = @"select *  from crm_sys_transfer_scheme where recid = @transschemeid";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("transschemeid",TransSchemeId)
            };
            return ExecuteQuery<TransferSchemeModel>(sql, param, tran).FirstOrDefault();
        }
        /// <summary>
        /// 设置转移方案状态
        /// </summary>
        /// <param name="list"></param>
        /// <param name="status"></param>
        /// <param name="tran"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public bool SetTransferSchemeStatus(List<Guid> list, int status, DbTransaction tran, int userNumber)
        {
            string sql = @"update crm_sys_transfer_scheme set recstatus =@recstatus where recid =ANY(@ids)";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("recstatus",status),
                new NpgsqlParameter("ids",list.ToArray())
            };
            return ExecuteNonQuery(sql, param, tran) > 0;
        }

        public List<Dictionary<string, object>> TransferSchemeList(int recStatus, string searchName, int userNumber)
        {
            string sql = @"select a.recid,a.recname,a.entityid,b.entityname,a.association,a.remark,a.recstatus
  from crm_sys_transfer_scheme as a INNER JOIN  crm_sys_entity as b on a.entityid = b.entityid where a.recStatus = @recStatus";
            if (!string.IsNullOrWhiteSpace(searchName))
            {
                sql += string.Format(" and transschemename like '%{0}%'", searchName);
            }
            sql += " order by a.reccreated desc ";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("recStatus",recStatus)
            };
            return ExecuteQuery(sql, param, null);

        }

        public List<Dictionary<string, object>> ListSchemeByEntity(DbTransaction tran, Guid EntityId, int userNumber)
        {
            string sql = @"select a.recid,a.recname,a.entityid,b.entityname,a.association,a.remark,a.recstatus,a.fieldid
  from crm_sys_transfer_scheme as a INNER JOIN  crm_sys_entity as b on a.entityid = b.entityid where a.entityid = @entityid";
            
            sql += " order by a.reccreated desc ";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("entityid",EntityId)
            };
            return ExecuteQuery(sql, param, tran);
        }
    }
}
