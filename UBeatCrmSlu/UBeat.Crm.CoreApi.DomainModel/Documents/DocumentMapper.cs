using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace UBeat.Crm.CoreApi.DomainModel.Documents
{
    public class DocumentInsert:BaseEntity
    {
        public string EntityId { set; get; }
        public string BusinessId { set; get; }

        public string FileId { set; get; }

        public string FileName { set; get; }

        public long FileLength { set; get; }

        public string FolderId { set; get; }

        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<DocumentInsert>
        {
            public Validator()
            {
                RuleFor(d => d.EntityId).NotNull().NotEmpty().WithMessage("EntityId不能为空");
                RuleFor(d => d.FileId).NotNull().NotEmpty().WithMessage("文件id不能为空");
                RuleFor(d => d.FileName).NotNull().NotEmpty().WithMessage("文件名称不能为空");
                RuleFor(d => d.FolderId).NotNull().NotEmpty().WithMessage("文件所属目录ID不能为空");
                RuleFor(d => d.FileLength).GreaterThanOrEqualTo(0).WithMessage("文件长度不能小于0");
            }
        }
    }
    public class DocumentMove: BaseEntity
    {
        public string DocumentId { set; get; }
        /// <summary>
        /// 文件目录ID，没有则为null
        /// </summary>
        public string FolderId { set; get; }
        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<DocumentMove>
        {
            public Validator()
            {
                RuleFor(d => d.DocumentId).NotNull().NotEmpty().WithMessage("文档id不能为空");
                RuleFor(d => d.FolderId).NotNull().NotEmpty().WithMessage("文件目录id不能为空");
            }
        }

    }
    public class DocumentList: BaseEntity
    {

        public string EntityId { set; get; }

        /// <summary>
        /// 业务类型的ID
        /// </summary>
        public string BusinessId { set; get; }
        /// <summary>
        /// 文件目录ID，没有则为空字符串
        /// </summary>
        public string FolderId { set; get; }
        /// <summary>
        /// 是否所有目录的文档，包含所有子目录的文档
        /// </summary>
        public bool IsAllDocuments { set; get; }
        /// <summary>
        /// 如果获取目录时，指定目录的版本
        /// </summary>
        public Int64 FolderRecVersion { get; set; }
        public Int64 RecVersion { get; set; }

        /// <summary>
        /// 记录状态过滤条件，-1时，不使用该条件
        /// </summary>
        public int RecStatus { set; get; } = 1;


        /// <summary>
        /// 搜索关键字
        /// </summary>
        public string SearchKey { set; get; }

        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<DocumentList>
        {
            public Validator()
            {
                RuleFor(d => d.EntityId).NotNull().NotEmpty().WithMessage("EntityId不能为空");
                RuleFor(d => d.RecVersion).NotNull().GreaterThanOrEqualTo(0).WithMessage("版本ID不能为空");
            }
        }

    }
}
