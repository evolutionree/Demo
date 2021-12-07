using System;
using System.Collections.Generic;
using UBeat.Crm.CoreApi.DomainModel.Reports;

namespace UBeat.Crm.CoreApi.DomainModel.Reports
{
    /// <summary>
    /// 首页报表定义，与crm_sys_mainpagereport表定义相近
    /// </summary>
    public class MainPageReportInfo
    {
        /// <summary>
        /// 首页报表ID
        /// </summary>
        public string recid { get; set; }
        /// <summary>
        /// 首页报表名称
        /// </summary>
        public string recname { get; set; }
        /// <summary>
        /// 首页描述
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 首页报表类型，1=通用报表（用于全局和角色），2=个人报表，只用于个人
        /// </summary>
        public int RptType { get; set; }
        /// <summary>
        /// 报表整体宽度，>0表示实际宽度，以px为单位，
        /// 小于0为百分比宽度，实际宽度=可用宽度*百分比宽度*-1
        /// </summary>
        public Decimal Width { get; set; }
        /// <summary>
        /// 报表高度，
        /// 大于0表示实际高度，以Px为单位
        /// 小于0表示百分比高度，实际高度=可用高度*百分比高度*-1
        /// 等于0表示自由高度，不限制
        /// </summary>
        public Decimal Height { get; set; }
        /// <summary>
        /// 列定义，是经过转化成为对象之后的列定义
        /// </summary>
        public List<MainPageReportColumnInfo> ColumnsInfo { get; set; }
        /// <summary>
        /// 动态计算的Datasource，
        /// 规整所有cell的datasource
        /// </summary>
        public List<MainPageDataSourceDefine> DataSources { get; set; }

        public static MainPageReportInfo fromDict(IDictionary<string, object> dict)
        {
            MainPageReportInfo ret = new MainPageReportInfo();
            if (dict == null) return null;
            foreach (string key in dict.Keys)
            {
                string tmp = key.ToLower();
                if (tmp.Equals("id") || tmp.Equals("recid"))
                {
                    ret.recid = dict[key].ToString();
                }
                else if (tmp.Equals("name") || tmp.Equals("recname"))
                {
                    ret.recname = (string)dict[key];
                }
                else if (tmp.Equals("rpttype"))
                {
                    ret.RptType = int.Parse(dict[key].ToString());
                }
                else if (tmp.Equals("height"))
                {
                    Decimal tmpResult = new Decimal(0.0);
                    if (Decimal.TryParse(dict[key].ToString(), out tmpResult))
                    {
                        ret.Height = tmpResult;
                    }
                }
                else if (tmp.Equals("width"))
                {
                    Decimal tmpResult = new Decimal(0);
                    if (Decimal.TryParse(dict[key].ToString(), out tmpResult))
                    {
                        ret.Width = tmpResult;
                    }
                }
                else if (tmp.Equals("remark"))
                {
                }
                else if (tmp.Equals("columns".ToLower()))
                {
                    string tmpString = (string)dict[key];
                    if (tmpString != null && tmpString.Length > 0)
                    {
                        ret.ColumnsInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MainPageReportColumnInfo>>(tmpString);
                    }
                }
            }
            return ret;
        }

    }


    /// <summary>
    /// 临时性定义，主要为了加快客户端的显示速度，避免客户端做过多的递归查询
    /// </summary>
    public class MainPageDataSourceDefine {
        public string InstId { get; set; }
        public string DataSourceDefineId { get; set; }
        public List<MainPageReportCellItemParamInfo> Params { get; set; }
    }
    /// <summary>
    /// 首页报表的每一列的列定义
    /// </summary>
    public class MainPageReportColumnInfo
    {
        /// <summary>
        /// 列序号，从0开始，取值为0,1,2三种
        /// </summary>
        public int RowIndex { get; set; }
        /// <summary>
        /// 列宽，
        /// 大于0，表示固定列宽，以px为单位
        /// 小于0，表示百分比列宽，实际列宽=报表列宽*百分比列宽*-1
        /// 等于0，表示剩余列宽
        /// 
        /// </summary>
        public Decimal Width { get; set; }
        /// <summary>
        /// 列中元素定义
        /// </summary>
        public List<MainPageReportCellItemInfo> CellItems { get; set; }
    }
    /// <summary>
    /// 定义首页报表的每个单元元素
    /// </summary>
    public class MainPageReportCellItemInfo
    {
        /// <summary>
        /// 子项id，用于数据源返回时的instid
        /// </summary>
        public string CellId { get; set; }
        /// <summary>
        /// 首页报表子项定义的id，关联首页报表子项表
        /// </summary>
        public string ReportItemId { get; set; }
        /// <summary>
        /// 单元的高度定义
        /// 大于0表示实际高度，以px为单位
        /// 小于0表示百分比高度，实际高度=百分比高度*实际宽度*-1；
        /// </summary>
        public Decimal Height { get; set; }
        /// <summary>
        /// 子项元素的标题（名字）
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 界面上是否显示title的信息
        /// </summary>
        public bool IsDisplayTitle { get; set; }
        /// <summary>
        /// 参数的值或者值的取值方式。
        /// 参数是由子项定义中定义的，但是作为报表元素，必须指定每个参数的实际取值，或者实际的取值方式。
        /// 取值方式包括但不限于以下变量:
        /// #curyear#,#curyear#+X ，#curdate#,#curdate#+1等。
        /// </summary>
        public List<MainPageReportCellItemParamInfo> Params { get; set; }
        /// <summary>
        /// 经过reportitemid获取的reportitem的定义，以便返回给前端展现。
        /// </summary>
        public MainPageReportSubItemDefineInfo ReportItemInfo { get; set; }
		/// <summary>
		/// 备注信息
		/// </summary>
		public String Remark { get; set; }

	}
    /// <summary>
    /// 首页报表元素的参数值以及值得定义
    /// </summary>
    public class MainPageReportCellItemParamInfo
    {
        public string ParamName { get; set; }
        public object ParamValue { get; set; }
        public string ValueScheme { get; set; }
        public MainPageParamType ParamType { get; set; }

    }

    /// <summary>
    /// 首页报表子项的定义
    /// </summary>
    public class MainPageReportSubItemDefineInfo
    {
        /// <summary>
        /// 首页报表子项定义id
        /// </summary>
        public string RecId { get; set; }
        /// <summary>
        /// 首页子项定义的名称
        /// </summary>
        public string RecName { get; set; }
        /// <summary>
        /// 取数数据源ID，可能为空
        /// </summary>
        public string DataSource { get; set; }
        /// <summary>
        /// 子项类型，
        /// 1=IFrame类
        /// 2=专用组件类
        /// 3=通用组件类
        /// </summary>
        public MainPageSubItemComponentType ComponentType { get; set; }
        /// <summary>
        /// 需要定义的参数
        /// </summary>
        public List<MainPageSubItemParamDefine> ParamSets { get; set; }

        /// <summary>
        /// 当子项定义为IFrame类时，IFrame所需要的信息
        /// </summary>
        public MainPageSubItemIFrameDefine IFrameInfo { get; set; }

        /// <summary>
        /// 如果是专用组件，专用组件定义
        /// </summary>
        public MainPageSubItemSpecComponentDefine SpecComponentInfo { get; set; }
        /// <summary>
        /// 内定自定义组件的定义
        /// </summary>
        public MainPageSubItemCommonComponentDefine CommonComponentInfo { get; set; }
        public static MainPageReportSubItemDefineInfo fromDict(IDictionary<string, object> dict) {
            if (dict == null || dict.Count == 0) return null;
            MainPageReportSubItemDefineInfo ret = new MainPageReportSubItemDefineInfo();
            foreach (string key in dict.Keys)
            {
                string tmp = key.ToLower();
                if (tmp.Equals("id") || tmp.Equals("recid"))
                {
                    ret.RecId = dict[key].ToString();
                }
                else if (tmp.Equals("name") || tmp.Equals("recname"))
                {
                    ret.RecName = (string)dict[key];
                }
                else if (tmp.Equals("componenttype") && dict[key] != null)
                {
                    ret.ComponentType = (MainPageSubItemComponentType)int.Parse(dict[key].ToString());
                }

                else if (tmp.Equals("datasource") && dict[key] != null )
                {
                    if (dict[key].GetType() == typeof(string)) {
                        string tmpstr = (string)dict[key];
                        if (tmpstr.Length > 0) {
                            Dictionary<string, object> tmpDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(tmpstr);
                            if (tmpDict != null && tmpDict.ContainsKey("id") && tmpDict["id"] != null) {
                                ret.DataSource = (string)tmpDict["id"];
                            }
                        }
                    }
                }
                else if (tmp.Equals("paramsets") && dict[key] != null)
                {
                    Object obj = dict[key];
                    string jsonString = "";
                    if (obj.GetType() == typeof(string))
                    {
                        jsonString = (string)dict[key];
                    }
                    else {
                        jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                    }
                    if (jsonString != null && jsonString.Length > 0) {
                        ret.ParamSets = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MainPageSubItemParamDefine>>(jsonString);
                    }
                }
                else if (tmp.Equals("iframeinfo") && dict[key] != null)
                {
                    Object obj = dict[key];
                    if (obj != null)
                    {
                        string jsonString = "";
                        if (obj.GetType() == typeof(string))
                        {
                            jsonString = (string)dict[key];
                        }
                        else
                        {
                            jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                        }
                        if (jsonString != null && jsonString.Length > 0)
                        {
                            ret.IFrameInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<MainPageSubItemIFrameDefine>(jsonString);
                        }
                    }
                    
                }
                else if (tmp.Equals("speccomponentinfo") && dict[key] != null)
                {
                    Object obj = dict[key];
                    if (obj != null) {
                        string jsonString = "";
                        if (obj.GetType() == typeof(string))
                        {
                            jsonString = (string)dict[key];
                        }
                        else
                        {
                            jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                        }
                        if (jsonString != null && jsonString.Length > 0)
                        {
                            ret.SpecComponentInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<MainPageSubItemSpecComponentDefine>(jsonString);
                        }
                    }
                   
                }
                else if (tmp.Equals("commoncomponentinfo") && dict[key] != null)
                {
                    Object obj = dict[key];
                    if (obj != null)
                    {
                        string jsonString = "";
                        if (obj.GetType() == typeof(string))
                        {
                            jsonString = (string)dict[key];
                        }
                        else
                        {
                            jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                        }
                        if (jsonString != null && jsonString.Length > 0)
                        {
                            ret.CommonComponentInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<MainPageSubItemCommonComponentDefine>(jsonString);
                        }
                    }
                    
                }
            }
            return ret;
        }
    }

    /// <summary>
    /// 首页报表子项类型定义
    /// </summary>
    public enum MainPageSubItemComponentType
    {
        IFrameType = 1,
        SpecComponent = 2,
        CommonComponent = 3
    }
    /// <summary>
    /// 报表子项参数定义
    /// </summary>
    public class MainPageSubItemParamDefine
    {
        /// <summary>
        /// 参数名称
        /// </summary>
        public string ParamName { get; set; }
        /// <summary>
        /// 参数的中文说明
        /// </summary>
        public string ParamRemark { get; set; }
        /// <summary>
        /// 参数类型
        /// </summary>
        public MainPageParamType ParamType { get; set; }
    }
    /// <summary>
    /// 首页报表子项参数类型定义
    /// </summary>
    public enum MainPageParamType
    {
        ParamType_Number = 1,
        ParamType_Text = 2
    }

    /// <summary>
    /// 首页报表子项中的IFrame模式的定义属性
    /// </summary>
    public class MainPageSubItemIFrameDefine
    {
        public string Url { get; set; }
    }

    /// <summary>
    /// 首页报表子项中关于专用组件的定义
    /// </summary>
    public class MainPageSubItemSpecComponentDefine
    {
        /// <summary>
        /// 用于编辑过滤条件的react组件
        /// </summary>
        public string ParamEditComponent { get; set; }
        /// <summary>
        /// 用于显示子项内容的React组件
        /// </summary>
        public string DisplayComponent { get; set; }

    }

    /// <summary>
    /// 通用的首页报表子项的通用组件的定义
    /// </summary>
    public class MainPageSubItemCommonComponentDefine
    {
        /// <summary>
        /// 子项类型
        /// </summary>
        public MainPageSubItemCommonComponentType ElemType { get; set; }
        /// <summary>
        /// 折线图和饼图的定义
        /// </summary>
        public MainPageSubItemBarAndLineInfo BarAndLineInfo { get; set; }
        /// <summary>
        /// 散点图定义
        /// </summary>
        public MainPageSubItemScatterInfo ScatterInfo { get; set; }
         
        /// <summary>
        /// 漏斗信息
        /// </summary>
        public MainPageSubItemFunnelDefine FunelInfo { get; set; }
        /// <summary>
        /// 仪表盘的定义数据
        /// </summary>
        public GaugeDefineInfo GaugeInfo { get; set; }
        /// <summary>
        /// 简单列表定义
        /// </summary>
        public MainPageSubItemSimpleTableDefine SimleTableInfo { get; set; }

        /// <summary>
        /// H5元素块定义
        /// </summary>
        public MainPageSubItemDivElemDefine DivElemInfo { get; set; }
        public MainPageSubItemListDefine SimpleListInfo { get; set; }

    }
    public class MainPageSubItemListDefine {
        public string StypeName { get; set; }
        public string Item1ValueScheme { get; set; }
        public string Item2ValueScheme { get; set; }
        public string Item3ValueScheme { get; set; }
        public string Item4ValueScheme { get; set; }
        public string Item5ValueScheme { get; set; }
        public string Item6ValueScheme { get; set; }
        public string Item7ValueScheme { get; set; }
        public string Item8ValueScheme { get; set; }


    }
    /// <summary>
    /// 漏斗图定义
    /// </summary>
    public class MainPageSubItemFunnelDefine {
        /// <summary>
        /// 取值的字段
        /// </summary>
        public string ValueFieldName { get; set; }
        /// <summary>
        /// 阶段的lable取值字段
        /// </summary>
        public string StageFieldName { get; set; }
        /// <summary>
        /// 鼠标停留显示，支持##模式
        /// </summary>
        public string DetailFormat { get; set; }

    }
    /// <summary>
    /// 散点图定义
    /// </summary>
    public class MainPageSubItemScatterInfo
    {
        /// <summary>
        /// X轴的取值字段 
        /// </summary>
        public string XFieldName { get; set; }
        /// <summary>
        /// X轴的名称，支持##配置
        /// </summary>
        public string XLabelFieldName { get; set; }
        /// <summary>
        /// X轴的显示样式，只支持{value}年，
        /// 注意:由于"{value}"会对json解析出现异常，如果仅仅是"{value}"，就保留空白即可。
        /// </summary>
        public string XFormat { get; set; }
        /// <summary>
        /// 图形中鼠标停留的显示样式，支持##模式
        /// </summary>
        public string DetailFormat { get; set; }
        /// <summary>
        /// Y轴的定义，支持多Y轴，主要为了适配多系列中每个系列的范围差距很远的情况。
        /// </summary>
        public List<YSerialLabelInfo> YFieldList { get; set; }
        /// <summary>
        /// 系列的定义
        /// </summary>
        public List<CommonSeriesInfo> Series { get; set; }

    }
    /// <summary>
    /// 折线图和饼图的定义
    /// </summary>
    public class MainPageSubItemBarAndLineInfo
    {
        /// <summary>
        /// X轴的取值字段 
        /// </summary>
        public string XFieldName { get; set; }
        /// <summary>
        /// X轴的名称，支持##配置
        /// </summary>
        public string XLabelFieldName { get; set; }
        /// <summary>
        /// X轴的显示样式，只支持{value}年，
        /// 注意:由于"{value}"会对json解析出现异常，如果仅仅是"{value}"，就保留空白即可。
        /// </summary>
        public string XFormat { get; set; }
        /// <summary>
        /// 图形中鼠标停留的显示样式，支持##模式
        /// </summary>
        public string DetailFormat { get; set; }
        /// <summary>
        /// Y轴的定义，支持多Y轴，主要为了适配多系列中每个系列的范围差距很远的情况。
        /// </summary>
        public List<YSerialLabelInfo> YFieldList { get; set; }
        /// <summary>
        /// 系列的定义
        /// </summary>
        public List<CommonSeriesInfo> Series { get; set; }

    }

    /// <summary>
    /// H5元素块的定义
    /// </summary>
    public class MainPageSubItemDivElemDefine
    {
        /// <summary>
        /// 样式，系统预制了部分样式，由前端提供
        /// </summary>
        public string StyleType { get; set; }
        public string Item1ValueScheme { get; set; }
        public string Item1LabelScheme { get; set; }
        public string Item2ValueScheme { get; set; }
        public string Item2LabelScheme { get; set; }
        public string Item3ValueScheme { get; set; }
        public string Item3LabelScheme { get; set; }
        public string Item4ValueScheme { get; set; }
        public string Item4LabelScheme { get; set; }
    }

    /// <summary>
    /// 简单把表格定义
    /// </summary>
    public class MainPageSubItemSimpleTableDefine
    {
    }
    public enum MainPageSubItemCommonComponentType
    {
        LineAndBar = 1,//折线图和柱状图
        Scatter = 2,//散点图
        Funnel = 3,//漏斗图
        DashBoard = 4,//仪表盘
        SimpleTable = 5,//简单表格
        DivElem = 6,//H5块元素
        SimpleList = 7//简单列表
    }
}
