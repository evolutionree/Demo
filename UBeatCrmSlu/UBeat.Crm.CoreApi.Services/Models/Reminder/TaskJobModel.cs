using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.Reminder
{
    public class TaskJobModel
    {
        public string Assembly { get; set; }
        public string Type { get; set; }
        public List<string> CronStrings { get; set; }
        public string ClientId { get; set; }
        public Dictionary<string, object> JobData { get; set; }
        public string ReMark { get; set; }
        public int UserId { get; set; }
        public string MessageId { get; set; }
    }


    public class TaskJobRemoveModel
    {
        public string Assembly { get; set; }
        public string Type { get; set; }
        public string ClientId { get; set; }
        public int UserId { get; set; }
        public string MessageId { get; set; }
    }

    public class TaskJobExecuteNowModel
    {
        public string Assembly { get; set; }
        public string Type { get; set; }
        public string ClientId { get; set; }
        public int UserId { get; set; }
        public string MessageId { get; set; }
    }

    public class TaskJobExecuteNowWithFullNameModel
    {
        public string Assembly { get; set; }
        public string Type { get; set; }
        public string JobName { get; set; }
        public string JobGroup { get; set; }
    }

    public class TaskJobFullNameModel
    {
        public string Assembly { get; set; }
        public string Type { get; set; }
        public string JobName { get; set; }
        public string JobGroup { get; set; }
    }



    public class RemindResultWrap
    {
        public bool IsSucc { get; set; }
        public string Result { get; set; }
        public object Data { get; set; }
    }
}
