using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.QRCode;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Utility;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class QRCodeServices: EntityBaseServices
    {
        private readonly JavaScriptUtilsServices _javaScriptUtilsServices;
        private readonly IQRCodeRepository _iQRCodeRepository;
        public QRCodeServices(JavaScriptUtilsServices javaScriptUtilsServices,
            IQRCodeRepository iQRCodeRepository) {
            _javaScriptUtilsServices = javaScriptUtilsServices;
            _iQRCodeRepository = iQRCodeRepository;
        }
        public OutputResult<object> CheckQrCode(string code, int codetype, int userid) {
            List<QRCodeEntryItemInfo> checkList = new List<QRCodeEntryItemInfo>();
            QRCodeEntryItemInfo ite1m = new QRCodeEntryItemInfo() {
                CheckType = QRCodeCheckTypeEnum.JSPlugInSearch,
                CheckParam = new QRCodeCheckMatchParamInfo() {
                    UScriptParam = new QRCodeUScriptCheckMatchParamInfo() {
                        UScript = "if (JsParam.QRCode == 'abc') return true; else return false ;"
                    }
                },
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

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="recId"></param>
        /// <param name="recName"></param>
        /// <param name="remark"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool Edit(Guid recId, string recName, string remark, int userId)
        {
            QRCodeEntryItemInfo info = this._iQRCodeRepository.GetFullInfo(null, recId, userId);
            if (info == null) throw (new Exception("没有找到记录"));
            return this._iQRCodeRepository.Save(null, recId, recName, remark, userId);
        }

        public bool Delete(string recIds, int userId)
        {
            List<QRCodeEntryItemInfo> rules = this._iQRCodeRepository.ListRules(null, false, userId);
            List<Guid> needDeletedIds = new List<Guid>();
            string[] tmpids = recIds.Split(',');
            foreach (string id in tmpids)
            {
                needDeletedIds.Add(Guid.Parse(id));
            }
            foreach (Guid id in needDeletedIds) {
                bool isFound = false;
                foreach (QRCodeEntryItemInfo item in rules) {
                    if (item.RecId == id) {
                        isFound = true;
                        break;
                    }
                }
                if (isFound == false) throw (new Exception("数据已经发生变化，请刷新后再试"));
            }
            this._iQRCodeRepository.Delete(null, needDeletedIds, userId);
            return true;
        }

        public List<QRCodeEntryItemInfo> List(bool isShowDisabled, int userId)
        {
            return  this._iQRCodeRepository.ListRules(null, isShowDisabled, userId);
        }

        /// <summary>
        /// 新增一个规则，且此规则放在所有规则最后
        /// 通过排序，可以调整顺序，禁用后的规则不排序
        /// </summary>
        /// <param name="paramInfo"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Guid AddNew(QRCodeAddNewModel paramInfo, int userId)
        {
            return this._iQRCodeRepository.Add(null, paramInfo.RecName, paramInfo.Remark, userId);
        }

        public bool SetStatus(string recIds, int recStatus, int userId)
        {
            string[] items = recIds.Split(',');
            List<Guid> ids = new List<Guid>();
            foreach (string id in items) {
                ids.Add(Guid.Parse(id));
            }
            this._iQRCodeRepository.SetStatus(null, ids, recStatus, userId);//更新完毕后要重新排序
            List<QRCodeEntryItemInfo> list = this._iQRCodeRepository.ListRules(null, false, userId);
            List<Guid> activeIds = new List<Guid>();
            foreach (QRCodeEntryItemInfo info in list) {
                activeIds.Add(info.RecId);
            }
            this._iQRCodeRepository.OrderRules(null, activeIds, userId);
            return true;
         }

        public Dictionary<string,object> GetCheckMatchParam(Guid recId, int userId)
        {
            Dictionary<string, object> retDict = new Dictionary<string, object>();
            QRCodeEntryItemInfo item = this._iQRCodeRepository.GetFullInfo(null, recId, userId);
            if (item == null) throw (new Exception("没有找到记录"));
            retDict.Add("checktype", item.CheckType);
            QRCodeCheckMatchParamInfo param = item.CheckParam;
            if (param != null) {
                if (item.CheckType == QRCodeCheckTypeEnum.EntitySearch && param.EntityMatchParam != null ) {
                    if (param.EntityMatchParam.EntityId != null && param.EntityMatchParam.EntityId != Guid.Empty) {
                        //需要更新EntityName
                    }
                    if (param.EntityMatchParam.FieldId != null && param.EntityMatchParam.FieldId != Guid.Empty) {
                        //更新fieldname信息
                    }
                    if (param.EntityMatchParam.RuleId != null && param.EntityMatchParam.RuleId != Guid.Empty) {
                        //更新规则
                    }

                }
            }
            retDict.Add("checkparam", param);
            return retDict;     
        }

        public void OrderRule(string recIds, int userId)
        {
            List<QRCodeEntryItemInfo> currentItems = this._iQRCodeRepository.ListRules(null, false, userId);
            string[] tmpItems = recIds.Split(',');
            List<Guid> orderids = new List<Guid>();
            foreach (string item in tmpItems) {
                orderids.Add(Guid.Parse(item));
            }
            //检查多余或者遗漏
            foreach (QRCodeEntryItemInfo info in currentItems) {
                bool isFound = false;
                foreach (Guid id in orderids) {
                    if (info.RecId == id) {
                        isFound = true;
                        break;
                    }
                }
                if (isFound == false) {
                    throw (new Exception("排序数据已经过时，请刷新后再排序"));
                }
            }
            foreach (Guid id in orderids) {
                bool IsFound = false;
                foreach (QRCodeEntryItemInfo info in currentItems) {
                    if (info.RecId == id) {
                        IsFound = true;
                        break;
                    }
                }
                if (IsFound == false) throw (new Exception("排序数据已经过时，请刷新后再排序"));
            }
            this._iQRCodeRepository.OrderRules(null, orderids, userId);
        }

        public bool UpdateDealParam( Guid recId, QRCodeCheckTypeEnum dealType, QRCodeDealParamInfo dealParam, int userId)
        {
            QRCodeEntryItemInfo item = this._iQRCodeRepository.GetFullInfo(null, recId, userId);
            if (item == null) throw (new Exception("未找到需要更新的记录"));
            return this._iQRCodeRepository.UpdateDealParamInfo(null, recId, dealType, dealParam, userId);
        }

        public bool UpdateCheckParam(Guid recId, QRCodeCheckTypeEnum checkType, QRCodeCheckMatchParamInfo checkParam, int userId)
        {
            QRCodeEntryItemInfo item = this._iQRCodeRepository.GetFullInfo(null, recId, userId);
            if (item == null) throw (new Exception("未找到需要更新的记录"));
            return this._iQRCodeRepository.UpdateMatchParamInfo(null, recId, checkType, checkParam, userId);
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
                string jscode = "function ukejsengin_func_qrcode_" + tick + "(){" + item.CheckParam.UScriptParam.UScript + "};ukejsengin_func_qrcode_" + tick + "();";
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
                        fieldItem.FieldName='field1'；
                        fieldItem.Value=JsParam.Code;
                        fieldItem.IsNeedEdit = '0';
                        fieldItem.FieldType='1';
                        returnObj.DetailsInfo.push(fieldItem);
                        fieldItem = {};
                        fieldItem.FieldName='field2'；
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
    /// <summary>
    /// 规则新增的参数结构
    /// </summary>
    public class QRCodeAddNewModel {
        public string RecName { get; set; }
        public string Remark { get; set; }
    }
    /// <summary>
    /// 规则编辑的参数结构
    /// </summary>
    public class QRCodeEditModel {
        public Guid RecId { get; set; }
        public string RecName { get; set; }
        public string Remark { get; set; }
    }
    public class QRCodeListModel {
        public bool IsShowDisabled { get; set; }
    }
    public class QRCodeStatusModel {
        public string RecIds { get; set; }
        public int RecStatus { get; set; }
    }
    public class QRCodeOrderModel {
        public string RecIds { get; set; }
    }
    public class QRCodeDeleteModel
    {
        public string RecIds { get; set; }
    }
    public class QRCodeDetailModel {
        public Guid RecId { get; set; }
    }
    public class QRCodeUpdateMatchRuleModel {
        public Guid RecId { get; set; }
        public QRCodeCheckTypeEnum CheckType { get; set; }
        public QRCodeCheckMatchParamInfo CheckParam { get; set; }
    }
    public class QRCodeUpdateDealParamModel {
        public Guid RecId { get; set; }
        public QRCodeCheckTypeEnum DealType { get; set; }
        public QRCodeDealParamInfo DealParam { get; set; }
    }
}
