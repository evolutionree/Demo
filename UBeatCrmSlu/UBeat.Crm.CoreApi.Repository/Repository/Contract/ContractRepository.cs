using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Contract;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Utility;

namespace UBeat.Crm.CoreApi.Repository.Repository.Contract
{
    public class ContractRepository : IContractRepository
    {

        /// <summary>
        /// 添加回款计划
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OperateResult AddPaymentPlan(PaymentPlanAdd data, int userNumber)
        {
            var executeSql = @"SELECT * FROM crm_func_payment_plan_insert(@plantime,@planmoney,@parentId,@remark,@refundType,@userno)";
            var args = new
            {
                PlanTime = data.PlanTime.ToString(),
                PlanMoney = data.PlanMoney.ToString(),
                ParentId = data.ParentId.ToString(),
                Remark = data.Remark,
                RefundType = data.RefundType,
                UserNo = userNumber
            };

            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }


        /// <summary>
        /// 编辑回款计划
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OperateResult EditPaymentPlan(PaymentPlanEdit data, int userNumber)
        {
            var executeSql = @"SELECT * FROM crm_func_payment_plan_update(@planid,@plantime,@planmoney,@remark,@userno)";
            var args = new
            {
                PlanId = data.PlanId.ToString(),
                PlanTime = data.PlanTime.ToString(),
                PlanMoney = data.PlanMoney.ToString(),
                Remark = data.Remark,
                UserNo = userNumber
            };

            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }


        /// <summary>
        /// 删除回款计划
        /// </summary>
        /// <param name="planId"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OperateResult DeletePaymentPlan(Guid planId, int userNumber)
        {
            var executeSql = @"SELECT * FROM crm_func_payment_plan_delete(@planid,@userno)";
            var args = new
            {
                PlanId = planId.ToString(),
                UserNo = userNumber
            };

            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }

        /// <summary>
        /// 获取回款计划
        /// </summary>
        /// <param name="userNumbe"></param>
        /// <returns></returns>
        public dynamic GetPaymentPlans(int userNumbe)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 添加回款记录
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OperateResult AddPayment(PaymentAdd data, int userNumber)
        {
            var executeSql = @"SELECT * FROM crm_func_payment_insert(@paytime,@paymoney,@payname,@remark,@BizType,@ParentId,@userno)";
            var args = new
            {
                PayTime = data.PayTime.ToString(),
                PayMoney = data.PayMoney.ToString(),
                PayName = data.PayName,
                Remark = data.Remark,
                BizType = data.BizType,
                ParentId = data.ParentId.ToString(),
                UserNo = userNumber
            };
            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }

        /// <summary>
        /// 编辑回款记录
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OperateResult EditPayment(PaymentEdit data, int userNumber)
        {
            var executeSql = @"SELECT * FROM crm_func_payment_update(@paymentid,@paytime,@paymoney,@payname,@remark,@userno)";
            var args = new
            {
                paymentid = data.PaymentId.ToString(),
                PayTime = data.PayTime.ToString(),
                PayMoney = data.PayMoney.ToString(),
                PayName = data.PayName,
                Remark = data.Remark,
                UserNo = userNumber
            };

            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }

        /// <summary>
        /// 删除回款记录
        /// </summary>
        /// <param name="paymentId"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OperateResult DeletePayment(Guid paymentId, int userNumber)
        {
            var executeSql = @"SELECT * FROM crm_func_payment_delete(@paymentid,@userno)";
            var args = new
            {
                paymentid = paymentId.ToString(),
                UserNo = userNumber
            };

            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }



        /// <summary>
        /// 获取回款记录和回款计划
        /// </summary>
        /// <param name="userNumbe"></param>
        /// <returns></returns>
        public dynamic GetPaymentsAndPlan(Guid parentId, PageParam page, int userNumber)
        {
            var executeSql = @"SELECT * FROM crm_func_payment_and_plan_select(@parentId,@userno,@pageindex,@pagesize)";
            var args = new
            {
                parentId = parentId.ToString(),
                UserNo = userNumber,
                PageIndex = page.PageIndex,
                PageSize = page.PageSize
            };

            var dataNames = new List<string> { "datacursor", "pagecursor" };
            var dataResult = DataBaseHelper.QueryStoredProcCursor(executeSql, dataNames, args, CommandType.Text);


            var pagedata = dataResult["datacursor"];
            var pagecount = dataResult["pagecursor"];

            //根据实体表查询对应数据，可改变
            string planTable = "crm_sys_payments_plan";
            string payTable = "crm_sys_payments";
            string orderTable = "crm_sys_order";
            string contractTable = "crm_sys_contract";

            string totalMoney ="0";
            string unpaymentmony = "0";
            string refundtype = "0";//0未知类型
            string contractName = string.Empty;
            string planEntity = string.Empty;
            string planEntiteName = "回款计划";
            string paymentsEntity = string.Empty;
            string paymentsEntityname = "回款登记";

            string entityInfoSql = "select 1 as type,entityid from crm_sys_entity where recstatus=1 and entitytable=@planTable " +
                "UNION select 2 as type,entityid from crm_sys_entity where recstatus = 1 and entitytable =@payTable ";

            var entityParam = new DynamicParameters();
            entityParam.Add("planTable", planTable);
            entityParam.Add("payTable", payTable);
            var entityInfos = DataBaseHelper.Query(entityInfoSql, entityParam);
            if (entityInfos.Count > 0)
            {
                foreach (var item in entityInfos)
                {
                    if (item["type"].ToString() == "1")
                    {
                        planEntity = item["entityid"].ToString();
                    }
                    else {
                        paymentsEntity = item["entityid"].ToString();
                    }
                }
            }

            string sql = "select  orderamount as totalMoney,1 as refundtype,reccode from "+orderTable + "  where recid=@recid" +
                " UNION select contractvolume as totalMoney,2 as refundtype,reccode from "+ contractTable + " where recid=@recid ";

            var param = new DynamicParameters();
            param.Add("recid", parentId);

            var parentBill = DataBaseHelper.Query(sql, param);
            if (parentBill.Count > 0) {
                totalMoney=parentBill[0]["totalmoney"].ToString();
                refundtype = parentBill[0]["refundtype"].ToString();
                contractName = parentBill[0]["reccode"].ToString();
            }

            decimal recAmount = 0;
            //计算业务单据总金额
            foreach (var item in pagedata)
            {
                //获取已回款金额
                if (item["rectype"].ToString() == "2")
                {
                    recAmount +=decimal.Parse(item["money"].ToString());
                }
            }
            unpaymentmony = (decimal.Parse(totalMoney) - recAmount).ToString();

            return new
            {
                pagecount = pagecount,
                pagedata = pagedata,
                parentId = parentId,
                totalMoney = totalMoney,
                unpaymentmony = unpaymentmony,
                contractname = contractName,
                refundtype = refundtype,
                planentity= planEntity,
                planentitename = planEntiteName,
                paymentsentity= paymentsEntity,
                paymentsentityname = paymentsEntityname
            };

        }


        public OperateResult LockContract(Guid contractId, int recStatus, int userNumber)
        {
            var executeSql = @"SELECT * FROM crm_func_contract_lock(@contractid,@recstatus,@userno)";
            var args = new
            {
                contractid = contractId.ToString(),
                recstatus = recStatus,
                UserNo = userNumber
            };

            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }


        public OperateResult UdpateContractStatus(string recIds, int userNumber)
        {
            var executeSql = @"SELECT * FROM crm_func_contract_status_handle(@recids,@userno)";
            var args = new
            {
                recIds = recIds,
                UserNo = userNumber
            };

            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }

    }
}
