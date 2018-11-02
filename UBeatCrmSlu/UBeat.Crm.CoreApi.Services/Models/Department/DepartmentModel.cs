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
    public class DepartmentListSubDeptParamInfo
    {
        public Guid DeptId { get; set; }
    }
    public class DepartMasterSlaveModel
    {
        public Guid DepartId { get; set; }

        public Guid PreDepartId { get; set; }

        public int Type { get; set; }
        public int IsMaster { get; set; }
    }
    public class DepartmentPositionModel 
    {
        public int UserId { get; set; }

        public List<DepartMasterSlaveModel> Departs { get; set; }
    }
 
}
