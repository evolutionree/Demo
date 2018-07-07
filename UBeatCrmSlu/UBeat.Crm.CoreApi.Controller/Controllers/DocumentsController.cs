using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.CoreApi.Services.Models.Documents;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.DomainModel.Documents;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class DocumentsController : BaseController
    {
        private readonly ILogger<DocumentsController> _logger;

        private readonly DocumentsServices _documentsService;


        public DocumentsController(ILogger<DocumentsController> logger, DocumentsServices service) : base(service)
        {
            _logger = logger;
            _documentsService = service;
        }



        #region --添加一个文档-- POST: /adddocument 

        [HttpPost("adddocument")]
        public OutputResult<object> InsertDocument([FromBody] AddOneDocModel body)
        {
            if (body == null)
                return ResponseError<object>("参数错误");

            var result = _documentsService.InsertDocument(body, UserId);
            if (result.Status == 0)
            {
                WriteOperateLog("添加一个文档", body);
            }
            return result;
        }

        #endregion


        #region --查询文档列表-- POST: /documentlist 

        [HttpPost("documentlist")]
        public OutputResult<object> DocumentList([FromBody] DocumentListRequest body)
        {
            if (body == null)
                return ResponseError<object>("参数错误");

            return _documentsService.DocumentList(body, UserId, GetAnalyseHeader());
        }
        #endregion



        #region --移动一个文档-- POST: /movedocument 

        [HttpPost("movedocument")]
        public OutputResult<object> MoveDocument([FromBody] MoveDocumentRequest body)
        {
            if (body == null)
                return ResponseError<object>("参数错误");
            WriteOperateLog("移动一个文档", body);
            return _documentsService.MoveDocument(body, UserId);
        }
        #endregion

        #region --批量移动文档-- POST: /movedocument 

        [HttpPost("movedocumentlist")]
        public OutputResult<object> MoveDocumentList([FromBody] MoveDocumentsRequest body)
        {
            if (body == null)
                return ResponseError<object>("参数错误");
            WriteOperateLog("批量移动文档", body);
            return _documentsService.MoveDocumentList(body, UserId);
        }
        #endregion


        #region --删除一个文档-- POST: /deletedocument 

        [HttpPost("deletedocument")]
        public OutputResult<object> DeleteDocument([FromBody] DeleteDocumentModel body)
        {
            if (body == null || body.DocumentId == null)
                return ResponseError<object>("参数错误");
            Guid documentid = Guid.Empty;
            if (body.DocumentId == null || !Guid.TryParse(body.DocumentId, out documentid))
            {
                return ResponseError<object>("参数错误");
            }
            var result = _documentsService.DeleteDocument(new List<Guid>() { documentid }, UserId);
            if (result.Status == 0)
            {
                WriteOperateLog("删除一个文档", body);
            }
            return result;
        }
        #endregion

        #region --批量删除文档-- POST: /deletedocumentlist 

        [HttpPost("deletedocumentlist")]
        public OutputResult<object> DeleteDocumentList([FromBody] DeleteDocumentListModel body)
        {
            if (body == null || body.DocumentIds == null || body.DocumentIds.Count == 0)
                return ResponseError<object>("参数错误");
            List<Guid> documentids = new List<Guid>();
            foreach (var tem in body.DocumentIds)
            {
                documentids.Add(new Guid(tem));
            }
            var result = _documentsService.DeleteDocument(documentids, UserId);
            if (result.Status == 0)
            {
                WriteOperateLog("批量删除文档", body);
            }
            return result;
        }
        #endregion


        #region --下载一个文档-- GET: /downloaddocument 
        [HttpGet("downloaddocument")]
        public IActionResult DownloadDocument([FromQuery]DownloadDocRequest queryParam)
        {
            if (queryParam == null || queryParam.DocumentId == null || queryParam.Userid <= 0)
                return ResponseError("参数错误");
            var result = _documentsService.DownloadDocument(queryParam.DocumentId, queryParam.Userid);

            return LocalRedirect(string.Format("/api/fileservice/download{0}", Request.QueryString.ToUriComponent()));
        }
        /// <summary>
        /// 只更新下载次数
        /// </summary>
        /// <param name="queryParam"></param>
        /// <returns></returns>
        [HttpPost("updatedownloadcount")]
        public OutputResult<object> DownloadDocumentPost([FromBody]DownloadCountRequest queryParam)
        {
            if (queryParam == null || queryParam.DocumentId == null)
                return ResponseError<object>("参数错误");
            return _documentsService.DownloadDocument(queryParam.DocumentId, UserId);
        }

        #endregion


        #region --添加一个文档目录-- POST: /addfolder 

        [HttpPost("addfolder")]
        public OutputResult<object> AddDocumentFolder([FromBody]AddDocFolderModel body)
        {
            if (body == null)
                return ResponseError<object>("参数错误");

            var result = _documentsService.InsertDocumentFolder(body, UserId);
            if (result.Status == 0)
            {
                WriteOperateLog("添加一个文档目录", body);
            }
            return result;
        }
        #endregion

        #region --编辑一个文档目录-- POST: /updatefolder 

        [HttpPost("updatefolder")]
        public OutputResult<object> UpdateDocumentFolder([FromBody]UpdateDocFolderModel body)
        {

            if (body == null || (body.FolderName == null && body.PfolderId == null))
                return ResponseError<object>("参数错误");

            var result = _documentsService.UpdateDocumentFolder(body, UserId);
            if (result.Status == 0)
            {
                WriteOperateLog("重命名一个文档目录", new { body.FolderId, body.FolderName });
            }
            return result;
        }
        #endregion

        #region --查询文档目录列表-- POST: /folderlist 

        [HttpPost("folderlist")]
        public OutputResult<object> DocumentFolderList([FromBody]DocFolderListModel body)
        {
            if (body == null)
                return ResponseError<object>("参数错误");
            return _documentsService.DocumentFolderList(body, UserId, GetAnalyseHeader());
        }
        #endregion

        #region --删除一个文档目录-- POST: /deletefolder 

        [HttpPost("deletefolder")]
        public OutputResult<object> DeleteDocumentFolder([FromBody]DeleteDocFolderModel body)
        {
            if (body == null || body.FolderId == null)
                return ResponseError<object>("参数错误");

            var result = _documentsService.DeleteDocumentFolder(body.FolderId, UserId);
            if (result.Status == 0)
            {
                WriteOperateLog("删除一个文档目录", body);
            }
            return result;
        }
        #endregion




    }

}
