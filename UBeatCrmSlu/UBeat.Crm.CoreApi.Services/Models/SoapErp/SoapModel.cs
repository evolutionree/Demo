using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.SoapErp
{
    public class SoapConfig
    {
        public string SoapUrl { get; set; }
        public string FunctionName { get; set; }
        public List<SoapParam> Params { get; set; }
    }
    public class SoapParam
    {
        public string ParamType { get; set; }
        public string ParamName { get; set; }
    }
    public class ToCustomerSoap
    {
        [JsonProperty("groupcodeid")]
        public string PreCustomer { get; set; }
        [JsonProperty("code")]
        public string CustCode { get; set; }
        [JsonProperty("email")]
        public string RecName { get; set; }
        [JsonProperty("regAdd")]
        public string Address { get; set; }
        [JsonProperty("bank")]
        public string Bank { get; set; }
        [JsonProperty("account")]
        public string Account { get; set; }
        [JsonProperty("taxRegNo")]
        public string TaxNumber { get; set; }
        [JsonProperty("creditLimited")]
        public string CreditLine { get; set; }
        [JsonProperty("paymentMethodId")]
        public string PayInstrument { get; set; }
        [JsonProperty("businessManId")]
        public string RecManager { get; set; }

        [JsonProperty("attach")]
        public string[] Attach { get; set; }
        [JsonProperty("invoiceattach")]
        public string[] InvoiceAttach { get; set; }
        [JsonProperty("saleType")]
        public string SaleType { get; set; }
        [JsonProperty("assistantId")]
        public string FollowUser { get; set; }
        [JsonProperty("ename")]
        public string EnglishName { get; set; }
        //  [JsonProperty("precustomer")]
        public string Country { get; set; }
        //       [JsonProperty("precustomer")]
        public string CustRegion { get; set; }
        [JsonProperty("areaid")]
        public string AreaId { get { return Country + CustRegion; } }
        [JsonProperty("officeAdd")]
        public string CustAddr { get; set; }
        [JsonProperty("reditId")]
        public string Level { get; set; }
        [JsonProperty("taxId")]
        public string TaxCategory { get; set; }
        [JsonProperty("ifTaxPrice")]
        public string IncludTax { get; set; }
        [JsonProperty("currencyId")]
        public string Currency { get; set; }
        [JsonProperty("customerId")]
        public string TaoZhang { get; set; }
    }
}
