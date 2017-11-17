using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.CoreApi.Services.Models.Documents;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.DomainModel.Documents;
using UBeat.Crm.CoreApi.Services.Models.Products;
using UBeat.Crm.CoreApi.Services.Models.Contract;

/// <summary>
/// 通用回款记录业务,不仅仅用于合同回款
/// </summary>
namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class ContractController : BaseController
    {
        private readonly ILogger<ContractController> _logger;
        private readonly ContractServices _service;
        private readonly EntityProServices _entityProService;


        public ContractController(ILogger<ContractController> logger, ContractServices service, EntityProServices entityProService) : base(entityProService)
        {
            _logger = logger;
            _service = service;
            _entityProService = entityProService;
        }

        #region 计划回款

        /// <summary>
        /// 获取回款计划协议
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("allocateplan")]
        public OutputResult<object> AllocateAddPlan([FromBody] PaymentPlanAddModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");

            return _service.AddPaymentPlan(body, UserId);
        }

        //编辑协议
        [HttpPost("editplan")]
        public OutputResult<object> EditPlan([FromBody] PaymentPlanAddModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");

            return _service.AddPaymentPlan(body, UserId);
        }


        /// <summary>
        /// 保存合同回款计划
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("saveplan")]
        public OutputResult<object> SavePlan([FromBody] PaymentPlanAddModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");
            if (body.PlanId.HasValue)
            {
                return _service.EditPaymentPlan(body, UserId);
            }
            else
            {
                return _service.AddPaymentPlan(body, UserId);
            }
        }


        /// <summary>
        /// 删除回款计划
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>

        [HttpPost("deleteplan")]
        public OutputResult<object> DeletePlanById([FromBody] PaymentPlanDeleteModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");
            if (body.PlanId == null) return ResponseError<object>("回款计划id不能为空");

            return _service.DeletePaymentPlan(body, UserId);

        }

        #endregion

        #region 合同回款详情


        /// <summary>
        /// 获取新增回款信息的协议
        /// </summary>
        /// <returns></returns>
        [HttpPost("allocatepayment")]
        public OutputResult<object> AllocateAddPayment([FromBody] PaymentAddModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");
            return _service.AddPayment(body, UserId);

        }


        //添加回款协议

        //编辑回款协议


        /// <summary>
        /// 保存合同回款信息
        /// </summary>
        /// <returns></returns>
        [HttpPost("savepayment")]
        public OutputResult<object> SavePayment([FromBody] PaymentAddModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");

            if (body.PaymentId == null)
            {
                return _service.AddPayment(body, UserId);
            }
            else
            {
                return _service.EditPayment(body, UserId);
            }
        }



        /// <summary>
        /// 获取修改回款信息的协议
        /// </summary>
        /// <returns></returns>
        [HttpPost("deletepayment")]
        public OutputResult<object> EditPayment([FromBody] PaymentDeleteModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");
            if (body.PaymentId == null) return ResponseError<object>("合同回款id不能为空");

            return _service.DeletePayment(body, UserId);
        }


        /// <summary>
        /// 获取合同所有的回款信息
        /// </summary>
        /// <param name="bodyData"></param>
        /// <returns></returns>
        [HttpPost("paymentsandplan")]
        public OutputResult<object> GetPaymentsAndPlan([FromBody] PaymentListModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");
            if (body.ParentId == null) return ResponseError<object>("业务单据id不能为空");
            return _service.GetPaymentsAndPlan(body, UserId);

        }




        #endregion

        #region 加锁解锁合同计划

        /// <summary>
        /// 锁定合同回款计划
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("lockplan")]
        public OutputResult<object> FixedPlan([FromBody] ContractLockModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");

            return _service.LockContract(body, (int)ContractLockStatus.Lock, UserId);

        }


        /// <summary>
        /// 解锁合同回款计划
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("unlockplan")]
        public OutputResult<object> UnFixedPlan([FromBody] ContractLockModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");
            return _service.LockContract(body, (int)ContractLockStatus.UnLock, UserId);

        }

        /// <summary>
        /// 更新合同状态
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("updatecontractstatus")]
        public OutputResult<object> UdpateContractStatus([FromBody] UpdateContractStatusModel entityModel)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");
            return _service.UdpateContractStatus(entityModel, UserId);

        }
        #endregion
    }
}






