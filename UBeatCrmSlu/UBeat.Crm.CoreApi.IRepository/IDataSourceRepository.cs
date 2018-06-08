using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.EntityPro;
using System.Data.Common;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IDataSourceRepository
    {
        Dictionary<string, List<IDictionary<string, object>>> SelectDataSource(DataSourceListMapper dataSource, int userNumber);

        OperateResult InsertSaveDataSource(DataSourceMapper dataSource, int userNumber);

        OperateResult UpdateSaveDataSource(DataSourceMapper dataSource, int userNumber);
        OperateResult DataSourceDelete(DataSrcDeleteMapper dataSource, int userNumber);
        Dictionary<string, List<IDictionary<string, object>>> SelectDataSourceDetail(DataSourceDetailMapper dataSource, int userNumber);
        OperateResult InsertSaveDataSourceDetail(InsertDataSourceConfigMapper dataSource, int userNumber);

        OperateResult UpdateSaveDataSourceDetail(UpdateDataSourceConfigMapper dataSource, int userNumber);

        List<Dictionary<string, object>> SelectFieldDicType(int status,int userNumber, string dicTypeId = "");
        Dictionary<string, object> SelectFieldDicTypeDetail(string dicTypeId, int userNumber);
        Dictionary<string, object> SelectFieldConfig(string dicTypeId, int userNumber);
        DicTypeDataModel HasParentDicType(int dicTypeId);
        List<DictionaryDataModel> SelectFieldDicVaue(int dicTypeId, int userNumber);
        string QueryDicId();
        string QueryRecOrder();
        bool HasDicTypeName(string name);
        bool AddFieldDicType(DictionaryTypeMapper entity, int userNumber);
        bool UpdateFieldDicType(DictionaryTypeMapper entity, int userNumber);
        bool AddDictionary(SaveDictionaryMapper entity, int userNumber);
        bool UpdateDictionary(SaveDictionaryMapper entity, int userNumber);
        bool UpdateDicTypeOrder(List<DictionaryTypeMapper> data, int userNumber);
        bool OrderByDictionary(List<OrderByDictionaryMapper> entity, int userNumber);
        bool UpdateFieldDicTypeStatus(string[] ids, int status, int userNumber);
        OperateResult SaveFieldOptValue(DictionaryMapper option, int userNumber);
        OperateResult DisabledDicType(int dicTypeId, int userNumber);
        OperateResult DeleteFieldOptValue(string dicId, int userNumber);

        OperateResult OrderByFieldOptValue(string dicIds, int userNumber);
        dynamic GetDataSourceInfo(Guid dataSrcId, int userNumber);






        Dictionary<string, List<IDictionary<string, object>>> DynamicDataSrcQuery(DynamicDataSrcMapper entity, int userNumber);

        /// <summary>
        /// 获取实体关联的数据源
        /// </summary>
        /// <param name="entityid"></param>
        /// <returns></returns>
        List<DataSourceEntityModel> GetEntityDataSources(Guid entityid );

        bool checkDataSourceInUsed(Guid datasourceid, int userNumber, DbTransaction trans = null);
        Dictionary<string, object> GetDictTypeByName(string dictTypeName);
        int GetDictValueByName(int dictype,string dictValueName);

        Dictionary<string, object> GetDataSourceByName(DbTransaction tran, string datasourcename, int userId);
        IDictionary<string, object> DynamicDataSrcQueryDetail(string sourceId, Guid recId, int userId);
    }
}