using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.Services.Utility;

namespace UBeat.Crm.CoreApi.Services.Models.FileService
{
    public class ImageFileThumbnail : BaseRequestModel
    {


        [JsonProperty("imagewidth")]
        public int ImageWidth { set; get; }

        [JsonProperty("imageheight")]
        public int ImageHeight { set; get; }

        [JsonProperty("thumbmodel")]
        public ThumbModel Mode { set; get; } = ThumbModel.NoDeformationAllThumb;

        [JsonIgnore]
        public bool IsValid
        {
            get { return ImageWidth > 0 && ImageHeight > 0; }
        }
    }
}
