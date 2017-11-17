using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Documents;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Utility;

namespace UBeat.Crm.CoreApi.Repository.Repository.Documents
{
    public class DocumentsRepository : RepositoryBase, IDocumentsRepository
    {

        #region --InsertDocument--
        public Guid? InsertDocument(DbTransaction transaction, DocumentInsert data, int userNumber)
        {
            Guid entityid = Guid.Empty;
            Guid fileid = Guid.Empty;
            Guid businessid = Guid.Empty;
            Guid folderid = Guid.Empty;
            int recorder = 0;
            DateTime reccreated = DateTime.Now;
            if (!Guid.TryParse(data.EntityId, out entityid))
            {
                throw new Exception("业务实体ID格式错误");
            }
            if (!Guid.TryParse(data.FileId, out fileid))
            {
                throw new Exception("文件ID格式错误");
            }
            if (!Guid.TryParse(data.BusinessId, out businessid))
            {
                //throw new Exception("业务ID格式错误");
            }
            if (!Guid.TryParse(data.FolderId, out folderid))
            {
                throw new Exception("目录ID格式错误");
            }
            var recorder_Sql = @"SELECT max(recorder) from crm_sys_documents where entityid=@entityid AND businessid=@businessid;";
            var recorder_param = new DbParameter[]
            {
                new NpgsqlParameter("entityid", entityid),
                new NpgsqlParameter("businessid", businessid),
            };
            var recorderResult = DBHelper.ExecuteScalar(transaction, recorder_Sql, recorder_param);
            if (recorderResult != null && int.TryParse(recorderResult.ToString(), out recorder))
            {
                recorder += 1;
            }

            var sql = @"
                    INSERT INTO crm_sys_documents( documentid,entityid, businessid, fileid, filename, filelength, folderid,  downloadcount, recorder, recstatus, reccreator, recupdator, reccreated,  recupdated)
                    VALUES(@documentid, @entityid, @businessid, @fileid, @filename, @filelength, @folderid, 0, @recorder, 1, @userNumber, @userNumber, @reccreated, @reccreated); ";

            Guid documentid = Guid.NewGuid();

            var param = new DbParameter[]
            {
                new NpgsqlParameter("documentid", documentid),
                new NpgsqlParameter("entityid", entityid),
                new NpgsqlParameter("businessid", businessid),
                new NpgsqlParameter("fileid", fileid),
                new NpgsqlParameter("filename", data.FileName),
                new NpgsqlParameter("filelength", data.FileLength),
                new NpgsqlParameter("folderid", folderid),
                new NpgsqlParameter("recorder", recorder),
                new NpgsqlParameter("userNumber",userNumber),
                new NpgsqlParameter("reccreated",reccreated),
            };
            var rowcount = DBHelper.ExecuteNonQuery(transaction, sql, param);
            if (rowcount > 0)
                return documentid;
            return null;
        }
        #endregion


        public DocumentInfo GetDocumentInfo(Guid documentid)
        {
            var sql = @"SELECT * FROM crm_sys_documents WHERE documentid=@documentid ";
            var param = new DbParameter[]
             {
                new NpgsqlParameter("documentid", documentid),

             };
            return DBHelper.ExecuteQuery<DocumentInfo>("", sql, param).FirstOrDefault();
        }

        //手机端专用接口
        public dynamic GetAllDocumentList(DbTransaction transaction, string ruleSql, DocumentList data, int userNumber)
        {
            DocumentFolderList folderdata = new DocumentFolderList()
            {
                EntityId = data.EntityId,
                FolderId = data.FolderId,
                Direction = "DOWNER",
                RecStatus = data.RecStatus,
                RecVersion = data.FolderRecVersion,
                Servicetype = 1
            };
            //获取目录列表
            var folders = DocumentFolderList(transaction, ruleSql, folderdata, userNumber);

            //获取所有文件
            data.IsAllDocuments = true;
            var documentsDic = DocumentList(transaction, ruleSql, null, data, userNumber);
            var documents = documentsDic.PageData;

            return new { folders, documents };
        }

        public dynamic DocumentList(DbTransaction transaction, string ruleSql, PageParam pageParam, DocumentList data, int userNumber)
        {
            if (pageParam == null)
                pageParam = new PageParam();
            if (data != null && data.IsAllDocuments)
            {
                pageParam.PageSize = -1;
            }

            Guid entityid = Guid.Empty;
            Guid businessid = Guid.Empty;
            Guid folderId = Guid.Empty;

            if (!Guid.TryParse(data.EntityId, out entityid))
            {
                throw new Exception("业务实体ID格式错误");
            }
            if (!Guid.TryParse(data.BusinessId, out businessid))
            {
                //throw new Exception("业务ID格式错误");
            }
            List<DbParameter> dbParameters = new List<DbParameter>();

            var _recstatus_sql = string.Empty;
            if (data.RecStatus != -1)
            {
                _recstatus_sql = " AND d.recstatus=@recstatus";
                dbParameters.Add(new NpgsqlParameter("recstatus", data.RecStatus));
            }

            var _recversion_sql = string.Empty;
            if (data.RecVersion > 0)
            {
                _recversion_sql = " AND d.recversion>@recversion";
                dbParameters.Add(new NpgsqlParameter("recversion", data.RecVersion));
            }

            var _folder_sql = string.Empty;

            if (!data.IsAllDocuments && string.IsNullOrEmpty(data.FolderId) && Guid.TryParse(data.FolderId, out folderId))
            {
                _folder_sql = " AND d.folderid=@folderid";
                dbParameters.Add(new NpgsqlParameter("folderid", folderId));
            }
            else
            {
                _folder_sql = " AND d.folderid IN(SELECT folderid FROM  crm_func_documentsfolder_select(@entityidtext,@folderidtext,'DOWNER',@userNumber))";
                dbParameters.Add(new NpgsqlParameter("entityidtext", data.EntityId));
                dbParameters.Add(new NpgsqlParameter("folderidtext", data.FolderId));
            }

            var executeSql = string.Format(@"SELECT d.*,u.username AS reccreatorname ,f.foldername
				            FROM public.crm_sys_documents AS d 
					        LEFT JOIN crm_sys_userinfo AS u ON u.userid=d.reccreator 
					        LEFT JOIN crm_sys_documents_folder AS f ON f.folderid=d.folderid 
					        WHERE d.entityid=@entityid AND d.businessid=@businessid {0} {1} {2}  ORDER BY d.recversion DESC", _folder_sql, _recstatus_sql, _recversion_sql);

            //if (!string.IsNullOrEmpty(ruleSql))
            //{
            //    executeSql = string.Format("SELECT * FROM({0}) AS e WHERE {1}", executeSql, ruleSql);
            //}

            // var executeSql = "select  crm_func_documents_select(@entityid,@businessid,@folderid,@includechild,@userno,@pageindex,@pagesize,@recversion,@recstatus,@searchkey)";
            dbParameters.Add(new NpgsqlParameter("userNumber", userNumber));
            dbParameters.Add(new NpgsqlParameter("entityid", entityid));
            dbParameters.Add(new NpgsqlParameter("businessid", businessid));
            if (pageParam.PageSize <= 0)
            {
                var result = ExecuteQuery(executeSql, dbParameters.ToArray(), transaction);
                return new
                {
                    PageData = result,
                    PageCount = new List<dynamic>() { new { total = 0, page = 0 } }
                };
            }
            else
            {
                var result = ExecuteQueryByPaging(executeSql, dbParameters.ToArray(), pageParam.PageSize, pageParam.PageIndex, transaction);
                return new
                {
                    PageData = result.DataList,
                    PageCount = new List<dynamic>() { new { total = result.PageInfo.TotalCount, page = result.PageInfo.PageCount } }
                };
            }
           
           
        }


        #region --移动一个文档--
        public bool MoveDocument(DbTransaction tran, DocumentMove data, int userNumber)
        {
            Guid documentid = Guid.Empty;
            Guid folderid = Guid.Empty;
            DateTime recupdated = DateTime.Now;
            if (!Guid.TryParse(data.DocumentId, out documentid))
            {
                throw new Exception("数据ID格式错误");
            }

            if (!Guid.TryParse(data.FolderId, out folderid))
            {
                throw new Exception("目录ID格式错误");
            }
            var sql = @"SELECT 1 FROM crm_sys_documents_folder WHERE folderid = @folderid AND recstatus = 1  LIMIT 1";
            var sql_param = new DbParameter[]
             {
                new NpgsqlParameter("folderid", folderid),
             };
            var sql_result = DBHelper.ExecuteScalar(tran, sql, sql_param);
            if (!(sql_result != null && sql_result.ToString() == "1"))
            {
                throw new Exception("目标目录不存在");
            }

            var executeSql = @"UPDATE crm_sys_documents SET folderid=@folderid, recupdator=@userno, recupdated=@recupdated WHERE documentid = @documentid; ";
            var param = new DbParameter[]
             {
                new NpgsqlParameter("documentid", documentid),
                new NpgsqlParameter("folderid", folderid),
                new NpgsqlParameter("userno", userNumber),
                new NpgsqlParameter("recupdated", recupdated),
             };
            var rows = DBHelper.ExecuteNonQuery(tran, executeSql, param);
            return rows > 0;
        }
        #endregion


        #region --批量移动文档--
        public bool MoveDocumentList(DbTransaction tran, Guid folderid, List<Guid> documentids, int userNumber)
        {
            DateTime recupdated = DateTime.Now;

            var sql = @"SELECT 1 FROM crm_sys_documents_folder WHERE folderid = @folderid AND recstatus = 1  LIMIT 1";
            var sql_param = new DbParameter[]
             {
                new NpgsqlParameter("folderid", folderid),
             };
            var sql_result = DBHelper.ExecuteScalar(tran, sql, sql_param);
            if (!(sql_result != null && sql_result.ToString() == "1"))
            {
                throw new Exception("目标目录不存在");
            }

            var executeSql = @"UPDATE crm_sys_documents SET folderid=@folderid, recupdator=@userno, recupdated=@recupdated WHERE documentid = ANY(@documentids); ";
            var param = new DbParameter[]
             {
                new NpgsqlParameter("documentids", documentids.ToArray()),
                new NpgsqlParameter("folderid", folderid),
                new NpgsqlParameter("userno", userNumber),
                new NpgsqlParameter("recupdated", recupdated),
             };
            var rows = DBHelper.ExecuteNonQuery(tran, executeSql, param);
            return rows == documentids.Count;
        }
        #endregion


        #region --删除文档--
        public bool DeleteDocument(DbTransaction tran, List<Guid> documentids, int userNumber)
        {

            DateTime recupdated = DateTime.Now;
            if (documentids == null || documentids.Count == 0)
            {
                throw new Exception("数据ID不可为空");
            }
            var executeSql = @"UPDATE crm_sys_documents SET recstatus=0, recupdator=@userno, recupdated=@recupdated WHERE documentid =ANY(@documentids); ";
            var param = new DbParameter[]
             {
                new NpgsqlParameter("documentids", documentids.ToArray()),
                new NpgsqlParameter("userno", userNumber),
                new NpgsqlParameter("recupdated", recupdated),
             };
            var rows = DBHelper.ExecuteNonQuery(tran, executeSql, param);
            return rows == documentids.Count;


        }
        #endregion



        public OperateResult DownloadDocument(string documentid, int userNumber)
        {
            var executeSql = "select * from crm_func_documents_download(@documentid,@userno)";

            var param = new DbParameter[]
             {
                    new NpgsqlParameter("documentid", documentid),

                    new NpgsqlParameter("userno", userNumber)
             };
            return DBHelper.ExecuteQuery<OperateResult>("", executeSql, param).FirstOrDefault();

        }


        public OperateResult InsertDocumentFolder(DbTransaction tran, DocumentFolderInsert data, int userNumber)
        {
            var executeSql = "select * from crm_func_documentsfolder_insert(@entityid,@foldername,@pfolderid,@isallvisible,@permissionids,@userno)";


            var param = new DbParameter[]
            {
                new NpgsqlParameter("entityid", data.EntityId),
                new NpgsqlParameter("foldername", data.Foldername),
                new NpgsqlParameter("pfolderid", data.Pfolderid),
                new NpgsqlParameter("isallvisible", data.IsAllVisible),
                new NpgsqlParameter("permissionids", string.Join(",", data.PermissionIds.ToArray())),
                new NpgsqlParameter("userno", userNumber)
            };
            return DBHelper.ExecuteQuery<OperateResult>(tran, executeSql, param).FirstOrDefault();



        }


        public OperateResult UpdateDocumentFolder(DbTransaction tran, DocumentFolderUpdate data, int userNumber)
        {
            var executeSql = "select * from crm_func_documentsfolder_update(@folderid,@foldername,@pfolderid,@isallvisible,@permissionids,@userno)";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("folderid", data.FolderId),
                new NpgsqlParameter("foldername", data.FolderName),
                new NpgsqlParameter("pfolderid", data.PfolderId),
                new NpgsqlParameter("isallvisible", data.IsAllVisible),
                new NpgsqlParameter("permissionids", string.Join(",", data.PermissionIds.ToArray())),
                new NpgsqlParameter("userno", userNumber)
            };
            return DBHelper.ExecuteQuery<OperateResult>(tran, executeSql, param).FirstOrDefault();

        }


        public dynamic DocumentFolderList(DbTransaction transaction, string ruleSql, DocumentFolderList data, int userNumber)
        {
            var executeSql = "select * from crm_func_documentsfolder_select(@entityid,@folderid,@direction,@userno,@recversion,@recstatus,@servicetype)";

            var param = new DbParameter[]
           {
                new NpgsqlParameter("entityid", data.EntityId),
                new NpgsqlParameter("folderid", data.FolderId ?? ""),
                new NpgsqlParameter("direction", data.Direction),
                new NpgsqlParameter("userno", userNumber),
                new NpgsqlParameter("recversion", data.RecVersion),
                new NpgsqlParameter("recstatus", data.RecStatus),
                new NpgsqlParameter("servicetype",  data.Servicetype)
           };
            return DBHelper.ExecuteQueryRefCursor(transaction, executeSql, param).FirstOrDefault().Value;


        }


        public OperateResult DeleteDocumentFolder(DbTransaction tran, string folderid, int userNumber)
        {
            var executeSql = "select * from crm_func_documentsfolder_delete(@folderid,@userno)";

            var param = new DbParameter[]
           {
                new NpgsqlParameter("folderid", folderid),

                new NpgsqlParameter("userno", userNumber)
           };
            return DBHelper.ExecuteQuery<OperateResult>(tran, executeSql, param).FirstOrDefault();


        }


        public bool MergeEntityDocument(DbTransaction tran, Guid entityid, Guid businessid, List<Guid> beMergeBusinessids, int usernumber)
        {

            var existSqlParameters = new List<DbParameter>();
            existSqlParameters.Add(new NpgsqlParameter("entityid", entityid));
            existSqlParameters.Add(new NpgsqlParameter("beMergeBusinessids", beMergeBusinessids.ToArray()));
            if (DBHelper.GetCount(tran, "crm_sys_documents", " entityid=@entityid AND businessid =ANY (@beMergeBusinessids) ", existSqlParameters.ToArray()) <= 0)
                return true;

            var sql = @"UPDATE crm_sys_documents  SET recupdator=@recupdator,recupdated=@recupdated,businessid=@businessid
                        WHERE entityid=@entityid AND businessid =ANY (@beMergeBusinessids)";

            var sqlParameters = new List<DbParameter>();

            sqlParameters.Add(new NpgsqlParameter("recupdator", usernumber));
            sqlParameters.Add(new NpgsqlParameter("recupdated", DateTime.Now));
            sqlParameters.Add(new NpgsqlParameter("businessid", businessid));
            sqlParameters.Add(new NpgsqlParameter("entityid", entityid));
            sqlParameters.Add(new NpgsqlParameter("beMergeBusinessids", beMergeBusinessids.ToArray()));

            var result = DBHelper.ExecuteNonQuery(tran, sql, sqlParameters.ToArray());
            return result > 0;
        }
    }
}
