using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace UBeat.Crm.CoreApi.DomainModel.OpreateLog
{
    public class OperateLogRecordListMapper:BaseEntity
    {
        public Guid DeptId { get; set; }
        public string UserName { get; set; }
        public string SearchBegin { get; set; }
        public string SearchEnd { get; set; }

        protected override IValidator GetValidator()
        {
            return new OperateLogRecordListMapperValidator();
        }
    }

    public class OperateLogRecordListMapperValidator : AbstractValidator<OperateLogRecordListMapper>
    {
        public OperateLogRecordListMapperValidator()
        {
            RuleFor(d => d.DeptId).NotNull().WithMessage("部门ID不能为空");
            RuleFor(d => d.UserName).NotNull().WithMessage("用户名称不能为NULL");
            RuleFor(d => d.SearchBegin).NotNull().WithMessage("开始时间不能为NULL");
            RuleFor(d => d.SearchEnd).NotNull().WithMessage("结束时间不能为NULL");
        }
    }
}
