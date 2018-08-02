using AutoMapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Services;

namespace UBeat.Crm.CoreApi.Desktop
{
    public class DesktopServices : BasicBaseServices
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IDesktopRepository _desktopRepository;
        private readonly IMapper _mapper;

        public DesktopServices(IMapper mapper, IAccountRepository accountRepository, IDesktopRepository desktopRepository, IConfigurationRoot config)
        {
            _accountRepository = accountRepository;
            _desktopRepository = desktopRepository;
            _mapper = mapper;

        }

        public OutputResult<object> GetDesktop(int userId)
        {
            return new OutputResult<object>(_desktopRepository.GetDesktop(userId));
        }

    }
}
