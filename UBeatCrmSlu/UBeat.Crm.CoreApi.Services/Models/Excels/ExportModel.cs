using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.Excels
{
    public class ExportModel
    {
        public byte[] ExcelFile { set; get; }

        public string FileName { set; get; }
        public bool IsAysnc { get; set; }
        public string Message { get; set; }
        public ExportModel()
        {
            IsAysnc = false;

        }
    }
}
