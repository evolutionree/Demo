using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// 主要是用于记录与邮箱有关的个人信息
/// 如个人签名，WEB主页布局
/// </summary>
namespace UBeat.Crm.CoreApi.DomainModel.EMail
{
    public class MailPersonelSetInfo
    {
    }

    public class MailBox
    {
        public Guid recid { get; set; }
        public string accountid { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        public string recname { get; set; }
        public int inwhitelist { get; set; }
        public string mailserver { get; set; }
        public int mailprovider { get; set; }
        public string imapaddress { get; set; }
        public int refreshinterval { get; set; }
        public int servertype { get; set; }
        public string smtpaddress { get; set; }
    }

    public class OrgAndStaffTreeModel
    {
        public string treeId { get; set; }

        public string keyword { get; set; }
        /// <summary>
        /// 要查询的页码
        /// </summary>
        public int PageIndex { get; set; }
        /// <summary>
        /// 每页返回的数量
        /// </summary>
        public int PageSize { get; set; }
    }

    public class AddCatalogModel
    {
        public Guid recId { get; set; }
        public string recName { get; set; }

        public Guid pid { get; set; }


        public Guid vPid { get; set; }
    }
    /// <summary>
    /// 目录移动的入口参数
    /// </summary>
    public class MoveCatalogModel {
        /// <summary>
        /// 要移动的目录的id
        /// </summary>
        public Guid recId { get; set; }
        /// <summary>
        /// 新的父目录id
        /// </summary>
        public Guid newPid { get; set; }
    }

    public class OrderCatalogModel
    {
        /// <summary>
        /// 要移动的目录的id
        /// </summary>
        public Guid recId { get; set; }
        /// <summary>
        /// doType 0 下移  doType  1 上移
        /// </summary>
        public int doType { get; set; }
    }
    /// <summary>
    ///目录转移的入口参数
    /// </summary>
    public class TransferCatalogModel {
        /// <summary>
        /// 目录id
        /// </summary>
        public Guid recId { get; set; }
        /// <summary>
        /// 持有用户的id
        /// </summary>
        public int newUserId { get; set; }
    }


    public class DelCatalogModel
    {
        public Guid recId { get; set; }
    }

    public class UserCatalogModel
    {
        public string catalogType { get; set; }

        public Guid recId { get; set; }
    }

    public class PersonalSign
    {
        public Guid recid { get; set; }
        public string signcontent { get; set; }

        public int devicetype { get; set; }

        public MailBox mailbox { get; set; }

        public string recname { get; set; }
    }

    /// <summary>
    /// web邮箱主页个性化布局
    /// </summary>
    public class WebMailPersonelLayoutInfo {
        /// <summary>
        /// 用户id
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// 左边目录视图的比例
        /// </summary>
        public Decimal LeftPrecent { get; set; }
        /// <summary>
        /// 右边关联数据视图的比例大小，
        /// 如果ShowRight=false，则此属性无效
        /// </summary>
        public Decimal RightPrecent { get; set; }
        /// <summary>
        /// 底部预览视图的比例
        /// 如果ShowBottom=false，则此属性无效
        /// </summary>
        public Decimal BottomPrecent { get; set; }
        /// <summary>
        /// 是否显示底部预览视图
        /// </summary>
        public bool ShowBottom { get; set; }
        /// <summary>
        /// 是否显示右边关联数据视图
        /// </summary>
        public bool ShowRight { get; set; }
    }
}
