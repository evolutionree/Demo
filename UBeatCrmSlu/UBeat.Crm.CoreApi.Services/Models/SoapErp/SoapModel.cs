using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.Services.Utility;

namespace UBeat.Crm.CoreApi.Services.Models.SoapErp
{
    public class ErpSyncFunc
    {
        public string EntityId { get; set; }
        public string FuncName { get; set; }
        public string FlowId { get; set; }
        public int IsFlow { get; set; }
    }
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
        [JsonProperty("regNo")]
        public string TaxNumber { get; set; }
        [DataType(DataTypeEnum.ChoosePerson)]
        [JsonProperty("businessManId")]
        public string RecManager { get; set; }
        [JsonIgnore]
        public string[] Attach { get; set; }
        [JsonIgnore]
        public string[] InvoiceAttach { get; set; }
        [DataType(DataTypeEnum.SingleChoose)]
        [JsonProperty("saleType")]
        public string SaleType { get; set; }
        [DataType(DataTypeEnum.ChoosePerson)]
        [JsonProperty("assistantId")]
        public string FollowUser { get; set; }
        [JsonProperty("eName")]
        public string EnglishName { get; set; }
        [DataType(DataTypeEnum.Region)]
        [JsonProperty("areaId")]
        public string CustRegion { get; set; }
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
        [JsonProperty("currencyId")]
        public string Currency { get; set; }
        [DataType(DataTypeEnum.SingleChoose)]
        [JsonProperty("companyId")]
        public string TaoZhang { get; set; }
        [JsonProperty("reconcileDate")]
        [EntityField("reconcileDate", FieldTypeEnum.Int)]
        public Int64 DeadLine { get; set; }
        [DataType(DataTypeEnum.ChoosePerson)]
        [JsonProperty("operator")]
        public string RecCreator { get; set; }
        [DataType(DataTypeEnum.ChoosePerson)]
        [JsonProperty("complainmentAssistantId")]
        public string ComplaintAssistant { get; set; }
        [DataType(DataTypeEnum.SingleChoose)]
        [JsonProperty("paymentTermId")]
        public string PayTime { get; set; }
        [DataType(DataTypeEnum.SingleChoose)]
        [JsonProperty("paymentMethodId")]
        public string PayInstrument { get; set; }
        //[DataType(DataTypeEnum.RelateEntity, typeof(CustomerAddress))]
        //[JsonProperty("customerAddress")]
        //public List<IDictionary<string, object>> customerAddress { get; set; }

    }
    [EntityInfo("689bc59b-f60d-4084-b99d-b0a3e406e873")]
    public class CustomerAddress
    {
        [DataType(DataTypeEnum.DataSouce, "CustDataSource")]
        [JsonProperty("custcode")]
        public int customer { get; set; }
        [JsonProperty("location")]
        public string addressname { get; set; }
        [JsonProperty("shipToAddress")]
        public string address { get; set; }
        [JsonProperty("shipToContact")]
        public string contact { get; set; }
        [JsonProperty("shipToPhone")]
        public string phone { get; set; }
        [JsonProperty("tax")]
        [DataType(DataTypeEnum.SingleChoose)]
        public string tax { get; set; }
        [JsonProperty("shipping")]
        [DataType(DataTypeEnum.SingleChoose)]
        public string transportway { get; set; }
        [JsonProperty("currency")]
        [DataType(DataTypeEnum.SingleChoose)]
        public string currency { get; set; }
        [JsonProperty("leadTime")]
        public int transporttime { get; set; }
        [JsonProperty("fob")]
        [DataType(DataTypeEnum.SingleChoose)]
        public int tradeway { get; set; }
        [JsonProperty("pctOverShip")]
        public decimal yzl { get; set; }
        [JsonProperty("qtyOverShip")]
        public decimal yzsl { get; set; }
        [JsonProperty("packRequirements")]
        public string bzyq { get; set; }
        [JsonProperty("inPackingReportUrl")]
        public string khbbq { get; set; }
        [JsonProperty("outPackingReportUrl")]
        public string khxbq { get; set; }
        [JsonProperty("ifActive")]
        [DataType(DataTypeEnum.SingleChoose)]
        [EntityField("ifActive", FieldTypeEnum.Int)]
        public int activate { get; set; }
        [JsonProperty("ifConsignment")]
        [DataType(DataTypeEnum.SingleChoose)]
        [EntityField("ifConsignment", FieldTypeEnum.Int)]
        public int consign { get; set; }
        [JsonProperty("custaddrid")]
        [EntityField("custaddrid", FieldTypeEnum.Text)]
        public string custaddrid { get; set; }

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
        [DataType(DataTypeEnum.DataSouce, "CustDataSource")]
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
        [EntityField("code")]
        public string MiCode { get; set; }
        [EntityField("partrev")]
        public string partRev { get; set; }
        [EntityField("partnum")]
        public string partNum { get; set; }
        [EntityField("salespartrev")]
        public string salespartRev { get; set; }
        [EntityField("longer")]
        public string productLen { get; set; }
        [EntityField("wide")] 
        public string productWid { get; set; }
    }
    public class FromOrderParam
    {
        public string startDate { get; set; }
        public string endDate { get; set; }
    }
    [EntityInfo("af949c2d-a101-46d5-a125-a9d0659959f0")]
    public class FromOrder
    {
        [EntityField("orderid")]
        public int recId { get; set; }
        [EntityField("contractdate")]
        public DateTime contractDate { get; set; }
        [EntityField("contractno")]
        public String contractNo { get; set; }
        [EntityField("factorycode")]
        public String factoryCode { get; set; }
        [EntityField("taxrate")]
        public decimal taxRate { get; set; }
        [EntityField("exchangeRate")]
        public decimal exchangeRate { get; set; }
        [DataType(DataTypeEnum.SingleChoose)]
        [EntityField("currency", FieldTypeEnum.Int)]
        public string currency { get; set; }
        [DataType(DataTypeEnum.RelateEntity, typeof(FromOrderDetail))]
        [EntityField("detail")]
        public List<FromOrderDetail> contractItem { get; set; }
    }
    [EntityInfo("0d6d41d5-f913-4ccf-8ffd-1414fd9ed736")]
    public class FromOrderDetail
    {
        [EntityField("factorycode")]
        public string businessName { get; set; }
        [DataType(DataTypeEnum.SingleChoose)]
        [EntityField("status", FieldTypeEnum.Int)]
        public string status { get; set; }
        [DataType(DataTypeEnum.SingleChoose)]
        [EntityField("approvestatus", FieldTypeEnum.Int)]
        public string approveStatus { get; set; }
        [EntityField("ifnew")]
        public string ifnew { get; set; }
        [EntityField("ifforecast")]
        public string ifforecast { get; set; }
        [EntityField("nonplanned")]
        public string nonplanned { get; set; }
        [EntityField("qtyplanned", FieldTypeEnum.Int)]
        public int qtyplanned { get; set; }
        [EntityField("qtytobeplanned", FieldTypeEnum.Int)]
        public int qty_tobe_planned { get; set; }
        [EntityField("qtyundo", FieldTypeEnum.Int)]
        public int qtyundo { get; set; }
        [EntityField("qtyforecasted", FieldTypeEnum.Int)]
        public int qtyforecasted { get; set; }
        [EntityField("deliveredquantity")]
        public int qtyArray { get; set; }
        [EntityField("qtyassigned")]
        public int qtyassigned { get; set; }
        [EntityField("qtygift")]
        public int qtygift { get; set; }
        [EntityField("qtyshipped")]
        public int qtyshipped { get; set; }
        [EntityField("qtyrepaired")]
        public int qtyrepaired { get; set; }
        [EntityField("qtyorderreturned")]
        public int qtyorderreturned { get; set; }
        [EntityField("qtyreplenishment")]
        public int qtyreplenishment { get; set; }
        [EntityField("qtyreturned")]
        public int qtyreturned { get; set; }
        [EntityField("requireddate")]
        public DateTime requireddate { get; set; }
        [EntityField("marketdate")]
        public DateTime marketdate { get; set; }
        [EntityField("planDate")]
        public DateTime planDate { get; set; }
        [EntityField("price")]
        public decimal price { get; set; }
        [DataType(DataTypeEnum.DataSouce, "CustDataSource")]
        [EntityField("customer", FieldTypeEnum.Jsonb)]
        public String customer { get; set; }
        [DataType(DataTypeEnum.DataSouce, "ProductDataSource")]
        [EntityField("product")]
        public string productCode { get; set; }
        [EntityField("customermodel")]
        public string customermodel { get; set; }
        [EntityField("custproductcode")]
        public string custproductcode { get; set; }
        [EntityField("area")]
        public decimal area { get; set; }
        [EntityField("mainrecid", FieldTypeEnum.Int)]
        public int recid { get; set; }
        [EntityField("itemrecId", FieldTypeEnum.Int)]
        public int itemRecId { get; set; }
    }



    public class FromPackingShipParam
    {
        public string startDate { get; set; }
        public string endDate { get; set; }
    }
    [EntityInfo("b56a7264-46b2-43d2-b22e-e5d777fb00db")]
    public class FromPackingShip
    {
        [EntityField("packingshipid")]
        public int recId { get; set; }
        [EntityField("shippingorderno")]
        public String shipingNotesNumer { get; set; }
        [DataType(DataTypeEnum.DataSouce, "CustDataSource")]
        [EntityField("customer", FieldTypeEnum.Jsonb)]
        public String customerCode { get; set; }
        public String customerName { get; set; }
        [EntityField("address")]
        public String shippingAddress { get; set; }
        [DataType(DataTypeEnum.DateTime)]
        [EntityField("shippingdate")]
        public DateTime shippedDate { get; set; }
        [DataType(DataTypeEnum.RelateEntity, typeof(FromPackingShipDetail))]
        [EntityField("detail")]
        public List<FromPackingShipDetail> packingSlipItem { get; set; }
    }
    [EntityInfo("658159ab-9ace-405b-ae06-00619230aa92")]
    public class FromPackingShipDetail
    {
        [EntityField("packingshipdetailid")]
        public int recId { get; set; }
        [EntityField("packingshipid")]
        public int packingSlipId { get; set; }
        [EntityField("customercontractno")]
        public String contractNumber { get; set; }
        [DataType(DataTypeEnum.DataSouce, "OrderDataSource")]
        [EntityField("orderno", FieldTypeEnum.Jsonb)]
        public String soNumber { get; set; }
        [EntityField("productcode")]
        [DataType(DataTypeEnum.DataSouce, "ProductDataSource")]
        public String salesPartNum { get; set; }
        [EntityField("productname")]
        public String salesPartName { get; set; }
        [EntityField("quantity")]
        public String qtyOfPcsAssigned { get; set; }
        [EntityField("units")]
        public string unit { get; set; }

    }
    [EntityInfo("b56a7264-46b2-43d2-b22e-e5d777fb00db")]
    public class FromPackingShipPrimeCost
    {
        [EntityField("packingshipid")]
        public int packingSlipId { get; set; }
        [EntityField("packingshipdetailid")]
        public int packingSlipItemId { get; set; }
        [EntityField("shippingorderno")]
        public string shipingNotesNumer { get; set; }
        [EntityField("productcode")]
        [DataType(DataTypeEnum.DataSouce, "ProductDataSource")]
        public String salesPartNum { get; set; }
        [EntityField("productname")]
        public String salesPartName { get; set; }
        [EntityField("Starting")]
        public DateTime starting { get; set; }
        [EntityField("Ending")]
        public DateTime ending { get; set; }
        [EntityField("cost")]
        public decimal averageCostByMonth { get; set; }
    }
    public class FromMakeCollectionOrderParam
    {
        public string startDate { get; set; }
        public string endDate { get; set; }
    }
    [EntityInfo("4d581576-cf39-4b05-bfa0-753f47dc8c72")]
    public class FromMakeCollectionsOrder
    {
        [EntityField("makecollectionsorderid")]
        public int recId { get; set; }
        [EntityField("code")]
        public string checkNumber { get; set; }
        [DataType(DataTypeEnum.DataSouce, "CustDataSource")]
        [EntityField("customercode", FieldTypeEnum.Jsonb)]
        public string sourceId { get; set; }
        [EntityField("dateofcollection")]
        public decimal totalAmount { get; set; }
        [EntityField("amountcollected")]
        public DateTime checkDate { get; set; }
        [EntityField("currency")]
        [DataType(DataTypeEnum.SingleChoose)]
        public string currencyId { get; set; }

    }
}
