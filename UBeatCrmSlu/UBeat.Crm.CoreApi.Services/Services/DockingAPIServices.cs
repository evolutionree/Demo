using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Utility;
using System.Linq;
namespace UBeat.Crm.CoreApi.Services.Services
{
    public class DockingAPIServices : BaseServices
    {
        class StaticMessageTip
        {
            public static string NOTAUTHMESSAGE = "接口未获取授权";
        }
        private readonly IDockingAPIRepository _dockingAPIRepository;
        public DockingAPIServices(IDockingAPIRepository dockingAPIRepository)
        {
            _dockingAPIRepository = dockingAPIRepository;
        }
        public OutputResult<object> GetBusinessList(CompanyModel api)
        {
            string[] splitStr = api.CompanyName.Split(" ");
            var outResult = new List<CompanySampleInfo>();
            var result = new List<CompanySampleInfo>();
            foreach (var str in splitStr)
            {
                api.CompanyName = str;
                result = BuildCompanySamples(api);
                outResult.AddRange(result);
            }
            return new OutputResult<object>(new CompanyInfoAPISubResult { Items = outResult, Total = outResult.Count });
        }
        public OutputResult<object> GetBusinessDetail(CompanyModel api, int userId)
        {
            var t = new CompanyInfo();
            var tmp = _dockingAPIRepository.GetBussinessInfomation("basicinfo", 1, api.CompanyName, userId);
            if (tmp != null && tmp.FirstOrDefault() != null && !string.IsNullOrEmpty(tmp.FirstOrDefault().BasicInfo))
                return new OutputResult<object>(JsonConvert.DeserializeObject<CompanyInfo>(tmp.FirstOrDefault().BasicInfo.Replace("\\","")));
            t = BuildCompanyInfo(new DockingAPIModel { CompanyName = api.CompanyName, AppKey = api.AppKey });
            if (t != null&&!string.IsNullOrEmpty(t.Id))
            {
                var t1 = BuildCompanyContactInfo(new DockingAPIModel { CompanyName = api.CompanyName, AppKey = api.AppKey });
                if (t1 != null)
                {
                    t.Telephone = t1.Telephone;
                    t.Email = t1.Email;
                    t.Address = t1.Address;
                }
                else
                {
                    t.Telephone = StaticMessageTip.NOTAUTHMESSAGE;
                    t.Email = StaticMessageTip.NOTAUTHMESSAGE;
                    t.Address = StaticMessageTip.NOTAUTHMESSAGE;
                }
                var t2 = BuildCompanyLogoInfo(new DockingAPIModel { CompanyName = api.CompanyName, AppKey = api.AppKey });
                if (t2 != null)
                {
                    t.Logo = t2.Logo;
                }
                else
                    t.Logo = StaticMessageTip.NOTAUTHMESSAGE;
                if (t != null)
                {
                    _dockingAPIRepository.InsertBussinessInfomation(new BussinessInformation { CompanyName = api.CompanyName, BasicInfo = JsonConvert.SerializeObject(t) }, userId);
                }
            }
            return new OutputResult<object>(t);
        }
        List<CompanySampleInfo> BuildCompanySamples(CompanyModel api)
        {
            string result = HttpLib.Get(string.Format(DockingAPIHelper.ADVSEARCH_API, api.CompanyName, "name", api.AppKey, api.SkipNum));
            var jObject = JObject.Parse(result);
            if (jObject["status"].ToString() == "200")
            {
                var data = JsonConvert.DeserializeObject<CompanyAPISubResult>(jObject["data"] == null ? string.Empty : jObject["data"].ToString());
                return data.Items ?? new List<CompanySampleInfo>();
            }
            else if (jObject["status"].ToString() == "105") return null;
            return new List<CompanySampleInfo>();
        }
        CompanyInfo BuildCompanyInfo(DockingAPIModel api)
        {
            string result = HttpLib.Get(string.Format(DockingAPIHelper.GETBASICINFO_API, api.AppKey, api.CompanyName));
            var jObject = JObject.Parse(result);
            if (jObject["status"].ToString() == "200")
            {
                var data = JsonConvert.DeserializeObject<CompanyInfo>(jObject["data"] == null ? string.Empty : jObject["data"].ToString());
                return data ?? new CompanyInfo();
            }
            return null;
        }

        CompanyInfo BuildCompanyContactInfo(DockingAPIModel api)
        {
            string result = HttpLib.Get(string.Format(DockingAPIHelper.GETCONTACTINFO_API, api.CompanyName, api.AppKey));
            var jObject = JObject.Parse(result);
            if (jObject["status"].ToString() == "200")
            {
                var data = JsonConvert.DeserializeObject<CompanyInfo>(jObject["data"] == null ? string.Empty : jObject["data"].ToString());
                return data ?? new CompanyInfo();
            }
            return null;
        }
        CompanyLogoInfo BuildCompanyLogoInfo(DockingAPIModel api)
        {
            string result = HttpLib.Get(string.Format(DockingAPIHelper.GETENTLOGOBYNAME_API, api.CompanyName, api.AppKey));
            var jObject = JObject.Parse(result);
            if (jObject["status"].ToString() == "200")
            {
                var data = JsonConvert.DeserializeObject<CompanyLogoInfo>(jObject["data"] == null ? string.Empty : jObject["data"].ToString());
                return data ?? new CompanyLogoInfo();
            }
            return null;
        }


        public OutputResult<object> GetYearReport(DockingAPIModel api, int userId)
        {
            var result = BuildYearReport(api);
            if (result != null)
            {
                _dockingAPIRepository.UpdateBussinessInfomation(new BussinessInformation { CompanyName = api.CompanyName, YearReport = JsonConvert.SerializeObject(result) }, userId);
            }
            return new OutputResult<object>(result);
        }
        YearReportAPISubResult BuildYearReport(DockingAPIModel api)
        {
            string result = HttpLib.Get(string.Format(DockingAPIHelper.GETREPORTLISTBYNAME_API, api.AppKey, api.CompanyName));
            var jObject = JObject.Parse(result);
            if (jObject["status"].ToString() == "200")
            {
                var data = JsonConvert.DeserializeObject<YearReportAPISubResult>(jObject["data"] == null ? string.Empty : jObject["data"].ToString());
                return data ?? new YearReportAPISubResult();
            }
            else if (jObject["status"].ToString() == "105") return null;
            return new YearReportAPISubResult();
        }

        public OutputResult<object> GetLawSuit(DockingAPIModel api, int userId)
        {
            var result = BuildLawSuit(api);
            if (result != null)
            {
                _dockingAPIRepository.UpdateBussinessInfomation(new BussinessInformation { CompanyName = api.CompanyName, LawSuit = JsonConvert.SerializeObject(result) }, userId);
            }
            return new OutputResult<object>(result);
        }
        LawSuitAPISubResult BuildLawSuit(DockingAPIModel api)
        {
            string result = HttpLib.Get(string.Format(DockingAPIHelper.GETLAWSUITLISTBYNAME_API, api.AppKey, api.CompanyName, api.SkipNum));
            var jObject = JObject.Parse(result);
            if (jObject["status"].ToString() == "200")
            {
                var data = JsonConvert.DeserializeObject<LawSuitAPISubResult>(jObject["data"] == null ? string.Empty : jObject["data"].ToString());
                return data ?? new LawSuitAPISubResult();
            }
            else if (jObject["status"].ToString() == "105") return null;
            return new LawSuitAPISubResult();
        }
        public OutputResult<object> GetCaseDetail(DockingAPIModel api, int userId)
        {
            var result = BuildCaseDetail(api);
            if (result != null)
            {
                _dockingAPIRepository.UpdateBussinessInfomation(new BussinessInformation { CompanyName = api.CompanyName, CaseDetail = JsonConvert.SerializeObject(result) }, userId);
            }
            return new OutputResult<object>(result);
        }
        CaseDetailAPISubResult BuildCaseDetail(DockingAPIModel api)
        {
            string result = HttpLib.Get(string.Format(DockingAPIHelper.GETCASEDETAILLISTBYNAME_API, api.AppKey, api.CompanyName, api.SkipNum));
            var jObject = JObject.Parse(result);
            if (jObject["status"].ToString() == "200")
            {
                var data = JsonConvert.DeserializeObject<CaseDetailAPISubResult>(jObject["data"] == null ? string.Empty : jObject["data"].ToString());
                return data ?? new CaseDetailAPISubResult();
            }
            else if (jObject["status"].ToString() == "105") return null;
            return new CaseDetailAPISubResult();
        }

        public OutputResult<object> GetCourtNotice(DockingAPIModel api, int userId)
        {
            var result = BuildCourtNotice(api);
            if (result != null)
            {
                _dockingAPIRepository.UpdateBussinessInfomation(new BussinessInformation { CompanyName = api.CompanyName, CourtNotice = JsonConvert.SerializeObject(result) }, userId);
            }
            return new OutputResult<object>(result);
        }
        CourtNoticeAPISubResult BuildCourtNotice(DockingAPIModel api)
        {
            string result = HttpLib.Get(string.Format(DockingAPIHelper.GETNOTICELISTBYNAME_API, api.AppKey, api.CompanyName, api.SkipNum));
            var jObject = JObject.Parse(result);
            if (jObject["status"].ToString() == "200")
            {
                var data = JsonConvert.DeserializeObject<CourtNoticeAPISubResult>(jObject["data"] == null ? string.Empty : jObject["data"].ToString());
                return data ?? new CourtNoticeAPISubResult();
            }
            else if (jObject["status"].ToString() == "105") return null;
            return new CourtNoticeAPISubResult();
        }
        public OutputResult<object> GetBuildBreakPromise(DockingAPIModel api, int userId)
        {
            var result = BuildBreakPromise(api);
            if (result != null)
            {
                _dockingAPIRepository.UpdateBussinessInfomation(new BussinessInformation { CompanyName = api.CompanyName, BreakPromise = JsonConvert.SerializeObject(result) }, userId);
            }
            return new OutputResult<object>(result);
        }
        BreakPromiseAPISubResult BuildBreakPromise(DockingAPIModel api)
        {
            string result = HttpLib.Get(string.Format(DockingAPIHelper.GETNOTICELISTBYNAME_API, api.AppKey, api.CompanyName, api.SkipNum));
            var jObject = JObject.Parse(result);
            if (jObject["status"].ToString() == "200")
            {
                var data = JsonConvert.DeserializeObject<BreakPromiseAPISubResult>(jObject["data"] == null ? string.Empty : jObject["data"].ToString());
                return data ?? new BreakPromiseAPISubResult();
            }
            else if (jObject["status"].ToString() == "105") return null;
            return new BreakPromiseAPISubResult();
        }
    }
}
