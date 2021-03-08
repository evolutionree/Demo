using System;
using System.Collections.Generic;
using System.Data.Common;
using UBeat.Crm.CoreApi.GL.Model;

namespace UBeat.Crm.CoreApi.GL.Repository
{
    public interface ICustomerRepository
    {
        List<string> getAddCode(List<string> codeList);
        List<string> getModifyCode(List<string> codeList);
        List<string> getDeleteCode(List<string> codeList);
        List<string> getCrmLostCode(List<string> codeList);
        bool AddList(List<SaveCustomerMainView> dataList, int userId);
        bool ModifyList(List<SaveCustomerMainView> dataList, int userId);
        bool ModifyLostList(List<SaveCustomerMainView> dataList, int userId);
        bool ModifyFetchList(List<SaveCustomerMainView> dataList, int userId); 
		bool DeleteList(List<string> codeList, int userId);
        int UpdateCustomerSapCode(Guid recId, string sapCode, DbTransaction tran = null);
        #region 同步银行信息
        List<Dictionary<string, object>> GetCRMBankInfoList();
        #endregion
       
        void DeleteCustomerReceivable(String KUNNR);
    }
}
