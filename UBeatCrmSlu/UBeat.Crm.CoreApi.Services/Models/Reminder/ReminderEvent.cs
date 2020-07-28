using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.Services.Models.Rule;
using UBeat.Crm.CoreApi.Services.Models.Vocation;

namespace UBeat.Crm.CoreApi.Services.Models.Reminder
{
    public class ReminderEventAddModel
    {
        public Guid EventId { get; set; }

        public string EventName { get; set; }

        public Guid EntityId { get; set; }

        public string Title { get; set; }

        public string ReminderContent { get; set; }

        public int CheckDay { get; set; }

        public string SendTime { get; set; }

        public int Rectype { get; set; }

        public string FuncName { get; set; }

        public Guid ExpandFieldId { get; set; }

        public string Param { get; set; }

        public string UserColumn { get; set; }

        public int RemindType { get; set; }
        public string TimeFormat { get; set; }

        public int RecStatus { get; set; }
    }



    public class ReminderEventEditModel
    {
        public Guid EventId { get; set; }

        public string EventName { get; set; }

        public Guid EntityId { get; set; }

        public string Title { get; set; }

        public string ReminderContent { get; set; }

        public int CheckDay { get; set; }

        public string SendTime { get; set; }

        public int RecType { get; set; }

        public string FuncName { get; set; }

        public Guid ExpandFieldId { get; set; }

        public string Param { get; set; }

        public string UserColumn { get; set; }

        public int RemindType { get; set; }
        public string TimeFormat { get; set; }
    }


    public class ReminderEventSetStatussModel
    {
        public List<string> EventIds { get; set; }
        public int Status { get; set; }
    }


    public class ReminderEventListModel
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }

    }


    public class ReminderEventGetModel
    {
        public Guid EventId { get; set; }


    }

    public class ReminderEventDeleteModel
    {
        public List<string> EventIds { get; set; }
    }

    public class ReminderEventActivateModel
    {

        public Guid? BusRecId { get; set; }
        public string RemindId { get; set; }

    }

    public class ReminderListModel
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
        public int PageIndex { get; set; }
        public int PageSize { get; set; }

    }

    public class ReminderSaveModel
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
        public Guid EntityId { get; set; }
        public bool IsRepeat { get; set; }
        public int RecStatus { get; set; }
        public int RepeatType { get; set; }
        public string CronString { get; set; }
        public string Remark { get; set; }

        public Dictionary<string, string> ReminderName_Lang { get; set; }

    }

    public class ReminderSelectModel
    {
        public Guid ReminderId { get; set; }

    }


    public class ReminderSaveRuleModel
    {
        public Guid ReminderId { get; set; }

        public Guid EntityId { get; set; }

        public int TypeId { get; set; }//0 代表菜单 1代表角色 2代表动态实体
        public string Id { get; set; }// 代表规则关联的某实体唯一id
        public string RoleId { get; set; }
        public string MenuName { get; set; }
        public string RuleId { get; set; }
        public string RuleName { get; set; }

        public string RelEntityId { get; set; }
        public string Rulesql { get; set; }

        public RuleContent Rule { get; set; }
        public ICollection<RuleItemModel> RuleItems { get; set; }
        public RuleSetModel RuleSet { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public string ContentParam { get; set; }

        public List<ReminderReceiverModel> Receiver { get; set; }

        public List<ReminderRecycleRuleModel> UpdateField { get; set; }


        public int? ReceiverRange { get; set; }

    }


    public class ReminderReceiverModel
    {
        /// <summary>
        ///  0 固定人，1表单中人，2 固定部门，3 表单中部门
        public int ItemType { get; set; }
        public int? UserId { get; set; }
        public Guid? UserField { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? DepartmentField { get; set; }
    }



    public class ReminderDisableModel
    {
        public Guid ReminderId { get; set; }
        public Guid EntityRecId { get; set; }
		public int	ReminderStatus { get; set; }

    }



    public class RuleSaveModel
    {

        public Guid EntityId { get; set; }

        public int TypeId { get; set; }//0 代表菜单 1代表角色 2代表动态实体
        public string Id { get; set; }// 代表规则关联的某实体唯一id
        public string RoleId { get; set; }
        public string MenuName { get; set; }
        public string RuleId { get; set; }
        public string RuleName { get; set; }

        public string RelEntityId { get; set; }
        public string Rulesql { get; set; }

        public RuleContent Rule { get; set; }
        public ICollection<RuleItemModel> RuleItems { get; set; }
        public RuleSetModel RuleSet { get; set; }

    }


    public class ReminderRecycleRuleModel
    {

        public Guid FieldId { get; set; }
        public string FieldValue { get; set; }
    }



    public class ReminderDetailModel
    {

        public string RuleId { get; set; }
        public string RuleName { get; set; }
        public string EntityId { get; set; }
        public string Rulesql { get; set; }

        public ICollection<RuleItemInfoModel> RuleItems { get; set; }
        public RuleSetInfoModel RuleSet { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public List<ReminderReceiverModel> Receiver { get; set; }

        public List<ReminderRecycleRuleModel> UpdateField { get; set; }


        public bool HasPerson { get; set; }
        public bool IsPersonFixed { get; set; }
        public bool HasDepartment { get; set; }
        public bool IsDepartmentFixed { get; set; }

        public string ContentParam { get; set; }

        public int ReceiverRange { get; set; }

    }



}
