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
        public Dictionary<string, List<IDictionary<string, object>>> SelectFieldDicType(int userNumber)
        {
            var procName = @"
                SELECT    crm_func_field_dictype_list( @userno)
            ";

            var dataNames = new List<string> { "FieldDicType" };
            var param = new DynamicParameters();
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }
        public Dictionary<string, List<IDictionary<string, object>>> SelectFieldDicVaue(int dicTypeId, int userNumber)
        {
            var procName = @"
                SELECT    crm_func_dictype_value_list(@dictypeid, @userno)
            ";

            var dataNames = new List<string> { "FieldDicTypeValue" };
            var param = new DynamicParameters();
            param.Add("dictypeid", dicTypeId);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }
        public OperateResult SaveFieldDicType(DictionaryTypeMapper option, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_dictype_save(@dictypeid,@dictypename,@dicremark, @userno)
            ";
            var param = new DynamicParameters();
            param.Add("dictypeid", option.DicTypeId);
            param.Add("dictypename", option.DicTypeName);
            param.Add("dicremark", option.DicRemark);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
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
    }
}
