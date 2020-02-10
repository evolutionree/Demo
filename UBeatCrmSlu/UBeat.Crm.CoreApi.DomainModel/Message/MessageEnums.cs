using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Message
{
    /// <summary>
    /// 消息分组类型
    /// </summary>
    public enum MessageGroupType
    {
        Default = 0,
        SaleRecord = 1001,//销售记录
        WorkReport = 1002,//工作报告
        DayliyReport = 1003,//日报周报
        Remind = 1004,//任务提醒
        Notice = 1005,//公告通知
        WorkFlow = 1006,//审批通知
        Dynamic = 1007,//实时动态
        Chat = 1008,//实时聊天
    }

    public enum MessageType
    {
        /// <summary>
        /// 普通消息
        /// </summary>
        Message=1,
        /// <summary>
        /// 动态+离线消息
        /// </summary>
        DynamicMessage=2,
    }

    /// <summary>
    /// 消息类型
    /// </summary>
    public enum MessageStyleType
    {
        
        
        SystemNotice = 0, //0系统通知
        EntityOperate = 1,//1实体操作消息
        EntityDynamic = 2,//2实体动态消息
        DynamicPrase = 3,//3实体动态带点赞
        RedmindNotSkip = 4,//4提醒
        WorkflowAudit = 5,//5审批
        WorkReport = 6,//6工作报告
        NoticeInfo = 7,//7公告通知
        RedmindCanSkip = 8,//8提醒(进行跳转的)

        ExportRedmind = 99,//99导入结果提醒
    }

    /// <summary>
    /// 消息推送范围类型
    /// </summary>
    public enum MessageUserType
    {
        EntityManager=1,//实体负责人
        EntityOldManager=2,//实体旧负责人
        EntityViewUser = 3,//实体的相关人
        EntityOldViewUser = 4,//实体的旧相关人
        EntityViewUserAdd = 5,//实体新添加的相关人
        EntityViewUserRemove = 6,//实体被移除的相关人
        EntityMember = 7,//实体圈子成员
        EntityOldMember = 8,//实体旧圈子成员
        EntityFollowUser = 9,//实体的关注人


        WorkFlowCreateUser =100,//流程审批发起人
        WorkFlowCarbonCopyUser=101,//流程审批抄送人
        WorkFlowNextApprover=102,//流程审批下一步骤审批人
        WorkFlowCompletedApprover=103,//流程审批已完成步骤审批人
        WorkFlowInformerUser = 105,//流程审批知会人
        WorkFlowSubscriber = 106,//流程审批传阅人

        DailyCreateUser =200,//周报日报添加人
        DailyApprover =201,//周报日报批阅人
        DailyCarbonCopyUser =202,//周报日报抄送人
        DailyNewCopyUser = 203,//周报日报新增的抄送人
        DailyNewApprover=204,//周报日报新增的批阅人
        DailyCopyUserDeleted = 205,//周报日报被删除抄送人
        DailyApproverDeleted = 206,//周报日报被删除的批阅人
        DailyEditApprover = 207,//周报日报批阅人(不包含编辑新增)
        DailyEditCarbonCopyUser = 208,//周报日报抄送人(不包含编辑新增)


        SpecificUser =300,//特定人员


    }
}
