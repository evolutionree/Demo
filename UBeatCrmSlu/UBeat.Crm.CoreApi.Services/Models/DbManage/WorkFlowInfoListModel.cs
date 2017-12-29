using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.DbManage
{
    public class WorkFlowInfoListModel
    {
        public Guid? FlowId { set; get; }
    }

    public class SaveWorkFlowInfoModel
    {
        public IFormFile Data { set; get; }
    }
}
