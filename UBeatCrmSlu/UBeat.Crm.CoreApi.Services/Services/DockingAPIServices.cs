﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Utility;
using UBeat.Crm.CoreApi.DomainModel.EntityPro;
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
                string countryStr = "";
                if (country == null)
                {
                    countryStr = "CN";
                }
                else {
                    countryStr = country.ExtField2;
                }
                api.Country = countryStr;
                if (countryStr == "CN")
                    BuildCompanySamples(api).ForEach(t =>
                    {
                        var p = t as AbstractCompanyInfo;
                        outResult.Add(p);
                    });
                else if (countryStr == "CNHK")
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
            //var country = _dataSourceRepository.SelectFieldDicVaue(53, userId).FirstOrDefault(t1 => t1.DataId == Convert.ToInt32(api.Country));
            //api.Country = country.ExtField2;
            var t = new CompanyInfo();
            var tmp = _dockingAPIRepository.GetCustomerInfomation("recname as name, beforename as OriginalNameStr,ucode as CreditCode,businesscode as No,organizationcode as OrgNo,qccenterprisenature as econKind,qccenterprisetype as EntType,enterprisestatus as Status, registeredcapital as RegistCapi, paidcapital as RecCap, registrationauthority as BelongOrg, establishmentdate as StartDate, corporatename as OperName, qcclocation as Address, businessscope as Scope, isiop as IsOnStock,penalty,exceptions,shixin", 1, api.CompanyName, userId);

            if (tmp != null&& tmp.Count!=0) {
                t = tmp[0];
            }
            else {
                t = BuildCompanyInfo(new DockingAPIModel { CompanyName = api.CompanyName, AppKey = api.AppKey, Secret = api.Secret });
                
                if (t != null && !string.IsNullOrEmpty(t.KeyNo))
                {
                    Dictionary<string, string> yesNo = new Dictionary<string, string>();
                    yesNo.Add("0","未上市");
                    yesNo.Add("1", "上市");
                    Dictionary<string, string> entType = new Dictionary<string, string>();
                    entType.Add("0", "公司");
                    entType.Add("1", "社会组织");
                    entType.Add("3", "香港公司");
                    entType.Add("4", "事业单位");
                    entType.Add("5", "");
                    entType.Add("6", "基金会");
                    entType.Add("7", "医院");
                    entType.Add("8", "海外公司");
                    entType.Add("9", "律师事务所");
                    entType.Add("10", "学校");
                    entType.Add("-1", "其他");
                    if (t.OriginalName!=null&&t.OriginalName.Count!=0) {
                        t.OriginalNameStr = t.OriginalName[0].Name;
                        t.OriginalName = null;
                    }
                    if (t.IsOnStock != null)
                    {
                        t.IsOnStock = yesNo[t.IsOnStock];
                    }
                    if (t.EntType != null)
                    {
                        t.EntType = entType[t.EntType];
                    }

                    if (!string.IsNullOrEmpty(t.CreditCode))
                    {
                        var runningInfo=BuildCompanyRunningInfo(new DockingAPIModel
                            {CompanyName = t.CreditCode, AppKey = api.AppKey, Secret = api.Secret});
                        if (runningInfo!=null)
                        {
                            t.Penalty = runningInfo.Penalty;
                            t.Exceptions = runningInfo.Exceptions;
                            t.Shixin = runningInfo.Shixin;
                        }
                    }

                   
                    // _dockingAPIRepository.InsertBussinessInfomation(new BussinessInformation { CompanyName = api.CompanyName, BasicInfo = JsonConvert.SerializeObject(t) }, userId);

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
            //var country = _dataSourceRepository.SelectFieldDicVaue(Convert.ToInt32(arr[0]), userId).FirstOrDefault(t => t.DataId == Convert.ToInt32(arr[1]));
            //api.Country = country.ExtField2;
            //var data = BuildForeignCompanySamples(api);
            var data = BuildCompanySamples(api);
            if (data != null && data.Count > 0)
            {
                //var realData = data.FirstOrDefault(t => t.Id == api.Id);
                //var result = _dockingAPIRepository.InsertBussinessInfomation(new BussinessInformation { Id = api.Id, CompanyName = api.CompanyName, BasicInfo = JsonConvert.SerializeObject(realData) }, userId);
                //return HandleResult(result);
            }
            return HandleResult(new OperateResult
            {
                Flag = 1
            });
        }

        List<CompanySampleInfo> BuildCompanySamples(CompanyModel api)
        {



            //启信宝
            //string result = HttpLib.Get(string.Format(DockingAPIHelper.ADVSEARCH_API, api.CompanyName, "name", api.AppKey, api.SkipNum, api.Secret));
            //var jObject = JObject.Parse(result);
            //企查查
            var url = string.Format(DockingAPIHelper.SEARCHWIDE_API, api.AppKey, api.CompanyName);
            var result = QichachaProgram.httpGet(url, QichachaProgram.getHeaderVals(api.AppKey, api.Secret));
            var jObject = JObject.Parse(result);
            if (jObject["Status"].ToString() == "200")
            {
                var data = JsonConvert.DeserializeObject<CompanyAPISubResult>(jObject["Result"] == null ? string.Empty : jObject.ToString());
                
                return data.Result ?? new List<CompanySampleInfo>();
            }
            else if (jObject["Status"].ToString() == "105") return null;
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
            //string result = HttpLib.Get(string.Format(DockingAPIHelper.GETBASICINFO_API, api.AppKey, api.CompanyName, api.Secret));
            //var jObject = JObject.Parse(result);
            //if (jObject["status"].ToString() == "200")
            //{
            //    var data = JsonConvert.DeserializeObject<CompanyInfo>(jObject["data"] == null ? string.Empty : jObject["data"].ToString());
            //    return data ?? new CompanyInfo();
            //}
            var url = string.Format(DockingAPIHelper.GETDETAILSBYNAME_API, api.AppKey, api.CompanyName);
            var result = QichachaProgram.httpGet(url, QichachaProgram.getHeaderVals(api.AppKey, api.Secret));
            var jObject = JObject.Parse(result);
            if (jObject["Status"].ToString() == "200") {
                var data = JsonConvert.DeserializeObject<CompanyInfo>(jObject["Result"] == null ? string.Empty : jObject["Result"].ToString());
                return data ?? new CompanyInfo();
            }
            return null;
        }
        
        public CompanyInfo BuildCompanyRunningInfo(DockingAPIModel api)
        {
            var url = string.Format(DockingAPIHelper.GETINFO_API, api.AppKey, api.CompanyName);
            var result = QichachaProgram.httpGet(url, QichachaProgram.getHeaderVals(api.AppKey, api.Secret));
            var jObject = JObject.Parse(result);
            if (jObject["Status"].ToString() == "200")
            {
                JToken res = jObject["Result"];
                JArray Penalty = JArray.FromObject(res["Penalty"]);
                JArray Exceptions = JArray.FromObject(res["Exceptions"]);
                JArray ShiXinItems = JArray.FromObject(res["ShiXinItems"]);
                 
                var data = new CompanyInfo();
                data.Penalty = GetJArrayStr(Penalty);
                data.Exceptions = GetJArrayStr(Exceptions);
                data.Shixin = GetJArrayStr(ShiXinItems);
                return data;
            }
            return null;
        }
        
        public string GetJArrayStr(JArray array)
        {
            var infDic = new Dictionary<string, string>();
            infDic.Add("DocNo","决定文书号");
            infDic.Add("PenaltyType","处罚事由");
            infDic.Add("OfficeName","处罚单位");
            infDic.Add("Content","处罚结果");
            infDic.Add("PenaltyDate","处罚日期");
             
            infDic.Add("AddReason","处罚日期");
            infDic.Add("AddDate","列入日期");
            infDic.Add("RomoveReason","移出异常原因");
            infDic.Add("RemoveDate","移出日期");
            infDic.Add("DecisionOffice","作出决定机关");
            infDic.Add("RemoveDecisionOffice","移除决定机关");
             
            infDic.Add("Name","企业名称");
            infDic.Add("Liandate","立案日期");
            infDic.Add("Anno","立案文书号");
            infDic.Add("Orgno","组织机构代码");
            infDic.Add("Executeno","执行依据文号");
            infDic.Add("Publicdate","发布时间");
            infDic.Add("Executestatus", "被执行人的履行情况");
            infDic.Add("Actionremark","行为备注");
            infDic.Add("Executegov","执行法院");
            StringBuilder contentPenalty = new StringBuilder();
            foreach (var jToken in array)
            {
                foreach (JProperty token in jToken)
                {
                    if (infDic.ContainsKey(token.Name)&&token.Value!=null&&token.Value.ToString()!="")
                    {
                        contentPenalty.Append(infDic[token.Name]);
                        contentPenalty.Append(":");
                        contentPenalty.Append(token.Value);
                        contentPenalty.Append(";");
                    }
                    if (token.Next==null&&jToken.Next!=null)
                    {
                        contentPenalty.Append("\n");
                    }
                }
            }
            return contentPenalty.ToString();
        }

        CompanyInfo BuildCompanyContactInfo(DockingAPIModel api)
        {
            
            string result = HttpLib.Get(string.Format(DockingAPIHelper.GETCONTACTINFO_API, api.CompanyName, api.AppKey, api.Secret));
            var jObject = JObject.Parse(result);
            if (jObject["Status"].ToString() == "200")
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
