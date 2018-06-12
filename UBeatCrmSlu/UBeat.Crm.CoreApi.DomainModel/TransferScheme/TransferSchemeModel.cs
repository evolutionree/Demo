using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.TransferScheme
{
    public class TransferSchemeModel
    {
        /// <summary>
        /// 转移方案ID
        /// </summary>
        public Guid RecId { get; set; }
        /// <summary>
        /// 转移方案名称
        /// </summary>
        public string RecName { get; set; }
        /// <summary>
        /// 目标对象ID
        /// </summary>
        public Guid EntityId { get; set; }
        /// <summary>
        /// 关联转移对象json
        /// </summary>
        public dynamic Association { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public int RecCreator { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime RecCreated { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public int RecStatus { get; set; }

        public Guid? FieldId { get; set; }

    }
}
