using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

using System.Text;
using System.Threading.Tasks;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.DynamicEntity;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.WJXModel;
using UBeat.Crm.CoreApi.Services.Utility;
using UBeat.Crm.CoreApi.Services.Utility.MsgForPug_inUtility;
using System.Linq;
using UBeat.Crm.LicenseCore;
using UBeat.Crm.CoreApi.Services.Services;

namespace UBeat.Crm.CoreApi.GL.Services
{
    public class OrderServices : BasicBaseServices
    {
        private readonly IConfigurationRoot _configurationRoot;
        private readonly DynamicEntityServices _dynamicEntityServices;
        public OrderServices(IConfigurationRoot configurationRoot, DynamicEntityServices dynamicEntityServices)
        {
            _configurationRoot = configurationRoot;
            _dynamicEntityServices = dynamicEntityServices;
        }
    }
}
