using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.MailService.Mail.Enum
{
    public enum EmailAddrType
    {
        /// <summary>
        /// 发件人
        /// </summary>
        From = 1,
        /// <summary>
        /// 收件人
        /// </summary>
        To = 2,
        /// <summary>
        /// 抄送人
        /// </summary>
        CC = 3,
        /// <summary>
        /// 密送人
        /// </summary>
        Bcc = 4,
    }
}
