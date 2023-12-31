﻿using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.Core.Utility;

namespace UBeat.Crm.CoreApi.DomainModel
{
    public class BussinessInformation
    {
        public string CompanyName { get; set; }
        public string BasicInfo { get; set; }
        public string YearReport { get; set; }
        public string CaseDetail { get; set; }
        public string LawSuit { get; set; }
        public string CourtNotice { get; set; }
        public string BreakPromise { get; set; }
        public string RecUpdated { get; set; }
        public string Id { get; set; }
    }
    public class CustomerInfo
    {
        public string Name { get; set; }
        public string OriginalName { get; set; }
        public string CreditCode { get; set; }
        public string No { get; set; }
        public string OrgNo { get; set; }
        public string econKind { get; set; }
        public string EntType { get; set; }
        public string Status { get; set; }
        public string RegistCapi { get; set; }
        public string RecCap { get; set; }
        public string BelongOrg { get; set; }
        public string StartDate { get; set; }
        public string OperName { get; set; }
        public string Address { get; set; }
        public string Scope { get; set; }
        public string IsOnStock { get; set; }
    }
    public class Bussiness
    {
        public string CompanyName { get; set; }
    }

    public class APIModel
    {
        public string AppKey { get; set; }
        public string Secret { get; set; }
        private static IConfigurationRoot configuration = ServiceLocator.Current.GetInstance<IConfigurationRoot>();
        public APIModel()
        {
            this.AppKey = configuration.GetSection("QiChaCha").Get<QiChaCha>().KEY;
            this.Secret = configuration.GetSection("QiChaCha").Get<QiChaCha>().SECRET;
        }
        public int SkipNum { get; set; }
    }

    public class QiChaCha
    {
        public String KEY { get; set; }
        public String SECRET { get; set; }
    }

    public class APIResult
    { 
        public int Total { get; set; }
        public int Num { get; set; }
    }
    public class DockingAPIModel : APIModel
    {
        public string CompanyName { get; set; }
    }
    public class CompanyModel : APIModel
    {
        public string Id { get; set; }
        public string CompanyName { get; set; }
        public string Country { get; set; }
    }
    public class CompanyAPISubResult : APIResult
    {
        public List<CompanySampleInfo> Items { get; set; }
        public List<CompanySampleInfo> Result { get; set; }
    }
    public class ForeignCompanyAPISubResult : APIResult
    {
        public List<ForeignCompanySampleInfo> Items { get; set; }
    }
    public class HKCompanyAPISubResult : APIResult
    {
        public List<HKCompanySampleInfo> Items { get; set; }
    }
    public class AbstractCompanyInfo
    { }
    public class CompanySampleInfo : AbstractCompanyInfo
    {
        public string Name { get; set; }
        public string Id { get=>KeyNo; 
            set=> KeyNo=value; }
        //企查查ID字段
        public string KeyNo { get; set; }
    }
    public class ForeignCompanySampleInfo : AbstractCompanyInfo
    {
        public string RecUpdated { get; set; }
        [JsonProperty("companyName")]
        public string Name { get; set; }
        [JsonProperty("id3a")]
        public string Id { get; set; }
        public string TradingName { get; set; }
        public string AliasName { get; set; }
        public string CountryCode { get; set; }
        public string Country { get; set; }
        public string StateProvince { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string Street { get; set; }
        public string PostCode { get; set; }
        public string Website { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public string Status { get; set; }
        public string Listed { get; set; }
        public string TypeOfEntity { get; set; }
        public string DateOfIncorporation { get; set; }
        public string SalesLastYear { get; set; }
        public string LastYear { get; set; }
    }
    public class HKCompanySampleInfo : AbstractCompanyInfo
    {
        public string Case_No { get; set; }
        public string Name_Cn { get; set; }
        public string Name_En { get; set; }
        public string Type { get; set; }
        public string Date { get; set; }
        public string Status { get; set; }
        public string Condition { get; set; }
        public string Model { get; set; }
        public string Dissolution_Date { get; set; }
        public string Register_Charges { get; set; }
        public string Important_Matter { get; set; }
       
    }
    public class CompanyInfoAPISubResult : APIResult
    {
        public List<AbstractCompanyInfo> Items { get; set; }
    }
    public class CompanyInfo
    {
        //启信宝字段
        //public string Id { get; set; }
        //public string Name { get; set; }
        //public string OperName { get; set; }
        //public string StartDate { get; set; }
        //public string RegistCapi { get; set; }
        //public string Telephone { get; set; }
        //public string Email { get; set; }
        //public string Address { get; set; }
        //public string Logo { get; set; }
        //public string EconKind { get; set; }
        //public string EconKindCode { get; set; }
        //public string RegNo { get; set; }
        //public string Scope { get; set; }
        //public string TermStart { get; set; }
        //public string TermEnd { get; set; }
        //public string BelongOrg { get; set; }
        //public string EndDate { get; set; }
        //public string CheckDate { get; set; }
        //public string Status { get; set; }
        //public string OrgNo { get; set; }
        //public string CreditNo { get; set; }
        //public string DistrictCode { get; set; }
        //public string DistrictCode_Name { get; set; }
        //public string Domain { get; set; }
        //public string RecUpdated { get; set; }
        //企查查字段
        public string KeyNo { get; set; }
        public string Name { get; set; }
        public string No { get; set; }
        public string BelongOrg { get; set; }
        public string OperName { get; set; }
        public string StartDate { get; set; }
        public string CreditCode { get; set; }
        public string RegistCapi { get; set; }
        public string EconKind { get; set; }
        public string Status { get; set; }
        public string Address { get; set; }
        public string Scope { get; set; }
        public string OrgNo { get; set; }
        public string IsOnStock { get; set; }
        public List<OriginalNameObj> OriginalName { get; set; }
        public string OriginalNameStr { get; set; }
        public string EntType { get; set; }
        public string RecCap { get; set; }
        
        public string Penalty { get; set; }
        public string Exceptions { get; set; }
        public string Shixin { get; set; }
        

    }
    public class OriginalNameObj
    {
        public string Name { get; set; }
    }
    public class CompanyContactInfo
    {
        public string Name { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
    }

    public class CompanyLogoInfo
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string Logo { get; set; }
    }
    public class YearReportAPISubResult : APIResult
    {
        public List<YearReport> Items { get; set; }
    }
    public class YearReport
    {
        [JsonProperty("report_year")]
        public string ReportYear { get; set; }
        [JsonProperty("report_date")]
        public string ReportDate { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("reg_no")]
        public string RegNo { get; set; }
        [JsonProperty("credit_no")]
        public string CreditNo { get; set; }
        [JsonProperty("telephone")]
        public string Telephone { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("address")]
        public string Address { get; set; }
        [JsonProperty("oper_name")]
        public string OperName { get; set; }
        [JsonProperty("zip_code")]
        public string ZipCode { get; set; }
        [JsonProperty("reg_capi")]
        public string RegCapi { get; set; }
        [JsonProperty("IfInvest")]
        public string if_invest { get; set; }
        [JsonProperty("if_website")]
        public string IfWebsite { get; set; }
        [JsonProperty("if_equity")]
        public string IfEquity { get; set; }
        [JsonProperty("if_external_guarantee")]
        public string IfExternalGuarantee { get; set; }
        [JsonProperty("collegues_num")]
        public string ColleguesNum { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("origin_status")]
        public string OriginStatus { get; set; }
        [JsonProperty("debit_amount")]
        public string DebitAmount { get; set; }
        [JsonProperty("net_amount")]
        public string NetAmount { get; set; }
        [JsonProperty("prac_person_num")]
        public string PracPersonNum { get; set; }
        [JsonProperty("profit_reta")]
        public string ProfitReta { get; set; }
        [JsonProperty("profit_total")]
        public string ProfitTotal { get; set; }
        [JsonProperty("tax_total")]
        public string TaxTotal { get; set; }
        [JsonProperty("total_equity")]
        public string TotalEquity { get; set; }
        [JsonProperty("fare_scope")]
        public string FareScope { get; set; }
        [JsonProperty("serv_fare_income")]
        public string ServFareIncome { get; set; }
        [JsonProperty("websites")]
        public List<WebSite> WebSites { get; set; }
        [JsonProperty("stock_changes")]
        public List<StockChanges> StockChanges { get; set; }
        [JsonProperty("invest_items")]
        public List<StockChanges> InvestItems { get; set; }
        [JsonProperty("guarantee_items")]
        public List<GuaranteeItems> GuaranteeItems { get; set; }
    }
    public class WebSite
    {
        [JsonProperty("web_type")]
        public string WebType { get; set; }
        [JsonProperty("web_name")]
        public string WebName { get; set; }
        [JsonProperty("web_url")]
        public string WebUrl { get; set; }
    }
    public class StockChanges
    {
        [JsonProperty("before_percent")]
        public string BeforePercent { get; set; }
        [JsonProperty("after_percent")]
        public string AfterPercent { get; set; }
        [JsonProperty("change_date")]
        public string ChangeDate { get; set; }
    }
    public class InvestItems
    {
        [JsonProperty("invest_name")]
        public string InvestName { get; set; }
        [JsonProperty("invest_reg_no")]
        public string InvestRegNo { get; set; }
        [JsonProperty("invest_capi")]
        public string InvestCapi { get; set; }
        [JsonProperty("invest_percent")]
        public string InvestPercent { get; set; }
    }
    public class partners
    {
        [JsonProperty("stock_name")]
        public string StockName { get; set; }
        [JsonProperty("stock_type")]
        public string StockType { get; set; }
        [JsonProperty("stock_percent")]
        public string StockPercent { get; set; }
        [JsonProperty("identify_type")]
        public string IdentifyType { get; set; }
        [JsonProperty("identify_no")]
        public string IdentifyNo { get; set; }
        [JsonProperty("should_capi_items")]
        public List<ShouldCapiItems> ShouldCapiItems { get; set; }
        [JsonProperty("real_capi_items")]
        public List<RealCapiItems> RealCapiItems { get; set; }
    }
    public class ShouldCapiItems
    {
        [JsonProperty("invest_type")]
        public string InvestType { get; set; }
        [JsonProperty("shoud_capi")]
        public string ShoudCapi { get; set; }
        [JsonProperty("should_capi_date")]
        public string ShouldCapiDate { get; set; }
    }
    public class RealCapiItems
    {
        [JsonProperty("real_capi")]
        public string RealCapi { get; set; }
        [JsonProperty("invest_type")]
        public string InvestType { get; set; }
        [JsonProperty("real_capi_date")]
        public string RealCapiDate { get; set; }
    }
    public class GuaranteeItems
    {
        [JsonProperty("creditor")]
        public string Creditor { get; set; }
        [JsonProperty("debitor")]
        public string Debitor { get; set; }
        [JsonProperty("debit_type")]
        public string DebitType { get; set; }
        [JsonProperty("debit_amount")]
        public string DebitAmount { get; set; }
        [JsonProperty("debit_period")]
        public string DebitPeriod { get; set; }
        [JsonProperty("guarant_method")]
        public string GuarantMethod { get; set; }
        [JsonProperty("guarant_period")]
        public string GuarantPeriod { get; set; }
        [JsonProperty("guarant_scope")]
        public string GuarantScope { get; set; }
    }
    public class LawSuitAPISubResult : APIResult
    {
        public List<LawSuit> Items { get; set; }
    }
    public class LawSuit
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("date")]
        public string Date { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("case_no")]
        public string CaseNo { get; set; }
        [JsonProperty("case_cause")]
        public string CaseCause { get; set; }
        [JsonProperty("disabled")]
        public string Disabled { get; set; }
    }
    public class CaseDetailAPISubResult : APIResult
    {
        public List<CaseDetail> Items { get; set; }
    }
    public class CaseDetail
    {
        [JsonProperty("case_id")]
        public string CaseId { get; set; }
        [JsonProperty("hearing_date")]
        public string HearingDate { get; set; }
        [JsonProperty("case_no")]
        public string CaseNo { get; set; }
        [JsonProperty("start_date")]
        public string StartDate { get; set; }
        [JsonProperty("case_status")]
        public string CaseStatus { get; set; }
        [JsonProperty("agent")]
        public string Agent { get; set; }
        [JsonProperty("assistant")]
        public string Assistant { get; set; }
        [JsonProperty("end_date")]
        public string EndDate { get; set; }
        [JsonProperty("related_items")]
        public List<RelatedItems> related_items { get; set; }
    }
    public class RelatedItems
    {
        [JsonProperty("role")]
        public string Role { get; set; }
        [JsonProperty("eid")]
        public string EId { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("entity_type")]
        public string EntityType { get; set; }
    }
    public class CourtNoticeAPISubResult : APIResult
    {
        public List<CourtNotice> Items { get; set; }
    }
    public class CourtNotice
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("content")]
        public string Content { get; set; }
        [JsonProperty("date")]
        public string Date { get; set; }
        [JsonProperty("court")]
        public string Court { get; set; }
        [JsonProperty("person")]
        public string Person { get; set; }
        [JsonProperty("disabled")]
        public string Disabled { get; set; }
    }
    public class BreakPromiseAPISubResult : APIResult
    {
        public List<BreakPromise> Items { get; set; }
    }
    public class BreakPromise
    {
        [JsonProperty("court")]
        public string Court { get; set; }
        [JsonProperty("oper_name")]
        public string OperName { get; set; }
        [JsonProperty("province")]
        public string Province { get; set; }
        [JsonProperty("doc_number")]
        public string DocNumber { get; set; }
        [JsonProperty("date")]
        public string Date { get; set; }
        [JsonProperty("case_number")]
        public string CaseNumber { get; set; }
        [JsonProperty("ex_department")]
        public string ExDepartment { get; set; }
        [JsonProperty("final_duty")]
        public string FinalDuty { get; set; }
        [JsonProperty("execution_status")]
        public string ExecutionStatus { get; set; }
        [JsonProperty("execution_desc")]
        public string ExecutionDesc { get; set; }
        [JsonProperty("publish_date")]
        public string PublishDate { get; set; }
        [JsonProperty("number")]
        public string Number { get; set; }
        [JsonProperty("disabled")]
        public string Disabled { get; set; }
    }
}
