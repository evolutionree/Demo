using System;
using System.Data.Common;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.DynamicEntity;
using UBeat.Crm.CoreApi.DomainModel.Reports;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.ReportDefine;
using UBeat.Crm.CoreApi.DomainModel.Vocation;
using UBeat.Crm.CoreApi.Services.Utility;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class ReportEngineServices : EntityBaseServices
    {
        private static Guid ReportDefineEntityId = Guid.Parse("ee74e5e9-64f4-4ed7-a025-04c68a5eb922");
        private static Guid ReportDataSourceEntityId = Guid.Parse("590ca8e5-ac65-4db6-8f70-99ab47c16c18");
        private static Guid MainPageDefine_EntityId = Guid.Parse("4a7be196-7a7f-43ba-b567-f0c1df928ad2");
        private static Guid MainPageSubItem_EntityId = Guid.Parse("adfc18a5-550e-4a4a-80a2-1c84a09ae794");
        private readonly IDynamicEntityRepository _dynamicEntityRepository;
        private readonly IReportEngineRepository _reportEngineRepository;
        public ReportEngineServices(IDynamicEntityRepository dynamicEntityRepository, IReportEngineRepository reportEngineRepository)
        {
            _dynamicEntityRepository = dynamicEntityRepository;
            _reportEngineRepository = reportEngineRepository;
        }
        /***
         * 获取报表定义
         * **/
        public ReportDefineInfo queryReportDefineInfo(string id, int userNum)
        {
            UserData userData = GetUserData(userNum, false);
            DynamicEntityDetailtMapper modeltemp = new DynamicEntityDetailtMapper()
            {
                EntityId = ReportDefineEntityId,
                RecId = Guid.Parse(id),
                NeedPower = 0
            };
            IDictionary<string, object> detail = _dynamicEntityRepository.Detail(modeltemp, userNum);
            if (detail == null) return null;
            ReportDefineInfo ret = ReportDefineInfo.fromDict(detail);
            RecalcDefaultValueScheme(ret, userData);
            return ret;

        }
        /// <summary>
        /// 计算过滤条件的默认值
        /// </summary>
        /// <param name="reportInfo"></param>
        public void RecalcDefaultValueScheme(ReportDefineInfo reportInfo, UserData userdata) {
             
            foreach (ReportComponentInfo componentInfo in reportInfo.components) {
                if ((ReportComponentInfo_CtrlType)componentInfo.CtrlType == ReportComponentInfo_CtrlType.FilterCtrl)
                {
                    if (componentInfo.FilterExtInfo == null || componentInfo.FilterExtInfo.Ctrls == null
                        || componentInfo.FilterExtInfo.Ctrls.Count == 0) continue;
                    foreach (FilterControlInfo filterInfo in componentInfo.FilterExtInfo.Ctrls) {
                        switch ((ReportFilter_CtrlType)filterInfo.CtrlType) {
                            case ReportFilter_CtrlType.Text:
                                CalcTextDefaultValueScheme(filterInfo,userdata.UserId);
                                break;
                            case ReportFilter_CtrlType.Commonbox:
                                CalcCommonBoxDefaultValueScheme(filterInfo, userdata.UserId);
                                break;
                            case ReportFilter_CtrlType.MultiChoose:
                                CalcMultiDefaultValueScheme(filterInfo, userdata);
                                break;
                            case ReportFilter_CtrlType.Series:
                                break;
                            case ReportFilter_CtrlType.DateCtl:
                                CalcDateDefaultValueScheme(filterInfo, userdata.UserId);
                                break;
                        }
                    }
                }
            }
        }

        private void CalcMultiDefaultValueScheme(FilterControlInfo filterInfo, UserData userdata) {
            if (filterInfo == null) return;
            if (filterInfo.MultiChooseData == null || filterInfo.MultiChooseData.DefaultValueScheme == null || filterInfo.MultiChooseData.DefaultValueScheme.Length == 0) {
                return;
            }
            List<Dictionary<string, object>> val = ReportFilterDefaultSchemeParseUtil.parseMultiScheme(filterInfo.MultiChooseData.DefaultValueScheme, userdata);
            if (val != null) {
                filterInfo.MultiChooseData.DefaultValues = val;
            }

        }
        /// <summary>
        /// 根据文本默认值模式，计算实际的文本默认值
        /// </summary>
        /// <param name="filterInfo"></param>
        /// <param name="userNum"></param>
        private void CalcTextDefaultValueScheme(FilterControlInfo filterInfo, int userNum) {
            if (filterInfo == null) return;
            if (filterInfo.TextDefaultValueScheme != null && filterInfo.TextDefaultValueScheme.Length != 0)
            {
                string defaultValue = ReportFilterDefaultSchemeParseUtil.parseScheme(filterInfo.TextDefaultValueScheme, userNum);
                filterInfo.TextDefaultValue = defaultValue;
            }
            if (filterInfo.TextDefaultValue_NameScheme != null && filterInfo.TextDefaultValue_NameScheme.Length != 0)
            {
                string defaultValue = ReportFilterDefaultSchemeParseUtil.parseScheme(filterInfo.TextDefaultValue_NameScheme, userNum);
                filterInfo.TextDefaultValue_Name = defaultValue;
            }

        }
        /// <summary>
        /// 根据日期默认值模式，计算实际的日期默认值
        /// </summary>
        /// <param name="filterInfo"></param>
        /// <param name="userNum"></param>
        private void CalcDateDefaultValueScheme(FilterControlInfo filterInfo, int userNum)
        {
            if (filterInfo == null) return;
            if (filterInfo.DateDefaultValueScheme == null || filterInfo.DateDefaultValueScheme.Length == 0) return;
            string defaultValue = ReportFilterDefaultSchemeParseUtil.parseScheme(filterInfo.DateDefaultValueScheme, userNum);
            filterInfo.DateDefaultValue = defaultValue;
        }
        /// <summary>
        /// 根据下拉框的默认值模型，计算下拉框的默认值
        /// </summary>
        /// <param name="filterInfo"></param>
        /// <param name="userNum"></param>
        private void CalcCommonBoxDefaultValueScheme(FilterControlInfo filterInfo ,int userNum)
        {
            if (filterInfo == null) return;
            if (filterInfo.ComboData == null) return;
            if (filterInfo.ComboData.DefaultValueScheme != null &&  filterInfo.ComboData.DefaultValueScheme.Length != 0)
            {
                string defaultValue = ReportFilterDefaultSchemeParseUtil.parseScheme(filterInfo.ComboData.DefaultValueScheme, userNum);
                filterInfo.ComboData.DefaultValue = defaultValue;
            }
            if (filterInfo.ComboData.DefaultValue_NameScheme != null && filterInfo.ComboData.DefaultValue_NameScheme.Length > 0) {
                string defaultValue = ReportFilterDefaultSchemeParseUtil.parseScheme(filterInfo.ComboData.DefaultValueScheme, userNum);
                filterInfo.ComboData.DefaultValue_Name = defaultValue;
            }
        }

        /***
         * 根据数据源获取数据
         * */
        public OutputResult<object> queryDataFromDataSource(IServiceProvider serviceProvider, DataSourceQueryDataModel queryModel, int userNum) {
            int errorCode = 0;
            string errorMsg = "";
            UserData u = this.GetUserData(userNum);
            DynamicEntityDetailtMapper modeltemp = new DynamicEntityDetailtMapper()
            {
                EntityId = ReportDataSourceEntityId,
                RecId = Guid.Parse(queryModel.DataSourceId),
                NeedPower = 0
            };
            IDictionary<string, object> detail = _dynamicEntityRepository.Detail(modeltemp, userNum);
            if (detail == null) {
                return new OutputResult<object>(null, "无法找到报表数据源", 1);
            }
            ReportDataSourceDefineInfo dataSourceInfo = ReportDataSourceDefineInfo.fromDict(detail);
            if (dataSourceInfo == null) {
                return new OutputResult<object>(null, "报表数据源信息定义异常", 1);
            }
            Guid tmpEntityId = Guid.Empty;
            if (dataSourceInfo.Params != null ) {
                if ((dataSourceInfo.Params.Contains("@usernum,") || dataSourceInfo.Params.EndsWith("@usernum"))) { 
                if (queryModel.Parameters != null) {
                    if (queryModel.Parameters.ContainsKey("@usernum")) {
                        queryModel.Parameters.Remove("@usernum");
                    }
                    queryModel.Parameters.Add("@usernum", userNum);
                }
                }
                if ((dataSourceInfo.Params.Contains("@userno,") || dataSourceInfo.Params.EndsWith("@userno")))
                {
                    if (queryModel.Parameters != null)
                    {
                        if (queryModel.Parameters.ContainsKey("@userno"))
                        {
                            queryModel.Parameters.Remove("@userno");
                        }
                        queryModel.Parameters.Add("@userno", userNum);
                    }
                }
                if ((dataSourceInfo.Params.Contains("@username,") || dataSourceInfo.Params.EndsWith("@username")))
                {
                    if (queryModel.Parameters != null)
                    {
                        if (queryModel.Parameters.ContainsKey("@username"))
                        {
                            queryModel.Parameters.Remove("@username");
                        }
                        queryModel.Parameters.Add("@username", u.AccountUserInfo.UserName);
                    }
                }
                if (dataSourceInfo.Params.Contains("@pagesize,") || dataSourceInfo.Params.EndsWith("@pagesize")) {
                    if (queryModel.Parameters == null) queryModel.Parameters = new Dictionary<string, object>();
                    if (queryModel.Parameters.ContainsKey("@pagesize") == false) {
                        queryModel.Parameters.Add("@pagesize", 100000);
                    }
                }
                if (dataSourceInfo.Params.Contains("@pageindex,") || dataSourceInfo.Params.EndsWith("@pageindex"))
                {
                    if (queryModel.Parameters == null) queryModel.Parameters = new Dictionary<string, object>();
                    if (queryModel.Parameters.ContainsKey("@pageindex") == false)
                    {
                        queryModel.Parameters.Add("@pageindex", 1);
                    }
                }
            }
            DataSourceQueryDataModel finalQueryModel = queryModel;
            OutputResult<object> resujlt = ExcuteSelectAction((transaction, arg, userData) =>
            {
                Dictionary<string, List<Dictionary<string, object>>> ret = null;
                if (dataSourceInfo.DstType == 1)
                {
                    //通过普通SQL
                    ret = _reportEngineRepository.queryDataFromDataSource_CommonSQL(transaction, dataSourceInfo.DataSQL, finalQueryModel.Parameters);
                }
                else if (dataSourceInfo.DstType == 2) {
                    ret = _reportEngineRepository.queryDataFromDataSource_FuncSQL(transaction, dataSourceInfo.DataSQL, dataSourceInfo.Params, finalQueryModel.Parameters);
                }
                else if (dataSourceInfo.DstType == 3)
                {
                    //调用本地服务
                    try
                    {
                        string serviceName = "UBeat.Crm.CoreApi.Services.Services.SaleForcastReportServices";
                        string methodName = "testReportData";
                        int tmp = dataSourceInfo.DataSQL.LastIndexOf(".");
                        serviceName = dataSourceInfo.DataSQL.Substring(0, tmp);
                        methodName = dataSourceInfo.DataSQL.Substring(tmp + 1);
                        object service = serviceProvider.GetService(Type.GetType(serviceName));
                        System.Reflection.MethodInfo methodInfo = null;
                        Type type = Type.GetType(serviceName);
                        methodInfo = type.GetMethod(methodName, new Type[] { typeof(Dictionary<string, object>), typeof(Dictionary<string, string>), typeof(int), typeof(int), typeof(int) });
                        if (finalQueryModel.Parameters == null)
                        {
                            finalQueryModel.Parameters = new Dictionary<string, object>();
                        }
                        if (finalQueryModel.SortBys == null)
                        {
                            finalQueryModel.SortBys = new Dictionary<string, string>();
                        }
                        var param = new object[] { finalQueryModel.Parameters, finalQueryModel.SortBys, finalQueryModel.PageIndex, finalQueryModel.PageCount, userNum };
                        ret = (Dictionary<string, List<Dictionary<string, object>>>)methodInfo.Invoke(service, param);
                    }
                    catch (Exception ex)
                    {
                        errorCode = -1;
                        if (ex.InnerException != null)
                        {

                            errorMsg = ex.InnerException.Message;
                        }
                        else
                        {

                            errorMsg = ex.Message;
                        }
                    }

                }
                else
                {
                    return new OutputResult<object>(null, "暂时不支持的数据源类型", 1);
                }
                if (ret == null) {
                    ret = new Dictionary<string, List<Dictionary<string, object>>>();
                }
                if (ret.ContainsKey("data") == false) {
                    ret.Add("data", new List<Dictionary<string, object>>());
                }
                if (ret.ContainsKey("page") == false) {
                    List<Dictionary<string, object>> pages = new List<Dictionary<string, object>>();
                    Dictionary<string, object> pageInfo = new Dictionary<string, object>();
                    pageInfo.Add("pageIndex", 0);
                    pageInfo.Add("pageCount", 1);
                    pages.Add(pageInfo);
                    ret.Add("page", pages);
                }
                Dictionary<string, object> retDat = new Dictionary<string, object>();
                foreach (string key in ret.Keys)
                {
                    retDat.Add(key, ret[key]);
                }
                retDat.Add("InstId", queryModel.InstId);
                return new OutputResult<object>(retDat, errorMsg, errorCode);
            }, "", tmpEntityId, userNum);

            return resujlt;

        }


        /***
         * 获取报表列表，用于Mobile
         * 根据报表的状态(发布状态)
         * */
        public OutputResult<object> queryReportListForMobile(int userNum) {
            List<ReportFolderInfo> list = this._reportEngineRepository.queryMobileReportList();
            if (list == null) list = new List<ReportFolderInfo>();
            Dictionary<string, FunctionInfo> allFuncs = this.AllMyFunctionIds(userNum);
            //CheckReportItemHasFunc(list, allFuncs);
            List<ReportFolderInfo> retList = new List<ReportFolderInfo>();
            //ClearEmptyReportFolder(list);
            FlatMobileReportList(retList, list);
            return new OutputResult<object>(retList);
        }

        private void FlatMobileReportList(List<ReportFolderInfo> retList, List<ReportFolderInfo> subFolders) {
            foreach (ReportFolderInfo info in subFolders) {
                if (info.IsFolder)
                {
                    if (info.SubFolders != null)
                    {
                        FlatMobileReportList(retList, info.SubFolders);
                    }
                }
                else {
                    if (info.ReportId != null && info.ReportId != Guid.Empty) {
                        retList.Add(info);
                    }
                }
            }

        }

        /// <summary>
        /// 内存删除没有权限的报表菜单项（这里仅仅处理关联了报表的item，而纯粹目录的不处理）
        /// </summary>
        /// <param name="folders"></param>
        /// <param name="funcs"></param>
        private void CheckReportItemHasFunc(List<ReportFolderInfo > folders , Dictionary<string, FunctionInfo> funcs )
        {
            List<ReportFolderInfo> needDeletedFolder = new List<ReportFolderInfo>();
            foreach (ReportFolderInfo item in folders) {
                if (item.IsFolder)
                {
                    //这里是目录
                    if (item.SubFolders != null && item.SubFolders.Count > 0) {
                        CheckReportItemHasFunc(item.SubFolders, funcs);
                    }
                }
                else {
                    //这里是明细报表
                    item.SubFolders = new List<ReportFolderInfo>();//清除所有不合理的子菜单
                    if (item.FuncId != null && item.FuncId.Length > 0) {
                        if (funcs.ContainsKey(item.FuncId) == false) {
                            needDeletedFolder.Add(item);
                        }
                    }
                }
            }
            foreach (ReportFolderInfo item in needDeletedFolder) {
                folders.Remove(item);
            }
        }


        /// <summary>
        /// 把空白的报表目录清除
        /// </summary>
        /// <param name="folders"></param>
        private void ClearEmptyReportFolder(List<ReportFolderInfo> folders) {
            List<ReportFolderInfo> needDeletedFolder = new List<ReportFolderInfo>();
            foreach (ReportFolderInfo item in folders) {
                if (item.IsFolder && (item.SubFolders == null || item.SubFolders.Count == 0))
                {
                    needDeletedFolder.Add(item);
                }
                else if (item.IsFolder)
                {
                    ClearEmptyReportFolder(item.SubFolders);
                    if (item.SubFolders.Count == 0) {
                        needDeletedFolder.Add(item);
                    }
                }
            }
            foreach (ReportFolderInfo item in needDeletedFolder)
            {
                folders.Remove(item);
            }
        }

        /// <summary>
        /// 获取个人所有权限项
        /// </summary>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        protected Dictionary<string, FunctionInfo> AllMyFunctionIds(int userNumber)
        {
            //获取公共缓存数据
            var commonData = GetCommonCacheData(userNumber);
            //获取个人用户数据
            UserData userData = GetUserData(userNumber);
            Dictionary<string, FunctionInfo> myFunctions = new Dictionary<string, FunctionInfo>();
            if (userData.Vocations != null)
            {
                foreach (VocationInfo vInfo in userData.Vocations)
                {
                    if (vInfo.Functions == null) continue;
                    foreach (FunctionInfo func in vInfo.Functions)
                    {
                        if (myFunctions.ContainsKey(func.FuncId.ToString()) == false)
                        {
                            myFunctions.Add(func.FuncId.ToString(), func);
                        }
                    }

                }
            }
            return myFunctions;
        }
        public void repairFunc(int userNum) {
            this._reportEngineRepository.repairWebReportFunctions(null, userNum);
            this._reportEngineRepository.repairMobReportFunctions(null, userNum);
            this._reportEngineRepository.repairWebMenuForReport(null, userNum);
            IncreaseDataVersion(DomainModel.Version.DataVersionType.BasicData);
        }


        public MainPageReportInfo getMyMainPageReport(int userNum) {
            string mainpageid = "";
            //去个人报表中找
            //到角色首页找
            //使用全局首页
            //这里应该是可以找到id，如果没有找到 ，就直接报错
            mainpageid = "f9610e36-e42e-465d-9cd8-c6cd42100731";
            if (mainpageid == null || mainpageid.Length == 0) {
                throw (new Exception("没有找到首页定义，请联系系统管理员设定首页"));
            }

            UserData userData = GetUserData(userNum, false);    
            DynamicEntityDetailtMapper modeltemp = new DynamicEntityDetailtMapper()
            {
                EntityId = MainPageDefine_EntityId,
                RecId = Guid.Parse(mainpageid),
                NeedPower = 0
            };
            IDictionary<string, object> detail = _dynamicEntityRepository.Detail(modeltemp, userNum);
            if (detail == null) return null;
            MainPageReportInfo  ret = MainPageReportInfo.fromDict(detail);
            if (ret == null) return null;
            #region 处理subreportitem的属性
            Dictionary<string, MainPageReportSubItemDefineInfo> cachedSubItemInfo = new Dictionary<string, MainPageReportSubItemDefineInfo>();
            foreach (MainPageReportColumnInfo colInfo in ret.ColumnsInfo) {
                foreach (MainPageReportCellItemInfo cellInfo in colInfo.CellItems) {
                    if (cellInfo.ReportItemId != null && cellInfo.ReportItemId.Length > 0) {
                        if (cachedSubItemInfo.ContainsKey(cellInfo.ReportItemId)) {
                            cellInfo.ReportItemInfo = cachedSubItemInfo[cellInfo.ReportItemId];
                            continue;
                        }
                        //开始找ReportItemInfo的信息
                        modeltemp = new DynamicEntityDetailtMapper()
                        {
                            EntityId = MainPageSubItem_EntityId,
                            RecId = Guid.Parse(cellInfo.ReportItemId),
                            NeedPower = 0
                        };
                        IDictionary<string, object> subDetail = _dynamicEntityRepository.Detail(modeltemp, userNum);
                        MainPageReportSubItemDefineInfo reportItemInfo = MainPageReportSubItemDefineInfo.fromDict(subDetail);
                        if (reportItemInfo != null) {
                            cellInfo.ReportItemInfo = reportItemInfo;
                            cachedSubItemInfo.Add(cellInfo.ReportItemId, reportItemInfo);
                        }
                    }
                }
            }
            #endregion
            #region 开始处理参数变量的默认值信息
            if (ret.DataSources == null) ret.DataSources = new List<MainPageDataSourceDefine>();
            foreach (MainPageReportColumnInfo colInfo in ret.ColumnsInfo)
            {
                foreach (MainPageReportCellItemInfo cellInfo in colInfo.CellItems)
                {
                    MainPageDataSourceDefine datasource = new MainPageDataSourceDefine();
                    datasource.DataSourceDefineId = cellInfo.ReportItemInfo.DataSource;
                    datasource.InstId = cellInfo.CellId;
                    if (cellInfo.Params != null && cellInfo.Params.Count > 0) {
                        foreach (MainPageReportCellItemParamInfo p in cellInfo.Params) {
                            RecalcDefaultValueScheme(p, userData);
                        }
                    }
                    datasource.Params = cellInfo.Params;
                    ret.DataSources.Add(datasource);
                }
            }
            #endregion
            return ret;
        }
        public void RecalcDefaultValueScheme(MainPageReportCellItemParamInfo  p, UserData userdata)
        {
            if (p.ValueScheme != null && p.ValueScheme.Length > 0) {
                string defaultValue = ReportFilterDefaultSchemeParseUtil.parseScheme(p.ValueScheme, userdata.UserId,this._reportEngineRepository);
                p.ParamValue = defaultValue;
            }
        }


    }
}
