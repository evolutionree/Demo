using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.Utility;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.CoreApi.Services.Services.ReportDetail;
using UBeat.Crm.CoreApi.Services.Utility;

namespace UBeat.Crm.CoreApi.ZGQY.Services
{
    public class ReportServices : EntityBaseServices
    {
        private static string OPP_EntityID = "6f9f19d8-73e0-4d4d-962e-f68b1448c0f5";
        private IDynamicEntityRepository _dynamicEntityRepository;
        private IReportEngineRepository _reportEngineRepository;
        public ReportServices(IDynamicEntityRepository dynamicEntityRepository,
                IReportEngineRepository reportEngineRepository)
        {
            _dynamicEntityRepository = dynamicEntityRepository;
            _reportEngineRepository = reportEngineRepository;
        }

        /// <summary>
        /// 销售漏斗预测
        /// </summary>
        /// <param name="param"></param>
        /// <param name="sortby"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageCount"></param>
        /// <param name="userNum"></param>
        /// <returns></returns>
        public Dictionary<string, List<Dictionary<string, object>>> getSaleForecastForDeptForTable(Dictionary<string, object> param, Dictionary<string, string> sortby, int pageIndex, int pageCount, int userNum)
        {
            Dictionary<string, List<Dictionary<string, object>>> retData = new Dictionary<string, List<Dictionary<string, object>>>();
            var retData2 = getSaleForecastForDept(param, sortby, pageIndex, pageCount, userNum)["data"];
            var dic = retData2[0];
            var list = new List<Dictionary<string, object>>();
            foreach (var item in JsonHelper.ToJsonArray(JsonHelper.ToJson(dic["data"])))
            {
                var dic2 = new Dictionary<string, object>();
                dic2.Add("stagename", item["stagename"]);
                dic2.Add("currentcount", item["currentcount"]);
                dic2.Add("proportion", item["proportion"]);
                dic2.Add("winrate", item["winrate"]);
                dic2.Add("money", item["money"]);
                dic2.Add("oppcount", item["oppcount"]);
                dic2.Add("converate", item["converate"]);

                list.Add(dic2);
            }
            retData.Add("data", list);
            return retData;
        }

        /// <summary>
        /// 销售预测商机明细
        /// </summary>
        /// <param name="param"></param>
        /// <param name="sortby"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageCount"></param>
        /// <param name="userNum"></param>
        /// <returns></returns>
        public Dictionary<string, List<Dictionary<string, object>>> getSaleForecastOppList(Dictionary<string, object> param, Dictionary<string, string> sortby, int pageIndex, int pageCount, int userNum)
        {
            string rulesql = "";
            string datesql = "";
            string deptsql = "";
            string department = "";
            DateTime dtfrom;
            DateTime dtto;

            department = ReportParamsUtils.parseString(param, "department");
            dtfrom = ReportParamsUtils.parseDateTime(param, "searchfrom");
            dtto = ReportParamsUtils.parseDateTime(param, "searchto");
            if (department == null || department.Length == 0)
            {
                throw (new Exception("参数异常"));
            }
            if (dtfrom > dtto)
            {
                throw (new Exception("参数异常"));
            }
            if (dtfrom != DateTime.MinValue)
            {
                datesql = datesql + $" and e.reccreated >='{ dtfrom.ToString("yyyy-MM-dd 00:00:00")}' ";
            }
            if (dtto != DateTime.MinValue)
            {
                datesql = datesql + $" and e.reccreated <='{dtto.ToString("yyyy-MM-dd 23:59:59")}' ";
            }
            deptsql = $" and e.recmanager in (select userid from crm_sys_account_userinfo_relate where deptid in(select deptid  from crm_func_department_tree('{department}',1))) ";
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
                rulesql = rulesql + " and e.recid in (" + userRoleRuleSQL + ") ";
            }
            string totalSQL = $@"SELECT
								'{OPP_EntityID}' as categoryid,
								'立项项目' as categoryname,
								e.recname as oppname, 
								e.recid as recid,
								userInfo.username as recmanager_name,
								userInfo.userid as recmanager_id,
								customer.recid as customer_id,
								customer.recname as customer_name ,
								to_char(e.predeal,'yyyy-mm-dd') as predeal,
								salesstage.salesstageid,
								salesstage.stagename,
								salesstage.winrate * 100 winrate,
								e.reccreated,
								e.premoney
								FROM
								crm_sys_opportunity e
								INNER JOIN crm_sys_salesstage_setting salesstage ON e.recstageid = salesstage.salesstageid
								left outer join crm_sys_userinfo userInfo on userInfo.Userid =  e.recmanager
								left outer join crm_sys_customer customer on customer.recid::text = jsonb_extract_path_text(e.belongcustomer,'id')
								WHERE e.rectype = '{OPP_EntityID}' and e.recstatus = 1 and salesstage.winrate > 0
                                {datesql + deptsql + rulesql}
								order by e.reccreated desc
							";
            List<Dictionary<string, object>> data = _reportEngineRepository.ExecuteSQL(totalSQL, new DbParameter[] { });


            #region 报价阶段金额，取报价单的单一产品线最新的记录并且超过2000的金额

            var sql2 = $@"SELECT
						t1.recid as projectid,
						t1.reccreated,
						t2.productline,
						t2.sysmoney
						FROM
						(
							SELECT
							e.recid as recid,
							regexp_split_to_table(qoute.systemtypedetail,',')::uuid as detail,
							qoute.reccreated
							FROM
							crm_sys_opportunity e
							INNER JOIN crm_sys_salesstage_setting salesstage ON e.recstageid = salesstage.salesstageid
							inner join crm_fhsj_qoute qoute on (qoute.project->>'id')::uuid = e.recid and qoute.recstatus = 1
							WHERE e.rectype = '{OPP_EntityID}' and e.recstatus = 1 and salesstage.winrate > 0 and salesstage.recorder = 2
							and e.recid = '3f20f1ce-4a0a-4f97-8ea2-3048f79e2381'
							and (qoute.flowstatus = 0 or qoute.flowstatus = 1)
						) t1 inner join crm_fhsj_qoute_discount t2 on t2.recid = t1.detail and t2.recstatus = 1
						order by t1.recid,t1.reccreated
				";

            var data2 = _reportEngineRepository.ExecuteSQL(sql2, new DbParameter[] { });

            var data3 = new List<Dictionary<string, object>>();

            foreach (var item2 in data2)
            {
                if (data3.Count > 0)
                {
                    //添加
                    int add = 1;

                    foreach (var item3 in data3)
                    {
                        if (item3["projectid"].ToString().Equals(item2["projectid"].ToString()))
                        {
                            if (item2["productline"].ToString().Equals("1") || item2["productline"].ToString().Equals("10"))
                            {
                                if (item3["productline"].ToString().Equals("1") || item3["productline"].ToString().Equals("10"))
                                {
                                    if (Convert.ToDateTime(item2["reccreated"]) >= Convert.ToDateTime(item3["reccreated"]))
                                    {
                                        if (Convert.ToDecimal(item3["sysmoney"]) < 2000 || Convert.ToDecimal(item2["sysmoney"]) > Convert.ToDecimal(item3["sysmoney"]) || Convert.ToDecimal(item2["sysmoney"]) > 2000)
                                        {
                                            item3["sysmoney"] = item2["sysmoney"];
                                            item3["productline"] = item2["productline"];
                                            item3["reccreated"] = item2["reccreated"];
                                            add = 0;
                                        }
                                    }
                                    else if (Convert.ToDecimal(item3["sysmoney"]) < 2000 && Convert.ToDecimal(item2["sysmoney"]) >= 2000)
                                    {
                                        item3["sysmoney"] = item2["sysmoney"];
                                        item3["productline"] = item2["productline"];
                                        item3["reccreated"] = item2["reccreated"];
                                        add = 0;
                                    }
                                }
                            }
                            else if (item3["productline"].ToString().Equals(item2["productline"].ToString()))
                            {
                                if (Convert.ToDateTime(item2["reccreated"]) >= Convert.ToDateTime(item3["reccreated"]))
                                {
                                    if (Convert.ToDecimal(item3["sysmoney"]) < 2000 || Convert.ToDecimal(item2["sysmoney"]) > Convert.ToDecimal(item3["sysmoney"]) || Convert.ToDecimal(item2["sysmoney"]) > 2000)
                                    {
                                        item3["sysmoney"] = item2["sysmoney"];
                                        item3["productline"] = item2["productline"];
                                        item3["reccreated"] = item2["reccreated"];
                                        add = 0;
                                    }
                                }
                                else if (Convert.ToDecimal(item3["sysmoney"]) < 2000 && Convert.ToDecimal(item2["sysmoney"]) >= 2000)
                                {
                                    item3["sysmoney"] = item2["sysmoney"];
                                    item3["productline"] = item2["productline"];
                                    item3["reccreated"] = item2["reccreated"];
                                    add = 0;
                                }
                            }
                        }
                    }

                    if (add == 1)
                    {
                        data3.Add(item2);
                    }
                }
                else
                {
                    data3.Add(item2);
                }
            }

            var data5 = new List<Dictionary<string, object>>();

            foreach (var item3 in data3)
            {
                var add = 1;
                foreach (var item5 in data5)
                {
                    if (item3["projectid"].ToString().Equals(item5["recid"].ToString()))
                    {
                        item5["money"] = Convert.ToDecimal(item3["sysmoney"]) + Convert.ToDecimal(item5["money"]);
                        add = 0;
                        break;
                    }
                }
                if (add == 1)
                {
                    var dic5 = new Dictionary<string, object>();
                    dic5.Add("recid", item3["projectid"]);
                    dic5.Add("money", item3["sysmoney"]);
                    data5.Add(dic5);
                }
            }

            #endregion

            #region 签约和赢单阶段取合同，签约取合同金额

            var sql4 = $@"SELECT
						e.recid as recid,
						coalesce(sum(contract.contractamount),0) as money
						FROM
						crm_sys_opportunity e
						INNER JOIN crm_sys_salesstage_setting salesstage ON e.recstageid = salesstage.salesstageid
						inner join crm_sys_contract contract on (contract.opportunity->>'id')::uuid = e.recid and contract.recstatus = 1
						and (contract.flowstatus = 0 or contract.flowstatus = 1)
						WHERE e.rectype = '{OPP_EntityID}' and e.recstatus = 1 and salesstage.winrate > 0 and salesstage.recorder = 3
						group by e.recid

						union ALL

						SELECT
						e.recid as recid,
						coalesce(sum(contract.contractamount),0) as money
						FROM
						crm_sys_opportunity e
						INNER JOIN crm_sys_salesstage_setting salesstage ON e.recstageid = salesstage.salesstageid
						inner join crm_sys_contract contract on (contract.opportunity->>'id')::uuid = e.recid and contract.recstatus = 1 
						and contract.flowstatus = 1
						WHERE e.rectype = '{OPP_EntityID}' and e.recstatus = 1 and salesstage.winrate > 0 and salesstage.recorder = 4
						group by e.recid";

            var data4 = _reportEngineRepository.ExecuteSQL(sql4, new DbParameter[] { });

            #endregion

            var data6 = new Dictionary<string, decimal>();

            foreach (var item in data4)
            {
                data6.Add(item["recid"].ToString(), Convert.ToDecimal(item["money"]));
            }
            foreach (var item in data5)
            {
                data6.Add(item["recid"].ToString(), Convert.ToDecimal(item["money"]));
            }

            foreach (var item in data)
            {
                if (data6.ContainsKey(item["recid"].ToString()))
                {
                    item["premoney"] = Math.Round(Convert.ToDecimal(data6[item["recid"].ToString()]) * Convert.ToDecimal(item["winrate"]) / 100, 2);
                }
                else
                {
                    item["premoney"] = Math.Round(Convert.ToDecimal(item["premoney"]) * Convert.ToDecimal(item["winrate"]) / 100, 2);
                }
            }

            //foreach (var item in data)
            //{
            //	foreach (var item4 in data4)
            //	{
            //		if (item["recid"].ToString().Equals(item4["recid"].ToString()))
            //		{
            //			item["premoney"] = item4["money"];
            //		}
            //	}

            //	foreach (var item5 in data5)
            //	{
            //		if (item["recid"].ToString().Equals(item5["recid"].ToString()))
            //		{
            //			item["premoney"] = item5["money"];
            //		}
            //	}
            //}

            Dictionary<string, List<Dictionary<string, object>>> retData = new Dictionary<string, List<Dictionary<string, object>>>();
            retData.Add("data", data);
            return retData;
        }

        /// <summary>
        /// 销售漏斗预测
        /// </summary>
        /// <param name="param"></param>
        /// <param name="sortby"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageCount"></param>
        /// <param name="userNum"></param>
        /// <returns></returns>
        public Dictionary<string, List<Dictionary<string, object>>> getSaleForecastForDept(Dictionary<string, object> param, Dictionary<string, string> sortby, int pageIndex, int pageCount, int userNum)
        {
            #region 定义变量
            string rulesql = "";
            string datesql = "";
            string deptsql = "";
            string department = "";
            string opptypeid = "";
            DateTime dtfrom;
            DateTime dtto;
            #endregion

            #region 获取变量
            department = ReportParamsUtils.parseString(param, "department");
            dtfrom = ReportParamsUtils.parseDateTime(param, "searchfrom");
            dtto = ReportParamsUtils.parseDateTime(param, "searchto");
            opptypeid = ReportParamsUtils.parseString(param, "opptypeid");
            if (department == null || department.Length == 0)
            {
                throw (new Exception("参数异常"));
            }
            if (string.IsNullOrEmpty(opptypeid))
            {
                throw (new Exception("参数异常"));
            }
            //if (dtfrom == DateTime.MinValue || dtto == DateTime.MinValue)
            //{
            //	throw (new Exception("参数异常"));
            //}
            if (dtfrom > dtto)
            {
                throw (new Exception("参数异常"));
            }
            #endregion

            #region 处理数据权限
            if (dtfrom != DateTime.MinValue)
            {
                datesql = datesql + $" and t1.reccreated >='{ dtfrom.ToString("yyyy-MM-dd 00:00:00")}' ";
            }
            if (dtto != DateTime.MinValue)
            {
                datesql = datesql + $" and t1.reccreated <='{dtto.ToString("yyyy-MM-dd 23:59:59")}' ";
            }
            deptsql = $" and t1.recmanager in (select userid from crm_sys_account_userinfo_relate where deptid in(select deptid  from crm_func_department_tree('{department}',1))) ";
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
                rulesql = rulesql + " and t1.recid in (" + userRoleRuleSQL + ") ";
            }
            #endregion

            decimal money2 = 0;
            decimal money3 = 0;
            decimal money4 = 0;
            string oppTypeConditon = "";
            if (opptypeid == "0")
            {
                oppTypeConditon = " and t1.opptype not in (-1) ";
            }
            else
            {
                oppTypeConditon = " and t1.opptype in (" + opptypeid + ") ";
            }
            string sql =$@"
 				select 
				'立项项目' as categoryname,
				t2.dataval stagename,
				t2.recorder,
				count(1) as currentcount,
				case when t2.recorder = 1 then sum(count(1)) over()
				when t2.recorder = 2 then sum(case when t2.recorder > 1 then count(1) else 0 end) over()
				when t2.recorder = 3 then sum(case when t2.recorder > 2 then count(1) else 0 end) over()
				when t2.recorder = 4 then sum(case when t2.recorder > 3 then count(1) else 0 end) over()
				when t2.recorder = 5 then sum(case when t2.recorder > 4 then count(1) else 0 end) over()
				when t2.recorder = 6 then sum(case when t2.recorder > 5 then count(1) else 0 end) over()
				when t2.recorder = 7 then sum(case when t2.recorder > 6 then count(1) else 0 end) over()
				else 0 end as oppcount,
				0 as totalmoney,
				round((count(1) / sum(count(1)) over())*100,2) || '%' as proportion,
				case when t2.recorder = 0 then round(sum(case when t2.recorder >= 1 then count(1) else 0 end) over() * 100 
				/ sum(case when t2.recorder >= 0 then count(1) else 0 end) over(),2) || '%'
				when t2.recorder = 1 then round(sum(case when t2.recorder >= 2 then count(1) else 0 end) over() * 100 
				/ sum(case when t2.recorder >= 1 then count(1) else 0 end) over(),2) || '%'
				when t2.recorder = 2 then round(sum(case when t2.recorder >= 3 then count(1) else 0 end) over() * 100 
				/ sum(case when t2.recorder >= 2 then count(1) else 0 end) over(),2) || '%'
				when t2.recorder = 3 then round(sum(case when t2.recorder >= 4 then count(1) else 0 end) over() * 100 
				/ sum(case when t2.recorder >= 3 then count(1) else 0 end) over(),2) || '%'
				else '' end as converate
				from crm_cee_opportunity t1
				inner join crm_sys_dictionary t2 on t1.stage = t2.dataid and t2.dictypeid=58
				where t1.rectype = '{OPP_EntityID}'  and t1.recstatus = 1 and t2.recstatus = 1  
				{oppTypeConditon+datesql + deptsql + rulesql}
				group by t2.dataval,t2.recorder
				order by t2.recorder
			";

            var data = _reportEngineRepository.ExecuteSQL(sql, new DbParameter[] { });


            var retData = new Dictionary<string, List<Dictionary<string, object>>>();
            var retData2 = new List<Dictionary<string, object>>();
            var dic = new Dictionary<string, object>();
            dic.Add("categoryid", OPP_EntityID);
            dic.Add("categoryname", "立项项目");
            dic.Add("data", data);
            retData2.Add(dic);
            retData.Add("data", retData2);
            return retData;

        }
    }
}
