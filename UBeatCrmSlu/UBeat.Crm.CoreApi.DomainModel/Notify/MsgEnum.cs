using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Notify
{
    /// <summary>
    /// 消息分组类型
    /// </summary>
    public enum MsgEnum
    {
        Default=0,
        SaleRecord = 1001,//销售记录
        WorkReport = 1002,//工作报告
        DayliyReport = 1003,//日报周报
        Remind = 1004,//任务提醒
        Notice = 1005,//公告通知
        WorkFlow = 1006,//审批通知
        Dynamic = 1007,//实时动态
        Chat = 1008,//实时聊天
    }

    /// <summary>
    /// 消息类型
    /// </summary>
    public enum MsgTypeEnum
    {
        //0系统通知 1实体操作消息 2实体动态消息 3实体动态带点赞  4提醒 5审批 6工作报告 7公告通知 99导入结果提醒
        SystemNotice = 0, //0系统通知
        EntityOperate=1,//1实体操作消息
        EntityDynamic=2,//2实体动态消息
        DynamicPrase=3,//3实体动态带点赞
        Redmind=4,//4提醒
        WorkflowAudit=5,//5审批
        WorkReport=6,//6工作报告
        NoticeInfo=7,//7公告通知

        ExportRedmind=99,//99导入结果提醒
    }
}
