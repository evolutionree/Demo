using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.FileService
{
    public class FileInfoModel
    {
        public string FileId { set; get; }
        public string FileName { set; get; }

        //public string FileMD5 { set; get; }

        public long Length { set; get; }

        public DateTime UploadDate { set; get; }


    }
}
