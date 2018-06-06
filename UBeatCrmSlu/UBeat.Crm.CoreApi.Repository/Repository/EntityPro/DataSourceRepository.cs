using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.EntityPro;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Utility;
using System.Linq;
using Newtonsoft.Json;

namespace UBeat.Crm.CoreApi.Repository.Repository.EntityPro
{
    public class DataSourceRepository : RepositoryBase, IDataSourceRepository
    {

        #region 数据源
        public Dictionary<string, List<IDictionary<string, object>>> SelectDataSource(DataSourceListMapper dataSource, int userNumber)
        {
            var procName =
                "SELECT crm_func_datasource_list(@datasourcename,@status,@pageindex,@pagesize,@userno)";

            var dataNames = new List<string> { "PageData", "PageCount" };
            var param = new DynamicParameters();
            param.Add("datasourcename", dataSource.DatasourceName);
            param.Add("pageindex", dataSource.PageIndex);
            param.Add("pagesize", dataSource.PageSize);
            param.Add("status", dataSource.RecStatus);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }

        public OperateResult InsertSaveDataSource(DataSourceMapper dataSource, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_datasource_add(@datasourcename,@srctype,@entityid, @srcmark,@isrelatepower, @status,@ispro, @userno)
            ";
            var param = new DynamicParameters();
            param.Add("datasourcename", dataSource.DatasourceName);
            param.Add("srctype", dataSource.SrcType);
            param.Add("entityid", dataSource.EntityId);
            param.Add("srcmark", dataSource.Srcmark);
            param.Add("isrelatepower", dataSource.IsRelatePower);
            param.Add("status", dataSource.RecStatus);
            param.Add("ispro", dataSource.IsPro);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult UpdateSaveDataSource(DataSourceMapper dataSource, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_datasource_edit(@datasourceid,@datasourcename,@srctype,@entityid, @srcmark, @status, @isrelatepower,@ispro,@userno)
            ";
            var param = new DynamicParameters();
            param.Add("datasourceid", dataSource.DatasourceId);
            param.Add("datasourcename", dataSource.DatasourceName);
            param.Add("srctype", dataSource.SrcType);
            param.Add("entityid", dataSource.EntityId);
            param.Add("srcmark", dataSource.Srcmark);
            param.Add("status", dataSource.RecStatus);
            param.Add("isrelatepower", dataSource.IsRelatePower);
            param.Add("ispro", dataSource.IsPro);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public Dictionary<string, List<IDictionary<string, object>>> SelectDataSourceDetail(DataSourceDetailMapper dataSource, int userNumber)
        {
            var procName =
                "SELECT crm_func_datasourcedetail_list(@datasrcid,@userno)";

            var dataNames = new List<string> { "DataSourceDetail" };
            var param = new DynamicParameters();
            param.Add("datasrcid", dataSource.DatasourceId);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }

        public OperateResult InsertSaveDataSourceDetail(InsertDataSourceConfigMapper dataSource, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_datasourcedetail_add(@datasourceid,@rulesql, @viewstyleid,@fieldkeys,@fonts,@colors, @userno)
            ";
            var param = new DynamicParameters();
            param.Add("datasourceid", dataSource.DataSourceId);
            param.Add("rulesql", dataSource.RuleSql);
            param.Add("viewstyleid", dataSource.ViewStyleId);
            param.Add("fieldkeys", dataSource.ColNames);
            param.Add("fonts", dataSource.Fonts);
            param.Add("colors", dataSource.Colors);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult UpdateSaveDataSourceDetail(UpdateDataSourceConfigMapper dataSource, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_datasourcedetail_edit(@configid,@rulesql,@viewstyleid, @fieldkeys,@fonts,@colors, @userno)
            ";
            var param = new DynamicParameters();
            param.Add("configid", dataSource.DataConfigId);
            param.Add("rulesql", dataSource.RuleSql);
            param.Add("viewstyleid", dataSource.ViewStyleId);
            param.Add("fieldkeys", dataSource.ColNames);
            param.Add("fonts", dataSource.Fonts);
            param.Add("colors", dataSource.Colors);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult DataSourceDelete(DataSrcDeleteMapper dataSource, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_datasource_delete(@datasrcid, @userno)
            ";
            var param = new DynamicParameters();
            param.Add("datasrcid", dataSource.DataSrcId);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }
        #endregion

        #region 单选 多选
        public List<Dictionary<string, object>> SelectFieldDicType(int userNumber, string dicTypeId = "")
        {
            string sql = @"select a.dictypeid,a.dictypename,a.relatedictypeid,b.dictypename as relatedictypname  from crm_sys_dictionary_type as a left join
 crm_sys_dictionary_type as b on a.relatedictypeid = b.dictypeid where a.recstatus = 1 ";
            if (!string.IsNullOrEmpty(dicTypeId))
                sql += string.Format(" and a.dictypeid <> {0}", dicTypeId);
            sql += " order by a.recorder";
            return ExecuteQuery(sql, new DbParameter[] { }, null);
        }

        public Dictionary<string, object> SelectFieldDicTypeDetail(string dicTypeId, int userNumber)
        {
            string sql = @"select dictypeid,dictypename,relatedictypeid,fieldconfig from crm_sys_dictionary_type where recstatus = 1 and dictypeid::text = @dictypeid";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("dictypeid",dicTypeId)
            };
            return ExecuteQuery(sql, param, null).FirstOrDefault();
        }

        public Dictionary<string, object> SelectFieldConfig(string dicTypeId, int userNumber)
        {
            string sql = @"select fieldconfig from crm_sys_dictionary_type where dictypeid::text = @dictypeid";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("dictypeid",dicTypeId)
            };
            return ExecuteQuery(sql, param, null).FirstOrDefault();
        }

        public DicTypeDataModel HasParentDicType(int dicTypeId)
        {
            string sql = @"select relatedictypeid,fieldconfig from crm_sys_dictionary_type where dictypeid = @dictypeid ";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("dictypeid",dicTypeId)
            };
            var data = ExecuteQuery(sql, param, null).FirstOrDefault();
            var dicType = new DicTypeDataModel { FieldConfig = data["fieldconfig"], RelateDicTypeId = data["relatedictypeid"] == null ? "" : data["relatedictypeid"].ToString() };
            return dicType;
        }

        public List<DictionaryDataModel> SelectFieldDicVaue(int dicTypeId, int userNumber)
        {
            string sql = @"select dictypeid,dataid,dataval,relatedataid,extfield1,extfield2,extfield3,extfield4,extfield5 from crm_sys_dictionary where dictypeid = @dictypeid";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("dictypeid",dicTypeId)
            };
            return ExecuteQuery<DictionaryDataModel>(sql, param, null);
        }

        public bool HasDicTypeName(string name)
        {
            string sql = @"select dictypeid from crm_sys_dictionary_type where dictypename = @dictypename";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("dictypename",name)
            };
            return ExecuteScalar(sql, param, null) == null;
        }

        public bool AddFieldDicType(DictionaryTypeMapper entity, int userNumber)
        {
            string sql = @"insert into crm_sys_dictionary_type(dictypeid, dictypename, reccreator, recupdator, dicremark, fieldconfig, RelateDicTypeId,RecOrder) values
     (@dictypeid::int4, @dictypename, @reccreator, @recupdator, @dicremark, @fieldconfig::jsonb, @RelateDicTypeId,@RecOrder::int4)";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("dictypeid", entity.DicTypeId),
                new NpgsqlParameter("dictypename", entity.DicTypeName),
                new NpgsqlParameter("reccreator",userNumber),
                new NpgsqlParameter("recupdator", userNumber),
                new NpgsqlParameter("dicremark", entity.DicRemark),
                new NpgsqlParameter("fieldconfig", entity.FieldConfig),
                new NpgsqlParameter("RelateDicTypeId", entity.RelateDicTypeId),
                new NpgsqlParameter("RecOrder",entity.RecOrder)
            };
            return ExecuteNonQuery(sql, param, null) > 0;
        }

        public bool UpdateFieldDicType(DictionaryTypeMapper entity, int userNumber)
        {
            string sql = @"update crm_sys_dictionary_type set recupdator=@recupdator,  dictypename = @dictypename,dicremark = @dicremark,fieldconfig = @fieldconfig::jsonb,RelateDicTypeId = @RelateDicTypeId,RecOrder=@RecOrder::int4
where dictypeid::text = @dictypeid";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("dictypeid", entity.DicTypeId),
                new NpgsqlParameter("dictypename", entity.DicTypeName),
                new NpgsqlParameter("recupdator", userNumber),
                new NpgsqlParameter("dicremark", entity.DicRemark),
                new NpgsqlParameter("fieldconfig", entity.FieldConfig),
                new NpgsqlParameter("RelateDicTypeId", entity.RelateDicTypeId),
                new NpgsqlParameter("RecOrder",entity.RecOrder)
            };
            return ExecuteNonQuery(sql, param, null) > 0;
        }

        public bool UpdateDicTypeOrder(List<DictionaryTypeMapper> data, int userNumber)
        {
            string sql = "";
            foreach (var item in data)
            {
                sql += string.Format("update crm_sys_dictionary_type set recorder = {0} where dictypeid = {1};/n", item.RecOrder, item.DicTypeId);
            }
            return ExecuteNonQuery(sql, new DbParameter[] { }, null) > 0;
        }

        public string QueryDicId()
        {
            string sql = "select (coalesce(max(dictypeid),0)+1) as dictypeid  from crm_sys_dictionary_type";
            return ExecuteScalar(sql, new DbParameter[] { }, null).ToString();
        }

        public string QueryRecOrder()
        {
            string sql = "select (coalesce(max(recorder),0)+1) as dictypeid  from crm_sys_dictionary_type";
            return ExecuteScalar(sql, new DbParameter[] { }, null).ToString();
        }
    
        public bool UpdateFieldDicTypeStatus(string[] ids, int status, int userNumber)
        {
            string sql = @"update crm_sys_dictionary_type set recstatus = @recstatus where dictypeid::text = ANY( @dictypeid) ";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("recstatus",status),
                new NpgsqlParameter("dictypeid",ids.ToArray())
            };
            return ExecuteNonQuery(sql, param, null) > 0;
        }

        public OperateResult SaveFieldOptValue(DictionaryMapper option, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_fieldopt_value_save(@dicid,@dictypeid,@dataval, @userno)
            ";
            var param = new DynamicParameters();
            param.Add("dicid", option.DicId);
            param.Add("dictypeid", option.DicTypeId);
            param.Add("dataval", option.DataValue);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }
        public OperateResult DisabledDicType(int dicTypeId, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_dictype_disabled(@dictypeid, @userno)
            ";
            var param = new DynamicParameters();
            param.Add("dictypeid", dicTypeId);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }
        public OperateResult DeleteFieldOptValue(string dicId, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_fieldopt_value_delete(@dicid,@status, @userno)
            ";
            var param = new DynamicParameters();
            param.Add("status", 0);
            param.Add("dicid", dicId);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult OrderByFieldOptValue(string dicIds, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_fieldopt_value_orderby(@dicids, @userno)
            ";
            var param = new DynamicParameters();
            param.Add("dicids", dicIds);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public dynamic GetDataSourceInfo(Guid dataSrcId, int userNumber)
        {
            var sql = @"
                SELECT entityid FROM crm_sys_entity_datasource WHERE datasrcid=@datasrcid and recstatus=1 LIMIT 1; 
            ";
            var param = new DynamicParameters();
            param.Add("datasrcid", dataSrcId);
            var result = DataBaseHelper.QuerySingle<dynamic>(sql, param);
            return result;
        }

        #endregion

        #region 动态数据源
        public Dictionary<string, List<IDictionary<string, object>>> DynamicDataSrcQuery(DynamicDataSrcMapper entity, int userNumber)
        {
            var procName = @"
                SELECT    crm_func_business_ds_list(@datasrckey,@keyword,@sqlwhere,@pageindex,@pagesize, @userno)
            ";

            var dataNames = new List<string>();
            if (entity.PageIndex == 1)
            {
                dataNames.Add("DSConfig");
            }
            dataNames.Add("Page");
            dataNames.Add("PageCount");
            var param = new DynamicParameters();
            param.Add("datasrckey", entity.SourceId);
            param.Add("keyword", entity.KeyWord);
            param.Add("sqlwhere", entity.SqlWhere);
            param.Add("pageindex", entity.PageIndex);
            param.Add("pagesize", entity.PageSize);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }
        #endregion



        /// <summary>
        /// 获取实体关联的数据源
        /// </summary>
        /// <param name="entityid"></param>
        /// <returns></returns>
        public List<DataSourceEntityModel> GetEntityDataSources(Guid entityid)
        {
            List<DataSourceEntityModel> resutl = new List<DataSourceEntityModel>();
            var sql = @"SELECT * FROM crm_sys_entity_datasource WHERE entityid=@entityid AND recstatus=1 ";

            var sqlParameters = new List<DbParameter>();
            sqlParameters.Add(new NpgsqlParameter("entityid", entityid));

            using (var conn = DBHelper.GetDbConnect())
            {
                conn.Open();
                var tran = conn.BeginTransaction();
                try
                {
                    resutl = DBHelper.ExecuteQuery<DataSourceEntityModel>(tran, sql, sqlParameters.ToArray());
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return resutl;
        }


        public bool checkDataSourceInUsed(Guid datasourceid, int userNumber, DbTransaction trans)
        {
            try
            {
                string cmdText = string.Format("Select count(*)from  crm_sys_entity_fields where jsonb_extract_path_text(fieldconfig,'dataSource','sourceId') = '{0}' ", datasourceid.ToString());
                object obj = ExecuteScalar(cmdText, new DbParameter[] { }, trans);
                if (obj != null)
                {
                    return int.Parse(obj.ToString()) > 0;
                }
                return false;
            }
            catch (Exception ex)
            {
            }
            return true;
        }

        public Dictionary<string, object> GetDictTypeByName(string dictTypeName)
        {
            try
            {
                string strSQL = "select * from crm_sys_dictionary_type where dictypename =@dictypename";
                DbParameter[] p = new DbParameter[] {
                    new Npgsql.NpgsqlParameter("@dictypename",dictTypeName)
                };
                List<Dictionary<string, object>> retList = ExecuteQuery(strSQL, p, null);
                if (retList == null || retList.Count == 0) return null;
                return retList[0];
            }
            catch (Exception ex) {
                return null;
            }
        }

        public int GetDictValueByName(int dictype, string dictValueName)
        {
            try
            {
                string strSQL = "select * from crm_sys_dictionary where dictypeid = @dictypeid and dataval = @dataval limit 1 ";
                DbParameter[] p = new DbParameter[] {
                    new Npgsql.NpgsqlParameter("@dictypeid",dictype),
                    new Npgsql.NpgsqlParameter("@dataval",dictValueName)
                };
                List<Dictionary<string, object>> retList = ExecuteQuery(strSQL, p, null);
                if (retList == null || retList.Count == 0) return -1;
                return int.Parse(retList[0]["dataid"].ToString());
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        public Dictionary<string, object> GetDataSourceByName(DbTransaction tran, string datasourcename, int userId)
        {
            try
            {
                string strSQL = "select * from crm_sys_entity_datasource  where datasrcname =@datasrcname limit 1 ";
                DbParameter[] p = new DbParameter[] {
                    new Npgsql.NpgsqlParameter("@datasrcname",datasourcename)
                };
                List<Dictionary<string, object>> retList = ExecuteQuery(strSQL, p, null);
                if (retList == null || retList.Count == 0) return null;
                return retList[0];
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public IDictionary<string, object> DynamicDataSrcQueryDetail(string sourceId, Guid recId, int userId)
        {
            var procName = @"
                SELECT    crm_func_business_ds_detail(@datasrckey,@recid, @userno)
            ";

            var dataNames = new List<string>();
            var param = new DynamicParameters();
            param.Add("datasrckey", sourceId);
            param.Add("recid", recId.ToString());
            param.Add("userno", userId);
            Dictionary<string, List<IDictionary<string, object>>> result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            List<IDictionary<string, object>> data = result["data"];
            if (data.Count > 0)
                return data[0];
            return null;
        }
    }
}
