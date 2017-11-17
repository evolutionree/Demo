using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace UBeat.Crm.CoreApi.DomainModel.EMail
{
    /// <summary>
    /// 邮件详情用于返回值
    /// </summary>
    public class MailInfo
    {
        /// <summary>
        /// 邮件id 
        /// </summary>
        public string RecId { get; set; }
        /// <summary>
        /// 发件人信息
        /// </summary>
        public MailReceiverInfo Sender { get; set; }
        /// <summary>
        /// 收件人信息
        /// </summary>
        public List<MailReceiverInfo> Receivers { get; set; }

        /// <summary>
        /// 抄送人信息
        /// </summary>
        public List<MailReceiverInfo> CCers { get; set; }
        /// <summary>
        /// 密送人信息，只有发件箱才会有的信息
        /// </summary>
        public List<MailReceiverInfo> BCCers { get; set; }
        /// <summary>
        /// 邮件标题 
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 邮件内容
        /// </summary>
        public string BodyContent { get; set; }
        /// <summary>
        ///附件数量
        /// </summary>
        public int AttachCount { get; set; }
        /// <summary>
        /// 已读状态，从别的地方获取，并不是在邮件体中存在
        /// </summary>
        public int ReadStatus { get; set; }
        /// <summary>
        /// 标签状态
        /// </summary>
        public int TagStatus { get; set; }
        /// <summary>
        /// 是否删除状态
        /// </summary>
        public int DeletedStatus { get; set; }

        /// <summary>
        /// 附件列表
        /// </summary>
        public List<MailAttachInfo> Attaches { get; set; }
    }
    /// <summary>
    /// 用于表示收发件人信息
    /// </summary>
    public class MailReceiverInfo
    {
        /// <summary>
        /// 邮箱地址信息
        /// </summary>
        public string MailAddress { get; set; }
        /// <summary>
        /// 显示名称
        /// </summary>
        public string DisplayName { get; set; }
    }
    /// <summary>
    /// 邮件附件信息
    /// </summary>
    public class MailAttachInfo
    {
        /// <summary>
        /// 附件ID
        /// </summary>
        public string RecId { get; set; }
        /// <summary>
        /// 邮件id
        /// </summary>
        public string MailId { get; set; }
        /// <summary>
        /// 附件名称
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 附件扩展名称
        /// </summary>
        public string ExtFileName { get; set; }
        /// <summary>
        /// 附件大小
        /// </summary>
        public long FileSize { get; set; }
    }
    /// <summary>
    /// 查询邮件的入口参数
    /// </summary>
    public class MailListActionParamInfo
    {
        /// <summary>
        /// 要查询的用户id,如果小于等于0，以及等于当前用户的id表示查询自己，否则是查询下属，如果查询下属，必须检查是否有权限。
        /// </summary>
        public int FetchUserId { get; set; }
        /// <summary>
        /// 要查询的目录id
        /// </summary>
        public Guid Catalog { get; set; }
        /// <summary>
        /// 要查询的关键字，用于标题，发送人，收件人、抄送人查询
        /// </summary>
        public string SearchKey { get; set; }

        /// <summary>
        /// 其他查询条件
        /// </summary>
        public Dictionary<string, object> AdvanceSearch { get; set; }
        /// <summary>
        /// 要查询的页码
        /// </summary>
        public int PageIndex { get; set; }
        /// <summary>
        /// 每页返回的数量
        /// </summary>
        public int pageSize { get; set; }
    }
    /// <summary>
    /// 邮件打标签的入口参数
    /// </summary>
    public class MailTagActionParamInfo
    {
        /// <summary>
        /// 要打标记或者取消打标记的邮件ids，用逗号分隔
        /// </summary>
        public string MailIds { get; set; }
        /// <summary>
        /// 是否打标记
        /// </summary>
        public MailTagActionType ActionType { get; set; }
    }
    public enum MailTagActionType
    {
        MailTagActionType_SetTag = 1,
        MailTagActionType_ClearTag = 0
    }

    /// <summary>
    /// 邮件删除或者恢复删除的接口参数
    /// </summary>
    public class MailDeleteActionParamInfo
    {
        /// <summary>
        /// 要操作的邮件id，用逗号分隔
        /// </summary>
        public string MailIds { get; set; }
        /// <summary>
        /// 操作模式
        /// </summary>
        public MailDeleteActionType ActionType { get; set; }
    }
    /// <summary>
    /// 删除操作枚举
    /// </summary>
    public enum MailDeleteActionType
    {
        /// <summary>
        /// 普通删除，可以在个人回收站中回收 
        /// </summary>
        MailDeleteActionType_NormalDelete = -1,
        /// <summary>
        /// 彻底删除，管理员可以在全局回收站中回收
        /// </summary>
        MailDeleteActionType_ThroughDelete = -2,
        /// <summary>
        /// 真正删除，谁也无法恢复，包括开发人员，暂不实现此功能。
        /// </summary>
        MailDeleteActionType_RealDelete = -3,
        /// <summary>
        /// 从个人回收站中回收
        /// </summary>
        MailDeleteActionType_RestoreFromPersonRecycle = 1,
        /// <summary>
        /// 管理员从全局回收站中回收,需要额外定义
        /// </summary>
        MailDeleteActionType_RestoreFromGlobalRecycle = 2
    }

    /// <summary>
    /// 读取和未读邮件接口入口参数
    /// </summary>
    public class MailReadActionParamInfo
    {
        /// <summary>
        /// 操作的邮件id
        /// </summary>
        public string MailIds { get; set; }
        /// <summary>
        /// 操作内容
        /// </summary>
        public MailReadActionType ActionType { get; set; }
    }
    /// <summary>
    /// 已读，未读枚举
    /// </summary>
    public enum MailReadActionType
    {
        MailReadActionType_SetToRead = 1,
        MailReadActionType_SetToUnread = 0
    }
    /// <summary>
    /// 查询邮件详情的接口参数入口
    /// </summary>
    public class MailDetailParamInfo
    {
        /// <summary>
        /// 要查询的邮件的id
        /// </summary>
        public string MailId { get; set; }
    }
    /// <summary>
    /// 移动邮件的接口的入口参数
    /// </summary>
    public class MailMoveParamInfo
    {
        /// <summary>
        /// 要移动的邮件
        /// </summary>
        public string MailIds { get; set; }
        /// <summary>
        /// 移动到的目录
        /// </summary>
        public string NewFolderId { get; set; }
    }
    /// <summary>
    /// 邮件分发参数
    /// </summary>
    public class MailDistribParamInfo
    {
        /// <summary>
        /// 要分发的邮件id
        /// </summary>
        public string MailId { get; set; }
        /// <summary>
        /// 要分发的用户id，仅仅检测有同邮箱服务器的用户
        /// </summary>
        public string UserIds { get; set; }
        /// <summary>
        /// 要分发的部门id，仅仅检测有同邮箱服务器的用户
        /// </summary>
        public string DeptIds { get; set; }
        /// <summary>
        /// 是否包含子目录
        /// </summary>
        public int IncludeSubDept { get; set; }
    }
    /// <summary>
    /// 邮件分发结果对象（其结果是列表）
    /// </summary>
    public class MailDistribResultInfo
    {
        /// <summary>
        ///生成新的邮件的id
        /// </summary>
        public string MailId { get; set; }
        /// <summary>
        /// 归属的用户的id
        /// </summary>
        public int userId { get; set; }
        /// <summary>
        /// 自动归类的目录id
        /// </summary>
        public string Catalog { get; set; }
        /// <summary>
        /// 关联的邮箱信息
        /// </summary>
        public int MailBoxId { get; set; }
    }
    /// <summary>
    /// 查询关联邮件的入口参数
    /// </summary>
    public class MailListRelateMailParamInfo
    {
        /// <summary>
        /// 邮件id
        /// </summary>
        public string MailId { get; set; }
        /// <summary>
        /// 关联的客户的id
        /// </summary>
        public string CustId { get; set; }
    }
    /// <summary>
    /// 获取邮件分发记录的参数
    /// </summary>
    public class MailListDistribRecordParamInfo
    {
        /// <summary>
        /// 邮件id
        /// </summary>
        public string MailId { get; set; }
    }
    public class ContactSearchInfo
    {
        public string keyword { get; set; }

        public int count { get; set; }
    }
}
