using FluentValidation;

namespace UBeat.Crm.CoreApi.DomainModel
{
    public class PageParam:BaseEntity
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }

        protected override IValidator GetValidator()
        {
            return new PageParamValidator();
        }
    }

    public class PageParamValidator : AbstractValidator<PageParam>
    {
        public PageParamValidator()
        {
            RuleFor(d => d.PageIndex).NotNull().GreaterThan(0).WithMessage("页码必须大于0");
            RuleFor(d => d.PageSize).NotNull().Must(ValidPageSize).WithMessage("分页大小不正确");
        }

        public static bool ValidPageSize(int pageSize)
        {
            return pageSize == -1 || pageSize > 0;
        }
    }
}
