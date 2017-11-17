using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.EntityPro;

namespace UBeat.Crm.CoreApi.DomainModel.Message
{


    public class MessageInfo
    {
        /// <summary>
        /// 消息ID,若没有手动生成，则新增时会自动生成
        /// </summary>
        public Guid MsgId { set; get; }
        /// <summary>
        /// 对应的实体ID
        /// </summary>
        public Guid EntityId { set; get; }

        /// <summary>
        /// 对应的实体ID
        /// </summary>
        public string EntityName { set; get; }

        /// <summary>
        /// 对应的实体模型
        /// </summary>
        public EntityModelType EntityModel { set; get; }

        /// <summary>
        /// 具体数据的ID，来源的
        /// </summary>
        public Guid BusinessId { set; get; }
        /// <summary>
        /// 消息分组ID，用于对消息分类
        /// </summary>
        public MessageGroupType MsgGroupId { set; get; }
        /// <summary>
        /// 消息样式 0系统通知 1实体操作消息 2实体动态消息 3实体动态带点赞  4提醒 5审批 6工作报告 7公告通知  99导入结果提醒'
        /// </summary>
        public MessageStyleType MsgStyleType { set; get; }
        /// <summary>
        /// 消息标题
        /// </summary>
        public string MsgTitle { set; get; }
        /// <summary>
        /// 消息提示内容-例如审批的"待办"
        /// </summary>
        public string MsgTitleTip { set; get; }
        /// <summary>
        /// 消息内容
        /// </summary>
        public string MsgContent { set; get; }
        /// <summary>
        /// 消息参数,以键值对存入,传递给手机端,固定格式
        /// </summary>
        public MsgParamInfo MsgParam { set; get; }
        /// <summary>
        /// 消息下发时间
        /// </summary>
        public DateTime SendTime { set; get; } = DateTime.Now;
        /// <summary>
        /// 状态 1启用 0停用
        /// </summary>
        public int RecStatus { set; get; } = 1;
        /// <summary>
        /// 版本号,自动生成，新增时不需赋值
        /// </summary>
        public long RecVersion { set; get; }
        /// <summary>
        /// 创建人
        /// </summary>
        public int RecCreator { set; get; }

        /// <summary>
        /// 创建人名称
        /// </summary>
        public string RecCreatorName { set; get; }

        /// <summary>
        /// 创建人头像
        /// </summary>
        public string RecCreatorIcon { set; get; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime RecCreated { set; get; } = DateTime.Now;

       
        /// <summary>
        /// 接收人ID
        /// </summary>
        public int ReceiverId { set; get; }

        /// <summary>
        /// 是否已读，0未读 1已查，2已读
        /// </summary>
        public MessageReadStatus ReadStatus { set; get; }
    }
   
}
