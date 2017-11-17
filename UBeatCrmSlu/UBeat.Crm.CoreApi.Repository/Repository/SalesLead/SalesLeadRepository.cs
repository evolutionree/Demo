using Dapper;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.SalesLead;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Utility;

namespace UBeat.Crm.CoreApi.Repository.Repository.SalesLead
{
    public class SalesLeadRepository: ISalesLeadRepository
    {
        public OperateResult ChangeSalesLeadToCustomer(SalesLeadMapper entity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_sales_lead_to_customer(@salesleadid,@typeid, @contypeid, @userno)
            ";
            var param = new DynamicParameters();
            param.Add("salesleadid", entity.SalesLeadId);
            param.Add("typeid", entity.TypeId);
            param.Add("contypeid", entity.Con_TypeId);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }
    }
}
