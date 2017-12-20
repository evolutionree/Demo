using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.DbManage;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IDbEntityManageRepository
    {
        DbEntityInfo getEntityInfo(string entityid, int userId, DbTransaction tran);
        List<DbEntityFieldInfo> getEntityFields(string entityid, DbEntityReflectConfigParam configParam, int userId, DbTransaction tran);
        string addEntityInfo(DbEntityInfo entityInfo, int userId, DbTransaction tran);
        void updateEntityInfo(DbEntityInfo entityInfo, int userId, DbTransaction tran);

        string createEmptyTableForEntity(string tablename, int userId, DbTransaction tran);
        List<DbEntityCatelogInfo> getCatelogs(string entityId, string[] exportCatelogIds, int userId, DbTransaction tran);
        List<DbEntityComponentConfigInfo> getComponentConfigList(string entityId, int userId, DbTransaction tran);
        List<DbEntityWebFieldInfo> getWebFieldList(string entityId, int userId, DbTransaction tran);
        DbEntityMobileListConfigInfo getMobileColumnConfig(string entityId, int userId, DbTransaction tran);
    }
}
