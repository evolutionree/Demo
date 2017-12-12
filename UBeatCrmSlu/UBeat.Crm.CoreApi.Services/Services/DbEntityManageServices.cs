using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.DbManage;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class DbEntityManageServices: EntityBaseServices
    {
        public readonly IDbEntityManageRepository _dbEntityManageRepository;
        public DbEntityManageServices(IDbEntityManageRepository dbEntityManageRepository){
            this._dbEntityManageRepository = dbEntityManageRepository; 
        }
        public DbEntityInfo  ReflectEntity(DbEntityReflectParamInfo paramInfo,int userId) {
            DbTransaction tran = null;
            if (paramInfo == null) paramInfo = new DbEntityReflectParamInfo();
            if (paramInfo.FieldConfigInfo == null) paramInfo.FieldConfigInfo = new DbEntityReflectConfigParam();
            paramInfo.FieldConfigInfo.IsReflectDeleted = true;
              DbEntityInfo entityInfo = this._dbEntityManageRepository.getEntityInfo(paramInfo.EntityId, userId, tran);
            List<DbEntityFieldInfo> fields = this._dbEntityManageRepository.getEntityFields(paramInfo.EntityId, paramInfo.FieldConfigInfo, userId, tran);
            entityInfo.Fields = fields;
            return entityInfo;
        }
        /// <summary>
        /// 导入并更新数据库
        /// </summary>
        /// <param name="entityInfo"></param>
        /// <param name="userId"></param>
        public void ImportEntity(DbEntityInfo entityInfo,int userId) {
            DbTransaction tran = null;
            DbEntityImportParamInfo importParamInfo = DefaultImportParamInfo();
            //检查实体是否存在
            DbEntityInfo curEntityInfo = this._dbEntityManageRepository.getEntityInfo(entityInfo.EntityId.ToString(), userId, tran);
            if (curEntityInfo == null)
            {
                this._dbEntityManageRepository.addEntityInfo(entityInfo, userId, tran);
            }
            else {
                if (importParamInfo.EntityUpdatePolicy == DbEntityImportEntityUpdatePolicyEnum.CancelIfExists)
                {
                    //更新策略标志不用修改
                    return;
                }
                this._dbEntityManageRepository.updateEntityInfo(entityInfo, userId, tran);
            }
        }
        private DbEntityImportParamInfo DefaultImportParamInfo() {
            DbEntityImportParamInfo paramInfo = new DbEntityImportParamInfo();
            paramInfo.EntityUpdatePolicy = DbEntityImportEntityUpdatePolicyEnum.UpdateAll;
            paramInfo.FieldNameUpdatePolicy = DbEntityImportFieldNameUpdatePolicyEnum.UpdateNewValue;
            paramInfo.FieldTypeUpdatePolicy = DbEntityImportFieldTypeUpdatePolicyEnum.ForceUpdate;
            paramInfo.FieldUpdatePolicy = DbEntityImportFieldUpdatePolicyEnum.UpdateAll;
            return paramInfo;
        }
    }
}
