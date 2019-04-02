using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Excels;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Utility;

namespace UBeat.Crm.CoreApi.Repository.Repository.Excels
{
    public class ExcelRepository : RepositoryBase, IExcelRepository
    {

        public OperateResult AddExcel(AddExcelDomainModel data)
        {
            var executeSql = "select * from crm_func_excel_insert(@templateid,@entityid,@businessname,@funcname,@remark,@excelname,@template,@userno)";
            var args = new DynamicParameters();
            args.Add("templateid", data.ExcelTemplateId);
            args.Add("entityid", data.Entityid);
            args.Add("businessname", data.BusinessName);
            args.Add("funcname", data.FuncName);
            args.Add("remark", data.Remark);
            args.Add("excelname", data.ExcelName);
            args.Add("template", data.TemplateContent);
            args.Add("userno", data.UserNo);
            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }

        public OperateResult DeleteExcel(DeleteExcelDomainModel data)
        {
            var executeSql = "select * from crm_func_excel_delete(@recId,@userno)";
            var args = new DynamicParameters();
            args.Add("recId", data.RecId);
            args.Add("userno", data.UserNo);
            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }

        public dynamic SelectExcels(PageParam pageParam, ExcelSelectDomainModel data)
        {
            var executeSql = "select * from  crm_func_excel_select(@entityid,@userno,@pageindex,@pagesize)";
            var dataNames = new List<string> { "PageData", "PageCount" };
            var args = new
            {
                entityid = data.Entityid,
                userno = data.UserNo,
                pageindex = pageParam.PageIndex,
                pagesize = pageParam.PageSize
            };
            return DataBaseHelper.QueryStoredProcCursor(executeSql, dataNames, args, CommandType.Text);


        }

        public OperateResult SaveExcelTemplate(ExcelTemplateModel data)
        {
            var executeSql = "select * from crm_func_excel_template_save(@templateid,@excelname,@templatecontent,@userno)";
            var args = new
            {
                templateid = data.ExcelTemplateId,
                excelname = data.ExcelName,
                templatecontent = data.TemplateContent,
                userno = data.UserNo
            };
            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }

        public ExcelTemplateModel SelectExcelTemplate(string funcname)
        {
            var executeSql = "select * from crm_func_excel_template_select(@funcname)";
            var args = new
            {
                funcname = funcname
            };
            return DataBaseHelper.QuerySingle<ExcelTemplateModel>(executeSql, args);
        }

        public OperateResult ImportRowData(DbTransaction tran, ImportRowDomainModel data)
        {

            var rowargs = new List<DbParameter>();
            //添加Excel导入的参数数据
            foreach (var column in data.DataRow)
            {
                rowargs.Add(new NpgsqlParameter(column.Key, column.Value));
            }
            if (data.DefaultParameters != null)
            {
                //添加不包含与Excel的默认参数数据
                foreach (var defaultColumn in data.DefaultParameters)
                {
                    if (defaultColumn.Key == "userno")
                        throw new Exception("userno为内置参数，不需传值");
                    rowargs.Add(new NpgsqlParameter(defaultColumn.Key, defaultColumn.Value));
                }
            }
            rowargs.Add(new NpgsqlParameter("operatetype", data.OperateType));
            rowargs.Add(new NpgsqlParameter("userno", data.UserNo));

            return ExecuteQuery<OperateResult>(data.Sql, rowargs.ToArray(), tran).FirstOrDefault();

        }


        public List<Dictionary<string, object>> ExportData(ExportDataDomainModel data)
        {
            var args = new List<DbParameter>();
            if (data.QueryParameters != null)
            {
                //添加不包含与Excel的默认参数数据
                foreach (var m in data.QueryParameters)
                {
                    if (data.Sql.Contains(m.Key))
                    {
                        if (m.Key == "userno")
                            throw new Exception("userno为内置参数，不需传值");
                        if (m.Key == "rulesql")
                            throw new Exception("rulesql为内置参数，不需传值");
                        args.Add(new NpgsqlParameter(m.Key, m.Value));
                    }

                }
            }
            args.Add(new NpgsqlParameter("userno", data.UserNo));
            args.Add(new NpgsqlParameter("rulesql", data.RuleSql));
            if (data.IsStoredProcCursor)
            {
                return ExecuteQueryRefCursor(data.Sql, args.ToArray()).FirstOrDefault().Value;

            }
            return ExecuteQuery(data.Sql, args.ToArray());

        }


        #region --查询导入的相关字段的id或字典值--
        public string GetEntityName(Guid entityid)
        {
            string entityname = null;
            var executeSql = "SELECT entityname FROM crm_sys_entity WHERE entityid=@_entityid";
            var args = new
            {
                _entityid = entityid
            };
            List<IDictionary<string, object>> result = DataBaseHelper.Query(executeSql, args);
            if (result.Count > 0)
            {
                object value = null;
                if (result.FirstOrDefault().TryGetValue("entityname", out value))
                {
                    entityname = value.ToString();
                }
            }
            return entityname;
        }


        public int GetRecManagerId(string recManagerName, out string errorMsg)
        {
            errorMsg = null;
            var executeSql = "SELECT userid FROM crm_sys_userinfo WHERE username=@managerName AND recstatus=1";
            var args = new
            {
                managerName = recManagerName
            };
            List<IDictionary<string, object>> result = DataBaseHelper.Query(executeSql, args);
            int userid = 0;
            if (result.Count == 0)
            {
                errorMsg = "人员不存在";
            }
            else if (result.Count >= 2)
            {
                errorMsg = "人员姓名在系统有重复";
            }
            else
            {
                object value = null;
                if (result.FirstOrDefault().TryGetValue("userid", out value))
                {
                    if (!int.TryParse(value.ToString(), out userid))
                    {
                        errorMsg = "userid转换失败";
                    }
                }
                else
                {
                    errorMsg = "userid转换失败";
                }
            }
            return userid;

        }

        public List<int> GetRecManagerId(List<string> recManagerNames, out string errorMsg)
        {

            errorMsg = null;
            var executeSql = "SELECT DISTINCT(userid) AS userid FROM crm_sys_userinfo WHERE username in( SELECT unnest(string_to_array(@managerName,','))) AND recstatus=1";
            var args = new
            {
                managerName = string.Join(",", recManagerNames.ToArray())
            };
            List<IDictionary<string, object>> result = DataBaseHelper.Query(executeSql, args);
            List<int> userids = new List<int>();

            if (result.Count == 0)
            {
                errorMsg = "人员不存在";
            }
            else if (result.Count != recManagerNames.Distinct().Count())
            {
                errorMsg = "存在不属于系统的人员姓名";
            }
            else
            {
                foreach (var m in result)
                {
                    object value = null;
                    if (m.TryGetValue("userid", out value))
                    {
                        int userid = 0;
                        if (!int.TryParse(value.ToString(), out userid))
                        {
                            errorMsg = "userid转换失败";
                        }
                        else userids.Add(userid);
                    }
                    else errorMsg = "userid转换失败";
                }
            }

            return userids;
        }

        public string GetAreaRegionId(string regionName, out string errorMsg)
        {
            errorMsg = null;
            string regionId = null;
            //SELECT * FROM crm_sys_region WHERE (replace(fullname,'.', '')  LIKE '%中国.广东省.广州市%' OR fullname LIKE '%中国.广东省.广州市%') AND '中国.广东省.广州市' LIKE '%'||regionname||'%' AND recstatus=1
            var executeSql = "SELECT regionid FROM crm_sys_region WHERE (replace(fullname,'.', '') LIKE '%'||@regionName||'%' OR fullname LIKE '%'||@regionName||'%' ) AND @regionName LIKE '%'||regionname||'%' AND recstatus=1";
            var args = new
            {
                regionName = regionName
            };

            List<IDictionary<string, object>> result = DataBaseHelper.Query(executeSql, args);
            if (result.Count == 0)
            {
                errorMsg = "数据不匹配，请仔细填写";
            }
            else if (result.Count >= 2)
            {
                errorMsg = "存在多条匹配的数据，请仔细填写";
            }
            else
            {
                object value = null;
                if (result.FirstOrDefault().TryGetValue("regionid", out value))
                {
                    regionId = value.ToString();
                }
                else
                {
                    errorMsg = "regionid转换失败";
                }
            }
            return regionId;
        }
        public int GetDictionaryDataId(int dicTypeid, string dicValue, out string errorMsg)
        {
            errorMsg = null;
            var executeSql = "SELECT dataid FROM crm_sys_dictionary WHERE dictypeid=@dicTypeid AND dataval=@dicValue AND recstatus=1";
            var args = new
            {
                dicTypeid = dicTypeid,
                dicValue = dicValue
            };
            List<IDictionary<string, object>> result = DataBaseHelper.Query(executeSql, args);
            int dataid = 0;
            if (result.Count == 0)
            {
                errorMsg = "字典数据不存在";
            }
            else if (result.Count >= 2)
            {
                errorMsg = "存在多条匹配的数据，请仔细填写";
            }
            else
            {
                object value = null;
                if (result.FirstOrDefault().TryGetValue("dataid", out value))
                {

                    if (!int.TryParse(value.ToString(), out dataid))
                    {
                        errorMsg = "dataid转换失败";
                    }
                }
                else
                {
                    errorMsg = "dataid转换失败";
                }
            }
            return dataid;
        }

        public List<int> GetDictionaryDataId(int dicTypeid, List<string> dicValues, out string errorMsg)
        {

            errorMsg = null;
            var executeSql = "SELECT DISTINCT(dataid) AS dataid FROM crm_sys_dictionary WHERE dictypeid=@dicTypeid AND dataval in( SELECT unnest(string_to_array(@datavals,','))) AND recstatus=1";
            var args = new
            {
                dicTypeid = dicTypeid,
                datavals = string.Join(",", dicValues.ToArray())
            };

            List<IDictionary<string, object>> result = DataBaseHelper.Query(executeSql, args);
            List<int> dataidlist = new List<int>();

            if (result.Count == 0)
            {
                errorMsg = "字典数据不存在";
            }
            else if (result.Count != dicValues.Distinct().Count())
            {
                errorMsg = "存在不属于系统的字典数据";
            }
            else
            {
                foreach (var m in result)
                {
                    object value = null;
                    if (m.TryGetValue("dataid", out value))
                    {
                        int dataid = 0;
                        if (!int.TryParse(value.ToString(), out dataid))
                        {
                            errorMsg = "dataid转换失败";
                        }
                        else dataidlist.Add(dataid);
                    }
                    else errorMsg = "dataid转换失败";
                }
            }

            return dataidlist;
        }

        public IDictionary<string, object> GetDataSourceMapDataId(string ruleSql, string namevalue, out string errorMsg)
        {
            errorMsg = null;
            var executeSql = string.Format(@"SELECT * FROM ({0}) AS t WHERE t.name = @namevalue", ruleSql.Trim(';'));
            var args = new
            {
                namevalue = namevalue
            };
            List<IDictionary<string, object>> result = DataBaseHelper.Query(executeSql, args);
            if (result.Count == 0)
            {
                errorMsg = "数据不存在";
            }
            else if (result.Count >= 2)
            {
                errorMsg = "存在多条匹配的数据，请仔细填写";
            }
            return result.FirstOrDefault();
        }

        public List<IDictionary<string, object>> GetDataSourceMapDataId(string ruleSql, List<string> values, out string errorMsg)
        {

            errorMsg = null;
            var executeSql = string.Format(@"SELECT * FROM ({0}) AS t WHERE t.name in( SELECT unnest(string_to_array(@namevalue,',')))", ruleSql.Trim(';'));

            var args = new
            {
                namevalue = string.Join(",", values.ToArray())
            };

            List<IDictionary<string, object>> result = DataBaseHelper.Query(executeSql, args);
            List<object> dataidlist = new List<object>();

            if (result.Count == 0)
            {
                errorMsg = "数据不存在";
            }
            else if (result.Count != values.Distinct().Count())
            {
                errorMsg = "存在不属于系统的数据";
            }
            return result;
        }


        public Guid GetProductId(string namepath, out string errorMsg, Dictionary<string, object> FieldFilters = null)
        {
            errorMsg = null;
            if (string.IsNullOrEmpty(namepath))
            {
                return Guid.Empty;
            }
            var namepathArray = namepath.Split('/');
            var productName = namepathArray.Last();
            var serialPath = string.Join("/", namepathArray.Take(namepathArray.Length - 1));
            var executeSql = string.Empty;
            if (string.IsNullOrEmpty(serialPath))
            {
                executeSql = @"SELECT p.recid,array_to_string( array(SELECT productsetname FROM crm_func_product_serial_tree(s.productsetid, 0) ORDER BY nodepath DESC),',') fullname 
                                FROM crm_sys_product AS p 
                                    INNER JOIN crm_sys_products_series AS s ON s.productsetid=p.productsetid
                                WHERE p.productname=@productname ";
            }
            else
            {
                executeSql = @"SELECT p.recid,
                            array_to_string( array(SELECT productsetname FROM crm_func_product_serial_tree(s.productsetid, 0) ORDER BY nodepath DESC),',') fullname
                            FROM crm_sys_product AS p
                            INNER JOIN crm_sys_products_series AS s ON s.productsetid=p.productsetid
                            WHERE productname=@productname AND  array_to_string( array(SELECT productsetname FROM crm_func_product_serial_tree(s.productsetid, 0) ORDER BY nodepath DESC),'/')=@serialPath
                            ";
            }

            var param = new DbParameter[]
            {
                new NpgsqlParameter("productname", productName),
                new NpgsqlParameter("serialPath",serialPath??"")
            };

            var dataResult = ExecuteQuery(executeSql, param);
            #region 处理过滤条件问题,主要解决产品名称可能重复的问题
            if (FieldFilters != null)
            {
                string includeFilters = "";
                string excludeFilters = "";
                string[] inFilters = null;
                string[] exFilters = null;
                if (FieldFilters.ContainsKey("includefilters"))
                {
                    includeFilters = FieldFilters["includefilters"].ToString();
                    if (inFilters != null && inFilters.Length > 0)
                        inFilters = includeFilters.Split(',', StringSplitOptions.RemoveEmptyEntries);
                }
                if (FieldFilters.ContainsKey("excludefilters"))
                {
                    excludeFilters = FieldFilters["excludefilters"].ToString();
                    if (excludeFilters != null && excludeFilters.Length > 0)
                        exFilters = excludeFilters.Split(',', StringSplitOptions.RemoveEmptyEntries);
                }
                List<Dictionary<string, object>> tmp = dataResult;
                dataResult = new List<Dictionary<string, object>>();
                foreach (Dictionary<string, object> item in tmp)
                {
                    if (item.ContainsKey("fullname") && item["fullname"] != null)
                    {
                        string fullName = item["fullname"].ToString();
                        bool IsMatch = true;
                        if (inFilters != null)
                        {
                            IsMatch = false;
                            foreach (string key in inFilters)
                            {
                                if (fullName.IndexOf(key) >= 0)
                                {
                                    IsMatch = true;
                                    break;
                                }
                            }
                        }
                        if (IsMatch == false) continue;
                        if (exFilters != null)
                        {
                            foreach (string key in exFilters)
                            {
                                if (fullName.IndexOf(key) >= 0)
                                {
                                    IsMatch = false;
                                    break;
                                }
                            }
                        }
                        if (IsMatch)
                        {
                            dataResult.Add(item);
                        }
                    }
                }

            }
            #endregion
            if (dataResult.Count == 0)
            {
                errorMsg = "产品不存在（或者不满足过滤条件）";
                return Guid.Empty;
            }
            else if (dataResult.Count > 1)
            {
                errorMsg = "存在多个同名产品，请使用全路径";
            }
            return new Guid(dataResult.FirstOrDefault().FirstOrDefault().Value.ToString());
        }


        public Guid GetProductSeriesId(string serialPath, out string errorMsg)
        {
            errorMsg = null;
            if (string.IsNullOrEmpty(serialPath))
            {
                return Guid.Empty;
            }
            var executeSql = @"SELECT p.productsetid FROM crm_sys_products_series AS p WHERE array_to_string( array(SELECT productsetname FROM crm_func_product_serial_tree(p.productsetid, 0) ORDER BY nodepath DESC),'/')=@serialPath";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("serialPath",serialPath??"")
            };

            var dataResult = ExecuteQuery(executeSql, param);

            if (dataResult.Count == 0)
            {
                errorMsg = "产品系列不存在";
                return Guid.Empty;
            }
            else if (dataResult.Count > 1)
            {
                errorMsg = "存在多个同名产品系列，请使用全路径";
            }
            return new Guid(dataResult.FirstOrDefault().FirstOrDefault().Value.ToString());
        }

        /// <summary>
        /// 获取部门id
        /// </summary>
        /// <param name="serialPath"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public Guid GetDepartmentId(string departPath, int userno, out string errorMsg)
        {
            errorMsg = null;
            try
            {
                if (string.IsNullOrEmpty(departPath))
                {
                    return Guid.Empty;
                }
                var executeSql = @"SELECT departmentid  FROM crm_func_department_nameparsing(@deptnamepath,@userno)";

                var param = new DbParameter[]
                {
                new NpgsqlParameter("deptnamepath",departPath??""),
                new NpgsqlParameter("userno",userno)
                };

                var dataResult = ExecuteQuery(executeSql, param);

                if (dataResult.Count == 0)
                {
                    errorMsg = "部门不存在";
                    return Guid.Empty;
                }
                else if (dataResult.Count > 1)
                {
                    errorMsg = "存在多个同名部门，请使用全路径";
                }
                return new Guid(dataResult.FirstOrDefault().FirstOrDefault().Value.ToString());
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                return Guid.Empty;
            }
        }


        /// <summary>
        /// 获取销售阶段id
        /// </summary>
        /// <param name="serialPath"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public Guid GetSalesStageId(Guid salestagetypeid, string salestagename, out string errorMsg)
        {
            errorMsg = null;
            if (string.IsNullOrEmpty(salestagename))
            {
                return Guid.Empty;
            }
            var executeSql = @"SELECT salesstageid  FROM crm_sys_salesstage_setting where stagename=@stagename and salesstagetypeid=@salestagetypeid";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("stagename",salestagename),
                new NpgsqlParameter("salestagetypeid",salestagetypeid)
            };

            var dataResult = ExecuteQuery(executeSql, param);

            if (dataResult.Count == 0)
            {
                errorMsg = "销售阶段不存在";
                return Guid.Empty;
            }

            return new Guid(dataResult.FirstOrDefault().FirstOrDefault().Value.ToString());
        }

        #endregion
    }
}
