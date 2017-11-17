using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel
{

    public enum IncrementDirection
    {
        /// <summary>
        /// 向前获取数据
        /// </summary>
        Forward = -1,
        /// <summary>
        /// 无方向，表示全量获取
        /// </summary>
        None=0,
        /// <summary>
        /// 先后获取数据
        /// </summary>
        Backward= 1
    }

    /// <summary>
    /// 增量请求数据的参数
    /// </summary>
    public class IncrementPageParameter
    {
       
        public IncrementPageParameter(long recVersion,IncrementDirection direction= IncrementDirection.Backward,int pageSize=-1)
        {
            RecVersion = recVersion;
            Direction = direction;
            PageSize = pageSize;
        }

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


    /// <summary>
    /// 增量方式获取数据的信息对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IncrementPageDataInfo<T>
    {
        public List<T> DataList { set; get; }
        /// <summary>
        /// 当前增量数据块的最大版本号
        /// </summary>
        public long PageMaxVersion { set; get; }
        /// <summary>
        /// 当前增量数据块的最小版本号
        /// </summary>
        public long PageMinVersion { set; get; }
        


    }

}
