using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Dynamics
{
    public class DynamicInfo
    {
        /// <summary>
        /// 记录id
        /// </summary>
        public Guid DynamicId { set; get; }
        /// <summary>
        /// 实体ID
        /// </summary>
        public Guid EntityId { set; get; }
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
        /// 关联实体业务id
        /// </summary>
        public Guid RelBusinessId { set; get; }
        /// <summary>
        /// 动态类型：1=系统，2=实体
        /// </summary>
        public DynamicType DynamicType { set; get; }
        /// <summary>
        /// 模板id
        /// </summary>
        public Guid TemplateId { set; get; }

        /// <summary>
        /// 动态模板内容
        /// </summary>
        public List<Dictionary<string, object>> TempContent { set; get; }

        /// <summary>
        /// 系统和实体对应动态的内容数据Json
        /// </summary>
        public Dictionary<string, object> Tempdata { set; get; }
        /// <summary>
        /// 动态类型为默认动态类型时的动态内容
        /// </summary>
        public string Content { set; get; }
        /// <summary>
        /// 状态 0停用 1启用
        /// </summary>
        public int Recstatus { set; get; }
        /// <summary>
        /// 创建人
        /// </summary>
        public int RecCreator { set; get; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime RecCreated { set; get; }
        /// <summary>
        /// 记录版本
        /// </summary>
        public long RecVersion { set; get; }

    }


    public class DynamicInfoExt: DynamicInfo
    {
        /// <summary>
        /// 实体名称
        /// </summary>
        [JsonProperty("entityname")]
        public string EntityName { set; get; }

        /// <summary>
        /// 关联实体名称
        /// </summary>
        [JsonProperty("relentityname")]
        public string RelEntityName { set; get; }
        /// <summary>
        /// 实体名称
        /// </summary>
        [JsonProperty("entityname_en")]
        public string EntityName_EN { set; get; }

        /// <summary>
        /// 关联实体名称
        /// </summary>
        [JsonProperty("relentityname_en")]
        public string RelEntityName_EN { set; get; }


        [JsonProperty("typename")]
        public string TypeName { set; get; }

        /// <summary>
        /// 创建人
        /// </summary>
        [JsonProperty("reccreator_name")]
        public string RecCreatorName { set; get; }

        /// <summary>
        /// 创建人头像
        /// </summary>
        [JsonProperty("usericon")]
        public string RecCreatorUserIcon { set; get; }

        /// <summary>
        /// 点赞人数组
        /// </summary>
        [JsonProperty("praiseusers")]
        public Array PraiseUsers { set; get; }

        /// <summary>
        /// 评论列表
        /// </summary>
        [JsonProperty("commentlist")]
        public List<DynamicCommentInfo> Comments { set; get; }
    }
}
