using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Utility;
using UBeat.Crm.CoreApi.Services.Models;

namespace UBeat.Crm.CoreApi.Services.Services.ReportDetail
{
    public class CustomersDistributionServices:EntityBaseServices
    {
        private IDynamicEntityRepository _dynamicEntityRepository;
        private IReportEngineRepository _reportEngineRepository;
		private readonly string customer_entityid = "f9db9d79-e94b-4678-a5cc-aa6e281c1246";
		public CustomersDistributionServices(IDynamicEntityRepository dynamicEntityRepository,
                IReportEngineRepository reportEngineRepository) {
            _dynamicEntityRepository = dynamicEntityRepository;
            _reportEngineRepository = reportEngineRepository;
        }
        /// <summary>
        /// 获取客户分布图
        /// </summary>
        /// <param name="param"></param>
        /// <param name="sortby"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageCount"></param>
        /// <param name="userNum"></param>
        /// <returns></returns>
        public Dictionary<string, List<Dictionary<string, object>>> getCustomerDistrubition(
                            Dictionary<string, object> param,
                            Dictionary<string, string> sortby,
                            int pageIndex, int pageCount,
                            int userNum)
        {
            Dictionary<string, string> ProvincesMap = getDiffMap();
            Dictionary<string, string> ProvincesRevertMap = getDiffRevertMap();
            #region 定义变量
            string p_regions = "";
            string p_custstatus = "";
            string p_custscale = "";
            string p_custlevel = "";
            string p_custindust = "";
			string RoleSQL = " ";
            string RegionSQL = "";
            string CustomerSearchSQL = "";
            string p_startdate = "";
            string p_enddate = "";
			string p_department = "";
			string PersonSQL = "";
			Dictionary<string, List<Dictionary<string, object>>> superdata = new Dictionary<string, List<Dictionary<string, object>>>();
            List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
			#endregion

			#region 角色权限
			Guid userDeptInfo = Guid.Empty;
			UserData userData = GetUserData(userNum, false);
			if (userData != null && userData.AccountUserInfo != null && userData.AccountUserInfo.DepartmentId != null)
			{
				userDeptInfo = userData.AccountUserInfo.DepartmentId;
			}

			string userRoleRuleSQL = this._reportEngineRepository.getRuleSQLByUserId(customer_entityid, userNum, null);
			if (userRoleRuleSQL == null || userRoleRuleSQL.Length == 0)
			{
				RoleSQL = "1=1";
			}
			else
			{
				userRoleRuleSQL = RuleSqlHelper.FormatRuleSql(userRoleRuleSQL, userNum, userDeptInfo);
				RoleSQL = RoleSQL + " e.recid in (" + userRoleRuleSQL + ")";
			}
			#endregion
			#region 处理参数
			if (param == null) throw (new Exception("参数异常"));
            if (param.ContainsKey("@regions"))
            {
                p_regions = param["@regions"].ToString();
            }
            else if (param.ContainsKey("regions"))
            {
                p_regions = param["regions"].ToString();
            }

            if (param.ContainsKey("@custstatus"))
            {
                p_custstatus = param["@custstatus"].ToString();
            }
            else if (param.ContainsKey("custstatus"))
            {
                p_custstatus = param["custstatus"].ToString();
            }

            if (param.ContainsKey("@custscale"))
            {
                p_custscale = param["@custscale"].ToString();
            }
            else if (param.ContainsKey("custscale"))
            {
                p_custscale = param["custscale"].ToString();
            }

            if (param.ContainsKey("@custlevel"))
            {
                p_custlevel = param["@custlevel"].ToString();
            }
            else if (param.ContainsKey("custlevel"))
            {
                p_custlevel = param["custlevel"].ToString();
            }

            if (param.ContainsKey("@custindust"))
            {
                p_custindust = param["@custindust"].ToString();
            }
            else if (param.ContainsKey("custindust"))
            {
                p_custindust = param["custindust"].ToString();
            }

            if (param.ContainsKey("@startdate"))
            {
                p_startdate = param["@startdate"].ToString();
            }
            else if (param.ContainsKey("startdate"))
            {
                p_startdate = param["startdate"].ToString();
            }

            if (param.ContainsKey("@enddate"))
            {
                p_enddate = param["@enddate"].ToString();
            }
            else if (param.ContainsKey("enddate"))
            {
                p_enddate = param["enddate"].ToString();
            }
			#endregion

			#region 处理部门
			p_department = ReportParamsUtils.parseString(param, "deptid");
			if (!string.IsNullOrEmpty(p_department))
			{
				string subDeptSQL = string.Format("select deptid  from crm_func_department_tree('{0}',1) ", p_department);

				string belongPerson = string.Format("select userid  from crm_sys_account_userinfo_relate where deptid in({0}) ", subDeptSQL);
				PersonSQL = string.Format("e.recmanager in ({0})", belongPerson);
			}
			else
			{
				PersonSQL = "1=1";
			}
			#endregion

			#region 处理地区
			int parentregionid = 1;
            int regiontype = 1;
            if (p_regions != null && p_regions.Length >0  ) {
                string[] tmp = p_regions.Split(',');
                for (int i = 0; i < tmp.Length; i++) {
                    string regionName = tmp[i];
                    regionName = regionName.Trim();
                    bool NeedPass = false;
                    if (i == 0) { 
                        if (ProvincesRevertMap.ContainsKey(regionName)) {
                            regionName = ProvincesRevertMap[regionName];
                        }
                        NeedPass = CheckIsDirectly(regionName);
                    }
                    int newRegionID = getRegionId(regionName, regiontype);
                    
                    regiontype = regiontype + 1;
                    if (NeedPass) {
                        newRegionID = getRegionId(regionName, regiontype);
                        regiontype = regiontype + 1;
                    }
                    parentregionid = newRegionID;
                }
            }

            RegionSQL = " e.continent in (select dataid from crm_sys_dictionary where recstatus = 1 and dictypeid = 103) ";
            #endregion
            #region  处理客户的查询脚本
            #region 处理客户状态过滤条件
            string StatusSQL = "1=1";
            if (p_custstatus != null && p_custstatus.Length > 0) {
                string[] tmp = p_custstatus.Split(',');
                bool hasZero = false;
                string inValue = "";
                foreach(string item in tmp)
                {
                    int id = 0;
                    if (int.TryParse(item, out id) == false) continue;
                    if (id == 0)
                    {
                        hasZero = true;
                    }
                    else {
                        inValue = inValue + "," + id;
                    }
                }
                if (inValue.Length > 0)
                {
                    inValue = inValue.Substring(1);
                    StatusSQL = string.Format(@" e.customerstatus in ({0})", inValue);
                }
                else {
                    StatusSQL = "1<>1";
                }
                if (hasZero) {
                    StatusSQL = string.Format(@" ({0} or e.customerstatus is null  or e.customerstatus = 0 )", StatusSQL);
                }
            }
            #endregion

            #region 处理客户规模过滤条件
            string ScaleSQL = "1=1";
            if (p_custscale != null && p_custscale.Length > 0)
            {
                string[] tmp = p_custscale.Split(',');
                bool hasZero = false;
                string inValue = "";
                foreach (string item in tmp)
                {
                    int id = 0;
                    if (int.TryParse(item, out id) == false) continue;
                    if (id == 0)
                    {
                        hasZero = true;
                    }
                    else
                    {
                        inValue = inValue + "," + id;
                    }
                }
                if (inValue.Length > 0)
                {
                    inValue = inValue.Substring(1);
                    ScaleSQL = string.Format(@" e.coldstorsize in ({0})", inValue);
                }else
                {
                    ScaleSQL = " 1<>1 ";
                }
                if (hasZero)
                {
                    ScaleSQL = string.Format(@" ({0} or e.coldstorsize is null  or e.coldstorsize = 0 )", ScaleSQL);
                }
            }
            #endregion

            #region 处理客户级别过滤条件
            string LevelSQL = "1=1";
            if (p_custlevel != null && p_custlevel.Length > 0)
            {
                string[] tmp = p_custlevel.Split(',');
                bool hasZero = false;
                string inValue = "";
                foreach (string item in tmp)
                {
                    int id = 0;
                    if (int.TryParse(item, out id) == false) continue;
                    if (id == 0)
                    {
                        hasZero = true;
                    }
                    else
                    {
                        inValue = inValue + "," + id;
                    }
                }
                if (inValue.Length > 0)
                {
                    inValue = inValue.Substring(1);
                    LevelSQL = string.Format(@" e.custlevel in ({0})", inValue);
                } else
                {
                    LevelSQL = " 1<>1 ";
                }
                if (hasZero)
                {
                    LevelSQL = string.Format(@" ({0} or e.custlevel is null  or e.custlevel = 0 )", LevelSQL);
                }
            }
            #endregion

            #region 处理客户行业过滤条件
            string InduSQL = "1=1";
            if (p_custindust != null && p_custindust.Length > 0)
            {
                string[] tmp = p_custindust.Split(',');
                bool hasZero = false;
                string inValue = "";
                foreach (string item in tmp)
                {
                    int id = 0;
                    if (int.TryParse(item, out id) == false) continue;
                    if (id == 0)
                    {
                        hasZero = true;
                    }
                    else
                    {
                        inValue = inValue + "," + id;
                    }
                }
                if (inValue.Length > 0)
                {
                    inValue = inValue.Substring(1);
                    InduSQL = string.Format(@" e.industry in ({0})", inValue);
                }
                else {
                    InduSQL = " 1<> 1";
                }
                if (hasZero)
                {
                    InduSQL = string.Format(@" ({0} or e.industry is null  or e.industry = 0 )", InduSQL);
                }
            }
			#endregion
            CustomerSearchSQL = string.Format(@" select * from crm_sys_customer e  where 1=1 and {0} and  {1}  and {2} and {3} And {4} And {5} and {6}", RoleSQL, RegionSQL,StatusSQL,LevelSQL,ScaleSQL, InduSQL, PersonSQL);

            #region 处理时间范围
            if (!string.IsNullOrEmpty(p_startdate))
            {
                CustomerSearchSQL += string.Format(@" and e.reccreated >= '{0}'", p_startdate);
            }
            if (!string.IsNullOrEmpty(p_enddate))
            {
                CustomerSearchSQL += string.Format(@" and e.reccreated <= '{0}'", p_enddate);
            }
            #endregion
            #endregion
            #region 处理按省市区汇总(返回数据集)
            string MapSummarySQL = "";
            List<Dictionary<string, object>> mapSummaryData = null;
            if (regiontype < 4)
            {
                if (regiontype == 1)
				{
                    MapSummarySQL = string.Format(@"SELECT
	                                    regionmap.regionid,
	                                    regionmap.regionname,
	                                    COUNT (*)
                                    FROM
	                                    (
		                                    SELECT
			                                    A.dataid as regionid,
                                                A.dataval as regionname
		                                    FROM
			                                    crm_sys_dictionary A
		                                    WHERE A.recstatus = 1 and dictypeid =124
	                                    ) regionmap
                                     inner JOIN (
	                                    {0}
                                    ) customer ON customer.province = regionmap.regionid
                                    GROUP BY
	                                    regionmap.regionid,
                                        regionmap.regionname
                                    ORDER BY
	                                    regionmap.regionid", CustomerSearchSQL);
                    mapSummaryData = _reportEngineRepository.ExecuteSQL(MapSummarySQL, new DbParameter[] { });
                }
                else if (regiontype == 2)
				{
                    MapSummarySQL = string.Format(@"SELECT
	                                    regionmap.regionid,
	                                    regionmap.regionname,
	                                    COUNT (*)
                                    FROM
	                                    (
		                                    SELECT
			                                    A.dataid as regionid,
                                                A.dataval as regionname
		                                    FROM
			                                    crm_sys_dictionary A
		                                    WHERE A.recstatus = 1 and dictypeid =125
			                                      AND A.extfield2 = '{1}'
	                                    ) regionmap
                                     inner JOIN (
	                                    {0}
                                    ) customer ON customer.city = regionmap.regionid
                                    GROUP BY
	                                    regionmap.regionid,
                                        regionmap.regionname
                                    ORDER BY
	                                    regionmap.regionid", CustomerSearchSQL, parentregionid);
                    mapSummaryData = _reportEngineRepository.ExecuteSQL(MapSummarySQL, new DbParameter[] { });
                }
                else if(regiontype == 3)
				{
                    MapSummarySQL = string.Format(@"SELECT
	                                    regionmap.regionid,
	                                    regionmap.regionname,
	                                    COUNT (*)
                                    FROM
	                                    (
		                                    SELECT
			                                    A.dataid as regionid,
                                                A.dataval as regionname
		                                    FROM
			                                    crm_sys_dictionary A
		                                    WHERE A.recstatus = 1 and dictypeid =126
			                                      AND A.extfield2 = '{1}'
	                                    ) regionmap
                                     inner JOIN (
	                                    {0}
                                    ) customer ON customer.area = regionmap.regionid
                                    GROUP BY
	                                    regionmap.regionid,
                                        regionmap.regionname
                                    ORDER BY
	                                    regionmap.regionid", CustomerSearchSQL, parentregionid);
                    mapSummaryData = _reportEngineRepository.ExecuteSQL(MapSummarySQL, new DbParameter[] { });
                }
                #region 对于省级返回，对省名称进行格式化
                if (regiontype == 1)
                {
                    foreach (Dictionary<string, object> item in mapSummaryData)
                    {
                        string regionName = item["regionname"].ToString();
                        if (regionName.EndsWith("省")) regionName = regionName.Substring(0, regionName.Length - 1);
                        if (regionName.EndsWith("市")) regionName = regionName.Substring(0, regionName.Length - 1);
                        if (ProvincesMap.ContainsKey(regionName))
                        {
                            regionName = ProvincesMap[regionName];
                        }
                        item["regionname"] = regionName;
                    }
                }

                #endregion
            }
            else {
                //显示明细地图，要返回实际地址
                MapSummarySQL = string.Format(@" select jsonb_extract_path_text(customer.customercompanyaddress,'lat') lat , 
                                                jsonb_extract_path_text(customer.customercompanyaddress,'lon') lon,
                                                jsonb_extract_path_text(customer.customercompanyaddress,'address') address,
                                                customer.recname,customer.recid,
                                                crm_func_entity_protocol_format_dictionary(12,customer.customerstatus::text) custstatus_name,
                                                crm_func_entity_protocol_format_dictionary(10,customer.coldstorsize::text) custscale_name,
                                                crm_func_entity_protocol_format_dictionary(11,customer.custlevel::text) custlevel_name,
                                                crm_func_entity_protocol_format_dictionary(9,customer.industry::text) custindust_name,
                                                customer.customercompanyaddress  from ({0}) customer 
                                                left join (
		                                                SELECT
			                                                A.dataid as regionid,
                                                            A.dataval as regionname
		                                                FROM
			                                                crm_sys_dictionary A
		                                                WHERE A.recstatus = 1 and dictypeid =126
			                                                  AND A.extfield1 = '{1}'
	                                                ) regionmap  ON customer.area = regionmap.regionid  where area = regionmap.regionid", CustomerSearchSQL, parentregionid);
                mapSummaryData = _reportEngineRepository.ExecuteSQL(MapSummarySQL, new DbParameter[] { });

            }
            Dictionary<string, object> tmpRecordData = new Dictionary<string, object>();
            tmpRecordData.Add("datasetname", "regiondata");
            tmpRecordData.Add("data", mapSummaryData);
            data.Add(tmpRecordData);
            #endregion
           
            #region 处理客户过滤条件返回值的公共部分
            string baseSQL = @"select  COALESCE(dict.dataid,0) dkey ,COALESCE(dict.dataval,'未定义') dvalue,COALESCE(dict.recorder,10000) dorder ,
                                sum (case when customer.recid is null then 0 else 1 end )  dcount
                                from 
                                ({0}) customer 
                                full outer join 
                                (select * from crm_sys_dictionary where dictypeid ={1} and recstatus = 1 ) dict  on dict.dataid = customer.{2} 
                                group by  dkey,dvalue,dorder
                                order by dorder ";
            CustomerSearchSQL = string.Format(@" select * 
                        from crm_sys_customer e  
                        where 1=1 and {0} and  {1} and {2} ",
						RoleSQL, RegionSQL, PersonSQL); 
            #endregion
            #region 处理客户状态数据集返回
            int DictType_Status = 52;
            string TotalSQL = string.Format(baseSQL, CustomerSearchSQL, DictType_Status, "customerstatus");
            List<Dictionary<string, object>> statusData = _reportEngineRepository.ExecuteSQL(TotalSQL, new DbParameter[] { });
            if (statusData != null) {
                tmpRecordData = new Dictionary<string, object>();
                tmpRecordData.Add("datasetname", "statusdata");
                tmpRecordData.Add("data", statusData);
                data.Add(tmpRecordData);
            }
            #endregion
            #region 处理客户规模数据集返回
            int DictType_Scale = 6;
            TotalSQL = string.Format(baseSQL, CustomerSearchSQL, DictType_Scale, "coldstorsize");
            List<Dictionary<string, object>> scaleData = _reportEngineRepository.ExecuteSQL(TotalSQL, new DbParameter[] { });
            if (scaleData != null)
            {
                tmpRecordData = new Dictionary<string, object>();
                tmpRecordData.Add("datasetname", "scaledata");
                tmpRecordData.Add("data", scaleData);
                data.Add(tmpRecordData); ;
            }
            #endregion
            #region 处理客户的级别数据集返回
            int DictType_Level = 7;
            TotalSQL = string.Format(baseSQL, CustomerSearchSQL, DictType_Level, "custlevel");
            List<Dictionary<string, object>> levelData = _reportEngineRepository.ExecuteSQL(TotalSQL, new DbParameter[] { });
            if (scaleData != null)
            {
                tmpRecordData = new Dictionary<string, object>();
                tmpRecordData.Add("datasetname", "leveldata");
                tmpRecordData.Add("data", levelData);
                data.Add(tmpRecordData); ;
            }
            #endregion
            #region 处理客户行业数据集返回
            int DictType_Indu = 114;
            TotalSQL = string.Format(baseSQL, CustomerSearchSQL, DictType_Indu, "industry");
            List<Dictionary<string, object>> indudata = _reportEngineRepository.ExecuteSQL(TotalSQL, new DbParameter[] { });
            if (scaleData != null)
            {
                tmpRecordData = new Dictionary<string, object>();
                tmpRecordData.Add("datasetname", "indudata");
                tmpRecordData.Add("data", indudata);
                data.Add(tmpRecordData); ;
            }
            #endregion
            Dictionary<string, List<Dictionary<string, object>>> retData = new Dictionary<string, List<Dictionary<string, object>>>();
            retData.Add("data", data);
            return retData;
        }
        /// <summary>
        /// 用于首页配置使用
        /// </summary>
        /// <param name="param"></param>
        /// <param name="sortby"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageCount"></param>
        /// <param name="userNum"></param>
        /// <returns></returns>
        public Dictionary<string, List<Dictionary<string, object>>> getCustomerDistrubitionSummaryForMainPage(
                            Dictionary<string, object> param,
                            Dictionary<string, string> sortby,
                            int pageIndex, int pageCount,
                            int userNum)
        {
            Dictionary<string, string> ProvincesMap = getDiffMap();
            Dictionary<string, string> ProvincesRevertMap = getDiffRevertMap();
            string RegionSQL = "";
            string p_regions = "";
            string RoleSQL = "";
            p_regions = ReportParamsUtils.parseString(param, "regions");
            int parentregionid = 1;
            int regiontype = 1;
            if (p_regions != null && p_regions.Length > 0)
            {
                string[] tmp = p_regions.Split(',');
                for (int i = 0; i < tmp.Length; i++)
                {
                    string regionName = tmp[i];
                    regionName = regionName.Trim();
                    bool NeedPass = false;
                    if (i == 0)
                    {
                        if (ProvincesRevertMap.ContainsKey(regionName))
                        {
                            regionName = ProvincesRevertMap[regionName];
                        }
                        NeedPass = CheckIsDirectly(regionName);
                    }
                    int newRegionID = getRegionId(regionName, regiontype);//得到新的id
                    regiontype = regiontype + 1;
                    if (NeedPass)
                    {
                        newRegionID = getRegionId(regionName, regiontype);
                        regiontype = regiontype + 1;
                    }
                    parentregionid = newRegionID;
                }
            }
            int rankInChina = 0;
            int CustomerCount = 0;
            int signCustomerCount = 0;
            Decimal signAmount = new Decimal(0);
            int signContractCount = 0;
            int curYearCustomerCount = 0;
            string strSQL = "";

            RegionSQL = " 1=2  ";
            if(regiontype == 1)
			{
                RegionSQL = string.Format(@" e.continent in (select dataid from crm_sys_dictionary where recstatus = 1 and dictypeid = 103) ");
            }
            else if(regiontype == 2)
			{
                RegionSQL = string.Format(@" e.province in (select dataid from crm_sys_dictionary where recstatus = 1 and dictypeid = 124 and extfield1 = '{0}')  ", parentregionid);
            }
            else if(regiontype == 3)
		    {
                RegionSQL = string.Format(@" e.city in (select dataid from crm_sys_dictionary where recstatus = 1 and dictypeid = 125 and extfield1 = '{0}')  ", parentregionid);
            }
            else if (regiontype == 4)
            {
                RegionSQL = string.Format(@" e.area in (select dataid from crm_sys_dictionary where recstatus = 1 and dictypeid = 126 and extfield1 = '{0}')  ", parentregionid);
            }

			Guid userDeptInfo = Guid.Empty;
			UserData userData = GetUserData(userNum, false);
			if (userData != null && userData.AccountUserInfo != null && userData.AccountUserInfo.DepartmentId != null)
			{
				userDeptInfo = userData.AccountUserInfo.DepartmentId;
			}

			string  userRoleRuleSQL = this._reportEngineRepository.getRuleSQLByUserId(customer_entityid, userNum, null);
            if (userRoleRuleSQL == null || userRoleRuleSQL.Length == 0)
            {
                RoleSQL = "1=1";
            }
            else
			{
				userRoleRuleSQL = RuleSqlHelper.FormatRuleSql(userRoleRuleSQL, userNum, userDeptInfo);
				RoleSQL = RoleSQL + " e.recid in (" + userRoleRuleSQL + ")";
            }
            List<Dictionary<string, object>> tmpDetail = null;
            #region 处理全国排名数据
            if (parentregionid == 1)
            {
                //全国模式，不用管排名
            }
            else
            {
                string tmpRegionSQL = " e.continent in (select dataid from crm_sys_dictionary where recstatus = 1 and dictypeid = 103) ";
                strSQL  = string.Format(@"SELECT
	                                    regionmap.regionid,
	                                    regionmap.regionname,
	                                   COUNT (*) customercount ,rank() over (order by count(*) desc ) customerrank
                                    FROM
	                                    (
                                            select dataid, extfield1 as regionid, dataval as regionname, 1 as regiontype, extfield2 as pregionid
                                            from crm_sys_dictionary where recstatus = 1 and dictypeid=124
                                            union all
                                            select dataid, extfield1 as regionid, dataval as regionname, 2 as regiontype, extfield2 as pregionid
                                            from crm_sys_dictionary where recstatus = 1 and dictypeid=125
                                            union all
                                            select dataid, extfield1 as regionid, dataval as regionname, 3 as regiontype, extfield2 as pregionid
                                            from crm_sys_dictionary where recstatus = 1 and dictypeid=126
	                                    ) regionmap
                                     inner JOIN (
	                                    select * from crm_sys_customer e where 1=1 and {0} and {1}
                                    ) customer ON (regionmap.regiontype = 1 and regionmap.dataid = customer.province) 
                                                or (regionmap.regiontype = 2 and regionmap.dataid = customer.city)
                                                or (regionmap.regiontype = 3 and regionmap.dataid = customer.area)
                                    GROUP BY
	                                    regionmap.regionid,
	                                    regionmap.regionname
                                    ORDER BY
	                                    regionmap.regionid", tmpRegionSQL, RoleSQL);
                strSQL = string.Format(@"select eee.customerrank from ({0}) eee where regionid='{1}'", strSQL, parentregionid);
                tmpDetail = this._reportEngineRepository.ExecuteSQL(strSQL, new DbParameter[] { });
                if (tmpDetail != null && tmpDetail.Count > 0)
                {
                    Dictionary<string, object> item = tmpDetail[0];
                    if (item.ContainsKey("customerrank") && item["customerrank"] != null)
                    {
                        int.TryParse(item["customerrank"].ToString(), out rankInChina);
                    }
                }
            }
            #endregion

            #region 处理客户数量
            strSQL = string.Format(@"select count(*) totalcount from crm_sys_customer e where  1=1 And e.recstatus = 1 and {0} And {1}  ", RoleSQL, RegionSQL);
            tmpDetail = this._reportEngineRepository.ExecuteSQL(strSQL, new DbParameter[] { });
            if (tmpDetail != null && tmpDetail.Count >0)
            {
                Dictionary<string, object> item = tmpDetail[0];
                if (item.ContainsKey("totalcount") && item["totalcount"] != null ) {
                    int.TryParse(item["totalcount"].ToString(), out CustomerCount);
                }
            }
            #endregion
            #region 获取本年签订合同的区域内签约客户数量
            string signSQL = @"select jsonb_extract_path_text(customer,'id')::uuid   
                            from crm_sys_contract
                            where   extract(year from signdate) = extract(year from now())   ";
            strSQL = string.Format(@"select count(*) totalcount from crm_sys_customer e where  1=1 And e.recstatus = 1 And {0} And {1}  and e.recid in ({2}) ", RoleSQL, RegionSQL,signSQL);
            tmpDetail = this._reportEngineRepository.ExecuteSQL(strSQL, new DbParameter[] { });
            if (tmpDetail != null && tmpDetail.Count > 0)
            {
                Dictionary<string, object> item = tmpDetail[0];
                if (item.ContainsKey("totalcount") && item["totalcount"] != null)
                {
                    int.TryParse(item["totalcount"].ToString(), out signCustomerCount);
                }
            }
            #endregion


            #region 处理业绩(不考虑合同权限),平均单价
            strSQL = string.Format(@"select count(*) totalcount,sum(contractamount) contractamount
                                    from crm_sys_contract a 
                                    inner join crm_sys_customer e on e.recid::text = jsonb_extract_path_text(a.customer,'id')
                                    where a.recstatus =1  And e.recstatus = 1 And {0} And {1}   ", RoleSQL, RegionSQL);
            tmpDetail = this._reportEngineRepository.ExecuteSQL(strSQL, new DbParameter[] { });
            if (tmpDetail != null && tmpDetail.Count > 0)
            {
                Dictionary<string, object> item = tmpDetail[0];
                if (item.ContainsKey("totalcount") && item["totalcount"] != null)
                {
                    int.TryParse(item["totalcount"].ToString(), out signContractCount);
                }
                if (item.ContainsKey("contractamount") && item["contractamount"] != null) {
                    Decimal.TryParse(item["contractamount"].ToString(), out signAmount);
                }
            }


            #endregion

            #region 新增客户数量
            strSQL = string.Format(@"select count(*) totalcount from crm_sys_customer e where  1=1 and extract(year from e.reccreated) = extract(year from now()) And e.recstatus = 1 And {0} And {1}  ", RoleSQL, RegionSQL);
            tmpDetail = this._reportEngineRepository.ExecuteSQL(strSQL, new DbParameter[] { });
            if (tmpDetail != null && tmpDetail.Count > 0)
            {
                Dictionary<string, object> item = tmpDetail[0];
                if (item.ContainsKey("totalcount") && item["totalcount"] != null)
                {
                    int.TryParse(item["totalcount"].ToString(), out curYearCustomerCount);
                }
            }
            #endregion
            #region 处理其他
            Decimal changeRate = new Decimal(0.0);
            Decimal avgContractAmount = new Decimal(0.00);
            if (CustomerCount > 0) {
                changeRate = new Decimal(signCustomerCount)  * 100/ new Decimal(CustomerCount);
                changeRate = Math.Round(changeRate, 2);
            }
            if (signContractCount > 0) {
                avgContractAmount = signAmount / signContractCount;
                avgContractAmount = Math.Round(avgContractAmount, 2);
            }
            #endregion

            #region 规整数据
            Dictionary<string, object> retItem = new Dictionary<string, object>();
            retItem.Add("rank", rankInChina);
            retItem.Add("changerate", changeRate);
            retItem.Add("customercount", CustomerCount);
            retItem.Add("signamount", signAmount);
            retItem.Add("signcount", signContractCount);
            retItem.Add("avgamount", avgContractAmount);
            retItem.Add("newcustomercount", curYearCustomerCount);
            List<Dictionary<string, object>> retList = new List<Dictionary<string, object>>();
            retList.Add(retItem);
            Dictionary<string,List<Dictionary<string, object>>> ret = new Dictionary<string, List<Dictionary<string, object>>>();
            ret.Add("data", retList);
            #endregion
            return ret;
        }

        private int getRegionId(string regionName, int regiontype)
        {
            try
            {
                var dictypeid = 0;
                if(regiontype == 1)
				{
                    dictypeid = 124;
				}
                else if(regiontype == 2)
				{
                    dictypeid = 125;
                }
                else if (regiontype == 3)
                {
                    dictypeid = 126;
                }
                string cmdText = "select extfield1 from crm_sys_dictionary where recstatus = 1 and dictypeid=" + dictypeid + " And dataval like '%" + regionName.Replace("'", "''") + "%'"; 
                List<Dictionary<string, object>> ret = _reportEngineRepository.ExecuteSQL(cmdText, new DbParameter[] { });
                if (ret != null && ret.Count > 0)
                {
                    return int.Parse(ret[0]["extfield1"].ToString());
                }
            }
            catch (Exception ex)
            {

            }
            return 0;
        }

        private int getRegionIdOld(string regionName, int regiontype, int parentid) {
            try {
                string cmdText = "Select regionid from crm_sys_region where regiontype=" + regiontype + " and pregionid=" + parentid + " And regionname like '%" + regionName.Replace("'", "''") + "%'";
                List<Dictionary<string, object>>  ret = _reportEngineRepository.ExecuteSQL(cmdText, new DbParameter[] { });
                if (ret != null && ret.Count > 0) {
                    return int.Parse(ret[0]["regionid"].ToString());
                }
            } catch (Exception ex){

            }
            return 0;
        }
        public bool CheckIsDirectly(string provinceName) {
            if (provinceName.StartsWith("北京")
                || provinceName.StartsWith("天津")
                || provinceName.StartsWith("上海")
                || provinceName.StartsWith("重庆"))
                return true;
            return false ;
        }
        public Dictionary<string, string> getDiffMap() {
            Dictionary<string, string> ret = new Dictionary<string, string>();
            ret.Add("澳门特别行政区", "澳门");
            ret.Add("广西壮族自治区", "广西");
            ret.Add("内蒙古自治区", "内蒙古");
            ret.Add("宁夏回族自治区", "宁夏");
            ret.Add("西藏自治区", "西藏");
            ret.Add("香港特别行政区", "香港");
            ret.Add("新疆维吾尔自治区", "新疆");
            return ret;
        }
        public Dictionary<string, string> getDiffRevertMap()
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();
            ret.Add( "澳门", "澳门特别行政区");
            ret.Add( "广西", "广西壮族自治区");
            ret.Add( "内蒙古", "内蒙古自治区");
            ret.Add( "宁夏", "宁夏回族自治区");
            ret.Add( "西藏", "西藏自治区");
            ret.Add( "香港", "香港特别行政区");
            ret.Add( "新疆", "新疆维吾尔自治区");
            return ret;
        }
    }
}
