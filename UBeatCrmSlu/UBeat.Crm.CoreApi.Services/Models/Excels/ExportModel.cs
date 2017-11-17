using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.Excels
{
    public class ExportModel
    {
        public byte[] ExcelFile { set; get; }

        public string FileName { set; get; }
    }
}
