using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.FileService
{
    public class FilesInfoModel
    {
        [JsonProperty("entityid")]
        public string EntityID { set; get; }

        [JsonProperty("fileids")]
        public List<string> FileIDs { set; get; }
    }
}
