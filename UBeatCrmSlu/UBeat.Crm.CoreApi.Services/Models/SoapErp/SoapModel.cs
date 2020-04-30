using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.Services.Utility;

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
    [EntityInfo("f9db9d79-e94b-4678-a5cc-aa6e281c1246")]
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
        [DataType(DataTypeEnum.MultiChoose)]
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
        [DataType(DataTypeEnum.ChoosePerson)]
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
        [DataType(DataTypeEnum.SingleChoose)]
        [JsonProperty("reditId")]
        public string Level { get; set; }
        [DataType(DataTypeEnum.SingleChoose)]
        [JsonProperty("taxId")]
        public string TaxCategory { get; set; }
        [DataType(DataTypeEnum.SingleChoose)]
        [JsonProperty("ifTaxPrice")]
        public string IncludTax { get; set; }
        [DataType(DataTypeEnum.SingleChoose)]
        [JsonProperty("currencyId")]
        public string Currency { get; set; }
        [JsonProperty("customerId")]
        public string TaoZhang { get; set; }
    }
}
