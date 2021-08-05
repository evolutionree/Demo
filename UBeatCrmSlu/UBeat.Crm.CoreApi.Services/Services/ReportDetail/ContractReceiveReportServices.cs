using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.IRepository;
using System.Data.Common;
using UBeat.Crm.CoreApi.Services.Utility;
using UBeat.Crm.CoreApi.Services.Models;

namespace UBeat.Crm.CoreApi.Services.Services.ReportDetail
{
    public class ContractReceiveReportServices: EntityBaseServices
    {
        private static string Contract_EntityId = "239a7c69-8238-413d-b1d9-a0d51651abfa";
        private IDynamicEntityRepository _dynamicEntityRepository;
        private IReportEngineRepository _reportEngineRepository;
        public ContractReceiveReportServices(IDynamicEntityRepository dynamicEntityRepository, IReportEngineRepository reportEngineRepository) {
            _dynamicEntityRepository = dynamicEntityRepository;
            _reportEngineRepository = reportEngineRepository;
        }



        /// <summary>
        /// 用于回款分析,
        /// 返回每个月内的合同额，以及截至目前为止该月所签的合同回款总额
        /// c1e7d7f5-d348-4f3e-91b1-d67f7c6aadef
        /// 
        /// </summary>
        /// <param name="param"></param>
        /// <param name="sortby"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageCount"></param>
        /// <param name="userNum"></param>
        /// <returns></returns>
        public Dictionary<string, List<Dictionary<string, object>>> getReceiveSummary(
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
            string DateTimeFieldName = "signdate";
            string receiveSQL = "SELECT (contract->>'id')::uuid as contractid,sum(paidmoney ) receiveAmount FROM crm_sys_payments group by contractid";
            #region 处理传入参数
            if (param == null) throw (new Exception("参数异常"));
            p_rangetype = ReportParamsUtils.parseInt(param, "range_type");
            if (p_rangetype != 1 && p_rangetype != 2) throw (new Exception("参数异常"));
            p_range = ReportParamsUtils.parseString(param, "range");
            if (p_range == null || p_range.Length == 0)  throw (new Exception("参数异常"));
            p_searchyear = ReportParamsUtils.parseInt(param, "searchyear");
            if (p_searchyear < 2000 || p_searchyear >= 2050)
            {
                throw (new Exception("参数异常"));
            }
            #endregion
            #region 处理ruleSQL
            string ruleSQL = " 1= 1";
            Guid userDeptInfo = Guid.Empty;
            UserData userData = GetUserData(userNum, false);
            if (userData != null && userData.AccountUserInfo != null && userData.AccountUserInfo.DepartmentId != null)
            {
                userDeptInfo = userData.AccountUserInfo.DepartmentId;
            }
            string userRoleRuleSQL = this._reportEngineRepository.getRuleSQLByUserId(Contract_EntityId, userNum, null);
            if (userRoleRuleSQL != null && userRoleRuleSQL.Length > 0)
            {
                userRoleRuleSQL = RuleSqlHelper.FormatRuleSql(userRoleRuleSQL, userNum, userDeptInfo);
                ruleSQL = ruleSQL + " And e.recid in (" + userRoleRuleSQL + ")";
            }
            #endregion
            string DateTimeSQL = string.Format(" extract(year from {0}.{1}) = {2} ", mainTableAlias, DateTimeFieldName, p_searchyear);
            string selectSQL = string.Format(@"Select extract(month from {0}.{1}) contractmonth,sum(e.contractamount) contractvolume,sum(receiveSum.receiveAmount) receiveAmount
                                from {2} {0} 
                                left outer join ({3}) receiveSum on receiveSum.contractid = {0}.recid
                                where {0}.recstatus = 1   ", mainTableAlias, DateTimeFieldName, mainTable , receiveSQL);
            
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
                for (int i = 1; i < tmp.Length; i++)
                {
                    personSql = personSql + ",'" + tmp[i] + "'";
                }
                personSql = string.Format("{0}.{1} in ({2})", mainTableAlias, personFieldName, personSql);
                totalSQL = string.Format("{0} and {1} and {2} and {3} group by contractmonth   order by contractmonth", selectSQL, ruleSQL, DateTimeSQL, personSql);
            }
            totalSQL = string.Format(@"Select totalmonth.fmonth contractmonth ,COALESCE(afterall.contractvolume,0) /10000 contractvolume,COALESCE(afterall.receiveAmount,0) /10000  receiveAmount
                                from ({0}) totalmonth left outer join ({1}) afterall on afterall.contractmonth =totalmonth.fmonth order by totalmonth.fmonth  ", this.get12MonthSQL(), totalSQL);
            List<Dictionary<string, object>> retList = _reportEngineRepository.ExecuteSQL(totalSQL, new DbParameter[] { });
            Dictionary<string, List<Dictionary<string, object>>> ret = new Dictionary<string, List<Dictionary<string, object>>>();
            ret.Add("data", retList);
            return ret;
        }

        /// <summary>
        /// 回款列表，用于回款分析报表
        /// 8c1ee441-7ffd-4c28-8f9b-3e214b2c7df2
        /// </summary>
        /// <param name="param"></param>
        /// <param name="sortby"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageCount"></param>
        /// <param name="userNum"></param>
        /// <returns></returns>
        public Dictionary<string, List<Dictionary<string, object>>> getReceiveList(
                            Dictionary<string, object> param,
                            Dictionary<string, string> sortby,
                            int pageIndex, int pageCount,
                            int userNum)
        {
            #region 定义变量
            int p_rangetype = 0;
            string p_range = "";
            int p_searchyear = 0;
            int p_searchmonth = 0;
            string mainTable = "crm_sys_contract";
            string mainTableAlias = "e";
            string personFieldName = "recmanager";
            string DateTimeFieldName = "contractdate";
            #endregion

            #region 处理传入参数
            if (param == null) throw (new Exception("参数异常"));
            p_rangetype = ReportParamsUtils.parseInt(param, "range_type");
            if (p_rangetype != 1 && p_rangetype != 2) throw (new Exception("参数异常"));
            p_range = ReportParamsUtils.parseString(param, "range");
            if (p_range == null || p_range.Length == 0) throw (new Exception("参数异常"));
            p_searchyear = ReportParamsUtils.parseInt(param, "searchyear");
            if (p_searchyear < 2000 || p_searchyear >= 2050)
            {
                throw (new Exception("参数异常"));
            }
            p_searchmonth = ReportParamsUtils.parseInt(param, "searchmonth");
            #endregion

            string DateTimeSQL = string.Format(" extract(year from {0}.{1}) = {2} ", mainTableAlias, DateTimeFieldName, p_searchyear);
            if (p_searchmonth > 0 && p_searchmonth <= 12)
            {
                DateTimeSQL = DateTimeSQL + " And  " + string.Format(" extract(month from {0}.{1}) = {2} ", mainTableAlias, DateTimeFieldName, p_searchmonth);
            }

            #region 处理ruleSQL
            string ruleSQL = " 1= 1";
            Guid userDeptInfo = Guid.Empty;
            UserData userData = GetUserData(userNum, false);
            if (userData != null && userData.AccountUserInfo != null && userData.AccountUserInfo.DepartmentId != null)
            {
                userDeptInfo = userData.AccountUserInfo.DepartmentId;
            }
            string userRoleRuleSQL = this._reportEngineRepository.getRuleSQLByUserId(Contract_EntityId, userNum, null);
            if (userRoleRuleSQL != null && userRoleRuleSQL.Length > 0)
            {
                userRoleRuleSQL = RuleSqlHelper.FormatRuleSql(userRoleRuleSQL, userNum, userDeptInfo);
                ruleSQL = ruleSQL + " And e.recid in (" + userRoleRuleSQL + ")";
            }
            #endregion
            string userdeptmapSQL = @"select  user_dept_relat.userid, department.deptid, department.deptname from crm_sys_account_userinfo_relate user_dept_relat 
                                    left outer join crm_sys_department department  on department.deptid = user_dept_relat.deptid ";
            string receiveSQL = "SELECT contractid,paidmoney  receiveamount,payer,paidtime  FROM crm_sys_payments  where recstatus = 1 and contractid is not null ";
            string selectSQL = string.Format(@"Select e.recid,e.recname,jsonb_extract_path_text(e.customerid,'id') customer_id,
                                    jsonb_extract_path_text(e.customerid,'name') customer_name,
                                    e.recmanager,useri.username recmanager_name ,
                                    dept.deptname  deptartment_name ,e.contractvolume  contractvolume,
                                    receiveSum.paidtime paymentdate ,receiveSum.receiveamount,receiveSum.payer
                                from {2} {0} 
                                left outer join ({3}) receiveSum on receiveSum.contractid = {0}.recid
                                left outer join crm_sys_userinfo useri on useri.userid= e.recmanager
                                left outer join ({4}) dept on dept.userid = useri.userid 
                                where {0}.recstatus = 1   ", mainTableAlias, DateTimeFieldName, mainTable, receiveSQL,userdeptmapSQL);

            string totalSQL = "";
            if (p_rangetype == 1)
            {
                //按团队
                string subDeptSQL = string.Format("select deptid  from crm_func_department_tree('{0}',1) ", p_range);
                string belongPerson = string.Format("select userid  from crm_sys_account_userinfo_relate where deptid in({0}) ", subDeptSQL);
                string personSql = string.Format("{0}.{1} in ({2})", mainTableAlias, personFieldName, belongPerson);
                totalSQL = string.Format("{0} and {1} and {2} and {3}  ", selectSQL, ruleSQL, DateTimeSQL, personSql);
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
                totalSQL = string.Format("{0} and {1} and {2} and {3} ", selectSQL, ruleSQL, DateTimeSQL, personSql);
            }
            List<Dictionary<string, object>> retList = _reportEngineRepository.ExecuteSQL(totalSQL, new DbParameter[] { });
            Dictionary<string, List<Dictionary<string, object>>> ret = new Dictionary<string, List<Dictionary<string, object>>>();
            ret.Add("data", retList);
            return ret;
        }
        /// <summary>
        /// c24d8a53-a8d1-4963-a2f6-7946acd6126d
        /// 合同回款汇总(用于合同回款报表)
        /// </summary>
        /// <param name="param"></param>
        /// <param name="sortby"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageCount"></param>
        /// <param name="userNum"></param>
        /// <returns></returns>
        public Dictionary<string, List<Dictionary<string, object>>> getReceiveSummaryByReceiveBill(
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
            string receiveSQL = @"SELECT (contract->>'id')::uuid as contractid,extract(year from paidtime) fyear ,extract(month from paidtime) fmonth , paidmoney  receiveAmount 
                                 FROM crm_sys_payments 
                                  where recstatus = 1 ";
            #region 处理传入参数
            if (param == null) throw (new Exception("参数异常"));
            p_rangetype = ReportParamsUtils.parseInt(param, "range_type");
            if (p_rangetype != 1 && p_rangetype != 2) throw (new Exception("参数异常"));
            p_range = ReportParamsUtils.parseString(param, "range");
            if (p_range == null || p_range.Length == 0) throw (new Exception("参数异常"));
            p_searchyear = ReportParamsUtils.parseInt(param, "searchyear");
            if (p_searchyear < 2000 || p_searchyear >= 2050)
            {
                throw (new Exception("参数异常"));
            }
            #endregion
            #region 处理ruleSQL
            string ruleSQL = " 1= 1";
            Guid userDeptInfo = Guid.Empty;
            UserData userData = GetUserData(userNum, false);
            if (userData != null && userData.AccountUserInfo != null && userData.AccountUserInfo.DepartmentId != null)
            {
                userDeptInfo = userData.AccountUserInfo.DepartmentId;
            }
            string userRoleRuleSQL = this._reportEngineRepository.getRuleSQLByUserId(Contract_EntityId, userNum, null);
            if (userRoleRuleSQL != null && userRoleRuleSQL.Length > 0)
            {
                userRoleRuleSQL = RuleSqlHelper.FormatRuleSql(userRoleRuleSQL, userNum, userDeptInfo);
                ruleSQL = ruleSQL + " And e.recid in (" + userRoleRuleSQL + ")";
            }
            #endregion
            string DateTimeSQL = string.Format(" receiveSum.fyear = {0} ", p_searchyear);
            string selectSQL = string.Format(@"Select receiveSum.fmonth  paidmonth ,sum(receiveSum.receiveAmount) receiveAmount
                                from {2} {0} 
                                inner join ({3}) receiveSum on receiveSum.contractid = {0}.recid
                                where {0}.recstatus = 1   ", mainTableAlias, DateTimeFieldName, mainTable, receiveSQL);

            string totalSQL = "";
            if (p_rangetype == 1)
            {
                //按团队
                string subDeptSQL = string.Format("select deptid  from crm_func_department_tree('{0}',1) ", p_range);
                string belongPerson = string.Format("select userid  from crm_sys_account_userinfo_relate where deptid in({0}) ", subDeptSQL);
                string personSql = string.Format("{0}.{1} in ({2})", mainTableAlias, personFieldName, belongPerson);
                totalSQL = string.Format("{0} and {1} and {2} and {3} group by paidmonth order by paidmonth", selectSQL, ruleSQL, DateTimeSQL, personSql);
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
                totalSQL = string.Format("{0} and {1} and {2} and {3} group by paidmonth   order by paidmonth", selectSQL, ruleSQL, DateTimeSQL, personSql);
            }
            totalSQL = string.Format(@"Select totalmonth.fmonth paidmonth ,COALESCE(afterall.receiveAmount,0)  /10000 receiveAmount
                                from ({0}) totalmonth left outer join ({1}) afterall on afterall.paidmonth =totalmonth.fmonth order by totalmonth.fmonth  ", this.get12MonthSQL(), totalSQL);

            List<Dictionary<string, object>> retList = _reportEngineRepository.ExecuteSQL(totalSQL, new DbParameter[] { });
            Dictionary<string, List<Dictionary<string, object>>> ret = new Dictionary<string, List<Dictionary<string, object>>>();
            ret.Add("data", retList);
            return ret;
        }
        /// <summary>
        /// 合同回款列表(用于合同回款报表)
        /// 19b4e627-9f1b-41b1-99df-6c3dc7d751a3
        /// </summary>
        /// <param name="param"></param>
        /// <param name="sortby"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageCount"></param>
        /// <param name="userNum"></param>
        /// <returns></returns>
        public Dictionary<string, List<Dictionary<string, object>>> getReceiveListByReceiveBill(
                            Dictionary<string, object> param,
                            Dictionary<string, string> sortby,
                            int pageIndex, int pageCount,
                            int userNum)
        {
            #region 定义变量
            int p_rangetype = 0;
            string p_range = "";
            int p_searchyear = 0;
            int p_searchmonth = 0;
            string mainTable = "crm_sys_contract";
            string mainTableAlias = "e";
            string personFieldName = "recmanager";
            string DateTimeFieldName = "contractdate";
            #endregion

            #region 处理传入参数
            if (param == null) throw (new Exception("参数异常"));
            p_rangetype = ReportParamsUtils.parseInt(param, "range_type");
            if (p_rangetype != 1 && p_rangetype != 2) throw (new Exception("参数异常"));
            p_range = ReportParamsUtils.parseString(param, "range");
            if (p_range == null || p_range.Length == 0) throw (new Exception("参数异常"));
            p_searchyear = ReportParamsUtils.parseInt(param, "searchyear");
            if (p_searchyear < 2000 || p_searchyear >= 2050)
            {
                throw (new Exception("参数异常"));
            }
            p_searchmonth = ReportParamsUtils.parseInt(param, "searchmonth");
            #endregion

            string DateTimeSQL = string.Format(" receiveSum.fyear = {0} ", p_searchyear);
            if (p_searchmonth > 0 && p_searchmonth <= 12)
            {
                DateTimeSQL = DateTimeSQL + " And  " + string.Format(" receiveSum.fmonth = {0} ", p_searchmonth);
            }

            #region 处理ruleSQL
            string ruleSQL = " 1= 1";
            Guid userDeptInfo = Guid.Empty;
            UserData userData = GetUserData(userNum, false);
            if (userData != null && userData.AccountUserInfo != null && userData.AccountUserInfo.DepartmentId != null)
            {
                userDeptInfo = userData.AccountUserInfo.DepartmentId;
            }
            string userRoleRuleSQL = this._reportEngineRepository.getRuleSQLByUserId(Contract_EntityId, userNum, null);
            if (userRoleRuleSQL != null && userRoleRuleSQL.Length > 0)
            {
                userRoleRuleSQL = RuleSqlHelper.FormatRuleSql(userRoleRuleSQL, userNum, userDeptInfo);
                ruleSQL = ruleSQL + " And e.recid in (" + userRoleRuleSQL + ")";
            }
            #endregion
            string userdeptmapSQL = @"select  user_dept_relat.userid, department.deptid, department.deptname from crm_sys_account_userinfo_relate user_dept_relat 
                                    left outer join crm_sys_department department  on department.deptid = user_dept_relat.deptid ";

            string receiveSQL = "SELECT contractid ,extract(year from paidtime) fyear ,extract(month from paidtime) fmonth ,paidmoney   receiveamount,payer,paidtime  FROM crm_sys_payments  where recstatus = 1 and contractid is not null ";
            string selectSQL = string.Format(@"Select e.recid,e.recname,jsonb_extract_path_text(e.customerid,'id') customer_id,
                                    jsonb_extract_path_text(e.customerid,'name') customer_name,
                                    e.recmanager,useri.username recmanager_name ,
                                    dept.deptname deptartment_name ,e.contractvolume  contractvolume,
                                    receiveSum.paidtime paymentdate ,receiveSum.receiveamount,receiveSum.payer
                                from {2} {0} 
                                inner join ({3}) receiveSum on receiveSum.contractid = {0}.recid
                                left outer join crm_sys_userinfo useri on useri.userid= e.recmanager
                                left outer join ({4}) dept on dept.userid = useri.userid 
                                where {0}.recstatus = 1   ", mainTableAlias, DateTimeFieldName, mainTable, receiveSQL, userdeptmapSQL);

            string totalSQL = "";
            if (p_rangetype == 1)
            {
                //按团队
                string subDeptSQL = string.Format("select deptid  from crm_func_department_tree('{0}',1) ", p_range);
                string belongPerson = string.Format("select userid  from crm_sys_account_userinfo_relate where deptid in({0}) ", subDeptSQL);
                string personSql = string.Format("{0}.{1} in ({2})", mainTableAlias, personFieldName, belongPerson);
                totalSQL = string.Format("{0} and {1} and {2} and {3}  ", selectSQL, ruleSQL, DateTimeSQL, personSql);
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
                totalSQL = string.Format("{0} and {1} and {2} and {3} ", selectSQL, ruleSQL, DateTimeSQL, personSql);
            }
            List<Dictionary<string, object>> retList = _reportEngineRepository.ExecuteSQL(totalSQL, new DbParameter[] { });
            Dictionary<string, List<Dictionary<string, object>>> ret = new Dictionary<string, List<Dictionary<string, object>>>();
            ret.Add("data", retList);
            return ret;
        }
        private string get12MonthSQL()
        {
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
    }
}
