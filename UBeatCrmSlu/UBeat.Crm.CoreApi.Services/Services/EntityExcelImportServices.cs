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

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class EntityExcelImportServices : EntityBaseServices
    {
        public static string[] DefaultFieldName = new string[] { "recid", "recname", "recmananger", "reccreator", "reccreated", "recupdator", "recupdated", "recstatus", "recversion" };
        private Dictionary<string, int> DictInDb = new Dictionary<string, int>();
        private Dictionary<string, int> DictNeedCreate = new Dictionary<string, int>();
        private Dictionary<string, Dictionary<string, object>> DataSourceInDb = new Dictionary<string, Dictionary<string, object>>();
        private Dictionary<string, string> DataSourceNeedCreate = new Dictionary<string, string>();
        private Dictionary<string, ExcelEntityInfo> dictEntity = null;
        private Dictionary<string, string> dictTable = null;
        private readonly IEntityProRepository _entityProRepository;
        private readonly IDataSourceRepository _dataSourceRepository;
        private readonly EntityProServices _entityProServices;

        public EntityExcelImportServices(IEntityProRepository entityProRepository,
                            IDataSourceRepository dataSourceRepository,
                            EntityProServices entityProServices)
        {
            _entityProRepository = entityProRepository;
            _dataSourceRepository = dataSourceRepository;
            _entityProServices = entityProServices;
        }
        #region 通过Excel导入实体配置,这里将会是一个大的代码块 
        public void ImportEntityFromExcel()
        {
            //this._excelServices

            System.IO.Stream r = new System.IO.FileStream(@"d:\配置导入示例.xlsx", System.IO.FileMode.Open);
            WorkbookPart workbookPart;
            try
            {
                var document = SpreadsheetDocument.Open(r, false);
                workbookPart = document.WorkbookPart;
                var sheet = workbookPart.Workbook.Descendants<Sheet>().FirstOrDefault();
                ExcelEntityInfo entityInfo = DealWithOneSheet(workbookPart, sheet);
                List<ExcelEntityInfo> listEntity = new List<ExcelEntityInfo>();
                listEntity.Add(entityInfo);
                CheckEntityAndField(listEntity);
                //先建立所有相关表的实体

            }
            catch (Exception ex)
            {
            }
            r.Close();
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
                    Relaudit = Relaudit

                };
                this._entityProServices.InsertEntityPro(model, 1);
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
                throw (new Exception("检查失败，实体【" + entityInfo.EntityName + "】必须是独立、简单或者动态实体"));
            }
            if (entityInfo.EntityTypeName.StartsWith("简单") && entityInfo.RelEntityName.Length > 0) {
                if (dictEntity.ContainsKey(entityInfo.RelEntityName)) {
                    ExcelEntityInfo relEntityInfo = dictEntity[entityInfo.RelEntityName];
                    if (!(relEntityInfo.EntityTypeName.StartsWith("独立") || relEntityInfo.EntityName.StartsWith("简单"))) {
                        throw (new Exception("简单实体只能关联简单实体或者独立实体"));
                    }
                }
                else {
                    //从数据库中获取实体定义信息
                    Dictionary<string, object> searchEntityInfo = this._entityProRepository.GetEntityInfoByEntityName(null, entityInfo.RelEntityName, 1);
                    if (searchEntityInfo != null)
                    {
                        int modeltype = int.Parse(searchEntityInfo["modeltype"].ToString());
                        if (!(modeltype == 0 || modeltype == 2)) {
                            throw (new Exception("简单实体只能关联简单实体或者独立实体"));
                        }
                        entityInfo.RelEntityId = Guid.Parse(searchEntityInfo["entityid"].ToString());
                    }
                    else {
                        throw (new Exception("无法找到实体定义" + entityInfo.RelEntityName));
                    }
                }
            } else if (entityInfo.EntityTypeName.StartsWith("独立") && entityInfo.RelEntityName.Length > 0) {
                throw (new Exception("独立实体不能有关联实体"));
            } else if (entityInfo.EntityTypeName.StartsWith("动态")) {
                if (entityInfo.RelEntityName.Length == 0 || entityInfo.RelFieldName.Length == 0) {
                    throw (new Exception("动态实体必须关联实体"));
                }
                if (dictEntity.ContainsKey(entityInfo.RelEntityName))
                {
                    ExcelEntityInfo relEntityInfo = dictEntity[entityInfo.RelEntityName];
                    if (!(relEntityInfo.EntityTypeName.StartsWith("独立") || relEntityInfo.EntityName.StartsWith("简单")))
                    {
                        throw (new Exception("简单实体只能关联简单实体或者独立实体"));
                    }
                    //需要检查是否存在字段
                    bool isFoundField = false;
                    foreach (ExcelEntityColumnInfo fieldInfo in relEntityInfo.Fields) {
                        if (!(fieldInfo.DisplayName.Equals(entityInfo.RelFieldName) || fieldInfo.FieldName.Equals(entityInfo.RelFieldName))) {
                            throw (new Exception("实体定义中，没有找到关联的字段信息"));
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
                            throw (new Exception("简单实体只能关联简单实体或者独立实体"));
                        }
                        entityInfo.RelEntityId = Guid.Parse(searchEntityInfo["entityid"].ToString());
                    }
                    else
                    {
                        throw (new Exception("无法找到实体定义" + entityInfo.RelEntityName));
                    }
                    Dictionary<string, object> searchFieldInfo = this._entityProRepository.GetFieldInfoByFieldName(null, entityInfo.RelFieldName, entityInfo.RelEntityId, 1);
                    if (searchFieldInfo == null) {
                        throw (new Exception("实体定义中，没有找到关联的字段信息"));
                    }
                    entityInfo.RelEntityId = Guid.Parse(searchFieldInfo["fieldid"].ToString());
                }
            }

            //开始检查实体定义是否在表中存在
            Dictionary<string, object> oldEntityInDb = this._entityProRepository.GetEntityInfoByEntityName(null, entityInfo.EntityName, 1);
            if (oldEntityInDb != null) {
                throw (new Exception("实体【" + entityInfo.EntityName + "】已经存在"));
            }
            oldEntityInDb = this._entityProRepository.GetEntityInfoByTableName(null, entityInfo.TableName, 1);
            if (oldEntityInDb != null)
            {
                throw (new Exception("实体【" + entityInfo.TableName + "】已经存在"));
            }
            if (entityInfo.SubEntitys != null && entityInfo.SubEntitys.Count > 0) {
                foreach (ExcelEntityInfo subEntityInfo in entityInfo.SubEntitys) {
                    if (subEntityInfo.TypeNameDict.Count > 1) {
                        if (subEntityInfo.TypeNameDict.Count != entityInfo.TypeNameDict.Count) {
                            throw (new Exception("嵌套实体的分类定义必须跟主实体一致"));
                        }
                    }
                    int typecount = subEntityInfo.TypeNameDict.Count;
                    for (int i = 0; i < typecount; i++)
                    {
                        EntityTypeSettingInfo main = entityInfo.TypeNameDict[i.ToString()];
                        EntityTypeSettingInfo sub = subEntityInfo.TypeNameDict[i.ToString()];
                        if (main.TypeName.Equals(sub.TypeName) == false) {
                            throw (new Exception("嵌套实体的分类定义必须跟主实体一致"));
                        }
                    }
                    oldEntityInDb = this._entityProRepository.GetEntityInfoByEntityName(null, subEntityInfo.EntityName, 1);
                    if (oldEntityInDb != null)
                    {
                        throw (new Exception("实体【" + subEntityInfo.EntityName + "】已经存在"));
                    }
                    oldEntityInDb = this._entityProRepository.GetEntityInfoByTableName(null, subEntityInfo.TableName, 1);
                    if (oldEntityInDb != null)
                    {
                        throw (new Exception("实体【" + subEntityInfo.TableName + "】已经存在"));
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
            bool isInRevert = DefaultFieldName.Contains(fieldInfo.FieldName);
            if (isInRevert) {
                if (fieldInfo.FieldTypeName.Equals("格式") == false) {
                    throw (new Exception("不能使用系统预知字段"));
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
            else if (fieldInfo.FieldTypeName.Equals("字典单选"))
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
                            throw (new Exception("字典值没有定义"));
                        }
                        fieldInfo.FieldConfig = MakeDictFieldConfig(dictType, defaultid);
                    }
                    else
                    {
                        fieldInfo.FieldConfig = MakeDictFieldConfig(dictType, -1);
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
            else if (fieldInfo.FieldTypeName.Equals("小数"))
            {
                fieldInfo.ControlType = 7; fieldInfo.FieldType = 2;
                if (fieldInfo.DefaultValue.Length > 0)
                {
                    decimal tmp = 0;
                    if (decimal.TryParse(fieldInfo.DefaultValue, out tmp) == false)
                    {
                        throw (new Exception("默认值转换失败"));
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
                        throw (new Exception("默认值转换失败"));
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
            else if (fieldInfo.FieldTypeName.Equals("单选数据源"))
            {

                fieldInfo.ControlType = 18; fieldInfo.FieldType = 2;
                if (fieldInfo.DataSourceName == null || fieldInfo.DataSourceName.Length == 0)
                    throw (new Exception("数据源配置错误"));
                Guid datasourceid = GetDataSourceIdFromName(fieldInfo.DataSourceName);
                if (datasourceid != Guid.Empty) {
                    fieldInfo.FieldConfig = MakeDataSourceFieldConfig(datasourceid.ToString(), false);
                }
            }
            else if (fieldInfo.FieldTypeName.Equals("多选数据源"))
            {
                fieldInfo.ControlType = 18; fieldInfo.FieldType = 2;
                if (fieldInfo.DataSourceName == null || fieldInfo.DataSourceName.Length == 0)
                    throw (new Exception("数据源配置错误"));
                Guid datasourceid = GetDataSourceIdFromName(fieldInfo.DataSourceName);
                if (datasourceid != Guid.Empty)
                {
                    fieldInfo.FieldConfig = MakeDataSourceFieldConfig(datasourceid.ToString(), true);
                }
            }
            else if (fieldInfo.FieldTypeName.Equals("引用对象"))
            {

                fieldInfo.ControlType = 31; fieldInfo.FieldType = 2;
                if (fieldInfo.DataSourceName == null) throw (new Exception("引用对象异常"));
                string[] tmp = fieldInfo.DataSourceName.Split(',');
                if (tmp.Length != 2) throw (new Exception("引用对象异常2"));
                fieldInfo.RelField_ControlFieldName = tmp[0];
                fieldInfo.RelField_originFieldName = tmp[1];
                //检查是否存在
                bool isExists = false;
                foreach (ExcelEntityColumnInfo fi in entityInfo.Fields) {
                    if (fi.DisplayName.Equals(fieldInfo.RelField_ControlFieldName)) {
                        if (fi.ControlType != 18) {
                            throw (new Exception("引用控件的控制控件只能是数据源控件"));
                        }
                        isExists = true;
                        break;
                    }
                }
            }
            else if (fieldInfo.FieldTypeName.Equals("表格")) {
                fieldInfo.ControlType = 23; fieldInfo.FieldType = 2;
                if (fieldInfo.DataSourceName == null) throw (new Exception("表格用对象异常"));
                string[] tmp = fieldInfo.DataSourceName.Split(',');
                if (tmp.Length != 2) throw (new Exception("表格对象异常2"));
                fieldInfo.Table_EntityName = tmp[0];
                fieldInfo.Table_TitleFieldName = tmp[1];
                ExcelEntityInfo tableEntityInfo = null;
                if (entityInfo.SubEntitys == null) throw (new Exception("表格对应的嵌套实体不存在"));
                foreach (ExcelEntityInfo subEntityInfo in entityInfo.SubEntitys) {
                    if (subEntityInfo.EntityName.Equals(fieldInfo.Table_EntityName)) {
                        tableEntityInfo = subEntityInfo;
                        break;
                    }
                }
                if (tableEntityInfo == null ) throw (new Exception("表格对应的嵌套实体不存在"));
                bool isFoundField = false;
                foreach (ExcelEntityColumnInfo subFieldInfo in tableEntityInfo.Fields)
                {
                    if (subFieldInfo.DisplayName.Equals(fieldInfo.Table_TitleFieldName))
                    {
                        isFoundField = true;
                        break;
                    }
                }
                if (!isFoundField) throw (new Exception("表格对应的嵌套实体不存在"));
            }
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
                    throw (new Exception("存在重复的实体名称:" + entityInfo.EntityName + ",请检查"));
                }
                if (dictTables.ContainsKey(entityInfo.TableName)) {
                    throw (new Exception("存在重复的实体表名:" + entityInfo.TableName + ",请检查"));
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
            else if (mainEntityInfo.ErrorMsg != null && mainEntityInfo.ErrorMsg.Length > 0) {
                throw (new Exception(mainEntityInfo.ErrorMsg));
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
            if (rowEnumerator.MoveNext() == false) {
                return mainEntityInfo;
            }
            //继续往下查找嵌套表格
            while (true) {

                #region 找到开始标志位
                EntityDefinedRow = null;
                while (rowEnumerator.MoveNext())
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
                else if (subEntityInfo.ErrorMsg != null && subEntityInfo.ErrorMsg.Length > 0)
                {
                    throw (new Exception(subEntityInfo.ErrorMsg));
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
                entityInfo.ErrorMsg = "实体定义格式异常";
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
                entityInfo.ErrorMsg = "实体类型(D列)的值不正确，必须是独立、简单、嵌套、动态这四个值之一";
                return entityInfo;
            }
            if (entityInfo.EntityName == null || entityInfo.EntityName.Length == 0) {

                entityInfo.ErrorMsg = "实体名称(B列)不能为空";
                return entityInfo;
            }
            if (entityInfo.TableName == null || entityInfo.TableName.Length == 0)
            {

                entityInfo.ErrorMsg = "数据库表名称(C列)不能为空";
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
                    entityInfo.ErrorMsg = "动态实体必须定义关联实体(E列)";
                    return entityInfo;
                }
                cell = row.Descendants<Cell>().ElementAt<Cell>(4);
                entityInfo.RelEntityName = GetCellValue(cell, workbookPart);
                if (CellCount < 6)
                {
                    entityInfo.ErrorMsg = "动态实体必须定义关联实体显示字段(F列）";
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
                columnInfo.ErrorMsg = string.Format("{0}\r\n 第{1}行第{2}列应该是数字，实际为{3}", columnInfo.ErrorMsg, row.RowIndex + 1, columnIndex + 1, cellValue);
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

            cell = row.Descendants<Cell>().ElementAt(columnIndex);
            cellValue = GetCellValue(cell, workbookPart);
            if (cellValue == null) cellValue = "";
            if (cellValue.Equals("√")) columnInfo.IsWebListField = true; else columnInfo.IsWebListField = false;
            columnIndex++;

            cell = row.Descendants<Cell>().ElementAt(columnIndex);
            cellValue = GetCellValue(cell, workbookPart);
            if (cellValue == null) cellValue = "";
            columnInfo.DefaultValue = cellValue;
            columnIndex++;

            cell = row.Descendants<Cell>().ElementAt(columnIndex);
            cellValue = GetCellValue(cell, workbookPart);
            if (cellValue == null) cellValue = "";
            if (cellValue.Equals("√")) columnInfo.IsAdvanceSearch = true; else columnInfo.IsAdvanceSearch = false;
            columnIndex++;

            cell = row.Descendants<Cell>().ElementAt(columnIndex);
            cellValue = GetCellValue(cell, workbookPart);
            if (cellValue == null) cellValue = "";
            if (cellValue.Equals("√")) columnInfo.IsCheckSameField = true; else columnInfo.IsCheckSameField = false;
            columnIndex++;

            cell = row.Descendants<Cell>().ElementAt(columnIndex);
            cellValue = GetCellValue(cell, workbookPart);
            if (cellValue == null) cellValue = "";
            columnInfo.Remark = cellValue;
            columnIndex++;
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
            List<string> dicStyles = GetNumberFormatsStyle(workBookPart);
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
                        string cellStyle = dicStyles[styleIndex - 1];//获取该索引的样式
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
                            cellStyle = cellStyle.Substring(cellStyle.LastIndexOf('.') - 1).Replace("\\", "");
                            decimal decimalNum = decimal.Parse(cellInnerText);
                            cellValue = decimal.Parse(decimalNum.ToString(cellStyle)).ToString();
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
        private List<string> GetNumberFormatsStyle(WorkbookPart workBookPart)
        {
            List<string> dicStyle = new List<string>();
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
                                dicStyle.Add(formatCode);//将格式化Code写入List集合
                            }
                        }
                    }
                }
            }
            return dicStyle;
        }

        #endregion
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

        public string ErrorMsg { get; set; }

        public List<ExcelEntityColumnInfo> Fields { get; set; }
        public List<ExcelEntityInfo> SubEntitys { get; set; }
        public Dictionary<string, EntityTypeSettingInfo> TypeNameDict { get; set; }
        public ExcelEntityInfo() {
            Fields = new List<ExcelEntityColumnInfo>();
            SubEntitys = new List<ExcelEntityInfo>();
            TypeNameDict = new Dictionary<string, EntityTypeSettingInfo>();
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
        public string Table_TitleFieldName { get; set; }
        public string Table_TitleFieldId { get; set; }
        public string ErrorMsg { get; set; }
        public Dictionary<string, ExcelEntityFieldViewInfo> ViewSet { get; set; }
        public ExcelEntityColumnInfo() {
            ViewSet = new Dictionary<string, ExcelEntityFieldViewInfo>();
            FieldConfig = new Dictionary<string, object>();
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
        public string TypeName { get; set; }
        public bool IsImport { get; set; }
    }
    #endregion
}
