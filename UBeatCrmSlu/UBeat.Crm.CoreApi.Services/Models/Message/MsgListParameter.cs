using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Message;

namespace UBeat.Crm.CoreApi.Services.Models.Message
{
    public class MsgsParameter
    {
        /// <summary>
        /// 实体id，只查询某个实体的消息
        /// </summary>
        public Guid? EntityId { set; get; }
        /// <summary>
        /// 实体记录ID，只查询某个实体某个业务数据的消息
        /// </summary>
        public Guid? BusinessId { set; get; }
        /// <summary>
        /// 消息分组，只查询某些消息分组的消息列表
        /// </summary>
        public List<MessageGroupType> MsgGroupIds { set; get; }

        /// <summary>
        /// 消息样式 
        /// </summary>
        public List<MessageStyleType> MsgStyleTypes { set; get; } 
        
    }

    public class IncrementMsgsParameter: MsgsParameter
    {
        /// <summary>
        /// 增量的依据版本号
        /// </summary>
        public long RecVersion { set; get; }
        /// <summary>
        /// 增量取值方向
        /// </summary>
        public IncrementDirection Direction { set; get; } = IncrementDirection.Backward;
        /// <summary>
        /// 当前增量大数据块大小，默认-1表示不划分数据块，取direction方向的所有数据
        /// </summary>
        public int PageSize { set; get; } = -1;
    }

    public class PageMsgsParameter : MsgsParameter
    {
        /// <summary>
        /// 分页页码，1开始
        /// </summary>
        public int PageIndex { set; get; }
        /// <summary>
        /// 分页大小
        /// </summary>
        public int PageSize { set; get; }
    }

    public class MsgStuausParameter
    {
        public int MsgGroupId { set; get; }
        public string MessageIds { get; set; }
    }

    public enum MsgStatus
    {
        Unread = 0, //未读
        Checked = 1, //已查
        Read = 2, //已读
        Del = 3 //删除
    }
}
