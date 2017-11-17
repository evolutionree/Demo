using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.FileService
{
    public class DownloadFileModel
    {
        public FileInfoModel FileInfo { set; get; }

        public Stream DataReader { set; get; }
    }
}
