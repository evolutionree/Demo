using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Message
{
    /// <summary>
    /// 消息配置表
    /// </summary>
    public class MessageConfigInfo
    {
        /// <summary>
        /// 消息配置ID
        /// </summary>
        public Guid MsgConfigId { set; get; }
        /// <summary>
        /// 对应的实体ID
        /// </summary>
        public Guid EntityId { set; get; }
        /// <summary>
        /// 对应实体的关联实体id
        /// </summary>
        public Guid RelEntityId { set; get; }

        public Guid FlowId { set; get; }

        /// <summary>
        /// 场景编码：
        /// 实体通用部分固定如下，其他场景的由实际情况而定：
        /// 新增=EntityDataAdd
        /// 编辑 = EntityDataEdit
        /// 删除=EntityDataDelete
        /// 添加相关人 = ViewUserAdd
        /// 删除相关人=ViewUserDelete
        /// 新增动态 = EntityDynamicAdd
        /// 动态评论=EntityDynamicComment
        /// 动态点赞 = EntityDynamicPrase
        /// </summary>
        public string FuncCode { set; get; }
        /// <summary>
        /// 场景名称
        /// </summary>
        public string FuncName { set; get; }
        /// <summary>
        /// 消息分组ID，用于对消息分类
        /// </summary>
        public MessageGroupType MsgGroupId { set; get; }
        /// <summary>
        /// 消息类型
        /// </summary>
        public MessageType MsgType { set; get; }
        /// <summary>
        /// 消息样式 
        /// </summary>
        public MessageStyleType MsgStyleType { set; get; }

        /// <summary>
        /// 消息内容模板
        /// </summary>
        public string MsgTemplate { set; get; }

        /// <summary>
        /// 消息title模板
        /// </summary>
        public string TitleTemplate { set; get; }
        /// <summary>
        /// 消息手机通知模板
        /// </summary>
        public string NotifyTemplate { set; get; }
        /// <summary>
        /// 消息推送范围，格式如"1,2,3,4"等
        /// </summary>
        public string MsgUserType { set; get; }
        /// <summary>
        /// 消息推送范围（枚举对象）
        /// </summary>
        public List<MessageUserType> MessageUserType {
            get
            {
                if (MsgUserType != null)
                {
                    var args = MsgUserType.Split(',').Select(m=>int.Parse(m));
                    return args.Cast<MessageUserType>().ToList();
                }
                else return new List<MessageUserType>();
            }
        }


    }
}
