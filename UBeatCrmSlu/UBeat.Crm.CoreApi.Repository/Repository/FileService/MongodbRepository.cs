using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UBeat.Crm.CoreApi.DomainModel.FileService;

namespace UBeat.Crm.CoreApi.Repository.Repository.FileService
{
    public class MongodbRepository
    {
        IMongoDatabase _db;
        GridFSBucket gridFSBucket;
        static Dictionary<string, MongodbRepository> instanceDic = new Dictionary<string, MongodbRepository>();
        IMongoCollection<BsonDocument> fschunkscollection;
        IMongoCollection<BsonDocument> fsfilescollection;
        static string fileIdKey = "filename";
        static string fileNameKey = "fileName";


        private MongodbRepository(string connectString, string databaseName)
        {
            // connectString = "mongodb://172.16.31.161:27017/employeedb_test";
            _db = GetDatabase(connectString, databaseName);
            fsfilescollection = _db.GetCollection<BsonDocument>("fs.files");
            fschunkscollection = _db.GetCollection<BsonDocument>("fs.chunks");
            gridFSBucket = GetGridFSBucket();
        }

        private IMongoDatabase GetDatabase(string connectString, string databaseName)
        {

            var mongoUrl = new MongoUrl(connectString);
            var client = new MongoClient(mongoUrl);
            return client.GetDatabase(databaseName);
        }

        private GridFSBucket GetGridFSBucket()
        {
            if (gridFSBucket == null)
            {
                //设置分片大小为2M
                GridFSBucketOptions option = new GridFSBucketOptions()
                {
                    ChunkSizeBytes = 1024 * 1024 * 2
                };
                gridFSBucket = new GridFSBucket(_db, option);
            }
            return gridFSBucket;
        }



        /// <summary>
        /// 获取实例
        /// </summary>
        /// <param name="connectString">标准格式： mongodb://[username:password@]host1[:port1][,host2[:port2],...[,hostN[:portN]]][/[database][?options]] </param>
        /// <returns></returns>
        public static MongodbRepository GetInstance(string dbName, MongodbConfig config)
        {
            if (string.IsNullOrEmpty(dbName))
            {
                dbName = "default_filedb";
            }

            //string connectString = string.Format("mongodb://172.16.31.161:27017/{0}", dbName);
            if (instanceDic == null)
            {
                instanceDic = new Dictionary<string, MongodbRepository>();
            }
            if (!instanceDic.ContainsKey(dbName))
            {
                //由于要支持每个实体一个数据库，所以这里不能传入数据库名称，直接使用用户密码连接服务后，再通过数据库名获取数据库实例
                string connectString = config.GetConnectString(string.Empty);
                instanceDic.Add(dbName, new MongodbRepository(connectString, dbName));
            }
            return instanceDic[dbName];
        }
        /// <summary>
        /// 获取文件信息
        /// </summary>
        /// <param name="fileids"></param>
        /// <returns></returns>
        public async Task<List<FileInfoModel>> GetFilesInfo(IEnumerable<string> fileids)
        {

            List<FileInfoModel> list = new List<FileInfoModel>();
            List<BsonValue> values = new List<BsonValue>();
            foreach (var m in fileids)
            {
                values.Add(BsonValue.Create(m));
            }
            var query = Builders<GridFSFileInfo>.Filter.In(fileIdKey, values);
            //var query = Builders<GridFSFileInfo>.Filter.Where(m =>  fileids.Contains(m.Id));
            var files = await GetGridFSBucket().Find(query).ToListAsync();
            if (files == null)
                return list;
            foreach (var item in files)
            {
                list.Add(new FileInfoModel()
                {
                    FileId = item.Filename,
                    FileName = item.Metadata.GetValue(fileNameKey).ToString(),
                    UploadDate = item.UploadDateTime,
                    //FileMD5 = item.MD5,
                    Length = item.Length
                });
            }
            return list;
        }

        /// <summary>
        /// 根据id 获取一个文件信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns>返回文件信息对象</returns>
        public async Task<FileInfoModel> GetFileInfo(string id)
        {
            //var file = GetGridFSBucket().FindOneById(id);
            var query = Builders<GridFSFileInfo>.Filter.Eq(fileIdKey, id);
            var fileInfo = await GetGridFSBucket().Find(query).FirstOrDefaultAsync();
            if (fileInfo == null)
                return null;
            return new FileInfoModel()
            {
                FileId = fileInfo.Filename,
                FileName = fileInfo.Metadata.GetValue(fileNameKey).ToString(),
                UploadDate = fileInfo.UploadDateTime,
                //FileMD5 = fileInfo.MD5,
                Length = fileInfo.Length
            };
        }

        /// <summary>
        /// 根据id 获取一个文件数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns>返回Data对象</returns>
        public async Task<byte[]> GetFileData(string id)
        {
            //var file = GetGridFSBucket().FindOneById(id);
            //var query = Builders<GridFSFileInfo>.Filter.Eq("_id", id);
            //var fileInfo = await GetGridFSBucket().Find(query).FirstOrDefaultAsync();
            var query = Builders<GridFSFileInfo>.Filter.Eq(fileIdKey, id);
            var queryData = GetGridFSBucket().Find(query);
            if (queryData == null)
                return null;

            var fileInfo = await queryData.FirstOrDefaultAsync();
            if(fileInfo==null)
                return null;
            var fileData = await GetGridFSBucket().DownloadAsBytesAsync(fileInfo.Id);
            return fileData;
            //return new { FileName = fileInfo.Filename, Data = fileData };
        }



        /// <summary>
        /// 根据id 获取一个文件数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns>返回FileName和Data对象</returns>
        public async Task GetFileData(string id, Stream destination)
        {
            var query = Builders<GridFSFileInfo>.Filter.Eq(fileIdKey, id);
            var fileInfo = await GetGridFSBucket().Find(query).FirstOrDefaultAsync();
            await GetGridFSBucket().DownloadToStreamAsync(fileInfo.Id, destination);

            //return new { FileName = fileInfo.Filename, Data = destination };
        }

        /// <summary>
        /// 上传一个文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileStream"></param>
        /// <returns>上传成功的fileID</returns>
        public async Task<string> UploadFile(string fileId, string fileName, Stream fileStream)
        {
            if (string.IsNullOrEmpty(fileId))
                fileId = Guid.NewGuid().ToString();
            GridFSUploadOptions option = new GridFSUploadOptions();
            ////option.BatchSize = 1024 * 1024 * 2;
            //option.ChunkSizeBytes = 1024 * 1024 * 2;
            option.Metadata = new BsonDocument(fileNameKey, fileName);
            var objid = await GetGridFSBucket().UploadFromStreamAsync(fileId, fileStream, option);
            if (objid != ObjectId.Empty)
                return fileId;
            return fileId;
        }
        /// <summary>
        /// 上传一个文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileStream"></param>
        /// <returns>上传成功的fileID</returns>
        public async Task<string> UploadFile(string fileId, string fileName, byte[] source)
        {
            if (string.IsNullOrEmpty(fileId))
                fileId = Guid.NewGuid().ToString();
            GridFSUploadOptions option = new GridFSUploadOptions();
            ////option.BatchSize = 1024 * 1024 * 2;
            //option.ChunkSizeBytes = 1024 * 1024 * 2;
            option.Metadata = new BsonDocument(fileNameKey, fileName);
            var objid = await GetGridFSBucket().UploadFromBytesAsync(fileId, source, option);
            if (objid != ObjectId.Empty)
                return fileId;
            return fileId;
        }


        /// <summary>
        /// 上传一个文件分片
        /// </summary>
        /// <param name="fileid">上传文件的id</param>
        /// <param name="fileName">文件名称</param>
        /// <param name="filedata">文件分片数据体</param>
        /// <param name="fileMD5">文件md5</param>
        /// <param name="chunkIndex">分片序号，0开始</param>
        /// <param name="fileLength">文件长度，必须保证准确</param>
        /// <param name="chunkSize">文件分片大小</param>
        /// <returns>最终文件分片上传到的文件fileid</returns>
        public async Task<string> UploadFileChunk(string fileid, string fileName, byte[] filedata, string fileMD5, int chunkIndex, long fileLength, int chunkSize)
        {
            try
            {
                if (string.IsNullOrEmpty(fileid))
                {
                    throw new Exception("文件ID不可为空");
                }
                if (string.IsNullOrEmpty(fileMD5))
                {
                    fileMD5 = "";
                }
                if (chunkSize == 0)
                {
                    throw new Exception("文件分片大小不可为" + chunkSize + "bytes");
                }
                if (filedata == null || filedata.Length > chunkSize)
                {
                    throw new Exception("文件数据段大小必须小于等于分片大小" + chunkSize + "bytes");
                }
                if (fileLength < filedata.Length)
                {
                    throw new Exception("文件长度不准确");
                }
                //以文件id作为唯一标识，如果找到已经存在的文件记录，则取数据库中的文件信息的filedid，否则fileid由参数决定
                var query = Builders<BsonDocument>.Filter.Eq(fileIdKey, fileid);
                var fileinfo = await fsfilescollection.Find(query).FirstOrDefaultAsync();
                if (fileinfo == null)
                {
                    var metadata = new BsonDocument(fileNameKey, fileName);
                    fileinfo = new BsonDocument()
                    {
                        new BsonElement("_id",ObjectId.GenerateNewId()),
                        new BsonElement("filename",fileid),
                        new BsonElement("length",fileLength),
                        new BsonElement("chunkSize",chunkSize),
                        new BsonElement("uploadDate",DateTime.Now),
                        new BsonElement("md5",fileMD5),
                        new BsonElement("metadata",metadata)
                    };

                    await fsfilescollection.InsertOneAsync(fileinfo);
                }


                //查找分片是否存在了，如果存在则重新上传该分片，否则新增数据
                var builder = Builders<BsonDocument>.Filter;
                var filter = builder.Eq("files_id", fileinfo.GetValue("_id")) & builder.Eq("n", chunkIndex);
                var filechunk = await fschunkscollection.Find(filter).FirstOrDefaultAsync();

                if (filechunk == null)
                {
                    var insert = new BsonDocument {
                        { "_id", ObjectId.GenerateNewId() },
                        { "files_id", fileinfo.GetValue("_id") },
                        { "n", chunkIndex },
                        { "data", filedata }
                     };
                    await fschunkscollection.InsertOneAsync(insert);
                }

                else
                {
                    byte[] data = (byte[])filechunk.GetValue("data");
                    //如果该分片的数据都已经上传完成了，则不重新上传
                    if (data.Length == chunkSize || data.Length == fileLength - chunkIndex * chunkSize)
                        return fileid;
                    var chunkid = filechunk.GetValue("_id");
                    var update = new BsonDocument("$set", new BsonDocument
                    {
                        { "_id", chunkid },
                        { "files_id", fileinfo.GetValue("_id") },
                        { "n", chunkIndex },
                        { "data", filedata }
                    });
                    var updatechunckquery = Builders<BsonDocument>.Filter.Eq("_id", chunkid);
                    await fschunkscollection.UpdateOneAsync(updatechunckquery, update);
                }
                return fileid;
            }
            catch (Exception ex)
            {
                throw new Exception("执行出错：" + ex.Message);
            }

        }

        /// <summary>
        /// 获取某个文件已上传的大小
        /// </summary>
        /// <param name="fileid">文件id</param>
        /// <returns>Uploaded和FileLength，分别表示已上传大小和文件大小,单位：字节</returns>
        public async Task<UploadedStatus> GetUploadedLength(string fileid)
        {
            long uploadedLength = 0;
            var queryFile = Builders<GridFSFileInfo>.Filter.Eq(fileIdKey, fileid);
            var fileInfo = await GetGridFSBucket().Find(queryFile).FirstOrDefaultAsync();
            if (fileInfo == null)
                throw new Exception("this file is no exist");


            var builder = Builders<BsonDocument>.Filter;
            var filter = builder.Eq("files_id", fileInfo.Id);
            List<BsonDocument> filechunks = await fschunkscollection.Find(filter).ToListAsync();
            //var query = Builders<GridFSFileInfo>.Filter.Eq("_id", fileid);
            // var fileInfo = await GetGridFSBucket().Find(query).FirstOrDefaultAsync();
            long fileLength = fileInfo.Length;
            foreach (var o in filechunks)
            {
                var data = (byte[])o.GetValue("data");
                var index = (int)o.GetValue("n");
                //if (data.Length == gridFSBucket.Options.ChunkSizeBytes || data.Length == fileLength - index * gridFSBucket.Options.ChunkSizeBytes)
                //{
                uploadedLength += data.Length;
                //}
            }
            return new UploadedStatus()
            {
                Uploaded = uploadedLength,
                FileLength = fileLength
            };
        }


        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="id"></param>
        /// <returns>FileInfo和DataReader组合对象</returns>
        public async Task<DownloadFileModel> DownloadFile(string id)
        {
            var query = Builders<GridFSFileInfo>.Filter.Eq(fileIdKey, id);
            var fileInfo = await GetGridFSBucket().Find(query).FirstOrDefaultAsync();
            if (fileInfo == null)
                throw new Exception("this file is no exist");

            GridFSDownloadOptions option = new GridFSDownloadOptions()
            {
                Seekable = true
            };

            var openStream = await GetGridFSBucket().OpenDownloadStreamAsync(fileInfo.Id, option);

            FileInfoModel m = new FileInfoModel()
            {
                FileId = fileInfo.Filename,
                //FileMD5 = fileInfo.MD5,
                FileName = fileInfo.Metadata.GetValue(fileNameKey).ToString(),
                Length = fileInfo.Length,
                UploadDate = fileInfo.UploadDateTime

            };

            return new DownloadFileModel()
            {
                FileInfo = m,
                DataReader = openStream
            };

        }

        public async Task DeleteAsync(string id)
        {
            var query = Builders<GridFSFileInfo>.Filter.Eq(fileIdKey, id);
            var fileInfo = await GetGridFSBucket().Find(query).FirstOrDefaultAsync();
            if (fileInfo == null)
                return;
            await GetGridFSBucket().DeleteAsync(fileInfo.Id);
        }




        public async Task<bool> Exists(string fileId)
        {
            //var query = Builders<GridFSFileInfo>.Filter.Eq("md5", fileMD5);
            //var fileInfo = await GetGridFSBucket().Find(query).FirstOrDefaultAsync();
            //return fileInfo == null ? ObjectId.Empty : fileInfo.Id;

            var query = Builders<BsonDocument>.Filter.Eq(fileIdKey, fileId);
            var fileinfo = await fsfilescollection.Find(query).FirstOrDefaultAsync();
            return fileinfo != null;
        }
    }
}
