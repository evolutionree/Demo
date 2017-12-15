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
        public void ImportEntity(DbEntityInfo entityInfo, DbEntityImportParamInfo importParamInfo, int userId) {
            StringBuilder sb = new StringBuilder();
            DbTransaction tran = null;
            if (importParamInfo == null ) importParamInfo = DefaultImportParamInfo();
            //检查实体是否存在
            #region 处理实体信息
            sb.AppendLine("-----------------开始处理实体-----------------");
            sb.AppendLine("EntityId:" + entityInfo.EntityId.ToString());
            DbEntityInfo curEntityInfo = this._dbEntityManageRepository.getEntityInfo(entityInfo.EntityId.ToString(), userId, tran);
            if (curEntityInfo == null)
            {
                sb.AppendLine("新增实体信息");
                this._dbEntityManageRepository.addEntityInfo(entityInfo, userId, tran);
                this._dbEntityManageRepository.createEmptyTableForEntity(entityInfo.EntityTable,userId, tran);
            }
            else
            {
                if (importParamInfo.EntityUpdatePolicy == DbEntityImportEntityUpdatePolicyEnum.CancelIfExists)
                {
                    sb.AppendLine("已存在，且根据策略，不更新实体信息");
                    //更新策略标志不用修改
                    return;
                }
                sb.AppendLine("更新实体信息");
                this._dbEntityManageRepository.updateEntityInfo(entityInfo, userId, tran);
                
            }
            sb.AppendLine("==================实体处理结束==================");
            #endregion 
            #region 处理entity中的
            #endregion
            #region 处理字段信息
            #endregion
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