using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.UkQrtz;
using UBeat.Crm.CoreApi.IRepository;
using System.Linq;
using UBeat.Crm.CoreApi.DomainModel;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace UBeat.Crm.CoreApi.Repository.Repository.Qrtz
{
    public class QrtzRepository : RepositoryBase, IQrtzRepository
    {
        private JsonSerializerSettings GetSerializerSettings() {
            JsonSerializerSettings formatterSettings = null;

                formatterSettings = new JsonSerializerSettings();
                formatterSettings.Formatting = Formatting.None;
            formatterSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            return formatterSettings;
            
        }
        public void AddTrigger(TriggerDefineInfo triggerInfo, int userid, DbTransaction tran)
        {
            if (triggerInfo.RecId.Equals(Guid.Empty)) {
                triggerInfo.RecId = Guid.NewGuid();
            }
            try
            {
                string strSQL = @"insert into crm_sys_qrtz_triggerdefine(recid,recname,recstatus,triggertime,actiontype,actioncmd,actionparameters,singlerun)
select recid,recname,0,triggertime,actiontype,actioncmd,actionparameters,singlerun,reclanguage, triggertype, triggerchildtype
from jsonb_populate_recordset(null::crm_sys_qrtz_triggerdefine,@qrtzdata)";
                List<TriggerDefineInfo> l = new List<TriggerDefineInfo>();
                l.Add(triggerInfo);
                ExecuteNonQuery(strSQL, new DbParameter[] { new Npgsql.NpgsqlParameter("@qrtzdata", Newtonsoft.Json.JsonConvert.SerializeObject(l, GetSerializerSettings())) { NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Jsonb } }, tran);
            }
            catch (Exception ex) {
                throw (ex);
            }
        }

        public void AddTriggerInstance(TriggerInstanceInfo instanceInfo, int userid, DbTransaction tran)
        {
            try
            {
                string strSQL = @"INSERT INTO crm_sys_qrtz_triggerinstance(
recid, triggerid, begintime, endtime, status, errormsg, runserver) 
select recid, triggerid, begintime, endtime, status, errormsg, runserver
from jsonb_populate_recordset(null::crm_sys_qrtz_triggerinstance,@qrtzinstance)";
                List<TriggerInstanceInfo> l = new List<TriggerInstanceInfo>();
                l.Add(instanceInfo);
                ExecuteNonQuery(strSQL, new DbParameter[] { new Npgsql.NpgsqlParameter("@qrtzinstance",Newtonsoft.Json.JsonConvert.SerializeObject(l, GetSerializerSettings())) { NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Jsonb } }, tran);
            }
            catch (Exception ex) {
                throw (ex);
            }
        }

        public void DeleteTrigger(Guid recid, int userid, DbTransaction tran)
        {
            try
            {
                string strSQL = "delete from crm_sys_qrtz_triggerdefine where recid = @recid  ";
                ExecuteNonQuery(strSQL, new DbParameter[] { new Npgsql.NpgsqlParameter("@recid", recid) }, tran);
            }
            catch (Exception ex) {
                throw (ex);
            }
            throw new NotImplementedException();
        }

        public List<TriggerDefineInfo> getAllTriggers(bool loadDeleted, int userid, DbTransaction tran)
        {
            try {
                string strSQL = "Select * from crm_sys_qrtz_triggerdefine ";
                if (loadDeleted == false) {
                    strSQL = strSQL + " Where recstatus=1 ";
                }
                strSQL = strSQL + " order by recname";
                return ExecuteQuery<TriggerDefineInfo>(strSQL, new DbParameter[] { }, tran);
            }
            catch(Exception ex)
            {
                throw (ex);
            }
        }

        public TriggerInstanceInfo InstanceDetail(Guid recid, int userid, DbTransaction tran)
        {
            try
            {
                string strSQL = "Select * from crm_sys_qrtz_triggerinstance ";
                
                strSQL = strSQL + " where recid=@recid";
                return ExecuteQuery<TriggerInstanceInfo>(strSQL, new DbParameter[] {new Npgsql.NpgsqlParameter("@recid",recid) }, tran).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public PageDataInfo<TriggerInstanceInfo> ListTriggerInstances(Guid triggerid, string triggername, 
            DateTime dtFrom, DateTime dtTo,bool listArchived, int pageindex, int pagesize, int userid, DbTransaction tran)
        {
            try
            {
                if (dtFrom == DateTime.MinValue) {
                    dtFrom = new DateTime(1900, 1, 1, 0, 0, 0);
                }
                if (dtTo == DateTime.MinValue) {
                    dtTo = new DateTime(2099, 12, 31, 23, 59, 59);
                }
                string instancetable = " crm_sys_qrtz_triggerinstance ";
                if (listArchived) {
                    instancetable = " (select * from crm_sys_qrtz_triggerinstance union all select * from crm_sys_qrtz_triggerinstance_archive) ";
                }
                string strSQL = string.Format(@"select a.* FROM 
                        {0} a
	                        inner join crm_sys_qrtz_triggerdefine b on a.triggerid = b.recid 
                        where b.recname like '%'|| @triggername  || '%' ", instancetable);
              
                strSQL = strSQL + " and  b.recid = @triggerid";
                strSQL = strSQL + " and a.begintime between @fromtime and @totime ";
                strSQL = strSQL + " order by b.recname,a.begintime desc ";
                DbParameter[] param = null;
                param = new DbParameter[] {
                    new Npgsql.NpgsqlParameter("@triggerid", triggerid),
                    new Npgsql.NpgsqlParameter("@triggername", triggername) ,
                    new Npgsql.NpgsqlParameter("@fromtime",dtFrom),
                    new Npgsql.NpgsqlParameter("@totime",dtTo)};
                
                return ExecuteQueryByPaging<TriggerInstanceInfo>(strSQL, param, pagesize, pageindex, tran);
            }
            catch (Exception ex) {
                throw (ex);
            }
        }

        public TriggerDefineInfo TriggerDetail(Guid recid, int userid, DbTransaction tran)
        {
            try
            {
                string strSQL = "select * from crm_sys_qrtz_triggerdefine where recid =@recid";
                return ExecuteQuery<TriggerDefineInfo>(strSQL, new DbParameter[] { new Npgsql.NpgsqlParameter("@recid", recid) }, tran).FirstOrDefault();
            }
            catch (Exception ex) {
                throw (ex);
            }
        }

        public void UpdateTrigger(TriggerDefineInfo triggerInfo, int userid, DbTransaction tran)
        {
            try
            {
                string strSQL = @"update crm_sys_qrtz_triggerdefine as a 
set (recname,recstatus,triggertime,actiontype,actioncmd,actionparameters,
singlerun,remark,inbusy,runningserver,startruntime,
endruntime,errorcount,lasterrortime,reclanguage, triggertype, triggerchildtype)
=(
select recname,recstatus,triggertime,actiontype,actioncmd,actionparameters,
singlerun,remark,inbusy,runningserver,startruntime,
endruntime,errorcount,lasterrortime,reclanguage, triggertype, triggerchildtype
from jsonb_populate_recordset(null::crm_sys_qrtz_triggerdefine,@qrtzdefines)
where recid = @recid)
where recid = @recid ";
                List<TriggerDefineInfo> l = new List<TriggerDefineInfo>();
                l.Add(triggerInfo);
                DbParameter[] p = new DbParameter[] {
                    new Npgsql.NpgsqlParameter("@recid",triggerInfo.RecId),
                    new Npgsql.NpgsqlParameter("@qrtzdefines",Newtonsoft.Json.JsonConvert.SerializeObject(l,GetSerializerSettings())){ NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Jsonb }
                };
                ExecuteNonQuery(strSQL, p, tran);
            }
            catch (Exception ex) {
                throw (ex);
            }
        }

        public void UpdateTriggerInstance(TriggerInstanceInfo instanceInfo, int userid, DbTransaction tran)
        {
            try
            {
                List<TriggerInstanceInfo> l = new List<TriggerInstanceInfo>();
                l.Add(instanceInfo);
                string strSQL = @"update crm_sys_qrtz_triggerinstance as a 
set (triggerid,begintime,endtime,status,errormsg,runserver)
=(select triggerid,begintime,endtime,status,errormsg,runserver
from jsonb_populate_recordset(null::crm_sys_qrtz_triggerinstance,@qrtzinstances)
LIMIT 1 )
where recid = @recid ";
                DbParameter[] p = new DbParameter[] {
                    new Npgsql.NpgsqlParameter("@recid",instanceInfo.RecId),
                    new Npgsql.NpgsqlParameter("@qrtzinstances",Newtonsoft.Json.JsonConvert.SerializeObject(l,GetSerializerSettings())){ NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Jsonb }
                };
                ExecuteNonQuery(strSQL, p, tran);
            }
            catch (Exception ex) {
                throw (ex);
            }
        }
        public List<TriggerDefineInfo> ListNeedCheckTriggers(int userid, DbTransaction tran) {
            try
            {
                string strSQL = @"select * 
from crm_sys_qrtz_triggerdefine 
where ( inbusy = 0   or inbusy is null) and recstatus = 1 
order by recname ";
                return ExecuteQuery<TriggerDefineInfo>(strSQL, new DbParameter[] { }, tran);
            }
            catch (Exception ex) {
                throw (ex);
            }
        }

        public void ExecuteSQL(string sql, int userid, DbTransaction tran)
        {
            try
            {
                ExecuteNonQuery(sql, new DbParameter[] { }, tran);
            }
            catch (Exception ex) {
                throw (ex);
            }
        }

        public List<TriggerDefineInfo> ListNeedArchiveTriggers(int fireMax,int userid, DbTransaction tran)
        {
            try
            {
                string strSQL = @"
select a.*
from crm_sys_qrtz_triggerdefine a	
inner join (
	select triggerid ,count(*) totalcount 
	from crm_sys_qrtz_triggerinstance
	group by triggerid 
) b on a.recid = b.triggerid 
where b.totalcount >=@maxnum";
                return ExecuteQuery<TriggerDefineInfo>(strSQL, new DbParameter[] { new Npgsql.NpgsqlParameter("@maxnum",fireMax) }, tran);
            }
            catch (Exception ex) {
                
            }
            return null;
        }

        public void ArchiveInstances(Guid recId, int maxInstancesCount, int userid, DbTransaction tran)
        {
            try
            {
                string strSQL = @"
insert into crm_sys_qrtz_triggerinstance_archive
select * 
from crm_sys_qrtz_triggerinstance 
where  triggerid =@triggerid
order by begintime asc 
limit (select count(*) 
		from crm_sys_qrtz_triggerinstance
			where triggerid =@triggerid) - @maxcount;

delete from crm_sys_qrtz_triggerinstance 
where triggerid = @triggerid
and recid in (
select recid from crm_sys_qrtz_triggerinstance_archive
where  triggerid = @triggerid
);";
                DbParameter[] p = new DbParameter[] {
                    new Npgsql.NpgsqlParameter("@triggerid",recId),
                    new Npgsql.NpgsqlParameter("@maxcount",maxInstancesCount)

                };
                ExecuteNonQuery(strSQL, p, tran);

            }
            catch (Exception ex) {
            }
        }

        public List<TriggerDefineInfo> ListDeadTriggers(string serverid, int userid, DbTransaction tran) {
            try
            {
                string strSQL = @"select * 
                        from crm_sys_qrtz_triggerdefine  
                        where recstatus = 1 and  runningserver = @serverid ";
                DbParameter[] p = new DbParameter[] {
                    new Npgsql.NpgsqlParameter("serverid",serverid)
                };
                return ExecuteQuery<TriggerDefineInfo>(strSQL, p, tran);
            }
            catch (Exception ex) {
                return new List<TriggerDefineInfo>();
            }

        }
        public void ClearTiggerRunningStatus(DbTransaction tran, Guid triggerid, string serverid, int userid)
        {
            try
            {
                //先清除运行实例，然后才重置事务定义
                string strSQL = @"update crm_sys_qrtz_triggerinstance  set status =2 where triggerid = @triggerid and status = 1 and runserver = @runserver";
                DbParameter[] p = new DbParameter[] {
                    new Npgsql.NpgsqlParameter("@triggerid",triggerid),
                    new Npgsql.NpgsqlParameter("@runserver",serverid)
                };
                ExecuteNonQuery(strSQL, p, tran);

                strSQL = @"update  crm_sys_qrtz_triggerdefine set inbusy = 0 , runningserver = null 
                        where recid = @recid ";
                p = new DbParameter[] {
                    new Npgsql.NpgsqlParameter("@recid",triggerid)
                };
                ExecuteNonQuery(strSQL, p, tran);
            }
            catch (Exception ex) {

            }
        }
        public PageDataInfo<TriggerDefineInfo> ListTriggers(string SearchKey, bool LoadNormal, bool LoadStop, bool LoadDeleted, int PageIndex, int PageSize, int userid, DbTransaction tran)
        {
            try
            {

                string statusString = "-1";
                if (LoadDeleted) statusString = statusString + ",2";
                if (LoadStop) statusString = statusString + ",0";
                if (LoadNormal) statusString = statusString + ",1";
                string strSQL = string.Format(@"
select * 
from crm_sys_qrtz_triggerdefine
where recname like '%'|| @searchkey||'%'
and recstatus in ({0})
order by recname 
                ", statusString);
                DbParameter[] p = new DbParameter[] {
                    new Npgsql.NpgsqlParameter("@searchkey",SearchKey.Replace("'","''"))
                };
                return ExecuteQueryByPaging<TriggerDefineInfo>(strSQL, p, PageSize, PageIndex, tran);
            }
            catch (Exception ex) {
                throw (ex);
            }
            return null;
        }
    }

}
