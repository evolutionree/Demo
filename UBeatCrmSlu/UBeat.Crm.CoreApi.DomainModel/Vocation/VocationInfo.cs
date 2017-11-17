using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Vocation
{
    public class VocationInfo
    {
        /// <summary>
        /// 职能ID
        /// </summary>
        public Guid VocationId { set; get; }

        /// <summary>
        /// 职能名称
        /// </summary>
        public string VocationName { set; get; }


        /// <summary>
        /// 职能描述
        /// </summary>
        public string Description { set; get; }

       
        public List<FunctionInfo> Functions { set; get; } = new List<FunctionInfo>();
    }
}
