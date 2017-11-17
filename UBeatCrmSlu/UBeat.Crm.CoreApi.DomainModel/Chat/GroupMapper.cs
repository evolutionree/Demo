using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Chat
{
    public class GroupInsert : BaseEntity
    {
        public string GroupName { set; get; }

        public string Pinyin { set; get; }

        /// <summary>
        /// 分组类型 1：讨论群  2：部门群  3：商机群
        /// </summary>
        public int GroupType { set; get; }

        public Guid EntityId { set; get; }

        public Guid BusinessId { set; get; }
        /// <summary>
        /// 群头像
        /// </summary>
        public string GroupIcon { set; get; }

        /// <summary>
        /// 成员id，如用户id
        /// </summary>
        public List<int> MemberIds { set; get; }

        public int UserNo { set; get; }

        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<GroupInsert>
        {
            public Validator()
            {
                RuleFor(d => d.GroupName).NotNull().NotEmpty().WithMessage("GroupName不能为空");
                RuleFor(d => d.UserNo).GreaterThan(0).WithMessage("UserNo无效，请重新登录");
            }
        }
    }

    public class GroupUpdate : BaseEntity
    {
        public Guid GroupId { set; get; }
        public string GroupName { set; get; }

        public string Pinyin { set; get; }

        /// <summary>
        /// 群头像
        /// </summary>
        public string GroupIcon { set; get; }
        public int UserNo { set; get; }

        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<GroupUpdate>
        {
            public Validator()
            {
                RuleFor(d => d.GroupId).NotNull().NotEmpty().WithMessage("GroupId不能为空");
                RuleFor(d => d.GroupName).NotNull().NotEmpty().WithMessage("GroupName不能为空");
                RuleFor(d => d.UserNo).GreaterThan(0).WithMessage("UserNo无效，请重新登录");
            }
        }
    }
    public class GroupMemberSet : BaseEntity
    {
        public Guid GroupId { set; get; } 

        public int Memberid { set; get; }

        /// <summary>
        /// 操作类型：0为设置管理员，1为取消管理员，2为设置屏蔽群，3为取消屏蔽群，4为管理员踢人
        /// </summary>
        public int OperateType { set; get; }

        public int UserNo { set; get; }

        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<GroupMemberSet>
        {
            public Validator()
            {
                RuleFor(d => d.GroupId).NotNull().NotEmpty().WithMessage("GroupId不能为空");
                RuleFor(d => d.Memberid).NotNull().NotEmpty().WithMessage("Memberid不能为空");
                RuleFor(d => d.UserNo).GreaterThan(0).WithMessage("UserNo无效，请重新登录");
            }
        }
    }
    public class GroupMemberAdd : BaseEntity
    {
        public Guid GroupId { set; get; }
        
        public List<int> Members { set; get; }

        public int UserNo { set; get; }

        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<GroupMemberAdd>
        {
            public Validator()
            {
                RuleFor(d => d.GroupId).NotNull().NotEmpty().WithMessage("GroupId不能为空");
                RuleFor(d => d.Members).NotNull().NotEmpty().WithMessage("Members不能为空");
                RuleFor(d => d.UserNo).GreaterThan(0).WithMessage("UserNo无效，请重新登录");
            }
        }
    }
    public class GroupMemberSelect : BaseEntity
    {
        public Guid GroupId { set; get; }
        /// <summary>
        /// 版本号
        /// </summary>
        public long MaxRecVersion { set; get; }

        public int UserNo { set; get; }

        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<GroupMemberSelect>
        {
            public Validator()
            {
                RuleFor(d => d.GroupId).NotNull().NotEmpty().WithMessage("GroupId不能为空");
                RuleFor(d => d.UserNo).GreaterThan(0).WithMessage("UserNo无效，请重新登录");
            }
        }
    }

    public class GroupDelete : BaseEntity
    {
        public Guid GroupId { set; get; }
        /// <summary>
        /// 操作类型，0为主动退群,1为管理员解散群
        /// </summary>
        public int OperateType { set; get; }

        public int UserNo { set; get; }
        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<GroupDelete>
        {
            public Validator()
            {
                RuleFor(d => d.GroupId).NotNull().NotEmpty().WithMessage("GroupId不能为空");
                RuleFor(d => d.OperateType).InclusiveBetween(0,1).WithMessage("OperateType只能为0或者1");
                RuleFor(d => d.UserNo).GreaterThan(0).WithMessage("UserNo无效，请重新登录");
            }
        }
    }

    //
    public class GroupSelect : BaseEntity
    {
        /// <summary>
        /// 分组类型 1：讨论群  2：部门群  3：商机群
        /// </summary>
        public int GroupType { set; get; }

        public Guid EntityId { set; get; }

        public Guid BusinessId { set; get; }
        /// <summary>
        /// 版本号
        /// </summary>
        public long MaxRecVersion { set; get; }
        public int UserNo { set; get; }
        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<GroupSelect>
        {
            public Validator()
            {
                RuleFor(d => d.GroupType).GreaterThan(0).WithMessage("GroupId不能为空");
                RuleFor(d => d.UserNo).GreaterThan(0).WithMessage("UserNo无效，请重新登录");
            }
        }
    }

    

}
