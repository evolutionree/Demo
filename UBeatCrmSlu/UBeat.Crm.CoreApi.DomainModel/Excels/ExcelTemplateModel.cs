using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Excels
{
    public class ExcelTemplateModel
    {
        public Guid EntityId { set; get; }
        /// <summary>
        /// excel导入导出的方式：0为模板导入导出，1为动态导入导出
        /// </summary>
        public int Exceltype { set; get; }
        public Guid ExcelTemplateId { set; get; }
         
        public string ExcelName { set; get; }

        public string TemplateContent { set; get; }

        public int UserNo { set; get; }
    }
}
