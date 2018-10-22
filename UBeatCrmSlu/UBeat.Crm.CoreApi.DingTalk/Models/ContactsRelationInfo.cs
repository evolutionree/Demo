using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DingTalk.Models
{
    public class ContactsRelationInfo
    {
        public string Id { get; set; }
        public Guid Contact1 { get; set; }
        public string Contact1Name { get; set; }
        public Guid Contact2 { get; set; }
        public string Contact2Name { get; set; }
        public ContactRelationEnum RelationType { get; set; }
    }
    public enum ContactRelationEnum {
        None = 0,
        PreWorkmate= 1,
        Workmate = 2,
        Friend = 3,
        Kinsman = 4,
        Remote = 5
    }
    public class ContactPositionInfo {
        public Guid ContactId { get; set; }
        public Guid CustId { get; set; }
        public DateTime JobStart { get; set; }
        public DateTime JobEnd { get; set; }
    }
}
