using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel
{
    public class AddressType
    {
        /// <summary>
        /// 经度
        /// </summary>
        public string Lat { get; set; }
        /// <summary>
        /// 纬度
        /// </summary>
        public string Lon { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }
    }
}
