using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.DbManage
{
    public class DbEntityParamMapper
    {

    }

    public class DbEntityReflectParamInfo {
        public string EntityId { get; set; }
        public DbEntityReflectConfigParam FieldConfigInfo { get; set; }


    }
    public class DbEntityReflectConfigParam {
        public bool IsReflectDeleted { get; set; }
    }


    public class DbEntityImportParamInfo {

        public Guid EntityId { get; set; }
        public DbEntityImportEntityUpdatePolicyEnum EntityUpdatePolicy { get; set; }
        public DbEntityImportFieldUpdatePolicyEnum FieldUpdatePolicy { get; set; }
        public DbEntityImportFieldNameUpdatePolicyEnum FieldNameUpdatePolicy { get; set; }
        public DbEntityImportFieldTypeUpdatePolicyEnum FieldTypeUpdatePolicy { get; set; }



    }

    /// <summary>
    /// 实体字段更新策略
    /// </summary>
    public enum DbEntityImportEntityUpdatePolicyEnum
    {
        /// <summary>
        /// 更新
        /// </summary>
        UpdateAll =1 ,
        /// <summary>
        /// 存在则不更新
        /// </summary>
        CancelIfExists = 0
    }
    /// <summary>
    /// 字段更新策略
    /// </summary>
    public enum DbEntityImportFieldUpdatePolicyEnum {
        /// <summary>
        /// 存在则不更新
        /// </summary>
        CancelIfExists = 0,
        /// <summary>
        /// 更新，按字段属性更新策略
        /// </summary>
        UpdateAll = 1

    }

    /// <summary>
    /// 字段名称更新策略
    /// </summary>
    public enum DbEntityImportFieldNameUpdatePolicyEnum {
        KeepOldValue =0,
        UpdateNewValue=1
    }
    /// <summary>
    /// 字段类型更新策略
    /// </summary>
    public enum DbEntityImportFieldTypeUpdatePolicyEnum {
        /// <summary>
        /// 不更新
        /// </summary>
        NoUpdate = 0,
        /// <summary>
        /// 不更新，但是如果不一致则直接报错
        /// </summary>
        RaiseErrorIfDif = 1,
        /// <summary>
        /// 发现不一致，则强制更新，如果更新失败（涉及数据），则提示
        /// </summary>
        ForceUpdate = 2
        
    }
    public class EntityExportParamInfo {
        public Guid EntityId { get; set; }
    }
    /// <summary>
    /// 数据存储大小统计
    /// </summary>
    public class DbSizeStatInfo {
        public string DbName { get; set; }
        public long PgSize { get; set; }
        public string PgSizeName { get; set; }
        public long MongoDbSize { get; set; }
        public string MongoDbSizeName { get; set; }
    }

}
