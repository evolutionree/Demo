using System;
using System.Collections.Generic;
using FluentValidation;

namespace UBeat.Crm.CoreApi.DomainModel.BasicData
{
    public class NotifyMessageMapper : BaseEntity
    {
        public Int64 RecVersion { get; set; }
        public int LoginType { get; set; }
        protected override IValidator GetValidator()
        {
            return new NotifyMessageMapperValidator();
        }
    }

    public class NotifyMessageMapperValidator : AbstractValidator<NotifyMessageMapper>
    {
        public NotifyMessageMapperValidator()
        {
            RuleFor(d => d.RecVersion).NotNull().GreaterThanOrEqualTo(0).WithMessage("记录版本不能为空");
            RuleFor(d => d.LoginType).NotNull().GreaterThanOrEqualTo(0).WithMessage("登录类型不能为空");
        }
    }

    public class SyncDataMapper : BaseEntity
    {
        public Dictionary<string, Int64> VersionKey { get; set; }
        protected override IValidator GetValidator()
        {
            return new SyncDataMapperValidator();
        }
    }

    public class SyncDataMapperValidator : AbstractValidator<SyncDataMapper>
    {
        public SyncDataMapperValidator()
        {
            RuleFor(d => d.VersionKey).NotNull().Must(ValidVersionKey).WithMessage("版本号必须大于等于0");
        }

        public static bool ValidVersionKey(Dictionary<string, Int64> versionDic)
        {
            foreach (var verNum in versionDic.Values)
            {
                if (verNum < 0) return false;
            }
            return true;
        }
    }

    public class DeptDataMapper : BaseEntity
    {
        public Guid DeptId { get; set; }
        public int Status { get; set; }
        public int Direction { get; set; }
        protected override IValidator GetValidator()
        {
            return new DeptDataMapperValidator();
        }
    }

    public class DeptDataMapperValidator : AbstractValidator<DeptDataMapper>
    {
        public DeptDataMapperValidator()
        {
            RuleFor(d => d.DeptId).NotNull().WithMessage("部门ID不能为空");
            RuleFor(d => d.Direction).NotNull().WithMessage("遍历方向不能为空");
        }
    }

    public class BasicDataUserContactListMapper : BaseEntity
    {
        public Int64 RecVersion { get; set; }
        public string SearchName { get; set; }
        protected override IValidator GetValidator()
        {
            return new BasicDataUserContactListMapperValidator();
        }
    }

    public class BasicDataUserContactListMapperValidator : AbstractValidator<BasicDataUserContactListMapper>
    {
        public BasicDataUserContactListMapperValidator()
        {
            RuleFor(d => d.RecVersion).NotNull().GreaterThanOrEqualTo(0).WithMessage("版本ID不能为空");
            RuleFor(d => d.SearchName).NotNull().WithMessage("查询名称不能为NULL");
        }
    }

    public class AddAnalyseMapper : BaseEntity
    {
        public string AnafuncName { get; set; }
        public int MoreFlag { get; set; }
        public string CountFunc { get; set; }
        public string MoreFunc { get; set; }

        protected override IValidator GetValidator()
        {
            return new AddAnalyseMapperValidator();
        }

        class AddAnalyseMapperValidator : AbstractValidator<AddAnalyseMapper>
        {
            public AddAnalyseMapperValidator()
            {
                RuleFor(d => d.AnafuncName).NotEmpty().WithMessage("指标名称不能为空");
                RuleFor(d => d.CountFunc).NotEmpty().WithMessage("统计函数不能为空");
                RuleFor(d => d.MoreFunc).NotEmpty().WithMessage("详情函数不能为空");
                RuleFor(d => d.MoreFlag >= 0).NotEmpty().WithMessage("参数不能为负数");
            }
        }
    }
    public class EditAnalyseMapper : BaseEntity
    {
        public string AnafuncId{ get; set; }
        public string AnafuncName { get; set; }
        public int MoreFlag { get; set; }
        public string CountFunc { get; set; }
        public string MoreFunc { get; set; }

        protected override IValidator GetValidator()
        {
            return new EditAnalyseMapperValidator();
        }

        class EditAnalyseMapperValidator : AbstractValidator<EditAnalyseMapper>
        {
            public EditAnalyseMapperValidator()
            {
                RuleFor(d => d.AnafuncId).NotEmpty().WithMessage("指标Id不能为空");
                RuleFor(d => d.AnafuncName).NotEmpty().WithMessage("指标名称不能为空");
                RuleFor(d => d.CountFunc).NotEmpty().WithMessage("统计函数不能为空");
                RuleFor(d => d.MoreFunc).NotEmpty().WithMessage("详情函数不能为空");
                RuleFor(d => d.MoreFlag >= 0).NotEmpty().WithMessage("参数不能为负数");
            }
        }
    }
    public class DisabledOrOderbyAnalyseMapper : BaseEntity
    {
        public string AnafuncIds { get; set; }
        protected override IValidator GetValidator()
        {
            return new DisabledOrOderbyAnalyseMapperValidator();
        }

        class DisabledOrOderbyAnalyseMapperValidator : AbstractValidator<DisabledOrOderbyAnalyseMapper>
        {
            public DisabledOrOderbyAnalyseMapperValidator()
            {
                RuleFor(d => d.AnafuncIds).NotEmpty().WithMessage("指标Id不能为空");
            }
        }
    }

    public class AnalyseListMapper : BaseEntity
    {
        public string AnafuncName { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }
            protected override IValidator GetValidator()
        {
            return new AnalyseListMapperValidator();
        }

        class AnalyseListMapperValidator : AbstractValidator<AnalyseListMapper>
        {
            public AnalyseListMapperValidator()
            {
                RuleFor(d => d.AnafuncName).NotEmpty().WithMessage("指标名称不能为空");
            }
        }
    }
}
