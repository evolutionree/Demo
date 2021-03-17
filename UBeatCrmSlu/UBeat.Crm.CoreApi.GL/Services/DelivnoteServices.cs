using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Utility;
using UBeat.Crm.CoreApi.GL.Model;
using UBeat.Crm.CoreApi.GL.Repository;
using UBeat.Crm.CoreApi.GL.Utility;
using UBeat.Crm.CoreApi.Services.Services;

namespace UBeat.Crm.CoreApi.GL.Services
{
    public class DelivnoteServices: BasicBaseServices
    {
        private static readonly Logger logger = LogManager.GetLogger("UBeat.Crm.CoreApi.GL.Services.DelivnoteServices");
        private BaseDataServices _baseDataServices;
        private IDelivnoteRepository _delivnoteRepository;
        private readonly IBaseDataRepository _baseDataRepository;
        private readonly CoreApi.IRepository.IDynamicEntityRepository _dynamicEntityRepository;

        public DelivnoteServices(BaseDataServices baseDataServices, IDelivnoteRepository delivnoteRepository, IBaseDataRepository baseDataRepository,
            CoreApi.IRepository.IDynamicEntityRepository dynamicEntityRepository)
        {
            _baseDataServices = baseDataServices;
            _delivnoteRepository = delivnoteRepository;
            _baseDataRepository = baseDataRepository;
            _dynamicEntityRepository = dynamicEntityRepository;
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
            var entryStr = JsonConvert.SerializeObject(resultData["deliverydetail"]);
            var entryList = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(entryStr);
            foreach (var item in entryList)
            {
                var dic = new Dictionary<string, object>();
                dic.Add("VBELN", _delivnoteRepository.GetOrderNoByRecId(resultData["sourceorder"]?.ToString().Substring(8, 36)));
                dic.Add("POSNR", item["orderlineno"]);
                dic.Add("JHQTY", item["deliveryqty"]);
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
                _delivnoteRepository.UpdateDeliverySapCode(recId, sapCode, tran);
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
            postData.Add("VBELN_JHDH", resultData["code"]);
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

        public SynResultModel SynSapDelivNoteData(Guid caseId, int userId, string userName, DbTransaction tran)
        {
            var result = new SynResultModel();
            var recId = _delivnoteRepository.GetRecIdByCaseId(caseId, tran);
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
            if (sapRequest == null)
            {
                return result;
            }
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
                var entryList = dataList.LIPS.Where(r => r["VBELN_JHDH"].ToString() == item["VBELN_JHDH"].ToString() && string.IsNullOrEmpty(r["CHARG"].ToString())).ToList();
                int index = 1;
                Dictionary<string, object> orderInfo = null;
                foreach (var entry in entryList)
                {
                    Dictionary<string, object> dic = new Dictionary<string, object>();
                    Dictionary<string, object> parentDic = new Dictionary<string, object>();
                    if (orderInfo == null)
                    {
                        //找对应订单及客户、销售部门、销售区域
                        orderInfo = _delivnoteRepository.GetOrderInfo(entry["VBELN_SO"].ToString());
                        if (orderInfo == null)
                            continue;
                    }
                    #region MyRegion
                    var material = entry["MATNR"].ToString().TrimStart('0');
                    var product = _delivnoteRepository.GetCrmProduct(material);
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

                        var mainId = _delivnoteRepository.IsExistsDelivnote(item["VBELN_JHDH"].ToString());
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


        public OperateResult InitDelivnoteData()
        {
            var total = 0;
            DateTime startDate = new DateTime(2018, 9, 30, 0, 0, 0);
            while (startDate <= DateTime.Now.Date)
            {
                Sync2CRMInfo param = new Sync2CRMInfo();
                param.ERDAT_FR = startDate.ToString("yyyy-MM-dd");
                param.ERDAT_TO = startDate.ToString("yyyy-MM-dd");
                var c = this.SyncDelivnote2CRM(param);
                //if (c.Result)
                //{
                //    total += int.Parse(c.DataBody.ToString());
                //}
                startDate = startDate.AddDays(1);
            }
            return new OperateResult
            {
                Flag = 1,
                Msg = string.Format(@"SAP交货单已同步")
            };

        }

        public OperateResult IncremDelivnoteData()
        {
            var total = 0;
            Sync2CRMInfo param = new Sync2CRMInfo();
            param.REQDate = DateTime.Now.ToString("yyyy-MM-dd");
            var c = this.SyncDelivnote2CRM(param);
            //if (c.Status == 0)
            //{
            //    total += int.Parse(c.DataBody.ToString());
            //}
            return new OperateResult
            {
                Flag = 1,
                Msg = string.Format(@"SAP交货单已同步")
            };

        }
    }
}
