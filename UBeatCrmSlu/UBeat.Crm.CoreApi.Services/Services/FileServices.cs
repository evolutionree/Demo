using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.DomainModel.FileService;
using UBeat.Crm.CoreApi.Repository.Repository.FileService;
using UBeat.Crm.CoreApi.Services.Models.FileService;
using UBeat.Crm.CoreApi.Services.Utility;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class FileServices: BaseServices
    {

        private readonly MongodbConfig mongoConfig;
        public readonly FileServiceConfig UrlConfig;

        public FileServices()
        {
            IConfigurationRoot config = ServiceLocator.Current.GetInstance<IConfigurationRoot>();
            mongoConfig = config.GetSection("MongodbSetting").Get<MongodbConfig>();
            UrlConfig = config.GetSection("FileServiceSetting").Get<FileServiceConfig>();
        }

        /// <summary>
        /// 获取MongodbRepository实例
        /// </summary>
        /// <param name="collectionName"></param>
        /// <returns></returns>
        private MongodbRepository GetMongodbRepository(string collectionName)
        {
            if (mongoConfig == null)
                throw new Exception("缺少Mongodb连接字符串配置信息");
            if (string.IsNullOrEmpty(collectionName))
            {
                collectionName = mongoConfig.DefaultDb;
            }
            return MongodbRepository.GetInstance(collectionName, mongoConfig);
        }

        
        /// <summary>
        /// 获取文件数据流
        /// </summary>
        /// <param name="collectionName">文件集合的名称（数据库名称）</param>
        /// <param name="fileid"></param>
        /// <returns></returns>
        public byte[] GetFileData(string collectionName, string fileid)
        {
            var task = GetMongodbRepository(collectionName).GetFileData(fileid);
            task.Wait();
            return task.Result;
        }
        /// <summary>
        ///  根据id 获取一个文件数据读取流对象
        /// </summary>
        /// <param name="collectionName">文件集合的名称（数据库名称）</param>
        /// <param name="fileid"></param>
        /// <param name="destination"></param>
        public void GetFileData(string collectionName, string fileid, Stream destination)
        {
            var task = GetMongodbRepository(collectionName).GetFileData(fileid, destination);
            task.Wait();
        }

        /// <summary>
        /// 获取图片文件的缩略图
        /// </summary>
        /// <param name="collectionName">文件集合的名称（数据库名称）</param>
        /// <param name="fileid">文件id</param>
        /// <param name="imagewidth">缩略图宽度（单位：Pixel）</param>
        /// <param name="imageheight">缩略图高度（单位：Pixel）</param>
        /// <param name="thumbModel">获取缩略图模式：0=不变形，全部（缩略图），1=变形，全部填充（缩略图），2=不变形，截中间（缩略图），3=不变形，截中间（非缩略图）</param>
        /// <param name="thumbnailName">生成缩略图后，重新生成的文件名称，后缀统一都是jpg</param>
        /// <returns></returns>
        public byte[] GetImageFileThumbnail(string collectionName, string fileid, int imagewidth, int imageheight, ThumbModel thumbModel, out string thumbnailName)
        {
            var fileinfo = GetOneFileInfo(collectionName, fileid);
            if (fileinfo == null)
                throw new Exception(string.Format("文件【{0}】不存在", fileid));
            var bytes = GetFileData(collectionName, fileid);
            if (bytes == null)
            {
                throw new Exception(string.Format("文件【{0}】数据为空", fileid));
            }
            bytes = ThumbnailHelper.CreateThumbnail(bytes, imagewidth, imageheight, fileinfo.FileName, thumbModel, out thumbnailName);
            return bytes;
        }

        /// <summary>
        /// 获取一个文件的文件信息
        /// </summary>
        /// <param name="collectionName">文件集合的名称（数据库名称）</param>
        /// <param name="fileid">文件id</param>
        /// <returns></returns>
        public  FileInfoModel GetOneFileInfo(string collectionName, string fileid)
        {
            var task = GetMongodbRepository(collectionName).GetFileInfo(fileid);
            task.Wait();
            return task.Result;
        }

        /// <summary>
        /// 获取多个文件的文件信息
        /// </summary>
        /// <param name="collectionName">文件集合的名称（数据库名称）</param>
        /// <param name="fileids">文件id列表</param>
        /// <returns>返回文件列表信息</returns>
        public List<FileInfoModel> GetFileListInfo(string collectionName, IEnumerable<string> fileids)
        {
            if (fileids == null)
                throw new Exception("fileids不可为null");

            var task = GetMongodbRepository(collectionName).GetFilesInfo(fileids);
            task.Wait();
            return task.Result;
        }

        /// <summary>
        /// 获取多个文件的文件数据流
        /// </summary>
        /// <param name="collectionName">文件集合的名称（数据库名称）</param>
        /// <param name="fileids">文件id列表</param>
        /// <returns></returns>
        public List<FileDataModel> GetFileListData(string collectionName, IEnumerable<string> fileids, int imagewidth = 0, int imageheight = 0, ThumbModel thumbModel = ThumbModel.NoDeformationAllThumb)
        {
            if (fileids == null)
                throw new Exception("fileids参数不对");
            List<FileDataModel> listData = new List<FileDataModel>();
            foreach (var m in fileids)
            {
                listData.Add(new FileDataModel()
                {
                    FileId = m
                });
            }

            var infoList = GetFileListInfo(collectionName, fileids);
            if (infoList != null && infoList.Count > 0)
            {
                foreach (var file in infoList)
                {
                    byte[] bytes = null;
                    string filename = file.FileName;
                    bytes = GetFileData(collectionName, file.FileId);
                    //获取缩略图
                    if (imagewidth > 0 && imageheight > 0)
                    {
                        bytes = ThumbnailHelper.CreateThumbnail(bytes, imagewidth, imageheight, filename, thumbModel, out filename);
                    }

                    var mdata = listData.Find(m => m.FileId.Equals(file.FileId));
                    mdata.Data = bytes;
                    mdata.FileName = filename;
                }
            }
            return listData;
        }

        /// <summary>
        /// 上传一个文件
        /// </summary>
        /// <param name="collectionName">文件集合的名称（数据库名称）</param>
        /// <param name="fileName"></param>
        /// <param name="fileStream"></param>
        /// <returns>上传成功的fileID</returns>
        public string UploadFile(string collectionName, string fileId, string fileName, Stream fileStream)
        {

            var task = GetMongodbRepository(collectionName).UploadFile(fileId, fileName, fileStream);
            task.Wait();
            return task.Result;
        }
        /// <summary>
        /// 上传一个文件
        /// </summary>
        /// <param name="collectionName">文件集合的名称（数据库名称）</param>
        /// <param name="fileName"></param>
        /// <param name="fileStream"></param>
        /// <returns>上传成功的fileID</returns>
        public string UploadFile(string collectionName, string fileId, string fileName, byte[] source)
        {

            var task = GetMongodbRepository(collectionName).UploadFile(fileId, fileName, source);
            task.Wait();
            return task.Result;
        }

        /// <summary>
        /// 上传文件分片，适用大文件上传,分片大小最大为2M
        /// </summary>
        /// <param name="collectionName">文件集合的名称（数据库名称）</param>
        /// <param name="fileid">上传文件的id</param>
        /// <param name="fileName">文件名称</param>
        /// <param name="filedata">文件分片数据体</param>
        /// <param name="fileMD5">文件md5</param>
        /// <param name="chunkIndex">分片序号，0开始</param>
        /// <param name="fileLength">文件长度，必须保证准确</param>
        /// <param name="chunkSize">文件分片大小</param>
        /// <returns>最终文件分片上传到的文件fileid</returns>
        public string UploadFileChunk(string collectionName, string fileid, string fileName, byte[] filedata, string fileMD5, int chunkIndex, long fileLength, int chunkSize)
        {

            var task = GetMongodbRepository(collectionName).UploadFileChunk(fileid, fileName, filedata, fileMD5, chunkIndex, fileLength, chunkSize);

            task.Wait();
            return task.Result;
        }

        /// <summary>
        /// 获取某个文件已上传的大小
        /// </summary>
        /// <param name="collectionName">文件集合的名称（数据库名称）</param>
        /// <param name="fileid"></param>
        /// <returns></returns>
        public UploadedStatus GetUploadedLength(string collectionName, string fileid)
        {
            var task = GetMongodbRepository(collectionName).GetUploadedLength(fileid);
            task.Wait();
            return task.Result;
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="collectionName">文件集合的名称（数据库名称）</param>
        /// <param name="fileid"></param>
        /// <returns></returns>
        public DownloadFileModel DownloadFile(string collectionName, string fileid)
        {
            var task = GetMongodbRepository(collectionName).DownloadFile(fileid);
            task.Wait();
            return task.Result;
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="collectionName">文件集合的名称（数据库名称）</param>
        /// <param name="fileid"></param>
        public void DeleteFile(string collectionName, string fileid)
        {
            var task = GetMongodbRepository(collectionName).DeleteAsync(fileid);
            task.Wait();

        }

        /// <summary>
        /// 判断文件是否存在
        /// </summary>
        /// <param name="collectionName">文件集合的名称（数据库名称）</param>
        /// <param name="fileId"></param>
        /// <returns>存在文件的id</returns>
        public bool Exists(string collectionName, string fileId)
        {
            var task = GetMongodbRepository(collectionName).Exists(fileId);
            task.Wait();
            var objid = task.Result;
            return objid;
        }


    }
}
