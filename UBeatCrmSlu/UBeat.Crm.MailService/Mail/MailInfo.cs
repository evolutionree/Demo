using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.MailService.Mail
{
    /// <summary>
    /// 异步发送邮件时保存的信息，用于释放和传递数据
    /// </summary>
    public class MailInfo
    {

        /// <summary>
        /// 设置此电子邮件的发信人地址。
        /// </summary>
        public string From { get; set; }

        public Dictionary<int, string> CC { get; set; }
        /// <summary>
        /// 设置此电子邮件的发信人名称。
        /// </summary>
        public Dictionary<int, string> FromDisplayName { get; set; }

        /// <summary>
        /// 设置此电子邮件的主题。
        /// </summary>
        public string Subject { get; set; }
        /// <summary>
        /// 设置邮件正文。
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// 设置邮件正文是否为 Html 格式的值。
        /// </summary>
        public bool IsBodyHtml { get; set; }

        private int priority = 0;
        /// <summary>
        /// 设置此电子邮件的优先级  0-Normal   1-Low   2-High
        /// 默认Normal。
        /// </summary>
        public int Priority
        {
            get { return this.priority; }
            set
            {
                if (value < 0 || value > 2)
                    priority = 0;
                else
                    priority = value;
            }
        }
    }


    public class EmailAttachmentFile
    {
        public string FileId { set; get; }
        public string FileName { set; get; }
        public byte[] Data { set; get; }
    }
}
