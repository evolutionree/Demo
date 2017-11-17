using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.IRepository;

namespace UBeat.Crm.CoreApi.Services.Services.ReportDetail
{
    public class MainPageReportServices : EntityBaseServices
    {
        private static string OPP_EntityID = "2c63b681-1de9-41b7-9f98-4cf26fd37ef1";
        private IDynamicEntityRepository _dynamicEntityRepository;
        private IReportEngineRepository _reportEngineRepository;
        public MainPageReportServices(IDynamicEntityRepository dynamicEntityRepository, IReportEngineRepository reportEngineRepository) {
            this._dynamicEntityRepository = dynamicEntityRepository;
            this._reportEngineRepository = reportEngineRepository;
        }

        /// <summary>
        /// 用于首页报表中的业绩计算位置
        /// </summary>
        /// <param name="param"></param>
        /// <param name="sortby"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageCount"></param>
        /// <param name="userNum"></param>
        /// <returns></returns>
        public Dictionary<string, List<Dictionary<string, object>>> getSaleAmountAndReceiveAmount(
                            Dictionary<string, object> param,
                            Dictionary<string, string> sortby,
                            int pageIndex, int pageCount,
                            int userNum)
        {
            #region 定义变量
            string p_range = "";
            string p_rangetype = "";
            string p_searchperiod = "";
            string p_target1 = "";
            string p_target2 = "";
            string p_target3 = "";
            DateTime dtFrom = DateTime.MinValue;
            DateTime dtTo = DateTime.MaxValue;
            #endregion

            #region 处理参数
            p_range = ReportParamsUtils.parseString(param, "range");
            p_rangetype = ReportParamsUtils.parseString(param, "rangetype");
            p_searchperiod = ReportParamsUtils.parseString(param, "searchperiod");
            p_target1 = ReportParamsUtils.parseString(param, "target1");
            p_target2 = ReportParamsUtils.parseString(param, "target2");
            p_target3 = ReportParamsUtils.parseString(param, "target3");

            if (p_range == null || p_range.Length == 0)
            {
                throw (new Exception("查询范围异常"));
            }
            if (p_rangetype != "1" && p_rangetype != "2")
            {
                throw (new Exception("查询范围类型异常"));
            }
            if (p_searchperiod != "本年" && p_searchperiod != "本季度" && p_searchperiod != "本月")
            {
                throw (new Exception("查询期间异常"));
            }
            if (p_searchperiod == "本年")
            {
                dtFrom = new DateTime(System.DateTime.Now.Year, 1, 1);
                dtTo = new DateTime(System.DateTime.Now.Year, 12, 31);
            }
            else if (p_searchperiod == "本月")
            {
                dtFrom = new DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, 1);
                dtTo = dtFrom.AddMonths(1).AddDays(-1);
            }
            else if (p_searchperiod == "本季度")
            {
                int month = System.DateTime.Now.Month;
                if (month >= 1 && month <= 3)
                {
                    dtFrom = new System.DateTime(System.DateTime.Now.Year, 1, 1);

                    dtTo = new System.DateTime(System.DateTime.Now.Year, 3, 31);
                }
                else if (month <= 6)
                {
                    dtFrom = new System.DateTime(System.DateTime.Now.Year, 4, 1);

                    dtTo = new System.DateTime(System.DateTime.Now.Year, 6, 30);
                }
                else if (month <= 9)
                {
                    dtFrom = new System.DateTime(System.DateTime.Now.Year, 7, 1);

                    dtTo = new System.DateTime(System.DateTime.Now.Year, 9, 30);
                }
                else if (month <= 12)
                {
                    dtFrom = new System.DateTime(System.DateTime.Now.Year, 10, 1);

                    dtTo = new System.DateTime(System.DateTime.Now.Year, 12, 31);
                }
                else
                {
                    throw (new Exception("日期范围异常"));
                }
            }
            #endregion

            Decimal completedAmount1 = new Decimal(0.00);
            TargetAndCompletedReportServices completedService =(TargetAndCompletedReportServices) this.dynamicCreateService(typeof(TargetAndCompletedReportServices).FullName, true);
            Dictionary<string, object> otherParam = new Dictionary<string, object>();
            otherParam.Add("unit", 1);
            otherParam.Add("targetid", p_target1);
            otherParam.Add("year", dtFrom.Year);
            otherParam.Add("month_from", dtFrom.Month);
            otherParam.Add("month_to", dtTo.Month);
            otherParam.Add("range_type", p_rangetype);
            otherParam.Add("range", p_range);
            Dictionary<string,List<Dictionary<string,object>>> tmp =  completedService.getTargetTotalSummary(otherParam, sortby, pageIndex, pageCount, userNum);
            if (tmp != null && tmp.ContainsKey("data") && tmp["data"] != null
                    && tmp["data"].Count > 0) {
                Dictionary<string, object> item = tmp["data"][0];
                if (item.ContainsKey("completedamount") && item["completedamount"] != null) {
                    Decimal.TryParse(item["completedamount"].ToString(), out completedAmount1);
                }
            }
            Decimal completedAmount2 = new Decimal(0.00);
            otherParam = new Dictionary<string, object>();
            otherParam.Add("unit", 1);
            otherParam.Add("targetid", p_target2);
            otherParam.Add("year", dtFrom.Year);
            otherParam.Add("month_from", dtFrom.Month);
            otherParam.Add("month_to", dtTo.Month);
            otherParam.Add("range_type", p_rangetype);
            otherParam.Add("range", p_range);
            tmp = completedService.getTargetTotalSummary(otherParam, sortby, pageIndex, pageCount, userNum);
            if (tmp != null && tmp.ContainsKey("data") && tmp["data"] != null
                    && tmp["data"].Count > 0)
            {
                Dictionary<string, object> item = tmp["data"][0];
                if (item.ContainsKey("completedamount") && item["completedamount"] != null)
                {
                    Decimal.TryParse(item["completedamount"].ToString(), out completedAmount2);
                }
            }
            Decimal completedAmount3 = new Decimal(0.00);
            otherParam = new Dictionary<string, object>();
            otherParam.Add("unit", 1);
            otherParam.Add("targetid", p_target3);
            otherParam.Add("year", dtFrom.Year);
            otherParam.Add("month_from", dtFrom.Month);
            otherParam.Add("month_to", dtTo.Month);
            otherParam.Add("range_type", p_rangetype);
            otherParam.Add("range", p_range);
            tmp = completedService.getTargetTotalSummary(otherParam, sortby, pageIndex, pageCount, userNum);
            if (tmp != null && tmp.ContainsKey("data") && tmp["data"] != null
                    && tmp["data"].Count > 0)
            {
                Dictionary<string, object> item = tmp["data"][0];
                if (item.ContainsKey("completedrate") && item["completedrate"] != null)
                {
                    Decimal.TryParse(item["completedrate"].ToString(), out completedAmount3);
                }
            }
            Dictionary<string, object> tmpItem = new Dictionary<string, object>();
            tmpItem.Add("saleamount", completedAmount1.ToString("N2"));
            tmpItem.Add("receivecount", completedAmount2.ToString("N2"));
            tmpItem.Add("completedrate",string.Format("{0}%", completedAmount3.ToString("N2")));
            List <Dictionary<string, object>> tmpList = new List<Dictionary<string, object>>();
            Dictionary<string, List<Dictionary<string, object>>> retList = new Dictionary<string, List<Dictionary<string, object>>>();
            tmpList.Add( tmpItem);
            retList.Add("data", tmpList);
            return retList;
        }
        
    }
}
