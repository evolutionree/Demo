using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.Message;
using UBeat.Crm.CoreApi.DomainModel.Version;
using UBeat.Crm.CoreApi.DomainModel.Vocation;

namespace UBeat.Crm.CoreApi.Services.Models
{
    /// <summary>
    /// 公共数据缓存对象
    /// </summary>
    public class CommonCacheData
    {
        /// <summary>
        /// 所有的功能信息
        /// </summary>
        public List<FunctionInfo> TotalFunctions { set; get; }


        //版本数据
        public List<DataVersionInfo> DataVersions { set; get; }


        

    }


    public class CommonCacheDataRedis
    {
        /// <summary>
        /// 所有的功能信息
        /// </summary>
        public long TotalFunctionsVersion { set; get; }


        //字典数据


        //

    }

}
