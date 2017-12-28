using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.DbManage
{
    public class WorkFlowInfoListModel
    {
        public List<Guid> FlowIds { set; get; }
    }

    public class SaveWorkFlowInfoModel
    {
        public IFormFile Data { set; get; }
    }
}
