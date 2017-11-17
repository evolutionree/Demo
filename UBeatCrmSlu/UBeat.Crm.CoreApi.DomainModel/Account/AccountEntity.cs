using FluentValidation;

namespace UBeat.Crm.CoreApi.DomainModel.Account
{
    //public class AccountEntity:BaseEntity
    //{
    //    public string AccountName { get; set; }
    //    public string AccountPwd { get; set; }
    //    public string AccessType { get; set; }

    //    protected override IValidator GetValidator()
    //    {
    //        return new AccountValidator();
    //    }
    //}

    //public class AccountValidator : AbstractValidator<AccountEntity>
    //{
    //    public AccountValidator()
    //    {
    //        RuleFor(d => d.AccessType).NotEmpty().WithMessage("登录类型不能为空");
    //        RuleFor(d => d.AccountName).NotEmpty().WithMessage("账户名称不能为空");
    //        RuleFor(d => d.AccountPwd).NotEmpty().WithMessage("账户密码不能为空");
    //    }
    //}
}
