using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Documents;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.Documents;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class DocumentsServices : BasicBaseServices
    {

        IDocumentsRepository _repository;
        private readonly Dictionary<DocumentType, string> staticEntityIdDic = null;

        public DocumentsServices(IDocumentsRepository repository)
        {
            _repository = repository;
            staticEntityIdDic = new Dictionary<DocumentType, string>();
            staticEntityIdDic.Add(DocumentType.Knowledge, "a3500e78-fe1c-11e6-aee4-005056ae7f49");//知识库专属ID，固定值，不可修改
        }

        #region --新增文档--
        public OutputResult<object> InsertDocument(AddOneDocModel data, int userNumber)
        {
            if (data.DocumentType != DocumentType.Entity && !staticEntityIdDic.ContainsKey(data.DocumentType))
            {
                return ShowError<object>("DocumentType参数错误");
            }
            //已上传成功，插入业务库记录
            DocumentInsert crmData = new DocumentInsert()
            {
                EntityId = data.DocumentType == DocumentType.Entity ? data.EntityId : staticEntityIdDic[data.DocumentType],
                BusinessId = data.BusinessId ?? "",
                FileId = data.FileId,
                FileName = data.FileName,
                FolderId = data.FolderId,
                FileLength = data.FileLength
            };
            if (!crmData.IsValid())
            {
                return HandleValid(crmData);
            }
            Guid entityId;
            if (!Guid.TryParse(crmData.EntityId, out entityId))
            {
                throw new Exception("业务实体ID格式错误");
            }
            return ExcuteAction((transaction, arg, userData) =>
            {
                //如果不是知识库，则为实体文档，需要判断业务数据权限
                if (crmData.EntityId != staticEntityIdDic[DocumentType.Knowledge])
                {
                    //判断某个实体的业务是否有权限新增文档，此处是判断实体业务表的id
                    if (!userData.HasDataAccess(transaction, RoutePath, entityId, DeviceClassic, new List<Guid>() { new Guid(crmData.BusinessId) }))
                    {
                        throw new Exception("您没有权限新增文档");
                    }
                }
                

                var id = _repository.InsertDocument(transaction, crmData, userNumber);
                if (!id.HasValue)
                {
                    throw new Exception("新增文档失败");
                }
                return new OutputResult<object>(id);
            }, data, userNumber);
        }
        #endregion


        public OutputResult<object> DocumentList(DocumentListRequest data, int userNumber, AnalyseHeader header)
        {

            if (data.DocumentType != DocumentType.Entity && !staticEntityIdDic.ContainsKey(data.DocumentType))
            {
                return ShowError<object>("DocumentType参数错误");
            }
            var pageParam = new PageParam { PageIndex = data.PageIndex, PageSize = data.PageSize };
            if (data.DataCategory == 0 && !pageParam.IsValid())
            {
                return HandleValid(pageParam);
            }
            bool isWebRequest = (header != null && header.Device != null) ? header.Device.ToLower().Equals("web") : true;
            DocumentList crmData = new DocumentList()
            {
                EntityId = data.DocumentType == DocumentType.Entity ? data.EntityId : staticEntityIdDic[data.DocumentType],
                BusinessId = data.BusinessId ?? "",
                FolderId = data.FolderId ?? "",
                IsAllDocuments = data.DataCategory != 0,
                RecStatus = isWebRequest ? 1 : -1,
                RecVersion = data.RecVersion,
                FolderRecVersion = data.FolderRecVersion,
                SearchKey = data.SearchKey
            };
            if (!crmData.IsValid())
            {
                return HandleValid(crmData);
            }
            Guid entityId;
            if (!Guid.TryParse(crmData.EntityId, out entityId))
            {
                throw new Exception("业务实体ID格式错误");
            }
            return ExcuteAction((transaction, arg, userData) =>
            {
                var ruleSql = userData.RuleSqlFormat(RoutePath, entityId, DeviceClassic);

                //获取文档和目录
                if (data.DataCategory == 2)
                {
                    return new OutputResult<object>(_repository.GetAllDocumentList(transaction, ruleSql, crmData, userNumber));
                }
                //仅仅获取文档数据
                else return new OutputResult<object>(_repository.DocumentList(transaction, ruleSql, pageParam, crmData, userNumber));

            }, data, userNumber);


        }

        #region --移动一个文档--
        public OutputResult<object> MoveDocument(MoveDocumentRequest data, int userNumber)
        {
            DocumentMove crmData = new DocumentMove()
            {
                DocumentId = data.DocumentId,
                FolderId = data.FolderId ?? ""
            };
            if (!crmData.IsValid())
            {
                return HandleValid(crmData);
            }

            var document = _repository.GetDocumentInfo(new Guid(crmData.DocumentId));

            return ExcuteAction((transaction, arg, userData) =>
            {
                //如果不是知识库，则为实体文档，需要判断业务数据权限
                if (document.EntityId.ToString() != staticEntityIdDic[DocumentType.Knowledge])
                {
                    //判断某个实体的业务是否有权限新增文档，此处是判断实体业务表的id
                    if (!userData.HasDataAccess(transaction, RoutePath, document.EntityId, DeviceClassic, new List<Guid>() { document.BusinessId }))
                    {
                        throw new Exception("您没有权限移动文档");
                    }
                }
                if (!_repository.MoveDocument(transaction, crmData, userNumber))
                {
                    throw new Exception("移动文档失败");
                }
                return new OutputResult<object>(crmData.DocumentId);

            }, data, userNumber);

        }
        #endregion

        #region --批量移动文档--
        public OutputResult<object> MoveDocumentList(MoveDocumentsRequest data, int userNumber)
        {
            if (data.DocumentIdList == null || data.DocumentIdList.Count == 0)
            {
                return ShowError<object>("DocumentIdList不可为空");
            }

            return ExcuteAction((transaction, arg, userData) =>
            {
                Guid folderid = new Guid(data.FolderId);
                List<Guid> documentids = new List<Guid>();

                foreach (var m in data.DocumentIdList)
                {
                    Guid documentId = new Guid(m);
                    documentids.Add(documentId);

                    //逐个检查数据权限
                    var document = _repository.GetDocumentInfo(documentId);
                    //如果不是知识库，则为实体文档，需要判断业务数据权限
                    if (document.EntityId.ToString() != staticEntityIdDic[DocumentType.Knowledge])
                    {
                        //判断某个实体的业务是否有权限新增文档，此处是判断实体业务表的id
                        if (!userData.HasDataAccess(transaction, RoutePath, document.EntityId, DeviceClassic, new List<Guid>() { document.BusinessId }))
                        {
                            throw new Exception("您没有权限移动文档");
                        }
                    }
                }
                if (!_repository.MoveDocumentList(transaction, folderid, documentids, userNumber))
                {
                    throw new Exception("移动文档失败");
                }
                return new OutputResult<object>();

            }, data, userNumber);
        }

        #endregion

        #region --删除文档--
        public OutputResult<object> DeleteDocument(List<Guid> documentids, int userNumber)
        {
            if (documentids == null || documentids.Count == 0)
                return ShowError<object>("documentid不可为空");

            return ExcuteAction((transaction, arg, userData) =>
            {
                foreach (var documentidtemp in documentids)
                {
                    var document = _repository.GetDocumentInfo(documentidtemp);
                    //如果不是知识库，则为实体文档，需要判断业务数据权限
                    if (document.EntityId.ToString() != staticEntityIdDic[DocumentType.Knowledge])
                    {
                        //判断某个实体的业务是否有权限新增文档，此处是判断实体业务表的id
                        if (!userData.HasDataAccess(transaction, RoutePath, document.EntityId, DeviceClassic, new List<Guid>() { document.BusinessId }))
                        {
                            throw new Exception("您没有权限删除文档");
                        }
                    }
                }
                if (!_repository.DeleteDocument(transaction, documentids, userNumber))
                {
                    throw new Exception("删除文档失败");
                }
                return new OutputResult<object>();
            }, documentids, userNumber);

        }
        #endregion

        public OutputResult<object> DownloadDocument(string documentid, int userNumber)
        {
            if (string.IsNullOrEmpty(documentid))
                return ShowError<object>("documentid不可为空");
            return HandleResult(_repository.DownloadDocument(documentid, userNumber));
        }

        public OutputResult<object> InsertDocumentFolder(AddDocFolderModel data, int userNumber)
        {

            if (data.DocumentType != DocumentType.Entity && !staticEntityIdDic.ContainsKey(data.DocumentType))
            {
                return ShowError<object>("DocumentType参数错误");
            }
            DocumentFolderInsert crmData = new DocumentFolderInsert()
            {
                EntityId = data.DocumentType == DocumentType.Entity ? data.EntityId : staticEntityIdDic[data.DocumentType],
                Foldername = data.FolderName,
                Pfolderid = data.PfolderId,
                IsAllVisible = data.IsAllVisible == 1 ? 1 : 0,
            };
            if (crmData.IsAllVisible == 0 && !string.IsNullOrEmpty(data.PermissionIds))
            {
                crmData.PermissionIds = data.PermissionIds.Split(',').ToList();
            }
            if (!crmData.IsValid())
            {
                return HandleValid(crmData);
            }

            Guid entityId;
            if (!Guid.TryParse(crmData.EntityId, out entityId))
            {
                throw new Exception("业务实体ID格式错误");
            }
            return ExcuteAction((transaction, arg, userData) =>
            {
                return HandleResult(_repository.InsertDocumentFolder(transaction, crmData, userNumber));
            }, data, userNumber);


        }

        public OutputResult<object> UpdateDocumentFolder(UpdateDocFolderModel data, int userNumber)
        {
            DocumentFolderUpdate crmdata = new DocumentFolderUpdate()
            {
                FolderName = data.FolderName ?? "",
                PfolderId = data.PfolderId ?? "",
                FolderId = data.FolderId,
                IsAllVisible = data.IsAllVisible == 1 ? 1 : 0,

            };
            if (crmdata.IsAllVisible == 0 && !string.IsNullOrEmpty(data.PermissionIds))
            {
                crmdata.PermissionIds = data.PermissionIds.Split(',').ToList();
            }

            if (string.IsNullOrEmpty(crmdata.FolderId))
                return ShowError<object>("FolderId不可为空");
            if (string.IsNullOrEmpty(crmdata.FolderName) && string.IsNullOrEmpty(crmdata.PfolderId))
                return ShowError<object>("更新的内容不可为空");

            if (!crmdata.IsValid())
            {
                return HandleValid(crmdata);
            }
            return ExcuteAction((transaction, arg, userData) =>
            {
                return HandleResult(_repository.UpdateDocumentFolder(transaction, crmdata, userNumber));

            }, data, userNumber);


        }

        public OutputResult<object> DocumentFolderList(DocFolderListModel data, int userNumber, AnalyseHeader header)
        {
            if (data.DocumentType != DocumentType.Entity && !staticEntityIdDic.ContainsKey(data.DocumentType))
            {
                return ShowError<object>("DocumentType参数错误");
            }
            bool isWebRequest = (header != null && header.Device != null) ? header.Device.ToLower().Equals("web") : true;

            DocumentFolderList crmdata = new DocumentFolderList()
            {
                EntityId = data.DocumentType == DocumentType.Entity ? data.EntityId : staticEntityIdDic[data.DocumentType],
                FolderId = data == null ? "" : data.FolderId,
                RecStatus = isWebRequest ? 1 : -1,
                RecVersion = data.RecVersion,
                Servicetype = isWebRequest ? 0 : 1
            };
            Guid entityId;
            if (!Guid.TryParse(crmdata.EntityId, out entityId))
            {
                throw new Exception("业务实体ID格式错误");
            }
            return ExcuteAction((transaction, arg, userData) =>
            {
                var ruleSql = userData.RuleSqlFormat(RoutePath, entityId, DeviceClassic);

                return new OutputResult<object>(_repository.DocumentFolderList(transaction, ruleSql, crmdata, userNumber));

            }, data, userNumber);

        }

        public OutputResult<object> DeleteDocumentFolder(string folderid, int userNumber)
        {
            if (string.IsNullOrEmpty(folderid))
                return ShowError<object>("folderid不可为空");
            return ExcuteAction((transaction, arg, userData) =>
            {
                return HandleResult(_repository.DeleteDocumentFolder(transaction, folderid, userNumber));

            }, folderid, userNumber);

        }
    }
}
