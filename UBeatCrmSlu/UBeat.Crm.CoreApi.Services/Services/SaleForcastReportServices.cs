using System;
using System.Data.Common;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.DynamicEntity;
using UBeat.Crm.CoreApi.DomainModel.Reports;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.ReportDefine;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class SaleForcastReportServices : EntityBaseServices
    {
        public SaleForcastReportServices()
        {
        }
        public Dictionary<string, List<Dictionary<string, object>>> testReportData(
                            Dictionary<string, object> param,
                            Dictionary<string, string> sortby,
                            int pageIndex, int pageCount,
                            int userNum) {
            Dictionary<string, List<Dictionary<string, object>>> ret = new Dictionary<string, List<Dictionary<string, object>>>();
            ret.Add("data", new List<Dictionary<string, object>>());
            return ret;
        }
		
    }
}
