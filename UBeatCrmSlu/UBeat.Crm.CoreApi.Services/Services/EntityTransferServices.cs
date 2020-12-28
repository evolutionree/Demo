using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.DomainModel.DynamicEntity;
using UBeat.Crm.CoreApi.DomainModel.EntityPro;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Reflection;
using UBeat.Crm.CoreApi.Services.Models.DynamicEntity;
using UBeat.Crm.CoreApi.Services.Models;
using System.Text.RegularExpressions;
using System.Data.Common;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class EntityTransferServices : EntityBaseServices
    {
        private readonly IEntityTransferRepository _entityTransferRepository = null;
        private readonly IDynamicEntityRepository _dynamicEntityRepository;
        private readonly IEntityProRepository _entityProRepository = null;
        public EntityTransferServices(IEntityTransferRepository entityTransferRepository,
            IDynamicEntityRepository dynamicEntityRepository,
            IEntityProRepository entityProRepository)
        {
            _entityTransferRepository = entityTransferRepository;
            _dynamicEntityRepository = dynamicEntityRepository;
            _entityProRepository = entityProRepository;
        }
        public List<EntityTransferRuleInfo> queryRules(EntityTransferRuleQueryModel queryInfo, int userNum)
        {
            return _entityTransferRepository.queryRules(queryInfo, userNum, null);
        }

        /// <summary>
        /// 
        /// 单据转化服务入口，根据情况进入不同的方法。
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="model"></param>
        /// <param name="userNum"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public Dictionary<string, IDictionary<string, object>> TransferBill(AnalyseHeader header, EntityTransferActionModel model, int userNum, string userName)
        {
            EntityTransferRuleInfo transferRuleInfo = _entityTransferRepository.getById(model.RuleId, userNum, null);
            if (transferRuleInfo == null)
            {
                throw (new Exception("转换规则异常"));
            }
            if (transferRuleInfo.TransferJson == null || transferRuleInfo.TransferJson.Length == 0)
            {
                throw (new Exception("转换规则异常"));
            }
            if (transferRuleInfo.DealWithOtherService != null && transferRuleInfo.DealWithOtherService.Length > 0)
            {
                //如果是其他接口处理，则转发到其他接口，直接返回其他接口的处理过程，且不做任何的事务处理

                if (transferRuleInfo.DealWithOtherService.StartsWith("this."))
                {
                    string methodName = transferRuleInfo.DealWithOtherService.Substring("this.".Length);
                    System.Reflection.MethodInfo methodInfo = null;
                    Type type = this.GetType();
                    methodInfo = type.GetMethod(methodName, new Type[] { typeof(AnalyseHeader), typeof(EntityTransferActionModel), typeof(int), typeof(string) });
                    return (Dictionary<string, IDictionary<string, object>>)methodInfo.Invoke(this, new object[] { header, model, userNum, userName });
                }
                else
                {
                    throw (new Exception("尚未支持通过第三方服务进行单据转换"));
                }

            }
            return CommonTransferBill(header, model, userNum, userName, null, null);
        }
        /// <summary>
        /// 通用转化接口，如果没有特殊情况，会在这里转化
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="model"></param>
        /// <param name="userNum"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public Dictionary<string, IDictionary<string, object>> CommonTransferBill(
                                        AnalyseHeader header,
                                        EntityTransferActionModel model,
                                        int userNum, string userName,
                                        Dictionary<string, object> extraData,
                                        Dictionary<string, object> otherProperties, DbTransaction transaction = null)
        {
            #region 获取转化规则
            EntityTransferRuleInfo transferRuleInfo = _entityTransferRepository.getById(model.RuleId, userNum, transaction);
            if (transferRuleInfo == null)
            {
                throw (new Exception("转换规则异常"));
            }
            if (transferRuleInfo.TransferJson == null || transferRuleInfo.TransferJson.Length == 0)
            {
                throw (new Exception("转换规则异常"));
            }
            try
            {

                transferRuleInfo.MapperSetting = Newtonsoft.Json.JsonConvert.DeserializeObject<EntityTransferSettingInfo>(transferRuleInfo.TransferJson);
            }
            catch (Exception ex)
            {
                throw (new Exception("转换规则异常"));
            }
            #endregion
            #region 获取源单信息
            IDictionary<string, object> srcBillInfo = getBillData(model.SrcEntityId, model.SrcRecId,userNum, transaction);
            if (srcBillInfo == null)
            {
                throw (new Exception("源单信息异常"));
            }

            List<DynamicEntityFieldSearch> srcFieldList = _dynamicEntityRepository.GetEntityFields(Guid.Parse(model.SrcEntityId), userNum);
            DealLinkTableFields(srcBillInfo, srcFieldList, userNum);
            #endregion

            IDictionary<string, object> retObj = new Dictionary<string, object>();
            #region 获取目标单据的字段定义
            List<EntityFieldProMapper> dstFieldList = _entityProRepository.FieldQuery(transferRuleInfo.DstEntity, userNum);
            Dictionary<string, List<EntityFieldProMapper>> subTablesFieldDefine = new Dictionary<string, List<EntityFieldProMapper>>();
            foreach (EntityFieldProMapper dstField in dstFieldList)
            {
                if ((EntityFieldControlType)dstField.ControlType == EntityFieldControlType.LinkeTable
                    && dstField.FieldConfig != null
                    && dstField.FieldConfig.Length > 0)
                {
                    Dictionary<string, object> configInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(dstField.FieldConfig);
                    if (configInfo.ContainsKey("entityId"))
                    {
                        string subEntityId = configInfo["entityId"].ToString();
                        List<EntityFieldProMapper> subFieldList = _entityProRepository.FieldQuery(subEntityId, userNum);
                        subTablesFieldDefine.Add(dstField.FieldName.ToLower(), subFieldList);
                    }
                }

            }
            #endregion

            Dictionary<string, object> dstDetail = new Dictionary<string, object>();

            #region 计算表头转换
            foreach (EntityFieldProMapper dstField in dstFieldList)
            {
                //初始化字段
                initDictData(dstField, dstDetail, transferRuleInfo.DstCategory, userNum, userName);
                if (transferRuleInfo.MapperSetting != null
                    && transferRuleInfo.MapperSetting.MainMappers != null
                    && transferRuleInfo.MapperSetting.MainMappers.Count > 0
                    && transferRuleInfo.MapperSetting.MainMappers.ContainsKey(dstField.FieldName.ToLower()))
                {
                    EntityFieldMapperInfo mapInfo = transferRuleInfo.MapperSetting.MainMappers[dstField.FieldName.ToLower()];
                    transferField(mapInfo, dstField, srcBillInfo, dstDetail, 0, userNum, userName, transaction);
                }
                recalcName(dstField, dstDetail, userNum);
            }
            #endregion

            #region 处理表头必填问题

            List<DynamicEntityDataFieldMapper> headerFields = _dynamicEntityRepository.GetTypeFields(Guid.Parse(transferRuleInfo.DstCategory), 0, userNum);
            foreach (DynamicEntityDataFieldMapper field in headerFields)
            {
                if (field.IsRequire == false) continue;
                if (dstDetail.ContainsKey(field.FieldName.ToLower()) == false
                    || dstDetail[field.FieldName.ToLower()] == null
                    || dstDetail[field.FieldName.ToLower()].ToString().Length == 0)
                {
                    string srcFieldName = "";
                    string srcFieldDisplayName = "";
                    string dstFieldDisplayName = "";
                    dstFieldDisplayName = "";
                    //先找配置表有没有要求
                    if (transferRuleInfo.MapperSetting != null
                        && transferRuleInfo.MapperSetting.MainMappers != null
                        && transferRuleInfo.MapperSetting.MainMappers.Count > 0
                        && transferRuleInfo.MapperSetting.MainMappers.ContainsKey(field.FieldName.ToLower()))
                    {
                        srcFieldName = transferRuleInfo.MapperSetting.MainMappers[field.FieldName.ToLower()].SrcFieldName;
                    }
                    if (srcFieldName != null)
                    {
                        foreach (DynamicEntityFieldSearch item in srcFieldList)
                        {
                            if (item.FieldName.ToLower() == srcFieldName.ToLower())
                            {
                                srcFieldDisplayName = item.FieldLabel;
                                break;
                            }
                        }
                    }
                    dstFieldDisplayName = field.FieldLabel;
                    if (srcFieldDisplayName != null && srcFieldDisplayName.Length > 0)
                    {
                        throw (new Exception(string.Format(@"转化规则要求源单字段【{0}】不能为空", srcFieldDisplayName)));
                    }
                    else
                    {
                        if (transferRuleInfo.IsAutoSave == 1)
                        {
                            throw (new Exception(string.Format(@"目标单据的字段【{0}】不能为空，但没有配置该字段的转换规则", dstFieldDisplayName)));
                        }
                    }

                }
            }
            #endregion

            #region 开始处理固定模式
             if (otherProperties == null)
            {
                otherProperties = new Dictionary<string, object>();
            }
            foreach (string key in otherProperties.Keys)
            {
                if (dstDetail.ContainsKey(key))
                {
                    dstDetail.Remove(key);
                }
                dstDetail.Add(key, otherProperties[key]);
            }
            #endregion
            #region 计算分录映射转换
            foreach (EntityFieldProMapper dstField in dstFieldList)
            {
                if (transferRuleInfo.MapperSetting.EntriesMappers == null) break;
                if ((EntityFieldControlType)dstField.ControlType != EntityFieldControlType.LinkeTable)
                {
                    continue;
                }
                if (subTablesFieldDefine.ContainsKey(dstField.FieldName.ToLower()) == false) continue;
                if (!(transferRuleInfo.MapperSetting.EntriesMappers != null && transferRuleInfo.MapperSetting.EntriesMappers.ContainsKey(dstField.FieldName.ToLower()))) continue;
                EntityTransferTableSettingInfo entryMapperInfo = (EntityTransferTableSettingInfo)transferRuleInfo.MapperSetting.EntriesMappers[dstField.FieldName.ToLower()];
                if (entryMapperInfo.MappingTables == null || entryMapperInfo.MappingTables.Length == 0) continue;//没有定义关联源Entry，则跳过
                string[] srcTables = entryMapperInfo.MappingTables.Split(',');
                bool isNeedCalc = true;
                if (srcTables.Length > 1) { throw new Exception("暂时不支持多个源表格映射到一个目标表格中"); }
                foreach (string tableFieldName in srcTables)
                {
                    string srcFieldName = tableFieldName.ToLower();
                    if (srcFieldName.Equals("")) {
                        continue;//如果为空，表示映射表头信息，就是表头转表体
                    }
                    if (srcBillInfo.ContainsKey(srcFieldName) == false)
                    {
                        isNeedCalc = false;
                        break;//没有找到源分录的数据
                    }
                    object srcEntries = srcBillInfo[srcFieldName];
                    if (!(srcEntries is List<IDictionary<string, object>>))
                    {
                        isNeedCalc = false;
                        break;
                    }
                    List<IDictionary<string, object>> srcEntriesData = (List<IDictionary<string, object>>)srcEntries;
                    if (srcEntriesData.Count == 0)
                    {
                        isNeedCalc = false;
                        break;
                    }
                }
                if (isNeedCalc == false) continue;
                //开始处理表格映射
                string srcTableName = srcTables[0].ToLower();
                List<IDictionary<string, object>> srcEntryDetail = null;
                if (srcTableName.ToLower().Equals("")) {
                    srcEntryDetail = new List<IDictionary<string, object>>();
                    srcEntryDetail.Add(srcBillInfo);
                }
                else
                {
                    srcEntryDetail = (List<IDictionary<string, object>>)srcBillInfo[srcTableName];
                }
                List<IDictionary<string, object>> dstEntryDetail = new List<IDictionary<string, object>>();
                string entryCategoryID = transferRuleInfo.DstCategory;//这里要重新映射一下
                string dstFieldName = dstField.FieldName.ToLower();
                for (int index = 0; index < srcEntryDetail.Count; index++)
                {
                    //处理每一行
                    Dictionary<string, object> dstEntryItemDetail = new Dictionary<string, object>();

                    foreach (EntityFieldProMapper dstEntryField in subTablesFieldDefine[dstFieldName])
                    {
                        initDictData(dstEntryField, dstEntryItemDetail, entryCategoryID, userNum, userName);
                        if (entryMapperInfo.Mappers != null
                            && entryMapperInfo.Mappers.Count > 0
                            && entryMapperInfo.Mappers.ContainsKey(dstEntryField.FieldName.ToLower()))
                        {
                            EntityFieldMapperInfo mapInfo = entryMapperInfo.Mappers[dstEntryField.FieldName.ToLower()];
                            transferField(mapInfo, dstEntryField, srcBillInfo, dstEntryItemDetail, index, userNum, userName, transaction);
                        }

                        recalcName(dstEntryField, dstEntryItemDetail, userNum);
                    }

                    dstEntryDetail.Add(dstEntryItemDetail);
                }
                if (dstDetail.ContainsKey(dstFieldName))
                {
                    dstDetail.Remove(dstFieldName);
                }

                dstDetail.Add(dstFieldName, dstEntryDetail);
            }
            #endregion
            #region 判断是否需要保存，如果需要保存，则先保存然后重新获取新的数据
            if (transferRuleInfo.IsAutoSave == 1)
            {
                DynamicEntityServices entityService = (DynamicEntityServices)dynamicCreateService(typeof(DynamicEntityServices).FullName, false);
                entityService.CacheService = this.CacheService;
                string routePath = this.RoutePath;
                entityService.PreActionExtModelList = new List<DomainModel.ActionExt.ActionExtModel>();
                entityService.FinishActionExtModelList = new List<DomainModel.ActionExt.ActionExtModel>();
                entityService.ActionExtService = this.ActionExtService;
                entityService.RoutePath = "api/dynamicentity/add";
                DynamicEntityAddModel addModel = new DynamicEntityAddModel();
                List<string> needChangeKeys = new List<string>();
                addModel.TypeId = Guid.Parse(dstDetail["rectype"].ToString());
                foreach (string  key in dstDetail.Keys) {
                    object obj = dstDetail[key];
                    if (obj == null) continue;
                    if (obj is IDictionary<string, object>) {
                        needChangeKeys.Add(key);
                    }
                }
                foreach (string key in needChangeKeys) {

                    string tmp = Newtonsoft.Json.JsonConvert.SerializeObject(dstDetail[key]);
                    dstDetail[key] = tmp;
                }
                addModel.FieldData = dstDetail;
                addModel.ExtraData = new Dictionary<string, object>();
                addModel.WriteBackData = new List<Dictionary<string, object>>();
                //暂时不处理反写规则
                //if (transferRuleInfo.WriteBackRules != null && transferRuleInfo.WriteBackRules.Count > 0)
                //{
                //    #region 处理反写规则,每个写入规则包括:recid,entityid,fieldname,fieldvalue,isguid
                //    foreach (EntityTransferWriteBackRuleInfo writeBackInfo in transferRuleInfo.WriteBackRules)
                //    {
                //        Dictionary<string, object> writeBackItem = new Dictionary<string, object>();
                //        writeBackItem.Add("entityid", transferRuleInfo.SrcEntity);
                //        writeBackItem.Add("recid", model.SrcRecId);
                //        writeBackItem.Add("fieldname", writeBackInfo.WriteBackFieldName);
                //        writeBackItem.Add("fieldvalue", writeBackInfo.WriteBackValue);
                //        addModel.WriteBackData.Add(writeBackItem);
                //    }
                //    #endregion
                //}
                if (extraData != null)
                {
                    addModel.ExtraData = extraData;
                }
                
                OutputResult<object> addResult = entityService.Add(addModel, header, userNum);
                if (addResult.Status == 0)
                {
                    dstDetail = (Dictionary<string, object>)getBillData(transferRuleInfo.DstEntity, (string)addResult.DataBody,userNum);
                }
                else
                {
                    throw (new Exception(addResult.Message));
                }
                addResult = null;
            }
            #endregion
            Dictionary<string, IDictionary<string, object>> retData = new Dictionary<string, IDictionary<string, object>>();
            retData.Add("detail", dstDetail);
            #region 检查是否提示下一个实体转换
            if (transferRuleInfo.CheckNextTransfer != null && transferRuleInfo.CheckNextTransfer.Length > 0)
            {
                EntityTransferRuleQueryModel checkNextActionModel = new EntityTransferRuleQueryModel();
                checkNextActionModel.SrcEntityId = model.SrcEntityId;
                if (srcBillInfo.ContainsKey("rectype") && srcBillInfo["rectype"] != null)
                {
                    checkNextActionModel.SrcCategoryId = srcBillInfo["rectype"].ToString();
                }
                checkNextActionModel.DstEntityId = transferRuleInfo.CheckNextTransfer;
                List<EntityTransferRuleInfo> nextTransferRules = _entityTransferRepository.queryRules(checkNextActionModel, userNum, transaction);
                if (nextTransferRules != null && nextTransferRules.Count > 0)
                {
                    foreach (EntityTransferRuleInfo r in nextTransferRules)
                    {
                        r.TransferJson = "";
                    }
                    Dictionary<string, object> nextTransferItem = new Dictionary<string, object>();
                    nextTransferItem.Add("rules", nextTransferRules);
                    retData.Add("nexttransfer", nextTransferItem);
                }
            }
            #endregion 
            return retData;
        }
        public Dictionary<string, IDictionary<string, object>> TransClue2CustomerAndLink(AnalyseHeader header, EntityTransferActionModel model, int userNum, string userName)
        {
            string Customer_Entity_Id = "f9db9d79-e94b-4678-a5cc-aa6e281c1246";
            Dictionary<string, IDictionary<string, object>> retData = new Dictionary<string, IDictionary<string, object>>();
            IDictionary<string, object> detailData = null;

            #region 检查客户是否已经存在，如果已经存在，则直接获取

            IDictionary<string, object> srcBillInfo = getBillData(model.SrcEntityId, model.SrcRecId,userNum);
            string baseCustId = "";
            if (srcBillInfo == null) return retData;
            string companyname = "";
            if (srcBillInfo.ContainsKey("companyname") && srcBillInfo["companyname"] != null)
            {
                companyname = srcBillInfo["companyname"].ToString();
            }
            Dictionary<string, object> oldCustomerDatasource = _entityTransferRepository.getCustomerDataSourceByClue(model.SrcRecId, userNum, userName, null);

            if (oldCustomerDatasource != null)
            {
                detailData = getBillData(Customer_Entity_Id, oldCustomerDatasource["id"].ToString(),userNum);
            }
            #endregion
            #region 如果不存在，则调用通用转化方法进行线索转客户
            if (detailData == null)
            {
                baseCustId = _entityTransferRepository.getBaseCustomIdByName(companyname, userNum, null);
                Dictionary<string, object> extraData = new Dictionary<string, object>();
                if (!(baseCustId == null || baseCustId.Length == 0))
                {
                    extraData.Add("commonid", baseCustId);
                }
                Dictionary<string, IDictionary<string, object>> tmpDetail = CommonTransferBill(header, model, userNum, userName, extraData, null);
                if (tmpDetail != null && tmpDetail.ContainsKey("detail"))
                {
                    detailData = (Dictionary<string, object>)tmpDetail["detail"];
                }
                if (tmpDetail != null && tmpDetail.ContainsKey("nexttransfer"))
                {
                    retData.Add("nexttransfer", tmpDetail["nexttransfer"]);
                }
            }
            else
            {
                EntityTransferRuleInfo transferRuleInfo = _entityTransferRepository.getById(model.RuleId, userNum);
                if (transferRuleInfo.CheckNextTransfer != null && transferRuleInfo.CheckNextTransfer.Length > 0)
                {
                    EntityTransferRuleQueryModel checkNextActionModel = new EntityTransferRuleQueryModel();
                    checkNextActionModel.SrcEntityId = model.SrcEntityId;
                    if (srcBillInfo.ContainsKey("rectype") && srcBillInfo["rectype"] != null)
                    {
                        checkNextActionModel.SrcCategoryId = srcBillInfo["rectype"].ToString();
                    }
                    checkNextActionModel.DstEntityId = transferRuleInfo.CheckNextTransfer;
                    List<EntityTransferRuleInfo> nextTransferRules = _entityTransferRepository.queryRules(checkNextActionModel, userNum);
                    if (nextTransferRules != null && nextTransferRules.Count > 0)
                    {
                        foreach (EntityTransferRuleInfo r in nextTransferRules)
                        {
                            r.TransferJson = "";
                        }
                        Dictionary<string, object> nextTransferItem = new Dictionary<string, object>();
                        nextTransferItem.Add("rules", nextTransferRules);
                        retData.Add("nexttransfer", nextTransferItem);
                    }
                }
            }
            #endregion
            if (detailData == null)
            {
                throw (new Exception("转化异常"));
            }
            retData.Add("detail", detailData);

            if (detailData.ContainsKey("recid") == false) return retData;
            #region 开始增加联系人信息
            string LinkEntityID = "e450bfd7-ff17-4b29-a2db-7ddaf1e79342";
            //检查是否该客户下已经有相同的联系人了
            bool hasContact = false;
            if (srcBillInfo != null && srcBillInfo.ContainsKey("recphone") && srcBillInfo["recphone"] != null)
            {
                if (srcBillInfo["recphone"].ToString().Length > 0)
                {
                    hasContact = this._entityTransferRepository.CheckHasContact(detailData["recid"].ToString(), srcBillInfo["recphone"].ToString());

                }
            }
            if (hasContact == false)
            {
                EntityTransferRuleQueryModel clue2linkSearchModel = new EntityTransferRuleQueryModel();
                clue2linkSearchModel.SrcEntityId = model.SrcEntityId;
                clue2linkSearchModel.DstEntityId = LinkEntityID;
                clue2linkSearchModel.IsInner = 1;
                List<EntityTransferRuleInfo> tmps = _entityTransferRepository.queryRules(clue2linkSearchModel, userNum);
                if (tmps == null || tmps.Count == 0) return retData;
                EntityTransferRuleInfo ruleInfo = tmps[0];
                EntityTransferActionModel clue2linkRule = new EntityTransferActionModel()
                {
                    SrcEntityId = model.SrcEntityId,
                    SrcRecId = model.SrcRecId,
                    RuleId = ruleInfo.RecId.ToString()
                };
                try
                {

                    CommonTransferBill(header, clue2linkRule, userNum, userName, null, null);
                }
                catch (Exception ex)
                {
                }
            }

            #endregion
            return retData;
        }


        private void recalcName(EntityFieldProMapper dstField, Dictionary<string, object> dstDetail, int userNum)
        {
            switch ((EntityFieldControlType)dstField.ControlType)
            {
                case EntityFieldControlType.None:
                case EntityFieldControlType.AreaGroup:
                case EntityFieldControlType.TreeSingle:
                case EntityFieldControlType.TreeMulti:
                case EntityFieldControlType.TakePhoto:
                case EntityFieldControlType.FileAttach:
                case EntityFieldControlType.LinkeTable:
                case EntityFieldControlType.QuoteControl:
                case EntityFieldControlType.HeadPhoto:
                case EntityFieldControlType.PersonSelectMulti:
                case EntityFieldControlType.RecId:
                    break;
                case EntityFieldControlType.Text:
                case EntityFieldControlType.TipText:
                case EntityFieldControlType.SelectMulti:
                case EntityFieldControlType.TextArea:
                case EntityFieldControlType.PhoneNum:
                case EntityFieldControlType.EmailAddr:
                case EntityFieldControlType.Telephone:
                case EntityFieldControlType.Department:
                case EntityFieldControlType.PersonSelectSingle:
                case EntityFieldControlType.RecName:
                    break;
                case EntityFieldControlType.SelectSingle:
                    recalcDict(dstDetail, dstField, userNum);
                    break;
                case EntityFieldControlType.NumberInt:
                    break;
                case EntityFieldControlType.AreaRegion:
                    break;
                case EntityFieldControlType.RecAudits:
                    recalcRecAudit(dstDetail, dstField);
                    break;
                case EntityFieldControlType.RecStatus:
                    recalcRecStatus(dstDetail, dstField);
                    break;
                case EntityFieldControlType.NumberDecimal:
                    break;
                case EntityFieldControlType.TimeDate:
                    break;
                case EntityFieldControlType.TimeStamp:
                    break;
                case EntityFieldControlType.RecCreated:
                case EntityFieldControlType.RecUpdated:
                case EntityFieldControlType.RecOnlive:
                    break;
                case EntityFieldControlType.Address:
                case EntityFieldControlType.Location:
                    recalcAddress(dstDetail, dstField.FieldName.ToLower());
                    break;
                case EntityFieldControlType.DataSourceSingle:
                    recalcDataSourceSingle(dstDetail, dstField);
                    break;
                case EntityFieldControlType.DataSourceMulti:
                    break;
                case EntityFieldControlType.RecCreator:
                case EntityFieldControlType.RecUpdator:
                case EntityFieldControlType.RecManager:
                    break;
                case EntityFieldControlType.RecType:
                    recalcRecType(dstDetail, dstField, userNum);
                    break;
                case EntityFieldControlType.SalesStage:
                    break;
                case EntityFieldControlType.Product:
                    reCalcProduct(dstDetail, dstField, userNum);
                    break;
                case EntityFieldControlType.ProductSet:
                    break;
            }
        }

        private void reCalcProduct(Dictionary<string, object> dstDetail, EntityFieldProMapper dstField, int userNum)
        {
            try
            {
                string fieldName = dstField.FieldName.ToLower();
                if (dstDetail.ContainsKey(fieldName) == false) return;
                string productid = dstDetail[fieldName].ToString();
                if (productid == null || productid.Length == 0) return;
                string productname = _entityTransferRepository.getProductName(productid, userNum);
                string nameFieldName = fieldName + "_name";
                if (dstDetail.ContainsKey(nameFieldName))
                {
                    dstDetail.Remove(nameFieldName);
                }
                dstDetail.Add(nameFieldName, productname);
            }
            catch (Exception ex)
            {
            }

        }
        private void recalcDict(Dictionary<string, object> dstDetail, EntityFieldProMapper dstField, int userNum)
        {
            try
            {
                string fieldName = dstField.FieldName.ToLower();
                if (dstDetail.ContainsKey(fieldName) == false) return;
                int dictdataid = int.Parse(dstDetail[fieldName].ToString());
                if (dictdataid <= 0) return;
                if (dstField.FieldConfig == null || dstField.FieldConfig.Length == 0) return;
                IDictionary<string, object> configInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<IDictionary<string, object>>(dstField.FieldConfig);
                if (configInfo == null) return;
                if (configInfo.ContainsKey("dataSource") == false) return;
                if (configInfo["dataSource"] != null && configInfo["dataSource"] is IDictionary<string, object>)
                {
                    IDictionary<string, object> dataSourceInfo = (IDictionary<string, object>)configInfo["dataSource"];
                    if (dataSourceInfo.ContainsKey("sourceId"))
                    {
                        int sourceId = int.Parse(dataSourceInfo["sourceId"].ToString());
                        if (sourceId > 0)
                        {
                            string dictValue = _entityTransferRepository.getDictNameByDataId(dictdataid, sourceId, userNum);
                            string nameFieldName = fieldName + "_name";
                            if (dstDetail.ContainsKey(nameFieldName))
                            {
                                dstDetail.Remove(nameFieldName);
                            }
                            dstDetail.Add(nameFieldName, dictValue);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        private void recalcRecType(Dictionary<string, object> dstDetail, EntityFieldProMapper dstField, int userNum)
        {
            try
            {
                string fieldName = dstField.FieldName.ToLower();

                if (dstDetail.ContainsKey(fieldName) == false) return;
                string recType = dstDetail[fieldName].ToString();
                if (recType != null && recType.Length > 0)
                {
                    string recName = _entityTransferRepository.getCategoryName(recType, userNum);
                    if (recName != null && recName.Length > 0)
                    {
                        string nameFieldName = fieldName + "_name";
                        if (dstDetail.ContainsKey(nameFieldName))
                        {
                            dstDetail.Remove(nameFieldName);
                        }
                        dstDetail.Add(nameFieldName, recName);
                    }
                }


            }
            catch (Exception ex)
            {
            }
        }
        /// <summary>
        /// 重新计算单据状态
        /// </summary>
        /// <param name="dstDetail"></param>
        /// <param name="dstField"></param>
        private void recalcRecStatus(Dictionary<string, object> dstDetail, EntityFieldProMapper dstField)
        {
            try
            {
                string newNameValue = "";
                string fieldName = dstField.FieldName.ToLower();
                if (dstDetail.ContainsKey(fieldName))
                {
                    int newValue = int.Parse(dstDetail[fieldName].ToString());
                    switch (newValue)
                    {
                        case 0:
                            newNameValue = "停用";
                            break;
                        case 1:
                            newNameValue = "启用";
                            break;
                        case 2:
                            newNameValue = "删除";
                            break;
                        default:
                            break;
                    }
                    string nameFieldName = fieldName + "_name";
                    if (dstDetail.ContainsKey(nameFieldName))
                    {
                        dstDetail.Remove(nameFieldName);
                    }
                    dstDetail.Add(nameFieldName, newNameValue);
                }
            }
            catch (Exception ex)
            {
            }

        }
        /// <summary>
        /// 
        /// 重新计算recaudit的名字
        /// </summary>
        /// <param name="dstDetail"></param>
        /// <param name="dstField"></param>
        private void recalcRecAudit(Dictionary<string, object> dstDetail, EntityFieldProMapper dstField)
        {
            try
            {
                string newNameValue = "";
                string fieldName = dstField.FieldName.ToLower();
                if (dstDetail.ContainsKey(fieldName))
                {
                    int newValue = int.Parse(dstDetail[fieldName].ToString());
                    switch (newValue)
                    {
                        case 0:
                            newNameValue = "审批中";
                            break;
                        case 1:
                            newNameValue = "通过";
                            break;
                        case 2:
                            newNameValue = "不通过";
                            break;
                        case 3:
                            newNameValue = "发起审批";
                            break;
                        default:
                            break;
                    }
                    string nameFieldName = fieldName + "_name";
                    if (dstDetail.ContainsKey(nameFieldName))
                    {
                        dstDetail.Remove(nameFieldName);
                    }
                    dstDetail.Add(nameFieldName, newNameValue);
                }
            }
            catch (Exception ex)
            {
            }

        }
        /// <summary>
        /// 修复数据源数据
        /// </summary>
        /// <param name="dstDetail"></param>
        /// <param name="fieldName"></param>
        private void recalcDataSourceSingle(Dictionary<string, object> dstDetail, EntityFieldProMapper dstField)
        {
            string fieldName = dstField.FieldName.ToLower();
            if (dstDetail.ContainsKey(fieldName))
            {
                var itemValue = dstDetail[fieldName];
                if (itemValue is IDictionary<string, object>)
                {
                    IDictionary<string, object> nameValue = (IDictionary<string, object>)itemValue;
                    if (nameValue.ContainsKey("name"))
                    {
                        string nameFieldName = fieldName + "_name";
                        if (dstDetail.ContainsKey(nameFieldName))
                        {
                            dstDetail.Remove(nameFieldName);
                        }
                        dstDetail.Add(nameFieldName, nameValue["name"]);
                    }
                }
            }
        }
        /// <summary>
        /// 转换地址
        /// </summary>
        /// <param name="dstDetail"></param>
        /// <param name="fieldName"></param>
        private void recalcAddress(Dictionary<string, object> dstDetail, string fieldName)
        {
            if (dstDetail == null) return;
            if (dstDetail.ContainsKey(fieldName))
            {
                object obj = dstDetail[fieldName];
                if (obj == null) return;
                if (!(obj is IDictionary<string, object>)) return;
                IDictionary<string, object> address = (IDictionary<string, object>)obj;
                if (address == null) return;
                if (address.ContainsKey("address"))
                {
                    if (dstDetail.ContainsKey(fieldName + "_name"))
                    {
                        dstDetail.Remove(fieldName + "_name");
                    }
                    dstDetail.Add(fieldName + "_name", address["address"]);
                }
            }
        }
        private void ExecuteServiceTransferField(EntityFieldMapperInfo mapInfo, EntityFieldProMapper dstField,
                            IDictionary<string, object> srcBillInfo,
                            Dictionary<string, object> dstDetail, int index, int userNum, string userName, DbTransaction transaction = null)
        {
            //临时处理，后续整理s
            if (mapInfo.CommandText.Equals("this.getCustomIdByName"))
            {
                string CustName = srcBillInfo["companyname".ToLower()].ToString();
                Guid id = _entityTransferRepository.getCustomIdByName(CustName, 0, "", transaction);
                Dictionary<string, object> dyobjInfo = new Dictionary<string, object>();
                dyobjInfo.Add("id", id);
                dyobjInfo.Add("name", CustName);
                if (dstDetail.ContainsKey(dstField.FieldName.ToLower()))
                {
                    dstDetail.Remove(dstField.FieldName.ToLower());
                }
                dstDetail.Add(dstField.FieldName.ToLower(), dyobjInfo);
            }
            else if (mapInfo.CommandText.StartsWith("this.getCustomerDataSourceByClue"))
            {
                string paramText = mapInfo.CommandText.Substring("this.getCustomerDataSourceByClue".Length);
                paramText = paramText.Substring(2, paramText.Length - 4);
                object paramObject = getValueFromSourceBill(srcBillInfo, paramText, index);
                string paramValue = "";
                if (paramObject != null)
                {
                    paramValue = paramObject.ToString();
                    Dictionary<string, object> tmpDict = _entityTransferRepository.getCustomerDataSourceByClue(paramValue, userNum, userName, transaction);
                    if (tmpDict != null)
                    {
                        if (dstDetail.ContainsKey(dstField.FieldName.ToLower()))
                        {
                            dstDetail.Remove(dstField.FieldName.ToLower());
                        }
                        dstDetail.Add(dstField.FieldName.ToLower(), tmpDict);
                    }
                }
            }


        }
        private void transferField(EntityFieldMapperInfo mapInfo, EntityFieldProMapper dstField,
                            IDictionary<string, object> srcBillInfo,
                            Dictionary<string, object> dstDetail, int index, int userNum, string userName, DbTransaction transaction = null)
        {
            switch (mapInfo.CalcType)
            {
                case EntityFieldMapperType.ClearValue:
                    if (dstDetail.ContainsKey(dstField.FieldName.ToLower()))
                    {
                        dstDetail.Remove(dstField.FieldName.ToLower());
                    }
                    break;
                case EntityFieldMapperType.ExecuteFunc:
                    throw (new Exception("尚未支持的转换"));
                    break;
                case EntityFieldMapperType.ExecuteService:
                    ExecuteServiceTransferField(mapInfo, dstField, srcBillInfo, dstDetail, index, userNum, userName, transaction);
                    break;
                case EntityFieldMapperType.ExecuteSQL:
                    throw (new Exception("尚未支持的转换"));
                    break;
                case EntityFieldMapperType.MapperEqual:
                    var val = getValueFromSourceBill(srcBillInfo, mapInfo.SrcFieldName.ToLower(), index);
                    if (val != null)
                    {
                        if (dstDetail.ContainsKey(dstField.FieldName.ToLower()))
                        {
                            dstDetail.Remove(dstField.FieldName.ToLower());
                        }
                        dstDetail.Add(dstField.FieldName.ToLower(), val);
                    }
                    break;
                case EntityFieldMapperType.ParamReplace:
                    if (mapInfo.ResultIsDict)
                    {
                        string tmp = replaceParamText(mapInfo.ParamReplaceText, srcBillInfo, index);
                        Dictionary<string, object> tmpDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(tmp);
                        if (tmpDict != null)
                        {
                            if (dstDetail.ContainsKey(dstField.FieldName.ToLower()))
                            {
                                dstDetail.Remove(dstField.FieldName.ToLower());
                            }
                            dstDetail.Add(dstField.FieldName.ToLower(), tmpDict);
                        }
                    }
                    else
                    {
                        if (dstDetail.ContainsKey(dstField.FieldName.ToLower()))
                        {
                            dstDetail.Remove(dstField.FieldName.ToLower());
                        }
                        dstDetail.Add(dstField.FieldName.ToLower(), replaceParamText(mapInfo.ParamReplaceText, srcBillInfo, index));
                    }
                    break;

            }
        }
        private string replaceParamText(string text, IDictionary<string, object> srcBillInfo, int index)
        {
            if (text == null || text.Length == 0) return text;
            Regex regex = new Regex("#([^#]*)#");

            MatchCollection matches = regex.Matches(text);
            foreach (Match match in matches)
            {
                string fieldname = match.Groups[1].Value;
                if (fieldname.StartsWith("#")) fieldname = fieldname.Substring(1);
                if (fieldname.EndsWith("#")) fieldname = fieldname.Substring(0, fieldname.Length - 1);
                object obj = getValueFromSourceBill(srcBillInfo, fieldname, index);
                string val = "";
                if (obj == null) val = "";
                else val = obj.ToString();
                if (val == null) val = "";
                text = text.Replace("#" + fieldname + "#", val);
            }
            return text;
        }
        private object getValueFromSourceBill(IDictionary<string, object> srcBillInfo, string fieldName, int index)
        {
            string[] ancestors = fieldName.Split('.');
            if (ancestors.Length == 0) return null;
            if (srcBillInfo.ContainsKey(ancestors[0]) == false) return null;
            var tmpValue = srcBillInfo[ancestors[0]];
            if (ancestors.Length > 1)
            {
                if (tmpValue is IDictionary<string, object>)
                {
                    IDictionary<string, object> tmp = (IDictionary<string, object>)tmpValue;
                    if (tmp.ContainsKey(ancestors[1]))
                    {
                        return tmp[ancestors[1]];
                    }
                }
                else if (tmpValue is List<IDictionary<string, object>>)
                {
                    List<IDictionary<string, object>> tmp = (List<IDictionary<string, object>>)tmpValue;
                    if (tmp.Count >= index)
                    {
                        IDictionary<string, object> item = tmp[index];
                        if (item.ContainsKey(ancestors[1]))
                        {
                            return item[ancestors[1]];
                        }
                    }
                }
                return null;
            }
            else
            {
                return tmpValue;
            }

        }
        private void initDictData(EntityFieldProMapper dstField, Dictionary<string, object> dstDetail, string dstCategoryId, int userNum, string userName)
        {
            switch ((EntityFieldControlType)dstField.ControlType)
            {
                case EntityFieldControlType.None:
                case EntityFieldControlType.AreaGroup:
                case EntityFieldControlType.TreeSingle:
                case EntityFieldControlType.TreeMulti:
                case EntityFieldControlType.TakePhoto:
                case EntityFieldControlType.FileAttach:
                case EntityFieldControlType.LinkeTable:
                case EntityFieldControlType.QuoteControl:
                case EntityFieldControlType.HeadPhoto:
                    break;
                case EntityFieldControlType.PersonSelectMulti:
                    break;
                case EntityFieldControlType.RecId:
                    break;
                case EntityFieldControlType.Text:
                case EntityFieldControlType.TipText:
                case EntityFieldControlType.SelectMulti:
                case EntityFieldControlType.TextArea:
                case EntityFieldControlType.PhoneNum:
                case EntityFieldControlType.EmailAddr:
                case EntityFieldControlType.Telephone:
                case EntityFieldControlType.Department:
                    break;
                case EntityFieldControlType.PersonSelectSingle:
                    break;
                case EntityFieldControlType.RecName:
                    dstDetail.Add(dstField.FieldName.ToLower(), "");
                    break;
                case EntityFieldControlType.SelectSingle:
                    break;
                case EntityFieldControlType.NumberInt:
                case EntityFieldControlType.AreaRegion:
                case EntityFieldControlType.RecAudits:

                    dstDetail.Add(dstField.FieldName.ToLower(), 0);
                    break;
                case EntityFieldControlType.RecStatus:
                    dstDetail.Add(dstField.FieldName.ToLower(), 1);
                    break;
                case EntityFieldControlType.NumberDecimal:
                    dstDetail.Add(dstField.FieldName.ToLower(), 0.00);
                    break;
                case EntityFieldControlType.TimeDate:
                    //dstDetail.Add(dstField.FieldName.ToLower(), new DateTime());
                    break;
                case EntityFieldControlType.TimeStamp:
                    break;
                case EntityFieldControlType.RecCreated:
                case EntityFieldControlType.RecUpdated:
                case EntityFieldControlType.RecOnlive:
                    dstDetail.Add(dstField.FieldName.ToLower(), System.DateTime.Now);
                    break;
                case EntityFieldControlType.Address:
                case EntityFieldControlType.Location:
                case EntityFieldControlType.DataSourceSingle:
                case EntityFieldControlType.DataSourceMulti:
                    dstDetail.Add(dstField.FieldName.ToLower(), new Dictionary<string, object>());
                    break;
                case EntityFieldControlType.RecCreator:
                case EntityFieldControlType.RecUpdator:
                case EntityFieldControlType.RecManager:
                    dstDetail.Add(dstField.FieldName.ToLower(), userNum);
                    dstDetail.Add(dstField.FieldName.ToLower() + "_name", userName);
                    break;
                case EntityFieldControlType.RecType:
                    dstDetail.Add(dstField.FieldName.ToLower(), Guid.Parse(dstCategoryId));
                    break;
                case EntityFieldControlType.SalesStage:
                    break;
                case EntityFieldControlType.Product:
                    dstDetail.Add(dstField.FieldName.ToLower(), "");
                    break;
                case EntityFieldControlType.ProductSet:
                    dstDetail.Add(dstField.FieldName.ToLower(), "");
                    break;
            }
        }


        private Guid getCustomerByName(string custName, int userNum, string userName, DbTransaction transaction)
        {
            try
            {
                return _entityTransferRepository.getCustomIdByName(custName, userNum, userName, transaction);

            }
            catch (Exception ex)
            {
            }
            return Guid.Empty;
        }
        private IDictionary<string, object> getBillData(string entityid, string recid, int userNum,DbTransaction transaction = null)
        {
            try
            {
                DynamicEntityDetailtMapper modeltemp = new DynamicEntityDetailtMapper()
                {
                    EntityId = Guid.Parse(entityid),
                    RecId = Guid.Parse(recid),
                    NeedPower = 0
                };
                IDictionary<string, object> detail = _dynamicEntityRepository.Detail(modeltemp, userNum, transaction);
                return detail;
            }
            catch (Exception ex)
            {
            }
            return null;

        }
        private IDictionary<string, object> DealLinkTableFields(IDictionary<string, object> data, List<DynamicEntityFieldSearch> searchFields,
             int userNumber)
        {
            var linkTableFields = searchFields.Where(m => (DynamicProtocolControlType)m.ControlType == DynamicProtocolControlType.LinkeTable).ToList();
            foreach (var filed in linkTableFields)
            {
                var fieldConfig = JObject.Parse(filed.FieldConfig);
                var linketable_entityid = new Guid(fieldConfig["entityId"].ToString());


                if (data.ContainsKey(filed.FieldName))
                {
                    var linketableRecids = data[filed.FieldName] == null ? "" : data[filed.FieldName].ToString();
                    if (string.IsNullOrEmpty(linketableRecids))
                        continue;
                    DynamicEntityDetailtListMapper modeltemp = new DynamicEntityDetailtListMapper()
                    {
                        EntityId = linketable_entityid,
                        RecIds = linketableRecids,
                        NeedPower = 0
                    };

                    data[filed.FieldName] = _dynamicEntityRepository.DetailList(modeltemp, userNumber,null);
                }
            }

            return data;
        }
    }
}
