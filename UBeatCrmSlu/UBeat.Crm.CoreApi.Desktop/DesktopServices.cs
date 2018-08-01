using AutoMapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Services;

namespace UBeat.Crm.CoreApi.Desktop
{
    public class DesktopServices : BasicBaseServices
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IMapper _mapper;

        public DesktopServices(IMapper mapper, IAccountRepository accountRepository, IConfigurationRoot config)
        {
            _accountRepository = accountRepository;
            _mapper = mapper;

        }

    }
}
