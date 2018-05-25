using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.TransferScheme
{
    public class TransferSchemeParam
    {
        /// <summary>
        /// 转移方案ID
        /// </summary>
        public Guid TransSchemeId { get; set; }
        /// <summary>
        /// 转移方案名称
        /// </summary>
        public string TransSchemeName { get; set; }
        /// <summary>
        /// 目标对象ID
        /// </summary>
        public Guid TargetTransferId { get; set; }
        /// <summary>
        /// 关联转移对象json
        /// </summary>
        public string AssociationTransfer{ get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

    }
    public class GetTransParam
    {
        public Guid TransSchemeId { get; set; }
    }

    public class TransStatus
    {
        public string RecIds { get; set; }
        public int Status { get; set; }
    }
}
