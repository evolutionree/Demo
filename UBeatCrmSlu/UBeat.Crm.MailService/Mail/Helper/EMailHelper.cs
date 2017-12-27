using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UBeat.Crm.MailService.Mail.Enum;
using System.IO;
using System.Dynamic;
using System.Net;

namespace UBeat.Crm.MailService.Mail.Helper
{
    public static class EMailHelper
    {

        /// <summary>
        /// 创建文本消息
        /// </summary>
        /// <param name="fromAddress">发件地址</param>
        /// <param name="toAddressList">收件地址</param>
        /// <param name="title">标题</param>
        /// <param name="content">内容</param>
        /// <param name="IsPostFiles">是否将POST上传文件加为附件</param>
        /// <returns></returns>
        public static MimeMessage CreateMessage(IList<MailboxAddress> fromAddressList, IList<MailboxAddress> toAddressList, IList<MailboxAddress> ccAddressList, IList<MailboxAddress> bccAddressList
            , string subject, string bodyContent, IList<ExpandoObject> attachmentFile)
        {
            var message = new MimeMessage();

            SetEmailAddress(EmailAddrType.From, message, fromAddressList);
            SetEmailAddress(EmailAddrType.To, message, toAddressList);
            SetEmailAddress(EmailAddrType.CC, message, ccAddressList);
            SetEmailAddress(EmailAddrType.Bcc, message, bccAddressList);

            message.Subject = string.IsNullOrEmpty(subject) ? string.Empty : subject;


            var builder = new BodyBuilder();
            builder.HtmlBody = string.IsNullOrEmpty(bodyContent) ? string.Empty : bodyContent;
            foreach (dynamic tmp in attachmentFile)
            {
                builder.Attachments.Add(tmp.filename, tmp.data, ContentType.Parse(GetFileType(tmp.filetype)));
            }

            message.Body = builder.ToMessageBody();
            return message;
        }
        static string GetFileType(string fileSuffix)
        {
            Dictionary<string, string> dicContentType = new Dictionary<string, string>();
            dicContentType.Add(".html", "text/html");
            dicContentType.Add(".txt", "text/plain");
            dicContentType.Add(".gif", "image/gif");
            dicContentType.Add(".jpeg", "image/jpeg");
            dicContentType.Add(".png", "image/png");
            dicContentType.Add(".xhtml", "application/xhtml");
            dicContentType.Add(".xml", "application/xml");
            dicContentType.Add(".json", "application/json");
            dicContentType.Add(".pdf", "application/pdf");
            dicContentType.Add(".doc", "application/msword");
            dicContentType.Add(".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
            dicContentType.Add(".xls", "application/vnd.ms-excel");
            dicContentType.Add(".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            dicContentType.Add(".ppt", "application/vnd.ms-powerpoint");
            dicContentType.Add(".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation");
            if (dicContentType.Keys.Contains(fileSuffix.ToLower()))
            {
                return dicContentType[fileSuffix.ToLower()];
            }
            return "application/octet-stream";
        }
        /// <summary>
        /// 添加附件
        /// </summary>
        private static IList<MimePart> AttachEmailFile(IList<ExpandoObject> attachmentFile)
        {
            IList<MimePart> mimePartList = new List<MimePart>();

            foreach (dynamic tmp in attachmentFile)
            {

                Stream ms = new MemoryStream(tmp.data);
                ms.Position = 0;
                var attachment = new MimePart("application", "vnd.ms-excel")
                {
                    ContentObject = new ContentObject(ms, ContentEncoding.Default),
                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                    ContentTransferEncoding = ContentEncoding.Base64,
                    FileName = tmp.filename
                };

                mimePartList.Add(attachment);

                //    Stream stream = new MemoryStream(tmp.data);
                //    MimePart attachment = new MimePart()
                //    {
                //        ContentObject = new ContentObject(stream, ContentEncoding.Default),
                //        ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                //        ContentTransferEncoding = ContentEncoding.Base64,
                //        FileName = tmp.filename,
                //    };

            }
            return mimePartList;
        }

        /// <summary>
        /// 将收件人、抄送人、密送人添加到 MailMessage 中
        /// </summary>
        /// <param name="type">收件人、抄送人、密送人</param>
        /// <param name="mMailMessage">待发送的MailMessage类</param>
        private static void SetEmailAddress(EmailAddrType type, MimeMessage mimeMessage, IList<MailboxAddress> addressList)
        {
            switch (type)
            {
                case EmailAddrType.From:
                    {
                        mimeMessage.From.AddRange(addressList);
                    }
                    break;
                case EmailAddrType.To:
                    {
                        mimeMessage.To.AddRange(addressList);
                    }
                    break;
                case EmailAddrType.CC:
                    {
                        mimeMessage.Cc.AddRange(addressList);
                    }

                    break;
                case EmailAddrType.Bcc:
                    {
                        mimeMessage.Bcc.AddRange(addressList);
                    }
                    break;
                default:
                    {
                        throw new Exception("邮件设置地址异常");
                    }
            }
        }

    }
}
