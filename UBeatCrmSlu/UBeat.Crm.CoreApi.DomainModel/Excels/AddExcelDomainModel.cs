using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Excels
{
    public class AddExcelDomainModel: BaseEntity
    {
        public Guid ExcelTemplateId { set; get; }

        public Guid Entityid { set; get; }

        public string BusinessName { set; get; }

        public string FuncName { set; get; }

        public string Remark { set; get; }
        //Excel的文件名称
        public string ExcelName { set; get; }
        //Excel的模板内容
        public string TemplateContent { set; get; }

        public int UserNo { set; get; }

        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<AddExcelDomainModel>
        {
            public Validator()
            {
                RuleFor(d => d.BusinessName).NotEmpty().NotNull().WithMessage("BusinessName不能为空");
                RuleFor(d => d.FuncName).NotEmpty().NotNull().WithMessage("FuncName不可为空");
            }
        }
    }
}
