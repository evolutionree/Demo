using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using UBeat.Crm.CoreApi.IRepository;

namespace UBeat.Crm.CoreApi.Services.Services.ReportDetail
{
    public class CustomersDistributionServices:EntityBaseServices
    {
        private IDynamicEntityRepository _dynamicEntityRepository;
        private IReportEngineRepository _reportEngineRepository;
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
            string RuleSQL = " 1=1 ";
            string RegionSQL = "";
            string CustomerSearchSQL = "";
            Dictionary<string, List<Dictionary<string, object>>> superdata = new Dictionary<string, List<Dictionary<string, object>>>();
            List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
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

            if (param.ContainsKey("@custindu"))
            {
                p_custindust = param["@custindu"].ToString();
            }
            else if (param.ContainsKey("custindu"))
            {
                p_custindust = param["custindu"].ToString();
            }
            #endregion


            #region 处理地区
            int parentregionid = 100000;
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
                    int newRegionID = getRegionId(regionName, regiontype, parentregionid) ;//得到新的id
                    regiontype = regiontype + 1;
                    if (NeedPass) {
                        newRegionID = getRegionId("", regiontype, newRegionID);
                        regiontype = regiontype + 1;
                    }
                    parentregionid = newRegionID;
                }
            }
            RegionSQL = " e.region in (select descendant from crm_sys_region_treepaths where ancestor = " + parentregionid + ") ";
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
                    StatusSQL = string.Format(@" e.custstatus in ({0})", inValue);
                }
                else {
                    StatusSQL = "1<>1";
                }
                if (hasZero) {
                    StatusSQL = string.Format(@" ({0} or e.custstatus is null  or e.custstatus = 0 )", StatusSQL);
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
                    ScaleSQL = string.Format(@" e.custscale in ({0})", inValue);
                }else
                {
                    ScaleSQL = " 1<>1 ";
                }
                if (hasZero)
                {
                    ScaleSQL = string.Format(@" ({0} or e.custscale is null  or e.custscale = 0 )", ScaleSQL);
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
                    InduSQL = string.Format(@" e.custindust in ({0})", inValue);
                }
                else {
                    InduSQL = " 1<> 1";
                }
                if (hasZero)
                {
                    InduSQL = string.Format(@" ({0} or e.custindust is null  or e.custindust = 0 )", InduSQL);
                }
            }
            #endregion
            CustomerSearchSQL = string.Format(@" select * from crm_sys_customer e  where 1=1 and {0} and  {1}  and {2} and {3} And {4} And {5}", RuleSQL, RegionSQL,StatusSQL,LevelSQL,ScaleSQL, InduSQL);
            #endregion
            #region 处理按省市区汇总(返回数据集)
            string MapSummarySQL = "";
            List<Dictionary<string, object>> mapSummaryData = null;
            if (regiontype < 4)
            {
                //处理汇总方式（也就是当前显示要求是省市区三级）
                MapSummarySQL = string.Format(@"SELECT
	                                    regionmap.regionid,
	                                    regionInfo.regionname,
	                                    COUNT (*)
                                    FROM
	                                    (
		                                    SELECT
			                                    A .regionid,
			                                    b.descendant
		                                    FROM
			                                    crm_sys_region A
		                                    LEFT OUTER JOIN crm_sys_region_treepaths b ON A .regionid = b.ancestor
		                                    WHERE
			                                    A .pregionid = {1}
	                                    ) regionmap
                                     inner JOIN (
	                                    {0}
                                    ) customer ON customer.region = regionmap.descendant
                                    INNER JOIN crm_sys_region regionInfo ON regionInfo.regionid = regionmap.regionid
                                    GROUP BY
	                                    regionmap.regionid,
	                                    regionInfo.regionname
                                    ORDER BY
	                                    regionInfo.regionname", CustomerSearchSQL, parentregionid);
                mapSummaryData = _reportEngineRepository.ExecuteSQL(MapSummarySQL, new DbParameter[] { });
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
                MapSummarySQL = string.Format(@" select jsonb_extract_path_text(customer.custaddr,'lat')  lat , 
                                                jsonb_extract_path_text(customer.custaddr,'lon')  lon,
                                                jsonb_extract_path_text(customer.custaddr,'address')  address,
                                                customer.recname,customer.recid,
                                                crm_func_entity_protocol_format_dictionary(12,customer.custstatus::text) custstatus_name,
                                                crm_func_entity_protocol_format_dictionary(10,customer.custscale::text) custscale_name,
                                                crm_func_entity_protocol_format_dictionary(11,customer.custlevel::text) custlevel_name,
                                                crm_func_entity_protocol_format_dictionary(9,customer.custindust::text) custindust_name,
                                                customer.custaddr  from ({0}) customer", CustomerSearchSQL);
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
                        where 1=1 and {0} and  {1}  ",
                        RuleSQL, RegionSQL); 
            #endregion
            #region 处理客户状态数据集返回
            int DictType_Status = 12;
            string TotalSQL = string.Format(baseSQL, CustomerSearchSQL, DictType_Status, "custstatus");
            List<Dictionary<string, object>> statusData = _reportEngineRepository.ExecuteSQL(TotalSQL, new DbParameter[] { });
            if (statusData != null) {
                tmpRecordData = new Dictionary<string, object>();
                tmpRecordData.Add("datasetname", "statusdata");
                tmpRecordData.Add("data", statusData);
                data.Add(tmpRecordData);
            }
            #endregion
            #region 处理客户规模数据集返回
            int DictType_Scale = 10;
            TotalSQL = string.Format(baseSQL, CustomerSearchSQL, DictType_Scale, "custscale");
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
            int DictType_Level = 11;
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
            int DictType_Indu = 9;
            TotalSQL = string.Format(baseSQL, CustomerSearchSQL, DictType_Indu, "custindust");
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
            string customer_entityid = "";
            Dictionary<string, string> ProvincesMap = getDiffMap();
            Dictionary<string, string> ProvincesRevertMap = getDiffRevertMap();
            string RegionSQL = "";
            string p_regions = "";
            string RoleSQL = "";
            p_regions = ReportParamsUtils.parseString(param, "regions");
            int parentregionid = 100000;
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
                    int newRegionID = getRegionId(regionName, regiontype, parentregionid);//得到新的id
                    regiontype = regiontype + 1;
                    if (NeedPass)
                    {
                        newRegionID = getRegionId("", regiontype, newRegionID);
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
            RegionSQL = " f.region in (select descendant from crm_sys_region_treepaths where ancestor = " + parentregionid + ") ";
           string  userRoleRuleSQL = this._reportEngineRepository.getRuleSQLByUserId(customer_entityid, userNum, null);
            if (userRoleRuleSQL == null || userRoleRuleSQL.Length == 0)
            {
                RoleSQL = "1=1";
            }
            else {
                RoleSQL = string.Format(" f.recid in ({0}) ", userRoleRuleSQL);
            }
            List<Dictionary<string, object>> tmpDetail = null;
            #region 处理全国排名数据
            if (parentregionid == 100000)
            {
                //全国模式，不用管排名
            }
            else
            {
                string tmpRegionSQL = " f.custregion in (select descendant from crm_sys_region_treepaths where ancestor = " + 100000.ToString() + ") ";
                strSQL  = string.Format(@"SELECT
	                                    regionmap.regionid,
	                                    regionInfo.regionname,
	                                   COUNT (*) customercount ,rank() over (order by count(*) desc ) customerrank
                                    FROM
	                                    (
		                                    SELECT
			                                    A .regionid,
			                                    b.descendant
		                                    FROM
			                                    crm_sys_region A
		                                    LEFT OUTER JOIN crm_sys_region_treepaths b ON A .regionid = b.ancestor
		                                    WHERE
			                                    A .pregionid = 100000
	                                    ) regionmap
                                     inner JOIN (
	                                    select * from crm_sys_customer f where 1=1 and {0} and {1}
                                    ) customer ON customer.custregion = regionmap.descendant
                                    INNER JOIN crm_sys_region regionInfo ON regionInfo.regionid = regionmap.regionid
                                    GROUP BY
	                                    regionmap.regionid,
	                                    regionInfo.regionname
                                    ORDER BY
	                                    regionInfo.regionname", tmpRegionSQL, RoleSQL);
                strSQL = string.Format(@"select eee.customerrank from ({0}) eee where regionid={1}", strSQL, parentregionid);
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
            strSQL = string.Format(@"select count(*) totalcount from crm_sys_customer f where  1=1 And f.recstatus = 1 And {0} And {1}  ", RoleSQL, RegionSQL);
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
            string signSQL = @"select jsonb_extract_path_text(customerid,'id')::uuid   
                            from crm_sys_contract
                            where   extract(year from Contractdate) = extract(year from now())   ";
            strSQL = string.Format(@"select count(*) totalcount from crm_sys_customer f where  1=1 And f.recstatus = 1 And {0} And {1}  and f.recid in ({2}) ", RoleSQL, RegionSQL,signSQL);
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
            strSQL = string.Format(@"select count(*) totalcount,sum(contractvolume) contractamount
                                    from crm_sys_contract a 
                                    inner join crm_sys_customer f on f.recid::text = jsonb_extract_path_text(a.customerid,'id')
                                    where a.recstatus =1  And f.recstatus = 1 And {0} And {1}   ", RoleSQL, RegionSQL);
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
            strSQL = string.Format(@"select count(*) totalcount from crm_sys_customer f where  1=1 and extract(year from f.reccreated) = extract(year from now()) And f.recstatus = 1 And {0} And {1}  ", RoleSQL, RegionSQL);
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
        private int getRegionId(string regionName, int regiontype, int parentid) {
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
