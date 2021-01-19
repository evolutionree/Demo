using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.DynamicEntity;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IEntityTransferRepository
    {
        List<EntityTransferRuleInfo> queryRules(EntityTransferRuleQueryModel queryModel, int userNum, DbTransaction transaction = null);
        EntityTransferRuleInfo getById(string id, int userNum, DbTransaction transaction = null);
        string getCategoryName(string categoryId, int userNum, DbTransaction transaction = null);
        string getDictNameByDataId(int dataid, int typeid, int userNum, DbTransaction transaction = null);
        string getDictByVal(int typeid, string vals);
        Guid getCustomIdByName(string CustName, int userNum, string userName, DbTransaction transaction = null);
        Dictionary<string, object> getCustomerDataSourceByClue(string clueid, int userNum, string userName, DbTransaction transaction = null);
        bool CheckHasContact(string custid, string phone, DbTransaction transaction = null);
        string getProductName(string productid, int userNum);
        string getBaseCustomIdByName(string custName, int userNum, DbTransaction transaction = null);
    }
}
