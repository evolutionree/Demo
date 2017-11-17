using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.Order;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Utility;

namespace UBeat.Crm.CoreApi.Repository.Repository.Order
{
    public class OrderRepository : RepositoryBase, IOrderRepository
    {

        public Dictionary<string, List<IDictionary<string, object>>> OrderPaymentQuery(OrderPaymentListMapper order, int userNumber)
        {
            var procName =
                "SELECT crm_func_order_back_payment_list(@recid,@entityid,@pageindex,@pagesize,@userno)";
            var dataNames = new List<string> { "Info", "PlanData" };
            var param = new DynamicParameters();
            param.Add("recid", order.RecId);
            param.Add("entityid", order.EntityId);
            param.Add("pageindex", order.PageIndex);
            param.Add("pagesize", order.PageSize);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }

        public int UpdateOrderStatus(OrderStatusMapper order, int userNumber)
        {
            string sql = "update crm_sys_order set orderstatus=@status where recid=@recid ";

            var param = new DynamicParameters();
            param.Add("recid", order.RecId);
            param.Add("status", order.Status);
            param.Add("userno", userNumber);
            if (order.Status == 2)
            {
                //当订单确认时，审核状态改为通过recaudit=1
                sql = "update crm_sys_order set recaudits=1,orderstatus=@status where recid=@recid ";
            }
            else if (order.Status == 1)
            {
                //当订单取消时，审核状态改为通过recaudit=0
                sql = "update crm_sys_order set recaudits=0,orderstatus=@status where recid=@recid ";
            }
            return DataBaseHelper.ExecuteNonQuery(sql, param);
        }
    }
}
