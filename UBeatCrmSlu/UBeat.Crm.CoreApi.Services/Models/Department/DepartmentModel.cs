using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.Department
{
    public class DepartmentAddModel
    {
        public Guid PDeptId { get; set; }
        public string DeptName { get; set; }
        public int OgLevel { get; set; }
        public string DeptLanguage { get; set; }
    }

    public class DepartmentEditModel
    {
        public Guid DeptId { get; set; }
        public string DeptName { get; set; }

        public Guid PDeptId { get; set; }
        public int OgLevel { get; set; }
        public string DeptLanguage { get; set; }
    }
    public class DepartmentListSubDeptParamInfo {
        public Guid DeptId { get; set; }
    }
}
