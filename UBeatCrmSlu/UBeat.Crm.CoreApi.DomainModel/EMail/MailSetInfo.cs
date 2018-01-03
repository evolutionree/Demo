using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace UBeat.Crm.CoreApi.DomainModel.EMail
{
    public class MailServer
    {
        public Guid recid { get; set; }
        public string recname { get; set; }

        public int servertype { get; set; }

        public string imapaddress { get; set; }

        public string smtpaddress { get; set; }

        public string mailprovider { get; set; }

        public int refreshinterval { get; set; }
    }

    public class OrgAndStaffTree
    {
        public string TreeId { get; set; }

        public string TreeName { get; set; }

        public string DeptName { get; set; }

        public string userJob { get; set; }

        /// <summary>
        /// 0是部门类型 1是人员类型
        /// </summary>
        public int nodeType { get; set; }

        public int unreadcount { get; set; }

    }

    public class MailCatalogInfo
    {
        /// <summary>
        /// 目录的id
        /// </summary>
        public Guid RecId { get; set; }
        /// <summary>
        /// 目录的名称
        /// </summary>
        public string RecName { get; set; }
        /// <summary>
        /// 目录拥有者的id
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// 目录查看者id,用于目录转移时使用
        /// </summary>
        public int ViewUserId { get; set; }
        /// <summary>
        /// 目录的类型
        /// </summary>
        public MailCatalogType CType { get; set; }
        /// <summary>
        /// 目录关联的客户类型(如有)
        /// </summary>
        public Guid CustCatalog { get; set; }
        /// <summary>
        /// 目录关联的客户（如有）
        /// </summary>
        public Guid CustId { get; set; }
        /// <summary>
        /// 目录状态
        /// </summary>
        public int RecStatus { get; set; }
        /// <summary>
        /// 目录的上级目录（拥有者目录，用于邮件归集）
        /// </summary>
        public Guid PId { get; set; }
        /// <summary>
        /// 查看目录的商机目录（查看着目录,用于邮件显示)
        /// </summary>
        public Guid VPId { get; set; }
        /// <summary>
        /// 目录的显示顺序号，
        /// </summary>
        public int RecOrder { get; set; }
        /// <summary>
        /// 未读邮件数
        /// </summary>
        public int UnReadCount { get; set; }

        /// <summary>
        /// 邮件数
        /// </summary>
        public int MailCount { get; set; }

        /// <summary>
        /// 默认目录模板id
        /// </summary>
        public Guid DefaultId { get; set; }

        public List<MailCatalogInfo> SubCatalogs { get; set; }
    }
    /// <summary>
    /// 目录的类型，用于标记前端显示和后端统计
    /// </summary>
    public enum MailCatalogType
    {
        InBox = 1001,//收件箱
        UnReadBox = 1002,//未读邮件
        UnSpec = 2003,//未归类邮件目录
        SendBox = 1003,//发件箱
        OutBox = 1004,//已发送
        DrafBox = 1005,//草稿箱
        PersonalRecycle = 1006,//个人回收箱
        GlobalRecycle = 1007,//集团回收箱
        FlagStar=1008,//标星邮件
        InnerTransfer = 1009,//内部分发目录
        CustType = 3001,//客户分类目录
        Cust = 2001,//客户目录
        CustDyn = 4001,//客户自定义目录
        Personal = 2002,//个人目录
        PersonalDyn = 3002,//个人自定义目录
        CustType_Send = 3003,//客户分类目录_发送
        Cust_Send = 2004,//客户目录_发送
        CustDyn_Send = 4002,//客户自定义目录_发送
        Personal_Send = 2005,//个人目录_发送
        PersonalDyn_Send = 3004,//个人自定义目录_发送
        UnSpec_Send = 2006,//未归类邮件目录_发送
    }


    public class CUMailCatalogMapper : BaseEntity
    {
        public Guid CatalogId { get; set; }
        public string CatalogName { get; set; }
        public int Ctype { get; set; }
        public Guid CustCataLog { get; set; }
        public Guid CustId { get; set; }
        public Guid CatalogPId { get; set; }
        public int Recorder { get; set; }
        public bool IsAdd { get; set; }

        protected override IValidator GetValidator()
        {
            return new CUMailCatalogMapperValidator(IsAdd);
        }

        class CUMailCatalogMapperValidator : AbstractValidator<CUMailCatalogMapper>
        {

            public CUMailCatalogMapperValidator(bool isAdd)
            {
                if (isAdd)
                {
                    RuleFor(d => d.CatalogName).NotEmpty().WithMessage("文件夹名称不能为空");
                }
                else
                {
                    RuleFor(d => d.CatalogId).NotNull().WithMessage("文件夹Id不能为空");
                }
            }
        }
    }

    public class DeleteMailCatalogMapper : BaseEntity
    {
        public Guid CatalogId { get; set; }

        protected override IValidator GetValidator()
        {
            return new DeleteMailCatalogMapperValidator();
        }

        class DeleteMailCatalogMapperValidator : AbstractValidator<DeleteMailCatalogMapper>
        {

            public DeleteMailCatalogMapperValidator()
            {
                RuleFor(d => d.CatalogId).NotEmpty().WithMessage("文件夹Id不能为空");
            }
        }
    }

    public class OrderByMailCatalogMapper : BaseEntity
    {
        public Guid CatalogId { get; set; }
        public Guid ChangeCatalogId { get; set; }
        protected override IValidator GetValidator()
        {
            return new OrderByMailCatalogMapperValidator();
        }

        class OrderByMailCatalogMapperValidator : AbstractValidator<OrderByMailCatalogMapper>
        {
            public OrderByMailCatalogMapperValidator()
            {
                RuleFor(d => d.CatalogId).NotEmpty().WithMessage("文件夹Id不能为空");
                RuleFor(d => d.ChangeCatalogId).NotEmpty().WithMessage("文件夹Id不能为空");
            }
        }
    }

    public class UserMailInfo
    {
        public string AccountId { get; set; }
        public string EncryptPwd { get; set; }
        public Guid ServerId { get; set; }
        public string ServerName { get; set; }
        public int Owner { get; set; }
        public string ImapAddress { get; set; }
        public int ImapPort { get; set; }
        public string SmtpAddress { get; set; }
        public int SmtpPort { get; set; }
        public int EnableSsl { get; set; }
        public Int64 Wgvhhx { get; set; }

        public string NickName { get; set; }
    }
}