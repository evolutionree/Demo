using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.IRepository;
using System.Data.Common;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Utility;
using UBeat.Crm.CoreApi.DomainModel.DynamicEntity;
using UBeat.Crm.CoreApi.DomainModel.Reports;
using UBeat.Crm.CoreApi.DomainModel.EntityPro;
using Newtonsoft.Json;
using UBeat.Crm.CoreApi.DomainModel.Utility;

namespace UBeat.Crm.CoreApi.Services.Services.ReportDetail
{
    public class ContractSignReportServices : EntityBaseServices
    {
        private static string Contract_EntityId = "239a7c69-8238-413d-b1d9-a0d51651abfa";
        private IDynamicEntityRepository _dynamicEntityRepository;
        private IReportEngineRepository _reportEngineRepository;
        private ITargetAndCompletedReportRepository _targetAndCompletedReportRepository;
        public ContractSignReportServices(IDynamicEntityRepository dynamicEntityRepository
                                            , IReportEngineRepository reportEngineRepository
                                            , ITargetAndCompletedReportRepository targetAndCompletedReportRepository) {
            _dynamicEntityRepository = dynamicEntityRepository;
            _reportEngineRepository = reportEngineRepository;
            _targetAndCompletedReportRepository = targetAndCompletedReportRepository;
        }
        public Dictionary<string, List<Dictionary<string, object>>> getContractSignSummary(
                            Dictionary<string, object> param,
                            Dictionary<string, string> sortby,
                            int pageIndex, int pageCount,
                            int userNum)
        {
            int p_rangetype = 0;
            string p_range = "";
            int p_searchyear = 0;
            string mainTable = "crm_sys_contract";
            string mainTableAlias = "e";
            string personFieldName = "recmanager";
            string DateTimeFieldName = "contractdate";
            string DeptFieldName = "";
            #region 处理传入参数
            if (param == null) throw (new Exception("参数异常"));
            if (param.ContainsKey("@rangetype"))
            {
                p_rangetype = int.Parse(param["@rangetype"].ToString());
            }
            else if (param.ContainsKey("rangetype")) {
                p_rangetype = int.Parse(param["rangetype"].ToString());
            }
            else
            {
                throw (new Exception("参数异常"));
            }
            if(p_rangetype !=1 && p_rangetype != 2){
                throw (new Exception("参数异常"));
            }
            if (param.ContainsKey("@range"))
            {
                p_range = param["@range"].ToString();
            }
            else if (param.ContainsKey("range"))
            {
                p_range = param["range"].ToString();
            }
            else {
                throw (new Exception("参数异常"));
            }
            if (p_range.Length == 0) {
                throw (new Exception("参数异常"));
            }
            if (param.ContainsKey("@searchyear"))
            {
                p_searchyear = int.Parse(param["@searchyear"].ToString());
            }
            else if (param.ContainsKey("searchyear"))
            {
                p_searchyear = int.Parse(param["searchyear"].ToString());
            }
            else
            {
                throw (new Exception("参数异常"));
            }

            if (p_searchyear < 2000 || p_searchyear >= 2050)
            {
                throw (new Exception("参数异常"));
            }
            
            #endregion
            string DateTimeSQL = string.Format(" extract(year from {0}.{1}) = {2} ", mainTableAlias, DateTimeFieldName, p_searchyear);
            string selectSQL = string.Format(@"Select  extract(month from {0}.{1})    contractmonth,sum(e.contractvolume/10000)  contractamount,count(*)  contractcount
                                from    {2} {0}   
                                where {0}.recstatus = 1   ", mainTableAlias, DateTimeFieldName, mainTable);
            #region 处理RuleSQL
            string ruleSQL =  " 1= 1";
            string userRoleRuleSQL = "";
            Guid userDeptInfo = Guid.Empty;
            //先处理目标的数据全系
            UserData userData = GetUserData(userNum, false);

            if (userData != null && userData.AccountUserInfo != null && userData.AccountUserInfo.DepartmentId != null)
            {
                userDeptInfo = userData.AccountUserInfo.DepartmentId;
            }

            //处理角色数据权限 
            userRoleRuleSQL = this._reportEngineRepository.getRuleSQLByUserId(Contract_EntityId, userNum, null);
            if (userRoleRuleSQL != null && userRoleRuleSQL.Length > 0)
            {
                userRoleRuleSQL = RuleSqlHelper.FormatRuleSql(userRoleRuleSQL, userNum, userDeptInfo);
                ruleSQL = ruleSQL + " And e.recid in (" + userRoleRuleSQL + ")";
            }
            #endregion
            string totalSQL = "";
            
            if (p_rangetype == 1)
            {
                //按团队
                
                string subDeptSQL = string.Format("select deptid  from crm_func_department_tree('{0}',1) ", p_range);

                string belongPerson = string.Format("select userid  from crm_sys_account_userinfo_relate where deptid in({0}) ", subDeptSQL);
                string personSql = string.Format("{0}.{1} in ({2})", mainTableAlias, personFieldName, belongPerson);
                totalSQL = string.Format("{0} and {1} and {2} and {3} group by contractmonth order by contractmonth", selectSQL, ruleSQL, DateTimeSQL, personSql);
            }
            else
            {
                //按个人,可多选
                string[] tmp = p_range.Split(',');
                if (tmp.Length == 0) throw (new Exception("参数异常"));
                string personSql = "'" + tmp[0] + "'";
                for (int i = 1; i < tmp.Length; i++) {
                    personSql = personSql + ",'" + tmp[i] + "'";
                }
                personSql = string.Format("{0}.{1} in ({2})", mainTableAlias, personFieldName, personSql);
                totalSQL = string.Format("{0} and {1} and {2} and {3} group by contractmonth   order by contractmonth", selectSQL, ruleSQL, DateTimeSQL, personSql);
            }

            totalSQL = string.Format(@"Select totalmonth.fmonth contractmonth ,
                                            COALESCE(afterall.contractamount,0) contractamount ,
                                            COALESCE(afterall.contractcount,0) contractcount from ({0}) totalmonth left outer join ({1}) afterall on afterall.contractmonth =totalmonth.fmonth order by totalmonth.fmonth  ", this.get12MonthSQL(),totalSQL);
            List<Dictionary<string, object>> retList =_reportEngineRepository.ExecuteSQL(totalSQL, new DbParameter[] { });
            Dictionary<string, List<Dictionary<string, object>>> ret = new Dictionary<string, List<Dictionary<string, object>>>();
            ret.Add("data", retList);
            return ret;
        }
        private string get12MonthSQL() {
            return string.Format(@" select 1 fmonth 
                                    union all select 2 fmonth 
                                    union all select 3 fmonth
                                    union all select 4 fmonth  
                                    union all select 5 fmonth 
                                    union all select 6 fmonth 
                                    union all select 7 fmonth 
                                    union all select 8 fmonth 
                                    union all select 9 fmonth 
                                    union all select 10 fmonth 
                                    union all select 11 fmonth 
                                    union all select 12 fmonth 
                                    ");

        }
        public Dictionary<string, List<Dictionary<string, object>>> getContractSignList(
                            Dictionary<string, object> param,
                            Dictionary<string, string> sortby,
                            int pageIndex, int pageCount,
                            int userNum)
        {
            int p_rangetype = 0;
            string p_range = "";
            int p_searchyear = 0;
            string mainTable = "crm_sys_contract";
            string mainTableAlias = "e";
            string personFieldName = "recmanager";
            string DateTimeFieldName = "contractdate";
            string DeptFieldName = "";
            string ruleSQL = "1=1";
            int p_searchmonth = 0;
            #region 处理传入参数
            if (param == null) throw (new Exception("参数异常"));
            if (param.ContainsKey("@rangetype"))
            {
                p_rangetype = int.Parse(param["@rangetype"].ToString());
            }
            else if (param.ContainsKey("rangetype"))
            {
                p_rangetype = int.Parse(param["rangetype"].ToString());
            }
            else
            {
                throw (new Exception("参数异常"));
            }
            if (p_rangetype != 1 && p_rangetype != 2)
            {
                throw (new Exception("参数异常"));
            }
            if (param.ContainsKey("@range"))
            {
                p_range = param["@range"].ToString();
            }
            else if (param.ContainsKey("range"))
            {
                p_range = param["range"].ToString();
            }
            else
            {
                throw (new Exception("参数异常"));
            }
            if (p_range.Length == 0)
            {
                throw (new Exception("参数异常"));
            }
            if (param.ContainsKey("@searchyear"))
            {
                p_searchyear = int.Parse(param["@searchyear"].ToString());
            }
            else if (param.ContainsKey("searchyear"))
            {
                p_searchyear = int.Parse(param["searchyear"].ToString());
            }
            else
            {
                throw (new Exception("参数异常"));
            }
            if (p_searchyear < 2000 || p_searchyear >= 2050)
            {
                throw (new Exception("参数异常"));
            }
            if (param.ContainsKey("@searchmonth"))
            {
                p_searchmonth = int.Parse(param["@searchmonth"].ToString());
            }
            else if (param.ContainsKey("searchmonth"))
            {
                p_searchmonth = int.Parse(param["searchmonth"].ToString());
            }

            if (p_searchmonth < 0 || p_searchmonth > 12)
            {
                throw (new Exception("参数异常"));
            }
            #endregion
            #region 处理RuleSQL
            ruleSQL = " 1= 1";
            string userRoleRuleSQL = "";
            Guid userDeptInfo = Guid.Empty;
            //先处理目标的数据全系
            UserData userData = GetUserData(userNum, false);

            if (userData != null && userData.AccountUserInfo != null && userData.AccountUserInfo.DepartmentId != null)
            {
                userDeptInfo = userData.AccountUserInfo.DepartmentId;
            }

            //处理角色数据权限 
            userRoleRuleSQL = this._reportEngineRepository.getRuleSQLByUserId(Contract_EntityId, userNum, null);
            if (userRoleRuleSQL != null && userRoleRuleSQL.Length > 0)
            {
                userRoleRuleSQL = RuleSqlHelper.FormatRuleSql(userRoleRuleSQL, userNum, userDeptInfo);
                ruleSQL = ruleSQL + " And e.recid in (" + userRoleRuleSQL + ")";
            }
            #endregion
            string DateTimeSQL = string.Format(" extract(year from {0}.{1}) = {2}  and (extract(month  from {0}.{1}) = {3} or {3} = 0)  ", mainTableAlias, DateTimeFieldName, p_searchyear,p_searchmonth);
            string selectSQL = string.Format(@"Select extract(month from {0}.{1}) contractmonth,sum(e.contractvolume) contractamount,count(*) contractcount
                                from {2} {0} 
                                where 1=1 ", mainTableAlias, DateTimeFieldName, mainTable);
            string baseSQL = @"SELECT
                                contracts.recid,userInfo.userid recmanager_id, userInfo.username recmanager_name,
                                contracts.recname ,customer.recid customer_id, customer.recname customer_name,
                                 department.deptid department_id, department.deptname deptartment_name,
                                 opportunity.recid opportunity_id, opportunity.recname opportunity_name, contracts.contractvolume,
	                            contracts.contractdate,contracts.reccreated
                            FROM
                                (
                                    SELECT
                                        *
                                    FROM
                                        crm_sys_contract e
                                    WHERE
                                        e.recstatus = 1 
                                       and {0}
                                        and  {1}
                                        and  {2}
                                ) contracts
                            LEFT OUTER JOIN crm_sys_userinfo
                                userInfo  ON userInfo.userid = contracts.recmanager
                            left outer join (select * from crm_sys_account_userinfo_relate where recstatus  =1 ) userRelate
                                        on userRelate.userid = userInfo.userid
                            left outer join crm_sys_department department
                                        on department.deptid = userRelate.deptid
                            left outer join crm_sys_customer customer
                                        on customer.recid::text = jsonb_extract_path_text(contracts.customerid, 'id')
                            left outer join crm_sys_opportunity opportunity
                                        on  opportunity.recid::text = jsonb_extract_path_text(contracts.opportunityid, 'id')";
            string totalSQL = "";
            if (p_rangetype == 1)
            {
                //按团队

                string subDeptSQL = string.Format("select deptid  from crm_func_department_tree('{0}',1) ", p_range);

                string belongPerson = string.Format("select userid  from crm_sys_account_userinfo_relate where deptid in({0}) ", subDeptSQL);
                string personSql = string.Format("{0}.{1} in ({2})", mainTableAlias, personFieldName, belongPerson);
                totalSQL = string.Format(@" {0} And {1} And {2} ", ruleSQL, DateTimeSQL, personSql);
            }
            else
            {
                //按个人,可多选
                string[] tmp = p_range.Split(',');
                if (tmp.Length == 0) throw (new Exception("参数异常"));
                string personSql = "'" + tmp[0] + "'";
                for (int i = 1; i < tmp.Length; i++)
                {
                    personSql = personSql + ",'" + tmp[i] + "'";
                }
                personSql = string.Format("{0}.{1} in ({2})", mainTableAlias, personFieldName, personSql);
                totalSQL = string.Format(@" {0} And {1} And {2} ", ruleSQL, DateTimeSQL, personSql);
            }
            totalSQL = totalSQL.Replace("'", "''");
            #region 获取WEB列表显示需要的列信息
            List<DynamicEntityWebFieldMapper> columnsInfo = _dynamicEntityRepository.GetWebFields(Guid.Parse(Contract_EntityId), (int)2, userNum);
            TableComponentInfo tableComponentInfo = new TableComponentInfo();
            tableComponentInfo.FixedX = 0;
            tableComponentInfo.FixedY = 1;

            tableComponentInfo.Columns = new List<TableColumnInfo>();
            foreach (DynamicEntityWebFieldMapper columnInfo in columnsInfo)
            {
                TableColumnInfo retColumnInfo = new TableColumnInfo();
                retColumnInfo.FieldName = columnInfo.FieldName;
                retColumnInfo.CanPaged = true;
                retColumnInfo.CanSorted = 1;
                retColumnInfo.TargetType = 1;
                retColumnInfo.Title = columnInfo.DisplayName;
                retColumnInfo.ControlType = columnInfo.ControlType;
                if (columnInfo.ControlType == 8 || columnInfo.ControlType == 9)
                {
                    try
                    {
                        Dictionary<string, object> tmpDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(columnInfo.FieldConfig);
                        if (tmpDict.ContainsKey("format"))
                        {
                            retColumnInfo.FormatStr = tmpDict["format"].ToString();
                        }

                    }
                    catch (Exception ex)
                    {
                    }
                }
                if ((EntityFieldControlType)columnInfo.ControlType == EntityFieldControlType.DataSourceSingle)
                {
                    if (columnInfo.FieldConfig != null && columnInfo.FieldConfig.Length > 0)
                    {
                        try
                        {
                            Dictionary<string, object> tmp = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(columnInfo.FieldConfig);
                            if (tmp != null && tmp.ContainsKey("dataSource"))
                            {
                                tmp = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(Newtonsoft.Json.JsonConvert.SerializeObject(tmp["dataSource"]));
                                string datasourceid = tmp["sourceId"].ToString();
                                string entityid = this._reportEngineRepository.getEntityIdByDataSourceId(datasourceid, userNum);
                                if (entityid == null) continue;
                                retColumnInfo.LinkScheme = string.Format("/entcomm/{0}/#{1}#", entityid, columnInfo.FieldName.ToLower());
                                retColumnInfo.TargetType = 2;
                            }
                            if (tmp != null && tmp.ContainsKey("multiple"))
                            {
                                int isMulti = 0;
                                int.TryParse(tmp["multiple"].ToString(), out isMulti);
                                retColumnInfo.IsDataSourceMulti = (isMulti == 1);
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
                if (columnInfo.FieldName == "recname" && (retColumnInfo.LinkScheme == null || retColumnInfo.LinkScheme.Length == 0))
                {
                    retColumnInfo.LinkScheme = string.Format("/entcomm/{0}/#recid#", Contract_EntityId);
                    retColumnInfo.TargetType = 2;
                }
                if (isNeedDisplayName(columnInfo.ControlType))
                {
                    retColumnInfo.FieldName = retColumnInfo.FieldName + "_name";
                }
                tableComponentInfo.Columns.Add(retColumnInfo);
            }
            #endregion

            #region 获取Mobile列表显示
            MobileTableDefineInfo mobileTable = null;
            
            //合同的特殊处理
            mobileTable = new MobileTableDefineInfo();
            mobileTable.MainTitleFieldName = "recname";
            mobileTable.SubTitleFieldName = "";
            mobileTable.EntityId = Contract_EntityId;
            mobileTable.DetailColumns = new List<MobileTableFieldDefineInfo>();
            mobileTable.DetailColumns.Add(new MobileTableFieldDefineInfo() { Title = "合同负责人", FieldName = "recmanager_name" });
            mobileTable.DetailColumns.Add(new MobileTableFieldDefineInfo() { Title = "客户名称", FieldName = "customerid_name" });
            mobileTable.DetailColumns.Add(new MobileTableFieldDefineInfo() { Title = "合同金额", FieldName = "contractvolume" });


            #endregion
            Dictionary<string, List<Dictionary<string, object>>> retData = (Dictionary<string, List<Dictionary<string, object>>>)_targetAndCompletedReportRepository.DataList(Contract_EntityId, totalSQL, " recid ", 1, 10000, userNum);

            retData.Add("columns", Newtonsoft.Json.JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(Newtonsoft.Json.JsonConvert.SerializeObject(tableComponentInfo.Columns)));

            if (mobileTable != null)
            {
                List<MobileTableDefineInfo> tm = new List<MobileTableDefineInfo>();
                tm.Add(mobileTable);
                JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
                jsonSerializerSettings.ContractResolver = new LowerCasePropertyNamesContractResolver();
                retData.Add("mobilecolumns", Newtonsoft.Json.JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(Newtonsoft.Json.JsonConvert.SerializeObject(tm, jsonSerializerSettings)));
            }
            return retData;
        }
        private bool isNeedDisplayName(int ctrlType)
        {
            switch ((EntityFieldControlType)ctrlType)
            {
                case EntityFieldControlType.Address:
                case EntityFieldControlType.AreaGroup:
                case EntityFieldControlType.AreaRegion:
                case EntityFieldControlType.DataSourceSingle:
                case EntityFieldControlType.Department:
                case EntityFieldControlType.Location:
                case EntityFieldControlType.PersonSelectSingle:
                case EntityFieldControlType.Product:
                case EntityFieldControlType.RecAudits:
                case EntityFieldControlType.RecCreator:
                case EntityFieldControlType.RecManager:
                case EntityFieldControlType.RecStatus:
                case EntityFieldControlType.RecType:
                case EntityFieldControlType.RecUpdator:
                case EntityFieldControlType.SalesStage:
                case EntityFieldControlType.SelectSingle:
                    return true;
                    break;
            }
            return false;
        }
    }
}
