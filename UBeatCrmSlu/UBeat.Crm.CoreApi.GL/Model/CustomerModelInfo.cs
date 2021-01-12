using System;
using System.Collections.Generic;
using UBeat.Crm.CoreApi.GL.Model;

namespace UBeat.Crm.CoreApi.GL.Model
{
    public class CustomerModelInfo
    {
        public CustomerModelInfo()
        {
        }
    }

    public class SapCustModelResult
    {
        public string TYPE { get; set; }
        public string MESSAGE { get; set; }
        public SapCustModel DATA { get; set; }

    }

    public class SapCustCreateModelResult
    {
        public string TYPE { get; set; }
        public string MESSAGE { get; set; }
        public string PARTNER { get; set; }

    }

    public class SapCustModel
    {
        public List<CUST_MAIN> CUST_MAIN { get; set; }
        public List<CUST_TAXN> CUST_TAXN { get; set; }
        public List<CUST_BANK> CUST_BANK { get; set; }
        public List<CUST_COMP> CUST_COMP { get; set; }
        public List<CUST_SALE> CUST_SALE { get; set; }
        public List<CUST_LOAD> CUST_LOAD { get; set; }
        public List<CUST_CRED> CUST_CRED { get; set; }
    }

    public class CUST_MAIN_MODIFY
    {
        public CUST_MAIN_MODIFY()
        {
            UPDATE = "U";
        }
        public string PARTNER { get; set; }
        public string CRMCUST { get; set; }
        public string UPDATE { get; set; }
        public string LOEVM { get; set; }
        public string KTOKD { get; set; }
        public string ANRED { get; set; }
        public string NAME1 { get; set; }
        public string NAME2 { get; set; }
        public string SORTL { get; set; }
        public string SORT2 { get; set; }
        public string NAME_CO { get; set; }
        public string STR_SUPPL1 { get; set; }
        public string STR_SUPPL2 { get; set; }
        public string STREET { get; set; }
        public string STR_SUPPL3 { get; set; }
        public string LOCATION { get; set; }
        public string PSTLZ { get; set; } //邮编默认123456
        public string ORT01 { get; set; }
        public string LAND1 { get; set; }
        public string REGIO { get; set; }
        public string SPRAS { get; set; }
        public string TELF2 { get; set; }
        public string TELF1 { get; set; }
        public string TELFX { get; set; }
        public string SMTP_ADDR { get; set; }
        public string STKZN { get; set; }
        public string STCD5 { get; set; }
        public string KUKLA { get; set; }
        public string BRAN1 { get; set; }
        public string TAXKD { get; set; }
        public string ZTEXT1 { get; set; }
        public string ZTEXT2 { get; set; }
        public string ZTEXT3 { get; set; }
        public string ZTEXT4 { get; set; }
        public string ZTEXT5 { get; set; }
        public string ZTEXT6 { get; set; }
        public string ZTEXT7 { get; set; }
        public string ZTEXT8 { get; set; }
        public string ZTEXT9 { get; set; }
        public string ZTEXT10 { get; set; }
        public string ZTEXT11 { get; set; }
        public string ZTEXT12 { get; set; }
        public string VBUND { get; set; }
    }

    public class CUST_TAXN_MODIFY
    {
        public string PARTNER { get; set; }
        public string TAXTYPE { get; set; }
    }

    public class CUST_BANK_MODIFY
    {
        public string PARTNER { get; set; }
        public string BKVID { get; set; }
        public string BANKS { get; set; }
        public string BANKL { get; set; }
        public string BANKN { get; set; }
        public string BKREF { get; set; }
        public string KOINH { get; set; }
    }
    public class CUST_COMP_MODIFY
    {
        public string PARTNER { get; set; }
        public string BUKRS { get; set; }
        public string LOEVM { get; set; }
        public string WBRSL { get; set; }
        public string AKONT { get; set; }
        public string ZTERM { get; set; }
    }

    public class CUST_SALE_MODIFY
    {
        public string PARTNER { get; set; }
        public string VKORG { get; set; }
        public string VTWEG { get; set; }
        public string SPART { get; set; }
        public string LOEVM { get; set; }
        public string BZIRK { get; set; }
        public string VKBUR { get; set; }
        public string VKGRP { get; set; }
        public string WAERS { get; set; }
        public string KDGRP { get; set; }
        public string KALKS { get; set; }
        public string VSBED { get; set; }
        public string KLABC { get; set; }
        public string INCO1 { get; set; }
        public string INCO2 { get; set; }
        public string ZTERM { get; set; }
        public string KTGRD { get; set; }
    }

    public class CUST_LOAD_MODIFY
    {
        public string PARTNER { get; set; }
        public string ABLAD { get; set; }
        public string KNFAK { get; set; }
    }
    public class CUST_CRED_MODIFY
    {
        public CUST_CRED_MODIFY()
        {
            CREDIT_LIMIT = 0;
            COMM_TOTAL_L = 0;
            LIMIT_USED = 0;
        }
        public string PARTNER { get; set; }
        public string CREDIT_SGMNT { get; set; }
        public Decimal CREDIT_LIMIT { get; set; }
        public string XBLOCKED { get; set; }
        public string LIMIT_VALID_DATE { get; set; }
        public Decimal COMM_TOTAL_L { get; set; }
        public Decimal LIMIT_USED { get; set; }
    }
    public class CUST_MAIN
    {
        public string PARTNER { get; set; }
        public string CRMCUST { get; set; }
        public string LOEVM { get; set; }
        public string KTOKD { get; set; }
        public string ANRED { get; set; }
        public string NAME1 { get; set; }
        public string NAME2 { get; set; }
        public string SORTL { get; set; }
        public string SORT2 { get; set; }
        public string NAME_CO { get; set; }
        public string STR_SUPPL1 { get; set; }
        public string STR_SUPPL2 { get; set; }
        public string STREET { get; set; }
        public string STR_SUPPL3 { get; set; }
        public string LOCATION { get; set; }
        public string PSTLZ { get; set; } //邮编默认123456
        public string ORT01 { get; set; }
        public string LAND1 { get; set; }
        public string REGIO { get; set; }
        public string SPRAS { get; set; }
        public string TELF2 { get; set; }
        public string TELF1 { get; set; }
        public string TELFX { get; set; }
        public string SMTP_ADDR { get; set; }
        public string STKZN { get; set; }
        public string STCD5 { get; set; }
        public string KUKLA { get; set; }
        public string BRAN1 { get; set; }
        public string TAXKD { get; set; }
        public string ZTEXT1 { get; set; }
        public string ZTEXT2 { get; set; }
        public string ZTEXT3 { get; set; }
        public string ZTEXT4 { get; set; }
        public string ZTEXT5 { get; set; }
        public string ZTEXT6 { get; set; }
        public string ZTEXT7 { get; set; }
        public string ZTEXT8 { get; set; }
        public string ZTEXT9 { get; set; }
        public string ZTEXT10 { get; set; }
        public string ZTEXT11 { get; set; }
        public string ZTEXT12 { get; set; }
        public string VBUND { get; set; }
    }

    public class CUST_TAXN
    {
        public string PARTNER { get; set; }
        public string TAXTYPE { get; set; }
    }

    public class CUST_BANK
    {
        public string PARTNER { get; set; }
        public string BKVID { get; set; }
        public string BANKS { get; set; }
        public string BANKL { get; set; }
        public string BANKN { get; set; }
        public string BKREF { get; set; }
        public string KOINH { get; set; }
    }
    public class CUST_COMP
    {
        public string PARTNER { get; set; }
        public string BUKRS { get; set; }
        public string LOEVM { get; set; }
        public string WBRSL { get; set; }
        public string AKONT { get; set; }
        public string ZTERM { get; set; }
    }

    public class CUST_SALE
    {
        public string PARTNER { get; set; }
        public string VKORG { get; set; }
        public string VTWEG { get; set; }
        public string SPART { get; set; }
        public string LOEVM { get; set; }
        public string BZIRK { get; set; }
        public string VKBUR { get; set; }
        public string VKGRP { get; set; }
        public string WAERS { get; set; }
        public string KDGRP { get; set; }
        public string KALKS { get; set; }
        public string VSBED { get; set; }
        public string KLABC { get; set; }
        public string INCO1 { get; set; }
        public string INCO2 { get; set; }
        public string ZTERM { get; set; }
        public string KTGRD { get; set; }
    }

    public class CUST_LOAD
    {
        public string PARTNER { get; set; }
        public string ABLAD { get; set; }
        public string KNFAK { get; set; }
    }
    public class CUST_CRED
    {
        public CUST_CRED()
        {
            CREDIT_LIMIT = 0;
            COMM_TOTAL_L = 0;
            LIMIT_USED = 0;
        }
        public string PARTNER { get; set; }
        public string CREDIT_SGMNT { get; set; }
        public Decimal CREDIT_LIMIT { get; set; }
        public string XBLOCKED { get; set; }
        public string LIMIT_VALID_DATE { get; set; }
        public Decimal COMM_TOTAL_L { get; set; }
        public Decimal LIMIT_USED { get; set; }
    }

    public class SaveCustomerMainView
    {
        public SaveCustomerMainView()
        {
            id = Guid.NewGuid();
            rectype = new Guid("f9db9d79-e94b-4678-a5cc-aa6e281c1246");//默认
            status = Status.Enable;
            manager = 1;//默认
            createfrom = DataSourceType.SAP;

            salesView = new List<SaveCustomerSalesView>();
            burkView = new List<SaveCustomerBurkView>();
        }
        public Guid id { get; set; }
        public Guid rectype { get; set; }
        public Status status { get; set; }
        public Int32 manager { get; set; }
        public string reccode { get; set; }
        public DataSourceType createfrom { get; set; }

        public Int32 customertype_crmid { get; set; }
        public string customertype_sapcode { get; set; }
        public string companyone { get; set; }
        public Int32 appellation_crmid { get; set; }
        public string appellation_sapcode { get; set; } //默认值003
        public string recname { get; set; }//全称
        public string customername { get; set; }
        public string searchone { get; set; }

        public string address { get; set; }
        public Int32 city_crmid { get; set; }
        public string city { get; set; }
        public Int32 country_crmid { get; set; }
        public string country_sapcode { get; set; }
        public Int32 region_crmid { get; set; }
        public string region_sapcode { get; set; }
        public string postcode { get; set; }

        public string language { get; set; } //默认值ZH
        public string taxphone { get; set; }
        public string extension { get; set; }
        public string mobilephone { get; set; }
        public string fax { get; set; }

        public string email { get; set; }
        public string valueadd { get; set; }
        public string open { get; set; }
        public string opencode { get; set; }
        public string openname { get; set; }
        public string accountcode { get; set; }
        public Int32 salesorganization_crmid { get; set; }
        public string salesorganization_sapcode { get; set; }

        public Int32 distribution_crmid { get; set; }
        public string distribution_sapcode { get; set; }
        public Int32 productgroup_crmid { get; set; }
        public string productgroup_sapcode { get; set; }
        public Int32 custgpone_crmid { get; set; }
        public string custgpone_sapcode { get; set; }
        public Int32 custgptwo_crmid { get; set; }
        public string custgptwo_sapcode { get; set; }
        public Int32 salesarea_crmid { get; set; }
        public string salesarea_sapcode { get; set; }

        public Int32 salesoffice_crmid { get; set; }
        public string salesoffice_sapcode { get; set; }
        public Int32 pricingpro_crmid { get; set; }
        public string pricingpro_sapcode { get; set; } //默认值1
        public Int32 delivery_crmid { get; set; }
        public string delivery_sapcode { get; set; }
        public Int32 shipment_crmid { get; set; }
        public string shipment_sapcode { get; set; }
        public Int32 payment_crmid { get; set; }
        public string payment_sapcode { get; set; }

        public Int32 accountgp_crmid { get; set; }
        public string accountgp_sapcode { get; set; }
        public Int32 taxgp_crmid { get; set; }
        public string taxgp_sapcode { get; set; }
        public Int32 currency_crmid { get; set; } //默认值国内客户
        public string currency_sapcode { get; set; } //默认CNY
        public decimal creditperiod { get; set; } //默认值1000
        public Int32 rules_crmid { get; set; }
        public string rules_sapcode { get; set; } //默认值B2B-EXIST

        public Int32 risktype_crmid { get; set; }
        public string risktype_sapcode { get; set; } //默认值A
        public Int32 checkrules_crmid { get; set; }
        public string checkrules_sapcode { get; set; } //默认值01
        public decimal risklimit { get; set; }
        public string companycode { get; set; } //默认值1000
        public string accountantsub { get; set; } //默认值 1122010000

        public List<SaveCustomerSalesView> salesView { get; set; }
        public List<SaveCustomerBurkView> burkView { get; set; }

        //增量
        public DateTime modifytime { get; set; }
    }

    public class SaveCustomerSalesView
    {
        public SaveCustomerSalesView()
        {
            id = Guid.NewGuid();
            rectype = new Guid("5fb34976-d626-4a8c-b304-d74d8daeb780");//默认
            status = Status.Enable;
            manager = 1;//默认
        }
        public Guid id { get; set; }
        public Guid rectype { get; set; }
        public Status status { get; set; }
        public Int32 manager { get; set; }
        public string reccode { get; set; }

        public Int32 salesorganization_crmid { get; set; }
        public string salesorganization_sapcode { get; set; }
        public Int32 distribution_crmid { get; set; }
        public string distribution_sapcode { get; set; }
        public Int32 productgroup_crmid { get; set; }
        public string productgroup_sapcode { get; set; }
        public Int32 custgpone_crmid { get; set; }
        public string custgpone_sapcode { get; set; }
        public Int32 custgptwo_crmid { get; set; }
        public string custgptwo_sapcode { get; set; }

        public Int32 salesarea_crmid { get; set; }
        public string salesarea_sapcode { get; set; }
        public Int32 salesoffice_crmid { get; set; }
        public string salesoffice_sapcode { get; set; }
        public Int32 pricingpro_crmid { get; set; }
        public string pricingpro_sapcode { get; set; } //默认值1 
        public Int32 delivery_crmid { get; set; }
        public string delivery_sapcode { get; set; }
        public Int32 shipment_crmid { get; set; }
        public string shipment_sapcode { get; set; }

        public Int32 payment_crmid { get; set; }
        public string payment_sapcode { get; set; }
        public Int32 accountgp_crmid { get; set; }
        public string accountgp_sapcode { get; set; }
        public Int32 taxgp_crmid { get; set; }
        public string taxgp_sapcode { get; set; }
        public Int32 currency_crmid { get; set; } //默认值国内客户 
        public string currency_sapcode { get; set; } //默认CNY

        public Guid customer_id { get; set; }
        public string customer_name { get; set; }
        public string customer { get; set; }
    }

    public class SaveCustomerBurkView
    {
        public SaveCustomerBurkView()
        {
            id = Guid.NewGuid();
            rectype = new Guid("62790e11-d468-46c9-96c7-4a3419df88ea");//默认
            status = Status.Enable;
            manager = 1;//默认
        }
        public Guid id { get; set; }
        public Guid rectype { get; set; }
        public Status status { get; set; }
        public Int32 manager { get; set; }
        public string reccode { get; set; }

        public Guid customer_id { get; set; }
        public string customer_name { get; set; }
        public string customer { get; set; }
        public string companycode { get; set; } //默认值1000
        public string accountantsub { get; set; } //默认值 1122010000
    }

    public class CustomerCreditLimitParam
    {
        public Guid RecId { get; set; }
        public Guid EntityId { get; set; }
    }
    public class CustomerCreditLimitModel
    {
        public String TYPE { get; set; }
        public String MESSAGE { get; set; }
        public List<CustomerCreditLimitDataModel> Data { get; set; }
    }
    public class CustomerCreditLimitDataModel
    {
        public String PARTNER { get; set; }
        public String CREDIT_SGMNT { get; set; }
        public String CURRENCY { get; set; }
        public int AMOUNT { get; set; }
        public int CREDIT_LIMIT { get; set; }
        public decimal CREDIT_LIMIT_USED { get; set; }
        public int CREDIT_LIMIT_USEDW { get; set; }
        public int AMOUNT_SEC { get; set; }
        public String CUST_GROUP { get; set; }
        public int CRED_LIM_CALC { get; set; }
        public String XBLOCKED { get; set; }
        public String XCRITICAL { get; set; }
        public String BLOCK_REASON { get; set; }
        public String FOLLOW_UP_DT { get; set; }
        public String RISK_CLASS { get; set; }
        public int CREDIT_GROUP { get; set; }
        public String CREDIT_SGMNT_TXT { get; set; }
        public String DESCRIP { get; set; }
        public String BP_COACH { get; set; }
        public String BP_COACH_LIST { get; set; }
        public int SECURITY_AMNT { get; set; }
        public String SECURITY_WAERS { get; set; }
        public int AMOUNT_DYN { get; set; }
        public String HORIZON_DATE { get; set; }
        public int HORIZON_DAYS { get; set; }
    }
}
