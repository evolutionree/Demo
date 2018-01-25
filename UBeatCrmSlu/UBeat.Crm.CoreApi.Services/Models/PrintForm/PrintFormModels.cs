using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.PrintForm
{
    public class OutputDocumentParameter
    {

        [JsonProperty("entityid")]
        public Guid? EntityId { set; get; }

        public int DocType { set; get; }

        [JsonProperty("data")]
        public IFormFile Data { set; get; }
    }
}
