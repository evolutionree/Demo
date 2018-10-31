using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.Account;

namespace UBeat.Crm.CoreApi.DingTalk.Repository
{
    public interface IDingTalkRepository
    {
        AccountUserMapper GetUserInfoforDingding(string dduserid);
        List<Dictionary<string, object>> GetEntranceList();

        AccountUserMapper GetUserInfoforDingdingByNick(string nickName);


        int DepartmentAdd(Guid departmentId, string departmentName, int ogLevel, int userNumber, long dingtalkId, long dingtalkParentId);

        int UserAdd(AccountUserRegistMapper registEntity, string dingTalkUserID, string dingTalkNick, int userNumber);

        bool IsDepartmentExist(string departmentName, long dtDepartmetnId);

        bool IsUserExist(string userMobile);

        Guid GetDepartmentId(string departmentName, long dingTalkId);

        bool IsDepartmentExist(string departmentName);

        bool IsUserExist(int userMobile);


        Guid AddGroup(String groupName, String dingDingGroupId, int userId);


        Guid AddRole(String roleName, String dingDingRoleId, int userId);

        bool AddRoleGroup(Guid groupId, Guid roleId, String dingDingGroupId, String dingDingRoleId, int userId);
    }
}