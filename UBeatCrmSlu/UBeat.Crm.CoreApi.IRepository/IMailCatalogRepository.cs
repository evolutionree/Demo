﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.EMail;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IMailCatalogRepository : IBaseRepository
    {

        List<MailCatalogInfo> GetMailCataLog(string catalogType, string vpid,string keyword, int userId);

        List<MailCatalogInfo> GetMailCataLogTreeByKeyword(string keyword, string catalogType, int userid);

        List<OrgAndStaffTree> GetOrgAndStaffTreeByLevel(int userId, string deptId, string keyword);

        MailCatalogInfo GetMailCatalogByCode(int userId, string catalogType);

        List<Dictionary<string, object>> GetDefaultCatalog(int userId);
        int InitCatalog(Dictionary<string, object> newCatalog, int userId);
        int NeedInitCatalog(int userId);
        OperateResult SaveMailOwner(List<Guid> MailBoxs, int newUserId, int userId, DbTransaction dbTrans = null);
        void SaveWhiteList(List<Guid> MailBoxs, string enable);
        void MailServerEnable(List<Guid> MailServers);
        void MailServerUnEnable(List<Guid> MailServers);

        OperateResult ToOrderCatalog(Guid recId, int doType);

        OperateResult InsertCatalog(CUMailCatalogMapper entity, int userId);

        OperateResult EditCatalog(CUMailCatalogMapper entity, int userId);

        OperateResult DeleteCatalog(DeleteMailCatalogMapper entity, int userId);

        OperateResult OrderbyCatalog(DbTransaction trans, OrderByMailCatalogMapper entity, int userId);
        /// <summary>
        /// 获取目录信息，不考虑权限
        /// </summary>
        /// <param name="catalog"></param>
        /// <param name="userNum"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        MailCatalogInfo GetMailCataLogById(Guid catalog, int userNum, DbTransaction p);

        MailCatalogInfo GetMailCataLogByViewUserId(Guid catalog, int userNum, DbTransaction p);
        MailCatalogInfo GetMailCataLogByCustId(Guid custId, int userid, DbTransaction p);
        UserMailInfo GetUserMailInfo(string fromAddress, int userId);

        IList<UserMailInfo> GetAllUserMail(int deviceType, int userId);
        bool checkHasMails(string recid, DbTransaction tran);
        bool checkCycleCatalog(string parentid, string recid, DbTransaction tran);
        void MoveCatalog(string v1, string v2, string v3, DbTransaction tran);
        MailCatalogInfo GetCatalogForCustType(Guid custCatalog, int newUserId, DbTransaction tran);
        void TransferCatalog(Guid recId, int newUserId, Guid newParentCatalogid, DbTransaction tran);
        void TransferMailsToNewCatalog(Guid newCatalogid, Guid oldCatalogid, DbTransaction tran);
        void TransferBatcCatalog(int newUserId, int oldUserId, Guid newParentCatalogid, int ctype, DbTransaction tran);
    }
}
