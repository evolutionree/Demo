using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.Services.Utility;

namespace UBeat.Crm.CoreApi.Services.Models.SoapErp
{
    public class ErpSoapInterfaces
    {
        public SoapConfig AuthSoap { get; set; }
        public SoapConfig CustomerSoap { get; set; }
    }
    public class SoapInterfacesCollection
    {
        public string SoapBasicUrl { get; set; }
        public List<SoapConfig> Interfaces { get; set; }
    }
    public class SoapConfig
    {
        public string SoapUrl { get; set; }
        public string FunctionName { get; set; }
        public List<SoapParam> Params { get; set; }
        public int IsSingleParam { get; set; }
    }
    public class SoapParam
    {
        public int IsComplex { get; set; }
        public string ParamType { get; set; }
        public string ParamName { get; set; }
        public string DefaultValue { get; set; }
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
        [DataType(DataTypeEnum.ChoosePerson)]
        [JsonProperty("businessManId")]
        public string RecManager { get; set; }
        [JsonIgnore]
        //   [JsonProperty("attach")]
        public string[] Attach { get; set; }
        // [JsonProperty("invoiceattach")]
        [JsonIgnore]
        public string[] InvoiceAttach { get; set; }
        [DataType(DataTypeEnum.SingleChoose)]
        [JsonProperty("saleType")]
        public string SaleType { get; set; }
        [DataType(DataTypeEnum.ChoosePerson)]
        [JsonProperty("assistantId")]
        public string FollowUser { get; set; }
        [JsonProperty("ename")]
        public string EnglishName { get; set; }
        //       [JsonProperty("precustomer")]
        [JsonIgnore]
        public string CustRegion { get; set; }
        [DataType(DataTypeEnum.SingleChoose)]
        [JsonProperty("areaid")]
        public string Country { get; set; }
        [DataType(DataTypeEnum.Address)]
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
        [DataType(DataTypeEnum.SingleChoose)]
        [JsonProperty("customerId")]
        public string TaoZhang { get; set; }
        [DataType(DataTypeEnum.ChoosePerson)]
        [JsonProperty("operator")]
        public string RecCreator { get; set; }
    }

    public class FromProductParam
    {
        public string startDate { get; set; }
        public string endDate { get; set; }
    }
    [EntityInfo("59cf141c-4d74-44da-bca8-3ccf8582a1f2")]
    public class FromProductSoap
    {
        [EntityField("customermodel")]
        public string salesPartName { get; set; }
        [DataType(DataTypeEnum.DataSouce)]
        [EntityField("cust", FieldTypeEnum.Jsonb)]
        public string code { get; set; }
        [EntityField("productcode")]
        public string salesPartNum { get; set; }
        [EntityField("productprice")]
        public string averageCostByMonthCurrent { get; set; }
        [EntityField("productdesciption")]
        public string jobSpec { get; set; }
        [EntityField("note")]
        public string note { get; set; }
        [DataType(DataTypeEnum.SingleChoose)]
        [EntityField("class", FieldTypeEnum.Int)]
        public string textValue { get; set; }
        [DataType(DataTypeEnum.SingleChoose)]
        [EntityField("subclass", FieldTypeEnum.Int)]
        public string appCategory { get; set; }
    }
}
