using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UBeat.Crm.CoreApi.DomainModel.Account;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.Account;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.CoreApi.Services.Models.SalesTarget;

using Microsoft.Extensions.Configuration;
using UBeat.Crm.CoreApi.Core.Utility;
using System.Security.Cryptography;
using System.Text;
using UBeat.Crm.CoreApi.Controllers;

namespace UBeat.Crm.CoreApi.Desktop
{
    [Route("api/[controller]")]
    public class DesktopController : BaseController
    {


    }
}
