using FluentValidation;
using System;

namespace UBeat.Crm.CoreApi.DomainModel.Attendance
{
    public class AttendanceSignMapper:BaseEntity
    {
        public string SignImg { get; set; }
        public AddressType Locations { get; set; }
        public int SignType { get; set; }
        public string SignMark { get; set; }
        public string SignTime { get; set; }
        public int CardType { get; set; }
        public int RecordSource { get; set; }

        protected override IValidator GetValidator()
        {
            return new AttendanceSignMapperValidator();
        }
    }

    public class AttendanceAddMapper : BaseEntity
    {
        public int SignType { get; set; }
        public string SignMark { get; set; }
        public string SignTime { get; set; }
        public int CardType { get; set; }
        public int SelectUser { get; set; }
        public int RecordSource { get; set; }

        protected override IValidator GetValidator()
        {
            return new AttendanceSignMapperValidator();
        }
    }

    public class GroupUserMapper : BaseEntity
    {
        public string DeptId { get; set; }

        public string UserName { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }
        protected override IValidator GetValidator()
        {
            return new GroupUserMapperValidator();
        }
        class GroupUserMapperValidator : AbstractValidator<GroupUserMapper>
        {
            public GroupUserMapperValidator()
            {
                RuleFor(d => d.DeptId).NotNull().NotEmpty().WithMessage("部门Id不能为空");
            }
        }
    }

    public class AttendanceSignMapperValidator : AbstractValidator<AttendanceSignMapper>
    {
        public AttendanceSignMapperValidator()
        {
            RuleFor(d => d.SignImg).NotEmpty().WithMessage("考勤图片不能为空");
            RuleFor(d => d.Locations).NotNull().Must(ValidLocation).WithMessage("定位信息不正确");
            RuleFor(d => d.SignType).NotNull().GreaterThanOrEqualTo(0).WithMessage("考勤类型不能为空");
            RuleFor(d => d.SignMark).NotNull().WithMessage("考勤备注不能为null");
            RuleFor(d => d.CardType).NotNull().WithMessage("考勤打开类型不能为空");
            RuleFor(d => d.SignTime).NotEmpty().WithMessage("考勤时间不能为空");
            RuleFor(d => d.RecordSource).NotEmpty().WithMessage("考勤来源不能为空");
        }

        public static bool ValidLocation(AddressType address)
        {
            if (string.IsNullOrWhiteSpace(address.Address)) return false;
            if (string.IsNullOrWhiteSpace(address.Lat)) return false;
            if (string.IsNullOrWhiteSpace(address.Lon)) return false;
            return true;
        }
    }

    public class AttendanceAddMapperValidator : AbstractValidator<AttendanceAddMapper>
    {
        public AttendanceAddMapperValidator()
        {
            RuleFor(d => d.SignType).NotNull().GreaterThanOrEqualTo(0).WithMessage("考勤类型不能为空");
            RuleFor(d => d.CardType).NotNull().WithMessage("考勤打开类型不能为空");
            RuleFor(d => d.SelectUser).NotNull().WithMessage("考勤人员不能为空");
            RuleFor(d => d.SignTime).NotEmpty().WithMessage("考勤时间不能为空");
            RuleFor(d => d.RecordSource).NotEmpty().WithMessage("考勤来源不能为空");
        }

        public static bool ValidLocation(AddressType address)
        {
            if (string.IsNullOrWhiteSpace(address.Address)) return false;
            if (string.IsNullOrWhiteSpace(address.Lat)) return false;
            if (string.IsNullOrWhiteSpace(address.Lon)) return false;
            return true;
        }
    }

    public class AttendanceSignListMapper:BaseEntity
    {

        public Guid DeptId { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public int Type { get; set; }
        public int MonthType { get; set; }
        public int ListType { get; set; }
        public string SearchName { get; set; }

        protected override IValidator GetValidator()
        {
            return new AttendanceSignListMapperValidator();
        }
    }

    public class AttendanceSignListMapperValidator : AbstractValidator<AttendanceSignListMapper>
    {
        public AttendanceSignListMapperValidator()
        {
            RuleFor(d => d.MonthType).NotNull().WithMessage("月份类型不能为null");
            RuleFor(d => d.ListType).NotNull().WithMessage("列表类型不能为null");
            RuleFor(d => d.SearchName).NotNull().WithMessage("搜索名称不能为null");
        }
    }
}
