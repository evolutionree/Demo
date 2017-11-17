using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.Services.Utility;

namespace UBeat.Crm.CoreApi.Services.Models.FileService
{
    public class FilesDataModel
    {
        [JsonProperty("entityid")]
        public string EntityID { set; get; }

        [JsonProperty("fileids")]
        public List<string> FileIDs { set; get; }

        #region --如果ImageHeight和ImageWidth有设置，则获取缩略图--
        //获取缩略图模式：0=不变形，全部（缩略图），1=变形，全部填充（缩略图），2=不变形，截中间（缩略图），3=不变形，截中间（非缩略图）
        [JsonProperty("thumbmodel")]
        public ThumbModel ThumbModel { set; get; }
        [JsonProperty("imagewidth")]
        public int ImageWidth { set; get; }
        [JsonProperty("imageheight")]
        public int ImageHeight { set; get; }
        #endregion



    }
}
