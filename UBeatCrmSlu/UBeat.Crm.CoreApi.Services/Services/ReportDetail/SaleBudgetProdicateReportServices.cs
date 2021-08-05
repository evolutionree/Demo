using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.Reports;
using System.Data.Common;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Utility;

namespace UBeat.Crm.CoreApi.Services.Services.ReportDetail
{
   
    public class SaleBudgetProdicateReportServices: EntityBaseServices
    {
        private static string OPP_EntityID = "2c63b681-1de9-41b7-9f98-4cf26fd37ef1";
        private IDynamicEntityRepository _dynamicEntityRepository;
        private IReportEngineRepository _reportEngineRepository;

        public SaleBudgetProdicateReportServices(IDynamicEntityRepository dynamicEntityRepository, IReportEngineRepository reportEngineRepository)
        {
            _dynamicEntityRepository = dynamicEntityRepository;
            _reportEngineRepository = reportEngineRepository;
        }
        public Dictionary<string, List<Dictionary<string, object>>> getOppoSummary(
                            Dictionary<string, object> param,
                            Dictionary<string, string> sortby,
                            int pageIndex, int pageCount,
                            int userNum)
        {
            #region 定义过程变量
            string p_range = "";
            int p_rangetype = 0;
            DateTime p_searchfrom;
            DateTime p_searchto;
            string p_oppotypeid = "";
            List<FilterSerieDefineInfo> series = null;
            string datetimeSQL = "";
            string ruleSQL = "1=1";
            string PersonSQL = "";
            string OppTypeSQL = "";
            #endregion
            #region 获取前端传输过来的变量
            #region 处理范围类型
            try
            {
                if (param.ContainsKey("@rangetype"))
                {
                    p_rangetype = int.Parse(param["@rangetype"].ToString());
                }
                else if (param.ContainsKey("rangetype"))
                {
                    p_rangetype = int.Parse(param["rangetype"].ToString());
                }
                else {
                    throw (new Exception("参数异常"));
                }

            }
            catch (Exception ex) {
                throw (new Exception("参数异常"));
            }
            #endregion

            #region 处理范围
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
            #endregion
            #region 处理开始日期
            if (param.ContainsKey("@searchfrom"))
            {
                p_searchfrom = DateTime.Parse(param["@searchfrom"].ToString());
            }
            else if (param.ContainsKey("searchfrom"))
            {

                p_searchfrom = DateTime.Parse(param["searchfrom"].ToString());
            }
            else {
                throw (new Exception("参数异常"));
            }
            #endregion
            #region 处理结束日期
            if (param.ContainsKey("@searchto"))
            {
                p_searchto = DateTime.Parse(param["@searchto"].ToString());
            }
            else if (param.ContainsKey("searchto"))
            {

                p_searchto = DateTime.Parse(param["searchto"].ToString());
            }
            else
            {
                throw (new Exception("参数异常"));
            }
            #endregion
            #region 处理系列参数
            if (param.ContainsKey("@seriessetting"))
            {
                series = parseSeries(param["@seriessetting"].ToString());
            }
            else if (param.ContainsKey("seriessetting"))
            {
                series = parseSeries(param["seriessetting"].ToString());
            }
            else {
                throw (new Exception("参数异常"));
            }
            if (series == null || series.Count == 0) {
                throw (new Exception("参数异常"));
            }
            #endregion

            p_oppotypeid = ReportParamsUtils.parseString(param,"opptype");
            if (p_searchto < p_searchfrom) {
                throw (new Exception("结束日期不能小于开始日期"));
            }
            if (p_range == null || p_range.Length == 0) throw (new Exception("搜索范围未选择或者选择异常"));
            if (p_rangetype != 1 && p_rangetype != 2) throw (new Exception("搜索范围未选择或者选择异常"));
            #endregion


            #region 处理DateSQL
            datetimeSQL = string.Format(" e.predeal >= '{0}' And e.predeal <='{1}'", p_searchfrom.ToString("yyyy-MM-dd 00:00:00"), p_searchto.ToString("yyyy-MM-dd 23:59:59"));
            #endregion

            #region 处理数据权限
            ruleSQL = " 1=1 ";
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
                ruleSQL = ruleSQL + " And e.recid in (" + userRoleRuleSQL + ")";
            }

            #endregion

            #region 处理查询范围
            if (p_rangetype == 1) {
                //按组织
                string subDeptSQL = string.Format("select deptid  from crm_func_department_tree('{0}',1) ", p_range);

                string belongPerson = string.Format("select userid  from crm_sys_account_userinfo_relate where deptid in({0}) ", subDeptSQL);
                PersonSQL = string.Format("e.recmanager in ({0})", belongPerson);
            }
            else
            {
                //按个人
                PersonSQL = string.Format(" e.recmanager in ({0})", p_range);

            }
            #endregion
            #region 处理商机类型
            if (p_oppotypeid == null || p_oppotypeid == "" ||  p_oppotypeid == Guid.Empty.ToString())
            {
                OppTypeSQL = " 1=1 ";
            }
            else
            {
                OppTypeSQL = string.Format(@"e.rectype='{0}'", p_oppotypeid);
            }
            #endregion
            #region 获取商机数据
            string totalSQL = string.Format(@"SELECT
	                                            e.recid,
	                                            to_char(e.predeal, 'MM/dd') days,
	                                            salesstage.winrate * 100 winrate,
	                                            e.premoney * 1.0 / 10000 amount,
	                                            e.recname ,
	                                            salesstage.stagename ,
	                                            manager.username recmanager_name,
	                                            customer.recname customer_name
                                            FROM
	                                            crm_sys_opportunity e
                                            INNER JOIN crm_sys_salesstage_setting salesstage ON e.recstageid = salesstageid
                                            left outer join crm_sys_userinfo manager on  manager.userid  = e.recmanager 
                                            left outer join crm_sys_customer customer on customer.recid::text = jsonb_extract_path_text(e.belongcustomer,'id')
                                where e.recstatus = 1
                                and {0} 
                                And {1} 
                                And {2} 
                                And {3} ", ruleSQL, datetimeSQL, PersonSQL,OppTypeSQL);
            List<Dictionary<string,object>> data = _reportEngineRepository.ExecuteSQL(totalSQL,new DbParameter[] { });
            Dictionary<string,List<Dictionary<string, object>>> retData = new Dictionary<string, List<Dictionary<string, object>>>();
            #endregion
            #region 规整分类（系列)

            List<Dictionary<string, object>> seriesList = new List<Dictionary<string, object>>();
            foreach (FilterSerieDefineInfo serieInfo in series) {
                if (serieInfo.SerieStatus == 2) continue;
                serieInfo.XFieldName = "days";
                serieInfo.YFieldName = "winrate";
                serieInfo.SizeFieldName = "amount";
                List<Dictionary<string, object>> serieData = new List<Dictionary<string, object>>();
				if(data != null)
				{
					foreach (Dictionary<string, object> sqlDataItem in data)
					{
						if (sqlDataItem.ContainsKey("amount") && sqlDataItem["amount"] != null)
						{
							Decimal amount = new Decimal();
							if (Decimal.TryParse(sqlDataItem["amount"].ToString(), out amount) == false) continue;
							if (amount >= serieInfo.SerieFrom && amount < serieInfo.SerieTo)
							{
								serieData.Add(sqlDataItem);
							}
						}
					}
				}

				Dictionary<string, object> tmpDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(Newtonsoft.Json.JsonConvert.SerializeObject(serieInfo));
                tmpDict.Add("data", serieData);
                seriesList.Add(tmpDict);
            }
            #endregion
            retData.Add("data", seriesList);
            #region 处理X轴坐标集合
            DateTime tmpDateTime = p_searchfrom;
            List<Dictionary<string, object>> xSeries = new List<Dictionary<string, object>>();
            while (tmpDateTime <= p_searchto) {
                Dictionary<string, object> item = new Dictionary<string, object>();
                item.Add("xvalue", tmpDateTime.ToString("MM/dd"));
                tmpDateTime = tmpDateTime + new TimeSpan(1, 0, 0, 0);
                xSeries.Add(item);
            }
            retData.Add("xseries", xSeries);
            #endregion
            return retData;
        }

        public Dictionary<string, List<Dictionary<string, object>>> getOppoList(
                            Dictionary<string, object> param,
                            Dictionary<string, string> sortby,
                            int pageIndex, int pageCount,
                            int userNum)
        {
            #region 定义过程变量
            string p_range = "";
            int p_rangetype = 0;
            DateTime p_searchfrom;
            DateTime p_searchto;
            string p_oppotypeid = "";
            List<FilterSerieDefineInfo> series = null;
            string datetimeSQL = "";
            string ruleSQL = "1=1";
            string PersonSQL = "";
            string OppTypeSQL = " 1=1 ";
            #endregion
            #region 获取前端传输过来的变量
            #region 处理范围类型
            try
            {
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

            }
            catch (Exception ex)
            {
                throw (new Exception("参数异常"));
            }
            #endregion

            #region 处理范围
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
            #endregion
            #region 处理开始日期
            if (param.ContainsKey("@searchfrom"))
            {
                p_searchfrom = DateTime.Parse(param["@searchfrom"].ToString());
            }
            else if (param.ContainsKey("searchfrom"))
            {

                p_searchfrom = DateTime.Parse(param["searchfrom"].ToString());
            }
            else
            {
                throw (new Exception("参数异常"));
            }
            #endregion
            #region 处理结束日期
            if (param.ContainsKey("@searchto"))
            {
                p_searchto = DateTime.Parse(param["@searchto"].ToString());
            }
            else if (param.ContainsKey("searchto"))
            {

                p_searchto = DateTime.Parse(param["searchto"].ToString());
            }
            else
            {
                throw (new Exception("参数异常"));
            }
            #endregion

            #endregion
            #region 处理DateSQL
            datetimeSQL = string.Format(" e.predeal >= '{0}' And e.predeal <='{1}'", p_searchfrom.ToString("yyyy-MM-dd 00:00:00"), p_searchto.ToString("yyyy-MM-dd 23:59:59"));
            #endregion

            #region 处理数据权限
            ruleSQL = " 1=1 ";
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
                ruleSQL = ruleSQL + " And e.recid in (" + userRoleRuleSQL + ")";
            }

            #endregion

            p_oppotypeid = ReportParamsUtils.parseString(param, "opptype");
            #region 处理查询范围
            if (p_rangetype == 1)
            {
                //按组织
                string subDeptSQL = string.Format("select deptid  from crm_func_department_tree('{0}',1) ", p_range);

                string belongPerson = string.Format("select userid  from crm_sys_account_userinfo_relate where deptid in({0}) ", subDeptSQL);
                PersonSQL = string.Format("e.recmanager in ({0})", belongPerson);
            }
            else
            {
                //按个人
                PersonSQL = string.Format(" e.recmanager in ({0})", p_range);

            }
            #endregion
            #region 处理商机类型
            if (p_oppotypeid == null || p_oppotypeid == "" || p_oppotypeid == Guid.Empty.ToString())
            {
                OppTypeSQL = " 1=1 ";
            }
            else {
                OppTypeSQL = string.Format(@"e.rectype='{0}'", p_oppotypeid);
            }
            #endregion
            string totalSQL = string.Format(@"select  to_char(e.predeal,'yyyy-MM-dd') predeal ,e.recid,e.recname ,
                                customer.recid customer_id,customer.recname customer_name,
                                category.categoryid categoryid,category.categoryname categoryname,
                                department.deptid ,department.deptname,
                                salesstage.stagename ||'(' || salesstage.winrate *100 ||')'  salestagename,
                                userinfo.userid recmanager_id ,userinfo.username recmanager_name,
                                salesstage.winrate *100 winrate ,e.premoney *1.0  /10000 amount
                                from crm_sys_opportunity e 
		                                inner join crm_sys_salesstage_setting salesstage on e.recstageid = salesstageid
		                                left outer join crm_sys_customer  customer on jsonb_extract_path_text(e.belongcustomer,'id') = customer.recid::text
		                                left outer join crm_sys_entity_category category on category.categoryid = e.rectype
                                        left outer join crm_sys_userinfo userinfo on userinfo.userid = e.recmanager
		                                left outer join (select userid,min(deptid::text) deptid from crm_sys_account_userinfo_relate group by userid  ) userdept on userdept.userid = e.recmanager 
		                                left outer join crm_sys_department department on department.deptid = userdept.deptid::uuid
                                where e.recstatus = 1
                                and {0} 
                                And {1} 
                                And {2}
                                And {3} order by  salesstage.winrate desc", ruleSQL, datetimeSQL, PersonSQL,OppTypeSQL);
            List<Dictionary<string, object>> data = _reportEngineRepository.ExecuteSQL(totalSQL, new DbParameter[] { });
            Dictionary<string, List<Dictionary<string, object>>> retData = new Dictionary<string, List<Dictionary<string, object>>>();
            retData.Add("data", data);
            return retData;
        }


        /// <summary>
        /// 根据参数
        /// </summary>
        /// <param name="param"></param>
        /// <param name="sortby"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageCount"></param>
        /// <param name="userNum"></param>
        /// <returns></returns>
        public Dictionary<string, List<Dictionary<string, object>>> getParamsDisplay(Dictionary<string, object> param,
                            Dictionary<string, string> sortby,
                            int pageIndex, int pageCount,
                            int userNum)
        {
            Dictionary<string, List<Dictionary<string, object>>> ret = new Dictionary<string, List<Dictionary<string, object>>>();
            try
            {
                string p_range = "";
                string  p_searchfrom = "";
                string p_searchto = "";
                string deptName = "";
                #region 处理参数
                if (param.ContainsKey("@range"))
                {
                    p_range = param["@range"].ToString();
                }
                else if (param.ContainsKey("range"))
                {
                    p_range = param["range"].ToString();
                }
                if (param.ContainsKey("@searchfrom"))
                {
                    p_searchfrom =  param["@searchfrom"].ToString();
                }
                else if (param.ContainsKey("searchfrom"))
                {
                    p_searchfrom = param["searchfrom"].ToString();
                }
                if (param.ContainsKey("@searchto"))
                {
                    p_searchto = param["@searchto"].ToString();
                }
                else if (param.ContainsKey("searchto"))
                {

                    p_searchto = param["searchto"].ToString();
                }
                #endregion
                if (p_range != null && p_range.Length > 0) {
                    try {
                        string cmdText = @"Select deptname from crm_sys_department where deptid=@deptid";
                        DbParameter[] dbParams = new Npgsql.NpgsqlParameter[] {
                            new Npgsql.NpgsqlParameter("deptid",Guid.Parse(p_range))
                        };
                        List<Dictionary<string,object>> data  = this._reportEngineRepository.ExecuteSQL(cmdText, dbParams);
                        if (data != null && data.Count > 0) {
                            deptName = data[0]["deptname"].ToString();
                        }
                    } catch (Exception ex) {

                    }
                }
                Dictionary<string, object> item = new Dictionary<string, object>();
                item.Add("rangename", deptName);
                item.Add("fromdate", p_searchfrom);
                item.Add("todate", p_searchto);
                List<Dictionary<string, object>> retData = new List<Dictionary<string, object>>();
                retData.Add(item);
                ret.Add("data", retData);
            }
            catch (Exception ex)
            {
            }
            return ret;
        }
        /// <summary>
        /// 获取商机的详情的基本信息
        /// 返回预计成交金额和赢单率
        /// </summary>
        /// <param name="param"></param>
        /// <param name="sortby"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageCount"></param>
        /// <param name="userNum"></param>
        /// <returns></returns>
        public Dictionary<string, List<Dictionary<string, object>>> getOppDetail(
                           Dictionary<string, object> param,
                           Dictionary<string, string> sortby,
                           int pageIndex, int pageCount,
                           int userNum)
        {
            Dictionary<string, List<Dictionary<string, object>>> retData = new Dictionary<string, List<Dictionary<string, object>>>();
            try
            {
                string recid = "";
                if (param == null) throw (new Exception("参数异常"));
                if (param.ContainsKey("@recid"))
                {
                    recid = param["@recid"].ToString();
                }
                else if (param.ContainsKey("recid"))
                {
                    recid = param["recid"].ToString();
                }
                else
                {
                    recid = Guid.Empty.ToString();
                }
                string cmdText = string.Format(@"	select e.recid,e.premoney,salesstage.winrate *100 winrate
	                                        from
		                                        crm_sys_opportunity e
		                                        inner join crm_sys_salesstage_setting salesstage on e.recstageid = salesstage.salesstageid
                                        where e.recid = '{0}' ", recid.Replace("'", "''"));
                List<Dictionary<string, object>> list = _reportEngineRepository.ExecuteSQL(cmdText, new DbParameter[] { });
                if (list == null)
                {
                    list = new List<Dictionary<string, object>>();
                }
                if (list.Count == 0)
                {
                    Dictionary<string, object> item = new Dictionary<string, object>();
                    item.Add("recid", Guid.Empty.ToString());
                    item.Add("premoney", 0.00);
                    item.Add("winrate", 0);
                    list.Add(item);
                    bool hasAddParams = false;
                    Dictionary<string, List<Dictionary<string, object>>> paramsValueRs = getParamsDisplay(param, sortby, pageIndex, pageCount, userNum);
                    if (paramsValueRs != null && paramsValueRs.ContainsKey("data"))
                    {
                        List<Dictionary<string, object>> innerData = paramsValueRs["data"];
                        if (innerData != null && innerData.Count > 0)
                        {
                            item.Add("rangename", innerData[0]["rangename"]);
                            item.Add("fromdate", innerData[0]["fromdate"]);
                            item.Add("todate", innerData[0]["todate"]);
                            hasAddParams = true;
                        }
                    }
                    if (hasAddParams ==false) {
                        item.Add("rangename", "");
                        item.Add("fromdate", "");
                        item.Add("todate","");
                    }
                }
                retData.Add("data", list);
                

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return retData;
        }
        /***
         * 处理前端传输过来的系列的参数，可能是字符串，也可能是list
         * */
        private List<FilterSerieDefineInfo> parseSeries(object   seriesString) {
            try
            {
                string tmp = Newtonsoft.Json.JsonConvert.SerializeObject(seriesString);
                return Newtonsoft.Json.JsonConvert.DeserializeObject<List<FilterSerieDefineInfo>>(tmp);
            }
            catch (Exception ex) {
            }
            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<List<FilterSerieDefineInfo>> (seriesString.ToString());
            }
            catch (Exception ex) {
                return null;
            }
        }
    }
}
