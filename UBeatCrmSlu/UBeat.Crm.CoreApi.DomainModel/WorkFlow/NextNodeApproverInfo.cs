using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.WorkFlow
{
    public class NextNodeApproverInfo
    {
        public Guid NodeId { set; get; }

        public string NodeName { set; get; }

        public List<ApproverInfo> Approvers { set; get; }
    }

   public  class ApproverInfo
    {
       
        public int UserId { set; get; }


        public string UserName { set; get; }

        public string UserIcon { set; get; }

        public string NamePinyin { set; get; }


        public Guid DeptId { set; get; }

        public string DeptName { set; get; }


    }
}
