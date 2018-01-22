using AutoMapper;
using System;
using System.Collections.Generic;
using UBeat.Crm.CoreApi.DomainModel.Department;
using UBeat.Crm.CoreApi.DomainModel.Version;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.Department;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class DepartmentServices : BasicBaseServices
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IMapper _mapper;

        public DepartmentServices(IMapper mapper, IDepartmentRepository departmentRepository)
        {
            _departmentRepository = departmentRepository;
            _mapper = mapper;
        }

        public OutputResult<object> DeptAdd(DepartmentAddModel deptAddModel, int userNumber)
        {
            var deptEntity = _mapper.Map<DepartmentAddModel, DepartmentAddMapper>(deptAddModel);
            if (deptEntity == null || !deptEntity.IsValid())
            {
                return HandleValid(deptEntity);
            }
           
            var entityId = Guid.Parse("3d77dfd2-60bb-4552-bb69-1c3e73cf4095");
 
            var res= ExcuteAction((transaction, arg, userData) =>
            {
                //验证通过后，插入数据
                var result = _departmentRepository.DeptAdd(transaction, deptEntity, userNumber);
                return HandleResult(result);
            }, deptEntity, userNumber);
            IncreaseDataVersion(DataVersionType.BasicData, null);
            return res;
        }

        public OutputResult<object> DeptEdit(DepartmentEditModel deptEditModel, int userNumber)
        {
            var deptEntity = _mapper.Map<DepartmentEditModel, DepartmentEditMapper>(deptEditModel);
            if (deptEntity == null || !deptEntity.IsValid())
            {
                return HandleValid(deptEntity);
            }
            
            var res= ExcuteAction((transaction, arg, userData) =>
            {
 
                var result = _departmentRepository.EditDepartment(deptEntity, userNumber);
                return HandleResult(result);
            }, deptEntity, userNumber);
            IncreaseDataVersion(DataVersionType.BasicData, null);
            return res;
        }
    }
}
