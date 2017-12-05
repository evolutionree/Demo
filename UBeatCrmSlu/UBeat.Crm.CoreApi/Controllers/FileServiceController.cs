using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UBeat.Crm.CoreApi.Services.Services;
using Microsoft.AspNetCore.Authorization;
using UBeat.Crm.CoreApi.Services.Models.FileService;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Core.Utility;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Net.Http.Headers;
using UBeat.Crm.CoreApi.Services.Utility;
using System.Text;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class FileServiceController : BaseController
    {
        private readonly ILogger<FileServiceController> _logger;

        private readonly FileServices _fileService;

        public FileServiceController(ILogger<FileServiceController> logger, FileServices fileService) : base(fileService)
        {
            _logger = logger;
            _fileService = fileService;
        }

        protected void WriteLog(LogLevel level, Exception exception, string message = null)
        {
            if (_logger != null)
            {
                if (message == null)
                    message = exception.Message;
                _logger.Log(level, 0, message, exception, (m, e) => m);
            }
        }

        #region --读取文件数据-- GET: /read?entityid=asdfadfdddd&fileid=sddffdad&imagewidth=100&imageheight=100&thumbmodel=0
        /// <summary>
        /// 读取文件数据，如图片直接浏览器预览，若获取缩略图，必须传URL查询参数
        /// </summary>
        /// <remarks>
        /// 若获取缩略图，必须传URL查询参数：<br/>
        /// imagewidth：缩略图宽度（单位：Pixel）<br/>
        /// imageheight：缩略图高度（单位：Pixel）<br/>
        /// thumbmodel：获取缩略图模式：0=不变形，全部（缩略图），1=变形，全部填充（缩略图），2=不变形，截中间（缩略图），3=不变形，截中间（非缩略图）<br/>
        /// </remarks>
        /// <param name="entityid">实体id（数据库名称）</param>
        /// <param name="fileid">文件ID</param>
        /// <returns>返回文件数据流</returns>
        [AllowAnonymous]
        [HttpGet("read")]
        public async Task<IActionResult> GetFile([FromQuery]ImageFileThumbnail queryParam)
        {
            if (queryParam == null)
                return ResponseError("缺乏查询参数");

            var entityid = queryParam.EntityId;
            var fileid = queryParam.FileId;
            var fileInfo = _fileService.GetOneFileInfo(queryParam.EntityId, fileid);
            if (fileInfo == null)
            {
                WriteLog(LogLevel.Warning, null, string.Format("文件【{0}】不存在", fileid));

                return ResponseError(string.Format("文件【{0}】不存在", fileid));
                
            }
            string filename = WebUtility.UrlEncode(fileInfo.FileName);

            Response.Headers.Add("filename", filename);
            Response.Headers.Add("filelength", fileInfo.Length.ToString());
            Response.Headers.Add("Content-Disposition", "inline; filename=" + filename);
            Response.Headers.Add("Content-Length", fileInfo.Length.ToString());//添加头文件，指定文件的大小，让浏览器显示文件下载的速度
            //获取缩略图
            if (queryParam.IsValid)
            {
                var bytes = _fileService.GetImageFileThumbnail(entityid, fileid, queryParam.ImageWidth, queryParam.ImageHeight, queryParam.Mode, out filename);

                Response.Headers["Content-Disposition"] = "inline; filename=" + WebUtility.UrlEncode(filename);
                Response.Headers["Content-Length"] = bytes.Length.ToString();
                await Response.Body.WriteAsync(bytes, 0, bytes.Length);
            }
            else
            {
                _fileService.GetFileData(entityid, fileid, Response.Body);
            }
            return File(Response.Body, "application/octet-stream");
        }

        #endregion

        #region --获取文件列表数据-- POST: /getfilesdata
        /// <summary>
        /// 获取文件列表数据(适用小文件，大文件请使用单个文件的接口)
        /// </summary>
        /// <param name="param">body对象</param>
        /// <remarks>
        /// 请求json格式：<br/>
        /// {
        ///	    "entityid":"employeedb_test",
        ///	    "fileids":[
        ///		    "586f3a15a5b5b94b64d913fb",
        ///		    "5875e8b8c1df3a37e48c2e2c"
        ///		],
        ///	    "thumbmodel":0,
        ///	    "imagewidth":100,
        ///	    "imageheight":100
        ///  }<br/>
        ///  响应格式：<br/>
        ///  {
        ///     "statusCode": 0,
        ///     "statusMessage": null,
        ///     "data":[{
        ///         "fileId": "587c8dccaafff7362c2517fa",
        ///         "fileName": "668573_170553606189_2.jpg",
        ///         "data": 【byte[] 对象】
        ///     }]
        ///   }
        /// </remarks>
        /// <returns>文件信息</returns>
        [HttpPost("getfilesdata")]
        public OutputResult<object> GetFilesData([FromBody] FilesDataModel param)
        {
            if (param == null)
                return ResponseError<object>("缺乏查询参数");
            var data = _fileService.GetFileListData(param.EntityID, param.FileIDs, param.ImageWidth, param.ImageHeight, param.ThumbModel);
            return new OutputResult<object>(data);
        }

        #endregion

        #region --获取文件列表信息-- POST： /getfilesinfo 
        /// <summary>
        /// 获取文件列表的信息
        /// </summary>
        /// <remarks>
        /// 请求json：<br/>
        /// {
        ///	    "entityid":"employeedb_test",
        ///	    "fileids":[
        ///		    "587c8dccaafff7362c2517fa",
        ///		    "587c8dccaafff7362c2517fa"
        ///		]
        ///   }<br/>
        ///  响应json：<br/>
        ///  {
        ///     "statusCode": 0,
        ///     "statusMessage": null,
        ///     "data":[{
        ///         "fileId": "587c8dccaafff7362c2517fa",
        ///         "fileName": "668573_170553606189_2.jpg",
        ///         "fileMD5": "dc892e5bc92aadc3fc0c56e57854c7c8",
        ///         "length": 113409,
        ///         "uploadDate": "2017-01-16T09:09:32.815Z"
        ///     }]
        ///    }
        /// </remarks>
        /// <param name="bodyData">body数据</param>
        /// <returns>文件列表的信息</returns>
        [HttpPost("getfilesinfo")]
        public OutputResult<object> GetFilesInfo([FromBody] FilesInfoModel bodyData)
        {
            if (bodyData == null)
                return ResponseError<object>("缺乏查询参数");
            var data = _fileService.GetFileListInfo(bodyData.EntityID, bodyData.FileIDs);
            return new OutputResult<object>(data);
        }
        #endregion

        #region --上传一个文件，适用于小文件-- POST: /upload 
        /// <summary>
        /// 上传一个文件，适用于小文件
        /// </summary>
        /// <remarks>
        /// request form参数,表单键值如下:<br/>
        /// data:文件<br/>
        /// entityid：实体id,数据库名称<br/>
        /// filename:文件名称<br/>
        /// fileMD5：文件MD5<br/>
        /// response json：<br/>
        /// {
        ///     "statusCode": 0,
        ///     "statusMessage": null,
        ///     "data": {
        ///          "fileId": "5898250a42e0d42f8073859d"
        ///     }
        /// }<br/>
        /// </remarks>
        /// <returns></returns>
        [HttpPost("upload")]
        public OutputResult<object> UploadFile([FromForm] UploadFileModel formData)
        {
            if (formData == null)
                return ResponseError<object>("缺乏查询参数");
            
            var isexist = _fileService.Exists(formData.EntityId, formData.FileId);
            string fileId = string.Empty;
            if (isexist == false)
            {
                fileId = _fileService.UploadFile(formData.EntityId, formData.FileId, formData.FileName, formData.Data.OpenReadStream());
            }
            WriteOperateLog("上传文件", new { formData.EntityId, FileId= fileId, formData.FileName });
            return new OutputResult<object>(fileId);
        }
       
        #endregion

        #region --大文件上传-- 

        /// <summary>
        /// 上传文件分片，适用大文件上传,分片大小最大为2M
        /// </summary>
        /// <returns></returns>
        [HttpPost("uploadchunk")]
        public OutputResult<object> UploadFileChunk([FromForm] UploadFileChunkRequest formData)
        {
            if (formData == null)
                return ResponseError<object>("缺乏查询参数");
           
            byte[] filedata = ImageHelper.StreamToBytes(formData.Data.OpenReadStream());
            var targetfileid = _fileService.UploadFileChunk(formData.EntityId, formData.FileId, formData.FileName, filedata, formData.FileMD5, formData.ChunkIndex, formData.FileLength, formData.ChunkSize);
            WriteOperateLog("上传文件", string.Format("文件【{0}】分片【{1}】已上传,文件ID为{2}", formData.FileName, formData.ChunkIndex, targetfileid));
            WriteLog(LogLevel.Information, null, string.Format("文件【{0}】分片【{1}】已上传,文件ID为{2}", formData.FileName, formData.ChunkIndex, targetfileid));
            return new OutputResult<object>(targetfileid);
        }

        /// <summary>
        /// 获取文件上传进度
        /// </summary>
        /// <remarks>
        /// request json ：无<br/>
        /// response json：<br/>
        /// {
        ///     "statusCode": 0,
        ///     "statusMessage": null,
        ///     "data": {
        ///         "uploaded": 113409,
        ///         "fileLength": 113409
        ///     }
        ///  }
        ///  </remarks>
        /// <param name="entityid">实体ID</param>
        /// <param name="fileid">文件id</param>
        /// <returns>Uploaded和FileLength，分别表示已上传大小和文件大小</returns>
        [HttpGet("getuploadprogress")]
        public OutputResult<object> GetUploadProgress([FromQuery]BaseRequestModel queryParam)
        {
            if (queryParam == null)
                return ResponseError<object>("缺乏查询参数");
            var data = _fileService.GetUploadedLength(queryParam.EntityId, queryParam.FileId);
            return new OutputResult<object>(data);
        }

        #endregion

        #region --下载文件-- GET： /download?entityid=dfdfdfdfd&fileid=dasdfasdfasdf
        /// <summary>
        /// 下载文件
        /// </summary>
        /// <remarks>
        /// 支持断点续传，如果断点续传，需要在Headers加Range信息
        /// </remarks>
        /// <param name="entityid">实体id，数据库名称</param>
        /// <param name="fileid">文件id</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("download")]
        public async Task<IActionResult> DownloadFileById([FromQuery]BaseRequestModel queryParam)
        {
            if (queryParam == null)
                return ResponseError("缺乏查询参数");
            return await Task.Run<IActionResult>(() =>
            {
                IActionResult result = NotFound();
                try
                {
                    //获取文件信息和文件流
                    var data = _fileService.DownloadFile(queryParam.EntityId, queryParam.FileId);

                    var fileInfo = data.FileInfo;
                    Stream dataReader = data.DataReader;
                    if (fileInfo == null)
                    {
                        Response.StatusCode = (int)HttpStatusCode.NotFound;
                        return ResponseError(string.Format("文件【{0}】不存在", queryParam.FileId));
                    }
                    WriteLog(LogLevel.Information, null, string.Format("下载文件【{0}】", fileInfo.FileName));
                    //设置header
                    RequestHeaders requestHeaders = new RequestHeaders(Request.Headers);
                   
                    Response.ContentType = "application/octet-stream; charset=utf-8" ;
                    Response.Headers.Add("Content-Disposition", string.Format("attachment; filename={0}; filename*=utf-8''{0}", WebUtility.UrlEncode(fileInfo.FileName)));
                    Response.Headers.Add("Accept-Ranges", "bytes");//告诉客户端接受资源为字节
                    Response.Headers.Add("filename", WebUtility.UrlEncode(fileInfo.FileName));
                    Response.Headers.Add("filelength", fileInfo.Length.ToString());
                    Response.Headers.Add("Content-Length", fileInfo.Length.ToString());//添加头文件，指定文件的大小，让浏览器显示文件下载的速度
                    //Response.Headers.Add("Content-Encoding", encoding);//告诉客户端接受资源为字节

                    //获取下载范围
                    RangeItemHeaderValue range = null;
                    //不是按照部分内容下载的情况下
                    if (requestHeaders.Range == null || requestHeaders.Range.Ranges.Count == 0)
                    {
                        range = new RangeItemHeaderValue(0, fileInfo.Length - 1);
                    }
                    else
                    {
                        var firstrange = requestHeaders.Range.Ranges.FirstOrDefault();
                        //Range:{from}-{to} Or {from}-
                        if (firstrange != null && firstrange.From.HasValue)
                        {
                            if (firstrange.From.Value < fileInfo.Length - 1)
                            {
                                long to = firstrange.To.HasValue ? Math.Min(firstrange.To.Value, fileInfo.Length - 1) : fileInfo.Length - 1;
                                range = new RangeItemHeaderValue(firstrange.From.Value, to);
                            }
                        }
                        //Range：-{size}
                        else if (firstrange != null && firstrange.To.Value != 0)
                        {
                            long size = Math.Min(fileInfo.Length, firstrange.To.Value);
                            range = new RangeItemHeaderValue(fileInfo.Length - size, fileInfo.Length - 1);
                        }

                        //如果range设置错误
                        if (range == null)
                        {
                            //range = new RangeItemHeaderValue(0, fileInfo.Length - 1);
                            Response.StatusCode = (int)HttpStatusCode.RequestedRangeNotSatisfiable;
                            return ResponseError("416 Range Not Satisfiable");
                        }
                        else
                        {
                            Response.StatusCode = (int)HttpStatusCode.PartialContent;
                            Response.Headers.Add("Content-Range", string.Format("bytes {0}-{1}/{2}", range.From, range.To, fileInfo.Length));//告诉客户端接受资源为字节
                            Response.Headers["Content-Length"] = (range.To - range.From + 1).ToString();//重新设置数据长度
                        }
                    }

                    //开始写入数据
                    int bufferSize = fileInfo.Length < 1024 ? (int)fileInfo.Length : 1024;
                    byte[] buffer = new byte[bufferSize];
                    dataReader.Seek(range.From.Value, SeekOrigin.Begin);
                    int readIndex = dataReader.Read(buffer, 0, bufferSize);

                    Response.Body.Write(buffer, 0, buffer.Length);
                    while (readIndex > 0 && dataReader.Position - 1 <= range.To.Value)
                    {
                        if (fileInfo.Length - dataReader.Position < bufferSize)
                        {
                            bufferSize = (int)(fileInfo.Length - dataReader.Position);
                            buffer = new byte[bufferSize];
                        }
                        readIndex = dataReader.Read(buffer, 0, bufferSize);
                        Response.Body.Write(buffer, 0, buffer.Length);
                    }
                    var ss = Response.ContentType.ToString();
                    result = File(Response.Body, Response.ContentType.ToString());

                }
                catch (Exception ex)
                {
                    WriteLog(LogLevel.Error, ex, "下载文件失败");
                    throw new Exception("下载文件失败", ex);
                }
                return result;
            });
        }


        #endregion

        #region --删除一个文件-- POST： delete?entityid=dfdfdfdfd&fileid=dasdfasdfasdf
        /// <summary>
        /// 删除一个文件
        /// </summary>
        /// <remarks>
        /// request json:无<br/>
        /// response json：<br/>
        /// {
        ///     "statusCode": 0,
        ///     "statusMessage": null,
        ///     "data": "5898250a42e0d42f8073859d"
        ///  }
        /// </remarks>
        /// <param name="entityid"></param>
        /// <param name="fileid"></param>
        /// <returns></returns>
        [HttpPost("delete")]
        public OutputResult<object> DeleteFile([FromBody]BaseRequestModel queryParam)
        {
            if (queryParam == null)
                return ResponseError<object>("缺乏查询参数");
            _fileService.DeleteFile(queryParam.EntityId, queryParam.FileId);
            WriteOperateLog("删除一个文件", queryParam);
            return new OutputResult<object>(queryParam.FileId);
        }
        #endregion

    }
}