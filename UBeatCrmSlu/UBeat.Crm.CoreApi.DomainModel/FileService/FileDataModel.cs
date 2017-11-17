using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.FileService
{
    public class FileDataModel
    {
        public string FileId { set; get; }
        public string FileName { set; get; }
        public byte[] Data { set; get; }
    }
}
