using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Reminder;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IReminderRepository
    {

        dynamic GetReminderSettingList(int userNumber);

        List<IDictionary<string, object>> getReminderSettingItems(int dicTypeid);

        void UpdateDocumentFolder(IList<ReminerSettingInsert> data, int userNumber);

        void UpdateReminderItem(List<ReminderItemInsert> data, IList<int> typeIdList);


        OperateResult AddCustomReminder(ReminerEventInsert data);

        OperateResult UpdateCustomReminder(ReminerEventUpdate data);

        OperateResult DeleteCustomReminder(List<string> eventids, int usernumber);


        void SetCustomReminderEnable(List<string> eventids, int status, int usernumber);

        List<IDictionary<string, object>> CustomReminderInfo(string eventid, int usernumber);

        List<IDictionary<string, object>> ReminderMessageList(int pageIndex, int pageSize, int usernumber);

        dynamic GetReminderList(PageParam page, ReminderListMapper data, int userNumber);

        OperateResult SaveReminder(ReminderSaveMapper data, int usernumber);

        IDictionary<string, object> GetReminder(ReminderSelectMapper body, int usernumber);

        OperateResult SaveReminderRule(ReminderSaveRuleMapper data, RuleInsertMapper ruleData, int usernumber);

        List<ReminderRuleDetailMapper> GetReminderRule(ReminderSelectMapper body, int usernumber);

        OperateResult DisableReminder(ReminderDisableMapper data, int usernumber);

        List<ReminderRecieverUserMapper> GetReminderReceiverUser(ReminderSelectMapper body, int usernumber);

        List<ReminderRecieverDepartmentMapper> GetReminderReceiverDepartment(ReminderSelectMapper body, int usernumber);

        List<ReminderRecycleRuleMapper> GetReminderRecycleRule(ReminderSelectMapper body, int usernumber);

         ReminderMapper GetReminderById(Guid id, int usernumber);


    }



}

