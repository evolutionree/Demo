using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Message
{
    public class MsgParamInfo
    {
        public object Template { set; get; }

        /// <summary>
        /// 数据对象：
        /// 有模板时，为模板数据
        /// 公告通知时，为通告通知的数据对象，目前约定为headimg字段的数据
        /// 任务提醒时，为reminderid字段的数据
        /// </summary>
        public object Data { set; get; }

        /// <summary>
        /// 实体ID
        /// </summary>
        public Guid EntityId { set; get; }

        /// <summary>
        /// 对应的实体名称
        /// </summary>
        public string EntityName { set; get; }

        /// <summary>
        /// 实体类型ID
        /// </summary>
        public Guid TypeId { set; get; }
        /// <summary>
        /// 业务id，如客户id
        /// </summary>
        public Guid BusinessId { set; get; }
        /// <summary>
        /// 关联实体id
        /// </summary>
        public Guid RelEntityId { set; get; }

        /// <summary>
        /// 对应的关联实体名称
        /// </summary>
        public string RelEntityName { set; get; }

        /// <summary>
        /// 关联实体业务id
        /// </summary>
        public Guid RelBusinessId { set; get; }

       
        /// <summary>
        /// 抄送人
        /// </summary>
        public List<int> CopyUsers { set; get; }


        /// <summary>
        /// 批阅人
        /// </summary>
        public List<int> ApprovalUsers { set; get; } 

        /// <summary>
        /// 通知人，工作流专用
        /// </summary>
        public List<int> NoticeUsers { get; set; }
    }
}
