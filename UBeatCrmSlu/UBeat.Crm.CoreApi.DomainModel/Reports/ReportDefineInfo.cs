﻿using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Reports
{
    public class ReportDefineInfo
    {
        public Guid Id { get; set; }//报表定义ID
        public string Name { get; set; }//报表名称
        public int RptType { get; set; }//报表类型,1=Mobile,2=WEB, 另外一个含义是每一行有多少个占位符
        public List<ReportDataSourceInfo> datasources = new List<ReportDataSourceInfo>();//引用的数据源，及定义数据源的形参和实参
        public List<ReportEventInfo> events = new List<ReportEventInfo>();//报表事件，暂时没有意义
        public List<ReportComponentInfo> components = new List<ReportComponentInfo>();//报表控件,包括：1=过滤控件，2=图控件，3=表控件
        /// <summary>
        /// 发布路径，仅供参考，实际按crm_sys_reportmenu;
        /// </summary>
        public string FolderPath { get; set; }
        /// <summary>
        /// funcid
        /// </summary>
        public Guid FuncID { get; set; }

        /***
         * 名称：从数据库查询出来的结果转换到ReportDefineInfo
         * 主要是因为datasources\events\components这三个字段存在数据库是json字符串，而返回给前端是对象
         * */
        public static ReportDefineInfo fromDict(IDictionary<string, object> dict) {
            ReportDefineInfo ret = new ReportDefineInfo();
            if (dict == null) return null;
            foreach (string key in dict.Keys) {
                string tmp = key.ToLower();
                if (tmp.Equals("id") || tmp.Equals("recid"))
                {
                    if (dict[key] is Guid)
                    {
                        ret.Id = (Guid)dict[key];
                    }
                    else {
                        ret.Id = Guid.Parse(dict[key].ToString());
                    }
                }
                else if (tmp.Equals("name") || tmp.Equals("recname"))
                {
                    ret.Name = (string)dict[key];
                }
                else if (tmp.Equals("rpttype"))
                {
                    ret.RptType = int.Parse(dict[key].ToString());
                }
                else if (tmp.Equals("datasources"))
                {
                    string tmpString = (string)dict[key];
                    if (tmpString != null && tmpString.Length > 0)
                    {
                        ret.datasources = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ReportDataSourceInfo>>(tmpString);
                    }
                }
                else if (tmp.Equals("RptEvents".ToLower()))
                {
                    string tmpString = (string)dict[key];
                    if (tmpString != null && tmpString.Length > 0)
                    {
                        ret.events = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ReportEventInfo>>(tmpString);
                    }
                }
                else if (tmp.Equals("Components".ToLower())) {
                    string tmpString = (string)dict[key];
                    if (tmpString != null && tmpString.Length > 0)
                    {
                        ret.components = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ReportComponentInfo>>(tmpString);
                    }
                }
            }
            return ret;
        }

    }
    public class ReportDataSourceInfo {
        public string InstId { get; set; }
        public Guid DataSourceDefineID { get; set; }
        public List<Dictionary<string, string>> Parameters { get; set; }

    }
    public class ReportEventInfo {
    }
    public enum ReportComponentInfo_CtrlType {
        Chart = 1,//图，现在包括散点图，柱状图，折线图，但不包括地图
        WebTable = 2,//web部分的表格控件
        FilterCtrl = 3,//过滤条件控件
        Series = 4,//系列控件
        DivCtrl = 7,//文本展示区（用于展示文本的）
        MapCtrl = 6,// 地图控件
        MobileTable = 8,//手机上的表格控件
        UKBarGraph = 9//移动端U客柱状图
    }
    public enum ReportFilter_CtrlType {
        Text = 1,
        Commonbox = 2,
        MultiChoose = 3,
        DateCtl = 4,
        Series = 5
    }

    public class ReportComponentInfo {
        public int Index { get; set; }
        /// <summary>
        /// 控件类型，详见ReportComponentCtrlType
        /// </summary>
        public int CtrlType { get; set; }
        public int Width { get; set; }
        public Decimal Height { get; set; }
        public string DataSourceName { get; set; }
        public Dictionary<string, object> TitleInfo { get; set; }
        public CommonComponentInfo CommonExtInfo { get; set; }
        public TableComponentInfo TableExtInfo { get; set; }
        public FilterPanelInfo FilterExtInfo { get; set; }
        public DivLabelExtInfo DivCtrlExtInfo { get; set; }//文本显示控件
        public List<ComponentEventInfo> Events { get; set; }
        public MapComponentInfo MapExtInfo { get; set; }
        public ComponentTitleInfo CompSummaryInfo { get; set; }
        public MobileTableDefineInfo MobileTableInfo { get; set; }
        public string DataSourceForSummary { get; set; }
        public MobileUKBarGraphDefineInfo UKBarGraphInfo { get; set; }
        /// <summary>
        /// 是否跟上一块链接，主要是用于手机端报表
        /// </summary>
        public bool IsLinkToUp { get; set; }
        /// <summary>
        /// 是否跟下一块链接，主要用于手机端报表
        /// </summary>
        public bool IsLinkToDown { get; set; }
        public ReportComponentInfo() {
            IsLinkToDown = false;
            IsLinkToUp = false;
        }
    }
    /// <summary>
    /// 移动端报表，U客柱状图定义
    /// </summary>
    public class MobileUKBarGraphDefineInfo {
        public string XFieldName { get; set; }
        public string ValueFieldName { get; set; }
        public string ValueLabelScheme { get;set; }
    }
    /// <summary>
    /// 移动端报表列表控件定义
    /// </summary>
    public class MobileTableDefineInfo {
        public string EntityId { get; set; }
        public string MainTitleFieldName { get; set; }
        public string SubTitleFieldName { get; set; }
        public List<MobileTableFieldDefineInfo> DetailColumns { get; set; }
    }
    public class MobileTableFieldDefineInfo {
        public string FieldLabel { get; set; }
        public string FieldName { get; set; }
    }
    public class ComponentTitleInfo {
        public string TitleScheme { get; set; }
        public List<string> ParamsKey { get; set; }
    }
    public class MapComponentInfo {
        public List<MapItemFilterExtInfo> FilterItems { get; set; }
        public string FormatForMap { get; set; }
        public string FormatForChart { get; set; }
    }

    public class MapItemFilterExtInfo {
        public int Index { get; set; }
        public bool IsRegion { get; set; }
        public string CtrlName { get; set; }
        public string ParamName { get; set; }
        public string DataSetName { get; set; }
        public string Title { get; set; }

    }
    public class DivLabelExtInfo {
        public string TextLabelScheme { get; set; }
        public string TextAlign { get; set; }
        public List<string> Params { get; set; }
        public DivLabelExtInfo() {
            TextAlign = "center";
        }
    }

    public class ComponentEventInfo {
        public string EventName { get; set; }
        public List<ComponentEventActionInfo> ActionList { get; set; }
    }
    public class ComponentEventActionInfo {
        public int ActionType { get; set; }//1=打开页面,2=更改变量,
        public List<ComponentEventActionChangeParamSetting> ChangeParam { get; set; }
        public string LinkScheme { get; set; }
        public int TargetType { get; set; }


    }
    public class ComponentEventActionChangeParamSetting {
        public int FilterCtrlIndex { get; set; }
        public string ParameterName { get; set; }
        public string ParameterValue { get; set; }
    }

    public enum ReportCommonComponent_ChatType {
        /// <summary>
        /// 折线图和柱状图
        /// </summary>
        Line = 1,
        /// <summary>
        /// 散点图
        /// </summary>
        Scatter = 2,
        /// <summary>
        /// 仪表盘
        /// </summary>
        Gauge = 3
    }
    public class CommonComponentInfo {
        public string XFieldName { get; set; }
        public string XFieldLableName { get; set; }
        public string DetailFormat { get; set; }
        public string XFormat { get; set; }//X轴显示格式，仅需要支持 “{value}年”这种定义
        public Dictionary<string, object> XFieldExtInfo { get; set; }
        public List<YSerialLabelInfo> YFieldList { get; set; }
        public List<CommonSeriesInfo> Series { get; set; }

        /// <summary>
        /// 图表类型
        /// 1=柱状图和折线图
        /// 2=散点图
        /// 3=仪表盘
        /// </summary>
        public int ChartType { get; set; }
        /// <summary>
        /// 仪表盘定义
        /// </summary>
        public GaugeDefineInfo GaugeInfo { get; set; }


    }
    /// <summary>
    /// 仪表盘定义
    /// </summary>
    public class GaugeDefineInfo {
        /// <summary>
        /// 仪表盘的最小值
        /// </summary>
        public int MinValue { get; set; }
        /// <summary>
        /// 仪表盘的最大值
        /// </summary>
        public int MaxValue { get; set; }
        /// <summary>
        /// 拆分的刻度数量
        /// </summary>
        public int SpliteNumber { get; set; }
        /// <summary>
        /// 仪表盘默认指针位置
        /// </summary>
        public int DefaultValue { get; set; }
        /// <summary>
        /// 仪表盘默认取值字段名称
        /// </summary>
        public String ValueFieldName { get; set; }
        /// <summary>
        /// 区域及区域颜色设定
        /// </summary>
        public List<GaugeAreaDefineInfo> AreaInfo { get; set; }
    }
    public class  GaugeAreaDefineInfo {
        /// <summary>
        /// 排序码
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// 累积面积比例（0.xxx)
        /// </summary>
        public Decimal AreaRate { get; set; }
        //本区域显示的颜色
        public string AreaColor { get; set; }
    }
    public class YSerialLabelInfo {
        public int Index { get; set; }
        public string Name { get; set; }
        public string FormatStr { get; set; }//Y轴显示格式，仅需要支持 “{value}年”这种定义 
        public Decimal MaxValue { get; set; }
        public Decimal MinValue { get; set; }
        
    }
    public class CommonSeriesInfo {
        public string FieldName { get; set; }
        public string SeriesName { get; set; }
        public string CharType { get; set; }
        public int YLabel { get; set; }
        public List<SerieColorSetting> ColorSettings { get; set; }
        public string Stack { get; set; }
        public string BaseColor { get; set; }

    }
    public class SerieColorSetting {
        public int Index { get; set; }
        public string LeftItem { get; set; }
        public string Operator { get; set; }
        public string RightItem { get; set; }
        public string Result { get; set; }
    }
    public class TableComponentInfo {
        public int FixedX { get; set; }
        public int FixedY { get; set; }
        /// <summary>
        /// 是否显示导出按钮
        /// </summary>
        public int ShowExport { get; set; }
        public List<TableColumnInfo> Columns { get; set; }
    }
    public class TableColumnInfo {
        public string Title { get; set; }
        public string FieldName { get; set; }
        public string LinkScheme { get; set; }
        public int TargetType { get; set; }
        public bool CanPaged { get; set; }
        public int CanSorted { get; set; }
        public int SortedType { get; set; } //0=没有排序，1=已经asc，-1已经desc
        public int Width { get; set; }
        public int ControlType { get; set; }//控件类型
        public string FormatStr { get; set; }//显示方式
        public bool IsDataSourceMulti { get; set; }//是否数据源多选
        public int IsColumnGroup { get; set; }//是否表头分组
        public List<TableColumnInfo> SubColumns { get; set; }//子Columns

    }
    /***
     * 过滤条件列表cd 
     * */
    public class FilterPanelInfo {
        public List<FilterControlInfo> Ctrls { get; set; }
    }
    public class FilterControlInfo {
        public int Index { get; set; }
        public string LabelText { get; set; }
        public int CtrlType { get; set; } //1=文本，2=下拉(数据来源)，3=选择（选择，是否多选）,4系列，5时间
        public string ParameterName { get; set; }
        public FilterComboDataInfo ComboData { get; set; }
        public MultiChooseDataInfo MultiChooseData { get; set; }
        public FilterSeriesCtrlInfo SeriesSetting { get; set; }
        public string TextDefaultValue { get; set;  }
        public string TextDefaultValue_Name { get; set; }
        public string TextDefaultValueScheme { get; set; }
        public string TextDefaultValue_NameScheme { get; set; }
        public string DateDefaultValue { get; set; }
        public string DateDefaultValueScheme { get; set; }
        public int Width { get; set; }
        public FilterControlInfo() {
            Width = 50;
            CtrlType = 1;
        }
    }

    public class FilterSeriesCtrlInfo {
        public bool CanAdd { get; set; }
        public List<FilterSerieDefineInfo> DefaultValues { get; set; }

    }
    public class FilterSerieDefineInfo {
        public int Index { get; set; }
        public string SerieName { get; set; }
        public string SerieColor { get; set; }
        public string ShadowColor { get; set; }
        public string SerieColor2 { get; set; }
        public Decimal SerieFrom { get; set; }
        public Decimal SerieTo { get; set; }
        public int SerieStatus { get; set; }
        public string YFieldName { get; set; }
        public string SizeFieldName { get; set; }
        public string XFieldName { get; set; }

    }


    public class FilterComboDataInfo {
        public int DataSourceType { get; set; } //1=自定义,2=数据源
        public List<Dictionary<string, string>> DataList { get; set; }
        public ReportDataSourceInfo DataSource { get; set; }
        public int DefaultIndex { get; set; }
        public string DefaultValue { get; set; }
        public string DefaultValue_Name { get; set; }
        /// <summary>
        /// 支持#CurYear#（int）,#CurMonth#(int),#CurDate#(date)
        /// #CurDate+x#,#FirstDayOfThisMonth#,#LastDayOfThisMonth#
        /// #CurUserId#(guid),#MyDept#(guid)，
        /// </summary>
        public string DefaultValueScheme { get; set; }
        public string DefaultValue_NameScheme { get; set; }
         
    }
    public class MultiChooseDataInfo {
        public bool IsMultiSelect { get; set; }
        public int ChooseType { get; set; }//1=部门,2=人员，3=产品 4=地区
        public List<Dictionary<string, object>> DefaultValues { get; set; }
        public string DefaultValueScheme { get; set; }
        public int RelateCtrlIndex { get; set; }
        public string RuleID { get; set; }
    }
}
