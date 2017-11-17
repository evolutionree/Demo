using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.Services.Models.FileService;

namespace UBeat.Crm.CoreApi.Services.Models.Documents
{
    public class AddOneDocModel
    {
        /// <summary>
        /// 实体ID， DocumentType=0时有效
        /// </summary>
        public string EntityId { set; get; }

        /// <summary>
        /// 业务对象数据ID， DocumentType=0时有效
        /// </summary>
        public string BusinessId { set; get; }
        /// <summary>
        /// 文档分类，0=实体，1=知识库
        /// </summary>
        public DocumentType DocumentType { set; get; }

        public string FolderId { set; get; }
        public string FileId { set; get; }
        public string FileName { set; get; }

        public long FileLength { set; get; }
    }

    #region --AddDocumentListModel--
    public class AddDocumentsModel
    {
        /// <summary>
        /// 实体ID， DocumentType=0时有效
        /// </summary>
        public string EntityId { set; get; }

        /// <summary>
        /// 业务对象数据ID， DocumentType=0时有效
        /// </summary>
        public string BusinessId { set; get; }
        /// <summary>
        /// 文档分类，0=实体，1=知识库
        /// </summary>
        public DocumentType DocumentType { set; get; }
        public string FolderId { set; get; }

        public List<DocumentsFileInfo> Files { set; get; }
    }

    public class DocumentsFileInfo
    {
        public string FileId { set; get; }
        public string FileName { set; get; }

        public long FileLength { set; get; }
    }
    #endregion



    public class DeleteDocumentModel
    {
        public string DocumentId { set; get; }
    }
    public class DeleteDocumentListModel
    {
        public List<string> DocumentIds { set; get; }
    }

    public class DocumentListRequest
    {
        /// <summary>
        /// 实体ID， DocumentType=0时有效
        /// </summary>
        public string EntityId { set; get; }

        /// <summary>
        /// 业务对象数据ID， DocumentType=0时有效
        /// </summary>
        public string BusinessId { set; get; }
        /// <summary>
        /// 文档分类，0=实体，1=知识库
        /// </summary>
        public DocumentType DocumentType { set; get; }

        /// <summary>
        /// 是否是所有文档，包括所有文档目录和子目录的文档,0=仅为当前FolderId的文档，1=仅为当前FolderId及子目录的文档，2=当前FolderId及子目录的文档和所有文档目录数据
        /// </summary>
        public int DataCategory { set; get; }

        public string FolderId { set; get; }

        /// <summary>
        /// 如果获取目录时(即DataCategory=2)，指定目录的版本
        /// </summary>
        public Int64 FolderRecVersion { get; set; }
        public Int64 RecVersion { get; set; }


        public int PageIndex { set; get; }
        public int PageSize { set; get; } = 20;

        /// <summary>
        /// 搜索关键字
        /// </summary>
        public string SearchKey { set; get; }


    }

    public class DownloadDocRequest : BaseRequestModel
    {
        public string DocumentId { set; get; }

        public int Userid { set; get; }
    }
    public class DownloadCountRequest
    {
        public string DocumentId { set; get; }
    }

    public class MoveDocumentRequest
    {
        public string DocumentId { set; get; }
        /// <summary>
        /// 文件目录ID，没有则为null
        /// </summary>
        public string FolderId { set; get; }
    }

    public class MoveDocumentsRequest
    {
        public List<string> DocumentIdList { set; get; }
        /// <summary>
        /// 文件目录ID，没有则为null
        /// </summary>
        public string FolderId { set; get; }
    }
}
