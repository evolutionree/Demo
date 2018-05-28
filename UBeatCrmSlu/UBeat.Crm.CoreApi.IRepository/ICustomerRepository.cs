using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Customer;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface ICustomerRepository
    {
        

        dynamic QueryCustRelate(DbTransaction tran, Guid custId);

        PageDataInfo<MergeCustEntity> GetMergeCustomerList(DbTransaction tran, string wheresql,string searchkey, DbParameter[] sqlParameters, int pageIndex, int pageSize);

        bool IsWorkFlowCustomer( List<Guid> custid, int usernumber);

        List<Custcommon_Customer_Model> IsCustomerExist(Dictionary<string, object> fieldData);


        bool UpdateCustomer(DbTransaction tran, Guid custid, Dictionary<string, object> updateFileds, int usernumber);

        /// <summary>
        /// 更新客户关联实体的关联数据
        /// </summary>
        /// <param name="tran"></param>
        /// <param name="custid"></param>
        /// <param name="oldCustid"></param>
        /// <param name="tableName"></param>
        /// <param name="usernumber"></param>
        /// <returns></returns>
        bool UpdateCustomerRelateDynamicEntity(DbTransaction tran, Guid custid, List<Guid> oldCustid, string tableName, int usernumber);

        List<CustRelFieldModel> GetCustomerRelateField(DbTransaction tran, string tableName, string updateFiledName,Guid custid);

        //更新客户关联控件的字段值
        bool UpdateCustomerRelateField(DbTransaction tran, string tableName,string updateFiledName,object filedValue, Guid recid,  int usernumber);


        bool DeleteBeMergeCustomer(DbTransaction tran, List<Guid> oldCustid, int usernumber);

        string getSaleClueIdFromCustomer(DbTransaction transaction, string recid, int userNum);
        string rewriteSaleClue(DbTransaction transaction, string recid, int userNum);
        bool checkNeedAddContact(DbTransaction transaction, string saleclueid, string custid);


        /// <summary>
        ///  根据客户id获取客户基础资料id
        ///  
        /// </summary>
        /// <param name="tran"></param>
        /// <param name="custid"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        string getCommonIdByCustId(DbTransaction tran, string custid, int userId);

    }
}
