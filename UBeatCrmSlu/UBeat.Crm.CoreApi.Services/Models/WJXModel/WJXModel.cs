using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.WJXModel
{
    public class WJXSSOConfigModel
    {
        public string AppId { get; set; }
        public string APPkey { get; set; }
        public string User { get; set; }
        public string SSOUrl { get; set; }
        public string QUrl { get; set; }
    }

    public class WJXQuestionModel
    {
        public string qid { get; set; }
        public string name { get; set; }
        public string answercount { get; set; }
        public string qurl { get; set; }
    }
}
