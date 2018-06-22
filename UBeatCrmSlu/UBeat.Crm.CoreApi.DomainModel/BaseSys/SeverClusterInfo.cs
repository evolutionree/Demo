using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.BaseSys
{
    public class ServerListInfo
    {
        public string ServerFinger { get; set; }
        public DateTime LasHeartTime { get; set; }
        public ServerFingerPrintInfo FingerDetail { get; set; }
    }
    public class ServerFingerPrintInfo
    {
        public Guid ServerGroupId { get; set; }
        public Guid ServerId { get; set; }
        public string WorkPath { get; set; }
        public string OsType { get; set; }
        public string MachineName { get; set; }
        public string ServerUrl { get; set; }
    }
}
