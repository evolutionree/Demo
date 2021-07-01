using System;
using System.Collections.Generic;
using System.Data.Common;
using UBeat.Crm.CoreApi.ZGQY.Model;

namespace UBeat.Crm.CoreApi.ZGQY.Repository
{
    public interface IBaseDataRepository
    {
        bool HasDicTypeId(Int32 typeId);
        int GetNextDataId(Int32 dicTypeId);
        List<SaveDicData> GetDicDataByTypeId(Int32 typeId);
        bool AddDictionary(SaveDicData entity);
        bool UpdateDictionary(SaveDicData entity);
        string GetDicDataByTypeIdAndId(Int32 typeId, Int32 dataId);
        string GetDicDatavalByTypeIdAndId(Int32 typeId, Int32 dataId);
        string GetSapCodeByTypeIdAndId(Int32 typeId, string dataId);
        string GetDicDataByTypeIdAndExtField1(Int32 typeId, string extfield1);
        List<SaveDicData> GetDicData();

        int UpdateSynStatus(Guid entityId, Guid recId, int isSynchrosap, DbTransaction tran = null);
        int UpdateSynTipMsg(Guid entityId, Guid recId, string tipMsg, DbTransaction tran = null);
		int UpdateSynTipMsg2(Guid entityId, Guid recId, string tipMsg, DbTransaction tran = null);

		string GetRegionFullNameById(string regionId);
        Int32 GetRegionIdByName(string regionName);
        DateTime GetLastDateTime(Guid entityId);
        int UpdateLastDateTime(Guid entityId, DateTime dt);
        string GetProductCodeById(string recId);
        string getUserCodeById(string userId);
        SimpleUserInfo GetUserDataById(int userid);
        string getUserIdByCode(string workcode);
        List<DataSourceInfo> GetCustomerData();
        List<DataSourceInfo> GetOpporData();
        List<DataSourceInfo> GetContractData();
        List<DataSourceInfo> GetOrderData();
        List<DataSourceInfo> GetDeliData();
        List<SimpleUserInfo> GetUserData();
        List<SimpleProductnfo> GetProductData();
        List<DataSourceInfo> GetCategoryDataByEntityId(Guid entityId);
        List<DataSourceInfo> GetBankData();
        List<DataSourceInfo> GetSapPayment();
        string GetSapPaymentById(string recId);
        AutoSynSapModel GetEntityIdAndRecIdByCaseId(DbTransaction trans, Guid caseId, int userId);
        Int32 GetProductLine(string productcode);
        dynamic ExcuteActionExt(DbTransaction transaction, string funcname, object basicParamData, object preActionResult, object actionResult, int usernumber);
        dynamic DoCloseCRMOrderRow(DbTransaction transaction, string _entityid, string _recids, string _inputstatus, int usernumber);
        List<RegionCityClass> GetRegionCityData();
        List<DataSourceInfo> GetCustData();
        string GetCustomerCodeByDataSource(string ds);
    }
}
