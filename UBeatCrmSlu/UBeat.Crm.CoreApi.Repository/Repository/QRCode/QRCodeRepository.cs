using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.QRCode;
using UBeat.Crm.CoreApi.IRepository;
using System.Linq;
using Newtonsoft.Json;
using Npgsql;

namespace UBeat.Crm.CoreApi.Repository.Repository.QRCode
{
    public class QRCodeRepository : RepositoryBase, IQRCodeRepository
    {
        public Guid Add(DbTransaction tran, string recName, string remark, int userid)
        {
            try
            {
                string strSQL = "select max(recorder) from crm_sys_qrcode_rules where recstatus = 1  ";
                object maxorderobj = ExecuteScalar(strSQL, new DbParameter[] { }, tran);
                int maxcount = 0;
                if (maxorderobj != null && !(maxorderobj is System.DBNull)) {
                    maxcount = int.Parse(maxorderobj.ToString());
                }
                maxcount++;
                strSQL = @"insert into crm_sys_qrcode_rules(recid,recname,remark,recorder,recstatus,reccreator,reccreated,recupdator,recupdated)
values(@recid,@recname,@remark,@recorder,1,@userid,now(),@userid,now())";
                Guid recid = Guid.NewGuid();
                DbParameter[] p = new DbParameter[] {
                    new Npgsql.NpgsqlParameter("@recid",recid),
                    new Npgsql.NpgsqlParameter("@recname",recName),
                    new Npgsql.NpgsqlParameter("@remark",remark),
                    new Npgsql.NpgsqlParameter("@recorder",maxcount),
                    new Npgsql.NpgsqlParameter("@userid",userid)
                };
                ExecuteNonQuery(strSQL, p, tran);
                return recid;

            }
            catch (Exception ex) {
                throw (ex);
            }
        }

        public bool Delete(DbTransaction tran, List<Guid> recId, int userid)
        {
            try
            {
                string strSQL = "delete from crm_sys_qrcode_rules where recid = ANY(@recid)";
                DbParameter[] p = new DbParameter[] {
                    new Npgsql.NpgsqlParameter("@recid",recId)
                };
                ExecuteNonQuery(strSQL, p, tran);
                return true;
            }
            catch (Exception ex) {
                throw (ex);
            }
        }

        public List<Dictionary<string, object>> ExecuteSQL(string strSQL, int userId)
        {
            try
            {
                return ExecuteQuery(strSQL, new DbParameter[] { }, null);
            }
            catch (Exception ex) {
                throw (ex);
            }
        }
        public List<Dictionary<string, object>> ExecuteSQL(string strSQL, DbParameter[] p ,int userId) {
            try
            {
                return ExecuteQuery(strSQL, p, null);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public Dictionary<string, object> GetDealParamInfo(DbTransaction tran, Guid recId, int userid)
        {

            try
            {
                string strSQL = "Select * from crm_sys_qrcode_rules where recid = @recid ";
                DbParameter[] p = new DbParameter[] {
                    new Npgsql.NpgsqlParameter("@recid",recId)
                };
                Dictionary<string, object> item = ExecuteQuery(strSQL, p, tran).FirstOrDefault();
                if (item == null)
                {
                    throw (new Exception("没有找到记录"));
                }
                int dealtype = 0;
                QRCodeDealParamInfo dealParamInfo = null;
                if (item["dealtype"] != null)
                {
                    dealtype = int.Parse(item["dealtype"].ToString());
                }
                if (item["dealparam"] == null)
                {
                    dealParamInfo = new QRCodeDealParamInfo();
                }
                else{
                    if (item["dealparam"] is string)
                    {
                        dealParamInfo = JsonConvert.DeserializeObject<QRCodeDealParamInfo>((string)item["dealparam"]);
                    }
                    else {

                        dealParamInfo = JsonConvert.DeserializeObject<QRCodeDealParamInfo>(JsonConvert.SerializeObject(item["dealparam"]));
                    }
                }
                Dictionary<string, object> retDict = new Dictionary<string, object>();
                retDict.Add("dealtype", dealtype);
                retDict.Add("dealparam", dealParamInfo);
                return retDict;
            }
            catch (Exception ex) {
                throw (ex);
            }
        }

        public QRCodeEntryItemInfo GetFullInfo(DbTransaction tran, Guid recId, int userid)
        {
            string strSQL = "Select * from crm_sys_qrcode_rules where recid = @recid  ";
            DbParameter[] p = new DbParameter[] {
                new NpgsqlParameter("@recid",recId)
            };
            return ExecuteQuery<QRCodeEntryItemInfo>(strSQL, p, tran).FirstOrDefault();
        }

        public Dictionary<string, object> GetMatchParamInfo(DbTransaction tran, Guid recId, int userid)
        {
            try
            {
                string strSQL = "Select * from crm_sys_qrcode_rules where recid = @recid ";
                DbParameter[] p = new DbParameter[] {
                    new Npgsql.NpgsqlParameter("@recid",recId)
                };
                Dictionary<string, object> item = ExecuteQuery(strSQL, p, tran).FirstOrDefault();
                if (item == null)
                {
                    throw (new Exception("没有找到记录"));
                }
                int checkType = 0;
                QRCodeCheckMatchParamInfo matchParamInfo = null;
                if (item["checktype"] != null)
                {
                    checkType = int.Parse(item["checktype"].ToString());
                }
                if (item["checkparam"] == null)
                {
                    matchParamInfo = new QRCodeCheckMatchParamInfo();
                }
                else
                {
                    if (item["checkparam"] is string)
                    {
                        matchParamInfo = JsonConvert.DeserializeObject<QRCodeCheckMatchParamInfo>((string)item["checkparam"]);
                    }
                    else
                    {

                        matchParamInfo = JsonConvert.DeserializeObject<QRCodeCheckMatchParamInfo>(JsonConvert.SerializeObject(item["checkparam"]));
                    }
                }
                Dictionary<string, object> retDict = new Dictionary<string, object>();
                retDict.Add("checktype", checkType);
                retDict.Add("checkparam", matchParamInfo);
                return retDict;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public int GetMaxOrderId(DbTransaction tran, int userid)
        {
            string strSQL = "select max(recorder) from crm_sys_qrcode_rules where recstatus = 1  ";
            object maxorderobj = ExecuteScalar(strSQL, new DbParameter[] { }, tran);
            int maxcount = 0;
            if (maxorderobj != null)
            {
                maxcount = int.Parse(maxorderobj.ToString());
            }
            return maxcount;
        }

        public List<QRCodeEntryItemInfo> ListRules(DbTransaction tran, bool showDisabled, int userid)
        {
            string strSQL = "Select * from crm_sys_qrcode_rules  ";
            if (showDisabled == false) {
                strSQL = strSQL + " Where recstatus = 1 ";
            }
            strSQL = strSQL + " order by recorder ";
            return ExecuteQuery<QRCodeEntryItemInfo>(strSQL, new DbParameter[] { }, tran);
        }

        public bool OrderRules(DbTransaction tran, List<Guid> recids, int userid)
        {
            if (recids == null || recids.Count == 0) return false; 
            string strSQL = @"update crm_sys_qrcode_rules a set recorder  = (
                        select b.recorder 
                        from json_populate_recordset(null::crm_sys_qrcode_rules, @orderdetail::json) b 
                        where b.recid = a.recid
                        )
                        where a.recid in (select c.recid
                        from json_populate_recordset(null::crm_sys_qrcode_rules,@orderdetail::json) c)
                        ";
            List<Dictionary<string, object>> details = new List<Dictionary<string, object>>();
            int index = 1;
            foreach (Guid id in recids) {
                Dictionary<string, object> item = new Dictionary<string, object>();
                item.Add("recid", id);
                item.Add("recorder", index);
                index++;
                details.Add(item);
            }
            DbParameter[] p = new DbParameter[] {
                new NpgsqlParameter("@orderdetail",JsonConvert.SerializeObject(details)){ NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Json}
            };
            ExecuteNonQuery(strSQL, p, tran);
            return true;
        }

        public bool Save(DbTransaction tran, Guid recId, string recName, string remark, int userid)
        {
            try
            {
                string strSQL = "update crm_sys_qrcode_rules set remark = @remark ,recname = @recname where recid = @recid";
                DbParameter[] p = new DbParameter[] {
                    new NpgsqlParameter("@recid",recId),
                    new NpgsqlParameter("@remark",remark),
                    new NpgsqlParameter("@recname",recName)
                };
                ExecuteNonQuery(strSQL, p, tran);
                return true;
            }
            catch (Exception ex) {
                throw (ex);
            }
        }

        public bool SetStatus(DbTransaction tran, List<Guid> recIds, int status, int userid)
        {
            try
            {
                string strSQL = "update crm_sys_qrcode_rules set recstatus = @recstatus,recorder=9999 where recid = ANY(@recids) ";
                DbParameter[] p = new DbParameter[] {
                    new NpgsqlParameter("recids",recIds.ToArray()),
                    new NpgsqlParameter("recstatus",status)
                };
                ExecuteNonQuery(strSQL, p, tran);
                return true;
            }
            catch (Exception ex) {
                throw (ex);
            }
        }

        public bool UpdateDealParamInfo(DbTransaction tran, Guid recid, QRCodeCheckTypeEnum dealType, QRCodeDealParamInfo dealParmInfo, int userid)
        {
            try
            {
                string strSQL = "update crm_sys_qrcode_rules set dealtype=@dealtype,dealparam = @dealparam where recid = @recid ";
                DbParameter[] p = new DbParameter[] {
                    new Npgsql.NpgsqlParameter("@recid",recid),
                    new Npgsql.NpgsqlParameter("@dealparam",JsonConvert.SerializeObject(dealParmInfo)){ NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Json},
                    new Npgsql.NpgsqlParameter("@dealtype",(int)dealType)
                };
                ExecuteNonQuery(strSQL, p, tran);
                return true;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public bool UpdateMatchParamInfo(DbTransaction tran, Guid recId, QRCodeCheckTypeEnum checkType, QRCodeCheckMatchParamInfo checkParam, int userid)
        {
            try
            {
                string strSQL = "update crm_sys_qrcode_rules set checktype=@checktype,checkparam = @checkparam where recid = @recid ";
                DbParameter[] p = new DbParameter[] {
                    new Npgsql.NpgsqlParameter("@recid",recId),
                    new Npgsql.NpgsqlParameter("@checkparam",JsonConvert.SerializeObject(checkParam)){ NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Json},
                    new Npgsql.NpgsqlParameter("@checktype",(int)checkType)
                };
                ExecuteNonQuery(strSQL, p, tran);
                return true;
            }
            catch (Exception ex) {
                throw (ex);
            }
        }
    }
}
