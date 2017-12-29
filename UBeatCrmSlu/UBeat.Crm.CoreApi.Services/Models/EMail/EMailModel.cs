using MailKit.Search;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.EMail;
using UBeat.Crm.MailService.Mail.Enum;

namespace UBeat.Crm.CoreApi.Services.Models.EMail
{
    public class SendEMailModel
    {
        public Guid MailId { get; set; }
        /// <summary>
        /// 发件人
        /// </summary>
        public string FromAddress { get; set; }

        /// <summary>
        /// 发件昵称
        /// </summary>
        public string FromName { get; set; }

        /// <summary>
        /// 收件人
        /// </summary>
        public IList<MailAddressModel> ToAddress { get; set; }
        /// <summary>
        /// 抄送人
        /// </summary>
        public IList<MailAddressModel> CCAddress { get; set; }
        /// <summary>
        /// 密送人
        /// </summary>
        public IList<MailAddressModel> BCCAddress { get; set; }
        /// <summary>
        /// 邮件主题
        /// </summary>
        public string Subject { get; set; }
        /// <summary>
        /// 主体消息内容
        /// </summary>
        public string BodyContent { get; set; }

        public Guid PMailId { get; set; }

        public IList<AttachmentFileModel> AttachmentFile { get; set; }

        public EmailType EmailType
        {

            get
            {
                return EmailType.Gmail;
            }
        }
    }
    public class MailAddressModel
    {
        public string Address { get; set; }
        public string DisplayName { get; set; }
    }

    public class AttachmentFileModel
    {
        public string FileId { get; set; }

        public int FileType { get; set; }
    }


    public class ReceiveEMailModel
    {
        public SearchQueryEnum Conditon { get; set; }

        public string ConditionVal { get; set; }


        public bool IsFirstInit { get; set; }

    }

    public class MailOwnModel
    {
        public List<Guid> RecIds { get; set; }

        public int NewUserId { get; set; }

    }

    public class CatalogModel
    {
        public string CatalogName { get; set; }

        public string CatalogType { get; set; }

        public string ParentId { get; set; }

        public int SearchUserId { get; set; }

    }

    public class WhiteListModel
    {
        /// <summary>
        /// 邮件列表
        /// </summary>
        public List<Guid> RecIds { get; set; }
        /// <summary>
        /// 部门列表
        /// </summary>
        public List<Guid> Depts { get; set; }
        /// <summary>
        /// 用户列表
        /// </summary>
        public List<int> UserIds { get; set; }

        /// <summary>
        /// 是否设置 1:是  2：否
        /// </summary>
        public string enable { get; set; }

    }

    public class PersonalSignModel
    {
        public string signcontent { get; set; }

        public int devicetype { get; set; }

        public string recname { get; set; }

    }


    /// <summary>
    /// 查询邮件的入口参数
    /// </summary>
    public class MailListActionParamInfoModel
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


    public class TagMailModel
    {
        public string MailIds { get; set; }

        public MailTagActionType actionType { get; set; }
    }

    public class DeleteMailModel
    {
        /// <summary>
        /// 是否彻底删除 
        /// </summary>
        public bool IsTruncate { get; set; }

        public string MailIds { get; set; }

    }

    public class ReConverMailModel
    {
        public int RecStatus { get; set; }
        public string MailIds { get; set; }

        public int UserId { get; set; }
    }

    public class ReadOrUnReadMailModel
    {
        public int IsRead { get; set; }
        public string MailIds { get; set; }
    }

    public class MailDetailModel
    {
        public Guid MailId { get; set; }
    }

    public class MailAttachmentModel
    {
        public Guid MongoId { get; set; }

        public string FileName { get; set; }
        public int FileSize { get; set; }
        public string FileType { get; set; }

        public Guid MailId { get; set; }
    }
    public class TransferMailDataModel
    {
        public List<Guid> MailIds { get; set; }
        public List<int> TransferUserIds { get; set; }
        public List<Guid> DeptIds { get; set; }
        public List<MailAttachmentMapper> Attachment { get; set; }
    }


    public class MoveMailModel
    {
        public string MailIds { get; set; }

        public Guid CatalogId { get; set; }
    }


    public class MimeMessageResult
    {
        public SendEMailMapper Entity { get; set; }
        public UserMailInfo UserMailInfo { get; set; }
        public MimeMessage Msg { get; set; }
        public int ActionType { get; set; }
        public int Status { get; set; }
        public string ExceptionMsg { get; set; }

        public IList<ExpandoObject> AttachFileRecord { get; set; }
    }
    public class ToAndFroModel
    {
        public Guid MailId { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public RelatedMySelf RelatedMySelf { get; set; }
        public RelatedSendOrReceive RelatedSendOrReceive { get; set; }
    }

    public class AttachmentListModel
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }

        public string KeyWord { get; set; }
    }

    public class TransferRecordParamModel
    {
        public Guid MailId { get; set; }

        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }


    public class InnerToAndFroMailModel
    {
        public string KeyWord { get; set; }

        public int FromUserId { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }
    }

    public class ReconvertMailModel
    {
        public int? UserId { get; set; }
        /// <summary>
        /// 空查询全部 
        /// </summary>
        public string MailAddress { get; set; }
        /// <summary>
        /// -1 查询所有邮件 1 发出 2 收到 
        /// </summary>
        public int Ctype { get; set; }

        public string KeyWord { get; set; }

        /// <summary>
        /// -1 不用时间范围筛选  0 两周内 1 三个月内 2 半年内 3 一年内 
        /// </summary>
        public int DateRange { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }
    }

    public class MailAddressPasswordModel
    {
        public Guid MailBoxId { get; set; }
    }

}
