using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.SalesLead;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.SalesLead;

namespace UBeat.Crm.CoreApi.Services.Services
{
  public  class SalesLeadServices:BaseServices
    {
        private readonly ISalesLeadRepository _salesLeadRepository;
        private readonly IMapper _mapper;

        public SalesLeadServices(IMapper mapper, ISalesLeadRepository salesLeadRepository)
        {
            _salesLeadRepository = salesLeadRepository;
            _mapper = mapper;
        }

        public OutputResult<object> ChangeSalesLeadToCustomer(SalesLeadModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<SalesLeadModel, SalesLeadMapper>(entityModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            return HandleResult(_salesLeadRepository.ChangeSalesLeadToCustomer(entity, userNumber));
        }
    }
}
