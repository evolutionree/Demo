using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Documents
{
    public class DocumentInfo
    {
        /// <summary>
        /// 记录id
        /// </summary>
        public Guid DocumentId { set; get; }

        /// <summary>
        /// 文件ID
        /// </summary>
        public Guid FileId { set; get; }

        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName { set; get; }

        /// <summary>
        /// 文件大小
        /// </summary>
        public long FileLength { set; get; }

        /// <summary>
        /// 文件目录ID
        /// </summary>
        public Guid FolderId { set; get; }

        /// <summary>
        /// 下载次数
        /// </summary>
        public long DownloadCount { set; get; }

        /// <summary>
        /// 实体ID
        /// </summary>
        public Guid EntityId { set; get; }

        /// <summary>
        /// 业务对象数据ID
        /// </summary>
        public Guid BusinessId { set; get; }

        /// <summary>
        /// 排序
        /// </summary>
        public int RecOrder { set; get; }

        /// <summary>
        /// 状态 0停用 1启用
        /// </summary>
        public int RecStatus { set; get; }

        /// <summary>
        /// 创建人
        /// </summary>
        public int RecCreator { set; get; }

        /// <summary>
        /// 修改人
        /// </summary>
        public int RecUpdator { set; get; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime RecCreated { set; get; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime RecUpdated { set; get; }

        /// <summary>
        /// 记录版本
        /// </summary>
        public long RecVersion { set; get; }

    }
}
