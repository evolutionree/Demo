using FluentValidation;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Dynamics
{

    public class DynamicInsert: BaseEntity
    {
        public int DynamicType { set; get; }

        public Guid EntityId { set; get; }

        public Guid BusinessId { set; get; }

        public Guid TypeId { set; get; }

        public Guid TypeRecId { set; get; }

        /// <summary>
        /// JArray类型的字符串
        /// </summary>
        public string JsonData { set; get; } 

        public string Content { set; get; }

        public int UserNo { set; get; }

        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<DynamicInsert>
        {
            public Validator()
            {
                RuleFor(d => d.Content).NotEmpty().NotNull().When(m => m.DynamicType == 0).WithMessage("内容不可为空");
                RuleFor(d => d.EntityId).NotEmpty().NotNull().When(m=>m.DynamicType!=0).WithMessage("EntityId不能为空");
                RuleFor(d => d.JsonData).NotEmpty().NotNull().When(m => m.DynamicType != 0).WithMessage("内容（JArray类型）不能为空");
                RuleFor(d => d.UserNo).GreaterThan(0).WithMessage("UserNo无效，请重新登录");
            }
        }
    }

    public class DynamicSelect: BaseEntity
    {
        public Guid? Businessid { set; get; }
        public List<int> DynamicTypes { set; get; }
        public Guid EntityId { set; get; } = Guid.Empty;

        public int UserNo { set; get; }

        public Int64 RecVersion { get; set; }
        /// <summary>
        /// 点赞记录的版本
        /// </summary>
        public Int64 PraiseRecVersion { get; set; }
        /// <summary>
        /// 评论记录的版本
        /// </summary>
        public Int64 CommentRecVersion { get; set; }

        /// <summary>
        /// 记录状态过滤条件，-1时，不使用该条件
        /// </summary>
        public int RecStatus { set; get; } = 1;

        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<DynamicSelect>
        {
            public Validator()
            {
                RuleFor(d => d.UserNo).GreaterThan(0).WithMessage("UserNo无效，请重新登录");
                RuleFor(d => d.RecVersion).NotNull().GreaterThanOrEqualTo(0).WithMessage("版本ID不能为空");
            }
        }
    }

    public class DynamicDelete: BaseEntity
    {
        public Guid DynamicId { set; get; }

        public int UserNo { set; get; }

        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<DynamicDelete>
        {
            public Validator()
            {
                RuleFor(d => d.DynamicId).NotNull().WithMessage("DynamicId不能为空");
                RuleFor(d => d.UserNo).GreaterThan(0).WithMessage("UserNo无效，请重新登录");
            }
        }
    }
}
