using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Contract;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IContractRepository
    {


        /// <summary>
        /// 添加回款计划
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        OperateResult AddPaymentPlan(PaymentPlanAdd data, int userNumber);


        /// <summary>
        /// 编辑回款计划
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        OperateResult EditPaymentPlan(PaymentPlanEdit data, int userNumber);


        /// <summary>
        /// 删除回款计划
        /// </summary>
        /// <param name="eventids"></param>
        /// <param name="usernumber"></param>
        /// <returns></returns>
        OperateResult DeletePaymentPlan(Guid planId, int userNumber);




        /// <summary>
        /// 获取回款计划
        /// </summary>
        /// <returns></returns>
        dynamic GetPaymentPlans(int userNumbe);


        /// <summary>
        /// 添加回款记录
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        OperateResult AddPayment(PaymentAdd data, int userNumber);


        /// <summary>
        /// 编辑回款记录
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        OperateResult EditPayment(PaymentEdit data, int userNumber);


        /// <summary>
        /// 删除回款记录
        /// </summary>
        /// <param name="eventids"></param>
        /// <param name="usernumber"></param>
        /// <returns></returns>
        OperateResult DeletePayment(Guid paymentId, int userNumber);


        /// <summary>
        /// 获取回款记录和回款计划
        /// </summary>
        /// <param name="contractId"></param>
        /// <param name="page"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        dynamic GetPaymentsAndPlan(Guid contractId, PageParam page, int userNumber);

        /// <summary>
        /// 锁定解锁合同
        /// </summary>
        /// <param name="contractId"></param>
        /// <param name="recStatus"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        OperateResult LockContract(Guid contractId, int recStatus, int userNumber);
        /// <summary>
        /// 更新合同状态
        /// </summary>
        /// <param name="recIds"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        OperateResult UdpateContractStatus(string recIds, int userNumber);
    }
}