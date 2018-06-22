using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.QRCode;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Utility;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class QRCodeServices: EntityBaseServices
    {
        private readonly JavaScriptUtilsServices _javaScriptUtilsServices;
        public QRCodeServices(JavaScriptUtilsServices javaScriptUtilsServices) {
            _javaScriptUtilsServices = javaScriptUtilsServices;
        }
        public OutputResult<object> CheckQrCode(string code, int codetype, int userid) {
            List<QRCodeEntryItemInfo> checkList = new List<QRCodeEntryItemInfo>();
            QRCodeEntryItemInfo ite1m = new QRCodeEntryItemInfo() {
                CheckType = QRCodeCheckTypeEnum.JSPlugInSearch,
                CheckDetail = "if (JsParam.QRCode == 'abc') return true; else return false ;",
                DealType = QRCodeCheckTypeEnum.JSPlugInSearch
            };
            checkList.Add(ite1m);
            QRCodeEntryItemInfo matchedItem = null;
            foreach (QRCodeEntryItemInfo item in checkList) {
                if (CheckMatchItem(item, code, codetype, userid)) {
                    matchedItem = item;
                    break;
                }
            }
            if (matchedItem == null)
            {
                //返回不做任何处理的结果
                return new OutputResult<object>(QRCodeEntryResultInfo.NoActionResultInfo);
            }
            else {
                return new OutputResult<object>(RunQRCode(matchedItem, code, codetype, userid));
            }
        }
        private bool CheckMatchItem(QRCodeEntryItemInfo item, string code, int type, int userid) {
            if (item.CheckType == QRCodeCheckTypeEnum.StringSearch)
            {
                return false;
            }
            else if (item.CheckType == QRCodeCheckTypeEnum.RegexSearch)
            {
                return false;
            }
            else if (item.CheckType == QRCodeCheckTypeEnum.EntitySearch)
            {
                return false;
            }
            else if (item.CheckType == QRCodeCheckTypeEnum.InnerService)
            {
                return false;
            }
            else if (item.CheckType == QRCodeCheckTypeEnum.SQLFunction)
            {
                return false;
            }
            else if (item.CheckType == QRCodeCheckTypeEnum.SQLScript) {
                return false;
            }
            else if (item.CheckType == QRCodeCheckTypeEnum.JSPlugInSearch)
            {
                UKJSEngineUtils utils = new UKJSEngineUtils(this._javaScriptUtilsServices);
                Dictionary<string, object> param = new Dictionary<string, object>();
                param.Add("QRCode", code);
                param.Add("QRType", type);
                utils.SetHostedObject("JsParam", param);
                long tick = System.DateTime.Now.Ticks;
                string jscode = "function ukejsengin_func_qrcode_" + tick + "(){" + item.CheckDetail + "};ukejsengin_func_qrcode_" + tick + "();";
                try
                {
                    return utils.Evaluate<bool>(jscode);
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            return false;
        }

        private QRCodeEntryResultInfo RunQRCode(QRCodeEntryItemInfo item, string code, int type, int userid) {
            if (item.DealType == QRCodeCheckTypeEnum.SQLFunction)
            {
                return QRCodeEntryResultInfo.NoActionResultInfo;
            }
            else if (item.DealType == QRCodeCheckTypeEnum.SQLScript)
            {
                return QRCodeEntryResultInfo.NoActionResultInfo;
            }
            else if (item.DealType == QRCodeCheckTypeEnum.JSPlugInSearch)
            {
                return testJsReturn(item,code,type,userid);
            }
            else {
                return QRCodeEntryResultInfo.NoActionResultInfo;
            }
        }
        private QRCodeEntryResultInfo testJsReturn(QRCodeEntryItemInfo item, string code, int type, int userid) {
            string jscode = @"
                        var returnObj = {};
                        returnObj.ActionType = '1';
                        returnObj.IsSuccess = '1';
                        returnObj.ButtonCount = '2';
                        returnObj.Button1 = {};
                        returnObj.Button2 = {};
                        returnObj.DetailsInfo = [];
                        //开始处理字段信息
                        var fieldItem = {};
                        fieldItem.Title='扫描结果';
                        fieldItem.Value=JsParam.Code;
                        fieldItem.IsNeedEdit = '0';
                        fieldItem.FieldType='1';
                        returnObj.DetailsInfo.push(fieldItem);
                        fieldItem = {};
                        fieldItem.Title='操作结果';
                        fieldItem.Value='';
                        fieldItem.IsNeedEdit = '1';
                        fieldItem.FieldType='3';
                        fieldItem.SelectionList = {'1':'第一选项','2':'第二选项'};
                        returnObj.DetailsInfo.push(fieldItem);
                        returnObj.Button1 = {Title:'取消',ActionType:'1'};
                        returnObj.Button2 = {Title:'确认',ActionType:'3',ServiceUrl:'api/testapi'};
                        return returnObj;
                    ";
            UKJSEngineUtils utils = new UKJSEngineUtils(this._javaScriptUtilsServices);
            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("QRCode", code);
            param.Add("QRType", type);
            utils.SetHostedObject("JsParam", param);
            long tick = System.DateTime.Now.Ticks;
            string jscode_rel = "function ukejsengin_func_qrcode_" + tick + "(){" + jscode + "};ukejsengin_func_qrcode_" + tick + "();";
            try
            {
                object obj =  utils.Evaluate(jscode_rel);
                if (obj != null)
                {
                    Dictionary<string, object> newObj = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(
                        Newtonsoft.Json.JsonConvert.SerializeObject(obj)
                        );
                    if (newObj.ContainsKey("ActionType") && newObj["ActionType"] != null)
                    {
                        int actiontype = int.Parse(newObj["ActionType"].ToString());
                        switch ((QRCodeActionTypeEnum)actiontype) {
                            case QRCodeActionTypeEnum.NoAction:
                                return QRCodeEntryResultInfo.NoActionResultInfo;
                            case QRCodeActionTypeEnum.SimpleMsg:
                                return Newtonsoft.Json.JsonConvert.DeserializeObject<QRCodeSimpleMsgResultInfo>(
                                Newtonsoft.Json.JsonConvert.SerializeObject(obj)
                                );
                        }
                        return QRCodeEntryResultInfo.NoActionResultInfo;
                    }
                    else {
                        return QRCodeEntryResultInfo.NoActionResultInfo;
                    }
                }
                else {

                    return QRCodeEntryResultInfo.NoActionResultInfo;
                }
                obj = null;
                return QRCodeEntryResultInfo.NoActionResultInfo;
            }
            catch (Exception ex)
            {
                return QRCodeEntryResultInfo.NoActionResultInfo; 
            }
        }
    }
}
