using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.EntityPro;
using UBeat.Crm.CoreApi.DomainModel.Message;

namespace UBeat.Crm.CoreApi.Services.Models.Message
{
   

    public class MessageParameter
    {
        /// <summary>
        /// 场景编码：FuncCode
        /// </summary>
        public string FuncCode { set; get; }

        /// <summary>
        /// 对应的实体ID
        /// </summary>
        public Guid EntityId { set; get; }

        /// <summary>
        /// 对应的实体名称
        /// </summary>
        public string EntityName { set; get; }

        /// <summary>
        /// 对应的实体ID
        /// </summary>
        public Guid TypeId { set; get; }
        /// <summary>
        /// 实体业务数据的ID
        /// </summary>
        public Guid BusinessId { set; get; }
       
        /// <summary>
        /// 对应的实体的关联实体
        /// </summary>
        public Guid? RelEntityId { set; get; }

        /// <summary>
        /// 对应的关联实体名称
        /// </summary>
        public string RelEntityName { set; get; }

        /// <summary>
        /// 对应的实体的关联实体数据的业务ID
        /// </summary>
        public Guid RelBusinessId { set; get; }

        public Guid? FlowId { set; get; }
        /// <summary>
        /// 消息参数Json字符串,如动态的数据对象，不包含模板定义
        /// 有模板时，为模板数据
        /// 公告通知时，为通告通知的数据对象，目前约定为headimg字段的数据
        /// 任务提醒时，为reminderid字段的数据
        /// </summary>
        public string ParamData { set; get; }

        /// <summary>
        /// 消息模板占位符的数据字典
        /// </summary>
        public Dictionary<string, string> TemplateKeyValue { set; get; } = new Dictionary<string, string>();

        /// <summary>
        /// 接收人id
        /// </summary>
        public Dictionary<MessageUserType, List<int>> Receivers { set; get; } = new Dictionary<MessageUserType, List<int>>();


        /// <summary>
        /// 抄送人
        /// </summary>
        public List<int> CopyUsers { set; get; } = new List<int>();
        /// <summary>
        /// 批阅人
        /// </summary>
        public List<int> ApprovalUsers { set; get; } = new List<int>();


    }
}
