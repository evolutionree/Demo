using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.DataSource
{
    public class DataSourceListModel
    {
        public string DatasourceName { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }
        public int RecStatus { get; set; }
    }
    public class DataSourceModel
    {
        public string DatasourceId { get; set; }
        public string DatasourceName { get; set; }
        public string DataSrcKey { get; set; }
        public int SrcType { get; set; }
        public string EntityId { get; set; }
        public int IsRelatePower { get; set; }
        public string Srcmark { get; set; }
        public string Rulesql { get; set; }
        public int RecStatus { get; set; }

        public int IsPro { get; set; }
    }
    public class DataSourceDetailModel
    {
        public string DatasourceId { get; set; }
    }

    public class DataSourceConfigModel
    {
        public string CellConfigId { get; set; }
        public string EntityId { get; set; }
        public int CssTypeId { get; set; }
        public int SourceId { get; set; }
        public string FieldKeys { get; set; }
        public string Fonts { get; set; }
        public string Colors { get; set; }

        public string RuleSql { get; set; }
    }

    public class InsertDataSourceConfigModel
    {
        public string DataSourceId { get; set; }
        public string EntityId { get; set; }
        public int CssTypeId { get; set; }
        public int ViewStyleId { get; set; }
        public string ColNames { get; set; }
        public string Fonts { get; set; }
        public string Colors { get; set; }
        public string RuleSql { get; set; }

    }


    public class UpdateDataSourceConfigModel
    {
        public string DataConfigId { get; set; }
        public string DataSourceId { get; set; }
        public int ViewStyleId { get; set; }
        public string ColNames { get; set; }
        public string Fonts { get; set; }
        public string Colors { get; set; }
        public string RuleSql { get; set; }

    }

    public class DataSrcDeleteModel
    {
        public Guid DataSrcId { get; set; }
    }

    public class DynamicDataSrcModel
    {
        public string SourceId { get; set; }

        public string KeyWord { get; set; }

        public List<Dictionary<string,object>> QueryData { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }

    }

    public class DynamicDataSrcQueryDataModel {

        public string FieldName { get; set; }
        public int IsLike { get; set; }
    }

    public class DictionaryTypeModel
    {
        public string DicTypeId { get; set; }

        public string DicTypeName { get; set; }
        public string DicRemark { get; set; }
        public int RecStatus { get; set; }
        public string FieldConfig { get; set; }
        public int? RelateDicTypeId { get; set; }
        public string RecOrder { get; set; }
        /// <summary>
        /// 0:使用自定义 1:使用全局
        /// </summary>
        public int IsConfig { get; set; } 
    }

    public class SrcDicTypeStatusList
    {
        public int Status { get; set; }
    }

    public class UpdateDicTypeParam
    {
        public string DicTypeIds { get; set; }

        public int RecStatus { get; set; }
    }

    public class DictionaryDisabledModel
    {
        public int DicTypeId { get; set; }
    }

    public class DictionaryModel
    {
        public string DicId { get; set; }

        public int DicTypeId { get; set; }

        public string DataId { get; set; }

        public string DataValue { get; set; }
    }

    public class SaveDictionaryModel
    {
        public Guid DicId { get; set; }
        public string DicTypeId { get; set; } 
        public int DataId { get; set; }
        public string DataVal { get; set; }
        public int RecOrder { get; set; }
        public int RecStatus { get; set; }
        public DateTime RecCreated { get; set; }
        public DateTime RecUpdated { get; set; }
        public int RecCreator { get; set; }
        public int RecUpdator { get; set; }
        public int? RelateDataId { get; set; }
        public string ExtField1 { get; set; }
        public string ExtField2 { get; set; }
        public string ExtField3 { get; set; }
        public string ExtField4 { get; set; }
        public string ExtField5 { get; set; }
    }
}
