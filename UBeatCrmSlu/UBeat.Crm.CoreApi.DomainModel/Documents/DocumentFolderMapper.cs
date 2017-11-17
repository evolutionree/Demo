using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace UBeat.Crm.CoreApi.DomainModel.Documents
{
    public class DocumentFolderInsert: BaseEntity
    {
        public string EntityId { set; get; }
        public string Foldername { set; get; }
        public string Pfolderid { set; get; }
        public int IsAllVisible { set; get; }

        public List<string> PermissionIds { set; get; } = new List<string>();


        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<DocumentFolderInsert>
        {
            public Validator()
            {
                RuleFor(d => d.EntityId).NotNull().NotEmpty().WithMessage("EntityId不能为空");
                RuleFor(d => d.Foldername).NotNull().NotEmpty().WithMessage("目录名称不能为空");
                RuleFor(d => d.Pfolderid).NotNull().NotEmpty().WithMessage("Pfolderid不能为空");
            }
        }
    }

    public class DocumentFolderUpdate : BaseEntity
    {
        public string FolderName { set; get; }
        public string PfolderId { set; get; }
        public string FolderId { set; get; }
        
        public int IsAllVisible { set; get; }

        public List<string> PermissionIds { set; get; } = new List<string>();

        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<DocumentFolderUpdate>
        {
            public Validator()
            {
                RuleFor(d => d.FolderId).NotNull().NotEmpty().WithMessage("FolderId不能为空");
                
            }
        }
    }

    public class DocumentFolderList : BaseEntity
    {
        /// <summary>
        /// 文件目录ID，没有则为空字符串
        /// </summary>
        public string FolderId { set; get; } = "";

        public string EntityId { set; get; }


        /// <summary>
        /// UPPER   OR   DOWNER
        /// </summary>
        public string Direction { set; get; } = "DOWNER";

        public Int64 RecVersion { get; set; }

        /// <summary>
        /// 记录状态过滤条件，-1时，不使用该条件
        /// </summary>
        public int RecStatus { set; get; } = 1;
        /// <summary>
        /// 设备类型，web=0，手机端=1，
        /// 如果是web，则只返回自己创建的目录和所在部门有权范围内的目录
        /// 由于增量接口无法保证数据完整，手机端的返回所有数据，手机端自己处理过滤逻辑
        /// </summary>
        public int Servicetype { set; get; }


        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<DocumentFolderList>
        {
            public Validator()
            {
                RuleFor(d => d.EntityId).NotNull().NotEmpty().WithMessage("EntityId不能为空");
                RuleFor(d => d.RecVersion).NotNull().GreaterThanOrEqualTo(0).WithMessage("版本ID不能为空");
            }
        }
    }


}
