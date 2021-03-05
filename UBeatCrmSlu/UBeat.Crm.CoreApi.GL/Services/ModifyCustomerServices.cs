using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using UBeat.Crm.CoreApi.DomainModel.DynamicEntity;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.DomainModel.Utility;
using UBeat.Crm.CoreApi.GL.Model;
using UBeat.Crm.CoreApi.GL.Repository;
using UBeat.Crm.CoreApi.GL.Utility;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.DynamicEntity;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.CoreApi.DomainModel;

namespace UBeat.Crm.CoreApi.GL.Services
{
    public class ModifyCustomerServices : BasicBaseServices
    {
        private static readonly Logger logger = LogManager.GetLogger("UBeat.Crm.GL.Services.ModifyCustomerServices");
        private readonly ICustomerRepository _customerRepository;
        private readonly IBaseDataRepository _baseDataRepository;
        private readonly CacheServices _cacheService;
        private BaseDataServices _baseDataServices;
        private readonly CoreApi.IRepository.IDynamicEntityRepository _dynamicEntityRepository;
        private FetchCustomerServices _fetchCustomerServices;
        public ModifyCustomerServices(ICustomerRepository customerRepository,
            IBaseDataRepository baseDataRepository,
            CacheServices cacheService,
            BaseDataServices baseDataServices, FetchCustomerServices fetchCustomerServices, CoreApi.IRepository.IDynamicEntityRepository dynamicEntityRepository)
        {
            _customerRepository = customerRepository;
            _baseDataRepository = baseDataRepository;
            _cacheService = cacheService;
            _baseDataServices = baseDataServices;
            _dynamicEntityRepository = dynamicEntityRepository;
            _fetchCustomerServices = fetchCustomerServices;
        }
        public ModifyCustomerServices() { }
        public SynResultModel SynSapCustData(Guid entityId, Guid recId, int UserId)
        {
            var result = new SynResultModel();
            var detailData = _baseDataServices.GetEntityDetailData(null, entityId, recId, UserId);
            if (detailData != null)
            {
                SynchrosapStatus isSyn = SynchrosapStatus.Yes;
                var sapno = string.Concat(detailData["erpcode"]);
                var issynchrosap = string.Concat(detailData["issynchrosap"]);
                if (!string.IsNullOrEmpty(sapno) && (issynchrosap == "1" || issynchrosap == "4"))
                    isSyn = SynchrosapStatus.No;
                try
                {
                    if (isSyn == SynchrosapStatus.Yes)
                    {
                        result = SynSapAddCustData(detailData, entityId, recId);
                    }
                    else
                    {
                        result = SynSapModifyCustData(detailData, entityId, recId);
                    }
                }
                catch (Exception ex)
                {
                    var str = "同步客户失败，请联系管理员";
                    logger.Info(string.Format(@"{0},{1}", str, ex.Message));
                    result.Message = str;
                }
            }
            else
            {
                result.Message = "同步失败，不存在客户记录";
            }

            return result;
        }

        public SynResultModel SynSapAddCustData(IDictionary<string, object> resultData, Guid entityId, Guid recId, DbTransaction tran = null)
        {
            var result = new SynResultModel();
            var sapResult = string.Empty;

            CUST_MAIN cust = new CUST_MAIN();
            List<CUST_TAXN> CUST_TAXN = new List<CUST_TAXN>();
            List<CUST_BANK> CUST_BANK = new List<CUST_BANK>();
            List<CUST_COMP> CUST_COMP = new List<CUST_COMP>();
            List<CUST_SALE> CUST_SALE = new List<CUST_SALE>();
            List<CUST_LOAD> CUST_LOAD = new List<CUST_LOAD>();
            List<CUST_CRED> CUST_CRED = new List<CUST_CRED>();

            CUST_LOAD load = new CUST_LOAD();
            CUST_LOAD.Add(load);

            #region main
            //CRMCUST, KTOKD,ANRED, TITLE, NAME1, SORTL
            cust.CRMCUST = "CRM" + string.Concat(resultData["reccode"]).StringMax(0, 20);
            //CRMCUST, KTOKD,ANRED, TITLE, NAME1, SORTL,ZTEXT1,TELF1
            cust.CRMCUST = "CRM" + string.Concat(resultData["reccode"]).StringMax(0, 20);

            var customertype = string.Concat(resultData["customertype"]);
            cust.KTOKD = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeEnum.客户账户组, customertype).StringMax(0, 4);//客户账户组

            //字典值，必须改
            cust.ANRED = "0003";

            //cust.TITLE = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeIdEnum.称谓, title).StringMax(0, 4);//地址关键字的表格
            cust.NAME1 = string.Concat(resultData["recname"]).StringMax(0, 35);//组织名称 1
            cust.SORTL = string.Concat(resultData["customername"]).StringMax(0, 10);//简称
            cust.ZTEXT1 = string.Concat(resultData["contacts"]).StringMax(0, 30);//联系人 必填
            cust.TELF1 = string.Concat(resultData["contactnumber"]).StringMax(0, 16);//联系电话 必填
            #endregion

            #region
            // STREET, ORT01, COUNTRY, REGIO
            var address = string.Concat(resultData["address"]);//街道
            if (!string.IsNullOrEmpty(address))
            {
                var ds = JsonHelper.ToObject<Address>(address);
                if (ds != null)
                    cust.STREET = ds.address.StringMax(0, 60);
                else
                    cust.STREET = address;
            }
            var city = string.Concat(resultData["region"]);
            cust.ORT01 = _baseDataRepository.GetRegionFullNameById(city).StringMax(0, 35);//城市
            var country = string.Concat(resultData["country"]);
            cust.LAND1 = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeEnum.国家, country).StringMax(0, 3);//国家/地区代码
            var region = string.Concat(resultData["saparea"]);
            cust.REGIO = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeEnum.地区, region).StringMax(0, 3);//地区（省/自治区/直辖市、市、县）
            #endregion

            #region
            //PSTLZ, LANGU,TAXKD
            cust.PSTLZ = string.Concat(resultData["postcode"]).StringMax(0, 10);//城市邮政编码
            cust.SPRAS = string.Concat(resultData["language"]).StringMax(0, 2);//语言
            var taxkd = string.Concat(resultData["taxgp"]);
            cust.TAXKD = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeEnum.税分类, taxkd).StringMax(0, 1);//客户税分类 
            #endregion

            #region bank
            CUST_BANK bank = new CUST_BANK();

            //银行编号
            //var bankl = string.Concat(resultData["bank"]);
            //bank.BANKL = string.Concat(resultData["bank"]).StringMax(0, 15); 
            //cust.BANKL = _baseDataRepository.GetBankCodeByDataSource(bankl).StringMax(0, 15); 
            //bank.BANKN = string.Concat(resultData["accountcode"]).StringMax(0, 18);//银行账户
            //CUST_BANK.Add(bank);
            #endregion

            #region sale
            CUST_SALE sale = new CUST_SALE();
            //VKORG, VTWEG, SPART, KVGR1, KVGR2 KALKS KTGRD
            var vkorg = string.Concat(resultData["salesorganization"]);
            sale.VKORG = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeEnum.销售组织, vkorg).StringMax(0, 4);//销售组织
            var vtweg = string.Concat(resultData["saledistribution"]);
            sale.VTWEG = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeEnum.分销渠道, vtweg).StringMax(0, 2);//分销渠道
            var spart = string.Concat(resultData["productgroup"]);
            sale.SPART = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeEnum.产品组, spart).StringMax(0, 2);//产品组
            var kvgr1 = string.Concat(resultData["custgpone"]);
            sale.KALKS = "1";//默认1
            sale.KTGRD = "03";//客户科目分配组
            /*sale.KVGR1 = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeEnum.客户组1, kvgr1).StringMax(0, 3);//客户组1
            var kvgr2 = string.Concat(resultData["custgptwo"]);
            sale.KVGR2 = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeEnum.客户组2, kvgr2).StringMax(0, 3);//客户组2*/

            //STCD5,BZIRK, VKBUR, KALKS, VWERK, VSBED,ZTERM
            cust.STCD5 = string.Concat(resultData["taxno"]).StringMax(0, 60);//增值税登记号
            var bzirk = string.Concat(resultData["area"]);
            sale.BZIRK = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeEnum.销售地区, bzirk).StringMax(0, 6);//销售地区 
            var vkbur = string.Concat(resultData["salesoffice"]);
            sale.VKBUR = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeEnum.销售办事处, vkbur).StringMax(0, 4);//销售办事处
            //var kalks = string.Concat(resultData["pricingpro"]);
            //sale.KALKS = kalks.StringMax(0, 2);//用于定价过程确定的客户分类
            //var vwerk = string.Concat(resultData["delivery"]);
            //sale.VWERK = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeEnum.交货工厂, vwerk).StringMax(0, 4);//交货工厂 (自有或外部)
            var vsbed = string.Concat(resultData["shipment"]);
            sale.VSBED = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeEnum.装运条件, vsbed).StringMax(0, 2);//装运条件
            var zterm = string.Concat(resultData["payrequirement"]);
            sale.ZTERM = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeEnum.付款条件, zterm).StringMax(0, 4);//付款条件代码 
            var waers = string.Concat(resultData["currency"]);
            sale.WAERS = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeEnum.币种, waers).StringMax(0, 5);//货币
            var ktgrd = string.Concat(resultData["accountgp"]);
            sale.KTGRD = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeEnum.客户科目分配组, ktgrd).StringMax(0, 2);//账户分配组
            CUST_SALE.Add(sale);
            #endregion

            #region
            // KTGRD, SEGMENT
            //var ktgrd = string.Concat(resultData["accountgp"]);
            //cust.KTGRD = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeIdEnum.账户分配组, ktgrd).StringMax(0, 2);//此客户的账户分配组
            /*var segment_str = string.Concat(resultData["creditperiod"]).StringMax(0, 10);//信用段 
            decimal segment_decimal = 0;
            if (decimal.TryParse(segment_str, out segment_decimal))
            {
                cust.SEGMENT = decimal.Round(segment_decimal, 0).ToString();
            }*/
            #endregion

            #region
            CUST_CRED cred = new CUST_CRED();
            //CREDIT_LIMIT, BUKRS
            var credit_limit = string.Concat(resultData["credit"]).StringMax(0, 15);
            decimal credit_limit_decimal = 0;
            if (decimal.TryParse(credit_limit, out credit_limit_decimal))
            {
                cred.CREDIT_LIMIT = decimal.Round(credit_limit_decimal, 2);
            }
            CUST_CRED.Add(cred);
            #endregion

            #region comp
            CUST_COMP comp = new CUST_COMP();
            //BUKRS AKONT
            var bukrs = string.Concat(resultData["companycode"]);//公司代码 
            comp.BUKRS = bukrs;//默认9000
            var akont = string.Concat(resultData["accountantsub"]);//总帐中的统驭科目 
            comp.AKONT = akont.StringMax(0, 10);
            CUST_COMP.Add(comp);
            #endregion


            var logTime = DateTime.Now;
            var postData = new Dictionary<string, object>();
            var headData = new Dictionary<string, string>();
            headData.Add("Transaction_ID", "CUSTOMER_CREATE");

            postData.Add("CUST_MAIN", cust);
            postData.Add("CUST_TAXN", CUST_TAXN);
            postData.Add("CUST_BANK", CUST_BANK);
            postData.Add("CUST_COMP", CUST_COMP);
            postData.Add("CUST_SALE", CUST_SALE);
            postData.Add("CUST_LOAD", CUST_LOAD);
            postData.Add("CUST_CRED", CUST_CRED);

            logger.Info(string.Concat("SAP客户创建接口请求参数：", JsonHelper.ToJson(postData)));
            var postResult = CallAPIHelper.ApiPostData(postData, headData);
            SapCustCreateModelResult sapRequest = JsonConvert.DeserializeObject<SapCustCreateModelResult>(postResult);

            if (sapRequest.TYPE == "S")
            {
                var sapCode = sapRequest.PARTNER;
                sapResult = sapRequest.MESSAGE;
                result.Result = true;
                _baseDataRepository.UpdateSynStatus(entityId, recId, (int)SynchrosapStatus.Yes, tran);
                _customerRepository.UpdateCustomerSapCode(recId, sapCode, tran);
                if (!string.IsNullOrEmpty(sapResult))
                    sapResult = string.Format(@"同步创建SAP客户成功，返回SAP客户号：{0}，SAP提示返回：{1}", sapCode, sapResult);
                else
                    sapResult = string.Format(@"同步创建SAP客户成功，返回SAP客户号：{0}", sapCode);
                result.Message = sapResult;
                _baseDataRepository.UpdateSynTipMsg(entityId, recId, sapResult, tran);
            }
            else
            {
                logger.Log(NLog.LogLevel.Error, $"创建SAP客户接口异常报错：{sapRequest.MESSAGE}");
                sapResult = sapRequest.MESSAGE;
                if (!string.IsNullOrEmpty(sapResult))
                {
                    sapResult = string.Concat("同步创建SAP客户失败，SAP错误返回：", sapResult);
                }
                else
                {
                    sapResult = "同步创建SAP客户失败，SAP返回无客户号";
                }
                result.Message = sapResult;
                _baseDataRepository.UpdateSynTipMsg2(entityId, recId, sapResult, tran);
            }

            return result;
        }

        public dynamic SynSapModifyCustData(IDictionary<string, object> resultData, Guid entityId, Guid recId, DbTransaction tran = null)
        {
            var result = new SynResultModel();
            var sapResult = string.Empty;

            CUST_MAIN_MODIFY cust = new CUST_MAIN_MODIFY();
            List<CUST_TAXN_MODIFY> CUST_TAXN = new List<CUST_TAXN_MODIFY>();
            List<CUST_BANK_MODIFY> CUST_BANK = new List<CUST_BANK_MODIFY>();
            List<CUST_COMP_MODIFY> CUST_COMP = new List<CUST_COMP_MODIFY>();
            List<CUST_SALE_MODIFY> CUST_SALE = new List<CUST_SALE_MODIFY>();
            List<CUST_LOAD_MODIFY> CUST_LOAD = new List<CUST_LOAD_MODIFY>();
            List<CUST_CRED_MODIFY> CUST_CRED = new List<CUST_CRED_MODIFY>();

            //CUST_LOAD_MODIFY load = new CUST_LOAD_MODIFY();
            //CUST_LOAD.Add(load);

            #region main
            //CRMCUST, KTOKD,ANRED, TITLE, NAME1, SORTL
            cust.CRMCUST = "CRM" + string.Concat(resultData["reccode"]).StringMax(0, 20);
            cust.PARTNER = string.Concat(resultData["erpcode"]).StringMax(0, 10);
            string erpcode = string.Concat(resultData["erpcode"]).StringMax(0, 10);

            var customertype = string.Concat(resultData["customertype"]);
            cust.KTOKD = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeEnum.客户账户组, customertype).StringMax(0, 4);//客户账户组

            //字典值，必须改
            cust.ANRED = "0003";

            //cust.TITLE = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeIdEnum.称谓, title).StringMax(0, 4);//地址关键字的表格
            cust.NAME1 = string.Concat(resultData["recname"]).StringMax(0, 35);//组织名称 1
            cust.SORTL = string.Concat(resultData["customername"]).StringMax(0, 10);//简称
            cust.ZTEXT1 = string.Concat(resultData["contacts"]).StringMax(0, 30);//联系人 必填
            cust.TELF1 = string.Concat(resultData["contactnumber"]).StringMax(0, 16);//联系电话 必填
            #endregion

            #region
            // STREET, ORT01, COUNTRY, REGIO
            var address = string.Concat(resultData["address"]);//街道
            if (!string.IsNullOrEmpty(address))
            {
                var ds = JsonHelper.ToObject<Address>(address);
                if (ds != null)
                    cust.STREET = ds.address.StringMax(0, 60);
                else
                    cust.STREET = address;
            }
            var city = string.Concat(resultData["region"]);
            cust.ORT01 = _baseDataRepository.GetRegionFullNameById(city).StringMax(0, 35);//城市
            var country = string.Concat(resultData["country"]);
            cust.LAND1 = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeEnum.国家, country).StringMax(0, 3);//国家/地区代码
            var region = string.Concat(resultData["saparea"]);
            cust.REGIO = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeEnum.地区, region).StringMax(0, 3);//地区（省/自治区/直辖市、市、县）
            #endregion

            #region
            //PSTLZ, LANGU,TAXKD
            cust.PSTLZ = string.Concat(resultData["postcode"]).StringMax(0, 10);//城市邮政编码
            cust.SPRAS = string.Concat(resultData["language"]).StringMax(0, 2);//语言
            var taxkd = string.Concat(resultData["taxgp"]);
            cust.TAXKD = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeEnum.税分类, taxkd).StringMax(0, 1);//客户税分类 
            #endregion

            #region bank
            CUST_BANK_MODIFY bank = new CUST_BANK_MODIFY();

            //银行编号
            //var bankl = string.Concat(resultData["bank"]);
            //bank.BANKL = string.Concat(resultData["bank"]).StringMax(0, 15); 
            //cust.BANKL = _baseDataRepository.GetBankCodeByDataSource(bankl).StringMax(0, 15); 
            //bank.BANKN = string.Concat(resultData["accountcode"]).StringMax(0, 18);//银行账户
            //CUST_BANK.Add(bank);
            #endregion

            #region sale
            CUST_SALE_MODIFY sale = new CUST_SALE_MODIFY();
            //VKORG, VTWEG, SPART, KVGR1, KVGR2 KALKS KTGRD
            var vkorg = string.Concat(resultData["salesorganization"]);
            sale.VKORG = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeEnum.销售组织, vkorg).StringMax(0, 4);//销售组织
            var vtweg = string.Concat(resultData["saledistribution"]);
            sale.VTWEG = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeEnum.分销渠道, vtweg).StringMax(0, 2);//分销渠道
            var spart = string.Concat(resultData["productgroup"]);
            sale.SPART = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeEnum.产品组, spart).StringMax(0, 2);//产品组
            var kvgr1 = string.Concat(resultData["custgpone"]);
            sale.KALKS = "1";//默认1
            sale.KTGRD = "03";//客户科目分配组
            /*sale.KVGR1 = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeEnum.客户组1, kvgr1).StringMax(0, 3);//客户组1
            var kvgr2 = string.Concat(resultData["custgptwo"]);
            sale.KVGR2 = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeEnum.客户组2, kvgr2).StringMax(0, 3);//客户组2*/

            //STCD5,BZIRK, VKBUR, KALKS, VWERK, VSBED,ZTERM
            cust.STCD5 = string.Concat(resultData["taxno"]).StringMax(0, 60);//增值税登记号
            var bzirk = string.Concat(resultData["area"]);
            sale.BZIRK = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeEnum.销售地区, bzirk).StringMax(0, 6);//销售地区 
            var vkbur = string.Concat(resultData["salesoffice"]);
            sale.VKBUR = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeEnum.销售办事处, vkbur).StringMax(0, 4);//销售办事处
            //var kalks = string.Concat(resultData["pricingpro"]);
            //sale.KALKS = kalks.StringMax(0, 2);//用于定价过程确定的客户分类
            //var vwerk = string.Concat(resultData["delivery"]);
            //sale.VWERK = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeEnum.交货工厂, vwerk).StringMax(0, 4);//交货工厂 (自有或外部)
            var vsbed = string.Concat(resultData["shipment"]);
            sale.VSBED = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeEnum.装运条件, vsbed).StringMax(0, 2);//装运条件
            var zterm = string.Concat(resultData["payrequirement"]);
            sale.ZTERM = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeEnum.付款条件, zterm).StringMax(0, 4);//付款条件代码 
            var waers = string.Concat(resultData["currency"]);
            sale.WAERS = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeEnum.币种, waers).StringMax(0, 5);//货币
            var ktgrd = string.Concat(resultData["accountgp"]);
            sale.KTGRD = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeEnum.客户科目分配组, ktgrd).StringMax(0, 2);//账户分配组
            CUST_SALE.Add(sale);
            #endregion

            #region
            // KTGRD, SEGMENT
            //var ktgrd = string.Concat(resultData["accountgp"]);
            //cust.KTGRD = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeIdEnum.账户分配组, ktgrd).StringMax(0, 2);//此客户的账户分配组
            /*var segment_str = string.Concat(resultData["creditperiod"]).StringMax(0, 10);//信用段 
            decimal segment_decimal = 0;
            if (decimal.TryParse(segment_str, out segment_decimal))
            {
                cust.SEGMENT = decimal.Round(segment_decimal, 0).ToString();
            }*/
            #endregion

            #region
            CUST_CRED_MODIFY cred = new CUST_CRED_MODIFY();
            //CREDIT_LIMIT, BUKRS
            var credit_limit = string.Concat(resultData["credit"]).StringMax(0, 15);
            decimal credit_limit_decimal = 0;
            if (decimal.TryParse(credit_limit, out credit_limit_decimal))
            {
                cred.CREDIT_LIMIT = decimal.Round(credit_limit_decimal, 2);
            }
            CUST_CRED.Add(cred);
            #endregion

            #region comp
            CUST_COMP_MODIFY comp = new CUST_COMP_MODIFY();
            //BUKRS AKONT
            var bukrs = string.Concat(resultData["companycode"]);//公司代码 
            comp.BUKRS = bukrs;//默认9000
            var akont = string.Concat(resultData["accountantsub"]);//总帐中的统驭科目 
            comp.AKONT = akont.StringMax(0, 10);
            //comp.PARTNER = erpcode;
            CUST_COMP.Add(comp);
            #endregion


            var logTime = DateTime.Now;
            var postData = new Dictionary<string, object>();
            var headData = new Dictionary<string, string>();
            headData.Add("Transaction_ID", "CUSTOMER_CHANGE");

            postData.Add("CUST_MAIN", cust);
            postData.Add("CUST_TAXN", CUST_TAXN);
            postData.Add("CUST_BANK", CUST_BANK);
            postData.Add("CUST_COMP", CUST_COMP);
            postData.Add("CUST_SALE", CUST_SALE);
            postData.Add("CUST_LOAD", CUST_LOAD);
            postData.Add("CUST_CRED", CUST_CRED);

            logger.Info(string.Concat("SAP客户修改接口请求参数：", JsonHelper.ToJson(postData)));
            var postResult = CallAPIHelper.ApiPostData(postData, headData);
            SapCustCreateModelResult sapRequest = JsonConvert.DeserializeObject<SapCustCreateModelResult>(postResult);

            if (sapRequest.TYPE == "S")
            {
                var sapCode = sapRequest.PARTNER;
                sapResult = sapRequest.MESSAGE;
                result.Result = true;
                _baseDataRepository.UpdateSynStatus(entityId, recId, (int)SynchrosapStatus.Yes, tran);
                _customerRepository.UpdateCustomerSapCode(recId, sapCode, tran);
                if (!string.IsNullOrEmpty(sapResult))
                    sapResult = string.Format(@"同步修改SAP客户成功，返回SAP客户号：{0}，SAP提示返回：{1}", sapCode, sapResult);
                else
                    sapResult = string.Format(@"同步修改SAP客户成功，返回SAP客户号：{0}", sapCode);
                result.Message = sapResult;
                _baseDataRepository.UpdateSynTipMsg(entityId, recId, sapResult, tran);
            }
            else
            {
                logger.Log(NLog.LogLevel.Error, $"修改SAP客户接口异常报错：{sapRequest.MESSAGE}");
                sapResult = sapRequest.MESSAGE;
                if (!string.IsNullOrEmpty(sapResult))
                {
                    sapResult = string.Concat("同步修改SAP客户失败，SAP错误返回：", sapResult);
                }
                else
                {
                    sapResult = "同步创建SAP客户失败，SAP返回无客户号";
                }
                result.Message = sapResult;
                _baseDataRepository.UpdateSynTipMsg2(entityId, recId, sapResult, tran);
            }

            return result;
        }

        public void AutoSubmitCustomerToSapNew(Guid caseId, int userId, DbTransaction tran)
        {
            /*var result = new SynResultModel();
            logger.Info(string.Concat("ModifyCustomerServices.AutoSubmitCustomerToSap-Begin"));
            var data = _baseDataRepository.GetEntityIdAndRecIdByCaseId(tran, caseId, userId);
            if (data != null)
            {
                //客户审批通过自动提交客户同步到SAP
                var custEntityId = EntityReg.CustomerEntityId();
                var detailData = _baseDataServices.GetEntityDetailData(tran, data.EntityId, data.RecId, userId);
                if (detailData != null)
                {
                    SynchrosapStatus isSyn = SynchrosapStatus.Yes;
                    var sapno = string.Concat(detailData["companyone"]);
                    var issynchrosap = string.Concat(detailData["issynchrosap"]);
                    if (!string.IsNullOrEmpty(sapno) && (issynchrosap == "1" || issynchrosap == "4"))
                        isSyn = SynchrosapStatus.No;
                    try
                    {
                        if (isSyn == SynchrosapStatus.Yes)
                        {
                            result = SynSapAddCustData(detailData, custEntityId, data.RecId);
                        }
                        else
                        {
                            result = SynSapModifyCustData(detailData, custEntityId, data.RecId);
                        }
                    }
                    catch (Exception ex)
                    {
                        var str = "同步客户失败，请联系管理员";
                        logger.Info(string.Format(@"{0},{1}", str, ex.Message));
                        result.Message = str;
                    }
                }
                else
                {
                    result.Message = "同步失败，不存在客户记录";
                }
            }*/
        }

        #region auto submit
        public void AutoSubmitCustomerToSap(Guid caseId, int userId, DbTransaction tran)
        {
            logger.Info(string.Concat("ModifyCustomerServices.AutoSubmitCustomerToSap-Begin"));
            var data = _baseDataRepository.GetEntityIdAndRecIdByCaseId(tran, caseId, userId);
            if (data != null)
            {
                //合同审批通过自动提交客户同步到SAP
                var custEntityId = EntityReg.CustomerEntityId();
                var detailData = _baseDataServices.GetEntityDetailData(tran, data.EntityId, data.RecId, userId);
                logger.Info(string.Format("GetEntityDetailData:{0}", JsonHelper.ToJson(detailData)));
                if (detailData != null)
                {
                    var customerJson = string.Concat(detailData["customer"]);
                    if (!string.IsNullOrEmpty(customerJson))
                    {
                        var ds = JsonHelper.ToObject<DataSourceInfo>(customerJson);
                        if (ds != null)
                        {
                            detailData = _baseDataServices.GetEntityDetailData(tran, custEntityId, ds.id, userId);
                            if (detailData != null)
                            {
                                if (detailData.ContainsKey("issynchrosap") == false
                                    || detailData["issynchrosap"] == null
                                    || detailData["issynchrosap"].ToString() != "1")
                                {
                                    //只有成功同步SAP的数据才调用同步SAP接口（issynchrosap字段）
                                    var result = SynSapAddCustData(detailData, custEntityId, ds.id, tran);
                                    if (result.Result == false)
                                    {
                                        logger.Error(string.Format("自动提交客户SAP数据失败:{0}", result.Message));
                                    }
                                }

                            }
                        }
                    }
                }
            }
            logger.Info(string.Concat("ModifyCustomerServices.AutoSubmitCustomerToSap-End"));
        }
        #endregion


        #region edit auto submit
        public dynamic EditCustAutoCommit(DbTransaction transaction, object basicParamData, object preActionResult, object actionResult, object userId)
        {
            var result = new SynResultModel();
            var funRet = _baseDataRepository.ExcuteActionExt(transaction, "crm_fhsj_customer_edit", basicParamData, preActionResult, actionResult, (int)userId);
            var paramData = basicParamData as DynamicEntityEditModel;
            Dictionary<string, object> fieldData = paramData.FieldData;
            logger.Info("修改客户开始：" + paramData.RecId.ToString());
            //判断是否需要提交
            var detailData = _baseDataServices.GetEntityDetailData(transaction, Guid.Parse("f9db9d79-e94b-4678-a5cc-aa6e281c1246"), paramData.RecId, (int)userId);
            if (detailData != null)
            {
                logger.Info("修改客户调用sap开始：" + paramData.RecId.ToString());
                SynchrosapStatus isSyn = SynchrosapStatus.Yes;
                var sapno = string.Concat(detailData["companyone"]);
                var flowstatus = string.Concat(detailData["flowstatus"]);
                var issynchrosap = string.Concat(detailData["issynchrosap"]);
                if (!string.IsNullOrEmpty(sapno) && (issynchrosap == "1" || issynchrosap == "4") && flowstatus == "1")
                    isSyn = SynchrosapStatus.No;
                try
                {
                    if (isSyn != SynchrosapStatus.Yes)
                    {
                        logger.Info("修改客户调用sap中：" + paramData.RecId.ToString());
                        result = SynSapModifyCustData(detailData, Guid.Parse("f9db9d79-e94b-4678-a5cc-aa6e281c1246"), paramData.RecId, transaction);
                        if (!result.Result)
                        {
                            throw (new Exception(result.Message));
                        }
                    }
                    else
                    {
                        logger.Info("修改客户调用直接调用后置没调用sap接口：" + paramData.RecId.ToString());
                        return funRet;
                    }
                }
                catch (Exception ex)
                {
                    var str = "同步客户失败，请联系管理员";
                    logger.Info(string.Format(@"{0},{1}", str, ex.Message));
                    throw (new Exception(result.Message));
                }
            }

            return result;
        }
        #endregion

        public void AutoSubmitCustomerToSapNewJX(Dictionary<string, object> param, int userId, DbTransaction tran)
        {
            if (param.Count == 0 || !param.ContainsKey("entityid") || !param.ContainsKey("recid")
            || string.IsNullOrWhiteSpace(param["entityid"]?.ToString())
            || string.IsNullOrWhiteSpace(param["recid"]?.ToString()))
            {
            }
            else
            {
                //经销审批单实体recId
                Guid recId = Guid.Parse(param["recid"]?.ToString());
                //经销审批单entityId
                Guid debugEntityId = Guid.Parse("6684796f-3811-4965-8eca-874cd5239250");
                //客户entityId
                Guid projectEntityId = Guid.Parse("f9db9d79-e94b-4678-a5cc-aa6e281c1246");
                try
                {
                    //获取当前经销审批单明细
                    var deBugDetailData = _baseDataServices.GetEntityDetailData(tran, debugEntityId, recId, userId);
                    if (deBugDetailData != null)
                    {
                        //客户名称
                        var company = string.Concat(deBugDetailData["companyname"]);
                        if (company != null)
                        {
                            //获取客户实例
                            Guid companyId = Guid.Parse(JsonHelper.ToJsonDictionary(company.ToString())["id"].ToString());
                            if (companyId != null)
                            {
                                this.SynSapCustDataJx(projectEntityId, companyId, userId);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error(string.Format("自动提交客户SAP数据失败:{0}", e.Message));
                }
            }
        }
        public void AutoSubmitCustomerToSapNewJxT(Dictionary<string, object> param, int userId, DbTransaction tran)
        {
            if (param.Count == 0 || !param.ContainsKey("recid")
            || string.IsNullOrWhiteSpace(param["recid"]?.ToString()))
            {
            }
            else
            {
                //经销审批单实体recId
                Guid recId = Guid.Parse(param["recid"]?.ToString());
                //经销审批单entityId
                Guid debugEntityId = Guid.Parse("6684796f-3811-4965-8eca-874cd5239250");
                //客户entityId
                Guid projectEntityId = Guid.Parse("f9db9d79-e94b-4678-a5cc-aa6e281c1246");
                try
                {
                    //获取当前经销审批单明细
                    this.SynSapCustDataJx(projectEntityId, recId, userId);
                }
                catch (Exception e)
                {
                    logger.Error(string.Format("自动提交客户SAP数据失败:{0}", e.Message));
                }
            }
        }
        public SynResultModel SynSapCustDataJx(Guid entityId, Guid recId, int UserId)
        {
            var result = new SynResultModel();
            var detailData = _baseDataServices.GetEntityDetailData(null, entityId, recId, UserId);
            if (detailData != null)
            {
                SynchrosapStatus isSyn = SynchrosapStatus.Yes;
                var sapno = string.Concat(detailData["companyone"]);
                detailData["distribution"] = "3";//经销
                if (!string.IsNullOrEmpty(sapno))
                    isSyn = SynchrosapStatus.No;
                try
                {
                    if (isSyn == SynchrosapStatus.Yes)
                    {
                        result = SynSapAddCustData(detailData, entityId, recId);
                    }
                    else
                    {
                        result = SynSapModifyCustData(detailData, entityId, recId);
                    }
                }
                catch (Exception ex)
                {
                    var str = "同步客户失败，请联系管理员";
                    logger.Info(string.Format(@"{0},{1}", str, ex.Message));
                    result.Message = str;
                }
            }
            else
            {
                result.Message = "同步失败，不存在客户记录";
            }

            return result;
        }


        #region 同步银行信息
        public SynResultModel SyncBankInfo2CRM()
        {
            var result = new SynResultModel();
            var sapResult = string.Empty;

            var postData = new Dictionary<string, object>();
            var headData = new Dictionary<string, string>();
            headData.Add("Transaction_ID", "BANK_DATA");
            var postResult = CallAPIHelper.ApiPostData(postData, headData);
            SapBankModelResult sapRequest = JsonConvert.DeserializeObject<SapBankModelResult>(postResult);
            if (sapRequest.TYPE == "S")
            {
                sapResult = sapRequest.MESSAGE;
                result.Result = true;
                result.Message = "同步SAP银行信息成功";
                //  SubmitInterfaceData("同步SAP银行信息", postResult);
                InitBankInfoData(sapRequest.DATA);
            }
            else
            {
                logger.Log(NLog.LogLevel.Error, $"同步SAP银行信息失败，提示消息：{sapRequest.MESSAGE}");
                sapResult = sapRequest.MESSAGE;
                if (!string.IsNullOrEmpty(sapResult))
                {
                    sapResult = string.Concat("同步SAP银行信息失败，提示消息：", sapResult);
                }
                else
                {
                    sapResult = "同步SAP银行信息失败，提示消息：无";
                }
                result.Message = sapResult;
            }

            return result;
        }

        private void InitBankInfoData(List<Dictionary<string, object>> dataList)
        {
            var entityId = new Guid("8c930228-074a-48de-ac14-3f6949392918");
            var crmInfoList = _customerRepository.GetCRMBankInfoList();

            foreach (var item in dataList)
            {
                var recid = crmInfoList.Where(r => r["bankl"].ToString() == item["BANKL"].ToString()).FirstOrDefault()?["recid"].ToString();
                if (!string.IsNullOrEmpty(recid))
                {
                    var isExists = crmInfoList.Exists(r =>
                    r["bankl"].ToString() == item["BANKL"].ToString() &&
                    r["banks"].ToString() == item["BANKS"].ToString() &&
                    r["ernam"].ToString() == item["ERNAM"].ToString() &&
                    r["banka"].ToString() == item["BANKA"].ToString() &&
                    r["ort01"].ToString() == item["ORT01"].ToString() &&
                    r["bnklz"].ToString() == item["BNKLZ"].ToString());
                    if (isExists)
                    {
                        continue;
                    }
                    _dynamicEntityRepository.DynamicEdit(null, entityId, new Guid(recid), item, 1);
                }
                else
                {
                    _dynamicEntityRepository.DynamicAdd(null, entityId, item, null, 1);
                }
            }

        }
        #endregion

        #region 推送交货单至SAP
        //供审批通过时调用
        public SynResultModel SynSapDelivNoteData(Guid caseId, int userId, string userName, DbTransaction tran)
        {
            var result = new SynResultModel();
            var recId = _customerRepository.GetRecIdByCaseId(caseId, tran);
            var entityId = new Guid("03b007dd-4600-4f4e-9b5c-23e8631d2f34");
            var detailData = _baseDataServices.GetEntityDetailData(null, entityId, recId, userId);
            if (detailData != null)
            {
                try
                {
                    result = SynSapAddDelivNote(detailData, entityId, recId);
                }
                catch (Exception ex)
                {
                    var str = "同步交货单失败，请联系管理员";
                    logger.Info(string.Format(@"{0},{1}", str, ex.Message));
                    result.Message = str;
                }
            }
            else
            {
                result.Message = "同步失败，不存在交货单记录";
            }
            return result;
        }

        public SynResultModel SynSapDelivNoteData(Guid entityId, Guid recId, int UserId)
        {
            var result = new SynResultModel();
            var detailData = _baseDataServices.GetEntityDetailData(null, entityId, recId, UserId);
            if (detailData != null)
            {
                SynchrosapStatus isSyn = SynchrosapStatus.Yes;
                var sapno = string.Concat(detailData["code"]);
                var issynchrosap = string.Concat(detailData["issynchrosap"]);
                if (!string.IsNullOrEmpty(sapno) && (issynchrosap == "1" || issynchrosap == "4"))
                    isSyn = SynchrosapStatus.No;
                try
                {
                    if (isSyn == SynchrosapStatus.Yes)
                    {
                        result = SynSapAddDelivNote(detailData, entityId, recId);
                    }
                    else
                    {
                        result = SynSapModifyDelivNote(detailData, entityId, recId);
                    }
                }
                catch (Exception ex)
                {
                    var str = "同步交货单失败，请联系管理员";
                    logger.Info(string.Format(@"{0},{1}", str, ex.Message));
                    result.Message = str;
                }
            }
            else
            {
                result.Message = "同步失败，不存在交货单记录";
            }
            return result;
        }

        public SynResultModel SynSapAddDelivNote(IDictionary<string, object> resultData, Guid entityId, Guid recId, DbTransaction tran = null)
        {
            var result = new SynResultModel();
            var sapResult = string.Empty;

            var postData = new Dictionary<string, object>();
            var headData = new Dictionary<string, string>();
            var mainData = new Dictionary<string, object>();
            var entryData = new List<Dictionary<string, object>>();
            headData.Add("Transaction_ID", "ODN_CREATE");
            mainData.Add("WADAT", resultData["plandate"]);
            #region entry
            var entryStr =JsonConvert.SerializeObject(resultData["deliverydetail"]);
            var entryList = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(entryStr);
            foreach (var item in entryList)
            {
                var dic = new Dictionary<string, object>();
                dic.Add("VBELN", _customerRepository.GetOrderNoByRecId(resultData["sourceorder"]?.ToString().Substring(8, 36)));
                dic.Add("POSNR",item["orderlineno"]);
                dic.Add("JHQTY",item["deliveryqty"]);
                dic.Add("JHDW", item["productunit_name"]);
                dic.Add("BATCH", item["charg"]);
                entryData.Add(dic);
            }  
            #endregion
            postData.Add("HEADER", mainData);
            postData.Add("ITEM", entryData);

            logger.Info(string.Concat("SAP交货单创建接口请求参数：", JsonHelper.ToJson(postData)));
            var postResult = CallAPIHelper.ApiPostData(postData, headData);
            SapDeliveryCreateModelResult sapRequest = JsonConvert.DeserializeObject<SapDeliveryCreateModelResult>(postResult);

            if (sapRequest.TYPE == "S")
            {
                var sapCode = sapRequest.JHDH;
                sapResult = sapRequest.MESSAGE;
                result.Result = true;
                _baseDataRepository.UpdateSynStatus(entityId, recId, (int)SynchrosapStatus.Yes, tran);
                _customerRepository.UpdateDeliverySapCode(recId, sapCode, tran);
                if (!string.IsNullOrEmpty(sapResult))
                    sapResult = string.Format(@"同步创建SAP交货单成功，返回SAP交货单号：{0}，SAP提示返回：{1}", sapCode, sapResult);
                else
                    sapResult = string.Format(@"同步创建SAP交货单成功，返回SAP交货单号：{0}", sapCode);
                result.Message = sapResult;
                _baseDataRepository.UpdateSynTipMsg(entityId, recId, sapResult, tran);
            }
            else
            {
                logger.Log(NLog.LogLevel.Error, $"创建SAP交货单接口异常报错：{sapRequest.MESSAGE}");
                sapResult = sapRequest.MESSAGE;
                if (!string.IsNullOrEmpty(sapResult))
                {
                    sapResult = string.Concat("同步创建SAP交货单失败，SAP错误返回：", sapResult);
                }
                else
                {
                    sapResult = "同步创建SAP交货单失败，SAP返回无交货单号";
                }
                result.Message = sapResult;
                _baseDataRepository.UpdateSynTipMsg2(entityId, recId, sapResult, tran);
            }

            return result;
        }

        public SynResultModel SynSapModifyDelivNote(IDictionary<string, object> resultData, Guid entityId, Guid recId, DbTransaction tran = null)
        {
            var result = new SynResultModel();
            var sapResult = string.Empty;

            var postData = new Dictionary<string, object>();
            var headData = new Dictionary<string, string>();
            var entryData = new List<Dictionary<string, object>>();
            headData.Add("Transaction_ID", "ODN_CHANGE");
            postData.Add("VBELN_JHDH",resultData["code"]);
            postData.Add("DELETE", "");
            postData.Add("BLDAT", resultData["docdate"]);
            postData.Add("WADAT", resultData["plandate"]);
            postData.Add("WADAT_IST", resultData["actualdate"]);
            postData.Add("KODAT", resultData["pickingdate"]);
           // postData.Add("ROUTE", "");
            #region entry
            var entryStr = JsonConvert.SerializeObject(resultData["deliverydetail"]);
            var entryList = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(entryStr);
            foreach (var item in entryList)
            {
                var dic = new Dictionary<string, object>();
                dic.Add("VBELN_JHDH", resultData["code"]);
                dic.Add("POSNR", item["lineno"]);
               // dic.Add("LGORT", "");  //库存地点
                dic.Add("LFIMG", item["deliveryqty"]);
                dic.Add("VRKME", "KG"); 
                dic.Add("JPQTY", item["jpqty"]);
                dic.Add("JPDNW", "KG");
                dic.Add("CHARG", item["charg"]);
                entryData.Add(dic);
            }
            #endregion
            postData.Add("LIPS", entryData);
            logger.Info(string.Concat("SAP交货单创建接口请求参数：", JsonHelper.ToJson(postData)));
            var postResult = CallAPIHelper.ApiPostData(postData, headData);
            return result;
        }
        #endregion

        #region 同步交货单
        public SynResultModel SyncDelivnote2CRM(Sync2CRMInfo info)
        {
            var result = new SynResultModel();
            var sapResult = string.Empty;

            var postData = new Dictionary<string, object>();
            var headData = new Dictionary<string, string>();
            headData.Add("Transaction_ID", "DELIVNOTE_GET");
            //if (string.IsNullOrEmpty(info.REQDate))
            //{
            //    postData.Add("REQDATE", DateTime.Now.ToString("yyyyMMdd"));
            //}
            //else
            //{
            //    postData.Add("REQDATE", info.REQDate);
            //}
            postData.Add("REQDATE", info.REQDate);
            postData.Add("ERDAT_FR", info.ERDAT_FR);
            postData.Add("ERDAT_TO", info.ERDAT_TO);
            postData.Add("VBELN_JHDH", info.VBELN_JHDH);
           // logger.Info(string.Concat("SAP同步交货单接口请求参数：", JsonHelper.ToJson(postData)));
            var postResult = CallAPIHelper.ApiPostData(postData, headData);
            SapDelivnoteResult sapRequest = JsonConvert.DeserializeObject<SapDelivnoteResult>(postResult);
            if (sapRequest.TYPE == "S")
            {
                sapResult = sapRequest.MESSAGE;
                result.Result = true;
                result.Message = "同步SAP交货单信息成功";
                InitDelivnoteInfoData(sapRequest.DATA);
            }
            else
            {
                logger.Log(NLog.LogLevel.Error, $"同步SAP交货单信息失败，提示消息：{sapRequest.MESSAGE}");
                sapResult = sapRequest.MESSAGE;
                if (!string.IsNullOrEmpty(sapResult))
                {
                    sapResult = string.Concat("同步SAP交货单信息失败，提示消息：", sapResult);
                }
                else
                {
                    sapResult = "同步SAP交货单信息失败，提示消息：无";
                }
                result.Message = sapResult;
            }

            return result;
        }

        public void InitDelivnoteInfoData(SapDelivnoteDATA dataList)
        {
            var mainEntity = new Guid("03b007dd-4600-4f4e-9b5c-23e8631d2f34");
            var detailEntity = new Guid("59a5ebfb-3f1c-4a9a-a8cd-e451e6d0c806");

            foreach (var item in dataList.LIKP)
            {
                var detailList = new List<Dictionary<string, object>>();
                var entryList = dataList.LIPS.Where(r => r["VBELN_JHDH"].ToString() == item["VBELN_JHDH"].ToString() && r["CHARG"] == null).ToList();
                int index = 1;
                Dictionary<string, object> orderInfo = null;    
                foreach (var entry in entryList)
                {
                    Dictionary<string, object> dic = new Dictionary<string, object>();
                    Dictionary<string, object> parentDic = new Dictionary<string, object>();
                    if (orderInfo == null)
                    {
                        //找对应订单及客户、销售部门、销售区域
                        orderInfo = _customerRepository.GetOrderInfo(entry["VBELN_SO"].ToString());
                        if (orderInfo == null)
                            continue;
                    }
                    #region MyRegion
                    var material = entry["MATNR"].ToString().TrimStart('0');
                    var product = _customerRepository.GetCrmProduct(material);
                    dic.Add("code", entry["VBELN_JHDH"]);
                    dic.Add("lineno", entry["POSNR"]);
                    dic.Add("deliverydate", entry["VDATU"]?.ToString() == "0000-00-00" ? "" : entry["VDATU"]);
                    dic.Add("parentno", entry["UECHA"]);
                    dic.Add("orderlineno", entry["POSNR_SO"]);
                    dic.Add("materialcode", material);
                    dic.Add("productname", product);
                    dic.Add("describe", entry["ARKTX"]);
                    dic.Add("deliveryqty", entry["LFIMG"]);
                    dic.Add("qty", entry["LGMNG"]);
                    dic.Add("jpqty", entry["JPQTY"]);
                    dic.Add("quantity", 0);
                    dic.Add("charg", entry["CHARG"]);
                    #endregion
                    parentDic.Add("TypeId", detailEntity);
                    parentDic.Add("FieldData", dic);
                    detailList.Add(parentDic);
                    if (index == entryList.Count)
                    {
                        // 找相关人会员
                        //var userInfo = _customerRepository.getUserInfo(item["ERNAM"]?.ToString());
                        //int userId = userInfo == null ? 1 : Convert.ToInt32(userInfo["userid"]);
                      
                        var res = new OperateResult();
                        dic = new Dictionary<string, object>();
                        dic.Add("code", item["VBELN_JHDH"].ToString());
                        dic.Add("docdate", item["BLDAT"]);
                        dic.Add("deliverydate", item["ZPOST_DATE"]?.ToString() == "0000-00-00" ? "" : item["ZPOST_DATE"]);
                        dic.Add("deliverytime", item["ZPOST_DATE"]?.ToString() == "0000-00-00" ? "" : item["ZPOST_DATE"] + " " + item["ZPOST_TIME"]);
                        dic.Add("reccreated", item["ERDAT"] + " " + item["ERZET"]);
                        dic.Add("recupdated", item["AEDAT"]);
                        dic.Add("plandate", item["WADAT"]);
                        dic.Add("actualdate", item["WADAT_IST"]);
                        dic.Add("pickingdate", item["KODAT"]);
                        dic.Add("recmanager", orderInfo["recmanager"]);
                        dic.Add("salesdept", orderInfo["salesdepartments"]);
                        dic.Add("salesarea", orderInfo["salesterritory"]);
                        dic.Add("sourceorder", orderInfo["orderjson"]);
                        dic.Add("customer", orderInfo["customer"]);
                        dic.Add("deliverydetail", JsonConvert.SerializeObject(detailList));

                        var mainId = _customerRepository.IsExistsDelivnote(item["VBELN_JHDH"].ToString());
                        if (mainId == Guid.Empty)
                        {
                            res = _dynamicEntityRepository.DynamicAdd(null, mainEntity, dic, null, 1);
                        }
                        else
                        {
                            res = _dynamicEntityRepository.DynamicEdit(null, mainEntity, mainId, dic, 1);
                        }
                    }
                    else
                        index++;
                }
            }
        }
        #endregion

        #region 写入日志表
        public void SubmitInterfaceData(string interfacename, string responseresult)
        {
            var interfaceEntityID = new Guid("195d52b7-4d98-4027-a9b2-627e6587fc44");
            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic.Add("interfacename", interfacename);
            dic.Add("responseresult", responseresult);
            _dynamicEntityRepository.DynamicAdd(null, interfaceEntityID, dic, null, 1);
        }
        #endregion
        public OutputResult<object> SyncSapCustCreditLimitData(Guid entityId, Guid recId, Int32 userId)
        {
            if (_baseDataServices == null)
                _baseDataServices = ServiceLocator.Current.GetInstance<BaseDataServices>();
            var creditLimitApplyData = _baseDataServices.GetEntityDetailData(null, entityId, recId, userId);
            StringBuilder sb = new StringBuilder();
            if (creditLimitApplyData["customerdetail"] != null)
            {
                var detailData = (List<IDictionary<String, object>>)creditLimitApplyData["customerdetail"];
                int index = 0;
                List<CustomerCreditLimitPush> customerCreditLimitPush = new List<CustomerCreditLimitPush>();
                detailData.ForEach(t =>
                {
                    var data = detailData[index];
                    var custJson = data["customer"];
                    JObject jObject = JObject.Parse(custJson.ToString());
                    var custId = jObject["id"];
                    CustomerCreditLimitPush push = new CustomerCreditLimitPush();
                    if (custId != null && !string.IsNullOrEmpty(custId.ToString()))
                    {
                        var custDetail = _baseDataServices.GetEntityDetailData(null, Guid.Parse("f9db9d79-e94b-4678-a5cc-aa6e281c1246"), Guid.Parse(custId.ToString()), userId);
                        if (custDetail["erpcode"] != null && custDetail["creditlimitsgmnt"] != null)
                        {
                            push.PARTNER = custDetail["erpcode"].ToString();
                            push.CREDIT_SGMNT = custDetail["creditlimitsgmnt"].ToString();
                            push.CREDIT_LIMIT = data["credit"].ToString();
                            push.LIMIT_VALID_DATE = data["creditlimitvaliddate"] == null ? "" : Convert.ToDateTime(data["creditlimitvaliddate"]).ToString("yyyyMMdd");
                            if (_fetchCustomerServices == null)
                                _fetchCustomerServices = ServiceLocator.Current.GetInstance<FetchCustomerServices>();
                            var actTimeSapCredit = _fetchCustomerServices.getCustomerCreditLimit(new CustomerCreditLimitParam
                            {
                                RecId = Guid.Parse(custId.ToString()),
                                EntityId = Guid.Parse("f9db9d79-e94b-4678-a5cc-aa6e281c1246")
                            }, userId);
                            if (actTimeSapCredit.Status == 0)
                            {
                                var actCreditData = (CustomerCreditLimitDataModel)actTimeSapCredit.DataBody;
                                if (actCreditData.AMOUNT_DYN == 0 && actCreditData.CREDIT_LIMIT == 0 && actCreditData.CREDIT_LIMIT_USEDW == 0)
                                    push.UPDATE = "I";
                                else
                                    push.UPDATE = "U";
                                var header = new Dictionary<String, string>();
                                header.Add("Transaction_ID", "CREDIT_CHANGE");
                                var postData = new Dictionary<String, string>();
                                List<Dictionary<String, string>> postDataList = new List<Dictionary<string, string>>();
                                postData.Add("PARTNER", push.PARTNER);
                                postData.Add("CREDIT_SGMNT", push.CREDIT_SGMNT);
                                postData.Add("CREDIT_LIMIT", push.CREDIT_LIMIT);
                                postData.Add("LIMIT_VALID_DATE", push.LIMIT_VALID_DATE);
                                postData.Add("UPDATE", push.UPDATE);
                                String result = CallAPIHelper.ApiPostData(postData, header);
                                if (!string.IsNullOrEmpty(result))
                                {
                                    var objResult = JsonConvert.DeserializeObject<SapCustCreateModelResult>(result);
                                    if (objResult.TYPE != "S")
                                        sb.Append("[" + jObject["name"].ToString() + "]" + objResult.MESSAGE);
                                }
                            }
                            else
                                sb.Append("[" + jObject["name"].ToString() + "]的客户号或信用段不能为空");
                        }
                        else
                            sb.Append("获取[" + jObject["name"].ToString() + "]实时信用额度失败");
                    }
                    else
                    {
                        sb.Append("获取客户信息异常");
                        return;
                    }
                    index++;
                });
            }
            if (string.IsNullOrEmpty(sb.ToString()))
                return new OutputResult<object>("同步成功");
            return new OutputResult<object>(null, sb.ToString(), 1);
        }
    }
}
