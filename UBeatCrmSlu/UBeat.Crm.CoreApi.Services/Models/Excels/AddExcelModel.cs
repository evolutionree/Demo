using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.Excels
{
    public class AddExcelModel
    {
        public Guid? ExcelTemplateId { set; get; }

        public Guid? Entityid { set; get; }

        public string BusinessName { set; get; }

        public string FuncName { set; get; }

        public string Remark { set; get; }

        public IFormFile ExcelTemplate { set; get; }
    }
}
