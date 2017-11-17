using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Dynamics;

namespace UBeat.Crm.CoreApi.Services.Models.Dynamics
{
    public class DynamicListModel
    {
        /// <summary>
        /// 请求类型：0为增量，1为分页
        /// </summary>
        public int RequestType { set; get; }

        public Guid? EntityId { set; get; }

        public Guid? Businessid { set; get; }

        public List<DynamicType> DynamicTypes { set; get; }

        /// <summary>
        /// 分页大小：小于1表示不分页
        /// 增量大小：默认-1表示不划分数据块，取direction方向的所有数据
        /// </summary>
        public int PageSize { set; get; } = -1;


        /// <summary>
        /// 分页请求的页码，1开始，小于1表示不分页
        /// </summary>
        public int PageIndex { set; get; }


        #region 增量请求参数
        /// <summary>
        /// 增量的依据版本号
        /// </summary>
        public long RecVersion { set; get; }
        /// <summary>
        /// 增量取值方向
        /// </summary>
        public IncrementDirection Direction { set; get; } = IncrementDirection.Backward;
        
    
        #endregion

    }
}
