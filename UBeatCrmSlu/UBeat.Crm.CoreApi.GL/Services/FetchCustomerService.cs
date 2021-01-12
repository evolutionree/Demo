using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

using System.Text;
using System.Threading.Tasks;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.DynamicEntity;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.WJXModel;
using UBeat.Crm.CoreApi.Services.Utility;
using UBeat.Crm.CoreApi.Services.Utility.MsgForPug_inUtility;
using System.Linq;
using UBeat.Crm.LicenseCore;
using NLog;
using UBeat.Crm.CoreApi.GL.Model;
using UBeat.Crm.CoreApi.GL.Repository;
using UBeat.Crm.CoreApi.DomainModel.Utility;
using UBeat.Crm.CoreApi.GL.Utility;
using Newtonsoft.Json;
using UBeat.Crm.CoreApi.Services.Services;
using ICustomerRepository = UBeat.Crm.CoreApi.GL.Repository.ICustomerRepository;
using System.Data.Common;

namespace UBeat.Crm.CoreApi.GL.Services
{
    public class FetchCustomerServices : BasicBaseServices
    {
        private readonly Logger logger = LogManager.GetLogger("UBeat.Crm.CoreApi.GL.Services.FetchCustomerService");

        private readonly IBaseDataRepository _baseDataRepository;
        private readonly BaseDataServices _baseDataServices;

        private Dictionary<Int32, Dictionary<string, SaveDicData>> _dicDicTypeData;

        private Dictionary<string, Int32> _dicRegionNameData;
        private Dictionary<string, Int32> _dicRegionFullNameData;
        private readonly ICustomerRepository _customerRepository;

        private Int32 SynLimitCount = 1;

        public FetchCustomerServices(IBaseDataRepository baseDataRepository, ICustomerRepository customerRepository, BaseDataServices baseDataServices)
        {
            _baseDataRepository = baseDataRepository;
            _customerRepository = customerRepository;
            _baseDataServices = baseDataServices;
            initCrmBaseData();
        }

        public void initCrmBaseData()
        {
            _dicDicTypeData = new Dictionary<int, Dictionary<string, SaveDicData>>();
            var list = _baseDataRepository.GetDicData();
            foreach (var item in list)
            {
                if (string.IsNullOrEmpty(item.ExtField1)) continue;
                if (item.DicTypeId <= 0) continue;

                if (!_dicDicTypeData.ContainsKey(item.DicTypeId))
                {
                    var dic = new Dictionary<string, SaveDicData>();
                    dic.Add(item.ExtField1, item);
                    _dicDicTypeData.Add(item.DicTypeId, dic);
                }
                else
                {
                    var dic = _dicDicTypeData[item.DicTypeId];
                    if (!dic.ContainsKey(item.ExtField1))
                        dic.Add(item.ExtField1, item);
                    _dicDicTypeData[item.DicTypeId] = dic;
                }
            }

            _dicRegionNameData = new Dictionary<string, int>();
            _dicRegionFullNameData = new Dictionary<string, int>();
            var regList = _baseDataRepository.GetRegionCityData();
            foreach (var item in regList)
            {
                if (string.IsNullOrEmpty(item.RegionName)) continue;
                if (string.IsNullOrEmpty(item.FullName)) continue;

                if (!_dicRegionNameData.ContainsKey(item.RegionName))
                {
                    _dicRegionNameData.Add(item.RegionName, item.RegionId);
                }
                if (!_dicRegionFullNameData.ContainsKey(item.FullName))
                {
                    _dicRegionFullNameData.Add(item.FullName, item.RegionId);
                }
            }
        }

        public OperateResult FetchCustData(SynSapModel model, int userid, int opt = 0, DbTransaction tran = null)
        {
            userid = userid == 0 ? 1 : userid;
            var sapResult = string.Empty;
            var optResult = new OperateResult();
            optResult.Msg = string.Format(@"同步SAP客户成功");
            optResult.Flag = 1;

            //opt=1是全量 opt=0增量 opt=3单个
            var logTime = DateTime.Now;
            var postData = new Dictionary<string, string>();
            var headData = new Dictionary<string, string>();
            headData.Add("Transaction_ID", "CUSTOMER_DATA");
            Dictionary<string, object> paramInfo = null;
            paramInfo = model.OtherParams;
            var fetchDateTime = DateTime.Now.ToString("yyyy-MM-dd");
            postData.Add("PARTNER", "");
            if (paramInfo != null)
            {
                try
                {
                    if (paramInfo.ContainsKey("fetchday") && paramInfo["fetchday"] != null)
                    {
                        fetchDateTime = DateTime.Parse(paramInfo["fetchday"].ToString()).ToString("yyyy-MM-dd");
                    }
                }
                catch (Exception ex1)
                {
                    logger.Log(LogLevel.Error, string.Format(@"获取SAP客户请求参数：{0}，错误返回：{1}", paramInfo.ToString(), ex1.Message));
                }
            }
            postData.Add("REQDATE", fetchDateTime);

            if (opt == 1)
            {
                //全量
                fetchDateTime = "";
                postData["REQDATE"] = fetchDateTime;
            }
            else if (opt == 3)
            {
                var detailData = _baseDataServices.GetEntityDetailData(tran, model.EntityId, model.RecIds[0], userid);
                string erpcode = isNull(detailData, "erpcode").StringMax(0, 10);
                if (string.IsNullOrEmpty(erpcode))
                {
                    optResult.Flag = 0;
                    optResult.Msg = "SAP编号不能为空";
                    return optResult;
                }
                else
                {
                    postData["PARTNER"] = erpcode;
                }
            }

            logger.Info(string.Concat("获取SAP客户请求参数：", JsonHelper.ToJson(postData)));
            var result = CallAPIHelper.ApiPostData(postData, headData);
            SapCustModelResult sapRequest = JsonConvert.DeserializeObject<SapCustModelResult>(result);
            if (sapRequest.TYPE != "S")
            {
                logger.Log(LogLevel.Error, $"获取SAP客户接口异常报错：{sapRequest.MESSAGE}");
            }

            initCrmBaseData();

            if (sapRequest.DATA != null && sapRequest.DATA.CUST_MAIN.Count > 0)
            {
                List<SaveCustomerMainView> cv = new List<SaveCustomerMainView>();

                Dictionary<string, CUST_SALE> dicSale = new Dictionary<string, CUST_SALE>();
                Dictionary<string, CUST_TAXN> dicTax = new Dictionary<string, CUST_TAXN>();
                Dictionary<string, CUST_CRED> dicCredit = new Dictionary<string, CUST_CRED>();
                Dictionary<string, CUST_COMP> dicComp = new Dictionary<string, CUST_COMP>();
                Dictionary<string, CUST_BANK> dicBank = new Dictionary<string, CUST_BANK>();

                foreach (var item in sapRequest.DATA.CUST_SALE)
                {
                    if (!dicSale.ContainsKey(item.PARTNER))
                        dicSale.Add(item.PARTNER, item);
                }
                foreach (var item in sapRequest.DATA.CUST_TAXN)
                {
                    if (!dicTax.ContainsKey(item.PARTNER))
                        dicTax.Add(item.PARTNER, item);
                }
                foreach (var item in sapRequest.DATA.CUST_CRED)
                {
                    if (!dicCredit.ContainsKey(item.PARTNER))
                        dicCredit.Add(item.PARTNER, item);
                }
                foreach (var item in sapRequest.DATA.CUST_COMP)
                {
                    if (!dicComp.ContainsKey(item.PARTNER))
                        dicComp.Add(item.PARTNER, item);
                }
                foreach (var item in sapRequest.DATA.CUST_BANK)
                {
                    if (!dicBank.ContainsKey(item.PARTNER))
                        dicBank.Add(item.PARTNER, item);
                }

                foreach (var item in sapRequest.DATA.CUST_MAIN)
                {
                    if (string.IsNullOrEmpty(string.Concat(item.PARTNER))) continue;

                    SaveCustomerMainView v = new SaveCustomerMainView();
                    v.companyone = item.PARTNER.Trim().PadLeft(10, '0');//客户编号
                    v.reccode = item.CRMCUST.Trim().Replace("CRM", "");//CRM流水号

                    if (!string.IsNullOrEmpty(item.KUKLA))
                    {
                        v.custgpone_sapcode = item.KUKLA.Trim();//客户分类
                        if (_dicDicTypeData.ContainsKey((int)DicTypeEnum.客户类型))
                        {
                            var dic = _dicDicTypeData[(int)DicTypeEnum.客户类型];
                            if (dic.ContainsKey(v.custgpone_sapcode))
                            {
                                v.custgpone_crmid = dic[v.custgpone_sapcode].DataId;
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(item.BRAN1))
                    {
                        v.custgptwo_sapcode = item.BRAN1.Trim();//客户行业 
                        if (_dicDicTypeData.ContainsKey((int)DicTypeEnum.客户行业))
                        {
                            var dic = _dicDicTypeData[(int)DicTypeEnum.客户行业];
                            if (dic.ContainsKey(v.custgptwo_sapcode))
                            {
                                v.custgptwo_crmid = dic[v.custgptwo_sapcode].DataId;
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(item.TAXKD))
                    {
                        v.taxgp_sapcode = item.TAXKD.Trim();
                        if (_dicDicTypeData.ContainsKey((int)DicTypeEnum.税分类))
                        {
                            var dic = _dicDicTypeData[(int)DicTypeEnum.税分类];
                            if (dic.ContainsKey(v.taxgp_sapcode))
                            {
                                v.taxgp_crmid = dic[v.taxgp_sapcode].DataId;
                            }
                        }
                    }

                    #region sale
                    //VKORG, VTWEG, SPART, BZIRK, VKBUR, KALKS, VSBED, ZTERM, KTGRD, WAERS 
                    CUST_SALE sales = null;
                    if (dicSale.ContainsKey(v.companyone))
                        sales = dicSale[v.companyone];
                    if (sales != null)
                    {
                        #region
                        if (!string.IsNullOrEmpty(sales.VKORG))
                        {
                            v.salesorganization_sapcode = sales.VKORG.Trim();//销售组织
                            if (_dicDicTypeData.ContainsKey((int)DicTypeEnum.销售组织))
                            {
                                var dic = _dicDicTypeData[(int)DicTypeEnum.销售组织];
                                if (dic.ContainsKey(v.salesorganization_sapcode))
                                {
                                    v.salesorganization_crmid = dic[v.salesorganization_sapcode].DataId;
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(sales.VTWEG))
                        {
                            v.distribution_sapcode = sales.VTWEG.Trim();//分销渠道
                            if (_dicDicTypeData.ContainsKey((int)DicTypeEnum.分销渠道))
                            {
                                var dic = _dicDicTypeData[(int)DicTypeEnum.分销渠道];
                                if (dic.ContainsKey(v.distribution_sapcode))
                                {
                                    v.distribution_crmid = dic[v.distribution_sapcode].DataId;
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(sales.SPART))
                        {
                            v.productgroup_sapcode = sales.SPART.Trim();//产品组
                            if (_dicDicTypeData.ContainsKey((int)DicTypeEnum.产品组))
                            {
                                var dic = _dicDicTypeData[(int)DicTypeEnum.产品组];
                                if (dic.ContainsKey(v.productgroup_sapcode))
                                {
                                    v.productgroup_crmid = dic[v.productgroup_sapcode].DataId;
                                }
                            }
                        }

                        #endregion
                        #region
                        if (!string.IsNullOrEmpty(sales.BZIRK))
                        {
                            v.salesarea_sapcode = sales.BZIRK.Trim();//销售地区
                            if (_dicDicTypeData.ContainsKey((int)DicTypeEnum.销售地区))
                            {
                                var dic = _dicDicTypeData[(int)DicTypeEnum.销售地区];
                                if (dic.ContainsKey(v.salesarea_sapcode))
                                {
                                    v.salesarea_crmid = dic[v.salesarea_sapcode].DataId;
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(sales.VKBUR))
                        {
                            v.salesoffice_sapcode = sales.VKBUR.Trim();//销售办事处
                            if (_dicDicTypeData.ContainsKey((int)DicTypeEnum.销售办事处))
                            {
                                var dic = _dicDicTypeData[(int)DicTypeEnum.销售办事处];
                                if (dic.ContainsKey(v.salesoffice_sapcode))
                                {
                                    v.salesoffice_crmid = dic[v.salesoffice_sapcode].DataId;
                                }
                            }
                        }
                        //用于定价过程确定的客户分类 默认值1
                        if (!string.IsNullOrEmpty(sales.KALKS))
                        {
                            v.pricingpro_sapcode = sales.KALKS;
                        }
                        if (!string.IsNullOrEmpty(sales.VSBED))
                        {
                            v.shipment_sapcode = sales.VSBED;//装运条件
                            if (_dicDicTypeData.ContainsKey((int)DicTypeEnum.装运条件))
                            {
                                var dic = _dicDicTypeData[(int)DicTypeEnum.装运条件];
                                if (dic.ContainsKey(v.shipment_sapcode))
                                {
                                    v.shipment_crmid = dic[v.shipment_sapcode].DataId;
                                }
                            }
                        }
                        #endregion
                        #region
                        if (!string.IsNullOrEmpty(sales.ZTERM))
                        {
                            v.payment_sapcode = sales.ZTERM;//付款条件代码
                            if (_dicDicTypeData.ContainsKey((int)DicTypeEnum.付款条件))
                            {
                                var dic = _dicDicTypeData[(int)DicTypeEnum.付款条件];
                                if (dic.ContainsKey(v.payment_sapcode))
                                {
                                    v.payment_crmid = dic[v.payment_sapcode].DataId;
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(sales.KTGRD))
                        {
                            v.accountgp_sapcode = sales.KTGRD.Trim();//客户科目分配组 
                            if (_dicDicTypeData.ContainsKey((int)DicTypeEnum.客户科目分配组))
                            {
                                var dic = _dicDicTypeData[(int)DicTypeEnum.客户科目分配组];
                                if (dic.ContainsKey(v.accountgp_sapcode))
                                {
                                    v.accountgp_crmid = dic[v.accountgp_sapcode].DataId;
                                }
                            }
                        }
                        //货币 默认值CNY 
                        if (!string.IsNullOrEmpty(sales.WAERS))
                        {
                            v.currency_sapcode = sales.WAERS.Trim();
                            if (_dicDicTypeData.ContainsKey((int)DicTypeEnum.币种))
                            {
                                var dic = _dicDicTypeData[(int)DicTypeEnum.币种];
                                if (dic.ContainsKey(v.currency_sapcode))
                                {
                                    v.currency_crmid = dic[v.currency_sapcode].DataId;
                                }
                            }
                        }
                        #endregion
                    }
                    #endregion

                    #region TAXKD
                    #endregion

                    #region CUST_COMP
                    //BUKRS
                    CUST_COMP burk = null;
                    if (dicComp.ContainsKey(v.companyone))
                        burk = dicComp[v.companyone];
                    //公司代码 默认值1000
                    //总帐中的统驭科目 默认值1122010000
                    if (burk != null)
                    {
                        #region
                        if (!string.IsNullOrEmpty(burk.BUKRS))
                        {
                            v.companycode = burk.BUKRS.Trim();
                        }
                        if (!string.IsNullOrEmpty(burk.AKONT))
                        {
                            v.accountantsub = burk.AKONT.Trim();
                        }
                        #endregion
                    }
                    #endregion

                    #region  BANKL, BANKD
                    CUST_BANK bank = null;
                    if (dicBank.ContainsKey(item.PARTNER))
                        bank = dicBank[item.PARTNER];
                    if (bank != null)
                    {
                        v.opencode = bank.BANKL.Trim();//银行编号
                        v.accountcode = bank.BANKL;//银行代码
                    }
                    #endregion

                    #region creadit CREDIT_SGMNT, CREDIT_LIMIT
                    CUST_CRED creadit = null;
                    if (dicCredit.ContainsKey(v.companyone))
                        creadit = dicCredit[v.companyone];
                    if (creadit != null)
                    {
                        #region
                        if (!string.IsNullOrEmpty(creadit.CREDIT_SGMNT))
                        {
                            decimal res = 1000;
                            if (decimal.TryParse(creadit.CREDIT_SGMNT.Trim(), out res))
                            {
                                v.creditperiod = res;
                            };
                        }
                        if (creadit.CREDIT_LIMIT >= 0)
                        {
                            v.risklimit = creadit.CREDIT_LIMIT;
                        }
                        #endregion
                    }
                    #endregion

                    #region KTOKD, TITLE, NAME1, NAME2, SORTL
                    if (!string.IsNullOrEmpty(item.KTOKD))
                    {
                        v.customertype_sapcode = item.KTOKD.Trim();//客户帐户组
                        if (_dicDicTypeData.ContainsKey((int)DicTypeEnum.客户账户组))
                        {
                            var dic = _dicDicTypeData[(int)DicTypeEnum.客户账户组];
                            if (dic.ContainsKey(v.customertype_sapcode))
                            {
                                v.customertype_crmid = dic[v.customertype_sapcode].DataId;
                            }
                        }
                    }
                    //v.appellation_sapcode = "0003";//称谓 默认值0003
                    //v.appellation_crmid = 1;
                    if (!string.IsNullOrEmpty(item.NAME1))
                    {
                        v.recname = item.NAME1.Trim();//名称1
                    }
                    if (!string.IsNullOrEmpty(item.SORTL))
                    {
                        v.searchone = item.SORTL.Trim();//检索词对于匹配码搜索 
                    }
                    #endregion

                    #region STREET, ORT01, LAND1, REGIO, PSTLZ
                    if (!string.IsNullOrEmpty(item.STREET))
                    {
                        var add = new Address();
                        add.address = item.STREET.Trim();
                        v.address = JsonHelper.ToJson(add);//街道 
                    }
                    if (!string.IsNullOrEmpty(item.ORT01))
                    {
                        v.city = item.ORT01.Trim();//城市
                        if (_dicRegionNameData.ContainsKey(v.city))
                        {
                            v.city_crmid = _dicRegionNameData[v.city];
                        }
                        else if (_dicRegionFullNameData.ContainsKey(v.city))
                        {
                            v.city_crmid = _dicRegionFullNameData[v.city];
                        }
                    }
                    if (!string.IsNullOrEmpty(item.LAND1))
                    {
                        v.country_sapcode = item.LAND1.Trim();//国家/地区代码
                        if (_dicDicTypeData.ContainsKey((int)DicTypeEnum.国家))
                        {
                            var dic = _dicDicTypeData[(int)DicTypeEnum.国家];
                            if (dic.ContainsKey(v.country_sapcode))
                            {
                                v.country_crmid = dic[v.country_sapcode].DataId;
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(item.REGIO))
                    {
                        v.region_sapcode = item.REGIO.Trim();//地区（省/自治区/直辖市、市、县）  
                        if (_dicDicTypeData.ContainsKey((int)DicTypeEnum.地区))
                        {
                            var dic = _dicDicTypeData[(int)DicTypeEnum.地区];
                            if (dic.ContainsKey(v.region_sapcode))
                            {
                                v.region_crmid = dic[v.region_sapcode].DataId;
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(item.PSTLZ))
                    {
                        v.postcode = item.PSTLZ.Trim();//城市邮政编码  
                    }
                    #endregion


                    #region SPRAS, TELF1, TELF2, TELFX
                    v.language = "ZH";//语言 默认值ZH
                    if (!string.IsNullOrEmpty(item.TELF1))
                    {
                        v.taxphone = item.TELF1.Trim();//第一个电话号  
                    }
                    if (!string.IsNullOrEmpty(item.TELF2))
                    {
                        v.mobilephone = item.TELF2.Trim();//第二个电话号  
                    }
                    if (!string.IsNullOrEmpty(item.TELFX))
                    {
                        v.fax = item.TELFX.Trim();//第一个传真号: 拨号 + 编号  
                    }
                    #endregion

                    #region TAXNUM, SMTP_ADDR 
                    if (!string.IsNullOrEmpty(item.STCD5))
                    {
                        v.valueadd = item.STCD5.Trim();//增值税登记号   
                    }
                    if (!string.IsNullOrEmpty(item.SMTP_ADDR))
                    {
                        v.email = item.SMTP_ADDR.Trim();//电子邮件地址  
                    }
                    #endregion

                    //convertItems(v, dicSales, dicTax, dicBurk);
                    cv.Add(v);
                }
                if (cv.Count > 0)
                {
                    saveCustData(cv);
                }
            }

            return optResult;
        }

        public void saveCustData(List<SaveCustomerMainView> dataList)
        {
            var userId = 1;
            var limit = SynLimitCount;
            var count = 0;
            var commitList = new List<SaveCustomerMainView>();
            foreach (var item in dataList)
            {
                count++;
                commitList.Add(item);

                if (count > limit)
                {
                    submit(commitList, userId);
                    count = 0;
                    commitList.Clear();
                }
            }

            if (commitList.Count > 0)
            {
                submit(commitList, userId);
            }

            var codeList = getDeleteCode(dataList);
            var deleteCodeList = _customerRepository.getDeleteCode(codeList);
            if (deleteCodeList.Count > 0)
            {
                _customerRepository.DeleteList(deleteCodeList, userId);
            }
        }

        private void submit(List<SaveCustomerMainView> dataList, int userId)
        {
            try
            {
                var dicData = getDicData(dataList);
                var codeList = getCode(dataList);
                var reccodeList = getLostCode(dataList);
                var reccodeLostList = getCrmLostData(dataList);


                var modifyCodeList = _customerRepository.getModifyCode(codeList);
                if (modifyCodeList.Count > 0)
                {
                    foreach (var item in modifyCodeList)
                    {
                        var list = new List<SaveCustomerMainView>();
                        try
                        {
                            if (dicData.ContainsKey(item))
                                list.Add(dicData[item]);
                            if (list.Count > 0)
                            {
                                _customerRepository.ModifyList(list, userId);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Info(string.Format(@"增量同步修改失败的客户数据：{0}，错误返回：{1}", list.Count > 0 ? JsonHelper.ToJson(list) : "", ex.Message));
                            continue;
                        }
                    }

                }

                var crmLostList = _customerRepository.getCrmLostCode(reccodeList);
                //查询同步异常，sap已经创建了，同时记录crm流水号的数据
                if (crmLostList.Count > 0)
                {
                    foreach (var item in crmLostList)
                    {
                        var list = new List<SaveCustomerMainView>();
                        try
                        {
                            if (reccodeLostList.ContainsKey(item))
                                list.Add(reccodeLostList[item]);
                            if (list.Count > 0)
                            {
                                _customerRepository.ModifyLostList(list, userId);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Info(string.Format(@"增量同步修改失败的客户数据：{0}，错误返回：{1}", list.Count > 0 ? JsonHelper.ToJson(list) : "", ex.Message));
                            continue;
                        }
                    }
                }

                var addCodeList = _customerRepository.getAddCode(codeList);
                if (addCodeList.Count > 0)
                {
                    foreach (var item in addCodeList)
                    {
                        var list = new List<SaveCustomerMainView>();
                        try
                        {
                            if (dicData.ContainsKey(item))
                                list.Add(dicData[item]);
                            if (list.Count > 0)
                            {
                                _customerRepository.AddList(list, userId);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Info(string.Format(@"增量同步插入失败的客户数据：{0}，错误返回：{1}", list.Count > 0 ? JsonHelper.ToJson(list) : "", ex.Message));
                            continue;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                logger.Info(string.Format(@"增量同步失败的客户数据：{0}，错误返回：{1}", dataList, ex.Message));
            }
        }
        private List<string> getDeleteCode(List<SaveCustomerMainView> dataList)
        {
            var list = new List<string>();

            foreach (var item in dataList)
            {
                if (item.status == Status.Disable)
                    list.Add(item.companyone);
            }

            return list;
        }

        private List<string> getCode(List<SaveCustomerMainView> dataList)
        {
            var list = new List<string>();

            foreach (var item in dataList)
            {
                list.Add(item.companyone);
            }

            return list;
        }

        private List<string> getLostCode(List<SaveCustomerMainView> dataList)
        {
            var list = new List<string>();

            foreach (var item in dataList)
            {
                if (string.IsNullOrEmpty(item.reccode)) continue;

                if (!list.Contains(item.reccode))
                {
                    list.Add(item.reccode);
                }
            }

            return list;
        }
        private Dictionary<string, SaveCustomerMainView> getDicData(List<SaveCustomerMainView> dataList)
        {
            var dic = new Dictionary<string, SaveCustomerMainView>();

            foreach (var item in dataList)
            {
                if (string.IsNullOrEmpty(item.companyone)) continue;

                if (!dic.ContainsKey(item.companyone))
                {
                    dic.Add(item.companyone, item);
                }
            }

            return dic;
        }

        private Dictionary<string, SaveCustomerMainView> getCrmLostData(List<SaveCustomerMainView> dataList)
        {
            var dic = new Dictionary<string, SaveCustomerMainView>();

            foreach (var item in dataList)
            {
                if (string.IsNullOrEmpty(item.reccode)) continue;

                if (!dic.ContainsKey(item.reccode))
                {
                    dic.Add(item.reccode, item);
                }
            }

            return dic;
        }

        public OperateResult getCustomerCreditLimit(CustomerCreditLimitParam param, int userId)
        {
            var detailData = _baseDataServices.GetEntityDetailData(null, param.EntityId, param.RecId, userId);
            if (detailData != null && detailData["erpcode"] != null && !string.IsNullOrEmpty(detailData["erpcode"].ToString()))
            {
                var header = new Dictionary<String, string>();
                header.Add("Transaction_ID", "CREDIT_QUERY");
                var postData = new Dictionary<String, string>();
                postData.Add("PARTNER", detailData["erpcode"].ToString());
                String result = CallAPIHelper.ApiPostData(postData, header);

            }
            return new OperateResult()
            {
                Flag = 0,
                Msg = "客户号不能为null"
            };
        }

    }
}
