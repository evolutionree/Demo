using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Order
{
    public class OrderPaymentListMapper : BaseEntity
    {
        public Guid RecId { get; set; }

        public Guid EntityId { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }
        protected override IValidator GetValidator()
        {
            return new OrderPaymentListMapperValidator();
        }
        class OrderPaymentListMapperValidator : AbstractValidator<OrderPaymentListMapper>
        {
            public OrderPaymentListMapperValidator()
            {
                RuleFor(d => d.RecId).NotEmpty().WithMessage("订单Id不能为空");
                RuleFor(d => d.EntityId).NotEmpty().WithMessage("订单实体Id不能为空");
            }
        }
    }

    public class OrderStatusMapper : BaseEntity
    {
        public Guid RecId { get; set; }

        public int Status { get; set; }
        protected override IValidator GetValidator()
        {
            return new OrderStatusMapperValidator();
        }
        class OrderStatusMapperValidator : AbstractValidator<OrderStatusMapper>
        {
            public OrderStatusMapperValidator()
            {
                RuleFor(d => d.RecId).NotEmpty().WithMessage("订单Id不能为空");
                RuleFor(d => d.Status).NotEmpty().WithMessage("订单状态不能为空");
            }
        }
    }
}
