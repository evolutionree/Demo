using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.Documents
{
    

    public class AddDocFolderModel
    {
        /// <summary>
        /// 实体ID， DocumentType=0时有效
        /// </summary>
        public string EntityId { set; get; }

        /// <summary>
        /// 文档分类，0=实体，1=知识库
        /// </summary>
        public DocumentType DocumentType { set; get; }
        public string FolderName { set; get; }

        public string PfolderId { set; get; }

        /// <summary>
        /// 是否全部可见，如果为全部可见，PermissionIds无效，不需传值
        /// </summary>
        public int IsAllVisible { set; get; } = 1;
        /// <summary>
        /// 目录可见范围的权限id，如部门id
        /// </summary>
        public string PermissionIds { set; get; }


    }
    public class DocFolderListModel
    {
        /// <summary>
        /// 实体ID， DocumentType=0时有效
        /// </summary>
        public string EntityId { set; get; }

        /// <summary>
        /// 文档分类，0=实体，1=知识库
        /// </summary>
        public DocumentType DocumentType { set; get; }
        public string FolderId { set; get; }

        public Int64 RecVersion { get; set; }

        /// <summary>
        /// 记录状态过滤条件，-1时，不使用该条件
        /// </summary>
        public int RecStatus { set; get; } = 1;
    }

    public class UpdateDocFolderModel
    {
        /// <summary>
        /// 目录名称,若不更新，则不传
        /// </summary>
        public string FolderName { set; get; }
        /// <summary>
        /// 父级id，若不调整，则不传,如果要设定为根目录时，则传为自己的目录ID
        /// </summary>
        public string PfolderId { set; get; }
        /// <summary>
        /// 目录ID，必须传
        /// </summary>
        public string FolderId { set; get; }
        /// <summary>
        /// 是否全部可见，如果为全部可见，PermissionIds无效，不需传值
        /// </summary>
        public int IsAllVisible { set; get; } = 1;
        /// <summary>
        /// 目录可见范围的权限id，如部门id
        /// </summary>
        public string PermissionIds { set; get; }
    }

    public class DeleteDocFolderModel
    {
        public string FolderId { set; get; }
    }
}

