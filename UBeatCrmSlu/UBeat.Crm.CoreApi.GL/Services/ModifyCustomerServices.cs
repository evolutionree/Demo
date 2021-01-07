using System;
using System.Collections.Generic;
using System.Data.Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NLog;
using UBeat.Crm.CoreApi.DomainModel.Utility;
using UBeat.Crm.CoreApi.GL.Model;
using UBeat.Crm.CoreApi.GL.Repository;
using UBeat.Crm.CoreApi.GL.Utility;
using UBeat.Crm.CoreApi.Services.Models.DynamicEntity;
using UBeat.Crm.CoreApi.Services.Services;

namespace UBeat.Crm.CoreApi.GL.Services
{
    public class ModifyCustomerServices: BasicBaseServices
    {
        private static readonly Logger logger = LogManager.GetLogger("UBeat.Crm.GL.Services.ModifyCustomerServices");
        private readonly ICustomerRepository _customerRepository;
        private readonly IBaseDataRepository _baseDataRepository;
        private readonly CacheServices _cacheService;
        private readonly BaseDataServices _baseDataServices;
         
        public ModifyCustomerServices(ICustomerRepository customerRepository,
            IBaseDataRepository baseDataRepository,
            CacheServices cacheService,
            BaseDataServices baseDataServices)
        {
            _customerRepository = customerRepository;
            _baseDataRepository = baseDataRepository;
            _cacheService = cacheService;
            _baseDataServices = baseDataServices; 
        }

        public SynResultModel SynSapCustData(Guid entityId, Guid recId, int UserId)
        {
            var result = new SynResultModel();
            var detailData = _baseDataServices.GetEntityDetailData(null,entityId, recId, UserId);
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
                        //result = SynSapModifyCustData(detailData, entityId, recId);
                    }
                }
                catch(Exception ex)
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

            CUST_LOAD load =new CUST_LOAD();
            CUST_LOAD.Add(load);

            #region main
            //CRMCUST, KTOKD,ANRED, TITLE, NAME1, SORTL
            cust.CRMCUST = string.Concat(resultData["reccode"]).StringMax(0, 20);

            var customertype = string.Concat(resultData["customertype"]);
            cust.KTOKD = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeEnum.客户账户组, customertype).StringMax(0, 4);//客户账户组

            //字典值，必须改
            cust.ANRED = "0003";

            //cust.TITLE = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeIdEnum.称谓, title).StringMax(0, 4);//地址关键字的表格
            cust.NAME1 = string.Concat(resultData["recname"]).StringMax(0, 35);//组织名称 1
            cust.SORTL = string.Concat(resultData["customername"]).StringMax(0, 10);//简称
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
            else {
                logger.Log(NLog.LogLevel.Error, $"创建SAP客户接口异常报错：{sapRequest.MESSAGE}");
                sapResult = sapRequest.MESSAGE;
                if (!string.IsNullOrEmpty(sapResult))
                {
                    sapResult = string.Concat("同步创建SAP客户失败，SAP错误返回：", sapResult);
                }
                else {
                    sapResult = "同步创建SAP客户失败，SAP返回无客户号";
                }
                result.Message = sapResult;
                _baseDataRepository.UpdateSynTipMsg2(entityId, recId, sapResult, tran);
            }

            return result;
        }

        public dynamic SynSapModifyCustData(IDictionary<string, object> resultData, Guid entityId, Guid recId, DbTransaction tran=null)
        {
            var result = new SynResultModel();
            var sapResult = string.Empty;
            return result;

           /* var remoteAddress = new System.ServiceModel.EndpointAddress(string.Format("{0}/{1}", SapServer,
                string.Format("sap/bc/srt/rfc/sap/zsd_crm_004s/{0}/zsd_crm_004_s/zsd_crm_004_s", SapClientId)));
            var binding = InitBind(remoteAddress);
            ZSD_CRM_004SClient client = new ZSD_CRM_004SClient(binding, remoteAddress);
            AuthBasic(client.ClientCredentials.UserName, client.Endpoint);

            var cust = new ZSD_CRM041();

            #region
            //PARTNER, NAME_ORG, BU_SORT1_TXT, STREET, CITY1
            cust.PARTNER = string.Concat(resultData["companyone"]).StringMax(0, 10);//业务伙伴编号
            cust.NAME_ORG = string.Concat(resultData["recname"]).StringMax(0, 80);//名称
            cust.BU_SORT1_TXT = string.Concat(resultData["searchone"]).StringMax(0, 20);//业务伙伴的搜索词 1
            var address = string.Concat(resultData["address"]);//街道
            if (!string.IsNullOrEmpty(address))
            {
                var ds = JsonHelper.ToObject<Address>(address);
                if (ds != null)
                    cust.STREET = ds.address.StringMax(0, 60);
                else
                    cust.STREET = address;
            }
            var city = string.Concat(resultData["city"]);
            cust.CITY1 = _baseDataRepository.GetRegionFullNameById(city).StringMax(0, 40);//城市
            #endregion

            #region
            //COUNTRY, REGION, POST_CODE1, LANGU, TEL_NUMBER
            var country = string.Concat(resultData["country"]);
            cust.COUNTRY = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeIdEnum.国家, country).StringMax(0, 3);//国家/地区代码
            var region = string.Concat(resultData["region"]);
            cust.REGION = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeIdEnum.地区, region).StringMax(0, 3);//地区（省/自治区/直辖市、市、县）
            cust.POST_CODE1 = string.Concat(resultData["postcode"]).StringMax(0, 10);//城市邮政编码
            cust.LANGU = string.Concat(resultData["language"]).StringMax(0, 2);//语言
            cust.TEL_NUMBER = string.Concat(resultData["taxphone"]).StringMax(0, 30);//第一个电话号码：区号 + 号码
            #endregion

            #region
            //TEL_EXTENS, MOB_NUMBER, FAX_NUMBER, SMTP_ADDR, STCEG
            cust.TEL_EXTENS = string.Concat(resultData["extension"]).StringMax(0, 10);//第一个电话号码:分机号 
            cust.MOB_NUMBER = string.Concat(resultData["mobilephone"]).StringMax(0, 30);//第一个电话号码：区号 + 号码
            cust.FAX_NUMBER = string.Concat(resultData["fax"]).StringMax(0, 30);//第一个传真号: 拨号 + 编号
            cust.SMTP_ADDR = string.Concat(resultData["email"]).StringMax(0, 241);//电子邮件地址
            cust.STCEG = string.Concat(resultData["valueadd"]).StringMax(0, 20);//增值税登记号  
            #endregion
            var vkorg = string.Concat(resultData["salesorganization"]);
            cust.VKORG = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeIdEnum.销售组织, vkorg).StringMax(0, 4);//销售组织
            var vtweg = string.Concat(resultData["distribution"]);
            cust.VTWEG = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeIdEnum.分销渠道, vtweg).StringMax(0, 2);//分销渠道
            var spart = string.Concat(resultData["productgroup"]);
            cust.SPART = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeIdEnum.产品组, spart).StringMax(0, 2);//产品组

            #region
            //BANKL, BANKN, ZCRM_ID KVGR1 KVGR2
            //银行编号
            var bankl = string.Concat(resultData["opencode"]);
            cust.BANKL = _baseDataRepository.GetBankCodeByDataSource(bankl).StringMax(0, 15);
            cust.BANKN = string.Concat(resultData["accountcode"]).StringMax(0, 38);//银行账户
            cust.TITLE_LET = string.Concat(resultData["openname"]);
            cust.ZCRM_ID = string.Concat(resultData["reccode"]).StringMax(0, 20);//CRM主键
            var kvgr1 = string.Concat(resultData["custgpone"]);
            cust.KVGR1 = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeIdEnum.客户组1, kvgr1).StringMax(0, 3);//客户组1
            var kvgr2 = string.Concat(resultData["custgptwo"]);
            cust.KVGR2 = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeIdEnum.客户组2, kvgr2).StringMax(0, 3);//客户组2
            var bzirk = string.Concat(resultData["salesarea"]);
            cust.BZIRK = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeIdEnum.销售区域, bzirk).StringMax(0, 6);//销售地区 
            var vkbur = string.Concat(resultData["salesoffice"]);
            cust.VKBUR = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeIdEnum.销售办事处, vkbur).StringMax(0, 4);//销售办事处
            #endregion 

            ZSD_CRM_004 request = new ZSD_CRM_004();
            request.LV_FLAG = SapOptType.U.ToString();
            request.LV_CRM041 = cust;
            request.LV_CUSTOM = new ZSD_CRM040();
            request.LT_RET2 = new ZCRMRET2[] { };

            logger.Info(string.Concat("SAP客户修改接口请求参数：", JsonHelper.ToJson(request))); 
            var ret = client.ZSD_CRM_004Async(request).Result;
            logger.Info(string.Concat("SAP返回客户修改接口数据：", JsonHelper.ToJson(ret))); 
            if (ret != null && ret.ZSD_CRM_004Response != null)
            {
                var response = ret.ZSD_CRM_004Response;
                var sapCode = cust.PARTNER;
                if (!string.IsNullOrEmpty(sapCode))
                { 
                    if (response.LT_RET2 != null && response.LT_RET2.Length > 0)
                    {
                        for (int i = 0; i < response.LT_RET2.Length; i++)
                        {
                            if(response.LT_RET2[i].TYPE == "E")
                            {
                                if (string.IsNullOrEmpty(sapResult))
                                    sapResult = response.LT_RET2[i].MESSAGE;
                                else
                                    sapResult = string.Concat(sapResult, ",", response.LT_RET2[i].MESSAGE);
                            } 
                        }  
                    }
                    if (string.IsNullOrEmpty(sapResult))
                    {
                        result.Result = true;
                        sapResult = string.Concat("同步修改SAP客户成功，SAP客户号：", sapCode);
                    }
                    else
                    {
                        sapResult = string.Concat("同步修改SAP客户失败，SAP错误返回：", sapResult);
                    }
                    result.Message = sapResult;
                    //这个写入是成功
                    _baseDataRepository.UpdateSynTipMsg(entityId, recId, sapResult, tran);
                }
                else
                {
                    sapResult = "同步修改SAP客户失败，SAP客户号异常";
                    result.Message = sapResult;
                    //这个写入是失败
                    _baseDataRepository.UpdateSynTipMsg2(entityId, recId, sapResult, tran);
                }
            }

            return result;
        }
        #endregion

        public void AutoSubmitCustomerToSapNew(Guid caseId, int userId, DbTransaction tran)
        {
            var result = new SynResultModel();
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
            var data = _baseDataRepository.GetEntityIdAndRecIdByCaseId(tran,caseId,userId);
            if (data != null)
            {
				//合同审批通过自动提交客户同步到SAP
				var custEntityId = EntityReg.CustomerEntityId();
				var detailData = _baseDataServices.GetEntityDetailData(tran,data.EntityId, data.RecId, userId);
				logger.Info(string.Format("GetEntityDetailData:{0}", JsonHelper.ToJson(detailData)));
				if (detailData != null)
				{
					var customerJson = string.Concat(detailData["customer"]);
					if (!string.IsNullOrEmpty(customerJson))
					{
						var ds = JsonHelper.ToObject<DataSourceInfo>(customerJson);
						if (ds != null)
						{
							detailData = _baseDataServices.GetEntityDetailData(tran,custEntityId, ds.id, userId);
							if (detailData != null)
							{
                                if (detailData.ContainsKey("issynchrosap")  ==false 
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
            var funRet=_baseDataRepository.ExcuteActionExt(transaction, "crm_fhsj_customer_edit", basicParamData, preActionResult, actionResult, (int)userId);
            var paramData = basicParamData as DynamicEntityEditModel;
            Dictionary<string, object> fieldData = paramData.FieldData;
            logger.Info("修改客户开始：" + paramData.RecId.ToString());
            //判断是否需要提交
            var detailData = _baseDataServices.GetEntityDetailData(transaction, Guid.Parse("f9db9d79-e94b-4678-a5cc-aa6e281c1246"), paramData.RecId, (int)userId);
            if (detailData != null)
            {
                logger.Info("修改客户调用sap开始："+ paramData.RecId.ToString());
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
                                this.SynSapCustDataJx(projectEntityId,companyId, userId);
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
            if (param.Count == 0  || !param.ContainsKey("recid")
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
                detailData["distribution"]="3";//经销
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
    }
}
