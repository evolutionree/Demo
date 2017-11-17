using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.SalesStage;
using System.Data.Common;
namespace UBeat.Crm.CoreApi.IRepository
{
    public interface ISalesStageRepository
    {
        Dictionary<string, List<IDictionary<string, object>>> SalesStageQuery(SalesstageTypeMapper entity, int userNumber);

        OperateResult InsertSalesStage(SaveSalesStageMapper entity, int userNumber);

        OperateResult UpdateSalesStage(SaveSalesStageMapper entity, int userNumber);

        OperateResult DisabledSalesStage(DisabledSalesStageMapper entity, int userNumber);

        OperateResult OrderBySalesStage(OrderBySalesStageMapper entity, int userNumber);
        OperateResult OpentHighSetting(OpenHighSettingMapper entity, int userNumber);
        int GetHighSetting(string TypeId, int userNumber);

        Dictionary<string, List<IDictionary<string, object>>> SalesStageSettingQuery(SalesStageSetLstMapper entity, int userNumber);
        OperateResult InsertSalesStageEventSetting(AddSalesStageEventSetMapper entity, int userNumber);

        OperateResult UpdateSalesStageEventSetting(EditSalesStageEventSetMapper entity, int userNumber);

        OperateResult DisabledSalesStageEventSetting(DisabledSalesStageEventSetMapper entity, int userNumber);



        //    Dictionary<string, List<IDictionary<string, object>>> SalesStageOppInfoSettingQuery(SalesStageOppInfoSetLstMapper entity, int userNumber);
        Dictionary<string, List<IDictionary<string, object>>> SalesStageInfoFieldsQuery(SalesStageOppInfoFieldsMapper entity, int userNumber);
        OperateResult SaveSalesStageOppInfoSetting(SaveSalesStageOppInfoSetMapper entity, int userNumber);

        Dictionary<string, List<IDictionary<string, object>>> SalesStageRelEntityQuery(int userNumber);
        OperateResult InsertSalesStageDynEntitySetting(AddSalesStageDynEntitySetMapper entity, int userNumber);
        OperateResult DeleteSalesStageDynEntitySetting(DelSalesStageDynEntitySetMapper entity, int userNumber);


        Dictionary<string, List<IDictionary<string, object>>> SalesStageStepInfoQuery(SalesStageStepInfoMapper entity, int userNumber);
        OperateResult UpdateOpportunityStatus(UpdateOpportunityStatusMapper entity, int userNumber);
        OperateResult CheckAllowPushSalesStage(SaveSalesStageStepInfoMapper entity, int userNumber);
        OperateResult CheckAllowReturnSalesStage(ReturnSalesStageStepInfoMapper entity, int userNumber);
        OperateResult SaveSalesStageEvent(SaveSalesStageStepInfoMapper entity, int userNumber);
        OperateResult SaveSalesStageOppInfo(SaveSalesStageStepInfoMapper entity, int userNumber);
        OperateResult SaveSalesStageInfo(SaveSalesStageStepInfoMapper entity, int userNumber);
        OperateResult SaveSalesStageDynEntity(SaveDynEntityMapper entity, int userNumber);
        string ReturnSalesStageDynentityId(string recId, string salesStageId, int userNumber);

        OperateResult SaveLoseOrderInfo(LoseOrderMapper entity, int userNumber);
        OperateResult SaveWinOrderInfo(WinOrderMapper entity, int userNumber);

        Dictionary<string, List<IDictionary<string, object>>> LoseOrderInfoQuery(OrderInfoMapper entity, int userNumber);
        Dictionary<string, List<IDictionary<string, object>>> WinOrderInfoQuery(OrderInfoMapper entity, int userNumber);


        OperateResult SalesStageRestart(SalesStageRestartMapper entity, int userNumber);

        string ReturnEntityId(string typeId, int userNumber);
        void CheckSaleStageDynamicFormSetting(string typeId, int userNumber, DbTransaction transaction = null);
        /// <summary>
        /// 根据商机id获取所有商机阶段关联的简单实体id
        /// </summary>
        /// <param name="oppid"></param>
        /// <param name="userNumber"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        List<string> queryDynamicRecIdsFromOppId(string oppid, int userNumber, DbTransaction transaction = null);
        /// <summary>
        /// 根据销售阶段获取在此销售阶段下的商机的数量（未删除）
        /// </summary>
        /// <param name="stageid"></param>
        /// <param name="userNum"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        int checkHasOppInStageID(string stageid, int userNum, DbTransaction transaction = null);

    }
}
