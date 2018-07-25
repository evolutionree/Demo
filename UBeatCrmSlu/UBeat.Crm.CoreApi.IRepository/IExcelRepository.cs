using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Excels;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IExcelRepository
    {
        OperateResult AddExcel(AddExcelDomainModel data);
        OperateResult DeleteExcel(DeleteExcelDomainModel data);
        dynamic SelectExcels(PageParam pageParam, ExcelSelectDomainModel data);

        OperateResult SaveExcelTemplate(ExcelTemplateModel data);

        ExcelTemplateModel SelectExcelTemplate(string funcname);

        OperateResult ImportRowData(DbTransaction tran, ImportRowDomainModel data);
        List<Dictionary<string, object>> ExportData(ExportDataDomainModel data);

        /// <summary>
        /// 获取实体名称
        /// </summary>
        /// <param name="entityid"></param>
        /// <returns></returns>
        string GetEntityName(Guid entityid);
        /// <summary>
        /// 获取负责人id
        /// </summary>
        /// <param name="recManagerName"></param>
        /// <returns></returns>
        int GetRecManagerId(string recManagerName, out string errorMsg);
        /// <summary>
        /// 获取负责人ids
        /// </summary>
        /// <param name="recManagerName"></param>
        /// <returns></returns>
        List<int> GetRecManagerId(List<string> recManagerNames, out string errorMsg);
        /// <summary>
        /// 获取行政区域id
        /// </summary>
        /// <param name="regionName"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        string GetAreaRegionId(string regionName, out string errorMsg);
        /// <summary>
        /// 获取字典dataid
        /// </summary>
        /// <param name="dicTypeid"></param>
        /// <param name="dicValue"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        int GetDictionaryDataId(int dicTypeid, string dicValue, out string errorMsg);
        /// <summary>
        /// 获取多选的字典dataid
        /// </summary>
        /// <param name="dicTypeid"></param>
        /// <param name="dicValues"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        List<int> GetDictionaryDataId(int dicTypeid, List<string> dicValues, out string errorMsg);
        /// <summary>
        /// 获取数据源相对应的业务数据id
        /// </summary>
        /// <param name="ruleSql"></param>
        /// <param name="namevalue"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        IDictionary<string, object> GetDataSourceMapDataId(string ruleSql, string namevalue, out string errorMsg);
        /// <summary>
        /// 获取多选数据源相对应的业务数据id
        /// </summary>
        /// <param name="ruleSql"></param>
        /// <param name="values"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        List<IDictionary<string, object>> GetDataSourceMapDataId(string ruleSql, List<string> values, out string errorMsg);

        /// <summary>
        /// 获取产品id
        /// </summary>
        /// <param name="namepath"></param>
        /// <param name="errorMsg"></param>
        /// <param name="FieldFilters">过滤条件</param>
        /// <returns></returns>
        Guid GetProductId(string namepath, out string errorMsg, Dictionary<string, object> FieldFilters = null);
        /// <summary>
        /// 获取产品系列id
        /// </summary>
        /// <param name="serialPath"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        Guid GetProductSeriesId(string serialPath, out string errorMsg);

        /// <summary>
        /// 获取部门id
        /// </summary>
        /// <param name="serialPath"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        Guid GetDepartmentId(string serialPath, int userno, out string errorMsg);

        
        /// <summary>
        /// 获取销售阶段id
        /// </summary>
        /// <param name="serialPath"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        Guid GetSalesStageId(Guid salestagetypeid, string salestatname,  out string errorMsg);
    }
}
