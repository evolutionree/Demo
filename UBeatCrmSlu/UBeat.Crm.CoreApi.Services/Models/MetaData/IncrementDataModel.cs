using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.Version;

namespace UBeat.Crm.CoreApi.Services.Models.MetaData
{



  
    public class IncrementDataModel
    {
        public DataVersionType VersionType { set; get; }

        public string VersionKey { set; get; }

        /// <summary>
        /// 版本key的描述 （可空）
        /// </summary>
        public string VersionKeyName { set; get; }

        public long RecVersion { set; get; } 
    }
}
