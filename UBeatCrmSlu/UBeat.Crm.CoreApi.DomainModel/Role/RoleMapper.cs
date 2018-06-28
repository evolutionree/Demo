using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace UBeat.Crm.CoreApi.DomainModel.Role
{
    public class SaveRoleGroupMapper : BaseEntity
    {
        public string RoleGroupId { get; set; }
        public string GroupName { get; set; }

        public int GroupType { get; set; }
        public string GroupLanguage { get; set; }

        protected override IValidator GetValidator()
        {
            return new SaveRoleGroupMapperValidator();
        }
        class SaveRoleGroupMapperValidator : AbstractValidator<SaveRoleGroupMapper>
        {
            public SaveRoleGroupMapperValidator()
            {
                RuleFor(d => d.GroupName).NotEmpty().WithMessage("角色分组名称不能为空");
            }
        }
    }

    public class RoleListMapper 
    {
        public int RoleType { get; set; }

        public string RoleName { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }

        public string GroupId { get; set; }
    }

    public class RoleMapper : BaseEntity
    {
        public string RoleGroupId { get; set; }
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public int RoleType { get; set; }
        public int RolePriority { get; set; }
        public string RoleRemark { get; set; }

        public string RoleLanguage { get; set; }
        protected override IValidator GetValidator()
        {
            return new RoleMapperValidator();
        }
        class RoleMapperValidator : AbstractValidator<RoleMapper>
        {
            public RoleMapperValidator()
            {
                RuleFor(d => d.RoleName).NotEmpty().WithMessage("角色名称不能为空");
                RuleFor(d => d.RoleType).NotNull().WithMessage("角色类型不能为空");
                RuleFor(d => d.RolePriority).NotNull().WithMessage("角色等级不能为空");
                RuleFor(d => d.RoleGroupId).NotEmpty().WithMessage("角色分组Id不能为空");
            }
        }
    }

    public class RoleCopyMapper : BaseEntity
    {
        public string RoleGroupId { get; set; }
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public int RoleType { get; set; }
        public int RolePriority { get; set; }
        public string RoleRemark { get; set; }
        protected override IValidator GetValidator()
        {
            return new RoleCopyMapperValidator();
        }
        class RoleCopyMapperValidator : AbstractValidator<RoleCopyMapper>
        {
            public RoleCopyMapperValidator()
            {
                RuleFor(d => d.RoleName).NotEmpty().WithMessage("角色名称不能为空");
                RuleFor(d => d.RoleType).NotNull().WithMessage("角色类型不能为空");
                RuleFor(d => d.RolePriority).NotNull().WithMessage("角色等级不能为空");
                RuleFor(d => d.RoleGroupId).NotEmpty().WithMessage("角色分组Id不能为空");
                RuleFor(d => d.RoleId).NotEmpty().WithMessage("角色Id不能为空");
            }
        }
    }

    public class AssigneUserToRoleMapper:BaseEntity
    {
        public string RoleIds { get; set; }

        public string UserIds { get; set; }

        public string FuncIds { get; set; }

        protected override IValidator GetValidator()
        {
            return new AssigneUserToRoleMapperValidator();
        }
        class AssigneUserToRoleMapperValidator : AbstractValidator<AssigneUserToRoleMapper>
        {
            public AssigneUserToRoleMapperValidator()
            {
                RuleFor(d => d.RoleIds).NotEmpty().WithMessage("角色不能为空");
                RuleFor(d => d.UserIds).NotNull().WithMessage("用户不能为空");
                RuleFor(d => d.FuncIds).NotNull().WithMessage("职能不能为空");
            }
        }
    }

    public class RoleUserMapper:BaseEntity
    {
        public string DeptId { get; set; }

        public string RoleId { get; set; }

        public string UserName { get; set; }

        public int PageIndex { get; set; }

         public int PageSize { get; set; }
        protected override IValidator GetValidator()
        {
            return new RoleUserMapperValidator();
        }
        class RoleUserMapperValidator : AbstractValidator<RoleUserMapper>
        {
            public RoleUserMapperValidator()
            {
                RuleFor(d => d.RoleId).NotNull().NotEmpty().WithMessage("角色Id不能为空");
            }
        }
    }

    public class UserRoleInfoMapper
    {
        public int UserId { get; set; }

        public string UserName { get; set; }

        ICollection<RoleInfoMapper> RoleInfoMapper { get; set; }
    }
    public class RoleInfoMapper
    {
        public string RoleId { get; set; }

        public string RoleName { get; set; }
    }

}
