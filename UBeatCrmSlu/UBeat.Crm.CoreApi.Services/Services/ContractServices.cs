using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Contract;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.Contract;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class ContractServices : BaseServices
    {
        private IContractRepository _repository;
        private readonly IMapper _mapper;
        public ContractServices(IMapper mapper, IContractRepository repository)
        {
            _repository = repository;
            _mapper = mapper;
        }


        /// <summary>
        /// 添加回款计划
        /// </summary>
        /// <param name="body"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OutputResult<object> AddPaymentPlan(PaymentPlanAddModel body, int userNumber)
        {
            var crmData = new PaymentPlanAdd()
            {
                PlanTime = body.PlanTime,
                ParentId = body.ParentId,
                PlanMoney = body.PlanMoney,
                RefundType =body.RefundType,
                Remark = body.Remark
            };

            if (!crmData.IsValid())
            {
                return HandleValid(crmData);
            }

            return HandleResult(_repository.AddPaymentPlan(crmData, userNumber));
        }





        /// <summary>
        /// 编辑回款计划
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public OutputResult<object> EditPaymentPlan(PaymentPlanAddModel body, int userNumber)
        {
            var crmData = new PaymentPlanEdit()
            {
                PlanId = body.PlanId.Value,
                PlanTime = body.PlanTime,
                PlanMoney = body.PlanMoney,
                Remark = body.Remark
            };

            if (!crmData.IsValid())
            {
                return HandleValid(crmData);
            }

            return HandleResult(_repository.EditPaymentPlan(crmData, userNumber));
        }



        /// <summary>
        /// 删除回款计划
        /// </summary>
        /// <param name="eventids"></param>
        /// <param name="usernumber"></param>
        /// <returns></returns>
        public OutputResult<object> DeletePaymentPlan(PaymentPlanDeleteModel body, int userNumber)
        {
            return HandleResult(_repository.DeletePaymentPlan(body.PlanId, userNumber));
        }



        /// <summary>
        /// 添加回款记录
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public OutputResult<object> AddPayment(PaymentAddModel body, int userNumber)
        {
            var crmData = new PaymentAdd()
            {
                PayTime = body.PayTime,
                PayMoney = body.PayMoney,
                PayName = body.PayName,
                Remark = body.Remark,
                BizType= body.BizType,
                ParentId = body.ParentId
            };

            if (!crmData.IsValid())
            {
                return HandleValid(crmData);
            }

            return HandleResult(_repository.AddPayment(crmData, userNumber));
        }


        /// <summary>
        /// 编辑回款记录
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public OutputResult<object> EditPayment(PaymentAddModel body, int userNumber)
        {
            var crmData = new PaymentEdit()
            {
                PaymentId = body.PaymentId.Value,
                PayTime = body.PayTime,
                PayMoney = body.PayMoney,
                PayName = body.PayName,
                Remark = body.Remark,
                ParentId = body.ParentId

            };

            if (!crmData.IsValid())
            {
                return HandleValid(crmData);
            }

            return HandleResult(_repository.EditPayment(crmData, userNumber));
        }


        /// <summary>
        /// 删除回款记录
        /// </summary>
        /// <param name="eventids"></param>
        /// <param name="usernumber"></param>
        /// <returns></returns>

        public OutputResult<object> DeletePayment(PaymentDeleteModel body, int userNumber)
        {
            return HandleResult(_repository.DeletePayment(body.PaymentId, userNumber));
        }


        /// <summary>
        /// 获取回款记录和回款计划
        /// </summary>
        /// <param name="contractId"></param>
        /// <param name="page"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public dynamic GetPaymentsAndPlan(PaymentListModel body, int userNumber)
        {

            PageParam page = new PageParam()
            {
                PageIndex = body.PageIndex,
                PageSize = body.PageSize
            };

            if (!page.IsValid())
            {
                return HandleValid(page);
            }

            return new OutputResult<object>(_repository.GetPaymentsAndPlan(body.ParentId, page, userNumber));
        }



        /// <summary>
        /// 加锁，解锁合同
        /// </summary>
        /// <param name="body"></param>
        /// <param name="status"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OutputResult<object> LockContract(ContractLockModel body, int status, int userNumber)
        {

            return HandleResult(_repository.LockContract(body.ContractId, status, userNumber));

        }
        /// <summary>
        /// 加锁，解锁合同
        /// </summary>
        /// <param name="body"></param>
        /// <param name="status"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OutputResult<object> UdpateContractStatus(UpdateContractStatusModel entityModel  , int userNumber)
        {
            var entity = _mapper.Map<UpdateContractStatusModel, UpdateContractStatusMapper>(entityModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }

            return HandleResult(_repository.UdpateContractStatus(entity.RecIds, userNumber));

        }
    }
}
