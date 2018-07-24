using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.Dynamics
{
    public class SelectDynamicModel
    {
        public Guid DynamicId { set; get; }
    }

    public class SelectDynamicByBizIdParamInfo {
        public Guid EntityId { get; set; }
        public Guid RecId { get; set; }
    }
    public class SelectDynamicListModel
    {
        public Guid? Businessid { set; get; }
        public List<int> DynamicTypes { set; get; } = new List<int>() { 0 };
        public Guid? EntityId { set; get; }

        public Int64 RecVersion { get; set; }
        /// <summary>
        /// 点赞记录的版本
        /// </summary>
        public Int64 PraiseVersion { get; set; }
        /// <summary>
        /// 评论记录的版本
        /// </summary>
        public Int64 CommentVersion { get; set; }


        public int PageIndex { set; get; } = 0;

        public int PageSize { set; get; } = 20;
    }

    public class AddDynamicModel
    {
        public string Content { set; get; }

    }

    public class DeleteDynamicModel
    {
        public Guid DynamicId { set; get; }
    }
}
