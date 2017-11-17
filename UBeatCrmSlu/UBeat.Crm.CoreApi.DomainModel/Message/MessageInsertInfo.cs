using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Message
{
    public class MessageInsertInfo
    {
        
        /// <summary>
        /// 对应的实体ID
        /// </summary>
        public Guid EntityId { set; get; }
        /// <summary>
        /// 具体数据的ID，来源的
        /// </summary>
        public Guid BusinessId { set; get; }
        /// <summary>
        /// 消息分组ID，用于对消息分类
        /// </summary>
        public MessageGroupType MsgGroupId { set; get; }
        /// <summary>
        /// 消息样式 
        /// </summary>
        public MessageStyleType MsgStyleType { set; get; }
        /// <summary>
        /// 消息标题
        /// </summary>
        public string MsgTitle { set; get; }
        /// <summary>
        /// 消息提示内容-例如审批的"待办"(暂时启用)
        /// </summary>
        public string MsgTitleTip { set; get; }
        /// <summary>
        /// 消息内容
        /// </summary>
        public string MsgContent { set; get; }

        string msgpParam;
        /// <summary>
        /// 消息参数,以键值对存入,传递给手机端,固定格式
        /// </summary>
        public string MsgpParam
        {
            set { msgpParam = value; }
            get
            {
                if (string.IsNullOrEmpty(msgpParam))
                    msgpParam = "{}";
                return msgpParam;
            }
        }
        
        /// <summary>
        /// 接收人id
        /// </summary>
        public List<int> ReceiverIds { set; get; }
    }

}
