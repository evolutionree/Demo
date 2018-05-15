using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Account
{
    public class HistoryPwd
    {
        public Guid RecId { get; set; }
        public int UserId { get; set; }
        public string OldPwd { get; set; }
        public string NewPwd { get; set; }
        public int ChangeType { get; set; }
        public int RecCreator { get; set; }
        public DateTime RecCreated { get; set; }
    }
}
