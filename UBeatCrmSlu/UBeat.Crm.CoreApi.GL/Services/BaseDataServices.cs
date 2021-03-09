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
using UBeat.Crm.CoreApi.GL.Utility;
using NLog;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.DomainModel.Utility;
using Newtonsoft.Json;
using UBeat.Crm.CoreApi.GL.Model;
using UBeat.Crm.CoreApi.GL.Repository;
using UBeat.Crm.CoreApi.Services.Services;
using System.Data.Common;

namespace UBeat.Crm.CoreApi.GL.Services
{
    public class BaseDataServices : BasicBaseServices
    {
        private readonly Logger logger = LogManager.GetLogger("UBeat.Crm.CoreApi.GL.Services.BaseDataServices");
        private readonly DynamicEntityServices _dynamicEntityServices;
        private readonly IBaseDataRepository _baseDataRepository;

        private string sapUrl;

        public BaseDataServices(IBaseDataRepository baseDataRepository, DynamicEntityServices dynamicEntityServices)
        {
            _dynamicEntityServices = dynamicEntityServices;
            _baseDataRepository = baseDataRepository;
        }

        #region 字典model
        public void InitDicDataQrtz()
        {
            InitDicData();
        }

        public OutputResult<object> InitDicData()
        {
            try
            {
                int mainSaleOrg = 9001;
                var Config = ServiceLocator.Current.GetInstance<IConfigurationRoot>().GetSection("SapConfig");
                if (Config != null)
                {
                    mainSaleOrg = Config.GetValue<int>("MainSaleOrg");
                }
                var logTime = DateTime.Now;
                var postData = new Dictionary<string, object>();
                var headData = new Dictionary<string, string>();
                headData.Add("Transaction_ID", "BASIC_DATA");
                logger.Info(string.Concat("获取全量SAP字典请求参数：", JsonHelper.ToJson(headData)));
                var result = CallAPIHelper.ApiPostData(postData, headData);
                SapDicModelResult sapRequest =JsonConvert.DeserializeObject<SapDicModelResult>(result);
                if (sapRequest.TYPE != "S")
                {
                    logger.Log(LogLevel.Error, $"获取全量SAP字典接口异常报错：{sapRequest.MESSAGE}");
                }
                var datas = new List<SaveDicData>();
                //渠道
                if (sapRequest.DATA.TVTWT.Count > 0) {
                    datas=convert_TVTWT_ToCrm(sapRequest.DATA.TVTWT);
                    initCrmData(datas, (Int32)DicTypeIdSynEnum.分销渠道, false);
                    logger.Info(string.Concat("同步分销渠道条数：", sapRequest.DATA.TVTWT.Count));
                }
                //销售组织
                if (sapRequest.DATA.TVKOT.Count > 0)
                {
                    datas=convert_TVKOT_ToCrm(sapRequest.DATA.TVKOT);
                    initCrmData(datas, (Int32)DicTypeIdSynEnum.销售组织, false);
                    logger.Info(string.Concat("同步销售组织条数：", sapRequest.DATA.TVKOT.Count));
                }
                //销售办事处
                if (sapRequest.DATA.TVKBT.Count > 0)
                {
                    datas=convert_TVKBT_ToCrm(sapRequest.DATA.TVKBT);
                    initCrmData(datas, (Int32)DicTypeIdSynEnum.销售办事处, false);
                    logger.Info(string.Concat("同步销售办事处条数：", sapRequest.DATA.TVKBT.Count));
                }
                //产品组
                if (sapRequest.DATA.TSPAT.Count > 0)
                {
                    datas=convert_TSPAT_ToCrm(sapRequest.DATA.TSPAT);
                    initCrmData(datas, (Int32)DicTypeIdSynEnum.产品组, false);
                    logger.Info(string.Concat("同步产品组条数：", sapRequest.DATA.TSPAT.Count));
                }
                //工厂
                if (sapRequest.DATA.T001W.Count > 0)
                {
                    datas=convert_T001W_ToCrm(sapRequest.DATA.T001W);
                    initCrmData(datas, (Int32)DicTypeIdSynEnum.工厂, false);
                    logger.Info(string.Concat("同步工厂条数：", sapRequest.DATA.T001W.Count));
                }
                //销售地区
                if (sapRequest.DATA.T171T.Count > 0)
                {
                    datas=convert_T171T_ToCrm(sapRequest.DATA.T171T);
                    initCrmData(datas, (Int32)DicTypeIdSynEnum.销售地区, false);
                    logger.Info(string.Concat("同步销售地区条数：", sapRequest.DATA.T171T.Count));
                }
                //物料类型
                if (sapRequest.DATA.T134T.Count > 0)
                {
                    datas=convert_T134T_ToCrm(sapRequest.DATA.T134T);
                    initCrmData(datas, (Int32)DicTypeIdSynEnum.物料类型, false);
                    logger.Info(string.Concat("同步物料类型条数：", sapRequest.DATA.T134T.Count));
                }
                //销售凭证类型
                if (sapRequest.DATA.TVAKT.Count > 0)
                {
                    datas=convert_TVAKT_ToCrm(sapRequest.DATA.TVAKT);
                    initCrmData(datas, (Int32)DicTypeIdSynEnum.订单类型, false);
                    logger.Info(string.Concat("同步销售凭证类型条数：", sapRequest.DATA.TVAKT.Count));
                }
                //库位
                if (sapRequest.DATA.T001L.Count > 0)
                {
                    datas=convert_T001L_ToCrm(sapRequest.DATA.T001L);
                    initCrmData(datas, (Int32)DicTypeIdSynEnum.库位, true);
                    logger.Info(string.Concat("同步库位条数：", sapRequest.DATA.T001L.Count));
                }
                //付款条件
                if (sapRequest.DATA.T052U.Count > 0)
                {
                    datas=convert_T052U_ToCrm(sapRequest.DATA.T052U);
                    initCrmData(datas, (Int32)DicTypeIdSynEnum.付款条件, false);
                    logger.Info(string.Concat("同步付款条件条数：", sapRequest.DATA.T052U.Count));
                }
                //订单原因
                if (sapRequest.DATA.TVAUT.Count > 0)
                {
                    datas=convert_TVAUT_ToCrm(sapRequest.DATA.TVAUT);
                    initCrmData(datas, (Int32)DicTypeIdSynEnum.订单原因, false);
                    logger.Info(string.Concat("同步订单原因条数：", sapRequest.DATA.TVAUT.Count));
                }
                //拒绝原因
                if (sapRequest.DATA.TVAGT.Count > 0)
                {
                    datas=convert_TVAGT_ToCrm(sapRequest.DATA.TVAGT);
                    initCrmData(datas, (Int32)DicTypeIdSynEnum.拒绝原因, false);
                    logger.Info(string.Concat("同步拒绝原因条数：", sapRequest.DATA.TVAGT.Count));
                }
                //成本中心
                if (sapRequest.DATA.CSKT.Count > 0)
                {
                    datas=convert_CSKT_ToCrm(sapRequest.DATA.CSKT);
                    initCrmData(datas, (Int32)DicTypeIdSynEnum.成本中心, false);
                    logger.Info(string.Concat("同步成本中心条数：", datas.Count));
                }
                //特殊处理标识
                if (sapRequest.DATA.TVSAKT.Count > 0)
                {
                    datas=convert_TVSAKT_ToCrm(sapRequest.DATA.TVSAKT);
                    initCrmData(datas, (Int32)DicTypeIdSynEnum.特殊处理标识, false);
                    logger.Info(string.Concat("同步特殊处理标识条数：", datas.Count));
                }
                //销售项目类别 行项目类别
                if (sapRequest.DATA.TVAPT.Count > 0)
                {
                    datas=convert_TVAPT_ToCrm(sapRequest.DATA.TVAPT);
                    initCrmData(datas, (Int32)DicTypeIdSynEnum.行项目类别, false);
                    logger.Info(string.Concat("同步销售项目类别条数：", datas.Count));
                }
                if (sapRequest.DATA.TVKO.Count > 0)
                {
                    datas = convert_TVKO_ToCrm(sapRequest.DATA.TVKO, mainSaleOrg + "");
                    initCrmData(datas, (Int32)DicTypeIdSynEnum.销售组织与公司关系, false);
                    logger.Info(string.Concat("同步销售组织与公司关系条数：", datas.Count));
                }
                if (sapRequest.DATA.TVKOV.Count > 0)
                {
                    datas = convert_TVKOV_ToCrm(sapRequest.DATA.TVKOV, mainSaleOrg + "");
                    initCrmData(datas, (Int32)DicTypeIdSynEnum.销售组织与渠道关系, true);
                    logger.Info(string.Concat("同步销售组织与渠道关系条数：", datas.Count));
                }
                if (sapRequest.DATA.TVKOS.Count > 0)
                {
                    datas = convert_TVKOS_ToCrm(sapRequest.DATA.TVKOS, mainSaleOrg + "");
                    initCrmData(datas, (Int32)DicTypeIdSynEnum.销售组织与产品组关系, true);
                    logger.Info(string.Concat("同步销售组织与产品组关系条数：", datas.Count));
                }
                if (sapRequest.DATA.TVTA.Count > 0)
                {
                    datas = convert_TVTA_ToCrm(sapRequest.DATA.TVTA, mainSaleOrg + "");
                    initCrmData(datas, (Int32)DicTypeIdSynEnum.销售组织与渠道与产品组关系, true);
                    logger.Info(string.Concat("同步销售组织与渠道与产品组关系条数：", datas.Count));
                }
                logger.Info($"获取全量SAP字典处理耗时:{DateTime.Now - logTime}");
            }
            catch (Exception ex) {
                logger.Log(LogLevel.Error, $"获取全量SAP字典报错：{ex.Message}");
                return new OutputResult<object>(ex.Message, $"获取全量SAP字典处理报错：{ex.Message}", 1);
            }
            return new OutputResult<object>("获取全量SAP字典成功！");
        }

        private void initCrmData(List<SaveDicData> origiData, Int32 dicTypeId, bool isDoubleKey = false)
        {
            if (dicTypeId == 0 || origiData == null || origiData.Count <= 0) return;

            var hasData = _baseDataRepository.HasDicTypeId(dicTypeId);
            if (hasData)
            {
                var crmData = _baseDataRepository.GetDicDataByTypeId(dicTypeId);
                var extendFieldList = getExtendFieldList(crmData, isDoubleKey);

                var addList = new List<SaveDicData>();
                var updateList = new List<SaveDicData>();
                var order = 0;
                foreach (var item in origiData)
                {
                    order++;
                    var key = "";
                    if (isDoubleKey)
                    {
                        key = string.Concat(item.DataVal, item.ExtField1, item.ExtField2);
                    }
                    else
                    {
                        key = string.Concat(item.DataVal, item.ExtField1);
                    }
                    if (extendFieldList.ContainsKey(key))
                    {
                        var crmItem = extendFieldList[key];
                        crmItem.DataVal = item.DataVal;
                        crmItem.RecUpdated = item.RecUpdated;
                        crmItem.RecUpdator = item.RecUpdator;
                        crmItem.RecStatus = 1;
                        crmItem.ExtField1 = item.ExtField1;
                        if (isDoubleKey)
                        {
                            crmItem.ExtField2 = item.ExtField2;

                            //对于已经赋值的不处理
                            if (string.IsNullOrEmpty(crmItem.ExtField3))
                            {
                                crmItem.ExtField3 = item.ExtField3;
                            }
                            if (string.IsNullOrEmpty(crmItem.ExtField4))
                            {
                                crmItem.ExtField4 = item.ExtField4;
                            }
                        }
                        else {
                            if (string.IsNullOrEmpty(crmItem.ExtField2))
                            {
                                crmItem.ExtField2 = item.ExtField2;
                            }
                            if (string.IsNullOrEmpty(crmItem.ExtField3))
                            {
                                crmItem.ExtField3 = item.ExtField3;
                            }
                            if (string.IsNullOrEmpty(crmItem.ExtField4))
                            {
                                crmItem.ExtField4 = item.ExtField4;
                            }
                        }
                        updateList.Add(crmItem);
                    }
                    else
                    {
                        item.DicTypeId = dicTypeId;
                        item.RecOrder = order;
                        addList.Add(item);
                    }

                    extendFieldList.Remove(key);
                }

                var deleteList = extendFieldList.Values;
                foreach (var item in addList)
                {
                    item.DataId = _baseDataRepository.GetNextDataId(dicTypeId);
                    _baseDataRepository.AddDictionary(item);
                }
                foreach (var item in updateList)
                {
                    _baseDataRepository.UpdateDictionary(item);
                }
                foreach (var item in deleteList)
                {
                    item.RecStatus = 0;
                    _baseDataRepository.UpdateDictionary(item);
                }
            }
        }
        private Dictionary<string, SaveDicData> getExtendFieldList(List<SaveDicData> data, bool isDoubleKey = false)
        {
            var dic = new Dictionary<string, SaveDicData>();
            foreach (var item in data)
            {
                var key = "";
                if (isDoubleKey)
                {
                    key = string.Concat(item.DataVal, item.ExtField1, item.ExtField2);
                }
                else
                {
                    key = string.Concat(item.DataVal, item.ExtField1);
                }

                if (string.IsNullOrEmpty(key)) continue;
                if (!dic.ContainsKey(key))
                {
                    dic.Add(key, item);
                }
            }

            return dic;
        }
        private List<SaveDicData> convert_TVTWT_ToCrm(List<TVTWT> datas)
        {
            var list = new List<SaveDicData>();
            foreach (var item in datas)
            {
                SaveDicData data = new SaveDicData();
                data.DataVal = item.VTWEG + "_" + item.VTEXT;
                data.ExtField1 = item.VTWEG;

                list.Add(data);
            }

            return list;
        }
        private List<SaveDicData> convert_TVKOT_ToCrm(List<TVKOT> datas)
        {
            var list = new List<SaveDicData>();
            foreach (var item in datas)
            {
                SaveDicData data = new SaveDicData();
                data.DataVal = item.VKORG + "_" + item.VTEXT;
                data.ExtField1 = item.VKORG;

                list.Add(data);
            }

            return list;
        }

        private List<SaveDicData> convert_TVKBT_ToCrm(List<TVKBT> datas)
        {
            var list = new List<SaveDicData>();
            foreach (var item in datas)
            {
                SaveDicData data = new SaveDicData();
                data.DataVal = item.VKBUR + "_" + item.BEZEI;
                data.ExtField1 = item.VKBUR;

                list.Add(data);
            }

            return list;
        }
        private List<SaveDicData> convert_TSPAT_ToCrm(List<TSPAT> datas)
        {
            var list = new List<SaveDicData>();
            foreach (var item in datas)
            {
                SaveDicData data = new SaveDicData();
                data.DataVal = item.SPART + "_" + item.VTEXT;
                data.ExtField1 = item.SPART;

                list.Add(data);
            }

            return list;
        }
        private List<SaveDicData> convert_T001W_ToCrm(List<T001W> datas)
        {
            var list = new List<SaveDicData>();
            foreach (var item in datas)
            {
                SaveDicData data = new SaveDicData();
                data.DataVal = item.WERKS + "_" + item.NAME1;
                data.ExtField1 = item.WERKS;
                data.ExtField2 = item.BWKEY;

                list.Add(data);
            }

            return list;
        }

        private List<SaveDicData> convert_T171T_ToCrm(List<T171T> datas)
        {
            var list = new List<SaveDicData>();
            foreach (var item in datas)
            {
                SaveDicData data = new SaveDicData();
                data.DataVal = item.BZTXT;
                data.ExtField1 = item.BZIRK;

                list.Add(data);
            }

            return list;
        }
        private List<SaveDicData> convert_T134T_ToCrm(List<T134T> datas)
        {
            var list = new List<SaveDicData>();
            foreach (var item in datas)
            {
                SaveDicData data = new SaveDicData();
                data.DataVal = item.MTART + "_" + item.MTBEZ;
                data.ExtField1 = item.MTART;

                list.Add(data);
            }

            return list;
        }
        private List<SaveDicData> convert_TVAKT_ToCrm(List<TVAKT> datas)
        {
            var list = new List<SaveDicData>();
            foreach (var item in datas)
            {
                SaveDicData data = new SaveDicData();
                data.DataVal = item.AUART + "_" + item.BEZEI;
                data.ExtField1 = item.AUART;

                list.Add(data);
            }

            return list;
        }
        private List<SaveDicData> convert_T001L_ToCrm(List<T001L> datas)
        {
            var list = new List<SaveDicData>();
            foreach (var item in datas)
            {
                SaveDicData data = new SaveDicData();
                data.DataVal = item.LGORT + "_" + item.LGOBE;
                data.ExtField1 = item.LGORT;
                data.ExtField2 = item.WERKS;

                list.Add(data);
            }

            return list;
        }

        private List<SaveDicData> convert_T052U_ToCrm(List<T052U> datas)
        {
            var list = new List<SaveDicData>();
            foreach (var item in datas)
            {
                SaveDicData data = new SaveDicData();
                data.DataVal = item.ZTEXT;
                data.ExtField1 = item.ZTERM;
                //data.ExtField2 = item.ZTAGG;

                list.Add(data);
            }

            return list;
        }

        private List<SaveDicData> convert_TVAUT_ToCrm(List<TVAUT> datas)
        {
            var list = new List<SaveDicData>();
            foreach (var item in datas)
            {
                SaveDicData data = new SaveDicData();
                data.DataVal = item.AUGRU + "_" + item.BEZEI;
                data.ExtField1 = item.AUGRU;

                list.Add(data);
            }

            return list;
        }
        private List<SaveDicData> convert_TVAGT_ToCrm(List<TVAGT> datas)
        {
            var list = new List<SaveDicData>();
            foreach (var item in datas)
            {
                SaveDicData data = new SaveDicData();
                data.DataVal = item.ABGRU + "_" + item.BEZEI;
                data.ExtField1 = item.ABGRU;

                list.Add(data);
            }

            return list;
        }

        private List<SaveDicData> convert_CSKT_ToCrm(List<CSKT> datas)
        {
            var list = new List<SaveDicData>();
            foreach (var item in datas)
            {
                SaveDicData data = new SaveDicData();
                data.DataVal = item.KOSTL + "_" + item.KTEXT;
                data.ExtField1 = item.KOSTL;
                data.ExtField2 = item.DATBI;

                list.Add(data);
            }

            return list;
        }
        private List<SaveDicData> convert_TVSAKT_ToCrm(List<TVSAKT> datas)
        {
            var list = new List<SaveDicData>();
            foreach (var item in datas)
            {
                SaveDicData data = new SaveDicData();
                data.DataVal = item.SDABW + "_" + item.BEZEI;
                data.ExtField1 = item.SDABW;

                list.Add(data);
            }

            return list;
        }
        private List<SaveDicData> convert_TVAPT_ToCrm(List<TVAPT> datas)
        {
            var list = new List<SaveDicData>();
            foreach (var item in datas)
            {
                SaveDicData data = new SaveDicData();
                data.DataVal = item.PSTYV + "_" + item.VTEXT;
                data.ExtField1 = item.PSTYV;

                list.Add(data);
            }

            return list;
        }
        private List<SaveDicData> convert_TVKO_ToCrm(List<TVKO> datas,string mainSaleOrg)
        {
            var list = new List<SaveDicData>();
            foreach (var item in datas)
            {
                if (item.VKORG != mainSaleOrg)
                    continue;
                SaveDicData data = new SaveDicData();
                data.DataVal = item.VKORG;
                data.ExtField1 = item.BUKRS;

                list.Add(data);
            }

            return list;
        }
        private List<SaveDicData> convert_TVKOV_ToCrm(List<TVKOV> datas, string mainSaleOrg)
        {
            var list = new List<SaveDicData>();
            foreach (var item in datas)
            {
                if (item.VKORG != mainSaleOrg)
                    continue;
                SaveDicData data = new SaveDicData();
                data.DataVal = item.VKORG;
                data.ExtField1 = item.VTWEG;

                list.Add(data);
            }

            return list;
        }

        private List<SaveDicData> convert_TVKOS_ToCrm(List<TVKOS> datas, string mainSaleOrg)
        {
            var list = new List<SaveDicData>();
            foreach (var item in datas)
            {
                if (item.VKORG != mainSaleOrg)
                    continue;
                SaveDicData data = new SaveDicData();
                data.DataVal = item.VKORG;
                data.ExtField1 = item.SPART;

                list.Add(data);
            }
            return list;
        }
        private List<SaveDicData> convert_TVTA_ToCrm(List<TVTA> datas, string mainSaleOrg)
        {
            var list = new List<SaveDicData>();
            foreach (var item in datas)
            {
                if (item.VKORG != mainSaleOrg)
                    continue;
                SaveDicData data = new SaveDicData();
                data.DataVal = item.VKORG;
                data.ExtField1 = item.VTWEG;
                data.ExtField2 = item.SPART;
                data.ExtField3 = item.VTWKU;
                data.ExtField4 = item.SPAKU;

                list.Add(data);
            }
            return list;
        }
        
        #endregion

        #region 
        public IDictionary<string, object> GetEntityDetailData(DbTransaction tran, Guid entityId, Guid recId, int userId)
        {
            DynamicEntityDetailtMapper entityModel = new DynamicEntityDetailtMapper();
            entityModel.EntityId = entityId;
            entityModel.RecId = recId;
            entityModel.NeedPower = 0;
            var resultData = _dynamicEntityServices.Detail(entityModel, userId,tran);

            if (resultData != null && resultData.ContainsKey("Detail") && resultData["Detail"] != null && resultData["Detail"].Count > 0)
                return resultData["Detail"][0];

            return null;
        }
        #endregion



    }
}
