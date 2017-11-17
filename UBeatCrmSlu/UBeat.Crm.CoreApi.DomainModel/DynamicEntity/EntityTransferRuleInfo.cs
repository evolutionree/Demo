using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.DynamicEntity
{
    public class EntityTransferRuleInfo
    {
        public Guid RecId { get; set; }
        public string RecName { get; set; }
        public string SrcEntity { get; set; }
        public string SrcEntityName { get; set; }
        public string SrcCategory { get; set; }
        public string SrcCategoryName { get; set; }
        public string DstEntity { get; set; }
        public string DstEntityName { get; set; }

    
        public string DstCategory { get; set; }
        public string DstCategoryName { get; set; }
        public int IsAutoSave { get; set; }
        public string DealWithOtherService { get; set; }
        public int IsUseForInner { get; set; }
        public string TransferJson { get; set; }
        public EntityTransferSettingInfo MapperSetting { get; set; }
        public string CheckNextTransfer { get; set; }
        /// <summary>
        /// 反写规则的json字符串
        /// </summary>
        public string WriteBack { get; set; }
        /// <summary>
        /// 解析之后的反写规则
        /// </summary>
        public List<EntityTransferWriteBackRuleInfo> WriteBackRules { get; set; }

        public EntityTransferRuleInfo parseAllJsonString() {
            if (TransferJson != null && TransferJson.Length > 0) {
                try
                {
                    MapperSetting = Newtonsoft.Json.JsonConvert.DeserializeObject<EntityTransferSettingInfo>(TransferJson);
                }
                catch (Exception ex) {

                }
            }
            if (WriteBack != null && WriteBack.Length > 0)
            {
                try
                {
                    WriteBackRules = Newtonsoft.Json.JsonConvert.DeserializeObject<List<EntityTransferWriteBackRuleInfo>>(WriteBack);
                }
                catch (Exception ex) {
                }
            }
            return this;
        }
    }

    public class EntityTransferWriteBackRuleInfo {
        public int Index { get; set; }
        public int WriteBackAction { get; set; }//目前仅支持1=直接赋值
        public string WriteBackFieldName { get; set; }
        public string WriteBackValue { get; set; }
        public string RuleId { get; set; }//标记哪些是需要回写的。
    }
    public class EntityTransferSettingInfo {
        public Dictionary<string, EntityFieldMapperInfo> MainMappers { get; set; }
        public Dictionary<string, EntityTransferTableSettingInfo> EntriesMappers { get; set; }
    }
    public class EntityTransferTableSettingInfo {
        public string MappingTables { get; set; }
        public Dictionary<string, EntityFieldMapperInfo> Mappers { get; set; }
    }
    public enum EntityFieldMapperType {
        MapperEqual = 1,
        ParamReplace = 2,
        ExecuteSQL = 3,
        ExecuteFunc = 4,
        ExecuteService = 5,
        ClearValue=6
    }
    public class EntityFieldMapperInfo {
        public string DstFieldName { get; set; }
        public bool ResultIsDict { get; set; }
        public int DstFieldType { get; set; }
        public EntityFieldMapperType CalcType { get; set; }
        public string SrcFieldName { get; set; }
        public string ParamReplaceText { get; set; }
        public string CommandText { get; set; }
        public bool isSrcFieldNotNull { get; set; }
    }

    public class EntityTransferRuleQueryModel {
        public string SrcEntityId{ get; set; }
        public string SrcCategoryId { get; set; }
        public string SrcRecId { get; set; }
        public string DstEntityId { get; set; }
        public string DstCategoryId{ get; set; }
        public int IsInner { get; set; }
    }
    public class EntityTransferActionModel {
        public string SrcEntityId { get; set; }
        public string SrcRecId { get; set; }
        public string RuleId { get; set; }
    }
}
