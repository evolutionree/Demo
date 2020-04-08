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
                SELECT * FROM crm_func_datasource_add(@datasourcename,@srctype,@entityid, @srcmark,@isrelatepower, @status,@ispro, @userno,@datasourcename_lang::jsonb)
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
            param.Add("datasourcename_lang", JsonConvert.SerializeObject(dataSource.DatasourceName_Lang));
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult UpdateSaveDataSource(DataSourceMapper dataSource, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_datasource_edit(@datasourceid,@datasourcename,@srctype,@entityid, @srcmark, @status, @isrelatepower,@ispro,@userno,@datasourcelanguage::jsonb)
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
            param.Add("datasourcelanguage", JsonConvert.SerializeObject(dataSource.DatasourceName_Lang));
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
                SELECT * FROM crm_func_datasourcedetail_add(@datasourceid,@rulesql, @viewstyleid,@fieldkeys,@fonts,@colors, @userno,@colnamesobj::jsonb)
            ";
            var param = new DynamicParameters();
            param.Add("datasourceid", dataSource.DataSourceId);
            param.Add("rulesql", dataSource.RuleSql);
            param.Add("viewstyleid", dataSource.ViewStyleId);
            param.Add("fieldkeys", dataSource.ColNames);
            param.Add("fonts", dataSource.Fonts);
            param.Add("colors", dataSource.Colors);
            param.Add("userno", userNumber);
            param.Add("colnamesobj", dataSource.Columns);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult UpdateSaveDataSourceDetail(UpdateDataSourceConfigMapper dataSource, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_datasourcedetail_edit(@configid,@rulesql,@viewstyleid, @fieldkeys,@fonts,@colors, @userno,@colnamesobj::jsonb)
            ";
            var param = new DynamicParameters();
            param.Add("configid", dataSource.DataConfigId);
            param.Add("rulesql", dataSource.RuleSql);
            param.Add("viewstyleid", dataSource.ViewStyleId);
            param.Add("fieldkeys", dataSource.ColNames);
            param.Add("fonts", dataSource.Fonts);
            param.Add("colors", dataSource.Colors);
            param.Add("userno", userNumber);
            param.Add("colnamesobj", dataSource.Columns);
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
        public List<Dictionary<string, object>> SelectFieldDicType(int status, int userNumber, string dicTypeId = "")
        {
            string sql = @"select a.dictypeid,a.dictypename,a.relatedictypeid,b.dictypename as relatedictypname ,a.isconfig,a.recorder,a.recstatus,a.dictypename_lang  from crm_sys_dictionary_type as a left join
 crm_sys_dictionary_type as b on a.relatedictypeid = b.dictypeid where a.dictypeid != -1 and a.recstatus = @recstatus {0} order by a.dictypeid desc;";
            if (!string.IsNullOrEmpty(dicTypeId))
                sql = string.Format(sql, string.Format(" and a.dictypeid <> {0}", dicTypeId));
            else
                sql = string.Format(sql, string.Empty);
            //sql += " order by a.recorder";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("recstatus",status)
            };
            return ExecuteQuery(sql, param, null);
        }

        public Dictionary<string, object> SelectFieldDicTypeDetail(string dicTypeId, int userNumber)
        {
            string sql = @"select dictypeid,dictypename,relatedictypeid,fieldconfig,isconfig,recorder,dictypename_lang from crm_sys_dictionary_type where recstatus = 1 and dictypeid::text = @dictypeid";
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
            string sql = @"select dicid,dictypeid,dataid,dataval,relatedataid,recstatus,recorder,extfield1,extfield2,extfield3,extfield4,extfield5,dataval_lang from crm_sys_dictionary where recstatus = 1 and dictypeid = @dictypeid ";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("dictypeid",dicTypeId)
            };
            var data = ExecuteQuery<DictionaryDataModel>(sql, param, null);
            return data.OrderBy(r => r.RecOrder).ToList();
        }

        public bool HasDicTypeName(string Name)
        {
            string sql = @"select dictypeid from crm_sys_dictionary_type where dictypename = @dictypename";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("dictypename",Name)
            };
            return ExecuteScalar(sql, param, null) == null;
        }

        public bool HasDicDataVal(string Name, string DicTypeId)
        {
            string sql = @"select dictypeid from crm_sys_dictionary where dataval = @dataval and dictypeid::text = @dictypeid";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("dataval",Name),
                new NpgsqlParameter("dictypeid",DicTypeId),
            };
            return ExecuteScalar(sql, param, null) == null;
        }

        public bool AddFieldDicType(DictionaryTypeMapper entity, int userNumber)
        {
            string sql = @"insert into crm_sys_dictionary_type(dictypeid, dictypename, reccreator, recupdator, dicremark, fieldconfig, RelateDicTypeId,RecOrder,isconfig,recstatus,dictypename_lang) values
     (@dictypeid::int4, @dictypename, @reccreator, @recupdator, @dicremark, @fieldconfig::jsonb, @RelateDicTypeId,@RecOrder::int4,@IsConfig,@RecStatus,@dictypelanguage::jsonb)";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("dictypeid", entity.DicTypeId),
                new NpgsqlParameter("dictypename", entity.DicTypeName),
                new NpgsqlParameter("reccreator",userNumber),
                new NpgsqlParameter("recupdator", userNumber),
                new NpgsqlParameter("dicremark", entity.DicRemark),
                new NpgsqlParameter("fieldconfig", entity.FieldConfig),
                new NpgsqlParameter("RelateDicTypeId", entity.RelateDicTypeId),
                new NpgsqlParameter("RecOrder",entity.RecOrder),
                new NpgsqlParameter("IsConfig",entity.IsConfig),
                new NpgsqlParameter("RecStatus",entity.RecStatus),
                new NpgsqlParameter("dictypelanguage",JsonConvert.SerializeObject( entity.DicTypeName_Lang))
            };
            return ExecuteNonQuery(sql, param, null) > 0;
        }

        public bool UpdateFieldDicType(DictionaryTypeMapper entity, int userNumber)
        {
            string sql = @"update crm_sys_dictionary_type set recupdator = @recupdator,  dictypename = @dictypename,dicremark = @dicremark,fieldconfig = @fieldconfig::jsonb,
relatedictypeid = @RelateDicTypeId,recorder = @RecOrder::int4, isconfig = @IsConfig,dictypename_lang=@dictypelanguage::jsonb
where dictypeid::text = @dictypeid";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("dictypeid", entity.DicTypeId),
                new NpgsqlParameter("dictypename", entity.DicTypeName),
                new NpgsqlParameter("recupdator", userNumber),
                new NpgsqlParameter("dicremark", entity.DicRemark),
                new NpgsqlParameter("fieldconfig", entity.FieldConfig),
                new NpgsqlParameter("RelateDicTypeId", entity.RelateDicTypeId),
                new NpgsqlParameter("RecOrder",entity.RecOrder),
                new NpgsqlParameter("IsConfig",entity.IsConfig),
                new NpgsqlParameter("dictypelanguage",JsonConvert.SerializeObject( entity.DicTypeName_Lang))
            };
            return ExecuteNonQuery(sql, param, null) > 0;
        }

        public bool AddDictionary(SaveDictionaryMapper entity, int userNumber)
        {
            string sql = @"insert into crm_sys_dictionary 
(dicid,dictypeid,dataid,dataval,recorder,recstatus,reccreated,recupdated,reccreator,recupdator,relatedataid,extfield1,extfield2,extfield3,extfield4,extfield5,dataval_lang)
values (@dicid,@dictypeid::int4,@dataid,@dataval,@recorder,@recstatus,@reccreated,@recupdated,@reccreator,@recupdator,@relatedataid,@extfield1,@extfield2,@extfield3,@extfield4,@extfield5,@datalanguage::jsonb)";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("dicid",entity.DicId),
                new NpgsqlParameter("dictypeid",entity.DicTypeId),
                new NpgsqlParameter("dataid",entity.DataId),
                new NpgsqlParameter("dataval",entity.DataVal),
                new NpgsqlParameter("recorder",entity.RecOrder),
                new NpgsqlParameter("recstatus",entity.RecStatus),
                new NpgsqlParameter("reccreated",entity.RecCreated),
                new NpgsqlParameter("recupdated",entity.RecUpdated),
                new NpgsqlParameter("reccreator",entity.RecCreator),
                new NpgsqlParameter("recupdator",entity.RecUpdator),
                new NpgsqlParameter("relatedataid",entity.RelateDataId),
                new NpgsqlParameter("extfield1",entity.ExtField1),
                new NpgsqlParameter("extfield2",entity.ExtField2),
                new NpgsqlParameter("extfield3",entity.ExtField3),
                new NpgsqlParameter("extfield4",entity.ExtField4),
                new NpgsqlParameter("extfield5",entity.ExtField5),
                new NpgsqlParameter("datalanguage",JsonConvert.SerializeObject( entity.DataVal_Lang))
            };
            return ExecuteNonQuery(sql, param, null) > 0;
        }

        public bool UpdateDictionary(SaveDictionaryMapper entity, int userNumber)
        {
            string sql = @"update crm_sys_dictionary set dictypeid = @dictypeid::int4,dataid = @dataid, dataval = @dataval,recorder = @recorder, recstatus = @recstatus, recupdated = @recupdated,
recupdator = @recupdator, relatedataid = @relatedataid, extfield1 = @extfield1, extfield2 = @extfield2, extfield3 = @extfield3, extfield4 = @extfield4, extfield5 = @extfield5,dataval_lang=@datalanguage::jsonb
where dicid = @dicid";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("dictypeid",entity.DicTypeId),
                new NpgsqlParameter("dataid",entity.DataId),
                new NpgsqlParameter("dataval",entity.DataVal),
                new NpgsqlParameter("recorder",entity.RecOrder),
                new NpgsqlParameter("recstatus",entity.RecStatus),
                new NpgsqlParameter("recupdated",entity.RecUpdated),
                new NpgsqlParameter("recupdator",entity.RecUpdator),
                new NpgsqlParameter("relatedataid",entity.RelateDataId),
                new NpgsqlParameter("extfield1",entity.ExtField1),
                new NpgsqlParameter("extfield2",entity.ExtField2),
                new NpgsqlParameter("extfield3",entity.ExtField3),
                new NpgsqlParameter("extfield4",entity.ExtField4),
                new NpgsqlParameter("extfield5",entity.ExtField5),
                new NpgsqlParameter("dicid",entity.DicId),
                new NpgsqlParameter("datalanguage",JsonConvert.SerializeObject( entity.DataVal_Lang))
            };
            return ExecuteNonQuery(sql, param, null) > 0;
        }

        public bool UpdateDicTypeOrder(List<DictionaryTypeMapper> data, int userNumber)
        {
            string sql = "";
            foreach (var item in data)
            {
                sql += string.Format("update crm_sys_dictionary_type set recorder = {0} where dictypeid = '{1}';\n ", item.RecOrder, item.DicTypeId);
            }
            return ExecuteNonQuery(sql, new DbParameter[] { }, null) > 0;
        }

        public bool OrderByDictionary(List<OrderByDictionaryMapper> entity, int userNumber)
        {
            string sql = "";
            foreach (var item in entity)
            {
                sql += string.Format("update crm_sys_dictionary set recorder = {0} where dicid = '{1}';\n ", item.RecOrder, item.DicId);
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
            catch (Exception ex)
            {
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

        public int GetNextDataId(DbTransaction tran, string dicTypeId, int userNumber)
        {
            try
            {
                string strSQL = "select max(dataid) dataid  from crm_sys_dictionary where dictypeid = @dictypeid";
                DbParameter[] p = new DbParameter[] {
                    new Npgsql.NpgsqlParameter("@dictypeid",int.Parse(dicTypeId))
                };
                object obj = ExecuteScalar(strSQL, p, tran);
                if (obj == null) return 0;
                else return int.Parse(obj.ToString());
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public Dictionary<string, object> GetDataSourceIsAdd(Guid DataSourceId)
        {
            try
            {
                string sql = @"select d.entityid,e.entityname from crm_sys_entity_datasource d
                                LEFT JOIN crm_sys_entity e on d.entityid = e.entityid
                                where d.datasrcid =@datasrcid";
                var p = new DbParameter[]
                {
                    new NpgsqlParameter("datasrcid",DataSourceId)
                };
                return ExecuteQuery(sql, p).FirstOrDefault();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
