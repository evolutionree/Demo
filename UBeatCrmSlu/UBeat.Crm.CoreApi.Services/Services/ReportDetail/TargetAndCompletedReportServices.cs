using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.SalesTarget;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.DomainModel.Vocation;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.Vocation;
using Newtonsoft.Json;
using UBeat.Crm.CoreApi.DomainModel.EntityPro;
using System.Linq;
using System.Data.Common;
using Newtonsoft.Json.Linq;
using UBeat.Crm.CoreApi.Services.Models.SalesTarget;
using AutoMapper;
using UBeat.Crm.CoreApi.Services.Models.Rule;
using UBeat.Crm.CoreApi.DomainMapper.Rule;
using UBeat.Crm.CoreApi.Services.Models.Excels;
using UBeat.Crm.CoreApi.Services.Utility.ExcelUtility;
using Microsoft.Extensions.Caching.Memory;
using UBeat.Crm.CoreApi.DomainModel.DynamicEntity;
using UBeat.Crm.CoreApi.DomainModel.Reports;
using UBeat.Crm.CoreApi.DomainModel.Utility;
using UBeat.Crm.CoreApi.Services.Utility;

namespace UBeat.Crm.CoreApi.Services.Services.ReportDetail
{
    public class TargetAndCompletedReportServices : EntityBaseServices
    {
        private static string SaleOppoEntityID = "2c63b681-1de9-41b7-9f98-4cf26fd37ef1";//关联商机的算法是特殊的
        private ISalesTargetRepository _salesTargetRepository;
        private IDynamicEntityRepository _dynamicEntityRepository;
        private IReportEngineRepository _reportEngineRepository;
        private IEntityProRepository _entityProRepository;
        private ITargetAndCompletedReportRepository _targetAndCompletedReportRepository;
        public TargetAndCompletedReportServices(ISalesTargetRepository salesTargetRepository, 
                IDynamicEntityRepository dynamicEntityRepository, 
                IReportEngineRepository reportEngineRepository,
                IEntityProRepository entityProRepository,
            ITargetAndCompletedReportRepository targetAndCompletedReportRepository)
        {
            _salesTargetRepository = salesTargetRepository;
            _dynamicEntityRepository = dynamicEntityRepository;
            _reportEngineRepository = reportEngineRepository;
            _targetAndCompletedReportRepository = targetAndCompletedReportRepository;
            _entityProRepository = entityProRepository;
        }


        /// <summary>
        /// 根据过滤条件获取目标及完成情况汇总值
        /// 用于移动端和WEB端使用
        /// 主要返回目标总金额（targetamount），完成总金额(completedamount), 完成率（completedrate），查询范围（targetamount），时间范围（fromdate,todate）
        /// </summary>
        /// <param name="param"></param>
        /// <param name="sortby"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageCount"></param>
        /// <param name="userNum"></param>
        /// <returns></returns>
        public Dictionary<string, List<Dictionary<string, object>>> getTargetTotalSummary(
                            Dictionary<string, object> param,
                            Dictionary<string, string> sortby,
                            int pageIndex, int pageCount,
                            int userNum)
        {

            param = copyParam(param);
            Dictionary<string, List<Dictionary<string, object>>> ret = new Dictionary<string, List<Dictionary<string, object>>>();
            string targetid = param["targetid"].ToString();
            targetid = targetid.Replace("'", "''");
            string mainTable = "";
            string mainTableAlias = "e";
            string ruleSQL = "1=1";
            string dateTimeSQL = "";
            string userIdFieldName = "recmanager";
            string dateTimeFieldName = "reccreated";
            string salestageSQL = "";
            string measureUnit = "万元";
            string calcFieldName = "";
            int unit = 10000;
            int p_year = int.Parse(param["year"].ToString());
            int p_unit = ReportParamsUtils.parseInt(param, "unit");
            if (p_year < 2000 || p_year > 2500)
            {
                throw (new Exception("年份不正确"));
            }
            if (p_unit == 1) {
                unit = 1;
            }
            /*int p_from = int.Parse(param["month_from"].ToString());
            if (p_from <= 0 || p_from > 12)
            {
                throw (new Exception("月份不正确"));
            }
            int p_to = int.Parse(param["month_to"].ToString());
            if (p_to <= 0 || p_to > 12)
            {
                throw (new Exception("月份不正确"));
            }
            if (p_from > p_to)
            {
                throw (new Exception("月份不正确"));
            }*/
            int p_from = 1;
            int p_to = 12;
            p_from = ReportParamsUtils.parseInt(param, "month_from");
            p_to = ReportParamsUtils.parseInt(param, "month_to");
            if (p_from <= 0 || p_to <= 0)
            {
                p_from = 1;
                p_to = 12;
            }
            int p_rangetype = int.Parse(param["range_type"].ToString());
            if (p_rangetype != 1 && p_rangetype != 2)
            {
                throw (new Exception("查询范围类型不正确，请选择团队或者个人"));
            }
            string p_range = param["range"].ToString();
            p_range = p_range.Replace("'", "''");
            if (p_range.Length == 0)
            {
                throw (new Exception("查询范围异常"));
            }
            string personSql = "";
            string sumSql = "";
            DateTime dt_from = new DateTime(p_year, p_from, 1, 0, 0, 0);
            DateTime dt_to = new DateTime(p_year, p_to, 1, 23, 59, 59);
            dt_to = dt_to.AddMonths(1);
            dt_to = dt_to.AddDays(-1);
            var sqlParams = new DbParameter[] {
                new Npgsql.NpgsqlParameter("dtfrom",dt_from),
                new Npgsql.NpgsqlParameter("dtto",dt_to)
            };

            List<SalesTargetNormRuleMapper> targetInfo = getTargetInfo(targetid, userNum);
            if (!(targetInfo != null && targetInfo.Count > 0))
            {
                return testData(param, sortby, pageIndex, pageCount, userNum);
            }

            SalesTargetNormRuleMapper firstInfo = targetInfo[0];
            if (firstInfo.BizDateFieldName != null && firstInfo.BizDateFieldName.Length > 0) {
                dateTimeFieldName = firstInfo.BizDateFieldName;
            }
            calcFieldName = firstInfo.FieldName;
            #region 处理数据权限
            string targetRuleSQL = "";
            string userRoleRuleSQL = "";
            Guid userDeptInfo = Guid.Empty;
            //先处理目标的数据全系
            UserData userData = GetUserData(userNum, false);
            if (userData != null && userData.AccountUserInfo != null && userData.AccountUserInfo.DepartmentId != null)
            {
                userDeptInfo = userData.AccountUserInfo.DepartmentId;
            }
            if (firstInfo.RuleId != null && firstInfo.RuleId.Length > 0 && firstInfo.RuleId != Guid.Empty.ToString())
            {
                targetRuleSQL = this._reportEngineRepository.getRuleSQLByRuleId(firstInfo.EntityId, firstInfo.RuleId, userNum, null);
            }
            //处理角色数据权限 
            userRoleRuleSQL = this._reportEngineRepository.getRuleSQLByUserId(firstInfo.EntityId, userNum, null);
            ruleSQL = " 1=1 ";
            if (targetRuleSQL != null && targetRuleSQL.Length > 0)
            {
                targetRuleSQL = RuleSqlHelper.FormatRuleSql(targetRuleSQL, userNum, userDeptInfo);
                ruleSQL = ruleSQL + "and e.recid in (" + targetRuleSQL + ")";

            }
            if (userRoleRuleSQL != null && userRoleRuleSQL.Length > 0)
            {
                userRoleRuleSQL = RuleSqlHelper.FormatRuleSql(userRoleRuleSQL, userNum, userDeptInfo);
                ruleSQL = ruleSQL + " And e.recid in (" + userRoleRuleSQL + ")";
            }

            #endregion
            //处理期间脚本
            dateTimeSQL = string.Format("({0}.{1} >=@dtfrom and {2}.{3} <=@dtto)", mainTableAlias, dateTimeFieldName, mainTableAlias, dateTimeFieldName);
            //处理范围脚本（按团队还是按个人）
            if (p_rangetype == 1)
            {
                //按团队,不可多选(需要跟主sql进行left outer join )·
            }
            else
            {
                //按个人，可多选（并入主实体即可）
                string[] tmp = p_range.Split(',');
                personSql = " 1 <> 1 ";
                foreach (string item in tmp)
                {
                    personSql = personSql + string.Format(" or {0}.{1} = {2}", mainTableAlias, "recmanager", item);
                }
                personSql = "(" + personSql + ")";
            }
            //获取主表
            Dictionary<string, object> entityInfo = _dynamicEntityRepository.getEntityBaseInfoById(Guid.Parse(firstInfo.EntityId), userNum);
            if (entityInfo == null) return null;
            mainTable = entityInfo["entitytable"].ToString();

            if (SaleOppoEntityID == firstInfo.EntityId)
            {
                //预测销售目标是有特殊算法的
                if (calcFieldName.ToLower() == "recid")
                {
                    sumSql = string.Format("count({0}.{1}) completed", mainTableAlias, calcFieldName);
                    measureUnit = "个";
                }
                else
                {
                    sumSql = string.Format("sum({0}.{1} * salestage.winrate /{2}) completed", mainTableAlias, calcFieldName,unit);
                    salestageSQL = string.Format(" left outer join crm_sys_salesstage_setting salestage on salestage.salesstageid={0}.recstageid ", mainTableAlias);
                }
            }
            else
            {
                if (calcFieldName.ToLower() == "recid")
                {
                    sumSql = string.Format("count({0}.{1}) completed", mainTableAlias, calcFieldName);
                    measureUnit = "个";
                }
                else
                {
                    sumSql = string.Format("sum({0}.{1} /{2}) completed", mainTableAlias, calcFieldName, unit);
                }
            }
            string totalSql = "";
            List<Dictionary<string, object>> retList = null;
            if (p_rangetype == 1)
            {
                //按团队
                string subDeptSQL = string.Format("select deptid  from crm_func_department_tree('{0}',1) ", p_range);
                personSql = "1=1";
                totalSql = string.Format(@"select {8} userid, extract(year from {11}.{0} ) as year  , extract(month from {12}.{1} ) as  month ,{2} from {3} as {4} {10}  
                        where  1=1 and {5} and {6} and {7}  
                        group by {9},EXTRACT(year FROM {11}.{0}),EXTRACT(month FROM {11}.{0})  ", dateTimeFieldName, dateTimeFieldName, sumSql, mainTable, mainTableAlias, ruleSQL, dateTimeSQL, personSql, userIdFieldName, userIdFieldName, salestageSQL, mainTableAlias, mainTableAlias);
                totalSql = "Select userid,year,month,sum(completed) completed from (" + totalSql + ")ddd  group by userid,year,month";

                string userDeptSQL = generateDeptAndUserFromTarget(targetid, p_year, p_from, p_to);
                string completedSQL = string.Format(@"Select userMonthSQL.year,userMonthSQL.month ,sum(completed) completed 
                            from ({0}) userMonthSQL left outer join ({1}) ok on userMonthSQL.year = ok.year and  userMonthSQL.month = ok.month and userMonthSQL.userid =ok.userid
                            where userMonthSQL.departmentid in ({2}) group by  userMonthSQL.year,userMonthSQL.month ", userDeptSQL, totalSql, subDeptSQL);
                string targetSQL = generateTargetSQLForDept(targetid, p_range, p_year, p_from, p_to);
                string realSQL = string.Format(@"Select  
                                                       COALESCE(targetsql.year,okouter.year) as year ,
                                                        COALESCE(targetsql.month,okouter.month) as month ,COALESCE(targetsql.target,0)  as  targetamount,okouter.completed completedamount 
                                           from ({0} ) as targetsql 
                                                        full outer join ({1}) as okouter on targetsql.year = okouter.year and targetsql.month = okouter.month", 
                                                        targetSQL, completedSQL);
                realSQL = string.Format(@"select sum(totalTarget.targetamount) targetamount ,sum(totalTarget.completedamount) completedamount from ({0}) totalTarget", realSQL);
                retList = _reportEngineRepository.ExecuteSQL(realSQL, sqlParams);
                
            }
            else
            {
                //按个人
                totalSql = string.Format(@"select extract(year from {9}.{0} ) as year  , extract(month from {10}.{1} ) as  month ,{2} from {3} as {4} {8} 
                                where  1=1 and {5} and {6} and {7}  
                                group by EXTRACT(year FROM {9}.{0}),EXTRACT(month FROM {9}.{0})  ", dateTimeFieldName, dateTimeFieldName, sumSql, mainTable, mainTableAlias, ruleSQL, dateTimeSQL, personSql, salestageSQL, mainTableAlias, mainTableAlias);
                totalSql = "Select year,month,sum(completed) completed from (" + totalSql + ")ddd  group by year,month";
                string targetSQL = generateTargetSQLForUser(targetid,p_range, p_year, p_from, p_to);
                string realSQL = string.Format(@"Select COALESCE(targetsql.year,okouter.year) as year ,
                                                        COALESCE(targetsql.month,okouter.month) as month ,COALESCE(targetsql.target,0)  as  targetamount,okouter.completed completedamount  
                                    from ({0} ) as targetsql full outer join ({1}) as okouter on targetsql.year = okouter.year and targetsql.month = okouter.month", targetSQL, totalSql);

                //执行脚本即可
                realSQL = string.Format(@"select sum(totalTarget.targetamount) targetamount ,sum(totalTarget.completedamount) completedamount from ({0}) totalTarget", realSQL);
                retList = _reportEngineRepository.ExecuteSQL(realSQL, sqlParams);
                
            }
            if (retList != null && retList.Count > 0)
            {
                Dictionary<string, object> dict = retList[0];
                Decimal completedAmount = new Decimal(0.0);
                Decimal targetAmount = new Decimal(0.0);
                Decimal completedPrecent = new Decimal(0.00);
                if (dict["completedamount"] != null)
                {
                    Decimal.TryParse(dict["completedamount"].ToString(), out completedAmount);
                }
                else
                {
                    dict["completedamount"] = new Decimal(0);
                }
                if (dict.ContainsKey("completedamount") == false) {
                    dict.Add("completedamount", new Decimal(0));
                }
                if (dict["targetamount"] != null)
                {
                    Decimal.TryParse(dict["targetamount"].ToString(), out targetAmount);
                }
                else {
                    dict["targetamount"] = new Decimal(0);
                }
                if (dict.ContainsKey("targetamount") == false)
                {
                    dict.Add("targetamount", new Decimal(0));
                }
                if (targetAmount == new Decimal(0.00))
                {
                    completedPrecent = new Decimal(100.00);
                }
                else
                {
                    Decimal result = completedAmount * 100 *unit /10000 / targetAmount;
                    completedPrecent = Math.Round(result, 2);
                }
                #region 如果是1月到12月的话，重新用yearcount来替代1-12月的目标汇总(这个功能暂定不要)
                if (p_from == 1 && p_to == 12 && false ) {
                    string adjustSQL = "";
                    if (p_rangetype == 1)
                    {
                        adjustSQL  = this.generateTargetYearCountSQLForDept(targetid, p_range, p_year);
                    }
                    else {
                        adjustSQL = this.generateTargetYearCountSQLForUser(targetid, p_range, p_year);
                    }
                    if (adjustSQL != null && adjustSQL.Length == 0) {
                        List < Dictionary < string, object>>  adjustDataList = this._reportEngineRepository.ExecuteSQL(adjustSQL, new DbParameter[] { });
                        if (adjustDataList != null && adjustDataList.Count > 0) {
                            Dictionary<string, object> adjustData = adjustDataList[0];
                            if (adjustData.ContainsKey("target") && adjustData["target"] != null ) {
                                Decimal tmpTarget = new Decimal(0);
                                if (Decimal.TryParse(adjustData["target"].ToString(), out tmpTarget)) {
                                    dict["targetamount"] = tmpTarget;
                                }
                            }
                        }
                    }
                }
                #endregion 
                dict.Add("completedrate", completedPrecent);
                ret.Add("data", retList);
            }
            #region 处理参数值，回传至请前端，用于手机前端显示
            if (ret.ContainsKey("data")) {
                Dictionary<string, object> dict = ret["data"][0];
                string outRange = "";
                //处理类型
                if (p_rangetype == 1)
                {
                    //处理部门
                    string cmdText = "Select deptname from crm_sys_department where deptid ='" + p_range.Replace("'", "''") + "'";
                    List<Dictionary<string,object>> tmp = this._reportEngineRepository.ExecuteSQL(cmdText, new DbParameter []{ });
                    if (tmp != null && tmp.Count > 0) {
                        outRange = tmp[0]["deptname"].ToString();
                    }
                }
                else {
                    string cmdText = "select array_to_string(array_agg(username),',') allusers from crm_sys_userinfo   where userid in (" + p_range + ")";

                    List<Dictionary<string, object>> tmp = this._reportEngineRepository.ExecuteSQL(cmdText, new DbParameter[] { });
                    if (tmp != null && tmp.Count > 0)
                    {
                        outRange = tmp[0]["allusers"].ToString();
                    }
                }
                dict.Add("range", outRange);
                //处理from日期
                string out_fromdate = string.Format("{0}-{1}", p_year, p_from);
                dict.Add("fromdate", out_fromdate);

                //处理to日期
                string out_todate = string.Format("{0}-{1}", p_year, p_to);
                dict.Add("todate", out_todate);
                dict.Add("measureunit", measureUnit);
            }
            #endregion

            return ret;
        }
        /// <summary>
        /// 根据过滤条件获取范围内按月汇总的目标和完成值。//按月汇总
        /// 仅供WEB端使用
        /// </summary>
        /// <param name="param"></param>
        /// <param name="sortby"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageCount"></param>
        /// <param name="userNum"></param>
        /// <returns></returns>
        public Dictionary<string, List<Dictionary<string, object>>> getTargetSummary(
                            Dictionary<string, object> param,
                            Dictionary<string, string> sortby,
                            int pageIndex, int pageCount,
                            int userNum)
        {

            param = copyParam(param);
            Dictionary<string, List<Dictionary<string, object>>> ret = new Dictionary<string, List<Dictionary<string, object>>>();
            string targetid = param["targetid"].ToString();
            targetid = targetid.Replace("'", "''");
            string mainTable = "";
            string mainTableAlias = "e";
            string ruleSQL = "1=1";
            string dateTimeSQL = "";
            string userIdFieldName = "recmanager";
            string dateTimeFieldName = "reccreated";
            string salestageSQL = "";
            string measureUnit = "万元";
            string measureRate = "10000";

            string calcFieldName = "";
            int p_year = int.Parse(param["year"].ToString());
            int p_unit = ReportParamsUtils.parseInt(param, "unit");
            if (p_unit == 1) {
                measureUnit = "元";
                measureRate = "1";
            }
            if (p_year < 2000 || p_year > 2500) {
                throw (new Exception("年份不正确"));
            }

            /*int p_from = int.Parse(param["month_from"].ToString());
            if (p_from <= 0 || p_from > 12) {
                throw (new Exception("月份不正确"));
            }
            int p_to = int.Parse(param["month_to"].ToString());
            if (p_to <= 0 || p_to > 12)
            {
                throw (new Exception("月份不正确"));
            }
            if (p_from > p_to)
            {
                throw (new Exception("月份不正确"));
            }*/
            int p_from = 1;
            int p_to = 12;
            p_from = ReportParamsUtils.parseInt(param, "month_from");
            p_to = ReportParamsUtils.parseInt(param, "month_to");
            if (p_from <= 0 || p_to <= 0) {
                p_from = 1;
                p_to = 12;
            }
            int p_rangetype = int.Parse(param["range_type"].ToString());
            if (p_rangetype != 1 && p_rangetype != 2) {
                throw (new Exception("查询范围类型不正确，请选择团队或者个人"));
            }
            string p_range = param["range"].ToString();
            p_range = p_range.Replace("'", "''");
            if (p_range.Length == 0) {
                throw (new Exception("查询范围异常"));
            }
            string personSql = "";
            string sumSql = "";
            DateTime dt_from = new DateTime(p_year, p_from, 1, 0, 0, 0);
            DateTime dt_to = new DateTime(p_year, p_to, 1, 23, 59, 59);
            dt_to = dt_to.AddMonths(1);
            dt_to = dt_to.AddDays(-1);
            var sqlParams = new DbParameter[] {
                new Npgsql.NpgsqlParameter("dtfrom",dt_from),
                new Npgsql.NpgsqlParameter("dtto",dt_to)
            };

            List<SalesTargetNormRuleMapper> targetInfo = getTargetInfo(targetid, userNum);
            if (!(targetInfo != null && targetInfo.Count > 0)) {
                return testData(param, sortby, pageIndex, pageCount, userNum);
            }
            SalesTargetNormRuleMapper firstInfo = targetInfo[0];
            if (firstInfo.BizDateFieldName != null && firstInfo.BizDateFieldName.Length > 0) {
                dateTimeFieldName = firstInfo.BizDateFieldName;
            }
            calcFieldName = firstInfo.FieldName;
            #region 处理数据权限
            string targetRuleSQL = "";
            string userRoleRuleSQL = "";
            Guid userDeptInfo =Guid.Empty;
            //先处理目标的数据全系
            UserData userData = GetUserData(userNum, false);
            if (userData != null && userData.AccountUserInfo != null && userData.AccountUserInfo.DepartmentId != null) {
                userDeptInfo = userData.AccountUserInfo.DepartmentId;
            }
            if (firstInfo.RuleId != null && firstInfo.RuleId.Length > 0 && firstInfo.RuleId != Guid.Empty.ToString()) {
                targetRuleSQL = this._reportEngineRepository.getRuleSQLByRuleId(firstInfo.EntityId,firstInfo.RuleId, userNum, null);
            }
            //处理角色数据权限 
            userRoleRuleSQL = this._reportEngineRepository.getRuleSQLByUserId(firstInfo.EntityId, userNum,null);
            ruleSQL = " 1=1 ";
            if (targetRuleSQL != null && targetRuleSQL.Length > 0) {
                targetRuleSQL = RuleSqlHelper.FormatRuleSql(targetRuleSQL, userNum, userDeptInfo);
                ruleSQL = ruleSQL + "and e.recid in (" + targetRuleSQL + ")";
                
            }
            if (userRoleRuleSQL != null && userRoleRuleSQL.Length > 0)
            {
                userRoleRuleSQL = RuleSqlHelper.FormatRuleSql(userRoleRuleSQL, userNum, userDeptInfo);
                ruleSQL = ruleSQL + " And e.recid in (" + userRoleRuleSQL + ")";
            }
            
            #endregion
            //处理期间脚本
            //处理范围脚本（按团队还是按个人）
            dateTimeSQL = string.Format("({0}.{1} >=@dtfrom  and {2}.{3} <=@dtto)", mainTableAlias, dateTimeFieldName, mainTableAlias, dateTimeFieldName);
            if (p_rangetype == 1)
            {
                //按团队,不可多选(需要跟主sql进行left outer join )
            }
            else {
                //按个人，可多选（并入主实体即可）
                string[] tmp = p_range.Split(',');
                personSql = " 1 <> 1 ";
                foreach (string item in tmp) {
                    personSql = personSql + string.Format(" or {0}.{1} = {2}", mainTableAlias, "recmanager", item);
                }
                personSql = "(" + personSql + ")";
            }
            //获取主表
            Dictionary<string, object> entityInfo = _dynamicEntityRepository.getEntityBaseInfoById(Guid.Parse(firstInfo.EntityId), userNum);
            if (entityInfo == null) return null;
            mainTable = entityInfo["entitytable"].ToString();

            if (SaleOppoEntityID == firstInfo.EntityId) {
                //预测销售目标是有特殊算法的
                if (calcFieldName.ToLower() == "recid")
                {
                    sumSql = string.Format("count({0}.{1}) completed", mainTableAlias, calcFieldName);
                    measureUnit = "个";
                }
                else
                {
                    sumSql = string.Format("sum({0}.{1} * salestage.winrate /{2}) completed", mainTableAlias, calcFieldName, measureRate);
                    salestageSQL = string.Format(" left outer join crm_sys_salesstage_setting salestage on salestage.salesstageid={0}.recstageid ", mainTableAlias);
                }
                    
            }
            else
            {
                if (calcFieldName.ToLower() == "recid")
                {
                    sumSql = string.Format("count({0}.{1}) completed", mainTableAlias, calcFieldName);
                    measureUnit = "个";
                }
                else {
                    sumSql = string.Format("sum({0}.{1} /{2}) completed", mainTableAlias, calcFieldName,measureRate);
                }
            }
            string totalSql = "";
            List<Dictionary<string, object>> retList = null;
            if (p_rangetype == 1)
            {
                //按团队
                string subDeptSQL = string.Format("select deptid  from crm_func_department_tree('{0}',1) ", p_range);
                personSql = "1=1";
                totalSql = string.Format(@"select {8} userid, extract(year from {11}.{0} ) as year  , extract(month from {12}.{1} ) as  month ,{2} 
                                        from {3} as {4} {10} where  1=1 and {5} and {6} and {7}  
                                        group by {9},EXTRACT(year FROM  {11}.{0}),EXTRACT(month FROM {11}.{0})  ", dateTimeFieldName, dateTimeFieldName, sumSql, mainTable, mainTableAlias, ruleSQL, dateTimeSQL, personSql, userIdFieldName, userIdFieldName, salestageSQL,mainTableAlias,mainTableAlias);
                totalSql = "Select userid,year,month,sum(completed) completed from (" + totalSql + ")ddd  group by userid,year,month";

                string userDeptSQL = generateDeptAndUserFromTarget(targetid, p_year, p_from, p_to);
                string completedSQL = string.Format(@"Select userMonthSQL.year,userMonthSQL.month ,sum(completed) completed 
                            from ({0}) userMonthSQL left outer join ({1}) ok on userMonthSQL.year = ok.year and  userMonthSQL.month = ok.month and userMonthSQL.userid =ok.userid
                            where userMonthSQL.departmentid in ({2}) group by  userMonthSQL.year,userMonthSQL.month ", userDeptSQL, totalSql, subDeptSQL);
                string targetSQL = generateTargetSQLForDept(targetid, p_range, p_year, p_from, p_to);
                string realSQL = string.Format(@"Select COALESCE(targetsql.year,okouter.year) as year ,
                                                        COALESCE(targetsql.month,okouter.month) as month ,COALESCE(targetsql.target,0)  as  targetamount,
                                                        okouter.completed completedamount from ({0} ) as targetsql full  outer join ({1}) as okouter on targetsql.year = okouter.year and targetsql.month = okouter.month", targetSQL, completedSQL);
                retList = _reportEngineRepository.ExecuteSQL(realSQL, sqlParams);
                
            }
            else {
                //按个人
                totalSql = string.Format(@"select extract(year from {9}.{0} ) as year  , extract(month from {10}.{1} ) as  month ,{2} 
                                            from {3} as {4} {8} 
                                            where  1=1 and {5} and {6} and {7}  
                                            group by EXTRACT(year FROM {9}.{0}),EXTRACT(month FROM {9}.{0})  ", dateTimeFieldName, dateTimeFieldName, sumSql, mainTable, mainTableAlias, ruleSQL, dateTimeSQL, personSql, salestageSQL, mainTableAlias, mainTableAlias);
                totalSql = "Select year,month,sum(completed) completed from (" + totalSql + ")ddd  group by year,month";
                string targetSQL = generateTargetSQLForUser(targetid,p_range,p_year, p_from, p_to);
                string realSQL = string.Format(@"Select COALESCE(targetsql.year,okouter.year) as year ,
                                                        COALESCE(targetsql.month,okouter.month) as month ,COALESCE(targetsql.target,0)  as  targetamount,okouter.completed  completedamount 
                                    from ({0} ) as targetsql full outer join ({1}) as okouter on targetsql.year = okouter.year and targetsql.month = okouter.month", targetSQL, totalSql);

                //执行脚本即可
                retList = _reportEngineRepository.ExecuteSQL(realSQL, sqlParams);
            }

            #region 后期整理
            foreach (Dictionary<string, object> item in retList) {
                Decimal targetAmount = new Decimal(0);
                Decimal completedAmount = new Decimal(0);
                Decimal completedAmountnot = new Decimal(0);
                Decimal completedAmountyet = new decimal(0);
                Decimal completedRate = new Decimal(0);
                if (item.ContainsKey("targetamount") == false || item["targetamount"] == null) {
                    item["targetamount"] = new Decimal(0);
                }
                if (item.ContainsKey("targetamount") && item["targetamount"] != null) {
                    Decimal.TryParse(item["targetamount"].ToString(), out targetAmount);
                }
                if (item.ContainsKey("completedamount") == false || item["completedamount"] == null)
                {
                    item["completedamount"] = new Decimal(0);
                }
                if (item.ContainsKey("completedamount") && item["completedamount"] != null)
                {
                    Decimal.TryParse(item["completedamount"].ToString(), out completedAmount);
                }
                if (targetAmount.CompareTo(new Decimal(0.0)) == 0)
                {
                    completedRate = new Decimal(100);
                }
                else {
                    completedRate = completedAmount * 100/10000 * int.Parse(measureRate) / targetAmount;
                }
                if (targetAmount > completedAmount)
                {
                    completedAmountnot = completedAmount;
                    completedAmountyet = new Decimal(0);
                }
                else {
                    completedAmountnot = new Decimal(0); 
                    completedAmountyet = completedAmount;
                }
                item.Add("completedrate", completedRate);
                item.Add("completedAmountnot", completedAmountnot);
                item.Add("completedAmountyet", completedAmountyet);
                item.Add("measureunit", measureUnit);
            }
            #endregion 
            ret.Add("data", retList);
            return ret;
        }

        /// <summary>
        /// 获取计算部门目标值的（每月）的脚本，主要是把目标表中的横表改为纵表
        /// 内部使用
        /// </summary>
        /// <param name="typeid"></param>
        /// <param name="deptid"></param>
        /// <param name="year"></param>
        /// <param name="from_month"></param>
        /// <param name="to_month"></param>
        /// <returns></returns>
        private string generateTargetSQLForDept(string typeid, string deptid, int year, int from_month, int to_month)
        {
            string[] monthFields = new string[] { "", "jancount", "febcount", "marcount", "aprcount", "maycount", "juncount", "julcount", "augcount", "sepcount", "octcount", "novcount", "deccount" };

            string retSQL = string.Format(@"select year ,{0} as month ,sum(" + monthFields[from_month] + @")  target  from crm_sys_sales_target 
                                        where recstatus = 1  
                                         and year = " + year.ToString() + @"
                                        and isgrouptarget = 't'
                                        and normtypeid = '{1}'::uuid
                                        and departmentid='{2}'
                                        group by year 
                                        ", from_month, typeid, deptid);
            for (int i = from_month + 1; i <= to_month; i++)
            {   
                retSQL = string.Format(@"{0} 
                                            union all 
                                            select year ,{1} as month ,sum(" + monthFields[i] + @")  target from crm_sys_sales_target 
                                        where recstatus = 1  
                                             and year = " + year.ToString() + @"
                                            and isgrouptarget = 't'
                                            and normtypeid = '{2}'::uuid
                                            and departmentid='{3}'
                                        group by year 
                                         ", retSQL, i, typeid, deptid);
            }
            return retSQL;
        }
        private string generateTargetYearCountSQLForDept(string typeid, string deptid, int year)
        {
            string sql = string.Format(@"select sum(yearcount) target 
                           from crm_sys_sales_target
                                where 1=1
                                and recstatus = 1
                                and year ={0} 
                                and isgrouptarget = 't'
                                and normtypeid = '{1}'::uuid
                                and departmentid='{2}'
                                ", year, typeid, deptid);
            return sql;
        }
        /// <summary>
        /// 获取指定范围内的销售目标，且按部门分组
        /// </summary>
        /// <param name="typeid"></param>
        /// <param name="deptids"></param>
        /// <param name="year"></param>
        /// <param name="from_month"></param>
        /// <param name="to_month"></param>
        /// <returns></returns>
        private string generateTargetSQLGroupByDepts(string typeid, string deptids, int year, int from_month, int to_month) {
            string[] monthFields = new string[] { "", "jancount", "febcount", "marcount", "aprcount", "maycount", "juncount", "julcount", "augcount", "sepcount", "octcount", "novcount", "deccount" };
            string[] tmp = deptids.Split(',');
            string deptranges = "'"+tmp[0] + "'";
            for (int j = 1; j < tmp.Length; j++) {
                deptranges = deptranges + ",'" + tmp[j] + "'";


            }
            string retSQL = string.Format(@"select departmentid,year ,{0} as month ,sum(" + monthFields[from_month] + @")  target  from crm_sys_sales_target 
                                        where recstatus = 1  
                                         and year = " + year.ToString() + @"
                                        and isgrouptarget = 't'
                                        and normtypeid = '{1}'::uuid
                                        and departmentid in({2})
                                        group by departmentid,year  
                                        ", from_month, typeid, deptranges);
            for (int i = from_month + 1; i <= to_month; i++)
            {
                retSQL = string.Format(@"{0} 
                                            union all 
                                            select departmentid,year ,{1} as month ,sum(" + monthFields[i] + @")  target from crm_sys_sales_target 
                                        where recstatus = 1  
                                             and year = " + year.ToString() + @"
                                            and isgrouptarget = 't'
                                            and normtypeid = '{2}'::uuid
                                            and departmentid in({3})
                                        group by departmentid,year 
                                         ", retSQL, i, typeid, deptranges);
            }
            retSQL = string.Format(@"Select totalTarget.departmentid,sum(totalTarget.target) target from ({0}) totalTarget group by totalTarget.departmentid ", retSQL);
            return retSQL;
        }
        /// <summary>
        /// 获取查询 用户+部门+月份与目标的关系的脚本
        /// 对于同一个目标，同一个人在不同月份可能归属不同的部门，计算部门完成度的时候要考虑这个事情。
        /// </summary>
        /// <param name="typeid"></param>
        /// <param name="year"></param>
        /// <param name="from_month"></param>
        /// <param name="to_month"></param>
        /// <returns></returns>
        private string generateDeptAndUserFromTarget(string typeid, int year , int from_month, int to_month) {
            string itemSQL = string.Format(@"select year ,{0} as month ,departmentid,userid from crm_sys_sales_target 
                                        where recstatus = 1  
                                         and year = " + year.ToString() + @"
                                        and isgrouptarget = 'f'
                                        and normtypeid = '{1}'::uuid
                                        and beginmonth <={2} and endmonth >={3}
                                        ", from_month, typeid, from_month, from_month);
            itemSQL = string.Format(@"Select {2} as year,{1} as month,u.userid, COALESCE(target.departmentid,rel.deptid ) as departmentid 
                                       from crm_sys_userinfo u  
                                            left outer join ({0}) target on u.userid = target.userid 
		                                    left outer join (select * from crm_sys_account_userinfo_relate where recstatus = 1 ) rel on rel.userid = u.userid ", itemSQL, from_month, year); 
            string userDeptSQL = "("+itemSQL+")";
            for (int i = from_month + 1; i <= to_month; i++) {
                itemSQL = string.Format(@"select year ,{0} as month ,departmentid,userid from crm_sys_sales_target 
                                        where recstatus = 1  
                                         and year = " + year.ToString() + @"
                                        and isgrouptarget = 'f'
                                        and normtypeid = '{1}'::uuid
                                        and beginmonth <={0} and endmonth >={0}
                                        ", i, typeid);
                itemSQL = string.Format(@"Select {2} as year, {1} as month,u.userid, COALESCE(target.departmentid,rel.deptid ) as departmentid 
                                       from crm_sys_userinfo u  
                                            left outer join ({0}) target on u.userid = target.userid 
		                                    left outer join (select * from crm_sys_account_userinfo_relate where recstatus = 1 ) rel on rel.userid = u.userid ", itemSQL, i,year);
                userDeptSQL = string.Format(@"{0}
                                            union all 
                                            ({1})", userDeptSQL, itemSQL);

            }
            return userDeptSQL;

        }



        /// <summary>
        /// 拷贝字典
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private Dictionary<string, object> copyParam(Dictionary<string, object> p) {
            Dictionary<string, object> ret = new Dictionary<string, object>();
            foreach (string key in p.Keys) {
                if (key.StartsWith("@")) {
                    ret.Add(key.Substring(1), p[key]);
                }
                else
                {

                    ret.Add(key, p[key]);
                }
            }
            return ret;
        }
        /// <summary>
        /// 获取多用户与目标的关系的脚本
        /// </summary>
        /// <param name="typeid"></param>
        /// <param name="year"></param>
        /// <param name="from_month"></param>
        /// <param name="to_month"></param>
        /// <returns></returns>
        private string generateTargetSQLForUser(string typeid,string userids, int year, int from_month, int to_month) {
            string[] monthFields = new string[] { "", "jancount", "febcount", "marcount", "aprcount", "maycount", "juncount", "julcount", "augcount", "sepcount", "octcount", "novcount", "deccount" };
            string totalSQL = @"select  year, " +from_month.ToString()+@" as month  ,sum("+ monthFields[from_month]+ @")  target
                                from crm_sys_sales_target
                                where normtypeid = '"+ typeid  +  @"'::uuid
                                and recstatus = 1
                                and year = "+year.ToString()+ @"
                                and userid in  ("+userids + @")
                                and beginmonth   <= " + from_month.ToString()+ @"
                                and endmonth >= " + from_month.ToString()+@"
                                group by year  
                                ";
            for (int i = from_month + 1; i <= to_month; i++) {
                totalSQL = totalSQL+" union all  " + @"
                                select  year, " + i.ToString() + @" as month  ,sum(" + monthFields[i] + @")  target
                                from crm_sys_sales_target
                                where normtypeid = '" + typeid + @"'::uuid
                                and recstatus = 1
                                and year = " + year.ToString() + @"
                                and userid in  (" + userids + @")
                                and beginmonth  <= " + i.ToString() + @"
                                and endmonth >= " + i.ToString() + @"
                                group by year  
                                ";
            }
            return totalSQL;
        }
        private string generateTargetYearCountSQLForUser(string typeid, string userids,int year) {
            string sql = @"select sum(yearcount) target 
                           from crm_sys_sales_target
                                where normtypeid = '" + typeid + @"'::uuid
                                and recstatus = 1
                                and year = " + year.ToString() + @"
                                and userid in  (" + userids + @")
                                ";
            return sql;
        }

        private string generateTargetSQLGroupByUser(string typeid, string userids ,int year, int from_month, int to_month) {
            string[] monthFields = new string[] { "", "jancount", "febcount", "marcount", "aprcount", "maycount", "juncount", "julcount", "augcount", "sepcount", "octcount", "novcount", "deccount" };
            string totalSQL = @"select  userid,sum(" + monthFields[from_month] + @")  target
                                from crm_sys_sales_target
                                where normtypeid = '" + typeid + @"'::uuid
                                and recstatus = 1
                                and year = " + year.ToString() + @"
                                and userid in  (" + userids + @")
                                and beginmonth <= " + from_month.ToString() + @"
                                and endmonth >= " + from_month.ToString() + @"
                                group by userid  
                                ";
            for (int i = from_month + 1; i <= to_month; i++)
            {
                totalSQL = totalSQL + " union all  " + @"
                                select userid  ,sum(" + monthFields[i] + @")  target
                                from crm_sys_sales_target
                                where normtypeid = '" + typeid + @"'::uuid
                                and recstatus = 1
                                and year = " + year.ToString() + @"
                                and userid in  (" + userids + @")
                                and beginmonth <=" + i.ToString() + @"
                                and endmonth  >=" + i.ToString() + @"
                                group by userid  
                                ";
            }
            totalSQL = string.Format(@"select targettmp.userid,sum(targettmp.target) target from ({0}) targettmp group by targettmp.userid", totalSQL);
            return totalSQL;
        }


        /// <summary>
        /// 判断一个字段是否需要改为显示名字，而不是显示字段本身的内容（主要是数据源，枚举等数据）
        /// </summary>
        /// <param name="ctrlType"></param>
        /// <returns></returns>
        private bool isNeedDisplayName(int ctrlType) {
            switch ((EntityFieldControlType)ctrlType) {
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
        /// <summary>
        /// 根据条件，获取条件范围内的单据数据（可能是不同类型的单据，但是同一次请求只有一种单据类型、实体类型）
        /// 除了返回单据列表外，还会返回列表的列定义。
        /// 列表的列定义主要在实体配置中的WEB列定义上。
        /// </summary>
        /// <param name="param"></param>
        /// <param name="sortby"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageCount"></param>
        /// <param name="userNum"></param>
        /// <returns></returns>
        public Dictionary<string, List<Dictionary<string, object>>> getTargetBillList(
                            Dictionary<string, object> param,
                            Dictionary<string, string> sortby,
                            int pageIndex, int pageCount,
                            int userNum)
        {
            string personsql = "";
            string datetimeSQL = "";
            string userIdFieldName = "recmanager";
            string dateTimeFieldName = "reccreated";
            param = copyParam(param);
            string targetid = param["targetid"].ToString();
            string mainTableAlias = "e";
            List<SalesTargetNormRuleMapper> targetInfo = getTargetInfo(targetid, userNum);
            SalesTargetNormRuleMapper firstInfo = targetInfo[0];
            if (firstInfo.BizDateFieldName != null && firstInfo.BizDateFieldName.Length > 0) {
                dateTimeFieldName = firstInfo.BizDateFieldName;
            }
            #region 获取WEB列表显示需要的列信息
            List < DynamicEntityWebFieldMapper > columnsInfo = _dynamicEntityRepository.GetWebFields(Guid.Parse(firstInfo.EntityId), (int)2, userNum);
            TableComponentInfo tableComponentInfo = new TableComponentInfo();
            tableComponentInfo.FixedX = 0;
            tableComponentInfo.FixedY = 1;
            
            tableComponentInfo.Columns = new List<TableColumnInfo>();
            foreach (DynamicEntityWebFieldMapper columnInfo in columnsInfo) {
                TableColumnInfo retColumnInfo = new TableColumnInfo();
                retColumnInfo.FieldName = columnInfo.FieldName;
                retColumnInfo.CanPaged = true;
                retColumnInfo.CanSorted = 1;
                retColumnInfo.TargetType = 1;
                retColumnInfo.Title = columnInfo.DisplayName;
                retColumnInfo.ControlType = columnInfo.ControlType;
                if (columnInfo.ControlType == 8 || columnInfo.ControlType == 9) {
                    try
                    {
                        Dictionary<string,object> tmpDict= Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(columnInfo.FieldConfig);
                        if (tmpDict.ContainsKey("format")) {
                            retColumnInfo.FormatStr = tmpDict["format"].ToString();
                        }
                         
                            } catch (Exception ex) {
                    }
                }
                if ((EntityFieldControlType)columnInfo.ControlType == EntityFieldControlType.DataSourceSingle){
                    if (columnInfo.FieldConfig != null && columnInfo.FieldConfig.Length > 0) {
                        try
                        {
                            Dictionary<string, object> tmp = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(columnInfo.FieldConfig);
                            if (tmp != null && tmp.ContainsKey("dataSource")) {
                                tmp = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(Newtonsoft.Json.JsonConvert.SerializeObject(tmp["dataSource"]));
                                string datasourceid = tmp["sourceId"].ToString();
                                string entityid = this._reportEngineRepository.getEntityIdByDataSourceId(datasourceid, userNum);
                                if (entityid == null ) continue;
                                retColumnInfo.LinkScheme = string.Format("/entcomm/{0}/#{1}#", entityid, columnInfo.FieldName.ToLower());
                                retColumnInfo.TargetType = 2;
                            }
                            if (tmp != null && tmp.ContainsKey("multiple")) {
                                int isMulti = 0;
                                int.TryParse(tmp["multiple"].ToString(), out isMulti);
                                retColumnInfo.IsDataSourceMulti = (isMulti == 1);
                            }
                        }
                        catch (Exception ex) {
                        }
                    }
                }
                if (columnInfo.FieldName == "recname" &&( retColumnInfo.LinkScheme == null || retColumnInfo.LinkScheme.Length ==0 )) {
                    retColumnInfo.LinkScheme = string.Format("/entcomm/{0}/#recid#", firstInfo.EntityId);
                    retColumnInfo.TargetType = 2;
                }
                if (isNeedDisplayName(columnInfo.ControlType)) {
                    retColumnInfo.FieldName = retColumnInfo.FieldName + "_name";
                }
                tableComponentInfo.Columns.Add(retColumnInfo);
            }
            #endregion

            #region 获取Mobile列表显示
            MobileTableDefineInfo mobileTable = null;
            if (firstInfo.EntityId == "2c63b681-1de9-41b7-9f98-4cf26fd37ef1")
            {
                //商机的特殊处理
                mobileTable = new MobileTableDefineInfo();
                mobileTable.MainTitleFieldName = "recname";
                mobileTable.SubTitleFieldName = "";
                mobileTable.EntityId = firstInfo.EntityId;
                mobileTable.DetailColumns = new List<MobileTableFieldDefineInfo>();
                mobileTable.DetailColumns.Add(new MobileTableFieldDefineInfo() { Title = "负责人", FieldName = "recmanager_name" });
                mobileTable.DetailColumns.Add(new MobileTableFieldDefineInfo() { Title = "客户名称", FieldName = "belongcustomer_name" });
                mobileTable.DetailColumns.Add(new MobileTableFieldDefineInfo() { Title = "金额", FieldName = "premoney" });

            }
            else if (firstInfo.EntityId == "239a7c69-8238-413d-b1d9-a0d51651abfa")
            {
                //合同的特殊处理
                mobileTable = new MobileTableDefineInfo();
                mobileTable.MainTitleFieldName = "recname";
                mobileTable.SubTitleFieldName = "";
                mobileTable.EntityId = firstInfo.EntityId;
                mobileTable.DetailColumns = new List<MobileTableFieldDefineInfo>();
                mobileTable.DetailColumns.Add(new MobileTableFieldDefineInfo() { Title = "合同负责人", FieldName = "recmanager_name" });
                mobileTable.DetailColumns.Add(new MobileTableFieldDefineInfo() { Title = "客户名称", FieldName = "customerid_name" });
                mobileTable.DetailColumns.Add(new MobileTableFieldDefineInfo() { Title = "合同金额", FieldName = "contractvolume" });

            }
            else {
                //其他通用实体
                Dictionary < string, List < IDictionary < string, object>>>  tmp = this._entityProRepository.FieldMOBVisibleQuery(firstInfo.EntityId, userNum);
                if (tmp != null && tmp.ContainsKey("fieldvisible")) {
                    mobileTable = new MobileTableDefineInfo();
                    mobileTable.MainTitleFieldName = "recname";
                    mobileTable.SubTitleFieldName = "";
                    mobileTable.EntityId = firstInfo.EntityId;
                    List<IDictionary<string, object>> list = tmp["fieldvisible"];
                    foreach (IDictionary<string, object> item in list) {
                        if (!item.ContainsKey("fieldname")) continue;
                        string fieldname = item["fieldname"].ToString().ToLower();
                        if (fieldname == "rec_name") continue;
                        string displayname = "";
                        int controltype = 0;
                        if (item.ContainsKey("controltype")) {
                            int.TryParse(item["controltype"].ToString(), out controltype);
                        }
                        if (isNeedDisplayName(controltype)) {
                            fieldname = fieldname + "_name";
                        }
                        if (item.ContainsKey("displayname")) {
                            displayname = item["displayname"].ToString();
                        }
                        mobileTable.DetailColumns.Add(new MobileTableFieldDefineInfo() { Title = displayname, FieldName = fieldname });
                    }
                }
            }
            #endregion
            //完成了项目
            string ruleSQL = "1=1";
            int p_year = int.Parse(param["year"].ToString());
            if (p_year < 2000 || p_year > 2500)
            {
                throw (new Exception("年份不正确"));
            }
            int p_from = ReportParamsUtils.parseInt(param, "month_from");
            if (p_from < 0 || p_from > 12)
            {
                throw (new Exception("月份不正确"));
            }
            int p_to = ReportParamsUtils.parseInt(param, "month_to"); 
            if (p_to < 0 || p_to > 12)
            {
                throw (new Exception("月份不正确"));
            }
            if (p_from > p_to)
            {
                throw (new Exception("月份不正确"));
            }
            if (p_from == 0 && p_to ==0 ) {
                p_from = 1;
                p_to = 12 ;
            }
            if (p_from <= 0) {
                p_from = System.DateTime.Now.Month;
                p_to = System.DateTime.Now.Month;
            }
            int p_rangetype = int.Parse(param["range_type"].ToString());
            if (p_rangetype != 1 && p_rangetype != 2)
            {
                throw (new Exception("查询范围类型不正确，请选择团队或者个人"));
            }
            string p_range = param["range"].ToString();
            p_range = p_range.Replace("'", "''");
            if (p_range.Length == 0)
            {
                throw (new Exception("查询范围异常"));
            }
            #region 处理数据权限
            string targetRuleSQL = "";
            string userRoleRuleSQL = "";
            Guid userDeptInfo = Guid.Empty;
            //先处理目标的数据全系
            UserData userData = GetUserData(userNum, false);
            if (userData != null && userData.AccountUserInfo != null && userData.AccountUserInfo.DepartmentId != null)
            {
                userDeptInfo = userData.AccountUserInfo.DepartmentId;
            }
            if (firstInfo.RuleId != null && firstInfo.RuleId.Length > 0 && firstInfo.RuleId != Guid.Empty.ToString())
            {
                targetRuleSQL = this._reportEngineRepository.getRuleSQLByRuleId(firstInfo.EntityId, firstInfo.RuleId, userNum, null);
            }
            //处理角色数据权限 
            userRoleRuleSQL = this._reportEngineRepository.getRuleSQLByUserId(firstInfo.EntityId, userNum,null);
            ruleSQL = " 1=1 ";
            if (targetRuleSQL != null && targetRuleSQL.Length > 0)
            {
                targetRuleSQL = RuleSqlHelper.FormatRuleSql(targetRuleSQL, userNum, userDeptInfo);
                ruleSQL = ruleSQL + "and e.recid in (" + targetRuleSQL + ")";

            }
            if (userRoleRuleSQL != null && userRoleRuleSQL.Length > 0)
            {
                userRoleRuleSQL = RuleSqlHelper.FormatRuleSql(userRoleRuleSQL, userNum, userDeptInfo);
                ruleSQL = ruleSQL + " And e.recid in (" + userRoleRuleSQL + ")";
            }

            #endregion
            DateTime dt_from = new DateTime(p_year, p_from, 1, 0, 0, 0);
            DateTime dt_to = new DateTime(p_year, p_to, 1, 23, 59, 59);
            dt_to = dt_to.AddMonths(1);
            dt_to = dt_to.AddDays(-1);

            datetimeSQL = string.Format(" ( {0}.{1} >='{2}' and {3}.{4} <='{5}' )", mainTableAlias, dateTimeFieldName, dt_from.ToString("yyyy-MM-dd HH:mm:ss"),mainTableAlias, dateTimeFieldName, dt_to.ToString("yyyy-MM-dd HH:mm:ss"));
            string totalWhereSQL = "";
            if (p_rangetype == 1)
            {
                string subDeptSQL = generateAllSubDeptSQL(p_range);
                string userDeptSQL = generateDeptAndUserFromTarget(targetid, p_year, p_from, p_to);
                string subTotal = string.Format(" select * from ({0})  aaaa where aaaa.departmentid in  ({1})", userDeptSQL, subDeptSQL);
                string subWhere = string.Format(@" exists(select 1 from ({0}) allSubDeptTable 
                                                    where  extract(year from {1}.{2}) = allSubDeptTable.year
                                                    and  extract(month from {1}.{2}) = allSubDeptTable.month
                                                    and {1}.{3} = allSubDeptTable.userid)", subTotal, mainTableAlias, dateTimeFieldName, userIdFieldName);
                totalWhereSQL = string.Format(" {0} And {1} and {2}", ruleSQL, datetimeSQL, subWhere);
                totalWhereSQL = totalWhereSQL.Replace("'", "''");

            }
            else {
                string[] tmp = p_range.Split(',');
                personsql = " 1 <> 1 ";
                foreach (string item in tmp)
                {
                    personsql = personsql + string.Format(" or {0}.{1} = {2}", mainTableAlias, "recmanager", item);
                }
                personsql = "(" + personsql + ")";
                totalWhereSQL = string.Format(" {0} And {1} and {2}", ruleSQL, datetimeSQL, personsql);
                totalWhereSQL = totalWhereSQL.Replace("'", "''");
            }
            Dictionary<string, List<Dictionary<string, object>>> retData = (Dictionary<string, List<Dictionary<string, object>>>)_targetAndCompletedReportRepository.DataList(firstInfo.EntityId, totalWhereSQL, " recid ", 1, 10000, userNum);
            
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
        private string generateAllSubDeptSQL(string p_range) {
            string retSQL = "";
            string[] tmp = p_range.Split(',');
            retSQL = string.Format("select deptid  from crm_func_department_tree('{0}',1) ", tmp[0]);
            for (int i = 1; i < tmp.Length; i++) {
                retSQL = string.Format("{0} union select deptid  from crm_func_department_tree('{1}',1) ", retSQL,tmp[i]);
            }

            return retSQL;
        }
        /// <summary>
        /// 获取目标的详情，主要用于获取目标对应的单据实体，以便获取单据信息
        /// </summary>
        /// <param name="targetid"></param>
        /// <param name="userNum"></param>
        /// <returns></returns>
        private List<SalesTargetNormRuleMapper>  getTargetInfo(string targetid,int userNum) {
            var crmData = new SalesTargetNormTypeDetailMapper()
            {
                Id = Guid.Parse(targetid),
            };

            List<SalesTargetNormRuleMapper> infoList = _salesTargetRepository.GetSalesTargetNormDetail(crmData, userNum);

            string _entityId = null;
            if (infoList != null && infoList.Count > 0)
			{
                _entityId = infoList.FirstOrDefault().EntityId;
            }

            var obj = infoList.GroupBy(t => new
            {
                t.RuleId,
                t.RuleName,
                t.RuleSet
            }).Select(group => new RoleRuleInfoModel
            {
                EntityId = _entityId,
                FieldName = infoList.FirstOrDefault().FieldName,
                CaculateType = infoList.FirstOrDefault().CaculateType,
                RuleId = group.Key.RuleId,
                RuleName = group.Key.RuleName,
                RuleItems = group.Select(t => new RuleItemInfoModel
                {
                    ItemId = t.ItemId,
                    ItemName = t.ItemName,
                    FieldId = t.FieldId,
                    Operate = t.Operate,
                    UseType = t.UseType,
                    RuleData = t.RuleData,
                    RuleType = t.RuleType
                }).ToList(),
                RuleSet = new RuleSetInfoModel
                {
                    RuleSet = group.Key.RuleSet
                }
            }).ToList();
            return infoList;
        }


        /// <summary>
        /// 根据条件计算目标与完成值，按计算范围（个人或者团体）汇总
        /// </summary>
        /// <param name="param"></param>
        /// <param name="sortby"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageCount"></param>
        /// <param name="userNum"></param>
        /// <returns></returns>
        public Dictionary<string, List<Dictionary<string, object>>> getPKSumList(
                            Dictionary<string, object> param,
                            Dictionary<string, string> sortby,
                            int pageIndex, int pageCount,
                            int userNum)
        {
            Dictionary<string, List<Dictionary<string, object>>> ret = new Dictionary<string, List<Dictionary<string, object>>>();
            #region 定义变量
            string p_targetid = "";
            string p_range = "";
            int p_rangetype = 0;
            int p_year = 0;
            int p_monthfrom = 0;
            int p_monthto = 0;
            string mainTable = "";
            string mainTableAlias = "e";
            string ruleSQL = "1=1";
            string dateTimeSQL = "";
            string userIdFieldName = "recmanager";
            string dateTimeFieldName = "reccreated";
            string salestageSQL = "";
            #endregion
            #region 获取变量
            p_targetid = ReportParamsUtils.parseString(param, "targetid");
            p_rangetype = ReportParamsUtils.parseInt(param, "range_type");
            p_range = ReportParamsUtils.parseString(param, "range");
            p_year = ReportParamsUtils.parseInt(param, "year");
            p_monthfrom = ReportParamsUtils.parseInt(param, "month_from");
            p_monthto = ReportParamsUtils.parseInt(param, "month_to");
            if (p_targetid == null || p_targetid.Length == 0) throw (new Exception("参数异常"));
            if (p_rangetype != 1 && p_rangetype != 2) { throw (new Exception("参数异常")); }
            if (p_range == null ||p_range.Length == 0  ) throw (new Exception("参数异常"));
            if(p_year <2015 || p_year > 2050) throw (new Exception("参数异常"));
            if (p_monthfrom < 1 || p_monthfrom > 12) throw (new Exception("参数异常"));
            if (p_monthto < 1 || p_monthto > 12) throw (new Exception("参数异常"));
            if (p_monthfrom > p_monthto) throw (new Exception("参数异常"));
            #endregion
            string personSql = "";
            string sumSql = "";
            string calcFieldName = "";
            DateTime dt_from = new DateTime(p_year, p_monthfrom, 1, 0, 0, 0);
            DateTime dt_to = new DateTime(p_year, p_monthto, 1, 23, 59, 59);
            dt_to = dt_to.AddMonths(1);
            dt_to = dt_to.AddDays(-1);
            var sqlParams = new DbParameter[] {
                new Npgsql.NpgsqlParameter("dtfrom",dt_from),
                new Npgsql.NpgsqlParameter("dtto",dt_to)
            };
            List<SalesTargetNormRuleMapper> targetInfo = getTargetInfo(p_targetid, userNum);
            SalesTargetNormRuleMapper firstInfo = targetInfo[0];
            if (firstInfo.BizDateFieldName != null && firstInfo.BizDateFieldName.Length > 0) {
                dateTimeFieldName = firstInfo.BizDateFieldName;
            }
            calcFieldName = firstInfo.FieldName;
            #region 处理数据权限
            string targetRuleSQL = "";
            string userRoleRuleSQL = "";
            Guid userDeptInfo = Guid.Empty;
            //先处理目标的数据全系
            UserData userData = GetUserData(userNum, false);
            if (userData != null && userData.AccountUserInfo != null && userData.AccountUserInfo.DepartmentId != null)
            {
                userDeptInfo = userData.AccountUserInfo.DepartmentId;
            }
            if (firstInfo.RuleId != null && firstInfo.RuleId.Length > 0 && firstInfo.RuleId != Guid.Empty.ToString())
            {
                targetRuleSQL = this._reportEngineRepository.getRuleSQLByRuleId(firstInfo.EntityId, firstInfo.RuleId, userNum, null);
            }
            //处理角色数据权限 
            userRoleRuleSQL = this._reportEngineRepository.getRuleSQLByUserId(firstInfo.EntityId, userNum, null);
            ruleSQL = " 1=1 ";
            if (targetRuleSQL != null && targetRuleSQL.Length > 0)
            {
                targetRuleSQL = RuleSqlHelper.FormatRuleSql(targetRuleSQL, userNum, userDeptInfo);
                ruleSQL = ruleSQL + "and e.recid in (" + targetRuleSQL + ")";

            }
            if (userRoleRuleSQL != null && userRoleRuleSQL.Length > 0)
            {
                userRoleRuleSQL = RuleSqlHelper.FormatRuleSql(userRoleRuleSQL, userNum, userDeptInfo);
                ruleSQL = ruleSQL + " And e.recid in (" + userRoleRuleSQL + ")";
            }

            #endregion
            //处理期间脚本
            //处理范围脚本（按团队还是按个人）
            dateTimeSQL = string.Format("({0}.{1} >=@dtfrom and {2}.{3} <=@dtto)", mainTableAlias, dateTimeFieldName, mainTableAlias, dateTimeFieldName);
            if (p_rangetype == 1)
            {
               //这里也是可以多选的
            }
            else
            {
                //按个人，可多选（并入主实体即可）
                string[] tmp = p_range.Split(',');
                personSql = " 1 <> 1 ";
                foreach (string item in tmp)
                {
                    personSql = personSql + string.Format(" or {0}.{1} = {2}", mainTableAlias, "recmanager", item);
                }
                personSql = "(" + personSql + ")";
            }
            //获取主表
            Dictionary<string, object> entityInfo = _dynamicEntityRepository.getEntityBaseInfoById(Guid.Parse(firstInfo.EntityId), userNum);
            if (entityInfo == null) return null;
            mainTable = entityInfo["entitytable"].ToString();

            if (SaleOppoEntityID == firstInfo.EntityId)
            {
                //预测销售目标是有特殊算法的
                if (calcFieldName.ToLower() == "recid")
                {
                    sumSql = string.Format("count({0}.{1}) completed", mainTableAlias, calcFieldName);
                }
                else
                {
                    sumSql = string.Format("sum({0}.{1} * salestage.winrate /10000) completed", mainTableAlias, calcFieldName);
                    salestageSQL = string.Format(" left outer join crm_sys_salesstage_setting salestage on salestage.salesstageid={0}.recstageid ", mainTableAlias);
                }
            }
            else
            {
                if (calcFieldName.ToLower() == "recid")
                {
                    sumSql = string.Format("count({0}.{1}) completed", mainTableAlias, calcFieldName);
                }
                else
                {
                    sumSql = string.Format("sum({0}.{1} /10000) completed", mainTableAlias, calcFieldName);
                }
            }
            string totalSql = "";
            if (p_rangetype == 1)
            {
                //按团队
                string subDeptSQL = generateDeptAndSubDeptSQL(p_range);
                personSql = "1=1";
                totalSql = string.Format(@"select {8} userid,
                                    extract(year from {11}.{0} ) as year  , 
                                    extract(month from {12}.{1} ) as  month ,
                                    {2} 
                                    from {3} as {4} {10} 
                                    where  1=1 and {5} and {6} and {7}  
                                    group by {9},EXTRACT(year FROM {11}.{0} ),EXTRACT(month FROM {11}.{0} )  ",
                                    dateTimeFieldName, dateTimeFieldName, sumSql, mainTable, mainTableAlias,
                                    ruleSQL, dateTimeSQL, personSql, userIdFieldName, userIdFieldName,
                                    salestageSQL, mainTableAlias, mainTableAlias);
                totalSql = "Select userid,year,month,sum(completed) completed from (" + totalSql + ")ddd  group by userid,year,month";

                string userDeptSQL = generateDeptAndUserFromTarget(p_targetid, p_year, p_monthfrom, p_monthto);
                string completedSQL = string.Format(@"Select subdeptsql.maindeptid, sum(completed) completed 
                            from ({0}) userMonthSQL left outer join ({1}) ok on userMonthSQL.year = ok.year 
                                                and  userMonthSQL.month = ok.month and userMonthSQL.userid =ok.userid
                                inner  join ({2}) subdeptsql on subdeptsql.deptid = userMonthSQL.departmentid 
                             group by  subdeptsql.maindeptid ", userDeptSQL, totalSql, subDeptSQL);
                string targetSQL = generateTargetSQLGroupByDepts(p_targetid, p_range, p_year, p_monthfrom, p_monthto);
                string realSQL = string.Format("Select targetsql.departmentid,targetsql.target targetamount ," +
                    " okouter.completed completedamount " +
                    " from ({0} ) as targetsql left outer join ({1}) as okouter on targetsql.departmentid = okouter.maindeptid ", targetSQL, completedSQL);
                realSQL = string.Format(@"Select realSQL.departmentid rangeid,dept.deptname rangename ,realSQL.targetamount,realSQL.completedamount from ({0}) realSQL left outer join crm_sys_department dept on dept.deptid = realSQL.departmentid", realSQL);
                List<Dictionary<string, object>> retList = _reportEngineRepository.ExecuteSQL(realSQL, sqlParams);
                ret.Add("data", retList);
            }
            else
            {
                    //按个人
                    totalSql = string.Format(@"select {1}.{7} userid,{2} 
                                        from {0} as {1} {3} 
                                        where  1=1 and {4} and {5} and {6}  
                                        group by {1}.{7}",
                                        mainTable, mainTableAlias,sumSql, salestageSQL,
                                        ruleSQL, dateTimeSQL, personSql, userIdFieldName);
                    totalSql = "Select userid,sum(completed) completed from (" + totalSql + ")ddd  group by userid";
                    string targetSQL = generateTargetSQLGroupByUser(p_targetid,p_range, p_year, p_monthfrom, p_monthto);
                    string realSQL = string.Format(@"Select targetsql.userid rangeid , userinfo.username rangename ,targetsql.target  targetamount,okouter.completed  completedamount 
                                    from ({0} ) as targetsql left outer join ({1}) as okouter on targetsql.userid = okouter.userid
                                    left outer join crm_sys_userinfo userinfo on userinfo.userid = targetsql.userid", targetSQL, totalSql);

                    //执行脚本即可
                    List<Dictionary<string, object>> retList = _reportEngineRepository.ExecuteSQL(realSQL, sqlParams);
                    ret.Add("data", retList);
            }
            if (ret.ContainsKey("data") && ret["data"] != null)
            {
                List<Dictionary<string, object>> retList = ret["data"];
                Decimal maxRate = new Decimal(0.0);
                foreach (Dictionary<string, object> item in retList)
                {
                    Decimal targetAmount;
                    Decimal rate;
                    Decimal.TryParse(item["targetamount"].ToString(), out targetAmount);
                    Decimal completedAmount = new Decimal(0.00);
                    if (item["completedamount"]  != null)
                    {
                        Decimal.TryParse(item["completedamount"].ToString(), out completedAmount);
                    }
                    if (targetAmount != 0)
                    {
                        rate = completedAmount * 100 / targetAmount;
                    }
                    else
                    {
                        if (completedAmount > 0) { rate = new Decimal(100.00); } else { rate = new Decimal(0.00); }
                        
                    }
                    rate = Math.Round(rate, 2);
                    if (maxRate < rate)
                    {
                        maxRate = rate;
                    }
                    item.Add("completedrate", rate);
                }
                foreach (Dictionary<string, object> item in retList)
                {
                    Decimal rate = (Decimal)item["completedrate"];
                    Decimal displayRate;
                    if (maxRate == 0)
                    {
                        displayRate = 0;
                    }
                    else
                    {
                        displayRate = rate * 100 / maxRate;
                    }
                    displayRate = Math.Round(displayRate, 2);
                    item.Add("displayrate", displayRate);
                }

                
            }
            return ret;
        }
        public Dictionary<string, List<Dictionary<string, object>>> getPKSummary(Dictionary<string, object> param,
                            Dictionary<string, string> sortby,
                            int pageIndex, int pageCount,
                            int userNum) {
            Dictionary<string, List<Dictionary<string, object>>> retData = getPKSumList(param, sortby, pageIndex, pageCount, userNum);
            if (retData.ContainsKey("data")) {
                List<Dictionary<string, object>> datalist = retData["data"];
                List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
                Dictionary<string, object> item = new Dictionary<string, object>();
                Decimal targetAmount = new Decimal(0.00);
                Decimal completedAmount = new Decimal(0.00);
                foreach (Dictionary<string, object> tmp in datalist) {
                    Decimal inner = new Decimal(0.00);
                    if (tmp["targetamount"] != null) {
                        Decimal.TryParse(tmp["targetamount"].ToString(), out inner);
                    }
                    inner = new Decimal(0.00);
                    targetAmount = targetAmount + inner;
                    if (tmp["completedamount"] != null)
                    {
                        Decimal.TryParse(tmp["completedamount"].ToString(), out inner);
                    }
                    completedAmount = completedAmount + inner;
                }
                item.Add("completedamount", completedAmount);
                item.Add("targetamount", targetAmount);
                Decimal rate = new Decimal(0.0);
                if (targetAmount == 0)
                {
                    if (completedAmount > 0) rate = new Decimal(100.0);
                    else rate = new Decimal(0.0);
                }
                else {
                    rate = completedAmount * 100 / targetAmount;
                }
                rate = Math.Round(rate, 2);
                item.Add("completedrate", rate);
                int p_year = ReportParamsUtils.parseInt(param, "year");
                int p_monthfrom = ReportParamsUtils.parseInt(param, "month_from");
                int p_monthto = ReportParamsUtils.parseInt(param, "month_to");
                item.Add("datefrom", string.Format("{0}-{1}", p_year, p_monthfrom));
                item.Add("dateto", string.Format("{0}-{1}", p_year, p_monthto));
                data.Add(item);
                retData.Remove("data");
                retData.Add("data", data);
            }
            return retData;
        }
        private string generateDeptAndSubDeptSQL(string p_range) {
            string retSQL = "";
            string[] tmp = p_range.Split(',');
            if (tmp.Length == 0) {
                return "select '' maindeptid,'' deptid from crm_sys_department where 1 <>1  ";
            }
            retSQL = string.Format(@"select '{0}'::uuid as maindeptid,deptid from crm_func_department_tree('{0}',1) ", tmp[0]);
            for (int i = 1; i < tmp.Length; i++) {

                retSQL = string.Format(@"{0} union all select '{1}' as maindeptid,deptid from crm_func_department_tree('{1}',1)", retSQL, tmp[i]);
            }
            return retSQL;

        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="param"></param>
        /// <param name="sortby"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageCount"></param>
        /// <param name="userNum"></param>
        /// <returns></returns>
        public Dictionary<string, List<Dictionary<string, object>>> testData
            (
                            Dictionary<string, object> param,
                            Dictionary<string, string> sortby,
                            int pageIndex, int pageCount,
                            int userNum)
        {

            string sfrom = "1";
            string sto = "12";
            if (param.ContainsKey("month_from"))
            {
                sfrom = param["month_from"].ToString();
            }
            if (param.ContainsKey("month_to"))
            {
                sto = param["month_to"].ToString();
            }
            int from = int.Parse(sfrom);
            int to = int.Parse(sto);
            Random r = new Random((int)(System.DateTime.Now.Ticks % 1000000));
            List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
            for (int i = from; i <= to; i++)
            {
                Dictionary<string, object> item = new Dictionary<string, object>();
                int target = r.Next(1000);
                int completed = (int)((r.NextDouble() + 0.5) * target);
                item.Add("month", i);
                item.Add("targetamount", target);
                item.Add("completedamount", completed);
                data.Add(item);
            }
            Dictionary<string, List<Dictionary<string, object>>> ret = new Dictionary<string, List<Dictionary<string, object>>>();
            ret.Add("data", data);
            return ret;
        }
    }
}
