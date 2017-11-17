using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.Contract
{
    public class PaymentPlanAddModel
    {
        public Guid? PlanId { get; set; }
        public DateTime PlanTime { get; set; }
        public decimal PlanMoney { get; set; }
        public Guid ParentId { get; set; }
        public int RefundType { get; set; }
        public string Remark { get; set; }

    }
    public class PaymentPlanEditModel
    {
        public Guid PlanId { get; set; }
        public DateTime PlanTime { get; set; }
        public decimal PlanMoney { get; set; }
        public string Remark { get; set; }

    }

    public class PaymentPlanDeleteModel
    {

        public Guid PlanId { get; set; }
    }




    public class PaymentPlanListModel
    {
    }


    public class PaymentAddModel
    {
        public Guid? PaymentId { get; set; }
        public DateTime PayTime { get; set; }
        public decimal PayMoney { get; set; }
        public string PayName { get; set; }
        public string Remark { get; set; }
        public int BizType { get; set; }
        public Guid ParentId { get; set; }

    }
    public class PaymentEditModel
    {

        public Guid PaymentId { get; set; }

        public DateTime PayTime { get; set; }
        public decimal PayMoney { get; set; }
        public string PayName { get; set; }
        public string Remark { get; set; }
        public Guid ContractId { get; set; }
        public int UserNo { get; set; }


    }

    public class PaymentDeleteModel
    {
        public Guid PaymentId { get; set; }

    }

    public class PaymentListModel
    {

        public Guid ParentId { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }


    public class ContractLockModel
    {
        public Guid ContractId { get; set; }
    }

    public class UpdateContractStatusModel
    {
        public string RecIds{ get; set; }
    }


    public enum ContractLockStatus
    {
        /// <summary>
        /// 解锁
        /// </summary>
        UnLock = 0,

        /// <summary>
        /// 加锁
        /// </summary>
        Lock = 1


    }






}
