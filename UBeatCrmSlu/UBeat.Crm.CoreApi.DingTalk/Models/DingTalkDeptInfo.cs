using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DingTalk.Models
{
    public class DingTalkDeptInfo
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long ParentId { get; set; }
        public Guid CRMRecId { get; set; }
        public List<DingTalkDeptInfo> SubDepts { get; set; }
        public DingTalkDeptInfo() {
            SubDepts = new List<DingTalkDeptInfo>();
        }
    }
    public class DingTalkListUsersByDeptIdInfo:DingTalkResponseInfo { 
        public List<string> UserIds { get; set; }

    }
}
