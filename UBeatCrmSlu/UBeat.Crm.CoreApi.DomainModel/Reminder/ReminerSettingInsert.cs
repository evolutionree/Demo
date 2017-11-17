using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Reminder
{
    public class ReminerSettingInsert : BaseEntity
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public int CheckDay { get; set; }

        public string CronString { get; set; }

        public string ConfigVal { get; set; }

        public int RecStatus { get; set; }

        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<ReminerSettingInsert>
        {
            public Validator()
            {
                RuleFor(d => d.Id).NotNull().WithMessage("ID不能为空");
                RuleFor(d => d.Name).NotNull().WithMessage("Name不能为空");
                RuleFor(d => d.CheckDay).NotNull().WithMessage("CheckDay不可为空");
                RuleFor(d => d.CronString).NotNull().WithMessage("CronString不可为空");
                RuleFor(d => d.ConfigVal).NotNull().WithMessage("ConfigVal不可为空");
                RuleFor(d => d.RecStatus).NotNull().WithMessage("IsValid不可为空");

            }
        }
    }


    public class ReminderListMapper : BaseEntity
    {
        public string ReminderName { get; set; }

        /// <summary>
        /// 0 不启用 1 启用
        /// </summary>
        public int RecStatus { get; set; }

        /// <summary>
        /// 0 智能提醒
        /// </summary>
        public int RecType { get; set; }

        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<ReminderListMapper>
        {
            public Validator()
            {
                RuleFor(d => d.ReminderName).NotNull().WithMessage("提醒名称不能为空");

            }
        }

    }


    public class ReminderSaveMapper : BaseEntity
    {

        /// <summary>
        ///null 新增， 非null 编辑
        /// </summary>
        public Guid? ReminderId { get; set; }

        /// <summary>
        /// 0 智能提醒， 1 回收机制
        /// </summary>

        public int RecType { get; set; }
        public string ReminderName { get; set; }
        public Guid EndityId { get; set; }
        public bool IsRepeat { get; set; }
        public int RecStatus { get; set; }
        public int RepeatType { get; set; }
        public string CronString { get; set; }
        public string Remark { get; set; }


        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<ReminderSaveMapper>
        {
            public Validator()
            {
                RuleFor(d => d.ReminderName).NotNull().WithMessage("提醒名称不能为空");
            }
        }

    }

    public class ReminderSelectMapper : BaseEntity
    {
        public Guid ReminderId { get; set; }

        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<ReminderSelectMapper>
        {
            public Validator()
            {
                RuleFor(d => d.ReminderId).NotNull().WithMessage("ID不能为空");

            }
        }

    }


    public class ReminderSaveRuleMapper : BaseEntity
    {
        public Guid ReminderId { get; set; }

        public string Title { get; set; }
        public string Content { get; set; }

        public string Receiver { get; set; }

        public bool HasPerson { get; set; }
        public bool IsPersonFixed { get; set; }
        public bool HasDepartment { get; set; }
        public bool IsDepartmentFixed { get; set; }


        /// <summary>
        /// 回收执行修改数据属性
        /// </summary>
        public string UpdateField { get; set; }


        /// <summary>
        /// 消息模板中的参数
        /// </summary>
        public string ContentParam { get; set; }


        /// <summary>
        /// 选择了部门，接受消息的人的范围：0 部门领导，1 全部人
        /// </summary>
        public int? ReceiverRange { get; set; }

        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<ReminderSaveRuleMapper>
        {
            public Validator()
            {
                RuleFor(d => d.ReminderId).NotNull().WithMessage("id不能为空");

            }
        }
    }

    public class RuleInsertMapper : BaseEntity
    {
        public string Rule { get; set; }
        public string RuleItem { get; set; }
        public string RuleSet { get; set; }

        public string RuleRelation { get; set; }

        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<RuleInsertMapper>
        {
            public Validator()
            {
                RuleFor(d => d.Rule).NotNull().WithMessage("规则不能为空");
                RuleFor(d => d.RuleItem).NotNull().WithMessage("规则明细不能为空");
                RuleFor(d => d.RuleSet).NotNull().WithMessage("规则设置不能为空");


            }
        }
    }




    public class ReminderReceiverMapper
    {
        /// <summary>
        ///  0 固定人，1表单中人，2 固定部门，3 表单中部门
        public int ItemType { get; set; }
        public int? UserId { get; set; }
        public Guid? UserField { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? DepartmentField { get; set; }

    }



    public class ReminderDisableMapper : BaseEntity
    {
        public Guid ReminderId { get; set; }
        public Guid EntityRecId { get; set; }

        public int ReminderStatus { get; set; }

        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<ReminderDisableMapper>
        {
            public Validator()
            {
                RuleFor(d => d.ReminderId).NotNull().WithMessage("提醒id不能为空");

                RuleFor(d => d.EntityRecId).NotNull().WithMessage("实体记录Id不能为空");

                RuleFor(d => d.ReminderStatus).NotNull().WithMessage("提醒状态不能为空");
            }
        }
    }



    public class ReminderRuleDetailMapper
    {
        public string EntityId { get; set; }
        public string RuleId { get; set; }
        public string RuleName { get; set; }
        public int RecStatus { get; set; }
        public string ItemId { get; set; }
        public string FieldId { get; set; }
        public string ItemName { get; set; }
        public string Operate { get; set; }
        public string RuleSql { get; set; }
        public int UseType { get; set; }
        public int RuleType { get; set; }
        public string RuleData { get; set; }
        public string RuleSet { get; set; }

        public bool HasPerson { get; set; }
        public bool IsPersonFixed { get; set; }
        public bool HasDepartment { get; set; }
        public bool IsDepartmentFixed { get; set; }

        public string TemplateTitle { get; set; }
        public string TemplateContent { get; set; }
        public string ContentParam { get; set; }
        public int ReceiverRange { get; set; }


    }


    public class ReminderRecieverUserMapper
    {
        public Guid ReminderId { get; set; }
        public bool IsEntityField { get; set; }
        public Guid? FieldId { get; set; }
        public int? UserId { get; set; }
    }


    public class ReminderRecieverDepartmentMapper
    {
        public Guid ReminderId { get; set; }
        public bool IsEntityField { get; set; }
        public Guid? FieldId { get; set; }
        public Guid? DepartmentId { get; set; }
    }


    public class ReminderRecycleRuleMapper
    {

        public Guid FieldId { get; set; }
        public string FieldValue { get; set; }
    }


    public class ReminderMapper
    {

        public Guid ReminderId { get; set; }

        public bool IsRepeat { get; set; }
        public int RepeatType { get; set; }

        public int RecStatus { get; set; }

        public string CronString { get; set; }

        public Int64 RecVersion { get; set; }

        public string ReminderName { get; set; }
    }




}


