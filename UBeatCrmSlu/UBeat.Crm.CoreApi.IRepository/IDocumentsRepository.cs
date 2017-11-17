using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Documents;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IDocumentsRepository : IBaseRepository
    {
        Guid? InsertDocument(DbTransaction transaction, DocumentInsert data, int userNumber);

        DocumentInfo GetDocumentInfo(Guid documentid);

        dynamic GetAllDocumentList(DbTransaction transaction,string ruleSql, DocumentList data, int userNumber);

        dynamic DocumentList(DbTransaction transaction, string ruleSql, PageParam pageParam, DocumentList data, int userNumber);

        bool MoveDocument(DbTransaction tran, DocumentMove data, int userNumber);

        bool MoveDocumentList(DbTransaction tran,Guid folderid, List<Guid> documentids, int userNumber);

        bool DeleteDocument(DbTransaction tran, List<Guid> documentids, int userNumber);

        OperateResult DownloadDocument(string documentid, int userNumber);

        OperateResult InsertDocumentFolder(DbTransaction tran, DocumentFolderInsert data, int userNumber);

        OperateResult UpdateDocumentFolder(DbTransaction tran, DocumentFolderUpdate data, int userNumber);

        dynamic DocumentFolderList(DbTransaction transaction, string ruleSql,DocumentFolderList data, int userNumber);

        OperateResult DeleteDocumentFolder(DbTransaction tran, string folderid, int userNumber);


        bool MergeEntityDocument(DbTransaction tran, Guid entityid, Guid businessid, List<Guid> beMergeBusinessids, int usernumber);

    }
}
