using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.Controllers;
using UBeat.Crm.CoreApi.ZGQY.Model;
using UBeat.Crm.CoreApi.ZGQY.Services;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.DynamicEntity;
using UBeat.Crm.CoreApi.Services.Models.WJXModel;
using UBeat.Crm.CoreApi.Services.Services;
using System.Linq;
using IRCS.DBUtility;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.DomainModel.Products;
using UBeat.Crm.CoreApi.DomainModel;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;

namespace UBeat.Crm.CoreApi.ZGQY.Controllers
{

    [Route("api/[controller]")]
    public class OaDatabaseController : BaseController
    {
        public static string connectionString = PubConstant.ConnectionString;  
        private readonly OaDataBaseServices _OaDataBaseServices;
        public OaDatabaseController(
            OaDataBaseServices OaDataBaseServices
            )
        {
            _OaDataBaseServices = OaDataBaseServices;
        }

        [Route("test")]
        [HttpPost]
        [AllowAnonymous]
        public OutputResult<object> Test()
        {
            return new OutputResult<object>(_OaDataBaseServices.updateContractData(LoginUser.UserId));
        }
    }
}
