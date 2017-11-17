using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UBeat.Crm.CoreApi.DomainModel.WorkFlow;

namespace UBeat.Crm.CoreApi.Models.Response.WorkFlow
{
    public class CaseDetailModel: WorkFlowCaseInfo
    {
       public string nodename { set; get; }

        public string flowname { set; get; }

        public Guid relentityid { set; get; }
    }
}
