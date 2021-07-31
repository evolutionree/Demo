using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.IRepository;
using System.Data.Common;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Utility;
using UBeat.Crm.CoreApi.DomainModel.Reports;

namespace UBeat.Crm.CoreApi.Services.Services.ReportDetail
{
    public class SaleForecastServices:EntityBaseServices
    {
        private static string OPP_EntityID = "2c63b681-1de9-41b7-9f98-4cf26fd37ef1";
        private IDynamicEntityRepository _dynamicEntityRepository;
        private IReportEngineRepository _reportEngineRepository;
        public SaleForecastServices(IDynamicEntityRepository dynamicEntityRepository, IReportEngineRepository reportEngineRepository) {
            _dynamicEntityRepository = dynamicEntityRepository;
            _reportEngineRepository = reportEngineRepository;
        }
        /***
         * 用于销售预测报表取数（汇总值），就是销售漏斗
         * 
        */
        public Dictionary<string, List<Dictionary<string, object>>> getSaleForecastForDept(
                            Dictionary<string, object> param,
                            Dictionary<string, string> sortby,
                            int pageIndex, int pageCount,
                            int userNum)
        {
            #region 定义变量
            string DateFieldName = "predeal";
            string p_searchdate = "";
            string p_category = "";
            string p_department = "";
            string PersonSQL = "";
            string RuleSQL = "";
            string DateTimeSQL = "";
            string CategorySQL = "";
            DateTime dtFrom;
            DateTime dtTo;
            #endregion
            #region 获取变量
            p_category = ReportParamsUtils.parseString(param, "category");
            p_department = ReportParamsUtils.parseString(param, "department");
            dtFrom = ReportParamsUtils.parseDateTime(param, "searchfrom");
            if (dtFrom <= DateTime.MinValue)
			{
                dtFrom = new DateTime(DateTime.Now.Year, 1, 1);
			}
            dtTo = ReportParamsUtils.parseDateTime(param, "searchto");
            if (dtTo <= DateTime.MinValue)
            {
                dtTo = new DateTime(DateTime.Now.Year, 12, 31);
            }
            if (p_department == null || p_department.Length == 0) {
                throw (new Exception("参数异常"));
            }
            if (p_category == null || p_category.Length == 0) {
                throw (new Exception("参数异常"));
            }
            if (dtFrom == DateTime.MinValue || dtTo == DateTime.MinValue)
            {
                throw (new Exception("参数异常"));
            }
            if (dtFrom > dtTo) {
                throw (new Exception("参数异常"));
            }
            #endregion
            #region 处理日期问题
            DateTimeSQL = string.Format(@"e.{0} >='{1}' and e.{0} <='{2}' ", DateFieldName, dtFrom.ToString("yyyy-MM-dd 00:00:00"), dtTo.ToString("yyyy-MM-dd 23:59:59"));
            #endregion

            #region 处理部门
            string subDeptSQL = string.Format("select deptid  from crm_func_department_tree('{0}',1) ", p_department);

            string belongPerson = string.Format("select userid  from crm_sys_account_userinfo_relate where deptid in({0}) ", subDeptSQL);
            PersonSQL = string.Format("e.recmanager in ({0})", belongPerson);

            #endregion

            #region 处理数据权限
            RuleSQL = " 1=1 ";
            string userRoleRuleSQL = "";
            Guid userDeptInfo = Guid.Empty;
            //先处理目标的数据全系
            UserData userData = GetUserData(userNum, false);

            if (userData != null && userData.AccountUserInfo != null && userData.AccountUserInfo.DepartmentId != null)
            {
                userDeptInfo = userData.AccountUserInfo.DepartmentId;
            }
            
            //处理角色数据权限 
            userRoleRuleSQL = this._reportEngineRepository.getRuleSQLByUserId(OPP_EntityID, userNum, null);
            if (userRoleRuleSQL != null && userRoleRuleSQL.Length > 0)
            {
                userRoleRuleSQL = RuleSqlHelper.FormatRuleSql(userRoleRuleSQL, userNum, userDeptInfo);
                RuleSQL = RuleSQL + " And e.recid in (" + userRoleRuleSQL + ")";
            }
            
            #endregion
            #region 处理商机类型
            if (p_category == Guid.Empty.ToString())
            {
                CategorySQL = "1=1";
            }
            else {
                CategorySQL = " e.rectype = '" + p_category.Replace("'", "''") + "'";
            }
            #endregion

            #region 最后处理逻辑
            string totalSQL = string.Format(@"SELECT
	                            categoryid,
	                            categoryname,
	                            stagename,
	                            winrate,
                                count(*) oppcount,
	                            SUM (totalAmount)/10000 totalAmount
                            FROM
	                            (
		                            SELECT
			                            CASE
		                            WHEN combinecategory.sourcecategoryid IS NULL THEN
			                            category.categoryid
		                            ELSE
			                            combinecategory.sourcecategoryid
		                            END categoryid,
		                            CASE
	                            WHEN combinecategory.sourcecategoryid IS NULL THEN
		                            category.categoryname
	                            ELSE
		                            combinecategory.combinecategoryname
	                            END categoryname,
	                            salesstage.stagename,
	                            salesstage.winrate,
	                             e.premoney totalAmount,
                                salesstage.winrate * e.premoney forcastAmount
                            FROM
	                            crm_sys_opportunity e
	                            INNER JOIN crm_sys_entity_category category ON category.categoryid = e.rectype
	                            INNER JOIN crm_sys_salesstage_setting salesstage ON e.recstageid = salesstage.salesstageid
	                            LEFT OUTER JOIN crm_sys_combinesalecategory combinecategory ON combinecategory.sourcecategoryid = category.categoryid
                            where 1=1 
                                    and salesstage.winrate > 0
                                    and ({0})
                                    and ({1})
                                    and ({2})
                                    and ({3})
	                            ) totalResult
                            GROUP BY
	                            categoryid,
	                            categoryname,
	                            stagename,
	                            winrate
                            ORDER BY
	                            categoryid,
	                            winrate", RuleSQL, DateTimeSQL, PersonSQL, CategorySQL);
            List<Dictionary<string, object>> data = _reportEngineRepository.ExecuteSQL(totalSQL, new DbParameter[] { });
            Dictionary<string, List<Dictionary<string, object>>> retData = new Dictionary<string, List<Dictionary<string, object>>>();
            Dictionary<string, SaleCategoryInfo> dd = new Dictionary<string, SaleCategoryInfo>();
            List<SaleCategoryInfo> dataList = new List<SaleCategoryInfo>();

            foreach (Dictionary<string, object> item in data) {
                SaleCategoryInfo info = null;
                string categoryid = item["categoryid"].ToString();
                if (dd.ContainsKey(categoryid))
                {
                    info = dd[categoryid];
                }
                else {
                    info = new SaleCategoryInfo();
                    info.CategoryId = categoryid;
                    info.CategoryName = item["categoryname"].ToString();
                    dd.Add(categoryid, info);
                    dataList.Add(info);
                }
                info.Data.Add(item);
            }
            List<Dictionary<string, object>> convertedData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(Newtonsoft.Json.JsonConvert.SerializeObject(dataList));
            retData.Add("data", convertedData);
            #endregion
            return retData;
        }

        
        /***
         * 获取销售预测中的商机列表
         * ***/

        public Dictionary<string, List<Dictionary<string, object>>> getSaleForecastOppList(
                            Dictionary<string, object> param,
                            Dictionary<string, string> sortby,
                            int pageIndex, int pageCount,
                            int userNum)
        {
            #region 定义变量
            string DateFieldName = "predeal";
            string p_searchdate = "";
            string p_category = "";
            string p_department = "";
            string p_clickcategoryid = "";
            string p_clickstagename = "";
            string PersonSQL = "";
            string RuleSQL = "";
            string DateTimeSQL = "";
            string CategorySQL = "";
            string ClickCategorySQL = "";
            string ClickStageNameSQL = "";
            DateTime dtFrom;
            DateTime dtTo;

            #endregion
            #region 获取变量
            p_category = ReportParamsUtils.parseString(param, "category");
            p_department = ReportParamsUtils.parseString(param, "department");
            dtFrom = ReportParamsUtils.parseDateTime(param, "searchfrom");
            dtTo = ReportParamsUtils.parseDateTime(param, "searchto");

            p_clickcategoryid = ReportParamsUtils.parseString(param, "clickcategory");
            p_clickstagename = ReportParamsUtils.parseString(param, "clickstagename");
            if (p_department == null || p_department.Length == 0)
            {
                throw (new Exception("参数异常"));
            }
            if (p_category == null || p_category.Length == 0)
            {
                throw (new Exception("参数异常"));
            }
            if (dtFrom == DateTime.MinValue || dtTo == DateTime.MinValue)
            {
                throw (new Exception("参数异常"));
            }
            if (dtFrom > dtTo)
            {
                throw (new Exception("参数异常"));
            }
            #endregion
            #region 处理日期问题
            
            DateTimeSQL = string.Format(@"e.{0} >='{1}' and e.{0} <='{2}' ", DateFieldName, dtFrom.ToString("yyyy-MM-dd 00:00:00"), dtTo.ToString("yyyy-MM-dd 23:59:59"));
            #endregion

            #region 处理部门
            string subDeptSQL = string.Format("select deptid  from crm_func_department_tree('{0}',1) ", p_department);

            string belongPerson = string.Format("select userid  from crm_sys_account_userinfo_relate where deptid in({0}) ", subDeptSQL);
            PersonSQL = string.Format("e.recmanager in ({0})", belongPerson);

            #endregion
            #region 处理数据权限
            RuleSQL = " 1=1 ";
            string userRoleRuleSQL = "";
            Guid userDeptInfo = Guid.Empty;
            //先处理目标的数据全系
            UserData userData = GetUserData(userNum, false);

            if (userData != null && userData.AccountUserInfo != null && userData.AccountUserInfo.DepartmentId != null)
            {
                userDeptInfo = userData.AccountUserInfo.DepartmentId;
            }

            //处理角色数据权限 
            userRoleRuleSQL = this._reportEngineRepository.getRuleSQLByUserId(OPP_EntityID, userNum, null);
            if (userRoleRuleSQL != null && userRoleRuleSQL.Length > 0)
            {
                userRoleRuleSQL = RuleSqlHelper.FormatRuleSql(userRoleRuleSQL, userNum, userDeptInfo);
                RuleSQL = RuleSQL + " And e.recid in (" + userRoleRuleSQL + ")";
            }

            #endregion
            #region 处理商机类型
            if (p_category == Guid.Empty.ToString())
            {
                CategorySQL = "1=1";
            }
            else
            {
                CategorySQL = " e.rectype = '" + p_category.Replace("'", "''") + "'";
            }
            #endregion
            #region 处理选中的商机类型（可能是合并后的）
            if (p_clickcategoryid == null || p_clickcategoryid.Length == 0)
            {
                ClickCategorySQL = "1=1";
            }
            else {
                ClickCategorySQL = string.Format(@"(CASE
                                        WHEN combinecategory.sourcecategoryid IS NULL THEN
	                                        category.categoryid::text
                                        ELSE
	                                        combinecategory.sourcecategoryid::text
                                        END) = '{0}'", p_clickcategoryid.Replace("'", "''"));
            }
            #endregion
            #region 处理选中的阶段类型
            if (p_clickstagename == null || p_clickstagename.Length == 0) {
                ClickStageNameSQL = "1=1";
            }
            else
            {
                ClickStageNameSQL = string.Format(@" salesstage.stagename ='{0}'", p_clickstagename.Replace("'", "''"));
            }

            #endregion 
            #region 最后处理逻辑
            string totalSQL = string.Format(@"SELECT
	                                    category.categoryid categoryid,category.categoryname categoryname ,
	                                    e.recname oppname, e.recid recid ,
	                                    userInfo.username recmanager_name,userInfo.userid recmanager_id,
	                                    customer.recid customer_id,customer.recname customer_name ,
	                                    e.predeal ,
                                    salesstage.salesstageid,
                                     salesstage.stagename,
                                     salesstage.winrate * 100 winrate,
	                                    e.reccreated,
                                    e.premoney,
                                     (salesstage.winrate * e.premoney) /10000 totalAmount
                                    FROM
	                                    crm_sys_opportunity e
                                    INNER JOIN crm_sys_entity_category category ON category.categoryid = e.rectype
                                    INNER JOIN crm_sys_salesstage_setting salesstage ON e.recstageid = salesstage.salesstageid
                                    LEFT OUTER JOIN crm_sys_combinesalecategory combinecategory ON combinecategory.sourcecategoryid = category.categoryid
                                    left outer join crm_sys_userinfo userInfo on userInfo.Userid =  e.recmanager
                                    left outer join crm_sys_customer customer on customer.recid::text = jsonb_extract_path_text(e.belongcustomer,'id')
                                    WHERE
	                                    1 = 1
                                    AND salesstage.winrate > 0
                                    and ({0})
                                    and ({1})
                                    and ({2})
                                    and ({3})
                                    and ({4})
                                    and ({5})
	                           ", RuleSQL, DateTimeSQL, PersonSQL, CategorySQL,ClickStageNameSQL,ClickCategorySQL);
            List<Dictionary<string, object>> data = _reportEngineRepository.ExecuteSQL(totalSQL, new DbParameter[] { });
            Dictionary<string, List<Dictionary<string, object>>> retData = new Dictionary<string, List<Dictionary<string, object>>>();
            retData.Add("data", data);
            #endregion
            return retData;
        }
        public Dictionary<string, List<Dictionary<string, object>>> getSaleForecastAndTargetForDept(
                           Dictionary<string, object> param,
                           Dictionary<string, string> sortby,
                           int pageIndex, int pageCount,
                           int userNum)
        {
            #region 定义变量
            string DateFieldName = "predeal";
            string p_category = "";
            string p_department = "";
            string PersonSQL = "";
            string RuleSQL = "";
            string DateTimeSQL = "";
            string TargetSumSQL = "";
            DateTime dtFrom;
            DateTime dtTo;
            #endregion
            #region 获取变量
            p_category = ReportParamsUtils.parseString(param, "category");
            p_department = ReportParamsUtils.parseString(param, "department");
            dtFrom = ReportParamsUtils.parseDateTime(param, "searchfrom");
            dtTo = ReportParamsUtils.parseDateTime(param, "searchto");
            if (p_department == null || p_department.Length == 0)
            {
                throw (new Exception("参数异常"));
            }
            if (p_category == null || p_category.Length == 0)
            {
                throw (new Exception("参数异常"));
            }
            if (dtFrom == DateTime.MinValue || dtTo == DateTime.MinValue)
            {
                throw (new Exception("参数异常"));
            }
            if (dtFrom > dtTo)
            {
                throw (new Exception("参数异常"));
            }
            #endregion
            #region 处理日期问题

            DateTimeSQL = string.Format(@"e.{0} >='{1}' and e.{0} <='{2}' ", DateFieldName, dtFrom.ToString("yyyy-MM-dd 00:00:00"), dtTo.ToString("yyyy-MM-dd 23:59:59"));
            #endregion

            #region 处理部门
            string subDeptSQL = string.Format("select deptid  from crm_func_department_tree('{0}',1) ", p_department);

            string belongPerson = string.Format("select userid  from crm_sys_account_userinfo_relate where deptid in({0}) ", subDeptSQL);
            PersonSQL = string.Format("e.recmanager in ({0})", belongPerson);

            #endregion
            #region 处理数据权限
            RuleSQL = " 1=1 ";
            string userRoleRuleSQL = "";
            Guid userDeptInfo = Guid.Empty;
            //先处理目标的数据全系
            UserData userData = GetUserData(userNum, false);

            if (userData != null && userData.AccountUserInfo != null && userData.AccountUserInfo.DepartmentId != null)
            {
                userDeptInfo = userData.AccountUserInfo.DepartmentId;
            }

            //处理角色数据权限 
            userRoleRuleSQL = this._reportEngineRepository.getRuleSQLByUserId(OPP_EntityID, userNum, null);
            if (userRoleRuleSQL != null && userRoleRuleSQL.Length > 0)
            {
                userRoleRuleSQL = RuleSqlHelper.FormatRuleSql(userRoleRuleSQL, userNum, userDeptInfo);
                RuleSQL = RuleSQL + " And e.recid in (" + userRoleRuleSQL + ")";
            }

            #endregion
            #region 最后处理逻辑
            string totalSQL = string.Format(@"
		                    SELECT
                                sum(salesstage.winrate * e.premoney) /10000 forcastAmount
                            FROM
	                            crm_sys_opportunity e
	                            INNER JOIN crm_sys_entity_category category ON category.categoryid = e.rectype
	                            INNER JOIN crm_sys_salesstage_setting salesstage ON e.recstageid = salesstage.salesstageid
	                            LEFT OUTER JOIN crm_sys_combinesalecategory combinecategory ON combinecategory.sourcecategoryid = category.categoryid
                            where 1=1 
                                    and salesstage.winrate > 0
                                    and ({0})
                                    and ({1})
                                    and ({2})
	                          ", RuleSQL, DateTimeSQL, PersonSQL);
            List<Dictionary<string, object>> data = _reportEngineRepository.ExecuteSQL(totalSQL, new DbParameter[] { });
            Decimal forcastAmount = new Decimal(0.00);
            if (data == null || data.Count == 0 || data[0]==null || data[0].ContainsKey("forcastamount") ==false || data[0]["forcastamount"] == null)
            {
                forcastAmount = new Decimal(0.00);
            }
            else {
                Decimal.TryParse(data[0]["forcastamount"].ToString(), out forcastAmount);
            }
            forcastAmount = Math.Round(forcastAmount, 3);
            Dictionary<string, object> groupData = new Dictionary<string, object>();
            groupData.Add("forcastamount", forcastAmount);
            #endregion
            #region 开始处理目标
            //得先找到与商机有关的目标有多少
            string cmdText = string.Format(@"select ({0}) TargetAmount  
                            from crm_sys_sales_target 
                            where normtypeid 
                            in (select normtypeid  from crm_sys_sales_target_norm_type where entityid = '2c63b681-1de9-41b7-9f98-4cf26fd37ef1' and recstatus = 1  and calcutetype = 0 )
                            and isgrouptarget =true 
                            and recstatus = 1 and departmentid='{1}'", TargetSumSQL,p_department.Replace("'","''"));
            data = _reportEngineRepository.ExecuteSQL(cmdText, new DbParameter[] { });
            Decimal TargetAmount = new Decimal(0.00);
            if (data == null || data.Count == 0)
            {
                TargetAmount = new Decimal(0.00);
            }
            else
            {
                Decimal.TryParse(data[0]["targetamount"].ToString(), out TargetAmount);
            }
            groupData.Add("targetamount", TargetAmount);
            #endregion
            List<Dictionary<string, object>> tmp = new List<Dictionary<string, object>>();
            tmp.Add(groupData);
            Dictionary<string, List<Dictionary<string, object>>> retData = new Dictionary<string, List<Dictionary<string, object>>>();
            retData.Add("data", tmp);
            
            return retData;
        }
        /// <summary>
        /// 应用于手机端的销售预测报表（团队），用户点击销售漏斗的某个阶段后调用此数据源，获取销售阶段名称，本阶段的商机预测总额，商机总条数等信息
        /// 当前部门名称（rangename),当前统计周期(staticdate),目标（targetamount），预测总额(completedamount) ,阶段名称（stagename),阶段总预测（stageoppamount)，阶段总商机数（stageoppcount)
        /// 注意可能有合并商机类型的可能性。
        /// </summary>
        /// <param name="param"></param>
        /// <param name="sortby"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageCount"></param>
        /// <param name="userNum"></param>
        /// <returns></returns>
        public Dictionary<string, List<Dictionary<string, object>>> getSaleStageSummary(
            Dictionary<string, object> param,
                            Dictionary<string, string> sortby,
                            int pageIndex, int pageCount,
                            int userNum) {
            #region 定义变量
            string DateFieldName = "predeal";
            string p_searchdate = "";
            string p_category = "";
            string p_department = "";
            string p_clickcategoryid = "";
            string p_clickstagename = "";
            string PersonSQL = "";
            string RuleSQL = "";
            string DateTimeSQL = "";
            string CategorySQL = "";
            DateTime dtFrom;
            DateTime dtTo;
            #endregion
            #region 获取变量
            p_category = ReportParamsUtils.parseString(param, "category");
            p_department = ReportParamsUtils.parseString(param, "department");
            dtFrom = ReportParamsUtils.parseDateTime(param, "searchfrom");
            dtTo = ReportParamsUtils.parseDateTime(param, "searchto");
            p_clickcategoryid = ReportParamsUtils.parseString(param, "clickcategory");
            p_clickstagename = ReportParamsUtils.parseString(param, "clickstagename");
            if (p_department == null || p_department.Length == 0)
            {
                throw (new Exception("参数异常"));
            }
            if (p_category == null || p_category.Length == 0)
            {
                throw (new Exception("参数异常"));
            }
            if (dtFrom == DateTime.MinValue || dtTo == DateTime.MinValue)
            {
                throw (new Exception("参数异常"));
            }
            if (dtFrom > dtTo)
            {
                throw (new Exception("参数异常"));
            }
            #endregion

            #region 处理日期问题


            DateTimeSQL = string.Format(@"e.{0} >='{1}' and e.{0} <='{2}' ", DateFieldName, dtFrom.ToString("yyyy-MM-dd 00:00:00"), dtTo.ToString("yyyy-MM-dd 23:59:59"));
            #endregion

            #region 处理部门
            string subDeptSQL = string.Format("select deptid  from crm_func_department_tree('{0}',1) ", p_department);

            string belongPerson = string.Format("select userid  from crm_sys_account_userinfo_relate where deptid in({0}) ", subDeptSQL);
            PersonSQL = string.Format("e.recmanager in ({0})", belongPerson);

            #endregion
            #region 处理数据权限
            RuleSQL = " 1=1 ";
            string userRoleRuleSQL = "";
            Guid userDeptInfo = Guid.Empty;
            //先处理目标的数据全系
            UserData userData = GetUserData(userNum, false);

            if (userData != null && userData.AccountUserInfo != null && userData.AccountUserInfo.DepartmentId != null)
            {
                userDeptInfo = userData.AccountUserInfo.DepartmentId;
            }

            //处理角色数据权限 
            userRoleRuleSQL = this._reportEngineRepository.getRuleSQLByUserId(OPP_EntityID, userNum, null);
            if (userRoleRuleSQL != null && userRoleRuleSQL.Length > 0)
            {
                userRoleRuleSQL = RuleSqlHelper.FormatRuleSql(userRoleRuleSQL, userNum, userDeptInfo);
                RuleSQL = RuleSQL + " And e.recid in (" + userRoleRuleSQL + ")";
            }

            #endregion
            #region 处理商机类型
            if (p_category == Guid.Empty.ToString())
            {
                CategorySQL = "1=1";
            }
            else
            {
                CategorySQL = " e.rectype = '" + p_category.Replace("'", "''") + "'";
            }
            #endregion

            Dictionary<string, List<Dictionary<string, object>>> retData = new Dictionary<string, List<Dictionary<string, object>>>();
            Dictionary<string, object> firstDataItem = new Dictionary<string, object>();
            #region 处理阶段名称返回
            if (p_clickstagename == null || p_clickstagename.Length == 0)
            {
                firstDataItem.Add("stagename", "全部阶段");
            }
            else
            {
                firstDataItem.Add("stagename", p_clickstagename);
            }
            #endregion
            #region 处理范围名称
            string deptname = "";
            if (p_department != null) {
                string cmdText = "Select deptname from crm_sys_department where deptid = '" + p_department.Replace("'", "''") + "'";
                List<Dictionary<string, object>> tmpList = this._reportEngineRepository.ExecuteSQL(cmdText, new DbParameter[] { });
                if (tmpList != null && tmpList.Count > 0 && tmpList[0]["deptname"] != null) {
                    deptname = tmpList[0]["deptname"].ToString();
                }
            }
            firstDataItem.Add("rangename", deptname);
            #endregion
            #region 处理期间
            firstDataItem.Add("staticdate", p_searchdate);
            #endregion
            #region 处理目标
            #endregion
            #region 计算阶段预测值和阶段商机数量
            Dictionary<string, List<Dictionary<string, object>>> oppListData = this.getSaleForecastOppList(param, sortby, pageIndex, pageCount, userNum);
            Decimal stageAmount = new Decimal(0.00);
            int stageCount = 0;
            if (oppListData != null && oppListData.ContainsKey("data")) {
                List<Dictionary<string, object>> oppList = oppListData["data"];
                if (oppList != null) {
                    foreach (Dictionary<string, object> item in oppList) {
                        if (item.ContainsKey("totalamount") && item["totalamount"] != null) {
                            Decimal tmp = new Decimal(0.00);
                            if (Decimal.TryParse(item["totalamount"].ToString(), out tmp)) {
                                stageAmount = stageAmount + tmp;
                            }
                        }
                        stageCount++;
                    }
                }
            }
            firstDataItem.Add("stageoppamount", stageAmount);
            firstDataItem.Add("stageoppcount", stageCount);
            #endregion

            #region 处理部门总预测值和总目标
            Dictionary<string, List<Dictionary<string, object>>> totalListData = this.getSaleForecastAndTargetForDept(param, sortby, pageIndex, pageCount, userNum);
            Decimal totalTargetAmount = new Decimal(0.00);
            Decimal totalCompletedAmount = new Decimal(0.00);
            if (totalListData != null && totalListData.ContainsKey("data") && totalListData["data"] != null)
            {
                List<Dictionary<string, object>> totalList = totalListData["data"];
                if (totalList.Count > 0)
                {
                    Dictionary<string, object> item = totalList[0];
                    if (item.ContainsKey("forcastamount") && item["forcastamount"] != null) {
                        Decimal tmp = new Decimal(0.00);
                        if (Decimal.TryParse(item["forcastamount"].ToString(), out tmp))
                        {
                            totalCompletedAmount = tmp;
                        }
                    }

                    if (item.ContainsKey("targetamount") && item["targetamount"] != null)
                    {
                        Decimal tmp = new Decimal(0.00);
                        if (Decimal.TryParse(item["targetamount"].ToString(), out tmp))
                        {
                            totalTargetAmount = tmp;
                        }
                    }
                }
            }
            firstDataItem.Add("targetamount", totalTargetAmount);
            firstDataItem.Add("completedamount", totalCompletedAmount);
            #endregion
            List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
            data.Add(firstDataItem);
            retData.Add("data", data);
            return retData;
        }

        /// <summary>
        /// 用于销售预测个人报表，这个部分是漏斗数据
        /// </summary>
        /// <param name="param"></param>
        /// <param name="sortby"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageCount"></param>
        /// <param name="userNum"></param>
        /// <returns></returns>
        public Dictionary<string, List<Dictionary<string, object>>> getSaleForecastForUsers(
                            Dictionary<string, object> param,
                            Dictionary<string, string> sortby,
                            int pageIndex, int pageCount,
                            int userNum)
        {
            #region 定义变量
            string DateFieldName = "predeal";
            string p_category = "";
            string p_users = "";
            string PersonSQL = "";
            string RuleSQL = "";
            string DateTimeSQL = "";
            string CategorySQL = "";
            DateTime dtFrom;
            DateTime dtTo;

            #endregion
            #region 获取变量
            p_category = ReportParamsUtils.parseString(param, "category");
            p_users = ReportParamsUtils.parseString(param, "users");
            dtFrom = ReportParamsUtils.parseDateTime(param, "searchfrom");
            dtTo = ReportParamsUtils.parseDateTime(param, "searchto");
            if (p_category == null || p_category.Length ==0)
            {
                throw (new Exception("参数异常"));
            }
            if (p_users == null || p_users.Length == 0)
            {
                throw (new Exception("参数异常"));
            }
            if (dtFrom == DateTime.MinValue) {
                throw (new Exception("参数异常"));
            }
            if (dtTo == DateTime.MinValue) {
                throw (new Exception("参数异常"));
            }
            if (dtFrom > dtTo) {
                throw (new Exception("参数异常"));
            }
            #endregion
            #region 处理日期问题
            
            DateTimeSQL = string.Format(@"e.{0} >='{1}' and e.{0} <='{2}' ", DateFieldName, dtFrom.ToString("yyyy-MM-dd 00:00:00"), dtTo.ToString("yyyy-MM-dd 23:59:59"));
            #endregion
            #region 处理用户
            PersonSQL = string.Format("e.recmanager in ({0})", p_users);
            #endregion

            #region 处理数据权限
            RuleSQL = " 1=1 ";
            string userRoleRuleSQL = "";
            Guid userDeptInfo = Guid.Empty;
            //先处理目标的数据全系
            UserData userData = GetUserData(userNum, false);

            if (userData != null && userData.AccountUserInfo != null && userData.AccountUserInfo.DepartmentId != null)
            {
                userDeptInfo = userData.AccountUserInfo.DepartmentId;
            }

            //处理角色数据权限 
            userRoleRuleSQL = this._reportEngineRepository.getRuleSQLByUserId(OPP_EntityID, userNum, null);
            if (userRoleRuleSQL != null && userRoleRuleSQL.Length > 0)
            {
                userRoleRuleSQL = RuleSqlHelper.FormatRuleSql(userRoleRuleSQL, userNum, userDeptInfo);
                RuleSQL = RuleSQL + " And e.recid in (" + userRoleRuleSQL + ")";
            }

            #endregion
            #region 处理商机类型
            if (p_category == Guid.Empty.ToString())
            {
                CategorySQL = "1=1";
            }
            else
            {
                CategorySQL = " e.rectype = '" + p_category.Replace("'", "''") + "'";
            }
            #endregion
            #region 最后处理逻辑
            string totalSQL = string.Format(@"SELECT
	                            categoryid,
	                            categoryname,
	                            stagename,
	                            winrate,
                                count(*) oppcount,
	                            SUM (totalAmount)/10000 totalAmount
                            FROM
	                            (
		                            SELECT
			                            CASE
		                            WHEN combinecategory.sourcecategoryid IS NULL THEN
			                            category.categoryid
		                            ELSE
			                            combinecategory.sourcecategoryid
		                            END categoryid,
		                            CASE
	                            WHEN combinecategory.sourcecategoryid IS NULL THEN
		                            category.categoryname
	                            ELSE
		                            combinecategory.combinecategoryname
	                            END categoryname,
	                            salesstage.stagename,
	                            salesstage.winrate,
	                             e.premoney totalAmount,
                                salesstage.winrate * e.premoney forcastAmount
                            FROM
	                            crm_sys_opportunity e
	                            INNER JOIN crm_sys_entity_category category ON category.categoryid = e.rectype
	                            INNER JOIN crm_sys_salesstage_setting salesstage ON e.recstageid = salesstage.salesstageid
	                            LEFT OUTER JOIN crm_sys_combinesalecategory combinecategory ON combinecategory.sourcecategoryid = category.categoryid
                            where 1=1 
                                    and salesstage.winrate > 0
                                    and ({0})
                                    and ({1})
                                    and ({2})
                                    and ({3})
	                            ) totalResult
                            GROUP BY
	                            categoryid,
	                            categoryname,
	                            stagename,
	                            winrate
                            ORDER BY
	                            categoryid,
	                            winrate", RuleSQL, DateTimeSQL, PersonSQL, CategorySQL);
            List<Dictionary<string, object>> data = _reportEngineRepository.ExecuteSQL(totalSQL, new DbParameter[] { });
            Dictionary<string, List<Dictionary<string, object>>> retData = new Dictionary<string, List<Dictionary<string, object>>>();
            Dictionary<string, SaleCategoryInfo> dd = new Dictionary<string, SaleCategoryInfo>();
            List<SaleCategoryInfo> dataList = new List<SaleCategoryInfo>();

            foreach (Dictionary<string, object> item in data)
            {
                SaleCategoryInfo info = null;
                string categoryid = item["categoryid"].ToString();
                if (dd.ContainsKey(categoryid))
                {
                    info = dd[categoryid];
                }
                else
                {
                    info = new SaleCategoryInfo();
                    info.CategoryId = categoryid;
                    info.CategoryName = item["categoryname"].ToString();
                    dd.Add(categoryid, info);
                    dataList.Add(info);
                }
                info.Data.Add(item);
            }
            List<Dictionary<string, object>> convertedData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(Newtonsoft.Json.JsonConvert.SerializeObject(dataList));
            retData.Add("data", convertedData);
            #endregion
            return retData;
        }
        /// <summary>
        /// 用于销售预测表（个人），返回商机列表，兼容点击和未点击
        /// </summary>
        /// <param name="param"></param>
        /// <param name="sortby"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageCount"></param>
        /// <param name="userNum"></param>
        /// <returns></returns>
        public Dictionary<string, List<Dictionary<string, object>>> getSaleForecastOppListForUsers(
                            Dictionary<string, object> param,
                            Dictionary<string, string> sortby,
                            int pageIndex, int pageCount,
                            int userNum)
        {
            #region 定义变量
            string DateFieldName = "predeal";
            string p_category = "";
            string p_users = "";
            string p_clickcategoryid = "";
            string p_clickstagename = "";
            string PersonSQL = "";
            string RuleSQL = "";
            string DateTimeSQL = "";
            string CategorySQL = "";
            string ClickCategorySQL = "";
            string ClickStageNameSQL = "";
            DateTime dtFrom;
            DateTime dtTo;

            #endregion
            #region 获取变量
            p_category = ReportParamsUtils.parseString(param, "category");
            p_users = ReportParamsUtils.parseString(param, "users");
            p_clickcategoryid = ReportParamsUtils.parseString(param, "clickcategory");
            p_clickstagename = ReportParamsUtils.parseString(param, "clickstagename");
            dtFrom = ReportParamsUtils.parseDateTime(param, "searchfrom");
            dtTo = ReportParamsUtils.parseDateTime(param, "searchto");
            if (p_users == null || p_users.Length == 0)
            {
                throw (new Exception("参数异常"));
            }
            if (p_category == null || p_category.Length == 0)
            {
                throw (new Exception("参数异常"));
            }
            if (dtFrom == DateTime.MinValue || dtTo == DateTime.MinValue || dtFrom > dtTo)
            {
                throw (new Exception("参数异常"));
            }

            #endregion
            #region 处理日期问题
            
            DateTimeSQL = string.Format(@"e.{0} >='{1}' and e.{0} <='{2}' ", DateFieldName, dtFrom.ToString("yyyy-MM-dd 00:00:00"), dtTo.ToString("yyyy-MM-dd 23:59:59"));
            #endregion

            #region 处理人员
            PersonSQL = string.Format("e.recmanager in ({0})", p_users);

            #endregion
            #region 处理数据权限
            RuleSQL = " 1=1 ";
            string userRoleRuleSQL = "";
            Guid userDeptInfo = Guid.Empty;
            //先处理目标的数据全系
            UserData userData = GetUserData(userNum, false);

            if (userData != null && userData.AccountUserInfo != null && userData.AccountUserInfo.DepartmentId != null)
            {
                userDeptInfo = userData.AccountUserInfo.DepartmentId;
            }

            //处理角色数据权限 
            userRoleRuleSQL = this._reportEngineRepository.getRuleSQLByUserId(OPP_EntityID, userNum, null);
            if (userRoleRuleSQL != null && userRoleRuleSQL.Length > 0)
            {
                userRoleRuleSQL = RuleSqlHelper.FormatRuleSql(userRoleRuleSQL, userNum, userDeptInfo);
                RuleSQL = RuleSQL + " And e.recid in (" + userRoleRuleSQL + ")";
            }

            #endregion
            #region 处理商机类型
            if (p_category == Guid.Empty.ToString())
            {
                CategorySQL = "1=1";
            }
            else
            {
                CategorySQL = " e.rectype = '" + p_category.Replace("'", "''") + "'";
            }
            #endregion
            #region 处理选中的商机类型（可能是合并后的）
            if (p_clickcategoryid  == null ||  p_clickcategoryid.Length == 0)
            {
                ClickCategorySQL = "1=1";
            }
            else
            {
                ClickCategorySQL = string.Format(@"(CASE
                                        WHEN combinecategory.sourcecategoryid IS NULL THEN
	                                        category.categoryid::text
                                        ELSE
	                                        combinecategory.sourcecategoryid::text
                                        END) = '{0}'", p_clickcategoryid.Replace("'", "''"));
            }
            #endregion
            #region 处理选中的阶段类型
            if (p_clickstagename == null || p_clickstagename.Length == 0)
            {
                ClickStageNameSQL = "1=1";
            }
            else
            {
                ClickStageNameSQL = string.Format(@" salesstage.stagename ='{0}'", p_clickstagename.Replace("'", "''"));
            }

            #endregion 
            #region 最后处理逻辑
            string totalSQL = string.Format(@"SELECT
	                                    category.categoryid categoryid,category.categoryname categoryname ,
	                                    e.recname oppname, e.recid recid ,
	                                    userInfo.username recmanager_name,userInfo.userid recmanager_id,
	                                    customer.recid customer_id,customer.recname customer_name ,
	                                    e.predeal ,
                                    salesstage.salesstageid,
                                     salesstage.stagename,
                                     salesstage.winrate * 100 winrate,
	                                    e.reccreated,
                                    e.premoney,
                                     (salesstage.winrate * e.premoney) /10000 totalAmount
                                    FROM
	                                    crm_sys_opportunity e
                                    INNER JOIN crm_sys_entity_category category ON category.categoryid = e.rectype
                                    INNER JOIN crm_sys_salesstage_setting salesstage ON e.recstageid = salesstage.salesstageid
                                    LEFT OUTER JOIN crm_sys_combinesalecategory combinecategory ON combinecategory.sourcecategoryid = category.categoryid
                                    left outer join crm_sys_userinfo userInfo on userInfo.Userid =  e.recmanager
                                    left outer join crm_sys_customer customer on customer.recid::text = jsonb_extract_path_text(e.belongcustomer,'id')
                                    WHERE
	                                    1 = 1
                                    AND salesstage.winrate > 0
                                    and ({0})
                                    and ({1})
                                    and ({2})
                                    and ({3})
                                    and ({4})
                                    and ({5})
	                           ", RuleSQL, DateTimeSQL, PersonSQL, CategorySQL, ClickStageNameSQL, ClickCategorySQL);
            List<Dictionary<string, object>> data = _reportEngineRepository.ExecuteSQL(totalSQL, new DbParameter[] { });
            Dictionary<string, List<Dictionary<string, object>>> retData = new Dictionary<string, List<Dictionary<string, object>>>();
            retData.Add("data", data);
            #endregion
            return retData;
        }
        /// <summary>
        /// 按团队预测销售目标，返回交叉表，且包含所有的阶段
        /// </summary>
        /// <param name="param"></param>
        /// <param name="sortby"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageCount"></param>
        /// <param name="userNum"></param>
        /// <returns></returns>
        public Dictionary<string, List<Dictionary<string, object>>> getSaleForecastCrossTableForDept(
                            Dictionary<string, object> param,
                            Dictionary<string, string> sortby,
                            int pageIndex, int pageCount,
                            int userNum)
        {
            #region 定义变量
            string p_category = "";
            string p_range = "";
            DateTime p_searchfrom = DateTime.MinValue;
            DateTime p_searchto = DateTime.MinValue;
            int year;
            int fromMonth;
            int toMonth;
            #endregion
            #region 获取参数
            p_category = ReportParamsUtils.parseString(param, "category");
            p_range = ReportParamsUtils.parseString(param, "range");
            p_searchfrom = ReportParamsUtils.parseDateTime(param, "searchfrom");
            p_searchto = ReportParamsUtils.parseDateTime(param, "searchto");
            if (p_category == null || p_category.Length == 0)
            {
                throw (new Exception("参数异常"));
            }
            if (p_range == null || p_range.Length == 0)
            {
                p_range = "0";
            }
            if (p_searchfrom == DateTime.MinValue)
            {
                throw (new Exception("参数异常"));
            }
            if (p_searchto == DateTime.MinValue)
            {
                throw (new Exception("参数异常"));
            }
            if (p_searchfrom.Year != p_searchto.Year)
            {
                throw (new Exception("不能跨年查询"));
            }
            year = p_searchto.Year;
            fromMonth = p_searchfrom.Month;
            toMonth = p_searchto.Month;

            #endregion
            #region 处理所有商机类型的销售阶段
            List<Dictionary<string, object>> salesstagelist = this.getAllSalesStageList(p_category);
            Dictionary<string, Dictionary<string, object>> salesstageDict = new Dictionary<string, Dictionary<string, object>>();
            if (salesstagelist == null || salesstagelist.Count == 0)
            {
                throw (new Exception("销售阶段设置异常"));
            }
            #endregion
            List<Dictionary<string, object>> details = new List<Dictionary<string, object>>();
            Dictionary<string, Dictionary<string, object>> detailDict = new Dictionary<string, Dictionary<string, object>>();
            List<Dictionary<string, object>> columnsList = new List<Dictionary<string, object>>();

            #region 制造列定义
            TableComponentInfo tableComponentInfo = new TableComponentInfo();
            tableComponentInfo.FixedX = 0;
            tableComponentInfo.FixedY = 1;

            tableComponentInfo.Columns = new List<TableColumnInfo>();
            TableColumnInfo tmpColumInfo = new TableColumnInfo();
            tmpColumInfo.CanPaged = false;
            tmpColumInfo.CanSorted = 0;
            tmpColumInfo.FieldName = "month";
            tmpColumInfo.Title = "月份";
            tableComponentInfo.Columns.Add(tmpColumInfo);
            int index = 1;
            foreach (Dictionary<string, object> item in salesstagelist)
            {
                TableColumnInfo columnnInfo = new TableColumnInfo();
                salesstageDict.Add(item["salesstageid"].ToString(), item);
                columnnInfo.Title = string.Format("{0}\r\n({1}%)", item["stagename"].ToString(), item["winrate"].ToString());
                columnnInfo.FieldName = "f_" + index.ToString();
                tableComponentInfo.Columns.Add(columnnInfo);
                item.Add("columninfo", columnnInfo);
                index++;
            }
            tmpColumInfo = new TableColumnInfo();
            tmpColumInfo.CanPaged = false;
            tmpColumInfo.CanSorted = 0;
            tmpColumInfo.FieldName = "totalamount";
            tmpColumInfo.Title = "金额汇总(万元)";
            tableComponentInfo.Columns.Add(tmpColumInfo);
            tmpColumInfo = new TableColumnInfo();
            tmpColumInfo.CanPaged = false;
            tmpColumInfo.CanSorted = 0;
            tmpColumInfo.FieldName = "preamount";
            tmpColumInfo.Title = "总预测值(万元)";
            tableComponentInfo.Columns.Add(tmpColumInfo);
            tmpColumInfo = new TableColumnInfo();
            tmpColumInfo.CanPaged = false;
            tmpColumInfo.CanSorted = 0;
            tmpColumInfo.FieldName = "targetamount";
            tmpColumInfo.Title = "目标(万元)";
            tableComponentInfo.Columns.Add(tmpColumInfo);
            for (int i = fromMonth; i <= toMonth; i++)
            {
                Dictionary<string, object> rowData = new Dictionary<string, object>();
                rowData.Add("month", i);
                foreach (TableColumnInfo columnInfo in tableComponentInfo.Columns)
                {
                    if (columnInfo.FieldName.StartsWith("f_"))
                    {
                        rowData.Add(columnInfo.FieldName, new Decimal(0.00));
                    }
                }
                rowData.Add("totalamount", new Decimal(0));
                rowData.Add("preamount", new Decimal(0));
                rowData.Add("targetamount", new Decimal(0));
                detailDict.Add(i.ToString(), rowData);
                details.Add(rowData);
            }
            columnsList = (List<Dictionary<string, object>>)Newtonsoft.Json.JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(Newtonsoft.Json.JsonConvert.SerializeObject(tableComponentInfo.Columns));
            #endregion

            #region 获取详情列表，不分页
            param.Add("department", p_range);//参数不一致，要重新加入参数
            Dictionary<string, List<Dictionary<string, object>>> billListDict = this.getSaleForecastOppList(param, sortby, 1, 
                00, userNum);
            if (billListDict != null && billListDict.ContainsKey("data") && billListDict["data"] != null)
            {
                List<Dictionary<string, object>> billList = billListDict["data"];
                foreach (Dictionary<string, object> bill in billList)
                {
                    string salesstageid = null;
                    DateTime predeal = DateTime.MinValue;
                    if (bill.ContainsKey("predeal") && bill["predeal"] != null)
                    {
                        predeal = (DateTime)bill["predeal"];
                    }
                    if (predeal == DateTime.MinValue)
                    {
                        continue;
                    }
                    if (bill.ContainsKey("salesstageid") && bill["salesstageid"] != null)
                    {
                        salesstageid = bill["salesstageid"].ToString();
                    }
                    if (salesstageid == null || salesstageid.Length == 0)
                    {
                        continue;
                    }
                    int month = predeal.Month;
                    if (detailDict.ContainsKey(month.ToString()))
                    {
                        Dictionary<string, object> rowData = detailDict[month.ToString()];
                        if (salesstageDict.ContainsKey(salesstageid))
                        {
                            Dictionary<string, object> columnItem = salesstageDict[salesstageid];
                            TableColumnInfo columnInfo = (TableColumnInfo)columnItem["columninfo"];
                            string fieldname = columnInfo.FieldName;
                            Decimal premoney = new Decimal(0);
                            if (bill.ContainsKey("premoney") && bill["premoney"] != null)
                            {
                                Decimal.TryParse(bill["premoney"].ToString(), out premoney);
                            }
                            if (rowData.ContainsKey(fieldname))
                            {
                                Decimal tmp = (Decimal)rowData[fieldname];
                                tmp = tmp + premoney;
                                rowData[fieldname] = tmp;
                            }
                        }
                    }

                }
            }
            #endregion

            #region 获取目标情况
            string TargetSumSQL = this.getSummaryField2ForDept(year, fromMonth, toMonth, p_range);
            List<Dictionary<string, object>> data = _reportEngineRepository.ExecuteSQL(TargetSumSQL, new DbParameter[] { });
            Decimal TargetAmount = new Decimal(0.00);
            foreach (Dictionary<string, object> item in data)
            {
                string sMonth = item["fmonth"].ToString();
                Decimal targetamount = new Decimal(0);
                Decimal.TryParse(item["targetamount"].ToString(), out targetamount);
                if (detailDict.ContainsKey(sMonth))
                {
                    Dictionary<string, object> info = detailDict[sMonth];
                    info["targetamount"] = targetamount;
                }
            }

            //计算合计
            #endregion
            #region 计算汇总值
            foreach (Dictionary<string, object> rowData in details)
            {
                Decimal summaryAmount = new Decimal(0);
                Decimal predictAmmount = new Decimal(0);
                foreach (Dictionary<string, object> columnInfo in salesstagelist)
                {
                    TableColumnInfo tmp = (TableColumnInfo)columnInfo["columninfo"];
                    Decimal tmpAmount = (Decimal)rowData[tmp.FieldName];
                    summaryAmount = summaryAmount + tmpAmount;
                    Decimal winrate = (Decimal)columnInfo["winrate"];
                    predictAmmount = predictAmmount + tmpAmount * winrate / 100;
                }
                rowData["totalamount"] = summaryAmount;
                rowData["preamount"] = predictAmmount;
            }
            #endregion
            #region 最后组装数据
            Dictionary<string, List<Dictionary<string, object>>> retDict = new Dictionary<string, List<Dictionary<string, object>>>();
            retDict.Add("data", details);
            retDict.Add("columns", columnsList);
            #endregion
            return retDict;
        }
        /// <summary>
        /// 按个人预测销售目标，返回交叉表，切包含所有阶段
        /// </summary>
        /// <param name="param"></param>
        /// <param name="sortby"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageCount"></param>
        /// <param name="userNum"></param>
        /// <returns></returns>
        public Dictionary<string, List<Dictionary<string, object>>> getSaleForecastCrossSummaryForUsers(
            Dictionary<string, object> param,
                            Dictionary<string, string> sortby,
                            int pageIndex, int pageCount,
                            int userNum)
        {
            #region 定义变量
            string p_category = "";
            string p_range = "";
            DateTime p_searchfrom = DateTime.MinValue;
            DateTime p_searchto = DateTime.MinValue;
            int year;
            int fromMonth;
            int toMonth;
            #endregion
            #region 获取参数
            p_category = ReportParamsUtils.parseString(param, "category");
            p_range = ReportParamsUtils.parseString(param, "range");
            p_searchfrom = ReportParamsUtils.parseDateTime(param, "searchfrom");
            p_searchto = ReportParamsUtils.parseDateTime(param, "searchto");
            if (p_category == null || p_category.Length == 0) {
                throw (new Exception("参数异常"));
            }
            if (p_range == null || p_range.Length == 0) {
                p_range = "0";
            }
            if (p_searchfrom == DateTime.MinValue) {
                throw (new Exception("参数异常"));
            }
            if (p_searchto == DateTime.MinValue) {
                throw (new Exception("参数异常"));
            }
            if (p_searchfrom.Year != p_searchto.Year) {
                throw (new Exception("不能跨年查询"));
            }
            year = p_searchto.Year;
            fromMonth = p_searchfrom.Month;
            toMonth = p_searchto.Month;

            #endregion
            #region 处理所有商机类型的销售阶段
            List<Dictionary<string, object>> salesstagelist = this.getAllSalesStageList(p_category);
            Dictionary<string, Dictionary<string, object>> salesstageDict = new Dictionary<string, Dictionary<string, object>>();
            if (salesstagelist == null || salesstagelist.Count == 0) {
                throw (new Exception("销售阶段设置异常"));
            }
            #endregion
            List<Dictionary<string,object> > details = new List<Dictionary<string, object>>();
            Dictionary<string, Dictionary<string, object>> detailDict = new Dictionary<string, Dictionary<string, object>>();
            List<Dictionary<string, object>> columnsList = new List<Dictionary<string, object>>();

            #region 制造列定义
            TableComponentInfo tableComponentInfo = new TableComponentInfo();
            tableComponentInfo.FixedX = 0;
            tableComponentInfo.FixedY = 1;

            tableComponentInfo.Columns = new List<TableColumnInfo>();
            TableColumnInfo tmpColumInfo = new TableColumnInfo();
            tmpColumInfo.CanPaged = false;
            tmpColumInfo.CanSorted = 0;
            tmpColumInfo.FieldName = "month";
            tmpColumInfo.Title = "月份";
            tableComponentInfo.Columns.Add(tmpColumInfo);
            int index = 1;
            foreach (Dictionary<string, object> item in salesstagelist) {
                TableColumnInfo columnnInfo = new TableColumnInfo();
                salesstageDict.Add(item["salesstageid"].ToString(), item);
                columnnInfo.Title = string.Format("{0}\r\n({1}%)", item["stagename"].ToString(), item["winrate"].ToString());
                columnnInfo.FieldName = "f_" + index.ToString();
                tableComponentInfo.Columns.Add(columnnInfo);
                item.Add("columninfo", columnnInfo);
                index++;
            }
            tmpColumInfo = new TableColumnInfo();
            tmpColumInfo.CanPaged = false;
            tmpColumInfo.CanSorted = 0;
            tmpColumInfo.FieldName = "totalamount";
            tmpColumInfo.Title = "金额汇总(万元)";
            tableComponentInfo.Columns.Add(tmpColumInfo);
            tmpColumInfo = new TableColumnInfo();
            tmpColumInfo.CanPaged = false;
            tmpColumInfo.CanSorted = 0;
            tmpColumInfo.FieldName = "preamount";
            tmpColumInfo.Title = "总预测值(万元)";
            tableComponentInfo.Columns.Add(tmpColumInfo);
            tmpColumInfo = new TableColumnInfo();
            tmpColumInfo.CanPaged = false;
            tmpColumInfo.CanSorted = 0;
            tmpColumInfo.FieldName = "targetamount";
            tmpColumInfo.Title = "目标(万元)";
            tableComponentInfo.Columns.Add(tmpColumInfo);
            for (int i = fromMonth; i <= toMonth; i++) {
                Dictionary<string, object> rowData = new Dictionary<string, object>();
                rowData.Add("month", i);
                foreach (TableColumnInfo columnInfo in tableComponentInfo.Columns) {
                    if (columnInfo.FieldName.StartsWith("f_")) {
                        rowData.Add(columnInfo.FieldName, new Decimal(0.00));
                    }
                }
                rowData.Add("totalamount", new Decimal(0));
                rowData.Add("preamount", new Decimal(0));
                rowData.Add("targetamount", new Decimal(0));
                detailDict.Add(i.ToString(), rowData);
                details.Add(rowData);
            }
            columnsList = (List<Dictionary<string, object>>)Newtonsoft.Json.JsonConvert.DeserializeObject< List<Dictionary<string, object>>>(Newtonsoft.Json.JsonConvert.SerializeObject(tableComponentInfo.Columns));
            #endregion

            #region 获取详情列表，不分页
            Dictionary<string,List<Dictionary<string, object>>> billListDict = this.getSaleForecastOppListForUsers(param, sortby, 1, 10000000,userNum);
            if (billListDict != null && billListDict.ContainsKey("data") && billListDict["data"] != null) {
                List<Dictionary<string, object>> billList = billListDict["data"];
                foreach (Dictionary<string, object> bill in billList) {
                    string salesstageid = null;
                    DateTime predeal = DateTime.MinValue;
                    if (bill.ContainsKey("predeal") && bill["predeal"] != null) {
                         predeal = (DateTime)bill["predeal"];
                    }
                    if (predeal == DateTime.MinValue)
                    {
                        continue;
                    }
                    if (bill.ContainsKey("salesstageid") && bill["salesstageid"] != null)
                    {
                        salesstageid = bill["salesstageid"].ToString();
                    }
                    if (salesstageid == null || salesstageid.Length == 0) {
                        continue;
                    }
                    int month = predeal.Month;
                    if (detailDict.ContainsKey(month.ToString())) {
                        Dictionary<string, object> rowData = detailDict[month.ToString()];
                        if (salesstageDict.ContainsKey(salesstageid)) {
                            Dictionary<string, object> columnItem = salesstageDict[salesstageid];
                            TableColumnInfo columnInfo = (TableColumnInfo)columnItem["columninfo"];
                            string fieldname = columnInfo.FieldName;
                            Decimal premoney = new Decimal(0);
                            if (bill.ContainsKey("premoney") && bill["premoney"] != null) {
                                Decimal.TryParse(bill["premoney"].ToString(),out  premoney);
                            }
                            if (rowData.ContainsKey(fieldname))
                            {
                                Decimal tmp = (Decimal)rowData[fieldname];
                                tmp = tmp + premoney;
                                rowData[fieldname] = tmp;
                            }
                        }
                    }

                }
            }
            #endregion

            #region 获取目标情况
            string TargetSumSQL = this.getSummaryField2ForUsers(year, fromMonth, toMonth, p_range);
            List<Dictionary<string,object>>  data = _reportEngineRepository.ExecuteSQL(TargetSumSQL, new DbParameter[] { });
            Decimal TargetAmount = new Decimal(0.00);
            foreach (Dictionary<string, object> item in data) {
                string sMonth = item["fmonth"].ToString();
                Decimal targetamount = new Decimal(0);
                Decimal.TryParse(item["targetamount"].ToString(), out targetamount);
                if (detailDict.ContainsKey(sMonth))
                {
                    Dictionary<string, object> info = detailDict[sMonth];
                    info["targetamount"] = targetamount;
                }
            }

            //计算合计
            #endregion
            #region 计算汇总值
            foreach (Dictionary<string, object> rowData in details) {
                Decimal summaryAmount = new Decimal(0);
                Decimal predictAmmount = new Decimal(0);
                foreach (Dictionary<string, object> columnInfo in salesstagelist) {
                    TableColumnInfo tmp = (TableColumnInfo)columnInfo["columninfo"];
                    Decimal tmpAmount = (Decimal)rowData[tmp.FieldName];
                    summaryAmount = summaryAmount + tmpAmount;
                    Decimal winrate = (Decimal)columnInfo["winrate"];
                    predictAmmount = predictAmmount + tmpAmount * winrate / 100;
                }
                rowData["totalamount"] = summaryAmount;
                rowData["preamount"] = predictAmmount;
            }
            #endregion
            #region 最后组装数据
            Dictionary<string, List<Dictionary<string, object>>> retDict = new Dictionary<string, List<Dictionary<string, object>>>();
            retDict.Add("data", details);
            retDict.Add("columns", columnsList); 
            #endregion
            return retDict;
        }
        public string getSummaryField2ForUsers(int year, int from, int to, string users) {
            string[] monthFields = new string[] { "", "jancount", "febcount", "marcount", "aprcount", "maycount", "juncount", "julcount", "augcount", "sepcount", "octcount", "novcount", "deccount" };
            string cmdText = string.Format(@"(select {0} fmonth,{1} TargetAmount  
                            from crm_sys_sales_target 
                            where normtypeid 
                            in (select normtypeid  from crm_sys_sales_target_norm_type where entityid = '2c63b681-1de9-41b7-9f98-4cf26fd37ef1' and recstatus = 1  and calcutetype = 0 )
                            and isgrouptarget =false 
                            and recstatus = 1 and userid in ({2}) And year={3})", from, monthFields[from], users, year);
            for (int i = from + 1; i <= to; i++) {
                cmdText = cmdText + string.Format(@"union all (select {0} fmonth,{1} TargetAmount  
                            from crm_sys_sales_target 
                            where normtypeid 
                            in (select normtypeid  from crm_sys_sales_target_norm_type where entityid = '2c63b681-1de9-41b7-9f98-4cf26fd37ef1' and recstatus = 1  and calcutetype = 0 )
                            and isgrouptarget =false 
                            and recstatus = 1 and userid in ({2}) And year={3})", i, monthFields[i], users, year);
            }
            cmdText = string.Format(@"Select targetsql.fmonth,sum(targetsql.TargetAmount) TargetAmount 
                                    from ({0}) targetsql
                                    group by targetsql.fmonth", cmdText);
            return cmdText;
        }
        public string getSummaryField2ForDept(int year, int from, int to, string dept)
        {
            string[] monthFields = new string[] { "", "jancount", "febcount", "marcount", "aprcount", "maycount", "juncount", "julcount", "augcount", "sepcount", "octcount", "novcount", "deccount" };
            string cmdText = string.Format(@"(select {0} fmonth,{1} TargetAmount  
                            from crm_sys_sales_target 
                            where normtypeid 
                            in (select normtypeid  from crm_sys_sales_target_norm_type where entityid = '2c63b681-1de9-41b7-9f98-4cf26fd37ef1' and recstatus = 1  and calcutetype = 0 )
                            and isgrouptarget =true 
                            and recstatus = 1 and departmentid ='{2}' And year={3})", from, monthFields[from], dept, year);
            for (int i = from + 1; i <= to; i++)
            {
                cmdText = cmdText + string.Format(@"union all (select {0} fmonth,{1} TargetAmount  
                            from crm_sys_sales_target 
                            where normtypeid 
                            in (select normtypeid  from crm_sys_sales_target_norm_type where entityid = '2c63b681-1de9-41b7-9f98-4cf26fd37ef1' and recstatus = 1  and calcutetype = 0 )
                            and isgrouptarget =true 
                            and recstatus = 1 and departmentid ='{2}' And year={3})", i, monthFields[i], dept, year);
            }
            cmdText = string.Format(@"Select targetsql.fmonth,sum(targetsql.TargetAmount) TargetAmount 
                                    from ({0}) targetsql
                                    group by targetsql.fmonth", cmdText);
            return cmdText;
        }
        public string getSummaryField(int from, int to) {
            string[] monthFields = new string[] { "", "jancount", "febcount", "marcount", "aprcount", "maycount", "juncount", "julcount", "augcount", "sepcount", "octcount", "novcount", "deccount" };
            string ret = "0";
            for (int i = from; i <= to; i++) {
                ret = ret + "+sum(" + monthFields[i] + ")";
            }
            return ret;
        }



        /// <summary>
        /// 获取商机类型和销售阶段的列表
        /// </summary>
        /// <param name="stageid"></param>
        /// <returns></returns>
        private List<Dictionary<string, object>> getAllSalesStageList(string stageid)
        {
            string cmdText = "";
            string filterCategory = " 1=1 ";
            try
            {
                if (!(stageid == null || stageid.Length == 0 || stageid == Guid.Empty.ToString())) {
                    filterCategory = " a.categoryid='" + stageid + "'";
                }

                cmdText = string.Format(@"select a.categoryid ,a.categoryname ,b.salesstageid ,b.stagename ,b.winrate *100 winrate 
                                from crm_sys_entity_category a 
	                                inner join crm_sys_salesstage_setting b on a.categoryid = b.salesstagetypeid 
                                where a.entityid = '2c63b681-1de9-41b7-9f98-4cf26fd37ef1' and a.recstatus = 1 
	                                and b.recstatus = 1
	                                and b.winrate > 0 
                                    and {0}
                                order by a.recorder ,b.recorder 
                                ", filterCategory);
                return this._reportEngineRepository.ExecuteSQL(cmdText, new DbParameter[] { });

            }
            catch (Exception ex) {

            }
            return null;
        }

    }
    public class SaleCategoryInfo {
        public string CategoryId { get; set; }

        public string CategoryName { get; set; }
        public List<Dictionary<string, object>> Data { get; set; }
        public SaleCategoryInfo() {
            Data = new List<Dictionary<string, object>>();
        }
    }
}
