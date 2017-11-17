using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.MailService.Mail.Enum;

namespace UBeat.Crm.MailService.Mail.Helper
{
    public class MailError
    {
        public string DisplayName { get; set; }
        public string EmailAddress { get; set; }
        public int Status { get; set; }
        public string ErrorMsg { get; set; }
        public string ErrorTime { get; set; }
    }
}
