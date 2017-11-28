using FluentValidation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UBeat.Crm.CoreApi.DomainModel.Utility;

namespace UBeat.Crm.CoreApi.DomainModel.EMail
{
    public class SendEMailMapper : BaseEntity
    {
        /// <summary>
        /// 发件人
        /// </summary>
        public string FromAddress { get; set; }

        /// <summary>
        /// 发件昵称
        /// </summary>
        public string FromName { get; set; }

        /// <summary>
        /// 收件人
        /// </summary>
        public IList<MailAddressMapper> ToAddress { get; set; }
        /// <summary>
        /// 抄送人
        /// </summary>
        public IList<MailAddressMapper> CCAddress { get; set; }
        /// <summary>
        /// 密送人
        /// </summary>
        public IList<MailAddressMapper> BCCAddress { get; set; }
        /// <summary>
        /// 邮件主题
        /// </summary>
        public string Subject { get; set; }
        /// <summary>
        /// 主体消息内容
        /// </summary>
        public string BodyContent { get; set; }

        public IList<AttachmentFileMapper> AttachmentFile { get; set; }

        public Guid PMailId { get; set; }

        protected override IValidator GetValidator()
        {
            return new SendEMailMapperValidator();
        }
        class SendEMailMapperValidator : AbstractValidator<SendEMailMapper>
        {
            /// <summary>
            /// 数据验证类使用的正则表述式选项
            /// </summary>
            private const RegexOptions Options = RegexOptions.IgnoreCase | RegexOptions.Compiled;
            /// <summary>
            /// 检测字符串是否为有效的邮件地址捕获正则
            /// </summary>
            private static readonly Regex EmailRegex = new Regex(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$", Options);

            public SendEMailMapperValidator()
            {
                RuleFor(d => d.FromAddress).NotEmpty().WithMessage("发件人地址不能为空");
                RuleFor(d => d.FromAddress).Matches(EmailRegex).WithMessage("发件人地址格式不符合邮箱格式");
                RuleFor(d => d.FromName).NotEmpty().WithMessage("发件人昵称不能为空");
                RuleFor(d => d.ToAddress).NotNull().Must(d => d.Count > 0).WithMessage("收件人不能为空");
                RuleFor(d => d.ToAddress).Must(ValidAddress).WithMessage("发件人信息异常");
                RuleFor(d => d.CCAddress).Must(ValidAddress).WithMessage("抄送人信息异常");
                RuleFor(d => d.BCCAddress).Must(ValidAddress).WithMessage("密送人信息异常");
            }
            bool ValidAddress(IList<MailAddressMapper> address)
            {
                foreach (var tmp in address)
                {
                    if (string.IsNullOrEmpty(tmp.Address))
                        return false;
                    return EmailRegex.IsMatch(tmp.Address);
                }
                return true;
            }
        }
    }

    public class MailAddressMapper
    {
        public string Address { get; set; }
        public string DisplayName { get; set; }
    }


    public class AttachmentFileMapper
    {
        public string FileId { get; set; }

        public int FileType { get; set; }
    }
    public class ReceiveEMailMapper
    {
        public int Conditon { get; set; }

        public string ConditionVal { get; set; }
        /// <summary>
        /// 预留给定时器
        /// </summary>
        public bool? IsDevice { get; set; }
    }
    public class MailSenderReceiversMapper
    {
        [JsonProperty("ctype")]
        public int Ctype { get; set; }
        [JsonProperty("biztype")]
        public int BizType { get; set; }
        [JsonProperty("mailaddress")]
        public string MailAddress { get; set; }
        [JsonProperty("displayname")]
        public string DisplayName { get; set; }
    }

    public class MailBodyMapper
    {
        public Guid MailId { get; set; }

        public string Sender { get; set; }

        public JArray Receivers { get; set; }

        public JArray Ccers { get; set; }

        public JArray Bccers { get; set; }

        public string Title { get; set; }

        public string MailBody { get; set; }
        public string Summary { get; set; }
        public DateTime ReceivedTime { get; set; }

        public DateTime SentTime { get; set; }

        public Int64 IsTag { get; set; }

        public Int64 IsRead { get; set; }

        public Int64 AttachCount { get; set; }
        public JArray AttachInfo { get; set; }
    }

    public class MailBodyDetailMapper
    {
        public Guid MailId { get; set; }

        public string Sender { get; set; }
        [JsonIgnore]
        public string ReceiversJson { get; set; }
        [JsonIgnore]
        public string CcersJson { get; set; }
        [JsonIgnore]
        public string BccersJson { get; set; }

        public JArray Receivers
        {
            get
            {
                return ReceiversJson.ToJsonArray();
            }
        }

        public JArray Ccers
        {
            get
            {
                return CcersJson.ToJsonArray();
            }
        }
        public JArray Bccers
        {
            get
            {
                return BccersJson.ToJsonArray();
            }
        }

        public string Title { get; set; }

        public string MailBody { get; set; }

        public DateTime ReceivedTime { get; set; }

        public DateTime SentTime { get; set; }

        public Int64 IsTag { get; set; }

        public Int64 IsRead { get; set; }

        public Int64 AttachCount { get; set; }
        [JsonIgnore]
        public string AttachInfoJson { get; set; }
        public JArray AttachInfo
        {
            get
            {
                return AttachInfoJson.ToJsonArray();
            }
        }
    }
    public class MailRelatedUser
    {
        public string DisplayName { get; set; }

        public string MailAddress { get; set; }
    }

    public class TagMailMapper : BaseEntity
    {
        public string MailIds { get; set; }

        public MailTagActionType actionType { get; set; }


        protected override IValidator GetValidator()
        {
            return new TagMailMapperValidator();
        }
        class TagMailMapperValidator : AbstractValidator<TagMailMapper>
        {
            public TagMailMapperValidator()
            {
                RuleFor(d => d.MailIds).NotEmpty().WithMessage("邮件Id不能为空");
            }
        }
    }

    public class DeleteMailMapper : BaseEntity
    {
        /// <summary>
        /// 是否彻底删除 
        /// </summary>
        public bool IsTruncate { get; set; }
        public string MailIds { get; set; }

        protected override IValidator GetValidator()
        {
            return new DeleteMailMapperValidator();
        }
        class DeleteMailMapperValidator : AbstractValidator<DeleteMailMapper>
        {
            public DeleteMailMapperValidator()
            {
                RuleFor(d => d.MailIds).NotEmpty().WithMessage("邮件Id不能为空");
            }
        }
    }
    public class ReConverMailMapper : BaseEntity
    {
        public int RecStatus { get; set; }
        public string MailIds { get; set; }

        protected override IValidator GetValidator()
        {
            return new ReConverMailMapperValidator();
        }
        class ReConverMailMapperValidator : AbstractValidator<ReConverMailMapper>
        {
            public ReConverMailMapperValidator()
            {
                RuleFor(d => d.MailIds).NotEmpty().WithMessage("邮件Id不能为空");
            }
        }
    }

    public class ReadOrUnReadMailMapper : BaseEntity
    {
        public int IsRead { get; set; }
        public string MailIds { get; set; }

        protected override IValidator GetValidator()
        {
            return new ReadOrUnReadMailMapperValidator();
        }
        class ReadOrUnReadMailMapperValidator : AbstractValidator<ReadOrUnReadMailMapper>
        {
            public ReadOrUnReadMailMapperValidator()
            {
                RuleFor(d => d.MailIds).NotEmpty().WithMessage("邮件Id不能为空");
            }
        }
    }
    public class MailDetailMapper : BaseEntity
    {
        public Guid MailId { get; set; }
        protected override IValidator GetValidator()
        {
            return new MailDetailMapperValidator();
        }
        class MailDetailMapperValidator : AbstractValidator<MailDetailMapper>
        {
            public MailDetailMapperValidator()
            {
                RuleFor(d => d.MailId).NotEmpty().WithMessage("邮件Id不能为空");
            }
        }
    }

    public class MailAttachmentMapper
    {
        public Guid MongoId { get; set; }

        public string FileName { get; set; }

        public string FileType { get; set; }

        public int FileSize { get; set; }

        public Guid MailId { get; set; }
    }

    public class TransferMailDataMapper : BaseEntity
    {
        public List<Guid> MailIds { get; set; }
        public List<int> TransferUserIds { get; set; }
        public List<Guid> DeptIds { get; set; }
        public List<MailAttachmentMapper> Attachment { get; set; }
        protected override IValidator GetValidator()
        {
            return new TransferMailDataMapperValidator();
        }
        class TransferMailDataMapperValidator : AbstractValidator<TransferMailDataMapper>
        {
            public TransferMailDataMapperValidator()
            {
                RuleFor(d => d).Must(ValidMail).WithMessage("没有需要转移的邮件");
                RuleFor(d => d).Must(ValidUser).WithMessage("没有需要转移邮件的人员");
            }
            bool ValidUser(TransferMailDataMapper entity)
            {
                if (entity.TransferUserIds.Count == 0 && entity.DeptIds.Count == 0)
                {
                    return false;
                }
                return true;
            }
            bool ValidMail(TransferMailDataMapper entity)
            {
                if (entity.MailIds.Count == 0)
                {
                    return false;
                }
                return true;
            }
        }
    }



    public class MoveMailMapper : BaseEntity
    {
        public string MailIds { get; set; }

        public Guid CatalogId { get; set; }

        protected override IValidator GetValidator()
        {
            return new MoveMailMapperValidator();
        }
        class MoveMailMapperValidator : AbstractValidator<MoveMailMapper>
        {
            public MoveMailMapperValidator()
            {
                RuleFor(d => d.MailIds).NotEmpty().WithMessage("邮件Id不能为空");
                RuleFor(d => d.CatalogId).NotNull().WithMessage("邮件目录Id不能为空");
            }
        }
    }

    public class ToAndFroMapper : BaseEntity
    {
        public Guid MailId { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int relatedMySelf { get; set; }
        public int relatedSendOrReceive { get; set; }

        protected override IValidator GetValidator()
        {
            return new ToAndFroMapperValidator();
        }
        class ToAndFroMapperValidator : AbstractValidator<ToAndFroMapper>
        {
            public ToAndFroMapperValidator()
            {
                RuleFor(d => d.PageIndex).Must(d => d > 0).WithMessage("页索引不能小于等于0");
                RuleFor(d => d.PageSize).Must(d => d > 0).WithMessage("页大小不能小于等于0");
                RuleFor(d => d.MailId).NotNull().WithMessage("邮件Id不能为空");
            }
        }
    }

    public class ToAndFroFileMapper
    {

        public string FileName { get; set; }

        public string FileType { get; set; }

        public Int64 FileSize { get; set; }

        public Guid MongoId { get; set; }

        public Guid MailId { get; set; }

        public DateTime ReceivedTime { get; set; }
    }

    public class AttachmentListMapper : BaseEntity
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }

        public string KeyWord { get; set; }


        protected override IValidator GetValidator()
        {
            return new AttachmentListMapperValidator();
        }
        class AttachmentListMapperValidator : AbstractValidator<AttachmentListMapper>
        {
            public AttachmentListMapperValidator()
            {
                RuleFor(d => d.PageIndex).Must(d => d > 0).WithMessage("页索引不能小于等于0");
                RuleFor(d => d.PageSize).Must(d => d > 0).WithMessage("页大小不能小于等于0");
            }
        }
    }

    public class AttachmentChooseListMapper
    {
        public Guid FileId { get; set; }

        public string FileName { get; set; }
    }


    public class MailBoxMapper
    {
        public string Accountid { get; set; }
        public int Owner { get; set; }
        public string OwnerName { get; set; }
    }

    public class InnerUserMailMapper
    {
        public string UserId { get; set; }
        public string UserEMail { get; set; }
    }

    public class MailUserMapper
    {
        public string EmailAddress { get; set; }

        public string Name { get; set; }

        public string customer { get; set; }

        public Guid icon { get; set; }
    }
    public class OrgAndStaffMapper
    {
        public string mail { get; set; }
        public string TreeId { get; set; }

        public string TreeName { get; set; }

        public string DeptName { get; set; }

        /// <summary>
        /// 0是部门类型 1是人员类型
        /// </summary>
        public int nodeType { get; set; }

    }

    public class InnerToAndFroUser
    {
        public int userId { get; set; }
        public string userName { get; set; }
        public decimal unRead { get; set; }
    }

    public class TransferRecordMapper
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FromUser { get; set; }
        public DateTime TransferTime { get; set; }
    }


    public class TransferRecordParamMapper : BaseEntity
    {
        public Guid MailId { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }

        protected override IValidator GetValidator()
        {
            return new TransferRecordParamMapperValidator();
        }
        class TransferRecordParamMapperValidator : AbstractValidator<TransferRecordParamMapper>
        {
            public TransferRecordParamMapperValidator()
            {
                RuleFor(d => d.MailId).NotNull().WithMessage("邮件Id不能为空");
            }
        }
    }

    public class ReceiveMailRelatedMapper
    {
        public int UserId { get; set; }
        public DateTime ReceiveTime { get; set; }
        public string MailServerId { get; set; }

        public Guid MailId { get; set; }
    }

    public class InnerToAndFroMailMapper:BaseEntity
    {
        public string KeyWord { get; set; }

        public int FromUserId { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }
        protected override IValidator GetValidator()
        {
            return new InnerToAndFroMailMapperValidator();
        }
        class InnerToAndFroMailMapperValidator : AbstractValidator<InnerToAndFroMailMapper>
        {
            public InnerToAndFroMailMapperValidator()
            {
                RuleFor(d => d.PageIndex).Must(d => d > 0).WithMessage("页索引不能小于等于0");
                RuleFor(d => d.PageSize).Must(d => d > 0).WithMessage("页大小不能小于等于0");
            }
        }
    }
}
