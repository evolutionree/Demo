using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.FileService
{
    public class UploadFileModel : BaseRequestModel
    {

        [JsonProperty("filename")]
        public string FileName { set; get; }

        [JsonProperty("data")]
        public IFormFile Data { set; get; }

    }
    public class UploadFileChunkRequest: UploadFileModel
    {
        [JsonProperty("fileMD5")]
        public string FileMD5 { set; get; }

        [JsonProperty("chunkindex")]
        public int ChunkIndex { set; get; }

        [JsonProperty("chunksize")]
        public int ChunkSize { set; get; }

        [JsonProperty("filelength")]
        public long FileLength { set; get; }
    }
}
