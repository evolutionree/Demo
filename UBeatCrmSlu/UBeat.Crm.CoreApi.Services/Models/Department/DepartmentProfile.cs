using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using UBeat.Crm.CoreApi.DomainModel.Department;

namespace UBeat.Crm.CoreApi.Services.Models.Department
{
    public class DepartmentProfile:Profile
    {
        public DepartmentProfile()
        {
            CreateMap<DepartmentAddModel, DepartmentAddMapper>();
            CreateMap<DepartmentEditModel, DepartmentEditMapper>();
            CreateMap<DepartMasterSlaveModel, DepartMasterSlave>();
        }
    }
}
