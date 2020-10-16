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
        private readonly IDataSourceRepository _dataSourceRepository;
        public DockingAPIServices(IDockingAPIRepository dockingAPIRepository, IDataSourceRepository dataSourceRepository)
        {
            _dockingAPIRepository = dockingAPIRepository;
            _dataSourceRepository = dataSourceRepository;
        }
        public OutputResult<object> GetBusinessList(CompanyModel api, int userId)
        {
            string[] splitStr = api.CompanyName.Split(" ");
            var outResult = new List<AbstractCompanyInfo>();
            var result = new List<ForeignCompanySampleInfo>();
            var arr = api.Country.Split(",");
            var country = _dataSourceRepository.SelectFieldDicVaue(Convert.ToInt32(arr[0]), userId).FirstOrDefault(t => t.DataId == Convert.ToInt32(arr[1]));
            foreach (var str in splitStr)
            {
                api.CompanyName = str;
                api.Country = country.ExtField2;
                if (country.ExtField2 == "CN")
                    BuildCompanySamples(api).ForEach(t =>
                    {
                        var p = t as AbstractCompanyInfo;
                        outResult.Add(p);
                    });
                else if (country.ExtField2 == "CNHK")
                    BuildCompanySamples(api).ForEach(t =>
                    {
                        var p = t as AbstractCompanyInfo;
                        outResult.Add(p);
                    });
                else
                    BuildForeignCompanySamples(api).ForEach(t =>
                    {
                        var p = t as AbstractCompanyInfo;
                        CompanySampleInfo company = new CompanySampleInfo
                        {
                            Id = t.Id,
                            Name = t.Name
                        };
                        outResult.Add(company);
                    });
            }
            return new OutputResult<object>(new CompanyInfoAPISubResult { Items = outResult, Total = outResult.Count });
        }
        public OutputResult<object> UpdateBusinessInfomation(CompanyModel api, int userId)
        {
            var basicInfo = this.GetBusinessDetail(api, 1, userId);
            var yearReport = BuildYearReport(new DockingAPIModel { CompanyName = api.CompanyName, AppKey = api.AppKey, SkipNum = api.SkipNum });
            var lawSuit = BuildLawSuit(new DockingAPIModel { CompanyName = api.CompanyName, AppKey = api.AppKey, SkipNum = api.SkipNum });
            var caseDetail = BuildCaseDetail(new DockingAPIModel { CompanyName = api.CompanyName, AppKey = api.AppKey, SkipNum = api.SkipNum });
            var courtNotice = BuildCourtNotice(new DockingAPIModel { CompanyName = api.CompanyName, AppKey = api.AppKey, SkipNum = api.SkipNum });
            var breakPromise = BuildBreakPromise(new DockingAPIModel { CompanyName = api.CompanyName, AppKey = api.AppKey, SkipNum = api.SkipNum });
            _dockingAPIRepository.UpdateBussinessInfomation(new BussinessInformation
            {
                CompanyName = api.CompanyName,
                BasicInfo = JsonConvert.SerializeObject(basicInfo.DataBody as CompanyInfo),
                YearReport = JsonConvert.SerializeObject(yearReport),
                LawSuit = JsonConvert.SerializeObject(lawSuit),
                CaseDetail = JsonConvert.SerializeObject(caseDetail),
                CourtNotice = JsonConvert.SerializeObject(courtNotice),
                BreakPromise = JsonConvert.SerializeObject(breakPromise),
            }
                 , userId);
            return HandleResult(new OperateResult
            {
                Flag = 1,
                Msg = DateTime.Now.ToString("yyyy-MM-dd HH:MM:ss")
            });
        }
        public OutputResult<object> UpdateForeignBusinessInfomation(CompanyModel api, int userId)
        {
            var basicInfo = this.GetForeignBusinessDetail(api, 1, userId);
            _dockingAPIRepository.UpdateForeignBussinessInfomation(new BussinessInformation
            {
                Id = api.Id,
                BasicInfo = JsonConvert.SerializeObject(basicInfo.DataBody as ForeignCompanySampleInfo)
            }
                 , userId);
            return HandleResult(new OperateResult
            {
                Flag = 1,
                Msg = DateTime.Now.ToString("yyyy-MM-dd HH:MM:ss")
            });
        }
        String explainDistrictCode(String regionCode, int userId)
        {
            return _dockingAPIRepository.explainDistrictCode(regionCode, userId);
        }
        public OutputResult<object> GetBusinessDetail(CompanyModel api, int isRefresh, int userId)
        {
            var country = _dataSourceRepository.SelectFieldDicVaue(53, userId).FirstOrDefault(t1 => t1.DataId == Convert.ToInt32(api.Country));
            api.Country = country.ExtField2;
            var t = new CompanyInfo();
            var tmp = _dockingAPIRepository.GetBussinessInfomation("basicinfo", 1, api.CompanyName, userId);
            if (isRefresh == 0 && tmp != null && tmp.FirstOrDefault() != null && !string.IsNullOrEmpty(tmp.FirstOrDefault().BasicInfo))
            {
                var data = JsonConvert.DeserializeObject<CompanyInfo>(tmp.FirstOrDefault().BasicInfo);
                data.RecUpdated = tmp.FirstOrDefault().RecUpdated;
                return new OutputResult<object>(data);
            }

            t = BuildCompanyInfo(new DockingAPIModel { CompanyName = api.CompanyName, AppKey = api.AppKey });
            if (t != null && !string.IsNullOrEmpty(t.Id))
            {
                t.DistrictCode_Name = explainDistrictCode(t.DistrictCode ?? String.Empty, userId);
                t.RecUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:MM:ss");
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
        public OutputResult<Object> GetForeignBusinessDetail(CompanyModel api, int isRefresh, int userId)
        {
            var country = _dataSourceRepository.SelectFieldDicVaue(53, userId).FirstOrDefault(t => t.DataId == Convert.ToInt32(api.Country));
            api.Country = country.ExtField2;
            if (api.Country != "CNHK")
            {
                var tmp = _dockingAPIRepository.GetBussinessInfomation("basicinfo", 1, api.CompanyName, userId);
                if (isRefresh == 0 && tmp != null && tmp.FirstOrDefault() != null && !string.IsNullOrEmpty(tmp.FirstOrDefault().BasicInfo))
                {
                    var data = JsonConvert.DeserializeObject<ForeignCompanySampleInfo>(tmp.FirstOrDefault().BasicInfo);
                    data.RecUpdated = tmp.FirstOrDefault().RecUpdated;
                    return new OutputResult<object>(data);
                }
                var actForData = BuildForeignCompanySamples(api);
                if (actForData != null && actForData.Count > 0)
                {
                    return new OutputResult<object>(actForData.FirstOrDefault(t => t.Id == api.CompanyName));
                }
                return new OutputResult<object>(new ForeignCompanySampleInfo());
            }
            else
            {
                var actHKData = BuildHKCompanySamples(api);

                if (actHKData != null && actHKData.Count > 0)
                {
                    return new OutputResult<object>(actHKData.FirstOrDefault(t => t.Name_Cn == api.CompanyName || t.Name_En == api.CompanyName));
                }
                return new OutputResult<object>(new HKCompanySampleInfo());
            }
        }

        public OutputResult<object> SaveForeignBusinessDetail(CompanyModel api, int isRefresh, int userId)
        {
            var arr = api.Country.Split(",");
            var country = _dataSourceRepository.SelectFieldDicVaue(Convert.ToInt32(arr[0]), userId).FirstOrDefault(t => t.DataId == Convert.ToInt32(arr[1]));
            api.Country = country.ExtField2;
            var data = BuildForeignCompanySamples(api);
            if (data != null && data.Count > 0)
            {
                var realData = data.FirstOrDefault(t => t.Id == api.Id);
                var result = _dockingAPIRepository.InsertBussinessInfomation(new BussinessInformation { Id = api.Id, CompanyName = api.CompanyName, BasicInfo = JsonConvert.SerializeObject(realData) }, userId);
                return HandleResult(result);
            }
            return HandleResult(new OperateResult
            {
                Flag = 1
            });
        }

        List<CompanySampleInfo> BuildCompanySamples(CompanyModel api)
        {
            string result = HttpLib.Get(string.Format(DockingAPIHelper.ADVSEARCH_API, api.CompanyName, "name", api.AppKey, api.SkipNum, api.Secret));
            var jObject = JObject.Parse(result);
            if (jObject["status"].ToString() == "200")
            {
                var data = JsonConvert.DeserializeObject<CompanyAPISubResult>(jObject["data"] == null ? string.Empty : jObject["data"].ToString());
                return data.Items ?? new List<CompanySampleInfo>();
            }
            else if (jObject["status"].ToString() == "105") return null;
            return new List<CompanySampleInfo>();
        }

        List<ForeignCompanySampleInfo> BuildForeignCompanySamples(CompanyModel api)
        {
            string result = HttpLib.Get(string.Format(DockingAPIHelper.FORENGIN_ADVSEARCH_API, api.Country, api.CompanyName, api.AppKey, api.Secret));
            var jObject = JObject.Parse(result);
            if (jObject["status"].ToString() == "200")
            {
                var data = JsonConvert.DeserializeObject<ForeignCompanyAPISubResult>(jObject["data"] == null ? string.Empty : jObject["data"].ToString());
                return data.Items ?? new List<ForeignCompanySampleInfo>();
            }
            else if (jObject["status"].ToString() == "105") return null;
            return new List<ForeignCompanySampleInfo>();
        }
        List<HKCompanySampleInfo> BuildHKCompanySamples(CompanyModel api)
        {
            string result = HttpLib.Get(string.Format(DockingAPIHelper.GETHKCOMPANYBYNAME_API, api.Country, api.AppKey, api.Secret, api.CompanyName));
            var jObject = JObject.Parse(result);
            if (jObject["status"].ToString() == "200")
            {
                var data = JsonConvert.DeserializeObject<HKCompanyAPISubResult>(jObject["data"] == null ? string.Empty : jObject["data"].ToString());
                return data.Items ?? new List<HKCompanySampleInfo>();
            }
            else if (jObject["status"].ToString() == "105") return null;
            return new List<HKCompanySampleInfo>();
        }
        CompanyInfo BuildCompanyInfo(DockingAPIModel api)
        {
            string result = HttpLib.Get(string.Format(DockingAPIHelper.GETBASICINFO_API, api.AppKey, api.CompanyName, api.Secret));
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
            string result = HttpLib.Get(string.Format(DockingAPIHelper.GETCONTACTINFO_API, api.CompanyName, api.AppKey, api.Secret));
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
            string result = HttpLib.Get(string.Format(DockingAPIHelper.GETENTLOGOBYNAME_API, api.CompanyName, api.AppKey, api.Secret));
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
            var tmp = _dockingAPIRepository.GetBussinessInfomation("yearreport", 1, api.CompanyName, userId);
            if (tmp != null && tmp.FirstOrDefault() != null && !string.IsNullOrEmpty(tmp.FirstOrDefault().YearReport))
                return new OutputResult<object>(JsonConvert.DeserializeObject<YearReportAPISubResult>(tmp.FirstOrDefault().YearReport));
            var result = BuildYearReport(api);
            if (result != null)
            {
                _dockingAPIRepository.UpdateBussinessInfomation(new BussinessInformation { CompanyName = api.CompanyName, YearReport = JsonConvert.SerializeObject(result) }, userId);
            }
            return new OutputResult<object>(result);
        }
        YearReportAPISubResult BuildYearReport(DockingAPIModel api)
        {
            string result = HttpLib.Get(string.Format(DockingAPIHelper.GETREPORTLISTBYNAME_API, api.AppKey, api.CompanyName, api.Secret));
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
            var tmp = _dockingAPIRepository.GetBussinessInfomation("lawsuit", 1, api.CompanyName, userId);
            if (tmp != null && tmp.FirstOrDefault() != null && !string.IsNullOrEmpty(tmp.FirstOrDefault().LawSuit))
                return new OutputResult<object>(JsonConvert.DeserializeObject<LawSuitAPISubResult>(tmp.FirstOrDefault().LawSuit));
            var result = BuildLawSuit(api);
            if (result != null)
            {
                _dockingAPIRepository.UpdateBussinessInfomation(new BussinessInformation { CompanyName = api.CompanyName, LawSuit = JsonConvert.SerializeObject(result) }, userId);
            }
            return new OutputResult<object>(result);
        }
        LawSuitAPISubResult BuildLawSuit(DockingAPIModel api)
        {
            string result = HttpLib.Get(string.Format(DockingAPIHelper.GETLAWSUITLISTBYNAME_API, api.AppKey, api.CompanyName, api.SkipNum, api.Secret));
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
            var tmp = _dockingAPIRepository.GetBussinessInfomation("casedetail", 1, api.CompanyName, userId);
            if (tmp != null && tmp.FirstOrDefault() != null && !string.IsNullOrEmpty(tmp.FirstOrDefault().CaseDetail))
                return new OutputResult<object>(JsonConvert.DeserializeObject<CaseDetailAPISubResult>(tmp.FirstOrDefault().CaseDetail));
            var result = BuildCaseDetail(api);
            if (result != null)
            {
                _dockingAPIRepository.UpdateBussinessInfomation(new BussinessInformation { CompanyName = api.CompanyName, CaseDetail = JsonConvert.SerializeObject(result) }, userId);
            }
            return new OutputResult<object>(result);
        }
        CaseDetailAPISubResult BuildCaseDetail(DockingAPIModel api)
        {
            string result = HttpLib.Get(string.Format(DockingAPIHelper.GETCASEDETAILLISTBYNAME_API, api.AppKey, api.CompanyName, api.SkipNum, api.Secret));
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
            var tmp = _dockingAPIRepository.GetBussinessInfomation("courtnotice", 1, api.CompanyName, userId);
            if (tmp != null && tmp.FirstOrDefault() != null && !string.IsNullOrEmpty(tmp.FirstOrDefault().CourtNotice))
                return new OutputResult<object>(JsonConvert.DeserializeObject<CourtNoticeAPISubResult>(tmp.FirstOrDefault().CourtNotice));
            var result = BuildCourtNotice(api);
            if (result != null)
            {
                _dockingAPIRepository.UpdateBussinessInfomation(new BussinessInformation { CompanyName = api.CompanyName, CourtNotice = JsonConvert.SerializeObject(result) }, userId);
            }
            return new OutputResult<object>(result);
        }
        CourtNoticeAPISubResult BuildCourtNotice(DockingAPIModel api)
        {
            string result = HttpLib.Get(string.Format(DockingAPIHelper.GETNOTICELISTBYNAME_API, api.AppKey, api.CompanyName, api.SkipNum, api.Secret));
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
            var tmp = _dockingAPIRepository.GetBussinessInfomation("breakpromise", 1, api.CompanyName, userId);
            if (tmp != null && tmp.FirstOrDefault() != null && !string.IsNullOrEmpty(tmp.FirstOrDefault().BreakPromise))
                return new OutputResult<object>(JsonConvert.DeserializeObject<BreakPromiseAPISubResult>(tmp.FirstOrDefault().BreakPromise));
            var result = BuildBreakPromise(api);
            if (result != null)
            {
                _dockingAPIRepository.UpdateBussinessInfomation(new BussinessInformation { CompanyName = api.CompanyName, BreakPromise = JsonConvert.SerializeObject(result) }, userId);
            }
            return new OutputResult<object>(result);
        }
        BreakPromiseAPISubResult BuildBreakPromise(DockingAPIModel api)
        {
            string result = HttpLib.Get(string.Format(DockingAPIHelper.GETNOTICELISTBYNAME_API, api.AppKey, api.CompanyName, api.SkipNum, api.Secret));
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
