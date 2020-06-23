using System;
using FluentValidation;

namespace UBeat.Crm.CoreApi.DomainModel.Account
{
    public class AccountEntity : BaseEntity
    {
        public string AccountName { get; set; }
        public string AccountPwd { get; set; }
        public string AccessType { get; set; }

        protected override IValidator GetValidator()
        {
            return new AccountValidator();
        }
    }

    public class AccountValidator : AbstractValidator<AccountEntity>
    {
        public AccountValidator()
        {
            RuleFor(d => d.AccessType).NotEmpty().WithMessage("登录类型不能为空");
            RuleFor(d => d.AccountName).NotEmpty().WithMessage("账户名称不能为空");
            RuleFor(d => d.AccountPwd).NotEmpty().WithMessage("账户密码不能为空");
        }
    }

    public class AccountUserMapper
    {
        public int IsCrmUser { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string AccessType { get; set; }
        public string AccountPwd { get; set; }

        public int RecStatus { get; set; }
        public int NextMustChangepwd { get; set; }
        public DateTime LastChangedPwdTime { get; set; }

        public String DDUserId { get; set; }
    }

    public class AccountUserRegistMapper : BaseEntity
    {
        public string AccountName { get; set; }
        /// <summary>
        /// 密码文本，若不加密则为明文，若加密，则使用“时间戳_密码”的格式
        /// </summary>
        public string AccountPwd { get; set; }

        /// <summary>
        /// 加密方式，0=不加密，1=RSA方式加密
        /// </summary>
        public int EncryptType { set; get; }
        public string UserName { get; set; }
        public string AccessType { get; set; }
        public string UserIcon { get; set; }
        public string UserPhone { get; set; }
        public string UserJob { get; set; }
        public Guid DeptId { get; set; }
        public DateTime? JoinedDate { get; set; }
        public DateTime? BirthDay { get; set; }
        public string WorkCode { get; set; }
        public string Email { get; set; }
        public string Remark { get; set; }
        public int Sex { get; set; }
        public string Tel { get; set; }

        public int Status { get; set; }
        public int NextMustChangePwd { get; set; }
        protected override IValidator GetValidator()
        {
            return new AccountUserRegistMapperValidator();
        }
    }

    public class AccountUserRegistMapperValidator : AbstractValidator<AccountUserRegistMapper>
    {
        public AccountUserRegistMapperValidator()
        {
            RuleFor(d => d.AccountName).NotEmpty().WithMessage("帐号名称不能为空");
            RuleFor(d => d.AccountPwd).NotEmpty().WithMessage("账户密码不能为空");
            RuleFor(d => d.UserName).NotEmpty().WithMessage("用户姓名不能为空");
            RuleFor(d => d.AccessType).NotEmpty().WithMessage("登录类型不能为空");
            RuleFor(d => d.UserIcon).NotEmpty().WithMessage("用户头像不能为空");
            RuleFor(d => d.UserPhone).NotEmpty().WithMessage("用户手机不能为空");
            RuleFor(d => d.UserJob).NotNull().WithMessage("用户岗位不能为空");
            RuleFor(d => d.DeptId).NotEmpty().WithMessage("所在部门不能为空");
        }
    }

    public class AccountUserEditMapper : BaseEntity
    {
        public int AccountId { get; set; }
        public string AccountName { get; set; }
        public DateTime? BirthDay { get; set; }
        public DateTime? JoinedDate { get; set; }
        public string Email { get; set; }
        public string Remark { get; set; }
        public string WorkCode { get; set; }
        public int Sex { get; set; }
        public string Tel { get; set; }
        public string UserIcon { get; set; }
        public string UserJob { get; set; }
        public string UserPhone { get; set; }
        public string UserName { get; set; }
        public string AccessType { get; set; }
        public Guid DeptId { get; set; }

        public int Status { get; set; }
        protected override IValidator GetValidator()
        {
            return new AccountUserEditMapperValidator();
        }
    }

    public class AccountUserEditMapperValidator : AbstractValidator<AccountUserEditMapper>
    {
        public AccountUserEditMapperValidator()
        {
            RuleFor(d => d.AccountId).NotEmpty().GreaterThan(0).WithMessage("帐号ID不能为空");
            RuleFor(d => d.AccountName).NotEmpty().WithMessage("帐号名称不能为空");
            RuleFor(d => d.UserName).NotEmpty().WithMessage("用户姓名不能为空");
            RuleFor(d => d.AccessType).NotEmpty().WithMessage("登录类型不能为空");
            RuleFor(d => d.UserIcon).NotEmpty().WithMessage("用户头像不能为空");
            RuleFor(d => d.UserPhone).NotEmpty().WithMessage("用户手机不能为空");
            RuleFor(d => d.UserJob).NotNull().WithMessage("用户岗位不能为空");
            RuleFor(d => d.DeptId).NotEmpty().WithMessage("所在部门不能为空");
        }
    }

    public class AccountUserPwdMapper : BaseEntity
    {
        public int AccountId { get; set; }
        public int UserId { get; set; }
        /// <summary>
        /// 密码文本，若不加密则为明文，若加密，则使用“时间戳_密码”的格式
        /// </summary>
        public string AccountPwd { get; set; }

        /// <summary>
        /// 加密方式，0=不加密，1=RSA方式加密
        /// </summary>
        public int EncryptType { set; get; }
        public string OrginPwd { get; set; }

        protected override IValidator GetValidator()
        {
            return new AccountUserPwdMapperValidator();
        }
    }

    public class AccountUserPwdMapperValidator : AbstractValidator<AccountUserPwdMapper>
    {
        public AccountUserPwdMapperValidator()
        {
            RuleFor(d => d.AccountId).NotNull().GreaterThanOrEqualTo(0).WithMessage("帐号ID不能为空");
            RuleFor(d => d.UserId).NotNull().GreaterThan(0).WithMessage("用户ID不能为空");
            RuleFor(d => d.AccountPwd).NotEmpty().WithMessage("新密码不能为空");
            RuleFor(d => d.OrginPwd).NotEmpty().WithMessage("原始密码不能为空");
        }
    }

    public class AccountMapper:BaseEntity
    {
        public string UserId { get; set; }
        /// <summary>
        /// 密码文本，若不加密则为明文，若加密，则使用“时间戳_密码”的格式
        /// </summary>
        public string Pwd { get; set; }

        /// <summary>
        /// 加密方式，0=不加密，1=RSA方式加密
        /// </summary>
        public int EncryptType { set; get; }

        protected override IValidator GetValidator()
        {
            return new AccountMapperValidator();
        }

         class AccountMapperValidator : AbstractValidator<AccountMapper>
        {
            public AccountMapperValidator()
            {
                RuleFor(d => d.Pwd).NotEmpty().WithMessage("密码不能为空");
            }
        }
    }

    public class AccountUserPhotoMapper : BaseEntity
    {
        public string UserIcon { get; set; }

        protected override IValidator GetValidator()
        {
            return new AccountUserPhotoMapperValidator();
        }
    }

    public class AccountUserPhotoMapperValidator : AbstractValidator<AccountUserPhotoMapper>
    {
        public AccountUserPhotoMapperValidator()
        {
            RuleFor(d => d.UserIcon).NotEmpty().WithMessage("用户头像不能为空");
        }
    }

    public class AccountUserQueryMapper : BaseEntity
    {
        public string UserName { get; set; }
        public string UserPhone { get; set; }
        public int RecStatus { get; set; }
        public Guid DeptId { get; set; }
        protected override IValidator GetValidator()
        {
            return new AccountUserQueryMapperValidator();
        }
    }

    public class AccountUserQueryMapperValidator : AbstractValidator<AccountUserQueryMapper>
    {
        public AccountUserQueryMapperValidator()
        {
            RuleFor(d => d.RecStatus).NotNull().WithMessage("状态不能为NULL");
            RuleFor(d => d.UserName).NotNull().WithMessage("用户名不能为NULL");
            RuleFor(d => d.UserPhone).NotNull().WithMessage("手机号不能为NULL");
            RuleFor(d => d.DeptId).NotNull().WithMessage("部门不能为NULL");
        }
    }

    public class AccountUserQueryForControlMapper
    {
        public string KeyWord { get; set; }
    }


    public class AccountStatusMapper
    {
        public int UserId { get; set; }

        public int Status { get; set; }
    }

    public class AccountDepartmentMapper : BaseEntity
    {
        public int UserId { get; set; }
        public string DeptId { get; set; }
        protected override IValidator GetValidator()
        {
            return new AccountDepartmentMapperValidator();
        }
        class AccountDepartmentMapperValidator : AbstractValidator<AccountDepartmentMapper>
        {
            public AccountDepartmentMapperValidator()
            {
                RuleFor(d => d.UserId).NotNull().WithMessage("用户Id不能为NULL");
                RuleFor(d => d.DeptId).NotEmpty().WithMessage("部门Id不能为空");
            }
        }
    }
    public class DeptOrderbyMapper : BaseEntity
    {
        public string ChangeDeptId { get; set; }
        public string DeptId { get; set; }
        protected override IValidator GetValidator()
        {
            return new DeptOrderbyMapperValidator();
        }
        class DeptOrderbyMapperValidator : AbstractValidator<DeptOrderbyMapper>
        {
            public DeptOrderbyMapperValidator()
            {
                RuleFor(d => d.ChangeDeptId).NotNull().WithMessage("部门Id不能为NULL");
                RuleFor(d => d.DeptId).NotEmpty().WithMessage("部门Id不能为空");
            }
        }
    }

    public class DeptDisabledMapper : BaseEntity
    {
        public int RecStatus { get; set; }
        public string DeptId { get; set; }
        protected override IValidator GetValidator()
        {
            return new DeptDisabledMapperValidator();
        }
        class DeptDisabledMapperValidator : AbstractValidator<DeptDisabledMapper>
        {
            public DeptDisabledMapperValidator()
            {
                RuleFor(d => d.DeptId).NotEmpty().WithMessage("部门Id不能为空");
            }
        }
    }

    public class SetLeaderMapper
    {
        public int UserId { get; set; }
        // 0 否 1 是
        public int IsLeader { get; set; }
    }
    public class SetIsCrmMapper
    {
        public int UserId { get; set; }
        // 0 否 1 是
        public int IsCrmUser { get; set; }
    }




}
