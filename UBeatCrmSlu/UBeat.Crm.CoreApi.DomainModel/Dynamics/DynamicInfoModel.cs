using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Dynamics
{
    public class DynamicInfoModel
    {
        [JsonProperty("dynamicid")]
        public object DynamicId { set; get; }
        /// <summary>
        /// 实体id，表示该动态属于哪种业务的，如客户时，该字段为客户实体ID
        /// </summary>
        [JsonProperty("entityid")]
        public object EntityId { set; get; }

        /// <summary>
        /// 实体名称
        /// </summary>
        [JsonProperty("entityname")]
        public object EntityName { set; get; }
        /// <summary>
        /// 业务id，表示该动态属于哪个业务对象的，如某个客户时，该字段为客户id
        /// </summary>
        [JsonProperty("businessid")]
        public object BusinessId { set; get; }

       

        [JsonProperty("typeentityid")]
        public object TypeEntityId { set; get; }

        [JsonProperty("typeentityname")]
        public object TypeEntityName { set; get; }

        [JsonProperty("typeid")]
        public object TypeId { set; get; }

        [JsonProperty("typerecid")]
        public object TypeRecId { set; get; }
        

        /// <summary>
        /// 动态模板内容
        /// </summary>
        [JsonProperty("tempcontent")]
        public object TempContent { set; get; }

       
        /// <summary>
        /// 动态内容，与模板对应的数据
        /// </summary>
        [JsonProperty("tempdata")]
        public object TempData { set; get; }


        /// <summary>
        /// 动态类型，0=默认，1=系统，2=实体
        /// </summary>
        [JsonProperty("dynamictype")]
        public int DynamicType { set; get; }
        /// <summary>
        /// 评论内容，属于普通动态时的内容，非系统和实体的动态
        /// </summary>
        [JsonProperty("content")]
        public object Content { set; get; }
        /// <summary>
        /// 记录状态
        /// </summary>
        [JsonProperty("recstatus")]
        public int RecStatus { set; get; }

        /// <summary>
        /// 创建人
        /// </summary>
        [JsonProperty("reccreator")]
        public int RecCreator { set; get; }
        /// <summary>
        /// 创建人
        /// </summary>
        [JsonProperty("reccreator_name")]
        public object RecCreatorName { set; get; }

        /// <summary>
        /// 创建人头像
        /// </summary>
        [JsonProperty("usericon")]
        public object UserIcon { set; get; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [JsonProperty("reccreated")]
        public DateTime RecCreateTime { set; get; }

        [JsonProperty("recversion")]
        public long RecVersion { set; get; }

        /// <summary>
        /// 点赞人数组
        /// </summary>
        [JsonProperty("praiseusers")]
        public Array PraiseUsers { set; get; }

        //[JsonProperty("praiselist")]
        //public List<DynamicPraise> Praises { set; get; }

        /// <summary>
        /// 评论列表
        /// </summary>
        [JsonProperty("commentlist")]
        public List<DynamicComment> Comments { set; get; }
    }
    /// <summary>
    /// 动态点赞
    /// </summary>
    public class DynamicPraise
    {
        /// <summary>
        /// 记录状态
        /// </summary>
        [JsonProperty("recstatus")]
        public int RecStatus { set; get; }
        /// <summary>
        /// 创建人
        /// </summary>
        [JsonProperty("reccreator")]
        public int RecCreator { set; get; }
        /// <summary>
        /// 创建人
        /// </summary>
        [JsonProperty("reccreator_name")]
        public object RecCreatorName { set; get; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [JsonProperty("reccreated")]
        public DateTime RecCreateTime { set; get; }

        /// <summary>
        /// 记录版本号
        /// </summary>
        [JsonProperty("recversion")]
        public long RecVersion { set; get; }
    }

    /// <summary>
    /// 动态评论
    /// </summary>
    public class DynamicComment
    {
        /// <summary>
        /// 动态id
        /// </summary>
        [JsonProperty("dynamicid")]
        public string DynamicId { set; get; }
        /// <summary>
        /// 评论id
        /// </summary>
        [JsonProperty("commentsid")]
        public string CommentsId { set; get; }
        /// <summary>
        /// 父级评论的id，有该id时，意味着该条数据为评论回复
        /// </summary>
        [JsonProperty("pcommentsid")]
        public string PcommentsId { set; get; }
        /// <summary>
        /// 评论内容
        /// </summary>
        [JsonProperty("comments")]
        public string Comments { set; get; }

        /// <summary>
        /// 评论人
        /// </summary>
        [JsonProperty("reccreator")]
        public string RecCreator { set; get; }
        /// <summary>
        /// 评论人头像
        /// </summary>
        [JsonProperty("reccreator_icon")]
        public string RecCreatorIcon { set; get; }

        /// <summary>
        /// 评论人
        /// </summary>
        [JsonProperty("reccreator_name")]
        public string RecCreatorName { set; get; }
        
       
        /// <summary>
        /// 评论时间
        /// </summary>
        [JsonProperty("reccreated")]
        public DateTime RecCreateTime { set; get; }


        /// <summary>
        /// 被回复的评论内容
        /// </summary>
        [JsonProperty("tocomments")]
        public string ToComments { set; get; }
        /// <summary>
        /// 被评论人
        /// </summary>
        [JsonProperty("tocommentor")]
        public string ToCommentor { set; get; }

        /// <summary>
        /// 回复对象
        /// </summary>
        //[JsonProperty("replycomment")]
        [JsonIgnore]
        public List<DynamicComment> Reply { set; get; }

    }
}
