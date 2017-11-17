using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.Excels
{
    public class ImportTemplateModel
    {
        public string TemplateId { set; get; }

        public IFormFile Data { set; get; }
    }
}
