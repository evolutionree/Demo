using System;
using System.Collections.Generic;
using System.Data.Common;
using UBeat.Crm.CoreApi.GL.Model;

namespace UBeat.Crm.CoreApi.GL.Repository
{
    public interface IOrderRepository
    {
        int UpdateOrderSapCode(Guid recId, string sapCode, Dictionary<string, string> lineDic, DbTransaction tran = null);
        Dictionary<string, object> GetContract(string contractno);
        Dictionary<string, object> GetSapOrderByCode(string orderCode);

        List<Guid> GetOrderListInitReturnByBooking();
        List<Guid> GetOrderListInitReturn();
        List<Guid> GetOrderListInitCrmReturn();
        List<Guid> GetOrderListInitReturnByContract();
        List<Dictionary<string, object>> GetOrderListInitOccupy();
        List<Dictionary<string, object>> GetOrderDetailByContractSortProdAndAccount(string code, DbTransaction tran = null);
        List<Dictionary<string, object>> GetOrderDetailByCodeSortProdAndAccount(string code, DbTransaction tran = null);
        List<Dictionary<string, object>> GetDeliDetailByAccount(Guid returnId, DbTransaction tran = null);
        bool DropHasReturnOrderDetailById(Guid returnId, DbTransaction tran = null);
        List<string> getAddCode(List<string> codeList);
        List<string> getModifyCode(List<string> codeList);
        List<string> getDeleteCode(List<string> codeList);

        bool DeleteList(List<string> codeList, int userId);

        int InsertOrderDetailSap(Guid orderId, Dictionary<string, string> lineDic, DbTransaction tran = null);
        int DeleteOrderDetailSap(List<Guid> recItemIds, DbTransaction tran = null);
        int UpdateOrderDetailStatusSap(Guid orderId, int isSynchrosap, DbTransaction tran = null);
        Dictionary<string, object> IsExistOrder(SoOrderDataModel sapOrder);
    }
}
