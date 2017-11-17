using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Notice
{
    public class NoticeListMapper
    {
        public string NoticeId { get; set; }
        public int NoticeType { get; set; }
        public string NoticeTitle { get; set; }
        public string KeyWord { get; set; }
        public string HeadImg { get; set; }
        public string HeadRemark { get; set; }
        public string MsgContent { get; set; }
        public string NoticeUrl { get; set; }
        public int RecStatus { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }

        public int RecVersion { get; set; }

        public int NoticeSendStatus { get; set; }
    }
    public class NoticeMapper : BaseEntity
    {
        public Guid EntityId
        {
            get
            {
                return Guid.Parse("00000000-0000-0000-0000-000000000002");
            }
        }
        public string NoticeId { get; set; }
        public int NoticeType { get; set; }
        public string NoticeTitle { get; set; }
        public string HeadImg { get; set; }
        public string HeadRemark { get; set; }
        public string MsgContent { get; set; }
        public string NoticeUrl { get; set; }
        public int RecStatus { get; set; }
        public int RecVersion { get; set; }
        public int NoticeSendStatus { get; set; }
        protected override IValidator GetValidator()
        {
            return new NoticeMapperValidator();
        }

        class NoticeMapperValidator : AbstractValidator<NoticeMapper>
        {
            public NoticeMapperValidator()
            {
                RuleFor(d => d.NoticeTitle).NotEmpty().WithMessage("通告标题不能为空");
                RuleFor(d => d.MsgContent).NotEmpty().WithMessage("通告内容不能为空");
            }
        }
    }

    public class NoticeSendRecordMapper:BaseEntity
    {
        public Guid NoticeId { get; set; }
        public string KeyWord { get; set; }
        public int ReadFlag { get; set; }
        public Guid DeptId { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }

        protected override IValidator GetValidator()
        {
            return new NoticeSendRecordMapperValidator();
        }

        class NoticeSendRecordMapperValidator : AbstractValidator<NoticeSendRecordMapper>
        {
            public NoticeSendRecordMapperValidator()
            {
                RuleFor(d => d.NoticeId).NotEmpty().WithMessage("通告Id不能为空");
            }
        }
    }

    public class NoticeReceiverMapper : BaseEntity
    {
        public Guid EntityId
        {
            get
            {
                return Guid.Parse("00000000-0000-0000-0000-000000000002");
            }
        }
        public string NoticeId { get; set; }
        public string UserIds { get; set; }
        public ICollection<NoticeReceiverDeptMapper> deptids { get; set; }
        public int IspopUp { get; set; }
        protected override IValidator GetValidator()
        {
            return new NoticeReceiverMapperValidator();
        }

        class NoticeReceiverMapperValidator : AbstractValidator<NoticeReceiverMapper>
        {
            public NoticeReceiverMapperValidator()
            {
                RuleFor(d => d.NoticeId).NotEmpty().WithMessage("通告Id不能为空");
            }
        }
    }

    public class NoticeReceiverDeptMapper : BaseEntity
    {
        public string deptid { get; set; }

        public ICollection<string> roleids { get; set; }

        protected override IValidator GetValidator()
        {
            return new NoticeReceiverDeptMapperValidator();
        }

        class NoticeReceiverDeptMapperValidator : AbstractValidator<NoticeReceiverDeptMapper>
        {
            public NoticeReceiverDeptMapperValidator()
            {
                RuleFor(d => d.deptid).NotEmpty().WithMessage("部门Id不能为空");
                RuleFor(d => d.roleids.Count).Must(t => t > 0).WithMessage("角色Id不能为空");
            }
        }
    }

    public class NoticeDisabledMapper : BaseEntity
    {
        public Guid EntityId
        {
            get
            {
                return Guid.Parse("00000000-0000-0000-0000-000000000002");
            }
        }
        public string NoticeIds { get; set; }

        protected override IValidator GetValidator()
        {
            return new NoticeDisabledMapperValidator();
        }

        class NoticeDisabledMapperValidator : AbstractValidator<NoticeDisabledMapper>
        {
            public NoticeDisabledMapperValidator()
            {
                RuleFor(d => d.NoticeIds).NotEmpty().WithMessage("通告Id不能为空");
            }
        }
    }

    public class NoticeReadFlagMapper : BaseEntity
    {
        public string NoticeId { get; set; }
        public int UserId { get; set; }
        protected override IValidator GetValidator()
        {
            return new NoticeReadFlagMapperValidator();
        }

        class NoticeReadFlagMapperValidator : AbstractValidator<NoticeReadFlagMapper>
        {
            public NoticeReadFlagMapperValidator()
            {
                RuleFor(d => d.NoticeId).NotEmpty().WithMessage("通告Id不能为空");
                RuleFor(d => d.UserId).NotNull().WithMessage("用户Id不能为空");
            }
        }
    }
}
