using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UBeat.Crm.MailService.Mail.Enum;
using System.IO;
using System.Dynamic;

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
            var html = new TextPart("html")
            {
                Text = string.IsNullOrEmpty(bodyContent) ? string.Empty : bodyContent,
            };
            //multipart / mixed：附件。
            //multipart / related：内嵌资源。
            //multipart / alternative：纯文本与超文本共存。
            var alternative = new Multipart("alternative");
            alternative.Add(html);

            var multipart = new Multipart("mixed");
            multipart.Add(alternative);

            var related = new Multipart("related");
            multipart.Add(related);

            foreach (var tmp in AttachEmailFile(attachmentFile))
            {
                multipart.Add(tmp);
            }

            message.Body = multipart;
            return message;
        }

        /// <summary>
        /// 添加附件
        /// </summary>
        private static IList<MimePart> AttachEmailFile(IList<ExpandoObject> attachmentFile)
        {
            IList<MimePart> mimePartList = new List<MimePart>();
            foreach (dynamic tmp in attachmentFile)
            {
                Stream stream = new MemoryStream(tmp.data);
                MimePart attachment = new MimePart()
                {
                    ContentObject = new ContentObject(stream, ContentEncoding.Default),
                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                    ContentTransferEncoding = ContentEncoding.Base64,
                    FileName = tmp.filename
                };
                mimePartList.Add(attachment);
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
