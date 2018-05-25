using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.Services.Utility.ExcelUtility;
using System.Linq;
using DocumentFormat.OpenXml;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models.EntityPro;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.DomainModel.EntityPro;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.Services.Models.DataSource;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class EntityExcelImportServices : EntityBaseServices
    {
        public static string[] DefaultFieldName = new string[] { "recid", "recname", "recmananger", "reccreator", "reccreated", "recupdator", "recupdated", "recstatus", "recversion" };
        private Dictionary<string, int> DictInDb = new Dictionary<string, int>();
        private Dictionary<string, int> DictNeedCreate = new Dictionary<string, int>();
        private List<Dictionary<string, object>> DictValueNeedCreate = new List<Dictionary<string, object>>();
        private Dictionary<string, Dictionary<string, object>> DataSourceInDb = new Dictionary<string, Dictionary<string, object>>();
        private Dictionary<string, string> DataSourceNeedCreate = new Dictionary<string, string>();
        private Dictionary<string, ExcelEntityInfo> dictEntity = null;
        private Dictionary<string, string> dictTable = null;
        private readonly IEntityProRepository _entityProRepository;
        private readonly IDataSourceRepository _dataSourceRepository;
        private readonly EntityProServices _entityProServices;
        private readonly DataSourceServices _dataSourceServices;

        public EntityExcelImportServices(IEntityProRepository entityProRepository,
                            IDataSourceRepository dataSourceRepository,
                            EntityProServices entityProServices,
                            DataSourceServices dataSourceServices)
        {
            _entityProRepository = entityProRepository;
            _dataSourceRepository = dataSourceRepository;
            _entityProServices = entityProServices;
            _dataSourceServices = dataSourceServices;
        }

        #region 通过Excel导入实体配置,这里将会是一个大的代码块 
        public Dictionary<string,object> ImportEntityFromExcel()
        {
            Dictionary<string, object> retDict = new Dictionary<string, object>();
            //this._excelServices
            List<ExcelEntityInfo> listEntity = new List<ExcelEntityInfo>();
            retDict.Add("entities", listEntity);
            System.IO.Stream r = new System.IO.FileStream(@"d:\配置导入示例.xlsx", System.IO.FileMode.Open);
            WorkbookPart workbookPart;
            try
            {
                var document = SpreadsheetDocument.Open(r, false);
                workbookPart = document.WorkbookPart;
                

                foreach (var sheet in workbookPart.Workbook.Descendants<Sheet>()) {
                    ExcelEntityInfo entityInfo = DealWithOneSheet(workbookPart, sheet);
                    listEntity.Add(entityInfo);
                }
                CheckEntityAndField(listEntity);
                //这里检查一下是否有问题，如果有问题就不继续了
                if (IsPreCheckOK(listEntity) == false) {
                    retDict.Add("result", -1);
                    return retDict;
                }

                //先建立所有相关表的实体
                CreateAllMainEntityInfo(listEntity);
                if (IsPreCheckOK(listEntity) == false)
                {
                    retDict.Add("result", -1);
                    return retDict;
                }
                //处理所有的数据源
                List<ExcelDataSourceInfo> datasourceList=  CreateAllDataSource();
                retDict.Add("datasource", datasourceList);
                if (IsPreCheckOK(datasourceList) == false) {
                    retDict.Add("result", -1);
                    return retDict;
                }
                //处理所有的字典
                List<ExcelDictTypeInfo> listDictType = CreateAllDictType();
                retDict.Add("dicttype", listDictType);
                if (IsPreCheckOK(listDictType) == false) {
                    retDict.Add("result", -1);
                    return retDict;
                }
                CreateAllDict();
                //处理所有的字段
                CreateAllFieldInfo(listEntity);
                if (IsPreCheckOK(listEntity) == false)
                {
                    retDict.Add("result", -1);
                    return retDict;
                }
                UpdateReferenceAndTableFieldInfo(listEntity);
                //创建分类
                CreateAllCatelog(listEntity);
                //处理所有的字段可见规则
                UpdateAllViewSetting(listEntity);
                //处理WEB列表查看
                SetWebListColumnView(listEntity);
                //处理查重条件
                SetCheckSameCondition(listEntity);
                retDict.Add("result", 0);
                return retDict;

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                r.Close();
            }
        }
        public string GenerateTotalMessage(Dictionary<string, object> result) {
            StringBuilder sb = new StringBuilder();
            List<ExcelEntityInfo> listEntity = (List<ExcelEntityInfo>)result["entities"];
            foreach (ExcelEntityInfo entityInfo in listEntity) {
                if (entityInfo.actionResult.ResultCode < 0) {
                    sb.Append("实体【" + entityInfo.EntityName + "】发生错误:" + entityInfo.actionResult.Message);
                }
                foreach (ExcelEntityColumnInfo fieldInfo in entityInfo.Fields) {
                    if (fieldInfo.actionResult.ResultCode < 0)
                        sb.Append("实体【" + entityInfo.EntityName + "】的字段【"+fieldInfo.DisplayName+"】发生错误:" + fieldInfo.actionResult.Message);
                }
                foreach (ExcelEntityInfo subEntityInfo in entityInfo.SubEntitys) {
                    if (subEntityInfo.actionResult.ResultCode < 0)
                    {
                        sb.Append("实体【" + subEntityInfo.EntityName + "】发生错误:" + subEntityInfo.actionResult.Message);
                    }
                    foreach (ExcelEntityColumnInfo fieldInfo in subEntityInfo.Fields)
                    {
                        if (fieldInfo.actionResult.ResultCode<0 )
                            sb.Append("实体【" + subEntityInfo.EntityName + "】的字段【" + fieldInfo.DisplayName + "】发生错误:" + fieldInfo.actionResult.Message);
                    }
                }
            }
            
            List<ExcelDictTypeInfo> dicttypes = (List<ExcelDictTypeInfo>)result["dicttype"];
            foreach (ExcelDictTypeInfo info in dicttypes) {
                if (info.actionResult.ResultCode < 0) {
                    sb.Append("字典类型【" + info.DictTypeName + "】发生错误：" + info.actionResult.Message);
                }
            }
            List<ExcelDataSourceInfo> datasources = (List<ExcelDataSourceInfo>)result["datasource"];
            foreach (ExcelDataSourceInfo info in datasources)
            {
                if (info.actionResult.ResultCode < 0)
                {
                    sb.Append("字典类型【" + info.DataSourceName + "】发生错误：" + info.actionResult.Message);
                }
            }
            return sb.ToString();
        }


        private bool IsPreCheckOK(List<ExcelDictTypeInfo> listDictType)
        {
            foreach (ExcelDictTypeInfo dictype in listDictType)
            {
                if (dictype.actionResult.ResultCode < 0) return false;
            }
            return true;
        }
        private bool IsPreCheckOK(List<ExcelDataSourceInfo> listDataSource)
        {
            foreach (ExcelDataSourceInfo datasourceInfo in listDataSource)
            {
                if (datasourceInfo.actionResult.ResultCode < 0) return false;
            }
            return true;
        }
        private bool IsPreCheckOK(List<ExcelEntityInfo> listEntity) {
            foreach (ExcelEntityInfo entityInfo in listEntity) {
                if (PreCheckEntity(entityInfo) == false) return false; 
            }
            return true;
        }
        private bool PreCheckEntity(ExcelEntityInfo entityInfo) {
            if (entityInfo.actionResult.ResultCode < 0) return false;
            foreach (ExcelEntityColumnInfo fieldInfo in entityInfo.Fields) {
                if (fieldInfo.actionResult.ResultCode < 0) return false;
            }
            if (entityInfo.SubEntitys != null && entityInfo.SubEntitys.Count > 0)
            {
                return IsPreCheckOK(entityInfo.SubEntitys);
            }
            return true;
        }

        private void SetCheckSameCondition(List<ExcelEntityInfo> listEntry) {
            foreach (ExcelEntityInfo entityInfo in listEntry) {
                if (entityInfo.EntityTypeName.StartsWith("简单") || entityInfo.EntityTypeName.StartsWith("独立"))
                {
                    List<string> ids = new List<string>();
                    foreach (ExcelEntityColumnInfo fieldInfo in entityInfo.Fields)
                    {
                        if (fieldInfo.FieldId != null && fieldInfo.FieldId != Guid.Empty && fieldInfo.IsWebListField)
                        {
                            ids.Add(fieldInfo.FieldId.ToString());
                        }
                    }
                    //只有独立和简单实体才有查重条件
                    this._entityProRepository.SaveSetRepeat(entityInfo.EntityId.ToString(), string.Join(',', ids.ToArray()), 1);
                }
            }
        }

        private void SetWebListColumnView(List<ExcelEntityInfo> listEntry){
            foreach (ExcelEntityInfo entityInfo in listEntry) {
                List<string> ids = new List<string>();
                foreach (ExcelEntityColumnInfo fieldInfo in entityInfo.Fields) {
                    if (fieldInfo.FieldId != null && fieldInfo.FieldId != Guid.Empty && fieldInfo.IsWebListField) {
                        ids.Add(fieldInfo.FieldId.ToString());
                    }
                }
                SaveListViewColumnMapper mapper = new SaveListViewColumnMapper() {
                    ViewType = 0,
                    EntityId = entityInfo.EntityId.ToString(),
                    FieldIds = string.Join(',', ids.ToArray())
                };
                this._entityProRepository.SaveWebFieldVisible(mapper, 1);
                if (entityInfo.SubEntitys != null && entityInfo.SubEntitys.Count > 0) {
                    SetWebListColumnView(entityInfo.SubEntitys);
                }
            }
        }
        private void UpdateReferenceAndTableFieldInfo(List<ExcelEntityInfo> listEntry) {
            foreach (ExcelEntityInfo entityInfo in listEntry) {
                foreach (ExcelEntityColumnInfo fieldInfo in entityInfo.Fields) {
                    if (fieldInfo.IsUpdate) continue;
                    if (fieldInfo.ControlType != 23 && fieldInfo.ControlType != 31) continue;
                    if (fieldInfo.ControlType == 23)
                    {
                        //表格控件
                        if (fieldInfo.Table_EntityId == null || fieldInfo.Table_EntityId.Length == 0)
                        {
                            checkOneField(fieldInfo, entityInfo, dictEntity, dictTable);
                            this._entityProRepository.UpdateEntityFieldConfig(fieldInfo.FieldId, fieldInfo.FieldConfig, 1);
                        }
                    }
                    else if (fieldInfo.ControlType == 31)
                    {
                        //引用控件
                        if (fieldInfo.RelField_ControlFieldId == null || fieldInfo.RelField_ControlFieldId.Length == 0
                            || fieldInfo.RelField_OriginEntityId == null || fieldInfo.RelField_OriginEntityId.Length == 0
                            || fieldInfo.RelField_originFieldId == null || fieldInfo.RelField_originFieldId.Length == 0) {
                            checkOneField(fieldInfo, entityInfo, dictEntity, dictTable);
                            this._entityProRepository.UpdateEntityFieldConfig(fieldInfo.FieldId, fieldInfo.FieldConfig, 1);
                        }
                    }
                }
            }
        }
        private void SaveFieldConfig(Guid fieldid, string fieldconfig) {
            
        }
        private void UpdateAllViewSetting(List<ExcelEntityInfo> listEntities)
        {
            foreach (ExcelEntityInfo entityInfo in listEntities)
            {
                Dictionary<string, List<IDictionary<string, object>>> dataDict=this._entityProRepository.EntityFieldProQuery(entityInfo.EntityId.ToString(), 1);
                List<IDictionary<string, object>> data = dataDict["EntityFieldPros"];
                int catelogcount = entityInfo.TypeNameDict.Count;
                for (int  i= 0; i < catelogcount; i++) {
                    foreach (Guid catelogid in entityInfo.TypeNameDict[i.ToString()].CatelogIds) {
                        List<EntityFieldRulesSaveModel> ll = new List<EntityFieldRulesSaveModel>();
                        foreach (IDictionary<string, object> field in data) {
                            EntityFieldRulesSaveModel item = new EntityFieldRulesSaveModel();
                            string fieldId = field["fieldid"].ToString();

                            ExcelEntityColumnInfo fieldInfo = null;
                            try
                            {
                                fieldInfo = entityInfo.Fields.Where(o => o.FieldId.Equals(Guid.Parse(fieldId))).First();
                            }
                            catch (Exception ex) { }
                            item.Rules = new List<FieldRulesDetailModel>();
                            item.RecStatus = 1;
                            if (fieldInfo == null)
                            {
                                item.FieldId = field["fieldid"].ToString();
                                item.FieldLabel = field["fieldlabel"].ToString();
                                item.RecStatus = 1;
                                //操作类型 新增 编辑 详情 列表 导入分别是0 1 2 3 4
                                FieldRulesDetailModel addnew = new FieldRulesDetailModel()
                                {
                                    OperateType = 0,
                                    IsReadOnly = 0,
                                    IsRequired = 0,
                                    IsVisible = 0 
                                };
                                CalcFieldViewRule(addnew);
                                item.Rules.Add(addnew);
                                FieldRulesDetailModel edit = new FieldRulesDetailModel()
                                {
                                    OperateType = 1,
                                    IsReadOnly = 0,
                                    IsRequired = 0,
                                    IsVisible = 0
                                };
                                CalcFieldViewRule(edit);
                                item.Rules.Add(edit);
                                FieldRulesDetailModel view = new FieldRulesDetailModel()
                                {
                                    OperateType = 2,
                                    IsReadOnly = 0,
                                    IsRequired = 0,
                                    IsVisible = 1
                                };
                                CalcFieldViewRule(view);
                                item.Rules.Add(view);
                                if (entityInfo.TypeNameDict[i.ToString()].IsImport) {
                                    FieldRulesDetailModel import = new FieldRulesDetailModel()
                                    {
                                        OperateType = 4,
                                        IsReadOnly = 0,
                                        IsRequired = 0,
                                        IsVisible = 0
                                    };
                                    CalcFieldViewRule(import);
                                    item.Rules.Add(import);
                                }

                            }
                            else
                            {
                                item.FieldId = fieldInfo.FieldId.ToString();
                                item.FieldLabel = fieldInfo.DisplayName;
                                item.RecStatus = 1;
                                ExcelEntityFieldViewInfo viewtype = fieldInfo.ViewSet[entityInfo.TypeNameDict[i.ToString()].TypeName];
                                //操作类型 新增 编辑 详情 列表 导入分别是0 1 2 3 4
                                FieldRulesDetailModel addnew = new FieldRulesDetailModel()
                                {
                                    OperateType = 0,
                                    IsReadOnly = viewtype.AddNew_Readonly ? 1 : 0,
                                    IsRequired = viewtype.AddNew_Must ? 1 : 0,
                                    IsVisible = viewtype.AddNew_Display ? 1 : 0,

                                };
                                CalcFieldViewRule(addnew);
                                item.Rules.Add(addnew);
                                FieldRulesDetailModel edit = new FieldRulesDetailModel()
                                {
                                    OperateType =1,
                                    IsReadOnly = viewtype.Edit_Readonly ? 1 : 0,
                                    IsRequired = viewtype.Edit_Must ? 1 : 0,
                                    IsVisible = viewtype.Edit_Display ? 1 : 0
                                };
                                CalcFieldViewRule(edit);
                                item.Rules.Add(edit);
                                FieldRulesDetailModel view = new FieldRulesDetailModel()
                                {
                                    OperateType =2,
                                    IsReadOnly = 0,
                                    IsRequired = 0,
                                    IsVisible = viewtype.View_Display ? 1 : 0
                                };
                                CalcFieldViewRule(view);
                                item.Rules.Add(view);
                                if (entityInfo.TypeNameDict[i.ToString()].IsImport)
                                {
                                    FieldRulesDetailModel import = new FieldRulesDetailModel()
                                    {
                                        OperateType = 4,
                                        IsReadOnly = 0,
                                        IsRequired = viewtype.Import_Must ? 1 : 0,
                                        IsVisible = viewtype.Import_Display ? 1 : 0
                                    };
                                    CalcFieldViewRule(import);
                                    item.Rules.Add(import);
                                }
                            }
                            item.TypeId = catelogid.ToString();
                            ll.Add(item);
                        }
                        this._entityProRepository.DeleteEntityFieldRules(catelogid, 1);
                        this._entityProServices.SaveEntityFieldRules(ll, 1);
                    } 
                }
                if (entityInfo.SubEntitys == null) continue;
                foreach (ExcelEntityInfo subEntityInfo in entityInfo.SubEntitys)
                {
                    catelogcount = subEntityInfo.TypeNameDict.Count;
                    for (int i = 0; i < catelogcount; i++)
                    {
                        foreach (Guid catelogid in subEntityInfo.TypeNameDict[i.ToString()].CatelogIds)
                        {
                            List<EntityFieldRulesSaveModel> ll = new List<EntityFieldRulesSaveModel>();
                            foreach (IDictionary<string, object> field in data)
                            {
                                EntityFieldRulesSaveModel item = new EntityFieldRulesSaveModel();
                                string fieldId = field["fieldid"].ToString();

                                ExcelEntityColumnInfo fieldInfo = null;
                                try
                                {
                                    fieldInfo = subEntityInfo.Fields.Where(o => o.FieldId.Equals(Guid.Parse(fieldId))).First();
                                }
                                catch (Exception ex) { }
                                item.Rules = new List<FieldRulesDetailModel>();
                                item.RecStatus = 1;
                                if (fieldInfo == null)
                                {
                                    item.FieldId = field["fieldid"].ToString();
                                    item.FieldLabel = field["fieldlabel"].ToString();
                                    item.RecStatus = 1;
                                    //操作类型 新增 编辑 详情 列表 导入分别是0 1 2 3 4
                                    FieldRulesDetailModel addnew = new FieldRulesDetailModel()
                                    {
                                        OperateType = 0,
                                        IsReadOnly = 0,
                                        IsRequired = 0,
                                        IsVisible = 0
                                    };
                                    CalcFieldViewRule(addnew);
                                    item.Rules.Add(addnew);
                                    FieldRulesDetailModel edit = new FieldRulesDetailModel()
                                    {
                                        OperateType = 1,
                                        IsReadOnly = 0,
                                        IsRequired = 0,
                                        IsVisible = 0
                                    };
                                    CalcFieldViewRule(edit);
                                    item.Rules.Add(edit);
                                    FieldRulesDetailModel view = new FieldRulesDetailModel()
                                    {
                                        OperateType = 2,
                                        IsReadOnly = 0,
                                        IsRequired = 0,
                                        IsVisible = 1
                                    };
                                    CalcFieldViewRule(view);
                                    item.Rules.Add(view);
                                    if (subEntityInfo.TypeNameDict[i.ToString()].IsImport)
                                    {
                                        FieldRulesDetailModel import = new FieldRulesDetailModel()
                                        {
                                            OperateType = 4,
                                            IsReadOnly = 0,
                                            IsRequired = 0,
                                            IsVisible = 0
                                        };
                                        CalcFieldViewRule(import);
                                        item.Rules.Add(import);
                                    }

                                }
                                else
                                {
                                    item.FieldId = fieldInfo.FieldId.ToString();
                                    item.FieldLabel = fieldInfo.DisplayName;
                                    item.RecStatus = 1;

                                    ExcelEntityFieldViewInfo viewtype = null;
                                    if (fieldInfo.ViewSet.ContainsKey(subEntityInfo.TypeNameDict[i.ToString()].TypeName))
                                    {
                                        viewtype = fieldInfo.ViewSet[subEntityInfo.TypeNameDict[i.ToString()].TypeName];
                                    }
                                    else {
                                        viewtype = fieldInfo.ViewSet.First().Value;
                                    }
                                    //操作类型 新增 编辑 详情 列表 导入分别是0 1 2 3 4
                                    FieldRulesDetailModel addnew = new FieldRulesDetailModel()
                                    {
                                        OperateType = 0,
                                        IsReadOnly = viewtype.AddNew_Readonly ? 1 : 0,
                                        IsRequired = viewtype.AddNew_Must ? 1 : 0,
                                        IsVisible = viewtype.AddNew_Display ? 1 : 0,

                                    };
                                    CalcFieldViewRule(addnew);
                                    item.Rules.Add(addnew);
                                    FieldRulesDetailModel edit = new FieldRulesDetailModel()
                                    {
                                        OperateType = 1,
                                        IsReadOnly = viewtype.Edit_Readonly ? 1 : 0,
                                        IsRequired = viewtype.Edit_Must ? 1 : 0,
                                        IsVisible = viewtype.Edit_Display ? 1 : 0
                                    };
                                    CalcFieldViewRule(edit);
                                    item.Rules.Add(edit);
                                    FieldRulesDetailModel view = new FieldRulesDetailModel()
                                    {
                                        OperateType = 2,
                                        IsReadOnly = 0,
                                        IsRequired = 0,
                                        IsVisible = viewtype.View_Display ? 1 : 0
                                    };
                                    CalcFieldViewRule(view);
                                    item.Rules.Add(view);
                                    if (subEntityInfo.TypeNameDict[i.ToString()].IsImport)
                                    {
                                        FieldRulesDetailModel import = new FieldRulesDetailModel()
                                        {
                                            OperateType = 4,
                                            IsReadOnly = 0,
                                            IsRequired = viewtype.Import_Must ? 1 : 0,
                                            IsVisible = viewtype.Import_Display ? 1 : 0
                                        };
                                        CalcFieldViewRule(import);
                                        item.Rules.Add(import);
                                    }
                                }
                                item.TypeId = catelogid.ToString();
                                ll.Add(item);
                            }
                            this._entityProRepository.DeleteEntityFieldRules(catelogid, 1);
                            this._entityProServices.SaveEntityFieldRules(ll, 1);
                        }
                    }
                }
            }
        }
        private void CalcFieldViewRule(FieldRulesDetailModel viewRule) {
            viewRule.ViewRuleStr = "{\"style\":0,\"isVisible\":" + viewRule.IsVisible + ",\"isReadOnly\":" + viewRule.IsReadOnly + "}";
            viewRule.ValidRuleStr = "{\"isRequired\":" + viewRule.IsRequired + "}";
        }
        private void CreateAllCatelog(List<ExcelEntityInfo> listEntities) {
            foreach(ExcelEntityInfo entityInfo in listEntities)
            {
                //if (entityInfo.EntityTypeName.StartsWith("独立") == false) continue;

                EntityTypeQueryMapper queryMapper = new EntityTypeQueryMapper() {
                    EntityId = entityInfo.EntityId.ToString()
                };
                Dictionary<string, List<IDictionary<string, object>>>  tmp =   this._entityProRepository.EntityTypeQuery(queryMapper, 1);
                Dictionary<string, string> typeDict = new Dictionary<string, string>();
                foreach (IDictionary<string, object> item in tmp["EntityTypePros"]) {
                    typeDict.Add(item["categoryname"].ToString(), item["categoryid"].ToString());
                } 
                int catelogcount = entityInfo.TypeNameDict.Count;
                for (int i = 0; i < catelogcount; i++) {
                    EntityTypeSettingInfo typeInfo = entityInfo.TypeNameDict[i.ToString()];
                    string[] names = typeInfo.TypeName.Split(",，".ToCharArray());
                    if (i == 0)
                    {
                        typeInfo.CatelogIds.Add(entityInfo.EntityId);
                        //更新名称
                        EntityTypeModel model = new EntityTypeModel() {
                            RecStatus = 1,
                            RecOrder = 1,
                            CategoryId = entityInfo.EntityId.ToString(),
                            CategoryName = names[0],
                            EntityId = entityInfo.EntityId.ToString()
                        };
                        this._entityProServices.UpdateEntityTypePro(model, 1);
                        for (int j = 1; j < names.Length; j++) {
                            if (typeDict.ContainsKey(names[j])) {
                                typeInfo.CatelogIds.Add(Guid.Parse(typeDict[names[j]]));
                            }
                            else
                            {
                                SaveEntityTypeMapper mapper = new SaveEntityTypeMapper()
                                {
                                    CategoryName = names[j],
                                    EntityId = entityInfo.EntityId.ToString()
                                };
                                OperateResult result = this._entityProRepository.InsertEntityTypePro(mapper, 1);
                                if (result.Flag != 1)
                                {
                                    throw (new Exception("新增分类失败:" + result.Msg));
                                }
                                typeInfo.CatelogIds.Add(Guid.Parse(result.Id));
                            }
                            
                        }

                    }
                    else {
                        for (int j = 0; j < names.Length; j++)
                        {
                            if (typeDict.ContainsKey(names[j]))
                            {
                                typeInfo.CatelogIds.Add(Guid.Parse(typeDict[names[j]]));
                            }
                            else
                            {
                                SaveEntityTypeMapper mapper = new SaveEntityTypeMapper()
                                {
                                    CategoryName = names[j],
                                    EntityId = entityInfo.EntityId.ToString()
                                };
                                OperateResult result = this._entityProRepository.InsertEntityTypePro(mapper, 1);
                                if (result.Flag != 1)
                                {
                                    throw (new Exception("新增分类失败:" + result.Msg));
                                }
                                typeInfo.CatelogIds.Add(Guid.Parse(result.Id));
                            }
                        }
                    }
                }
                #region 处理嵌套实体的分类
                if (entityInfo.SubEntitys == null) continue;
                foreach (ExcelEntityInfo subEntityInfo in entityInfo.SubEntitys)
                {
                    catelogcount = entityInfo.TypeNameDict.Count;
                    subEntityInfo.TypeNameDict = new Dictionary<string, EntityTypeSettingInfo>();
                    for (int i = 0; i < catelogcount; i++)
                    {
                        EntityTypeSettingInfo mainSetting = entityInfo.TypeNameDict[i.ToString()];
                        EntityTypeSettingInfo subSetting = new EntityTypeSettingInfo()
                        {
                            TypeName = mainSetting.TypeName,
                            IsImport = mainSetting.IsImport
                        };
                        subEntityInfo.TypeNameDict.Add(i.ToString(), subSetting);
                        string[] names = subSetting.TypeName.Split(",，".ToCharArray());
                        if (i == 0)
                        {
                            subSetting.CatelogIds.Add(entityInfo.EntityId);
                            //更新名称
                            EntityTypeModel model = new EntityTypeModel()
                            {
                                RecStatus = 1,
                                RecOrder = 1,
                                CategoryId = subEntityInfo.EntityId.ToString(),
                                CategoryName = names[0],
                                EntityId = subEntityInfo.EntityId.ToString()
                            };
                            this._entityProServices.UpdateEntityTypePro(model, 1);
                            this._entityProRepository.MapEntityType(subEntityInfo.EntityId, entityInfo.EntityId);
                            for (int j = 1; j < names.Length; j++)
                            {
                                if (typeDict.ContainsKey(names[j]))
                                {
                                    subSetting.CatelogIds.Add(Guid.Parse(typeDict[names[j]]));
                                }
                                else
                                {
                                    SaveEntityTypeMapper mapper = new SaveEntityTypeMapper()
                                    {
                                        CategoryName = names[j],
                                        EntityId = subEntityInfo.EntityId.ToString()
                                    };
                                    OperateResult result = this._entityProRepository.InsertEntityTypePro(mapper, 1);
                                    if (result.Flag != 1)
                                    {
                                        throw (new Exception("新增分类失败:" + result.Msg));
                                    }
                                    subSetting.CatelogIds.Add(Guid.Parse(result.Id));
                                }
                            }

                        }
                        else
                        {
                            for (int j = 0; j < names.Length; j++)
                            {
                                if (typeDict.ContainsKey(names[j]))
                                {
                                    subSetting.CatelogIds.Add(Guid.Parse(typeDict[names[j]]));
                                }
                                else
                                {
                                    SaveEntityTypeMapper mapper = new SaveEntityTypeMapper()
                                    {
                                        CategoryName = names[j],
                                        EntityId = subEntityInfo.EntityId.ToString()
                                    };
                                    OperateResult result = this._entityProRepository.InsertEntityTypePro(mapper, 1);
                                    if (result.Flag != 1)
                                    {
                                        throw (new Exception("新增分类失败:" + result.Msg));
                                    }
                                    subSetting.CatelogIds.Add(Guid.Parse(result.Id));
                                }
                            }
                        }
                    }

                }
                #endregion 
            }
        }
        private void CreateAllDict() {
            foreach (Dictionary<string, object> item in DictValueNeedCreate) {
                int dictypeid = 0;
                string dictypename = "";
                string dictname = "";
                int dictid = 0;
                if (item.ContainsKey("dictypeid")) {
                    dictypeid = (int)item["dictypeid"];
                }
                if (item.ContainsKey("dictypename")) {
                    dictypename = (string)item["dictypename"];
                }
                dictname = (string)item["dictname"];
                if (dictypeid <= 0) {
                    if (DictInDb.ContainsKey(dictypename))
                    {
                        dictypeid = DictInDb[dictypename];
                    }
                    else {
                        throw (new Exception("配置异常"));
                    }
                }
                dictid = this._dataSourceRepository.GetDictValueByName(dictypeid, dictname);
                if (dictid <= 0) {
                    DictionaryMapper model = new DictionaryMapper() {
                        DicTypeId = dictypeid,
                        DataValue = dictname
                    };
                    this._dataSourceRepository.SaveFieldOptValue(model,1);
                }
            }
        }
        private List<ExcelDictTypeInfo>  CreateAllDictType() {
            List<ExcelDictTypeInfo> ret = new List<ExcelDictTypeInfo>();
            foreach (string dictypename in DictNeedCreate.Keys) {
                DictionaryTypeModel model = new DictionaryTypeModel()
                {
                    DicTypeName = dictypename,
                    RecStatus = 1,
                    DicRemark = "导入系统自动创建"
                };
                ExcelDictTypeInfo dictTypeInfo = new ExcelDictTypeInfo();
                dictTypeInfo.DictTypeName = dictypename;
                ret.Add(dictTypeInfo);
                OutputResult<object>  r =  this._dataSourceServices.SaveFieldDicType(model, 1);
                if (r.Status != 0) {
                    dictTypeInfo.actionResult = new ActionResult()
                    {
                        ActionType  = 1,
                        ResultCode = -2,
                        Message = "创建字典类型失败"
                    };
                    continue;
                }

                Dictionary<string, object>  tmp = this._dataSourceRepository.GetDictTypeByName(dictypename);
                if (tmp == null || tmp.Count == 0) {
                    dictTypeInfo.actionResult = new ActionResult()
                    {
                        ActionType = 1,
                        ResultCode = -2,
                        Message = "创建字典类型失败"
                    };
                    continue;
                }
                int dictypeid = int.Parse(tmp["dictypeid"].ToString());
                DictInDb.Add(dictypename, dictypeid);
                dictTypeInfo.actionResult = new ActionResult()
                {
                    ActionType = 1,
                    ResultCode =1,
                    Message = "创建字典成功"
                };
            }
            return ret;
        }

        private List<ExcelDataSourceInfo> CreateAllDataSource() {
            List<ExcelDataSourceInfo> retList = new List<ExcelDataSourceInfo>();
            foreach (string datasourcename in DataSourceNeedCreate.Keys) {
                //根据名称创建数据源
                Guid EntityId = Guid.Empty;
                string entitytable = "";
                ExcelDataSourceInfo datasourceInfo = new ExcelDataSourceInfo();
                retList.Add(datasourceInfo);
                datasourceInfo.DataSourceName = datasourcename;
                if (dictEntity.ContainsKey(datasourcename))
                {
                    ExcelEntityInfo entityInfo = dictEntity[datasourcename];
                    EntityId = entityInfo.EntityId;
                    entitytable = entityInfo.TableName;
                }
                else {
                    Dictionary<string, object> entityInfo = this._entityProRepository.GetEntityInfoByEntityName(null, datasourcename, 1);
                    if (entityInfo == null) {
                        datasourceInfo.actionResult = new ActionResult()
                        {
                            ActionType = 1,
                            ResultCode = -3,
                            Message = "创建数据源时无法查找实体信息"
                        };
                        continue;//继续尝试下一个，争取一次获得更多的错误信息
                    }
                    EntityId = Guid.Parse(entityInfo["entityid"].ToString());
                    entitytable = entityInfo["entitytable"].ToString();
                }
                DataSourceMapper mapper = new DataSourceMapper() {
                    DatasourceName = datasourcename,
                    EntityId = EntityId.ToString(),
                    IsPro = 1,
                    IsRelatePower = 0,
                    RecStatus = 1 ,
                    Srcmark = "导入系统自动创建",
                    SrcType = 0 
                };
                OperateResult result =  this._dataSourceRepository.InsertSaveDataSource(mapper, 1);
                if (result.Flag!=1) {
                    datasourceInfo.actionResult = new ActionResult()
                    {
                        ActionType = 1,
                        ResultCode = -3,
                        Message = "创建数据源失败：" + result.Msg
                    };
                    continue;//继续尝试下一个，争取一次获得更多的错误信息
                }
                Guid DataSoruceId = Guid.Parse(result.Id);
                datasourceInfo.actionResult = new ActionResult()
                {
                    ActionType = 1,
                    ResultCode = 1,
                    Message = result.Id
                };
                datasourceInfo.DataSourceId = result.Id;
                string RuleSql = string.Format("Select recid as id ,recname as name from {0} where ",entitytable)+" {queryData}";

                InsertDataSourceConfigMapper detailMapper = new InsertDataSourceConfigMapper() {
                    DataSourceId = DataSoruceId.ToString(),
                    ColNames = "name",
                    Colors = "#666666,#666666,#666666,#666666,#666666,#666666",
                    Fonts = "14,14,14,14,14,14",
                    RuleSql = RuleSql,
                    ViewStyleId =201,
                };

                result = this._dataSourceRepository.InsertSaveDataSourceDetail(detailMapper, 1);
                if (result.Flag !=1) {
                    datasourceInfo.actionResult = new ActionResult()
                    {
                        ActionType = 1,
                        ResultCode = -3,
                        Message = "更新数据源失败：" + result.Msg
                    };
                    continue;//继续尝试下一个，争取一次获得更多的错误信息
                }
                DataSourceInDb.Add(datasourcename, this._dataSourceRepository.GetDataSourceByName(null, datasourcename, 1));
            }
            return retList;

        }
        private void CreateAllFieldInfo(List<ExcelEntityInfo> listEntities)  {
            foreach (ExcelEntityInfo entityInfo in listEntities) {
                CreateOneEntityFieldInfo(entityInfo);
            }

        }
        private void CreateOneEntityFieldInfo(ExcelEntityInfo entityInfo) {
            foreach (ExcelEntityColumnInfo fieldInfo in entityInfo.Fields) {

                //检查fieldconfig
                fieldInfo.actionResult = new ActionResult() { ResultCode = 0 };//重置状态，避免前面错误导致后面无法更新
                checkOneField(fieldInfo, entityInfo, dictEntity, dictTable);
                if (fieldInfo.FieldTypeName.Equals("系统字段"))
                {
                    if (fieldInfo.FieldId == null && fieldInfo.FieldId == Guid.Empty) {
                        fieldInfo.actionResult = new ActionResult()
                        {
                            ActionType = 0,
                            ResultCode = -3,
                            Message = "系统字段必须是存在的字段"
                        };
                        continue;
                    }
                    fieldInfo.IsUpdate = true;
                }
                if (fieldInfo.actionResult.ResultCode < 0) {
                    continue;//本字段已经错了，需要重新处理
                }
                //新建字段
                EntityFieldProModel model = new EntityFieldProModel()
                {
                    FieldName = fieldInfo.FieldName,
                    EntityId = entityInfo.EntityId.ToString(),
                    FieldLabel = fieldInfo.DisplayName,
                    DisplayName = fieldInfo.DisplayName,
                    ControlType = fieldInfo.ControlType,
                    FieldType = fieldInfo.FieldType,
                    RecStatus = 1,
                    FieldConfig = Newtonsoft.Json.JsonConvert.SerializeObject(fieldInfo.FieldConfig)
                };
                if (fieldInfo.IsUpdate)
                {
                    this._entityProRepository.UpdateEntityFieldName(null,fieldInfo.FieldId, fieldInfo.DisplayName, 1);
                    fieldInfo.actionResult = new ActionResult()
                    {
                        ActionType = 1,
                        ResultCode = 1,
                        Message = "更新成功 "
                    };
                }
                else {

                    OutputResult<object> result = this._entityProServices.InsertEntityField(model, 1);
                    if (result.Status != 0)
                    {
                        fieldInfo.actionResult = new ActionResult()
                        {
                            ActionType = 2,
                            ResultCode = -3,
                            Message = "新增字段失败:"+result.Message
                        };
                    }
                    else
                    {
                        fieldInfo.actionResult = new ActionResult()
                        {
                            ActionType = 2,
                            ResultCode = 1,
                            Message = "新增字段成功 "
                        };
                        fieldInfo.FieldId = Guid.Parse(result.DataBody.ToString());
                    }
                }
            }
            if (entityInfo.SubEntitys != null && entityInfo.SubEntitys.Count > 0) {
                foreach (ExcelEntityInfo subEntityInfo in entityInfo.SubEntitys)
                {
                    CreateOneEntityFieldInfo(subEntityInfo);
                }
            }
        }
        /// <summary>
        /// 仅仅是创建实体的基本信息，包含基本字段
        /// </summary>
        /// <param name="listEntity"></param>
        private void CreateAllMainEntityInfo(List<ExcelEntityInfo> listEntity) {
            Queue<ExcelEntityInfo> queue = new Queue<ExcelEntityInfo>();
            foreach (ExcelEntityInfo entity in dictEntity.Values) {
                queue.Enqueue(entity);
            }
            while (queue.Count > 0) {
                ExcelEntityInfo entityInfo = queue.Dequeue();
                if (entityInfo == null) break;
                if (entityInfo.EntityId != null && entityInfo.EntityId != Guid.Empty) continue;
                if (entityInfo.EntityTypeName.StartsWith("简单") || entityInfo.EntityTypeName.StartsWith("嵌套") || entityInfo.EntityTypeName.StartsWith("动态")) {
                    if (entityInfo.RelEntityName != null && entityInfo.RelEntityName.Length > 0) {
                        if (dictEntity.ContainsKey(entityInfo.RelEntityName))
                        {
                            ExcelEntityInfo relEntityInfo = dictEntity[entityInfo.RelEntityName];
                            if (relEntityInfo.EntityId.Equals(Guid.Empty))
                            {
                                queue.Enqueue(entityInfo);///关联对象尚未生成
                            }
                            else
                            {
                                entityInfo.RelEntityId = relEntityInfo.EntityId;//赋值。
                                //这里还需要处理fieldid
                            }
                        }
                        else {
                            //不是本次创建的额，实体已经在库中了，而且已经转化为entityid,所以可以直接使用了
                            entityInfo.actionResult = new ActionResult()
                            {
                                ActionType = 1,
                                ResultCode = 1,
                                Message = "无需更新"
                            };
                        }
                    }

                }
                int TypeId = 0;
                if (entityInfo.EntityTypeName.StartsWith("独立")) {
                    TypeId = 0;
                } else if (entityInfo.EntityTypeName.StartsWith("简单")) {
                    TypeId = 2;
                } else if (entityInfo.EntityTypeName.StartsWith("动态")) {
                    TypeId = 3;
                } else {
                    TypeId = 1;
                }
                string RelEntityId = "";
                Guid RelFieldId = Guid.Empty;
                int Relaudit = 0;
                if (entityInfo.IsLinkToWorkFlow != null && entityInfo.IsLinkToWorkFlow.Equals("√")) {
                    Relaudit = 1;
                }
                RelEntityId = entityInfo.RelEntityId.ToString();
                RelFieldId = entityInfo.RelFieldId;
                if (entityInfo.EntityTypeName.StartsWith("简单")) {
                    
                }
                EntityProModel model = new EntityProModel() {
                    EntityName = entityInfo.EntityName,
                    EntityTable = entityInfo.TableName,
                    TypeId = TypeId,
                    RecStatus = 1,
                    Icons = "00000000-0000-0000-0000-200000000001",
                    RelEntityId = RelEntityId,
                    RelFieldId = RelFieldId,
                    Relaudit = Relaudit,
                    Remark = "",
                    Styles = ""

                };
                OutputResult<object>  result = this._entityProServices.InsertEntityPro(model, 1);
                if (result.Status == 0)
                {
                    entityInfo.EntityId = Guid.Parse(result.DataBody.ToString());
                    entityInfo.actionResult = new ActionResult()
                    {
                        ActionType = 2,
                        ResultCode = 1,
                        Message = "新增成功，guid:="+entityInfo.EntityId.ToString()
                    };
                }
                else {
                    entityInfo.actionResult = new ActionResult()
                    {
                        ActionType = 2,
                        ResultCode = -3,
                        Message = "新增失败:"+ result.Message
                    };
                }
            }

        }
        private void CheckEntityAndField(List<ExcelEntityInfo> listEntity) {
            dictEntity = new Dictionary<string, ExcelEntityInfo>();
            dictTable  = new Dictionary<string, string>();
            CheckSaveEntityNameInMem(ref dictEntity, ref dictTable, listEntity);

            //检查实体有有效性
            foreach (ExcelEntityInfo entityInfo in listEntity) {
                CheckOneEntityInDb(entityInfo, dictEntity, dictTable);
            }

        }

        private void CheckOneEntityInDb(ExcelEntityInfo entityInfo, Dictionary<string, ExcelEntityInfo> dictEntity, Dictionary<string, string> dictTable) {
            /**
             * 处理主实体信息
             * 1、只能是独立实体、简单实体和动态实体，嵌套实体不能放在主实体上
             * 2、如果是简单实体和独立实体，引用的关联实体有可能是需要导入的，有可能是已经存在数据库中，如果存在数据库中，则先拿出来EntityID
             * 3、如果带有嵌套实体，嵌套实体关联的主实体必须是本sheet内定义的，
             * 4、如果带有嵌套实体，嵌套实体要不就只有一个分类，要不分类就必须跟主实体一致。
             */
            if (!(entityInfo.EntityTypeName.StartsWith("独立") || entityInfo.EntityTypeName.StartsWith("简单") || entityInfo.EntityTypeName.StartsWith("动态")))
            {
                entityInfo.actionResult = new ActionResult()
                {
                    ActionType = 0,
                    ResultCode = -2,
                    Message = "检查失败，实体【" + entityInfo.EntityName + "】必须是独立、简单或者动态实体"
                };
            }
            if (entityInfo.EntityTypeName.StartsWith("简单") && entityInfo.RelEntityName.Length > 0) {
                if (dictEntity.ContainsKey(entityInfo.RelEntityName)) {
                    ExcelEntityInfo relEntityInfo = dictEntity[entityInfo.RelEntityName];
                    if (!(relEntityInfo.EntityTypeName.StartsWith("独立") || relEntityInfo.EntityName.StartsWith("简单"))) {
                        entityInfo.actionResult = new ActionResult()
                        {
                            ActionType = 0,
                            ResultCode = -2,
                            Message = "简单实体只能关联简单实体或者独立实体"
                        };
                    }
                }
                else {
                    //从数据库中获取实体定义信息
                    Dictionary<string, object> searchEntityInfo = this._entityProRepository.GetEntityInfoByEntityName(null, entityInfo.RelEntityName, 1);
                    if (searchEntityInfo != null)
                    {
                        int modeltype = int.Parse(searchEntityInfo["modeltype"].ToString());
                        if (!(modeltype == 0 || modeltype == 2)) {
                            entityInfo.actionResult = new ActionResult()
                            {
                                ActionType = 0,
                                ResultCode = -2,
                                Message = "简单实体只能关联简单实体或者独立实体"
                            };
                        }
                        entityInfo.RelEntityId = Guid.Parse(searchEntityInfo["entityid"].ToString());
                    }
                    else {
                        
                             entityInfo.actionResult = new ActionResult()
                             {
                                 ActionType = 0,
                                 ResultCode = -2,
                                 Message = "简单实体只能关联简单实体或者独立实体"
                             };
                    }
                }
            } else if (entityInfo.EntityTypeName.StartsWith("独立") && entityInfo.RelEntityName.Length > 0) {
                entityInfo.actionResult = new ActionResult()
                {
                    ActionType = 0,
                    ResultCode = -2,
                    Message = "独立实体不能有关联实体"
                };
            } else if (entityInfo.EntityTypeName.StartsWith("动态")) {
                if (entityInfo.RelEntityName.Length == 0 || entityInfo.RelFieldName.Length == 0) {
                    entityInfo.actionResult = new ActionResult()
                    {
                        ActionType = 0,
                        ResultCode = -2,
                        Message = "动态实体必须关联实体"
                    };
                }
                if (dictEntity.ContainsKey(entityInfo.RelEntityName))
                {
                    ExcelEntityInfo relEntityInfo = dictEntity[entityInfo.RelEntityName];
                    if (!(relEntityInfo.EntityTypeName.StartsWith("独立") || relEntityInfo.EntityName.StartsWith("简单")))
                    {
                        entityInfo.actionResult = new ActionResult()
                        {
                            ActionType = 0,
                            ResultCode = -2,
                            Message = "简单实体只能关联简单实体或者独立实体"
                        };
                    }
                    //需要检查是否存在字段
                    bool isFoundField = false;
                    foreach (ExcelEntityColumnInfo fieldInfo in relEntityInfo.Fields) {
                        if (!(fieldInfo.DisplayName.Equals(entityInfo.RelFieldName) || fieldInfo.FieldName.Equals(entityInfo.RelFieldName))) {
                            entityInfo.actionResult = new ActionResult()
                            {
                                ActionType = 0,
                                ResultCode = -2,
                                Message = "实体定义中，没有找到关联的字段信息"
                            };
                        }
                    }
                }
                else
                {
                    //从数据库中获取实体定义信息
                    Dictionary<string, object> searchEntityInfo = this._entityProRepository.GetEntityInfoByEntityName(null, entityInfo.RelEntityName, 1);
                    if (searchEntityInfo != null)
                    {
                        int modeltype = int.Parse(searchEntityInfo["modeltype"].ToString());
                        if (!(modeltype == 0 || modeltype == 2))
                        {
                            entityInfo.actionResult = new ActionResult()
                            {
                                ActionType = 0,
                                ResultCode = -2,
                                Message = "简单实体只能关联简单实体或者独立实体"
                            };
                        }
                        entityInfo.RelEntityId = Guid.Parse(searchEntityInfo["entityid"].ToString());
                    }
                    else
                    {
                        entityInfo.actionResult = new ActionResult()
                        {
                            ActionType = 0,
                            ResultCode = -2,
                            Message = "无法找到实体定义" + entityInfo.RelEntityName
                        };
                    }
                    Dictionary<string, object> searchFieldInfo = this._entityProRepository.GetFieldInfoByFieldName(null, entityInfo.RelFieldName, entityInfo.RelEntityId, 1);
                    if (searchFieldInfo == null) {
                        entityInfo.actionResult = new ActionResult()
                        {
                            ActionType = 0,
                            ResultCode = -2,
                            Message = "实体定义中，没有找到关联的字段信息"
                        };
                    }
                    entityInfo.RelEntityId = Guid.Parse(searchFieldInfo["fieldid"].ToString());
                }
            }

            //开始检查实体定义是否在表中存在
            Dictionary<string, object> oldEntityInDb = null;
            oldEntityInDb = this._entityProRepository.GetEntityInfoByTableName(null, entityInfo.TableName, 1);
            if (oldEntityInDb != null)
            {
                entityInfo.EntityId = Guid.Parse(oldEntityInDb["entityid"].ToString());
                entityInfo.IsUpdate = true;
            }
            if (!entityInfo.IsUpdate)
            {
                oldEntityInDb = this._entityProRepository.GetEntityInfoByEntityName(null, entityInfo.EntityName, 1);
                if (oldEntityInDb != null)
                {
                    entityInfo.actionResult = new ActionResult()
                    {
                        ActionType = 0,
                        ResultCode = -2,
                        Message = "实体【" + entityInfo.EntityName + "】已经存在"
                    };
                }
            }
            
            if (entityInfo.SubEntitys != null && entityInfo.SubEntitys.Count > 0) {
                foreach (ExcelEntityInfo subEntityInfo in entityInfo.SubEntitys) {
                    if (subEntityInfo.TypeNameDict.Count > 1) {
                        if (subEntityInfo.TypeNameDict.Count != entityInfo.TypeNameDict.Count) {
                            subEntityInfo.actionResult = new ActionResult()
                            {
                                ActionType = 0,
                                ResultCode = -2,
                                Message = "嵌套实体的分类定义必须跟主实体一致"
                            };
                        }
                    }
                    int typecount = subEntityInfo.TypeNameDict.Count;
                    for (int i = 0; i < typecount; i++)
                    {
                        EntityTypeSettingInfo main = entityInfo.TypeNameDict[i.ToString()];
                        EntityTypeSettingInfo sub = subEntityInfo.TypeNameDict[i.ToString()];
                        if (main.TypeName.Equals(sub.TypeName) == false) {
                            subEntityInfo.actionResult = new ActionResult()
                            {
                                ActionType = 0,
                                ResultCode = -2,
                                Message = "嵌套实体的分类定义必须跟主实体一致"
                            };
                        }
                    }

                    oldEntityInDb = this._entityProRepository.GetEntityInfoByTableName(null, subEntityInfo.TableName, 1);
                    if (oldEntityInDb != null)
                    {

                        subEntityInfo.EntityId = Guid.Parse(oldEntityInDb["entityid"].ToString());
                        subEntityInfo.IsUpdate = true;
                    }
                    if (!subEntityInfo.IsUpdate)
                    {
                        oldEntityInDb = this._entityProRepository.GetEntityInfoByEntityName(null, subEntityInfo.EntityName, 1);
                        if (oldEntityInDb != null)
                        {
                            subEntityInfo.actionResult = new ActionResult()
                            {
                                ActionType = 0,
                                ResultCode = -2,
                                Message = "实体【" + subEntityInfo.EntityName + "】已经存在"
                            };
                        }
                    }
                }
            }
            //检查字段是否合理
            foreach (ExcelEntityColumnInfo fieldInfo in entityInfo.Fields) {
                checkOneField(fieldInfo, entityInfo, dictEntity, dictTable);
            }
            if (entityInfo.SubEntitys != null && entityInfo.SubEntitys.Count > 0) {
                foreach (ExcelEntityInfo subEntityInfo in entityInfo.SubEntitys) {
                    foreach (ExcelEntityColumnInfo fieldInfo in subEntityInfo.Fields)
                    {
                        checkOneField(fieldInfo, subEntityInfo, dictEntity, dictTable);
                    }
                }
            }
        }

        private void checkOneField(ExcelEntityColumnInfo fieldInfo, ExcelEntityInfo entityInfo, Dictionary<string, ExcelEntityInfo> dictEntity, Dictionary<string, string> dictTable) {
            if (fieldInfo.DisplayName.Length == 0 || fieldInfo.FieldName.Length == 0 || fieldInfo.FieldTypeName.Length == 0) throw (new Exception("字段显示名称和数据库名称都不能为空"));
            //bool isInRevert = DefaultFieldName.Contains(fieldInfo.FieldName);
            //if (isInRevert) {
            //    if (fieldInfo.FieldTypeName.Equals("格式") == false) {
            //        throw (new Exception("不能使用系统预知字段"));
            //    }
            //}
            //检查是否已经存在的表格
            if (fieldInfo.FieldId == null || fieldInfo.FieldId == Guid.Empty) {
                //尝试检查字段是否存在，如果存在，则只能更新字段名称（中文）和显示名称，其他都不能更改
                if (entityInfo.EntityId != null && entityInfo.EntityId != Guid.Empty)
                {
                    Dictionary<string, object> tmpFieldInfo = this._entityProRepository.GetFieldInfoByFieldName(null, fieldInfo.FieldName,entityInfo.EntityId , 1);
                    Dictionary<string, object> tmpFieldInfo2 = this._entityProRepository.GetFieldInfoByDisplayName(null, fieldInfo.DisplayName, entityInfo.EntityId, 1);
                    if (tmpFieldInfo != null)
                    {
                        fieldInfo.FieldId = Guid.Parse(tmpFieldInfo["fieldid"].ToString());
                        fieldInfo.IsUpdate = true;
                    }
                    else {
                        if (tmpFieldInfo2 != null)
                        {
                            fieldInfo.actionResult = new ActionResult()
                            {
                                ActionType = 0 ,
                                ResultCode = -2,
                                Message = "字段的显示名称或者字段名称重复:" + fieldInfo.DisplayName + ":" + fieldInfo.FieldName
                            };
                        }
                    }
                }
            }
            if (fieldInfo.FieldTypeName.Equals("文本"))
            {
                //默认1000大小
                fieldInfo.ControlType = 1; fieldInfo.FieldType = 2;
                fieldInfo.FieldConfig = MakeTextFieldConfig();
            }
            else if (fieldInfo.FieldTypeName.Equals("大文本"))
            {
                fieldInfo.ControlType = 5; fieldInfo.FieldType = 2;
                fieldInfo.FieldConfig = MakeBigTextFieldConfig(fieldInfo.DefaultValue);
            }
            else if (fieldInfo.FieldTypeName.Equals("字典单选") || fieldInfo.FieldTypeName.Equals("单选"))
            {
                fieldInfo.ControlType = 3; fieldInfo.FieldType = 2;
                int dictType = GetDictTypeId(fieldInfo.DataSourceName);
                if (dictType > 0)
                {
                    if (fieldInfo.DefaultValue != null && fieldInfo.DefaultValue.Length > 0)
                    {
                        //检验默认值
                        int defaultid = this._dataSourceRepository.GetDictValueByName(dictType, fieldInfo.DefaultValue);
                        if (defaultid <= 0)
                        {
                            fieldInfo.actionResult = new ActionResult()
                            {
                                ActionType = 0,
                                ResultCode = -2,
                                Message = "字典值没有定义" + fieldInfo.DefaultValue
                            };
                        }
                        fieldInfo.FieldConfig = MakeDictFieldConfig(dictType, defaultid);
                    }
                    else
                    {
                        fieldInfo.FieldConfig = MakeDictFieldConfig(dictType, -1);
                    }
                }
                else
                {
                    if (fieldInfo.DefaultValue != null && fieldInfo.DefaultValue.Length > 0)
                    {
                        Dictionary<string, object> tmp = new Dictionary<string, object>();
                        tmp.Add("dictypename", fieldInfo.DataSourceName);
                        tmp.Add("dictname", fieldInfo.DefaultValue);
                        DictValueNeedCreate.Add(tmp);
                    }
                }
            }
            else if (fieldInfo.FieldTypeName.Equals("字典多选"))
            {
                fieldInfo.ControlType = 4; fieldInfo.FieldType = 2;
                int dictType = GetDictTypeId(fieldInfo.DataSourceName);
                if (dictType > 0)
                {
                    fieldInfo.FieldConfig = MakeDictFieldConfig(dictType, -1);
                }
            }
            else if (fieldInfo.FieldTypeName.Equals("小数") || fieldInfo.FieldTypeName.Equals("小数(金额)"))
            {
                fieldInfo.ControlType = 7; fieldInfo.FieldType = 2;
                if (fieldInfo.DefaultValue.Length > 0)
                {
                    decimal tmp = 0;
                    if (decimal.TryParse(fieldInfo.DefaultValue, out tmp) == false)
                    {
                        fieldInfo.actionResult = new ActionResult()
                        {
                            ActionType = 0,
                            ResultCode = -2,
                            Message = "默认值转换失败" + fieldInfo.DefaultValue
                        };
                    }
                    fieldInfo.FieldConfig = MakeDecimalFieldConfig(true, tmp);
                }
                else
                {
                    fieldInfo.FieldConfig = MakeDecimalFieldConfig(false, new decimal(-1.0));
                }
            }
            else if (fieldInfo.FieldTypeName.Equals("整数"))
            {
                fieldInfo.ControlType = 6; fieldInfo.FieldType = 2;
                if (fieldInfo.DefaultValue.Length > 0)
                {
                    int tmp = 0;
                    if (int.TryParse(fieldInfo.DefaultValue, out tmp) == false)
                    {
                        fieldInfo.actionResult = new ActionResult()
                        {
                            ActionType = 0,
                            ResultCode = -2,
                            Message = "默认值转换失败" + fieldInfo.DefaultValue
                        };
                    }
                    fieldInfo.FieldConfig = MakeIntFieldConfig(true, tmp);
                }
                else
                {
                    fieldInfo.FieldConfig = MakeIntFieldConfig(false, -1);
                }
            }
            else if (fieldInfo.FieldTypeName.Equals("日期"))
            {
                fieldInfo.ControlType = 8; fieldInfo.FieldType = 2;
                fieldInfo.FieldConfig = MakeDateFieldConfig(false);
            }
            else if (fieldInfo.FieldTypeName.Equals("日期时间"))
            {

                fieldInfo.ControlType = 9; fieldInfo.FieldType = 2;
                fieldInfo.FieldConfig = MakeDateFieldConfig(true);
            }
            else if (fieldInfo.FieldTypeName.Equals("手机"))
            {
                fieldInfo.ControlType = 10; fieldInfo.FieldType = 2;
                fieldInfo.FieldConfig = MakeCommonTextFieldConfig("[0-9]+");
            }
            else if (fieldInfo.FieldTypeName.Equals("邮箱"))
            {
                fieldInfo.ControlType = 11; fieldInfo.FieldType = 2;
                fieldInfo.FieldConfig = MakeCommonTextFieldConfig(@"\\w+([-+.]\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*");
            }
            else if (fieldInfo.FieldTypeName.Equals("电话"))
            {
                fieldInfo.ControlType = 11; fieldInfo.FieldType = 2;
                fieldInfo.FieldConfig = MakeCommonTextFieldConfig(@"\\w+([-+.]\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*");

            }
            else if (fieldInfo.FieldTypeName.Equals("地址"))
            {
                fieldInfo.ControlType = 13; fieldInfo.FieldType = 2;
                fieldInfo.FieldConfig = MakeCommonTextFieldConfig(@"");
            }
            else if (fieldInfo.FieldTypeName.Equals("定位"))
            {
                fieldInfo.ControlType = 14; fieldInfo.FieldType = 2;
                fieldInfo.FieldConfig = MakeCommonTextFieldConfig(@"");
            }
            else if (fieldInfo.FieldTypeName.Equals("行政区域"))
            {
                fieldInfo.ControlType = 16; fieldInfo.FieldType = 2;
                fieldInfo.FieldConfig = MakeCommonTextFieldConfig(@"");
            }
            else if (fieldInfo.FieldTypeName.Equals("头像"))
            {
                fieldInfo.ControlType = 15; fieldInfo.FieldType = 2;
                fieldInfo.FieldConfig = MakeHeadIconFieldConfig();
            }
            else if (fieldInfo.FieldTypeName.Equals("团队组织"))
            {
                fieldInfo.ControlType = 16; fieldInfo.FieldType = 2;
                if (fieldInfo.DataSourceName != null && fieldInfo.DataSourceName.Equals("多选"))
                {
                    fieldInfo.FieldConfig = MakeDeptFieldConfig(true);
                }
                else
                {
                    fieldInfo.FieldConfig = MakeDeptFieldConfig(false);
                }
            }

            else if (fieldInfo.FieldTypeName.Equals("分组"))
            {
                fieldInfo.ControlType = 20; fieldInfo.FieldType = 2;
                fieldInfo.FieldConfig = MakeGroupFieldConfig(true);
            }
            else if (fieldInfo.FieldTypeName.Equals("图片"))
            {
                fieldInfo.ControlType = 22; fieldInfo.FieldType = 2;
                fieldInfo.FieldConfig = MakePhotoFieldConfig();
            }
            else if (fieldInfo.FieldTypeName.Equals("附件"))
            {
                fieldInfo.ControlType = 23; fieldInfo.FieldType = 2;
                fieldInfo.FieldConfig = MakeAttachFieldConfig();
            }
            else if (fieldInfo.FieldTypeName.Equals("选人"))
            {
                fieldInfo.ControlType = 25; fieldInfo.FieldType = 2;
                if (fieldInfo.DataSourceName != null && fieldInfo.DataSourceName.Equals("多选"))
                    fieldInfo.FieldConfig = MakePersonFieldConfig(true);
                else
                    fieldInfo.FieldConfig = MakePersonFieldConfig(false);
            }
            else if (fieldInfo.FieldTypeName.Equals("产品"))
            {
                fieldInfo.ControlType = 28; fieldInfo.FieldType = 2;
                if (fieldInfo.DataSourceName != null && fieldInfo.DataSourceName.Equals("多选"))
                    fieldInfo.FieldConfig = MakeDeptFieldConfig(true);
                else
                    fieldInfo.FieldConfig = MakeDeptFieldConfig(false);
            }
            else if (fieldInfo.FieldTypeName.Equals("产品系列"))
            {
                fieldInfo.ControlType = 29; fieldInfo.FieldType = 2;
                if (fieldInfo.DataSourceName != null && fieldInfo.DataSourceName.Equals("多选"))
                    fieldInfo.FieldConfig = MakeDeptFieldConfig(true);
                else
                    fieldInfo.FieldConfig = MakeDeptFieldConfig(false);
            }
            else if (fieldInfo.FieldTypeName.Equals("单选数据源") || fieldInfo.FieldTypeName.Equals("数据源"))
            {

                fieldInfo.ControlType = 18; fieldInfo.FieldType = 2;
                if (fieldInfo.DataSourceName == null || fieldInfo.DataSourceName.Length == 0)
                {
                    fieldInfo.actionResult = new ActionResult()
                    {
                        ActionType = 0,
                        ResultCode = -2,
                        Message = "数据源配置错误" + fieldInfo.FieldName
                    };
                }
                Guid datasourceid = GetDataSourceIdFromName(fieldInfo.DataSourceName);
                if (datasourceid != Guid.Empty)
                {
                    fieldInfo.FieldConfig = MakeDataSourceFieldConfig(datasourceid.ToString(), false);
                }
            }
            else if (fieldInfo.FieldTypeName.Equals("多选数据源"))
            {
                fieldInfo.ControlType = 18; fieldInfo.FieldType = 2;
                if (fieldInfo.DataSourceName == null || fieldInfo.DataSourceName.Length == 0)
                {
                    fieldInfo.actionResult = new ActionResult()
                    {
                        ActionType = 0,
                        ResultCode = -2,
                        Message = "数据源配置错误" + fieldInfo.FieldName
                    };
                }
                Guid datasourceid = GetDataSourceIdFromName(fieldInfo.DataSourceName);
                if (datasourceid != Guid.Empty)
                {
                    fieldInfo.FieldConfig = MakeDataSourceFieldConfig(datasourceid.ToString(), true);
                }
            }
            else if (fieldInfo.FieldTypeName.Equals("引用对象") || fieldInfo.FieldTypeName.Equals("引用字段"))
            {

                fieldInfo.ControlType = 31; fieldInfo.FieldType = 2;
                if (fieldInfo.DataSourceName == null) {
                    fieldInfo.actionResult = new ActionResult()
                    {
                        ActionType = 0,
                        ResultCode = -2,
                        Message = "引用对象异常"
                    };
                }
                string[] tmp = fieldInfo.DataSourceName.Split(',');
                if (tmp.Length != 2) {
                    fieldInfo.actionResult = new ActionResult()
                    {
                        ActionType = 0,
                        ResultCode = -2,
                        Message = "引用对象异常:" + fieldInfo.DataSourceName
                    };
                }
                fieldInfo.RelField_ControlFieldName = tmp[0];

                fieldInfo.RelField_originFieldName = tmp[1];
                //检查是否存在
                bool isExists = false;
                ExcelEntityColumnInfo foundFieldInfo = null;
                foreach (ExcelEntityColumnInfo fi in entityInfo.Fields)
                {
                    if (fi.DisplayName.Equals(fieldInfo.RelField_ControlFieldName))
                    {
                        if (fi.ControlType != 18)
                        {
                            fieldInfo.actionResult = new ActionResult()
                            {
                                ActionType = 0,
                                ResultCode = -2,
                                Message = "引用控件的控制控件只能是数据源控件"
                            };
                        }
                        else if (fi.FieldId != Guid.Empty)
                        {
                            fieldInfo.RelField_ControlFieldId = fi.FieldId.ToString();
                        }
                        foundFieldInfo = fi;
                        isExists = true;
                        break;
                    }
                }
                #region 处理引用对象的字段fieldInfo.RelField_originFieldName
                if (foundFieldInfo != null && foundFieldInfo.FieldId != Guid.Empty)
                {

                    Dictionary<string, object> tmpField = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(Newtonsoft.Json.JsonConvert.SerializeObject(this._entityProRepository.GetFieldInfo(foundFieldInfo.FieldId, 1)));
                    if (tmpField != null)
                    {
                        Guid datasourceid = GetDataSourceIdFromFieldDict(tmpField);
                        if (datasourceid != Guid.Empty)
                        {
                            //找到DATa Source就开始找entityid
                            Dictionary<string, object> datasourcetmp = this._dataSourceRepository.GetDataSourceInfo(datasourceid, 1);
                            if (datasourcetmp != null && datasourcetmp.ContainsKey("entityid"))
                            {
                                string tmp_entityid = datasourcetmp["entityid"].ToString();
                                fieldInfo.RelField_OriginEntityId = tmp_entityid;
                            }
                        }
                    }
                }
                if (fieldInfo.RelField_OriginEntityId != null && fieldInfo.RelField_OriginEntityId.Length > 0)
                {
                    //通过originentityid 和rel_fieldname 获取fieldInfo.relfield_originfieldid 
                    Dictionary<string, object> tmpFielddict = this._entityProRepository.GetFieldInfoByFieldName(null, fieldInfo.RelField_originFieldName, Guid.Parse(fieldInfo.RelField_OriginEntityId), 1);
                    if (tmpFielddict != null && tmpFielddict.ContainsKey("fieldid"))
                    {
                        fieldInfo.RelField_originFieldId = tmpFielddict["fieldid"].ToString();
                    }
                }
                fieldInfo.FieldConfig = MakeReferenceFieldConfig(fieldInfo);
                #endregion
            }
            else if (fieldInfo.FieldTypeName.Equals("表格")|| fieldInfo.FieldTypeName.Equals("表格字段"))
            {
                fieldInfo.ControlType = 24; fieldInfo.FieldType = 2;
                if (fieldInfo.DataSourceName == null)   {
                    fieldInfo.actionResult = new ActionResult()
                    {
                        ActionType = 0,
                        ResultCode = -2,
                        Message = "表格用对象异常"
                    };
                } 
                string[] tmp = fieldInfo.DataSourceName.Split(',');
                if (tmp.Length != 2) {
                    fieldInfo.actionResult = new ActionResult()
                    {
                        ActionType = 0,
                        ResultCode = -2,
                        Message = "表格用对象异常："+fieldInfo.DataSourceName
                    };
                }
                fieldInfo.Table_EntityName = tmp[0];
                fieldInfo.Table_TitleDisplayName = tmp[1];
                ExcelEntityInfo tableEntityInfo = null;
                if (entityInfo.SubEntitys == null) {
                    fieldInfo.actionResult = new ActionResult()
                    {
                        ActionType = 0,
                        ResultCode = -2,
                        Message = "表格对应的嵌套实体不存在：" + fieldInfo.DataSourceName
                    };
                }
                foreach (ExcelEntityInfo subEntityInfo in entityInfo.SubEntitys)
                {
                    if (subEntityInfo.EntityName.Equals(fieldInfo.Table_EntityName))
                    {
                        tableEntityInfo = subEntityInfo;
                        break;
                    }
                }
                if (tableEntityInfo == null) {
                    fieldInfo.actionResult = new ActionResult()
                    {
                        ActionType = 0,
                        ResultCode = -2,
                        Message = "表格对应的嵌套实体不存在：" + fieldInfo.Table_EntityName
                    };
                }
                if (tableEntityInfo.EntityId != null && tableEntityInfo.EntityId != Guid.Empty)
                {
                    fieldInfo.Table_EntityId = tableEntityInfo.EntityId.ToString();
                }
                ExcelEntityColumnInfo foundField = null;
                foreach (ExcelEntityColumnInfo subFieldInfo in tableEntityInfo.Fields)
                {
                    if (subFieldInfo.DisplayName.Equals(fieldInfo.Table_TitleDisplayName))
                    {
                        foundField = subFieldInfo;
                        break;
                    }
                }
                if (foundField == null) {
                    fieldInfo.actionResult = new ActionResult()
                    {
                        ActionType = 0,
                        ResultCode = -2,
                        Message = "表格对应的嵌套实体的字段不存在：" + fieldInfo.Table_TitleFieldName
                    };
                }
                fieldInfo.Table_TitleFieldName = foundField.FieldName;
                fieldInfo.FieldConfig = MakeTableFieldConfig(fieldInfo);
            }
            else {
                if ((fieldInfo.FieldId == null || fieldInfo.FieldId == Guid.Empty) && !fieldInfo.FieldTypeName.Equals("系统字段")) {
                    fieldInfo.actionResult = new ActionResult()
                    {
                        ActionType = 0,
                        ResultCode = -2,
                        Message = "字段类型错误:" + fieldInfo.FieldTypeName
                    };
                }
            }
        }

        private Dictionary<string, object> MakeTableFieldConfig(ExcelEntityColumnInfo fieldInfo) {
            Dictionary<string, object> retDict = MakeCommonTextFieldConfig("");
            retDict.Add("entityId", fieldInfo.Table_EntityId);
            retDict.Add("titleField", fieldInfo.Table_TitleFieldName);
            return retDict;
        }
        private Dictionary<string, object> MakeReferenceFieldConfig(ExcelEntityColumnInfo fieldInfo )
        {
            Dictionary<string, object> retDict = MakeCommonTextFieldConfig("");
            if (fieldInfo.RelField_originFieldId != null && fieldInfo.RelField_originFieldId.Length > 0)
            {

                retDict.Add("originEntity", fieldInfo.RelField_originFieldId);
            }
            else {

                retDict.Add("originEntity", Guid.Empty.ToString());
            }
            if (fieldInfo.RelField_OriginEntityId != null && fieldInfo.RelField_OriginEntityId.Length > 0)
            {

                retDict.Add("originField", fieldInfo.RelField_OriginEntityId);
            }
            else
            {

                retDict.Add("originField", Guid.Empty.ToString());
            }
            if (fieldInfo.RelField_ControlFieldId != null && fieldInfo.RelField_ControlFieldId.Length > 0)
            {

                retDict.Add("controlField", fieldInfo.RelField_ControlFieldId);
            }
            else
            {

                retDict.Add("controlField", Guid.Empty.ToString());
            }
            return retDict;
        }
        private Guid GetDataSourceIdFromFieldDict(Dictionary<string, object> data)
        {
            if (data.ContainsKey("fieldconfig")) {
                string s_fieldConfig = data["fieldconfig"].ToString();
                Dictionary<string, object> fieldConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(s_fieldConfig);
                if (fieldConfig != null && fieldConfig.ContainsKey("datasource")) {
                    string s_datasource = Newtonsoft.Json.JsonConvert.SerializeObject(fieldConfig["datasource"]);
                    Dictionary<string,object> datasource = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(s_datasource);
                    if (datasource != null && datasource.ContainsKey("sourceId")) {
                        Guid tmp = Guid.Empty;
                        if (Guid.TryParse(datasource["sourceId"].ToString(), out tmp))
                            return tmp;
                    }
                }
            }
            return Guid.Empty;
        }
        private Guid GetDataSourceIdFromName(string datasourceName) {
            if (DataSourceInDb.ContainsKey(datasourceName))
                return Guid.Parse(DataSourceInDb[datasourceName]["datasrcid"].ToString());
            if (DataSourceNeedCreate.ContainsKey(datasourceName))
                return Guid.Empty;//不用查数据库
            Dictionary<string, object> dsInfo = this._dataSourceRepository.GetDataSourceByName(null, datasourceName, 1);
            if (dsInfo != null)
            {
                DataSourceInDb.Add(datasourceName, dsInfo);
                return Guid.Parse(dsInfo["datasrcid"].ToString());
            }
            else {
                
                DataSourceNeedCreate.Add(datasourceName,"");
                return Guid.Empty;
            }

        }
        private Dictionary<string, object> MakeDataSourceFieldConfig(string dataSourceId, bool isMulti) {
            Dictionary<string, object> retDict = MakeCommonTextFieldConfig("");
            Dictionary<string, object> dataSource = new Dictionary<string, object>();
            dataSource.Add("type", "network");
            dataSource.Add("sourceId", dataSourceId);
            retDict.Add("dataSource", dataSource);
            if (isMulti)
                retDict.Add("multiple", 1);
            else
                retDict.Add("multiple", 0);
            return retDict;

        }
        private Dictionary<string, object> MakePersonFieldConfig(bool isMulti)
        {
            Dictionary<string, object> retDict = MakeCommonTextFieldConfig("");
            retDict.Add("dataRange",0);
            if (isMulti)
                retDict.Add("multiple", 1);
            else
                retDict.Add("multiple", 0);
            return retDict;
        }
        private Dictionary<string, object> MakeAttachFieldConfig() {
            Dictionary<string, object> retDict = MakeCommonTextFieldConfig("");
            retDict.Add("limit", 6);
            return retDict;
        }
        private Dictionary<string, object> MakePhotoFieldConfig() {
            Dictionary<string, object> retDict = MakeCommonTextFieldConfig("");
            retDict.Add("pictureType", 0);
            retDict.Add("limit", 6);
            return retDict;
        }
        private Dictionary<string, object> MakeGroupFieldConfig(bool canFold) {
            Dictionary<string, object> retDict = MakeCommonTextFieldConfig("");
            if (canFold)
                retDict.Add("foldable", 1);
            else
                retDict.Add("foldable", 0);
            return retDict;
        }
        private Dictionary<string, object> MakeDeptFieldConfig(bool isMulti) {
            Dictionary<string, object> retDict = MakeCommonTextFieldConfig("");
            if (isMulti)
                retDict.Add("multiple", 1);
            else
                retDict.Add("multiple", 0);
            return retDict;
        }
        private Dictionary<string, object> MakeHeadIconFieldConfig() {
            Dictionary<string, object> retDict = MakeCommonTextFieldConfig("");
            retDict.Add("headShape", "1");
            return retDict;
        }
        private Dictionary<string, object> MakeCommonTextFieldConfig(string regExp) {
            Dictionary<string, object> retDict = new Dictionary<string, object>();
            retDict.Add("regExp", regExp);
            return retDict;

        }
        private Dictionary<string, object> MakeDateFieldConfig(bool hasTime)
        {
            Dictionary<string, object> retDict = new Dictionary<string, object>();
            if (hasTime)
            {
                retDict.Add("format", "yyyy-MM-dd HH:mm:ss");
            }
            else { retDict.Add("format", "yyyy-MM-dd"); }

            
            retDict.Add("defaultValue", "");
            retDict.Add("regExp", "");
            return retDict;
        }
        private Dictionary<string, object> MakeDecimalFieldConfig(bool hasDefault, decimal defaultValue) {
            Dictionary<string, object> retDict = new Dictionary<string, object>();
            retDict.Add("maxLength", "20");
            retDict.Add("decimalsLength", "2");
            retDict.Add("separator", "1"); 
            retDict.Add("regExp", "");
            if (hasDefault)
            {

                retDict.Add("defaultValue", defaultValue.ToString());
            }
            return retDict;
        }
        private Dictionary<string, object> MakeIntFieldConfig(bool hasDefault,int defaultValue) {
            Dictionary<string, object> retDict = new Dictionary<string, object>();
            retDict.Add("maxLength", "20");
            retDict.Add("separator", "1");
            retDict.Add("regExp", "");
            if (hasDefault) {

                retDict.Add("defaultValue", defaultValue.ToString());
            }
            return retDict;
        }
        private Dictionary<string, object> MakeDictFieldConfig(int dicTypeId,int DefaultValueId) {
            ///"dataSource":{"type":"local","sourceKey":"dicitonary","sourceId":"65"},"regExp":""
            Dictionary<string, object> retDict = new Dictionary<string, object>();
            Dictionary<string, object> datasource = new Dictionary<string, object>();
            datasource.Add("type", "local");
            datasource.Add("sourceKey", "dicitonary");
            datasource.Add("sourceId", dicTypeId.ToString());
            retDict.Add("dataSource", datasource);
            retDict.Add("regExp", "");
            if (DefaultValueId >0)
                retDict.Add("defaultValue", DefaultValueId.ToString());
            return retDict;

        }
        private int GetDictTypeId(string dictTypeName) {
            if (DictInDb.ContainsKey(dictTypeName)) {
                return DictInDb[dictTypeName];
            }
            if (DictNeedCreate.ContainsKey(dictTypeName)) {
                return DictNeedCreate[dictTypeName];
            }
            Dictionary<string, object> dictTypeInfo = _dataSourceRepository.GetDictTypeByName(dictTypeName);
            if (dictTypeInfo == null) {
                DictNeedCreate.Add(dictTypeName, -1);
            }
            return -1;
        }
        private Dictionary<string, object> MakeBigTextFieldConfig(string defaultValue) {
            Dictionary<string, object> ret = new Dictionary<string, object>();
            ret.Add("defaultValue", defaultValue);
            ret.Add("regExp", "");
            return ret;
        }
        private Dictionary<string, object> MakeTextFieldConfig() {
            Dictionary<string, object> ret = new Dictionary<string, object>();
            ret.Add("maxLength", 1000);
            ret.Add("encrypted", 0);
            ret.Add("scanner", 0);
            ret.Add("regExp", "");
            return ret;

        }
        /// <summary>
        /// 导入文件中是否存在重复的实体名称和重复的数据库表名
        /// </summary>
        /// <param name="dictEntity"></param>
        /// <param name="dictTables"></param>
        /// <param name="listEntity"></param>
        private void CheckSaveEntityNameInMem(ref Dictionary<string, ExcelEntityInfo> dictEntity,ref Dictionary<string,string> dictTables, List<ExcelEntityInfo> listEntity) {
            foreach (ExcelEntityInfo entityInfo in listEntity)
            {
                if (dictEntity.ContainsKey(entityInfo.EntityName)) {
                    entityInfo.actionResult = new ActionResult()
                    {
                        ActionType = 0,
                        ResultCode = -1,
                        Message = "存在重复的实体名称:" + entityInfo.EntityName + ",请检查"
                    };
                }
                if (dictTables.ContainsKey(entityInfo.TableName)) {
                    entityInfo.actionResult = new ActionResult()
                    {
                        ActionType = 0,
                        ResultCode = -1,
                        Message = "存在重复的实体表名:" + entityInfo.TableName + ",请检查"
                    };
                }
                dictTables.Add(entityInfo.TableName, entityInfo.TableName);
                dictEntity.Add(entityInfo.EntityName, entityInfo);
                if (entityInfo.SubEntitys != null && entityInfo.SubEntitys.Count > 0) {
                    CheckSaveEntityNameInMem(ref dictEntity,ref  dictTables, entityInfo.SubEntitys);
                }
            }
        }

        private ExcelEntityInfo DealWithOneSheet(WorkbookPart workbookPart, Sheet sheet)
        {
            var workSheet = ((WorksheetPart)workbookPart.GetPartById(sheet.Id)).Worksheet;
            SheetData sheetData = workSheet.Elements<SheetData>().First();
            List<Row> rows = sheetData.Elements<Row>().ToList();
            var rowEnumerator = rows.GetEnumerator();
            int curRowIndex =0;
            Row EntityDefinedRow = null;
            #region 找到第一个标志位“UK100”,并获取实体表格的主要信息定义
            while (rowEnumerator.MoveNext()) {
                curRowIndex++;
                var row = rowEnumerator.Current;
                Cell cell = row.Descendants<Cell>().First();
                if (cell == null) break;
                string cellValue = GetCellValue(cell, workbookPart);
                if (cellValue != null && cellValue.ToLower().Equals("uk100")) {
                    EntityDefinedRow = row;
                    break;
                }
            }
            if (EntityDefinedRow == null) {
                throw (new Exception("没有找到实体定义信息，请检查标志位\"UK100\"是否存在"));
            }
            ExcelEntityInfo mainEntityInfo = GenerateEntityMainInfo(workbookPart, sheet, EntityDefinedRow);
            if (mainEntityInfo == null)
            {
                throw (new Exception("无法解析实体信息"));
            }
            else if (mainEntityInfo.actionResult.ResultCode <0 ) {
                return mainEntityInfo;
            }
            #endregion
            #region 读取并分析列头信息，有可能是两行合并，有可能是三行合并
            rowEnumerator.MoveNext();
            Dictionary<string, EntityTypeSettingInfo> TypeNameDict = null;
            try
            {
                TypeNameDict = GetEntityTypeList(workbookPart, sheet,ref  rowEnumerator);
                mainEntityInfo.TypeNameDict = TypeNameDict;
            }
            catch (Exception ex) {
                throw (ex);

            }
            #endregion
            #region 开始读取行数据
            while (true) {
                if (rowEnumerator.MoveNext() == false) break;
                Row row = rowEnumerator.Current;
                ExcelEntityColumnInfo columInfo = GetColumnInfo(workbookPart, sheet, row, TypeNameDict);
                if (columInfo == null) break;
                mainEntityInfo.Fields.Add(columInfo);
            }
            #endregion 
            if (rowEnumerator.Current == null) {
                return mainEntityInfo;
            }
            //继续往下查找嵌套表格
            while (true) {

                #region 找到开始标志位
                EntityDefinedRow = null;
                while(true)
                {
                    curRowIndex++;
                    var row = rowEnumerator.Current;
                    Cell cell = row.Descendants<Cell>().First();
                    if (cell == null) break;
                    string cellValue = GetCellValue(cell, workbookPart);
                    if (cellValue != null && cellValue.ToLower().Equals("uk100"))
                    {
                        EntityDefinedRow = row;
                        break;
                    }
                    if (rowEnumerator.MoveNext() == false) break;
                }
                if (EntityDefinedRow == null)
                {
                    return mainEntityInfo;
                }
                ExcelEntityInfo subEntityInfo = GenerateEntityMainInfo(workbookPart, sheet, EntityDefinedRow);
                if (subEntityInfo == null)
                {
                    throw (new Exception("无法解析实体信息"));
                }
                else if (subEntityInfo.actionResult.ResultCode <0 )
                {
                    continue;//如果发现错了，还是要继续，争取找到更多的错误
                }
                mainEntityInfo.SubEntitys.Add(subEntityInfo);
                #endregion
                #region 读取并分析列头信息，有可能是两行合并，有可能是三行合并
                rowEnumerator.MoveNext();
                try
                {
                    TypeNameDict = GetEntityTypeList(workbookPart, sheet,ref  rowEnumerator);
                    subEntityInfo.TypeNameDict = TypeNameDict;
                }
                catch (Exception ex)
                {
                    throw (ex);

                }
                #endregion
                #region 开始读取行数据
                while (true)
                {
                    if (rowEnumerator.MoveNext() == false)
                    {
                        return mainEntityInfo;
                    }
                    Row row = rowEnumerator.Current;
                    ExcelEntityColumnInfo columInfo = GetColumnInfo(workbookPart, sheet, row, TypeNameDict);
                    if (columInfo == null) break;
                    subEntityInfo.Fields.Add(columInfo);
                }
                #endregion
            }
            
        }


        /// <summary>
        ///  获取实体主要定义
        /// </summary>
        /// <param name="workbookPart"></param>
        /// <param name="sheet"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public ExcelEntityInfo GenerateEntityMainInfo(WorkbookPart workbookPart, Sheet sheet, Row row) {
            ExcelEntityInfo entityInfo = new ExcelEntityInfo();
            int CellCount = row.Descendants<Cell>().Count();
            if (CellCount < 4) {
                entityInfo.actionResult.ActionType = 0;
                entityInfo.actionResult.ResultCode = -1;
                entityInfo.actionResult.Message = "实体定义格式异常";
                return entityInfo;
            }
            Cell cell = row.Descendants<Cell>().ElementAt<Cell>(1);
            entityInfo.EntityName = GetCellValue(cell, workbookPart);
            cell = row.Descendants<Cell>().ElementAt<Cell>(2);
            entityInfo.TableName = GetCellValue(cell, workbookPart);
            cell = row.Descendants<Cell>().ElementAt<Cell>(3);
            entityInfo.EntityTypeName = GetCellValue(cell, workbookPart);
            if (entityInfo.EntityTypeName == null || !(entityInfo.EntityTypeName.Equals("独立") || entityInfo.EntityTypeName.Equals("独立实体")
                || entityInfo.EntityTypeName.Equals("简单") || entityInfo.EntityTypeName.Equals("简单实体")
                || entityInfo.EntityTypeName.Equals("嵌套") || entityInfo.EntityTypeName.Equals("嵌套实体")
                || entityInfo.EntityTypeName.Equals("动态") || entityInfo.EntityTypeName.Equals("动态实体")))
            {
                entityInfo.actionResult = new ActionResult()
                {
                    ActionType = 0 ,
                    ResultCode = -1,
                    Message  = "实体类型(D列)的值不正确，必须是独立、简单、嵌套、动态这四个值之一"
                };
                return entityInfo;
            }
            if (entityInfo.EntityName == null || entityInfo.EntityName.Length == 0) {
                
                entityInfo.actionResult = new ActionResult()
                {
                    ActionType = 0,
                    ResultCode = -1,
                    Message = "实体名称(B列)不能为空"
                };
                return entityInfo;
            }
            if (entityInfo.TableName == null || entityInfo.TableName.Length == 0)
            {
                entityInfo.actionResult = new ActionResult()
                {
                    ActionType = 0,
                    ResultCode = -1,
                    Message = "数据库表名称(C列)不能为空"
                };
                return entityInfo;
            }
            if (entityInfo.EntityTypeName.Equals("简单") || entityInfo.EntityTypeName.Equals("简单实体"))
            {
                //简单实体需要继续读取（关联实体（E列）和是否审批（N列））
                if (CellCount >= 5)
                {
                    cell = row.Descendants<Cell>().ElementAt<Cell>(4);
                    entityInfo.RelEntityName = GetCellValue(cell, workbookPart);
                }
                if (CellCount >= 14)
                {
                    cell = row.Descendants<Cell>().ElementAt<Cell>(13);
                    entityInfo.IsLinkToWorkFlow = GetCellValue(cell, workbookPart);
                }
            }
            else if (entityInfo.EntityTypeName.Equals("动态") || entityInfo.EntityTypeName.Equals("动态实体"))
            {
                //动态实体需要继续读取（关联实体（E列）和是否审批（N列）、关联实体显示字段(F列））
                if (CellCount < 5)
                {
                    entityInfo.actionResult = new ActionResult()
                    {
                        ActionType = 0,
                        ResultCode = -1,
                        Message = "动态实体必须定义关联实体(E列)"
                    };
                    return entityInfo;
                }
                cell = row.Descendants<Cell>().ElementAt<Cell>(4);
                entityInfo.RelEntityName = GetCellValue(cell, workbookPart);
                if (CellCount < 6)
                {
                    entityInfo.actionResult = new ActionResult()
                    {
                        ActionType = 0,
                        ResultCode = -1,
                        Message = "动态实体必须定义关联实体显示字段(F列）"
                    };
                    return entityInfo;
                }
                cell = row.Descendants<Cell>().ElementAt<Cell>(5);
                entityInfo.RelFieldName = GetCellValue(cell, workbookPart);
                if (CellCount >= 14)
                {
                    cell = row.Descendants<Cell>().ElementAt<Cell>(13);
                    entityInfo.IsLinkToWorkFlow = GetCellValue(cell, workbookPart);
                }
            }
            else if (entityInfo.EntityTypeName.Equals("嵌套") || entityInfo.EntityTypeName.Equals("嵌套实体")) {
                if (CellCount>=5)
                {
                    cell = row.Descendants<Cell>().ElementAt<Cell>(4);
                    entityInfo.RelEntityName = GetCellValue(cell, workbookPart);
                }
            }
            return entityInfo;
        }

        /// <summary>
        /// 分析表头数据
        /// </summary>
        /// <param name="workbookPart"></param>
        /// <param name="sheet"></param>
        /// <param name="rowEnumerator"></param>
        /// <returns></returns>
        public Dictionary<string, EntityTypeSettingInfo>  GetEntityTypeList(WorkbookPart workbookPart, Sheet sheet,ref  List<Row>.Enumerator rowEnumerator) {
            Dictionary<string, EntityTypeSettingInfo> ret = new Dictionary<string, EntityTypeSettingInfo>();
            int typeIndex = 0;
            Row row = rowEnumerator.Current;
            rowEnumerator.MoveNext();
            Row row2 = rowEnumerator.Current ;
            Row row3 = null;
            bool isThreeRow =false;
            Cell cell = row.Descendants<Cell>().ElementAt(0);
            string cellValue = GetCellValue(cell, workbookPart);
            if (cellValue == null) cellValue = "";
            if (!cellValue.Equals("序号")) {
                throw (new Exception("此处应该是'序号'，但实际'" + cellValue + "'"));
            }
            cell = row.Descendants<Cell>().ElementAt(1);
            cellValue = GetCellValue(cell, workbookPart);
            if (cellValue == null) cellValue = "";
            if (!cellValue.Equals("字段名称"))
            {
                throw (new Exception("此处应该是'字段名称'，但实际'" + cellValue + "'"));
            }

            cell = row.Descendants<Cell>().ElementAt(2);
            cellValue = GetCellValue(cell, workbookPart);
            if (cellValue == null) cellValue = "";
            if (!cellValue.Equals("存储名称"))
            {
                throw (new Exception("此处应该是'存储名称'，但实际'" + cellValue + "'"));
            }
            cell = row.Descendants<Cell>().ElementAt(3);
            cellValue = GetCellValue(cell, workbookPart);
            if (cellValue == null) cellValue = "";
            if (!cellValue.Equals("格式"))
            {
                throw (new Exception("此处应该是'格式'，但实际'" + cellValue + "'"));
            }
            cell = row.Descendants<Cell>().ElementAt(4);
            cellValue = GetCellValue(cell, workbookPart);
            if (cellValue == null) cellValue = "";
            if (!cellValue.Equals("数据源/字典"))
            {
                throw (new Exception("此处应该是'数据源/字典'，但实际'" + cellValue + "'"));
            }
            cell = row.Descendants<Cell>().ElementAt(5);
            cellValue = GetCellValue(cell, workbookPart);
            if (cellValue == null) cellValue = "";
            if (cellValue.Trim().Length == 0)
            {
                throw (new Exception("第6列应该有文本，但实际没有"));
            }
            else if (cellValue.Equals("是否使用"))
            {
                isThreeRow = false;
            }
            else if (cellValue.Equals("列表默认查看字段"))
            {
                throw (new Exception("应该提供字段查看必填配置"));
            }
            else {
                isThreeRow = true;
            }
            if (isThreeRow) {
                rowEnumerator.MoveNext();
                row3 = rowEnumerator.Current;
            }
            int curColumn =5;
            while (true) {
                string typename = "";
                if (isThreeRow)
                {
                    cell = row.Descendants<Cell>().ElementAt(curColumn);
                    cellValue = GetCellValue(cell, workbookPart);
                    if (cellValue == null || cellValue.Length == 0) {
                        throw (new Exception(string.Format("第{0}行{1}列应该出现实体类型名称", row.RowIndex + 1, curColumn + 1)));
                    }
                    typename = cellValue;
                    cell = row2.Descendants<Cell>().ElementAt(curColumn);
                    cellValue = GetCellValue(cell, workbookPart);
                    if (cellValue == null) cellValue = "";
                    if (cellValue == null || cellValue.Equals("是否使用") ==false)
                    {
                        throw (new Exception(string.Format("第{0}行{1}列应该出现'是否使用',实际是'{2}'", row2.RowIndex + 1, curColumn + 1, cellValue)));
                    }
                    curColumn++;

                    cell = row2.Descendants<Cell>().ElementAt(curColumn);
                    cellValue = GetCellValue(cell, workbookPart);
                    if (cellValue == null) cellValue = "";
                    if (cellValue == null || cellValue.Equals("新增") == false)
                    {
                        throw (new Exception(string.Format("第{0}行{1}列应该出现'新增',实际是'{2}'", row2.RowIndex + 1, curColumn + 1, cellValue)));
                    }
                    cell = row3.Descendants<Cell>().ElementAt(curColumn);
                    cellValue = GetCellValue(cell, workbookPart);
                    if (cellValue == null) cellValue = "";
                    if (cellValue == null || cellValue.Equals("显示") == false)
                    {
                        throw (new Exception(string.Format("第{0}行{1}列应该出现'显示',实际是'{2}'", row3.RowIndex + 1, curColumn + 1, cellValue)));
                    }
                    curColumn++;
                    cell = row3.Descendants<Cell>().ElementAt(curColumn);
                    cellValue = GetCellValue(cell, workbookPart);
                    if (cellValue == null) cellValue = "";
                    if (cellValue == null || cellValue.Equals("必填") == false)
                    {
                        throw (new Exception(string.Format("第{0}行{1}列应该出现'必填',实际是'{2}'", row3.RowIndex + 1, curColumn + 1, cellValue)));
                    }
                    curColumn++;
                    cell = row3.Descendants<Cell>().ElementAt(curColumn);
                    cellValue = GetCellValue(cell, workbookPart);
                    if (cellValue == null) cellValue = "";
                    if (cellValue == null || cellValue.Equals("只读") == false)
                    {
                        throw (new Exception(string.Format("第{0}行{1}列应该出现'只读',实际是'{2}'", row3.RowIndex + 1, curColumn + 1, cellValue)));
                    }
                    curColumn++;

                    cell = row2.Descendants<Cell>().ElementAt(curColumn);
                    cellValue = GetCellValue(cell, workbookPart);
                    if (cellValue == null) cellValue = "";
                    if (cellValue == null || cellValue.Equals("编辑") == false)
                    {
                        throw (new Exception(string.Format("第{0}行{1}列应该出现'编辑',实际是'{2}'", row2.RowIndex + 1, curColumn + 1, cellValue)));
                    }
                    cell = row3.Descendants<Cell>().ElementAt(curColumn);
                    cellValue = GetCellValue(cell, workbookPart);
                    if (cellValue == null) cellValue = "";
                    if (cellValue == null || cellValue.Equals("显示") == false)
                    {
                        throw (new Exception(string.Format("第{0}行{1}列应该出现'显示',实际是'{2}'", row3.RowIndex + 1, curColumn + 1, cellValue)));
                    }
                    curColumn++;
                    cell = row3.Descendants<Cell>().ElementAt(curColumn);
                    cellValue = GetCellValue(cell, workbookPart);
                    if (cellValue == null) cellValue = "";
                    if (cellValue == null || cellValue.Equals("必填") == false)
                    {
                        throw (new Exception(string.Format("第{0}行{1}列应该出现'必填',实际是'{2}'", row3.RowIndex + 1, curColumn + 1, cellValue)));
                    }
                    curColumn++;
                    cell = row3.Descendants<Cell>().ElementAt(curColumn);
                    cellValue = GetCellValue(cell, workbookPart);
                    if (cellValue == null) cellValue = "";
                    if (cellValue == null || cellValue.Equals("只读") == false)
                    {
                        throw (new Exception(string.Format("第{0}行{1}列应该出现'只读',实际是'{2}'", row3.RowIndex + 1, curColumn + 1, cellValue)));
                    }
                    curColumn++;

                    cell = row2.Descendants<Cell>().ElementAt(curColumn);
                    cellValue = GetCellValue(cell, workbookPart);
                    if (cellValue == null) cellValue = "";
                    if (cellValue == null || cellValue.Equals("查看") == false)
                    {
                        throw (new Exception(string.Format("第{0}行{1}列应该出现'查看',实际是'{2}'", row2.RowIndex + 1, curColumn + 1, cellValue)));
                    }
                    cell = row3.Descendants<Cell>().ElementAt(curColumn);
                    cellValue = GetCellValue(cell, workbookPart);
                    if (cellValue == null) cellValue = "";
                    if (cellValue == null || cellValue.Equals("显示") == false)
                    {
                        throw (new Exception(string.Format("第{0}行{1}列应该出现'显示',实际是'{2}'", row3.RowIndex + 1, curColumn + 1, cellValue)));
                    }
                    curColumn++;
                    cell = row.Descendants<Cell>().ElementAt(curColumn);
                    cellValue = GetCellValue(cell, workbookPart);
                    if (cellValue == null) cellValue = "";
                    if (cellValue.Equals("列表默认查看字段"))
                    {
                        //全部结束
                        ret.Add(typeIndex.ToString(), new EntityTypeSettingInfo() {
                            TypeName = typename,
                            IsImport = false
                        });
                        typeIndex++;
                        break;
                    }
                    else {
                        //可能是导入配置，也可能是下一个分类
                        cell = row2.Descendants<Cell>().ElementAt(curColumn);
                        cellValue = GetCellValue(cell, workbookPart);
                        if (cellValue == null) cellValue = "";
                        if (cellValue.Equals("导入"))
                        {
                            //这里要处理导入
                            cell = row3.Descendants<Cell>().ElementAt(curColumn);
                            cellValue = GetCellValue(cell, workbookPart);
                            if (cellValue == null) cellValue = "";
                            if (cellValue.Equals("显示") == false) {
                                throw (new Exception(string.Format("第{0}行{1}列应该出现'显示',实际是'{2}'", row3.RowIndex + 1, curColumn + 1, cellValue)));
                            }
                            curColumn++;
                            cell = row3.Descendants<Cell>().ElementAt(curColumn);
                            cellValue = GetCellValue(cell, workbookPart);
                            if (cellValue == null) cellValue = "";
                            if (cellValue.Equals("必填") == false)
                            {
                                throw (new Exception(string.Format("第{0}行{1}列应该出现'必填',实际是'{2}'", row3.RowIndex + 1, curColumn + 1, cellValue)));
                            }
                            curColumn++;
                            ret.Add(typeIndex.ToString(), new EntityTypeSettingInfo()
                            {
                                TypeName = typename,
                                IsImport = true
                            });
                            typeIndex++;
                            cell = row.Descendants<Cell>().ElementAt(curColumn);
                            cellValue = GetCellValue(cell, workbookPart);
                            if (cellValue == null) cellValue = "";
                            if (cellValue.Equals("列表默认查看字段"))
                            {
                                break;
                            }
                        }
                        else {

                            ret.Add(typeIndex.ToString(), new EntityTypeSettingInfo()
                            {
                                TypeName = typename,
                                IsImport = false
                            });
                            typeIndex++;
                            continue;//下一个实体分类尝试
                        }
                    }

                }
                else {
                    typename = "默认分类";
                    cell = row.Descendants<Cell>().ElementAt(curColumn);
                    cellValue = GetCellValue(cell, workbookPart);
                    if (cellValue == null) cellValue = "";
                    if (cellValue == null || cellValue.Equals("是否使用") == false)
                    {
                        throw (new Exception(string.Format("第{0}行{1}列应该出现'是否使用',实际是'{2}'", row.RowIndex + 1, curColumn + 1, cellValue)));
                    }
                    curColumn++;
                    cell = row.Descendants<Cell>().ElementAt(curColumn);
                    cellValue = GetCellValue(cell, workbookPart);
                    if (cellValue == null) cellValue = "";
                    if (cellValue == null || cellValue.Equals("新增") == false)
                    {
                        throw (new Exception(string.Format("第{0}行{1}列应该出现'新增',实际是'{2}'", row.RowIndex + 1, curColumn + 1, cellValue)));
                    }
                    cell = row2.Descendants<Cell>().ElementAt(curColumn);
                    cellValue = GetCellValue(cell, workbookPart);
                    if (cellValue == null) cellValue = "";
                    if (cellValue == null || cellValue.Equals("显示") == false)
                    {
                        throw (new Exception(string.Format("第{0}行{1}列应该出现'显示',实际是'{2}'", row2.RowIndex + 1, curColumn + 1, cellValue)));
                    }
                    curColumn++;
                    cell = row2.Descendants<Cell>().ElementAt(curColumn);
                    cellValue = GetCellValue(cell, workbookPart);
                    if (cellValue == null) cellValue = "";
                    if (cellValue == null || cellValue.Equals("必填") == false)
                    {
                        throw (new Exception(string.Format("第{0}行{1}列应该出现'必填',实际是'{2}'", row2.RowIndex + 1, curColumn + 1, cellValue)));
                    }
                    curColumn++;
                    cell = row2.Descendants<Cell>().ElementAt(curColumn);
                    cellValue = GetCellValue(cell, workbookPart);
                    if (cellValue == null) cellValue = "";
                    if (cellValue == null || cellValue.Equals("只读") == false)
                    {
                        throw (new Exception(string.Format("第{0}行{1}列应该出现'只读',实际是'{2}'", row2.RowIndex + 1, curColumn + 1, cellValue)));
                    }
                    curColumn++;
                    cell = row.Descendants<Cell>().ElementAt(curColumn);
                    cellValue = GetCellValue(cell, workbookPart);
                    if (cellValue == null) cellValue = "";
                    if (cellValue == null || cellValue.Equals("编辑") == false)
                    {
                        throw (new Exception(string.Format("第{0}行{1}列应该出现'编辑',实际是'{2}'", row.RowIndex + 1, curColumn + 1, cellValue)));
                    }
                    cell = row2.Descendants<Cell>().ElementAt(curColumn);
                    cellValue = GetCellValue(cell, workbookPart);
                    if (cellValue == null) cellValue = "";
                    if (cellValue == null || cellValue.Equals("显示") == false)
                    {
                        throw (new Exception(string.Format("第{0}行{1}列应该出现'显示',实际是'{2}'", row2.RowIndex + 1, curColumn + 1, cellValue)));
                    }
                    curColumn++;
                    cell = row2.Descendants<Cell>().ElementAt(curColumn);
                    cellValue = GetCellValue(cell, workbookPart);
                    if (cellValue == null) cellValue = "";
                    if (cellValue == null || cellValue.Equals("必填") == false)
                    {
                        throw (new Exception(string.Format("第{0}行{1}列应该出现'必填',实际是'{2}'", row2.RowIndex + 1, curColumn + 1, cellValue)));
                    }
                    curColumn++;
                    cell = row2.Descendants<Cell>().ElementAt(curColumn);
                    cellValue = GetCellValue(cell, workbookPart);
                    if (cellValue == null) cellValue = "";
                    if (cellValue == null || cellValue.Equals("只读") == false)
                    {
                        throw (new Exception(string.Format("第{0}行{1}列应该出现'只读',实际是'{2}'", row2.RowIndex + 1, curColumn + 1, cellValue)));
                    }
                    curColumn++;

                    cell = row.Descendants<Cell>().ElementAt(curColumn);
                    cellValue = GetCellValue(cell, workbookPart);
                    if (cellValue == null) cellValue = "";
                    if (cellValue == null || cellValue.Equals("查看") == false)
                    {
                        throw (new Exception(string.Format("第{0}行{1}列应该出现'查看',实际是'{2}'", row.RowIndex + 1, curColumn + 1, cellValue)));
                    }
                    cell = row2.Descendants<Cell>().ElementAt(curColumn);
                    cellValue = GetCellValue(cell, workbookPart);
                    if (cellValue == null) cellValue = "";
                    if (cellValue == null || cellValue.Equals("显示") == false)
                    {
                        throw (new Exception(string.Format("第{0}行{1}列应该出现'显示',实际是'{2}'", row2.RowIndex + 1, curColumn + 1, cellValue)));
                    }
                    curColumn++;
                    cell = row.Descendants<Cell>().ElementAt(curColumn);
                    cellValue = GetCellValue(cell, workbookPart);
                    if (cellValue == null) cellValue = "";
                    if (cellValue.Equals("列表默认查看字段"))
                    {

                        ret.Add(typeIndex.ToString(), new EntityTypeSettingInfo()
                        {
                            TypeName = typename,
                            IsImport = false
                        });
                        typeIndex++;
                        break;
                    }
                    else {
                        if ( cellValue.Equals("导入") == false)
                        {
                            throw (new Exception(string.Format("第%d行%d列应该出现'导入',实际是'{2}'", row.RowIndex + 1, curColumn + 1, cellValue)));
                        }
                        cell = row2.Descendants<Cell>().ElementAt(curColumn);
                        cellValue = GetCellValue(cell, workbookPart);
                        if (cellValue == null) cellValue = "";
                        if (cellValue == null || cellValue.Equals("显示") == false)
                        {
                            throw (new Exception(string.Format("第%d行%d列应该出现'显示',实际是'{2}'", row2.RowIndex + 1, curColumn + 1, cellValue)));
                        }
                        curColumn++;
                        cell = row2.Descendants<Cell>().ElementAt(curColumn);
                        cellValue = GetCellValue(cell, workbookPart);
                        if (cellValue == null) cellValue = "";
                        if (cellValue == null || cellValue.Equals("必填") == false)
                        {
                            throw (new Exception(string.Format("第%d行%d列应该出现'必填',实际是'{2}'", row2.RowIndex + 1, curColumn + 1, cellValue)));
                        }
                        curColumn++;

                        ret.Add(typeIndex.ToString(), new EntityTypeSettingInfo()
                        {
                            TypeName = typename,
                            IsImport = true
                        });
                        typeIndex++;
                        break;
                    }
                    
                    
                }
            }
            cell = row.Descendants<Cell>().ElementAt(curColumn);
            cellValue = GetCellValue(cell, workbookPart);
            if (cellValue == null) cellValue = "";
            if (cellValue.Equals("列表默认查看字段") ==false  )
            {
                throw (new Exception(string.Format("第%d行%d列应该出现'列表默认查看字段',实际是'{2}'", row.RowIndex + 1, curColumn + 1, cellValue)));
            }
            curColumn++;
            cell = row.Descendants<Cell>().ElementAt(curColumn);
            cellValue = GetCellValue(cell, workbookPart);
            if (cellValue == null) cellValue = "";
            if (cellValue.Equals("默认值") == false)
            {
                throw (new Exception(string.Format("第%d行%d列应该出现'默认值',实际是'{2}'", row.RowIndex + 1, curColumn + 1, cellValue)));
            }
            curColumn++;
            cell = row.Descendants<Cell>().ElementAt(curColumn);
            cellValue = GetCellValue(cell, workbookPart);
            if (cellValue == null) cellValue = "";
            if (cellValue.Equals("高级搜索默认筛选") == false)
            {
                throw (new Exception(string.Format("第%d行%d列应该出现'高级搜索默认筛选',实际是'{2}'", row.RowIndex + 1, curColumn + 1, cellValue)));
            }
            curColumn++;
            cell = row.Descendants<Cell>().ElementAt(curColumn);
            cellValue = GetCellValue(cell, workbookPart);
            if (cellValue == null) cellValue = "";
            if (cellValue.Equals("默认查重字段") == false)
            {
                throw (new Exception(string.Format("第%d行%d列应该出现'默认查重字段',实际是'{2}'", row.RowIndex + 1, curColumn + 1, cellValue)));
            }
            curColumn++;
            cell = row.Descendants<Cell>().ElementAt(curColumn);
            cellValue = GetCellValue(cell, workbookPart);
            if (cellValue == null) cellValue = "";
            if (cellValue.Equals("说明") == false)
            {
                throw (new Exception(string.Format("第%d行%d列应该出现'说明',实际是'{2}'", row.RowIndex + 1, curColumn + 1, cellValue)));
            }
            curColumn++;
            return ret;
        }

        public ExcelEntityColumnInfo GetColumnInfo(WorkbookPart workbookPart, Sheet sheet, Row row, Dictionary<string, EntityTypeSettingInfo> TypeNameDict)
        {
            ExcelEntityColumnInfo columnInfo = new ExcelEntityColumnInfo();
            int columnIndex = 0;
            Cell cell = null;
            string cellValue = null;
            cell = row.Descendants<Cell>().ElementAt(columnIndex);
            cellValue = GetCellValue(cell, workbookPart);
            if (cellValue == null) cellValue = "";
            if (cellValue.Length == 0) return null;//代表这一行已经是空行了
            int tmp = 0;
            if (int.TryParse(cellValue, out tmp) == false) {
                // columnInfo.ErrorMsg = string.Format("{0}\r\n 第{1}行第{2}列应该是数字，实际为{3}", columnInfo.ErrorMsg, row.RowIndex + 1, columnIndex + 1, cellValue);
                return null;
            }
            columnInfo.Index = tmp;
            columnIndex++;

            cell = row.Descendants<Cell>().ElementAt(columnIndex);
            cellValue = GetCellValue(cell, workbookPart);
            if (cellValue == null) cellValue = "";
            if (cellValue.Length == 0) {
                columnInfo.ErrorMsg = string.Format("{0}\r\n 第{1}行第{2}列应该是字段名称,不能为空", columnInfo.ErrorMsg, row.RowIndex + 1, columnIndex + 1);
            }
            columnInfo.DisplayName = cellValue;
            columnIndex++;

            cell = row.Descendants<Cell>().ElementAt(columnIndex);
            cellValue = GetCellValue(cell, workbookPart);
            if (cellValue == null) cellValue = "";
            columnInfo.FieldName = cellValue;
            columnIndex++;

            cell = row.Descendants<Cell>().ElementAt(columnIndex);
            cellValue = GetCellValue(cell, workbookPart);
            if (cellValue == null) cellValue = "";
            columnInfo.FieldTypeName = cellValue;
            columnIndex++;

            cell = row.Descendants<Cell>().ElementAt(columnIndex);
            cellValue = GetCellValue(cell, workbookPart);
            if (cellValue == null) cellValue = "";
            columnInfo.DataSourceName = cellValue;
            columnIndex++;

            int entitytypecount = TypeNameDict.Count;
            for (int i = 0; i < entitytypecount; i++) {
                EntityTypeSettingInfo typeSettingInfo = TypeNameDict[i.ToString()];

                ExcelEntityFieldViewInfo viewInfo = new ExcelEntityFieldViewInfo();
                cell = row.Descendants<Cell>().ElementAt(columnIndex);
                cellValue = GetCellValue(cell, workbookPart);
                if (cellValue == null) cellValue = "";
                if (cellValue.Equals("√")) viewInfo.IsEnable = true; else viewInfo.IsEnable = false;
                columnIndex++;


                cell = row.Descendants<Cell>().ElementAt(columnIndex);
                cellValue = GetCellValue(cell, workbookPart);
                if (cellValue == null) cellValue = "";
                if (cellValue.Equals("√")) viewInfo.AddNew_Display = true; else viewInfo.AddNew_Display = false;
                columnIndex++;
                cell = row.Descendants<Cell>().ElementAt(columnIndex);
                cellValue = GetCellValue(cell, workbookPart);
                if (cellValue == null) cellValue = "";
                if (cellValue.Equals("√")) viewInfo.AddNew_Must = true; else viewInfo.AddNew_Must = false;
                columnIndex++;
                cell = row.Descendants<Cell>().ElementAt(columnIndex);
                cellValue = GetCellValue(cell, workbookPart);
                if (cellValue == null) cellValue = "";
                if (cellValue.Equals("√")) viewInfo.AddNew_Readonly = true; else viewInfo.AddNew_Readonly = false;
                columnIndex++;

                cell = row.Descendants<Cell>().ElementAt(columnIndex);
                cellValue = GetCellValue(cell, workbookPart);
                if (cellValue == null) cellValue = "";
                if (cellValue.Equals("√")) viewInfo.Edit_Display = true; else viewInfo.Edit_Display = false;
                columnIndex++;
                cell = row.Descendants<Cell>().ElementAt(columnIndex);
                cellValue = GetCellValue(cell, workbookPart);
                if (cellValue == null) cellValue = "";
                if (cellValue.Equals("√")) viewInfo.Edit_Must = true; else viewInfo.Edit_Must = false;
                columnIndex++;
                cell = row.Descendants<Cell>().ElementAt(columnIndex);
                cellValue = GetCellValue(cell, workbookPart);
                if (cellValue == null) cellValue = "";
                if (cellValue.Equals("√")) viewInfo.Edit_Readonly = true; else viewInfo.Edit_Readonly = false;
                columnIndex++;


                cell = row.Descendants<Cell>().ElementAt(columnIndex);
                cellValue = GetCellValue(cell, workbookPart);
                if (cellValue == null) cellValue = "";
                if (cellValue.Equals("√")) viewInfo.View_Display = true; else viewInfo.View_Display = false;
                columnIndex++;
                if (typeSettingInfo.IsImport)
                {

                    cell = row.Descendants<Cell>().ElementAt(columnIndex);
                    cellValue = GetCellValue(cell, workbookPart);
                    if (cellValue == null) cellValue = "";
                    if (cellValue.Equals("√")) viewInfo.Import_Display = true; else viewInfo.Import_Display = false;
                    columnIndex++;

                    cell = row.Descendants<Cell>().ElementAt(columnIndex);
                    cellValue = GetCellValue(cell, workbookPart);
                    if (cellValue == null) cellValue = "";
                    if (cellValue.Equals("√")) viewInfo.Import_Must = true; else viewInfo.Import_Must = false;
                    columnIndex++;
                }
                columnInfo.ViewSet.Add(typeSettingInfo.TypeName, viewInfo);
            }

            if (row.Descendants<Cell>().Count() > columnIndex)
            {
                cell = row.Descendants<Cell>().ElementAt(columnIndex);
                cellValue = GetCellValue(cell, workbookPart);
                if (cellValue == null) cellValue = "";
                if (cellValue.Equals("√")) columnInfo.IsWebListField = true; else columnInfo.IsWebListField = false;
                columnIndex++;
            }
            if (row.Descendants<Cell>().Count() > columnIndex)
            {
                cell = row.Descendants<Cell>().ElementAt(columnIndex);
                cellValue = GetCellValue(cell, workbookPart);
                if (cellValue == null) cellValue = "";
                columnInfo.DefaultValue = cellValue;
                columnIndex++;
            }
            if (row.Descendants<Cell>().Count() > columnIndex)
            {
                cell = row.Descendants<Cell>().ElementAt(columnIndex);
                cellValue = GetCellValue(cell, workbookPart);
                if (cellValue == null) cellValue = "";
                if (cellValue.Equals("√")) columnInfo.IsAdvanceSearch = true; else columnInfo.IsAdvanceSearch = false;
                columnIndex++;
            }
            if (row.Descendants<Cell>().Count() > columnIndex)
            {
                cell = row.Descendants<Cell>().ElementAt(columnIndex);
                cellValue = GetCellValue(cell, workbookPart);
                if (cellValue == null) cellValue = "";
                if (cellValue.Equals("√")) columnInfo.IsCheckSameField = true; else columnInfo.IsCheckSameField = false;
                columnIndex++;
            }
            if (row.Descendants<Cell>().Count() > columnIndex)
            {
                cell = row.Descendants<Cell>().ElementAt(columnIndex);
                cellValue = GetCellValue(cell, workbookPart);
                if (cellValue == null) cellValue = "";
                columnInfo.Remark = cellValue;
                columnIndex++;
            }
            return columnInfo;
        }

        public string GetCellValue(Cell cell, WorkbookPart workBookPart)
        {
            string cellValue = string.Empty;
            if (cell.ChildElements.Count == 0)//Cell节点下没有子节点
            {
                return cellValue;
            }
            string cellRefId = cell.CellReference.InnerText;//获取引用相对位置
            string cellInnerText = cell.CellValue.InnerText;//获取Cell的InnerText
            cellValue = cellInnerText;//指定默认值(其实用来处理Excel中的数字)

            //获取WorkbookPart中NumberingFormats样式集合
            Dictionary<string,string> dicStyles = GetNumberFormatsStyle(workBookPart);
            //获取WorkbookPart中共享String数据
            SharedStringTable sharedTable = workBookPart.SharedStringTablePart.SharedStringTable;

            try
            {
                EnumValue<CellValues> cellType = cell.DataType;//获取Cell数据类型
                if (cellType != null)//Excel对象数据
                {
                    switch (cellType.Value)
                    {
                        case CellValues.SharedString://字符串
                            //获取该Cell的所在的索引
                            int cellIndex = int.Parse(cellInnerText);
                            cellValue = sharedTable.ChildElements[cellIndex].InnerText;
                            break;
                        case CellValues.Boolean://布尔
                            cellValue = (cellInnerText == "1") ? "TRUE" : "FALSE";
                            break;
                        case CellValues.Date://日期
                            cellValue = Convert.ToDateTime(cellInnerText).ToString();
                            break;
                        case CellValues.Number://数字
                            cellValue = Convert.ToDecimal(cellInnerText).ToString();
                            break;
                        default: cellValue = cellInnerText; break;
                    }
                }
                else//格式化数据
                {
                    if (dicStyles.Count > 0 && cell.StyleIndex != null)//对于数字,cell.StyleIndex==null
                    {
                        int styleIndex = Convert.ToInt32(cell.StyleIndex.Value);
                        string cellStyle = "";
                        if (dicStyles.ContainsKey(styleIndex.ToString())) {
                            cellStyle = dicStyles[styleIndex.ToString()];
                        }
                        if (cellStyle.Contains("yyyy") || cellStyle.Contains("h")
                            || cellStyle.Contains("dd") || cellStyle.Contains("ss"))
                        {
                            //如果为日期或时间进行格式处理,去掉“;@”
                            cellStyle = cellStyle.Replace(";@", "");
                            while (cellStyle.Contains("[") && cellStyle.Contains("]"))
                            {
                                int otherStart = cellStyle.IndexOf('[');
                                int otherEnd = cellStyle.IndexOf("]");

                                cellStyle = cellStyle.Remove(otherStart, otherEnd - otherStart + 1);
                            }
                            double doubleDateTime = double.Parse(cellInnerText);
                            DateTime dateTime = DateTime.FromOADate(doubleDateTime);//将Double日期数字转为日期格式
                            if (cellStyle.Contains("m")) { cellStyle = cellStyle.Replace("m", "M"); }
                            if (cellStyle.Contains("AM/PM")) { cellStyle = cellStyle.Replace("AM/PM", ""); }
                            cellValue = dateTime.ToString(cellStyle);//不知道为什么Excel 2007中格式日期为yyyy/m/d
                        }
                        else//其他的货币、数值
                        {
                            try
                            {
                                if (cellStyle.LastIndexOf('.') > 0)
                                    cellStyle = cellStyle.Substring(cellStyle.LastIndexOf('.') - 1).Replace("\\", "");
                                else
                                    cellStyle = cellStyle.Replace("\\", "");
                                decimal decimalNum = decimal.Parse(cellInnerText);
                                cellValue = decimal.Parse(decimalNum.ToString(cellStyle)).ToString();
                            }
                            catch (Exception ex) { }
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                cellValue = "N/A";
            }
            return cellValue;
        }
        /// <summary>
        /// 根据WorkbookPart获取NumberingFormats样式集合
        /// </summary>
        /// <param name="workBookPart">WorkbookPart对象</param>
        /// <returns>NumberingFormats样式集合</returns>
        private Dictionary<string,string> GetNumberFormatsStyle(WorkbookPart workBookPart)
        {
            Dictionary<string, string> dicStyle = new Dictionary<string, string>();
            Stylesheet styleSheet = workBookPart.WorkbookStylesPart.Stylesheet;
            OpenXmlElementList list = styleSheet.NumberingFormats.ChildElements;//获取NumberingFormats样式集合

            foreach (var element in list)//格式化节点
            {
                if (element.HasAttributes)
                {
                    using (OpenXmlReader reader = OpenXmlReader.Create(element))
                    {
                        if (reader.Read())
                        {
                            if (reader.Attributes.Count > 0)
                            {
                                string numFmtId = reader.Attributes[0].Value;//格式化ID
                                string formatCode = reader.Attributes[1].Value;//格式化Code
                                dicStyle.Add(numFmtId,formatCode);//将格式化Code写入List集合
                            }
                        }
                    }
                }
            }
            return dicStyle;
        }

        #endregion
    }
    public class ActionResult {
        public int ResultCode { get; set; }
        public int ActionType { get; set; }
        public string Message { get; set; }
    }
    #region 以下是定义相关的中间的信息存储类
    public class ExcelEntityInfo {
        public Guid EntityId { get; set;  }
        public string EntityName { get; set; }
        public string TableName { get; set; }
        public string EntityTypeName { get; set; }
        public Guid RelEntityId { get; set; }
        public string RelEntityName { get; set; }
        public Guid RelFieldId { get; set; }
        public string RelFieldName { get; set; }
        public string IsLinkToWorkFlow { get; set; }
        
        public List<ExcelEntityColumnInfo> Fields { get; set; }
        public List<ExcelEntityInfo> SubEntitys { get; set; }
        public bool IsUpdate { get; set; }
        public Dictionary<string, EntityTypeSettingInfo> TypeNameDict { get; set; }
        public ActionResult actionResult { get; set;  }
        public ExcelEntityInfo() {
            Fields = new List<ExcelEntityColumnInfo>();
            SubEntitys = new List<ExcelEntityInfo>();
            TypeNameDict = new Dictionary<string, EntityTypeSettingInfo>();
            EntityId = Guid.Empty;
            EntityName = "";
            TableName = "";
            EntityTypeName = "";
            RelEntityId = Guid.Empty;
            RelEntityName = "";
            RelFieldName = "";
            IsUpdate = false;
            actionResult = new ActionResult();

        }
    }
    public class ExcelEntityColumnInfo {
        public Guid FieldId { get; set; }
        public int Index { get; set; }
        public string DisplayName { get; set; }
        public string FieldName { get; set; }
        public string FieldTypeName { get; set; }
        public int ControlType { get; set; }
        public int FieldType { get; set; }
        public Dictionary<string, object> FieldConfig { get; set; }
        public string DataSourceName { get; set; }
        public bool IsWebListField { get; set; }
        public string DefaultValue { get; set; }
        public bool IsAdvanceSearch { get; set; }
        public bool IsCheckSameField { get; set; }
        public string Remark { get; set; }
        public string RelField_OriginEntityId { get; set; }
        public string RelField_originFieldName { get; set; }
        public string RelField_originFieldId { get; set; }
        public string RelField_ControlFieldName { get; set; }
        public string RelField_ControlFieldId { get; set; }
        public string Table_EntityName { get; set; }
        public string Table_EntityId { get; set; }
        public string Table_TitleDisplayName { get; set; }
        public string Table_TitleFieldName { get; set; }
        public string ErrorMsg { get; set; }
        public bool IsUpdate { get; set; }
        public Dictionary<string, ExcelEntityFieldViewInfo> ViewSet { get; set; }
        public ActionResult actionResult { get; set; }
        public ExcelEntityColumnInfo() {
            ViewSet = new Dictionary<string, ExcelEntityFieldViewInfo>();
            FieldConfig = new Dictionary<string, object>();
            IsUpdate = false;
            actionResult = new ActionResult();
        }
    }
    public class ExcelEntityFieldViewInfo {
        public bool IsEnable { get; set; }
        public bool AddNew_Display { get; set; }
        public bool AddNew_Must { get; set; }
        public bool AddNew_Readonly { get; set; }

        public bool Edit_Display { get; set; }
        public bool Edit_Must { get; set; }
        public bool Edit_Readonly { get; set; }
        public bool View_Display { get; set; }
        public bool Import_Display{ get; set; }
        public bool Import_Must { get; set; }
    }
    public class EntityTypeSettingInfo {
        public List<Guid> CatelogIds { get; set; }
        public string TypeName { get; set; }
        public bool IsImport { get; set; }

        public ActionResult actionResult { get; set; }
        public EntityTypeSettingInfo() {
            CatelogIds = new List<Guid>();
            actionResult = new ActionResult();
        }
    }
    public class ExcelDataSourceInfo {
        public string DataSourceId{ get; set; }
        public string DataSourceName { get; set; }
        public ActionResult actionResult { get; set; }
        public ExcelDataSourceInfo() {
            actionResult = new ActionResult();
        }
    }
    public class ExcelDictTypeInfo {
        public int DictType { get; set; }
        public string DictTypeName { get; set; }
        public ActionResult actionResult { get; set; }
        public ExcelDictTypeInfo()
        {

            actionResult = new ActionResult();
        }
    }
    #endregion
}
