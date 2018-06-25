using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.TransferScheme;
using UBeat.Crm.CoreApi.DomainModel.TransferScheme;
using System.Data.Common;
using UBeat.Crm.CoreApi.Services.Models.DynamicEntity;
using Newtonsoft.Json;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class TransferSchemeServices : EntityBaseServices
    {
        private readonly ITransferSchemeRepository _transferSchemeRepository;
        private readonly IEntityProRepository _entityProRepository;
        public TransferSchemeServices(ITransferSchemeRepository transferSchemeRepository, IEntityProRepository entityProRepository)
        {
            _transferSchemeRepository = transferSchemeRepository;
            _entityProRepository = entityProRepository;
        }

        public OutputResult<object> SaveTransferScheme(TransferSchemeParam data, int userNumber)
        {
            DbTransaction tran = null;
            bool flag, isAdd;
            isAdd = data.TransSchemeId == Guid.Empty;
            if (isAdd)
                data.TransSchemeId = Guid.NewGuid();

            var model = new TransferSchemeModel
            {
                RecId = data.TransSchemeId,
                RecName = data.TransSchemeName,
                EntityId = data.TargetTransferId,
                Association = data.AssociationTransfer,
                RecCreator = userNumber,
                RecStatus = 1,
                RecCreated = DateTime.Now,
                Remark = data.Remark,
                FieldId = data.FieldId
            };

            if (isAdd)
                flag = _transferSchemeRepository.AddTransferScheme(model, tran);
            else
                flag = _transferSchemeRepository.UpdateTransferScheme(model, tran);

            if (flag)
                return new OutputResult<object>(null, "操作成功");
            else
                return new OutputResult<object>(null, "操作失败", 1);
        }

        public OutputResult<TransferSchemeModel> GetTransferScheme(Guid TransSchemeId, int userNumber)
        {
            DbTransaction tran = null;
            var data = _transferSchemeRepository.GetTransferScheme(TransSchemeId, tran, userNumber);
            return new OutputResult<TransferSchemeModel>(data);

        }

        public OutputResult<object> SetTransferSchemeStatus(List<Guid> list, int status, int userNumber)
        {
            DbTransaction tran = null;
            var flag = _transferSchemeRepository.SetTransferSchemeStatus(list, status, tran, userNumber);
            if (flag)
                return new OutputResult<object>(null, "操作成功");
            else
                return new OutputResult<object>(null, "操作失败", 1);
        }

        public OutputResult<object> TransferSchemeList(ListModel model, int userNumber)
        {
            var data = _transferSchemeRepository.TransferSchemeList(model.RecStatus, model.SearchName, userNumber);
            return new OutputResult<object>(data);
        }

        public OutputResult<object> ListTransferSchemesByEntity(Guid entityId, int userId)
        {
            try
            {
                List<SearchEntitySchemeResultInfo> result = new List<SearchEntitySchemeResultInfo>();
                List<SearchEntitySchemeResultInfo> FieldsResult = new List<SearchEntitySchemeResultInfo>();
                Dictionary<string, List<IDictionary<string, object>>> alllFieldsResult = this._entityProRepository.EntityFieldProQuery(entityId.ToString(), userId);
                if (alllFieldsResult == null || alllFieldsResult.ContainsKey("EntityFieldPros") == false || alllFieldsResult["EntityFieldPros"] == null)
                    throw (new Exception("获取字段信息失败"));
                List<IDictionary<string, object>> allFields = alllFieldsResult["EntityFieldPros"];
                foreach (IDictionary<string, object> f in allFields) {
                    int controltype = int.Parse(f["controltype"].ToString());
                    if (controltype == (int)DynamicProtocolControlType.RecManager) {
                        SearchEntitySchemeResultInfo item = new SearchEntitySchemeResultInfo()
                        {
                            FieldName = f["displayname"].ToString(),
                            FieldId = Guid.Parse(f["fieldid"].ToString())
                        };
                        FieldsResult.Add(item);
                    } else if (   controltype == (int)DynamicProtocolControlType.PersonSelectSingle) {
                        Dictionary<string, string> fieldConfig = JsonConvert.DeserializeObject<Dictionary<string,string>>(f["fieldconfig"].ToString());
                        //if (fieldConfig.ContainsKey("multiple") && fieldConfig["multiple"] != null) {//现在改为多选也生效
                        //    if (fieldConfig["multiple"].ToString().Equals("1")) {
                        //        continue;
                        //    }
                        //}
                        SearchEntitySchemeResultInfo item = new SearchEntitySchemeResultInfo()
                        {
                            FieldName = f["displayname"].ToString(),
                            FieldId = Guid.Parse(f["fieldid"].ToString())
                        };
                        FieldsResult.Add(item);
                    }
                }

                List < Dictionary<string, object> > schemes =  this._transferSchemeRepository.ListSchemeByEntity(null, entityId, userId);
                foreach (Dictionary<string, object> newItem in schemes) {
                    if (newItem.ContainsKey("fieldid") == false || newItem["fieldid"] == null) continue;
                    Guid fieldid = Guid.Empty;
                    if (Guid.TryParse(newItem["fieldid"].ToString(), out fieldid) == false) continue;
                    foreach (SearchEntitySchemeResultInfo fieldItem in FieldsResult) {
                        if (fieldItem.FieldId == fieldid) {
                            SearchEntitySchemeResultInfo resultItem = new SearchEntitySchemeResultInfo()
                            {
                                FieldId = fieldid,
                                FieldName = fieldItem.FieldName,
                                SchemeId = Guid.Parse(newItem["recid"].ToString()),
                                SchemeName = newItem["recname"].ToString()
                            };
                            result.Add(resultItem);
                            break;
                        }
                    }
                }
                //补充空白的字段（没有转移方案的字段）
                foreach (SearchEntitySchemeResultInfo fieldItem in FieldsResult) {
                    bool isFound = false;
                    foreach (SearchEntitySchemeResultInfo nowItem in result) {
                        if (nowItem.FieldId == fieldItem.FieldId) {
                            isFound = true;
                            break;
                        }
                    }
                    if (!isFound) {
                        result.Add(fieldItem);
                    }

                }
                return new OutputResult<object>(result);
            }
            catch (Exception ex) {
                return new OutputResult<object>(null, ex.Message, -1);
            }
        }
    }
}
