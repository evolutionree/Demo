using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.FileService
{
    [JsonObject(MemberSerialization.OptIn)]
    public class BaseRequestModel
    {
        [JsonProperty("entityid")]
        public string EntityId { set; get; }

        [JsonProperty("fileid")]
        public string FileId { set; get; }
    }
}
