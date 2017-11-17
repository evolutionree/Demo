using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.Excels
{
    public class ExcelSelectModel
    {
        public string Entityid { set; get; }

        public int PageIndex { set; get; } = 0;

        public int PageSize { set; get; } = 20;

    }
}
