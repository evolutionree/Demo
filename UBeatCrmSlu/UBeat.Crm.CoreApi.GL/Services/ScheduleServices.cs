using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.Services.Models.SoapErp;
using System.Linq;
using UBeat.Crm.CoreApi.Services.Utility;
using UBeat.Crm.CoreApi.IRepository;
using static UBeat.Crm.CoreApi.Services.Services.SoapServices;
using System.Data.Common;
using UBeat.Crm.CoreApi.Repository.Utility;
using System.Reflection;
using Newtonsoft.Json;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.Department;
using UBeat.Crm.CoreApi.Repository.Repository.DynamicEntity;
using UBeat.Crm.CoreApi.Services.Models.Account;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UBeat.Crm.CoreApi.Services.Services;

namespace UBeat.Crm.CoreApi.ZGQY.Services
{
    public class ScheduleServices : BasicBaseServices
    {
        private readonly SoapServices _soapServices;
        private readonly IConfigurationRoot _configurationRoot;
        private readonly IDynamicEntityRepository _dynamicEntityRepository;
        private readonly DepartmentServices _departmentServices;
        private readonly ActionExtServices _actionExtServices;
        private readonly AccountServices _accountServices;

        public ScheduleServices(SoapServices soapServices)
        {
            _configurationRoot = ServiceLocator.Current.GetInstance<IConfigurationRoot>();
            _soapServices = soapServices;

        }
        public void InitServices(BaseServices[] baseServices)
        {
            foreach (var t in baseServices)
            {
                t.RoutePath = this.RoutePath;
                t.PreActionExtModelList = _actionExtServices.CheckActionExt(RoutePath, 0);
                t.FinishActionExtModelList = _actionExtServices.CheckActionExt(RoutePath, 1);
            }
        }

        public string SyncOaOrder(int userId,string startDate, string endTime, string companyList, string customer, string recid)
        {
            return this.FromOaOrder(null, "createWorkFlow", "发送日程", userId, startDate,  endTime,  companyList,  customer,  recid);
        }
        public string FromOaOrder(IDictionary<string, object> detail, string filterKey, string orignalName, int userId,string startDate,string endTime,string companyList,string customer,string recId)
        {
            string logId = string.Empty;
            try
            {
                var config = _soapServices.ValidConfig("CustomerVisitSoap", filterKey, orignalName);
                if (config.Flag == 0)
                {
                    return HandleResult(config).ToString();
                }

                var interfaces = (config.Data as SoapInterfacesCollection).Interfaces;
                var soapConfig = interfaces.FirstOrDefault(t => t.FunctionName == filterKey);
                System.Net.WebHeaderCollection headers = new WebHeaderCollection();
                headers.Add("token", _soapServices.AuthToLoginERP(userId));
                logId = SoapHttpHelper.Log(new List<string> { "soapparam", "soapurl" }, new List<string> { string.Empty, soapConfig.SoapUrl }, 0, userId).ToString();
                var result = HttpLib.Get(soapConfig.SoapUrl + "?startTime=" + startDate + "&endTime=" + endTime + "&companyList=" + companyList + "&customer="+ customer+ "&recid="+ recId, headers);
                SoapHttpHelper.Log(new List<string> { "soapresresult", "soapexceptionmsg" }, new List<string> { result, string.Empty }, 1, userId, logId.ToString());
                JObject jObject = JObject.Parse(result);
                if ((Convert.ToInt32(jObject["requestId"]) > 0))
                {
                    return "Success";
                }
                else
                {
                    return "false";
                }
            }
            catch (Exception ex)
            {
                return "false";
            }
        }
      



    }
}
