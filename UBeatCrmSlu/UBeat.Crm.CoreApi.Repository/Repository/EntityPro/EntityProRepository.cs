using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.EntityPro;
using UBeat.Crm.CoreApi.DomainModel.Utility;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Utility;
using System.Linq;
using System.Data.Common;
using Npgsql;
using System.Text;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UBeat.Crm.CoreApi.DomainModel.Vocation;
using NpgsqlTypes;

namespace UBeat.Crm.CoreApi.Repository.Repository.EntityPro
{
    public class EntityProRepository : RepositoryBase, IEntityProRepository
    {
        IDBHelper myDBHelper = new PostgreHelper();

        #region 协议
        public Dictionary<string, List<IDictionary<string, object>>> EntityProQuery(EntityProQueryMapper crmEntityQuery, int userNumber)
        {
            var procName =
                "SELECT crm_func_entity_list(@entityname, @typeid,@status, @pageindex, @pagesize, @userno)";

            var dataNames = new List<string> { "PageData", "PageCount" };
            var param = new
            {
                EntityName = crmEntityQuery.EntityName,
                TypeId = crmEntityQuery.TypeId,
                Status = crmEntityQuery.Status,
                PageIndex = crmEntityQuery.PageIndex,
                PageSize = crmEntityQuery.PageSize,
                UserNo = userNumber
            };

            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }
        public Dictionary<string, List<IDictionary<string, object>>> EntityProInfoQuery(EntityProInfoMapper entity, int userNumber)
        {
            var procName =
                "SELECT crm_func_entity_info(@entityid, @userno)";

            var dataNames = new List<string> { "EntityProInfo" };
            var param = new
            {
                EntityId = entity.EntityId,
                UserNo = userNumber
            };

            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }
        public OperateResult InsertEntityPro(EntityProSaveMapper entity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_entity_add(@entityname,@entitytable, @typeid, @remark, @styles, @icons,@relentityid,@relfieldid,@relaudit, @recstatus,@userno,@entitylanguage::jsonb)
            ";
            var param = new DynamicParameters();
            param.Add("entityname", entity.EntityName);
            param.Add("entitytable", entity.EntityTable);
            param.Add("typeid", entity.TypeId);
            param.Add("remark", entity.Remark);
            param.Add("styles", entity.Styles);
            param.Add("icons", entity.Icons);
            param.Add("relentityid", entity.RelEntityId);
            param.Add("relfieldid", entity.RelFieldId);
            param.Add("relaudit", entity.Relaudit);
            param.Add("recstatus", 1);
            param.Add("userno", userNumber);
            param.Add("entitylanguage", JsonConvert.SerializeObject(entity.EntityName_Lang));
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult SaveEntityGlobalJs(EntityGlobalJsMapper entity, int userNumber)
        {
            var sql = @"
                Update crm_sys_entity Set newload=@newload,editload=@editload,checkload=@checkload Where entityid=@entityid;
            ";
            string field = string.Empty;
            var param = new DbParameter[]
            {
                    new NpgsqlParameter("newload",entity.NewLoad),
                    new NpgsqlParameter("editload",entity.EditLoad),
                    new NpgsqlParameter("checkload", entity.CheckLoad),
                    new NpgsqlParameter("entityid",entity.EntityId)
            };
            var result = ExecuteNonQuery(sql, param);
            if (result == 1)
            {
                return new OperateResult()
                {
                    Flag = 1,
                    Msg = "保存全局Js成功"
                };
            }
            else
            {
                return new OperateResult()
                {
                    Msg = "保存全局Js失败"
                };
            }
        }

        public OperateResult UpdateEntityPro(EntityProSaveMapper entity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_entity_edit(@entityid,@entityname, @typeid,@icons, @remark,@relentityid,@relfieldid,@relaudit,@userno,@entitylanguage::jsonb)
            ";
            var param = new DynamicParameters();
            param.Add("entityid", entity.EntityId);
            param.Add("entityname", entity.EntityName);
            param.Add("typeid", entity.TypeId);
            param.Add("icons", entity.Icons);
            param.Add("remark", entity.Remark);
            param.Add("relentityid", entity.RelEntityId);
            param.Add("relfieldid", entity.RelFieldId);
            param.Add("relaudit", entity.Relaudit);
            param.Add("userno", userNumber);
            param.Add("entitylanguage", JsonConvert.SerializeObject(entity.EntityName_Lang));
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult DisabledEntityPro(EntityProMapper entity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_entity_disabled(@entityid,@status,@userno)
            ";
            var param = new DynamicParameters();
            param.Add("entityid", entity.EntityId);
            param.Add("status", entity.RecStatus);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult DeleteEntityData(DeleteEntityDataMapper entity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_entity_data_delete(@entityid,@userno)
            ";
            var param = new DynamicParameters();
            param.Add("entityid", entity.EntityId);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public Dictionary<string, List<IDictionary<string, object>>> EntityClassQuery(EntityProMapper entity, int userNumber)
        {
            var procName =
                "SELECT crm_func_entity_classic_list(@typeid,@status, @userno)";

            var dataNames = new List<string> { "EntityClass" };
            var param = new DynamicParameters();
            param.Add("typeid", entity.TypeId);
            param.Add("status", entity.RecStatus);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }
        public Dictionary<string, List<IDictionary<string, object>>> EntityOrderbyQuery(EntityOrderbyMapper entity, int userNumber)
        {
            var procName =
                "SELECT crm_func_entity_modeltype_orderby(@modeltype,@relentityid, @userno)";

            var dataNames = new List<string> { "EntityOrderby" };
            var param = new DynamicParameters();
            param.Add("modeltype", entity.ModelType);
            param.Add("relentityid", entity.RelEntityId);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }
        public OperateResult OrderByEntityPro(OrderByEntityProMapper entity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_entity_ordery(@entityids,@userno)
            ";
            var param = new DynamicParameters();
            param.Add("entityids", entity.EntityIds);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);

            return result;
        }
        #endregion

        #region 字段
        public List<EntityFieldProMapper> FieldQuery(string entityId, int userNumber)
        {
            var procName =
                "SELECT crm_func_entity_field_list(@entityid,@status,@userno)";

            var dataNames = new List<string> { "EntityFieldPros" };
            var param = new DynamicParameters();
            param.Add("status", 2);
            param.Add("entityId", entityId);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QueryStoredProcCursor<EntityFieldProMapper>(procName, param, CommandType.Text);
            return result;
        }

        public Dictionary<string, List<IDictionary<string, object>>> EntityFieldProQuery(string entityId, int userNumber)
        {
            var procName =
                "SELECT crm_func_entity_field_list(@entityid,@status,@userno)";

            var dataNames = new List<string> { "EntityFieldPros" };
            var param = new DynamicParameters();
            param.Add("status", 2);
            param.Add("entityId", entityId);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }

        public OperateResult InsertEntityField(EntityFieldProSaveMapper entity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_entity_field_add(@entityid,@fieldlable, @displayname, @fieldname, @fieldconfig, @fieldtype, @status,@controltype, @userno,@fieldlanguage::jsonb,@displaylanguage::jsonb)
            ";
            var param = new DynamicParameters();
            param.Add("entityid", entity.EntityId);
            param.Add("fieldlable", entity.FieldLabel);
            param.Add("displayname", entity.DisplayName);
            param.Add("fieldname", entity.FieldName);
            param.Add("fieldconfig", entity.FieldConfig);
            param.Add("fieldtype", entity.FieldType);
            param.Add("controltype", entity.ControlType);
            param.Add("status", entity.RecStatus);
            param.Add("userno", userNumber);
            param.Add("fieldlanguage", JsonConvert.SerializeObject(entity.FieldLabel_Lang));
            param.Add("displaylanguage", JsonConvert.SerializeObject(entity.DisplayName_Lang));
           
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult UpdateEntityField(EntityFieldProSaveMapper entity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_entity_field_edit(@entityid,@fieldid,@fieldlabel, @displayname, @fieldconfig, @fieldtype, @status,@controltype, @userno,@fieldlanguage::jsonb,@displaylanguage::jsonb)
            ";
            var param = new DynamicParameters();
            param.Add("entityid", entity.EntityId);
            param.Add("fieldid", entity.FieldId);
            param.Add("fieldlabel", entity.FieldLabel);
            param.Add("displayname", entity.DisplayName);
            param.Add("status", entity.RecStatus);
            param.Add("fieldconfig", entity.FieldConfig);
            param.Add("fieldtype", entity.FieldType);
            param.Add("controltype", entity.ControlType);
            param.Add("userno", userNumber);
            param.Add("displaylanguage", JsonConvert.SerializeObject(entity.DisplayName_Lang));
            param.Add("fieldlanguage", JsonConvert.SerializeObject(entity.FieldLabel_Lang));
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult UpdateEntityFieldExpandJS(EntityFieldExpandJSDataMapper entity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_entity_field_expandjs_edit(@fieldid, @expandjs, @userno)
            ";
            var param = new DynamicParameters();
            param.Add("fieldid", entity.FieldId);
            param.Add("expandjs", entity.ExpandJS);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult UpdateEntityFieldFilterJS(EntityFieldFilterJSDataMapper entity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_entity_field_filterjs_edit(@fieldid, @filterjs, @userno)
            ";
            var param = new DynamicParameters();
            param.Add("fieldid", entity.FieldId);
            param.Add("filterjs", entity.FilterJS);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult DisabledEntityFieldPro(EntityFieldProMapper entity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_entity_field_disabled(@fieldid,@status, @userno)
            ";
            var param = new DynamicParameters();
            param.Add("status", entity.RecStatus);
            param.Add("fieldid", entity.FieldId);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }
        public OperateResult DeleteEntityFieldPro(EntityFieldProMapper entity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_entity_field_delete(@fieldid, @userno)
            ";
            var param = new DynamicParameters();
            param.Add("fieldid", entity.FieldId);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult OrderByEntityFieldPro(ICollection<EntityFieldProMapper> entities, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_entity_field_orderby(@fieldid,@orderby,@userno)
            ";
            OperateResult result = new OperateResult();
            foreach (var entity in entities)
            {
                var param = new DynamicParameters();
                param.Add("fieldid", entity.FieldId);
                param.Add("orderby", entity.RecOrder);
                param.Add("userno", userNumber);
                result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
                if (result.Flag == 0)
                    return result;
            }
            return result;
        }

        #endregion

        #region 协议类型

        public Dictionary<string, List<IDictionary<string, object>>> EntityTypeQuery(EntityTypeQueryMapper entityType, int userNumber)
        {
            var procName =
                "SELECT crm_func_entity_type_list(@entityid,@status,@userno)";

            var dataNames = new List<string> { "EntityTypePros" };
            var param = new DynamicParameters();
            param.Add("entityid", entityType.EntityId);
            param.Add("status", 2);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }
        /***
         * 检查是否是嵌套实体，如果是嵌套实体，则检查嵌套实体的类型是否符合要求，如果不符合要求，则更新至符合要求
         * ***/
        public bool CheckAndNestEntityType(string entityid, int userNum)
        {
            string cmdText = "select relentityid from crm_sys_entity where entityid = @entityid and modeltype = 1 ";
            var param = new DbParameter[] {
                new NpgsqlParameter("entityid",Guid.Parse(entityid))
            };
            object ret = ExecuteScalar(cmdText, param);
            if (ret == null || ((Guid)ret) == Guid.Empty)
                return false;
            Guid relEntityID = (Guid)ret;
            cmdText = "Select * from  crm_sys_entity_category where entityid=@entityid";
            param = new DbParameter[] {
                new NpgsqlParameter("entityid",Guid.Parse(entityid))
            };
            List<Dictionary<string, object>> EntityTypeList = ExecuteQuery(cmdText, param);
            cmdText = "Select * from  crm_sys_entity_category where entityid=@entityid";
            param = new DbParameter[] {
                new NpgsqlParameter("entityid",relEntityID)
            };
            List<Dictionary<string, object>> RelEntityTypeList = ExecuteQuery(cmdText, param);
            Guid EntityGuid = Guid.Parse(entityid);
            bool isChanged = false;
            foreach (Dictionary<string, object> EntityItem in EntityTypeList)
            {
                Guid categoryid = (Guid)EntityItem["categoryid"];
                if (categoryid == EntityGuid)
                {
                    Dictionary<string, object> relItem = FindRelType(RelEntityTypeList, relEntityID);
                    if (relItem == null)
                    {
                        //如果没有找到
                        if (EntityItem["recstatus"] != null && (int)EntityItem["recstatus"] != 2)
                        {
                            EntityItem["recstatus"] = 2;
                            UpdateNestTypeInfo(EntityItem);
                            isChanged = true;
                        }

                    }
                    else
                    {
                        //如果找到,则判断是否有变化
                        if (!CheckObjectSame(EntityItem["relcategoryid"], relItem["categoryid"])
                            || !CheckObjectSame(EntityItem["categoryname"], relItem["categoryname"])
                            || !CheckObjectSame(EntityItem["recorder"], relItem["recorder"])
                            || !CheckObjectSame(EntityItem["recstatus"], relItem["recstatus"]))
                        {
                            EntityItem["relcategoryid"] = relItem["categoryid"];
                            EntityItem["categoryname"] = relItem["categoryname"];
                            EntityItem["recorder"] = relItem["recorder"];
                            EntityItem["recstatus"] = relItem["recstatus"];
                            UpdateNestTypeInfo(EntityItem);
                            isChanged = true;
                        }
                    }
                }
                else
                {
                    if (EntityItem["relcategoryid"] == System.DBNull.Value || (Guid)EntityItem["relcategoryid"] == null || (Guid)EntityItem["relcategoryid"] == Guid.Empty)
                    {
                        //原来没有关联relcategoryid的，直接删除即可(标记状态为2)
                        if (EntityItem["recstatus"] != null && (int)EntityItem["recstatus"] != 2)
                        {
                            EntityItem["recstatus"] = 2;
                            UpdateNestTypeInfo(EntityItem);
                            isChanged = true;
                        }

                    }
                    else
                    {
                        Dictionary<string, object> relItem = FindRelType(RelEntityTypeList, (Guid)EntityItem["relcategoryid"]);
                        if (relItem == null)
                        {
                            //没有找到，则重置relcategoryid == null && recstatus = 2;
                            if (EntityItem["recstatus"] != null && (int)EntityItem["recstatus"] != 2)
                            {
                                EntityItem["recstatus"] = 2;
                                UpdateNestTypeInfo(EntityItem);
                                isChanged = true;
                            }
                        }
                        else
                        {
                            if (!CheckObjectSame(EntityItem["relcategoryid"], relItem["categoryid"])
                           || !CheckObjectSame(EntityItem["categoryname"], relItem["categoryname"])
                           || !CheckObjectSame(EntityItem["recorder"], relItem["recorder"])
                           || !CheckObjectSame(EntityItem["recstatus"], relItem["recstatus"]))
                            {
                                EntityItem["relcategoryid"] = relItem["categoryid"];
                                EntityItem["categoryname"] = relItem["categoryname"];
                                EntityItem["recorder"] = relItem["recorder"];
                                EntityItem["recstatus"] = relItem["recstatus"];
                                UpdateNestTypeInfo(EntityItem);
                                isChanged = true;
                            }
                        }
                    }
                }
            }

            foreach (Dictionary<string, object> RelEntityItem in RelEntityTypeList)
            {
                if (!CheckFromRelType(EntityTypeList, (Guid)RelEntityItem["categoryid"]))
                {
                    if (CheckObjectSame(RelEntityItem["categoryid"], RelEntityItem["entityid"])) continue;
                    InsertNestTypeInfoFromRel(RelEntityItem, EntityGuid, userNum);
                    isChanged = true;
                }
            }
            return isChanged;
        }
        private bool CheckObjectSame(object obj1, object obj2)
        {
            if (obj1 == null && obj2 == null)
            {
                return true;
            }
            if (obj1 != null && obj2 != null) return obj1.ToString() == obj2.ToString();
            return false;

        }
        private Dictionary<string, object> FindRelType(List<Dictionary<string, object>> RelEntityTypeList, Guid categoryid)
        {
            foreach (Dictionary<string, object> EntityItem in RelEntityTypeList)
            {
                if ((Guid)EntityItem["categoryid"] == categoryid)
                {
                    return EntityItem;
                }
            }
            return null;
        }
        private bool CheckFromRelType(List<Dictionary<string, object>> EntityTypeList, Guid categoryid)
        {
            foreach (Dictionary<string, object> item in EntityTypeList)
            {
                if (item["relcategoryid"] != null && item["relcategoryid"] != DBNull.Value && (Guid)item["relcategoryid"] != Guid.Empty)
                {
                    if ((Guid)item["relcategoryid"] == categoryid)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void InsertNestTypeInfoFromRel(Dictionary<string, object> item, Guid entityid, int userNum)
        {
            string cmdText = "Insert into crm_sys_entity_category(categoryid,categoryname,entityid,recorder,recstatus,reccreator,recupdator,reccreated,recupdated,relcategoryid)" +
                " values(@categoryid,@categoryname,@entityid,@recorder,@recstatus,@reccreator,@recupdator,@reccreated,@recupdated,@relcategoryid)";
            System.DateTime now = DateTime.Now;
            var param = new DbParameter[] {
                new  NpgsqlParameter("categoryid",Guid.NewGuid()),
                new NpgsqlParameter("categoryname",item["categoryname"]),
                new NpgsqlParameter("entityid",entityid),
                new NpgsqlParameter("recorder",item["recorder"]),
                new NpgsqlParameter("recstatus",item["recstatus"]),
                new NpgsqlParameter("relcategoryid",item["categoryid"]),
                new NpgsqlParameter("reccreator",userNum),
                new NpgsqlParameter("recupdator",userNum),
                new NpgsqlParameter("reccreated",now),
                new NpgsqlParameter("recupdated",now)
            };
            ExecuteNonQuery(cmdText, param);
        }
        private void UpdateNestTypeInfo(Dictionary<string, object> item)
        {
            string cmdText = "update  crm_sys_entity_category set categoryname=@categoryname ,recorder=@recorder,recstatus=@recstatus,relcategoryid=@relcategoryid where categoryid =@categoryid";
            var param = new DbParameter[] {
                new NpgsqlParameter("categoryname",item["categoryname"]),
                new NpgsqlParameter("recorder",item["recorder"]),
                new NpgsqlParameter("recstatus",item["recstatus"]),
                new NpgsqlParameter("relcategoryid",item["relcategoryid"]),
                new NpgsqlParameter("categoryid",item["categoryid"]),
            };
            ExecuteNonQuery(cmdText, param);
        }
        public OperateResult InsertEntityTypePro(SaveEntityTypeMapper entityType, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_entity_type_add(@categoryname,@entityid, @userno,@categoryname_lang::jsonb)
            ";
            var param = new DynamicParameters();
            param.Add("categoryname", entityType.CategoryName);
            param.Add("entityid", entityType.EntityId.ToString());
            param.Add("userno", userNumber);
            param.Add("categoryname_lang", JsonConvert.SerializeObject(entityType.CategoryName_Lang));
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult UpdateEntityTypePro(SaveEntityTypeMapper entityType, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_entity_type_edit(@entityid,@categoryid,@categoryname, @userno,@categoryname_lang::jsonb)
            ";
            var param = new DynamicParameters();
            param.Add("entityid", entityType.EntityId);
            param.Add("categoryid", entityType.CategoryId);
            param.Add("categoryname", entityType.CategoryName);
            param.Add("userno", userNumber);
            param.Add("categoryname_lang", JsonConvert.SerializeObject(entityType.CategoryName_Lang));
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult DisabledEntityTypePro(EntityTypeMapper entityType, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_entity_type_disabled(@categoryid,@status, @userno)
            ";
            var param = new DynamicParameters();
            param.Add("categoryid", entityType.CategoryId);
            param.Add("status", entityType.RecStatus);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult OrderByEntityType(ICollection<EntityTypeMapper> entities, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_entity_type_orderby(@categoryid,@order,@userno)
            ";
            OperateResult result = new OperateResult();
            foreach (var entity in entities)
            {
                var param = new DynamicParameters();
                param.Add("categoryid", entity.CategoryId);
                param.Add("order", entity.RecOrder);
                param.Add("userno", userNumber);
                result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
                if (result.Flag == 0)
                    return result;
            }
            return result;
        }

        #endregion

        #region 控件规则
        public List<EntityFieldRulesMapper> EntityFieldRulesQuery(string entityId, string typeId, int userNumber)
        {
            var procName =
                "SELECT crm_func_field_rule_list(@entityid,@typeid,@userno)";
            var param = new DynamicParameters();
            param.Add("entityid", entityId);
            param.Add("typeid", typeId);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QueryStoredProcCursor<EntityFieldRulesMapper>(procName, param, CommandType.Text);
            return result;
        }

        public OperateResult SaveEntityFieldRules(ICollection<EntityFieldRulesSaveMapper> rules, int userNumber)
        {
            OperateResult result = new OperateResult();
            foreach (var rule in rules)
            {
                if (rule.RecStatus == 1)
                {
                    foreach (var tmp in rule.Rules)
                    {
                        if (String.IsNullOrWhiteSpace(tmp.FieldRulesId))
                        {
                            //if (tmp.IsVisible == 0) continue; 此处不能跳过，必须插入，不然新增实体数据时，有些默认字段会被过滤
                            var sql = @"
                SELECT * FROM crm_func_fieldrules_add(@typeid,@fieldid,@operatetype,@isrequire,@isvisible,@isreadonly, @viewrules, @validrules, @userno)
            ";
                            var param = new DynamicParameters();
                            param.Add("typeid", rule.TypeId);
                            param.Add("fieldid", rule.FieldId);
                            param.Add("operatetype", tmp.OperateType);
                            param.Add("isvisible", tmp.IsVisible);
                            param.Add("isrequire", tmp.IsRequired);
                            param.Add("isreadonly", tmp.IsReadOnly);
                            param.Add("viewrules", tmp.ViewRuleStr);
                            param.Add("validrules", tmp.ValidRuleStr);
                            param.Add("userno", tmp.UserId);
                            result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
                            if (result.Flag == 0) return result;
                        }
                        else
                        {
                            var sql = @"
                SELECT * FROM crm_func_fieldrules_edit(@fieldrulesid,@isrequire,@isvisible,@isreadonly, @viewrules, @validrules, @userno)
            ";
                            var param = new DynamicParameters();
                            param.Add("fieldrulesid", tmp.FieldRulesId);
                            param.Add("isvisible", tmp.IsVisible);
                            param.Add("isrequire", tmp.IsRequired);
                            param.Add("isreadonly", tmp.IsReadOnly);
                            param.Add("viewrules", tmp.ViewRuleStr);
                            param.Add("validrules", tmp.ValidRuleStr);
                            param.Add("userno", tmp.UserId);
                            result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
                            if (result.Flag == 0) return result;
                        }
                    }
                }
                else
                {
                    string sql = "";
                    DynamicParameters param = null;
                    foreach (var tmp in rule.Rules)
                    {
                        if (String.IsNullOrWhiteSpace(tmp.FieldRulesId))
                        {
                            //if (tmp.IsVisible == 0) continue; 此处不能跳过，必须插入，不然新增实体数据时，有些默认字段会被过滤
                              sql = @"
                SELECT * FROM crm_func_fieldrules_add(@typeid,@fieldid,@operatetype,@isrequire,@isvisible,@isreadonly, @viewrules, @validrules, @userno)
            ";
                              param = new DynamicParameters();
                            param.Add("typeid", rule.TypeId);
                            param.Add("fieldid", rule.FieldId);
                            param.Add("operatetype", tmp.OperateType);
                            param.Add("isvisible", tmp.IsVisible);
                            param.Add("isrequire", tmp.IsRequired);
                            param.Add("isreadonly", tmp.IsReadOnly);
                            param.Add("viewrules", tmp.ViewRuleStr);
                            param.Add("validrules", tmp.ValidRuleStr);
                            param.Add("userno", tmp.UserId);
                            result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
                            if (result.Flag == 0) return result;
                        }
                        else
                        {
                              sql = @"
                SELECT * FROM crm_func_fieldrules_edit(@fieldrulesid,@isrequire,@isvisible,@isreadonly, @viewrules, @validrules, @userno)
            ";
                              param = new DynamicParameters();
                            param.Add("fieldrulesid", tmp.FieldRulesId);
                            param.Add("isvisible", tmp.IsVisible);
                            param.Add("isrequire", tmp.IsRequired);
                            param.Add("isreadonly", tmp.IsReadOnly);
                            param.Add("viewrules", tmp.ViewRuleStr);
                            param.Add("validrules", tmp.ValidRuleStr);
                            param.Add("userno", tmp.UserId);
                            result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
                            if (result.Flag == 0) return result;
                        }
                    }
                    sql = @"
                SELECT * FROM crm_func_fieldrules_disabled(@fieldid,@typeid, @status, @userno)
            ";
                      param = new DynamicParameters();
                    param.Add("@fieldid", rule.FieldId);
                    param.Add("@typeid", rule.TypeId);
                    param.Add("@status", rule.RecStatus);
                    param.Add("@userno", userNumber);
                    result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
                    if (result.Flag == 0) return result;
                }
            }
            return result;
        }

        public List<EntityFieldRulesVocationMapper> EntityFieldRulesVocationQuery(string entityId, string vocationId, int userNumber)
        {
            var procName =
                "SELECT crm_func_field_rule_vocation_list(@entityid,@vocationId,@userno)";

            var param = new DynamicParameters();
            param.Add("entityid", entityId);
            param.Add("vocationId", vocationId);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QueryStoredProcCursor<EntityFieldRulesVocationMapper>(procName, param, CommandType.Text);
            return result;
        }

        public OperateResult SaveEntityFieldRulesVocation(string entityid, ICollection<EntityFieldRulesVocationSaveMapper> rules, int userNumber)
        {
            OperateResult result = new OperateResult();
            var conn = DataBaseHelper.GetDbConnect();
            conn.Open();
            string sql = string.Empty;
            DynamicParameters param = null;
            var trans = conn.BeginTransaction();
            try
            {
                foreach (var rule in rules)
                {
                    foreach (var tmp in rule.Rules)
                    {
                        if (String.IsNullOrWhiteSpace(tmp.FieldRulesId))
                        {
                            sql = @"
                SELECT * FROM crm_func_fieldrules_vocation_add(@entityid,@vocationid,@fieldid,@operatetype,@isvisible,@isreadonly,@viewrules, @userno)
            ";
                            param = new DynamicParameters();
                            param.Add("entityid", entityid);
                            param.Add("vocationid", rule.VocationId);
                            param.Add("fieldid", rule.FieldId);
                            param.Add("operatetype", tmp.OperateType);
                            param.Add("isvisible", tmp.IsVisible);
                            param.Add("isreadonly", tmp.IsReadOnly);
                            param.Add("viewrules", tmp.ViewRules());
                            param.Add("userno", userNumber);
                            result = DataBaseHelper.QuerySingle<OperateResult>(conn, sql, param);
                            if (result.Flag == 0) return result;
                        }
                        else
                        {
                            sql = @"
                SELECT * FROM crm_func_fieldrules_vocation_edit(@fieldrulesid,@isvisible, @isreadonly,@viewrules,  @userno)
            ";
                            param = new DynamicParameters();
                            param.Add("fieldrulesid", tmp.FieldRulesId);
                            param.Add("isvisible", tmp.IsVisible);
                            param.Add("isreadonly", tmp.IsReadOnly);
                            param.Add("viewrules", tmp.ViewRules());
                            param.Add("userno", userNumber);
                            result = DataBaseHelper.QuerySingle<OperateResult>(conn, sql, param);
                            if (result.Flag == 0) return result;
                        }
                    }
                    sql = @"
                SELECT * FROM crm_func_fieldrules_vocation_disabled(@vocationid,@fieldid, @status, @userno)
            ";
                    param = new DynamicParameters();
                    param.Add("@vocationid", rule.VocationId);
                    param.Add("@fieldid", rule.FieldId);
                    param.Add("@status", rule.RecStatus);
                    param.Add("@userno", userNumber);
                    result = DataBaseHelper.QuerySingle<OperateResult>(conn, sql, param);
                    if (result.Flag == 0) return result;
                }
                trans.Commit();
                return result;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                throw ex;
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }
        #endregion

        #region 列表搜索
        public Dictionary<string, List<IDictionary<string, object>>> EntityFieldFilterQuery(string entityid, int userNumber)
        {

            var procName =
                "SELECT crm_func_field_filter_list(@entityid,@userno)";

            var dataNames = new List<string> { "Fields", "FieldsSearch" };
            var param = new DynamicParameters();
            param.Add("entityid", entityid);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }

        public OperateResult UpdateEntityFieldFilter(string searchJson, string simpleSearchJson, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_fieldsearch_save(@searchjson,@simplesearchjson, @userno)
            ";
            var param = new DynamicParameters();
            param.Add("searchjson", searchJson);
            param.Add("simplesearchjson", simpleSearchJson);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }
        #endregion

        #region 设置MOB WEB端字段显示
        public Dictionary<string, List<IDictionary<string, object>>> FieldWebVisibleQuery(string entityId, int userNumber)
        {

            var procName =
                "SELECT crm_func_field_visible_web_list(@entityid,@viewtype,@userno)";

            var dataNames = new List<string> { "FieldNotVisible", "FieldVisible" };
            var param = new DynamicParameters();
            param.Add("entityid", entityId);
            param.Add("viewtype", 0);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }

        public OperateResult SaveWebFieldVisible(SaveListViewColumnMapper listView, int userNumber)
        {

            var sql = @"
                SELECT * FROM crm_func_field_visible_web_save(@entityid,@viewtype,@fieldids, @userno)
            ";
            var param = new DynamicParameters();
            param.Add("entityid", listView.EntityId);
            param.Add("viewtype", listView.ViewType);
            param.Add("fieldids", listView.FieldIds);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public Dictionary<string, List<IDictionary<string, object>>> FieldMOBVisibleQuery(string entityId, int userNumber)
        {

            var procName =
                "SELECT crm_func_field_visible_mob_list(@entityid,@viewtype,@userno)";

            var dataNames = new List<string> { "FieldVisible", "FieldMOBStyleConfig" };
            var param = new DynamicParameters();
            param.Add("entityid", entityId);
            param.Add("viewtype", 1);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }

        public OperateResult InsertMOBFieldVisible(ListViewMapper view, int userNumber)
        {

            var sql = @"
                SELECT * FROM crm_func_field_visible_mob_add(@entityid,@viewtype,@fieldids,@viewstyleid,@fieldkeys,@fonts,@colors, @userno)
            ";
            var param = new DynamicParameters();
            param.Add("entityid", view.EntityId);
            param.Add("viewtype", view.ViewType);
            param.Add("fieldids", view.FieldIds);
            param.Add("viewstyleid", view.ViewStyleId);
            param.Add("fieldkeys", view.FieldKeys);
            param.Add("fonts", view.Fonts);
            param.Add("colors", view.Colors);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult UpdateMOBFieldVisible(ListViewMapper view, int userNumber)
        {

            var sql = @"
                SELECT * FROM crm_func_field_visible_mob_edit(@viewconfid,@entityid,@viewtype,@fieldids,@viewstyleid,@fieldkeys,@fonts,@colors, @userno)
            ";
            var param = new DynamicParameters();
            param.Add("viewconfid", view.ViewConfId);
            param.Add("entityid", view.EntityId);
            param.Add("viewtype", view.ViewType);
            param.Add("fieldids", view.FieldIds);
            param.Add("viewstyleid", view.ViewStyleId);
            param.Add("fieldkeys", view.FieldKeys);
            param.Add("fonts", view.Fonts);
            param.Add("colors", view.Colors);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }
        #endregion

        #region 设置顶部显示字段

        public Dictionary<string, List<IDictionary<string, object>>> EntityPageConfigInfoQuery(EntityPageConfigMapper pageConfig, int userNumber)
        {
            var procName =
                "SELECT crm_func_page_config_info(@entityid,@userno)";

            var dataNames = new List<string> { "NoticeHistoryList" };
            var param = new DynamicParameters();
            param.Add("entityid", pageConfig.EntityId);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }
        public OperateResult SaveEntityPageConfig(EntityPageConfigMapper pageConfig, int userNumber)
        {

            var sql = @"
                SELECT * FROM crm_func_page_config_save(@entityid,@titlefieldid,@titlefieldname,@subfieldids,@subfieldnames,@modules,@relentityid,@relfieldid,@relfieldname, @userno)
            ";
            var param = new DynamicParameters();
            param.Add("entityid", pageConfig.EntityId);
            param.Add("titlefieldid", pageConfig.TitlefieldId);
            param.Add("titlefieldname", pageConfig.TitlefieldName);
            param.Add("subfieldids", pageConfig.SubfieldIds);
            param.Add("subfieldnames", pageConfig.SubfieldNames);
            param.Add("modules", pageConfig.Modules);
            param.Add("relentityid", pageConfig.RelentityId);
            param.Add("relfieldid", pageConfig.RelfieldId);
            param.Add("relfieldname", pageConfig.RelfieldName);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }
        #endregion



        public dynamic GetEntityInfo(Guid typeId, int userNumber)
        {
            //插入操作
            //生成插入语句
            var sql = @"
                select entityid,entityname,entitytable,modeltype,relentityid,relaudit,servicesjson from crm_sys_entity where entityid=(select entityid from crm_sys_entity_category where categoryid=@typeid limit 1);
            ";

            var param = new
            {
                TypeId = typeId,
                UserNo = userNumber
            };
            var result = DataBaseHelper.QuerySingle<dynamic>(sql, param);
            return result;
        }

        public SimpleEntityInfo GetEntityInfo(Guid typeId)
        {
            //插入操作
            //生成插入语句
            var sql = @"SELECT e.relentityid,e.relfieldid,e.relfieldname,e.entitytable,c.categoryid ,c.categoryname, e.entityid,e.entityname,e.modeltype,e.relentityid,re.entityname AS relentityname,e.relaudit,e.servicesjson 
                        FROM crm_sys_entity_category AS c
                        INNER JOIN crm_sys_entity AS e ON e.entityid=c.entityid
                        LEFT JOIN crm_sys_entity AS re ON re.entityid=e.relentityid
                        WHERE c.categoryid=@categoryid LIMIT 1;
            ";

            var sqlParameters = new List<DbParameter>();
            sqlParameters.Add(new NpgsqlParameter("categoryid", typeId));

            return ExecuteQuery<SimpleEntityInfo>(sql, sqlParameters.ToArray()).FirstOrDefault();
        }

        public dynamic GetFieldInfo(Guid fieldId, int userNumber)
        {
            //插入操作
            //生成插入语句
            var sql = @"
                select * from crm_sys_entity_fields where  fieldid=@fieldid and recstatus=1 ;
            ";

            var param = new
            {
                FieldId = fieldId,
                UserNo = userNumber
            };
            var result = DataBaseHelper.QuerySingle<dynamic>(sql, param);
            return result;
        }


        /// <summary>
        /// 获取实体功能配置数据
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public FunctionJsonInfo GetFunctionJsonInfo(Guid entityId)
        {
            //插入操作
            //生成插入语句
            var sql = @"SELECT e.functionbuttons FROM crm_sys_entity AS e WHERE e.entityid=@entityid LIMIT 1; ";

            var sqlParameters = new List<DbParameter>();
            sqlParameters.Add(new NpgsqlParameter("entityid", entityId));
            var functionjson = ExecuteScalar(sql, sqlParameters.ToArray());
            FunctionJsonInfo result = null;
            if (functionjson != null && !string.IsNullOrEmpty(functionjson.ToString()))
            {
                result = JsonConvert.DeserializeObject<FunctionJsonInfo>(functionjson.ToString());
            }
            return result;
        }

        /// <summary>
        /// 获取实体页面入口信息
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public ServicesJsonInfo GetEntryPagesInfo(Guid entityId)
        {
            //插入操作
            //生成插入语句
            var sql = @"SELECT e.servicesjson FROM crm_sys_entity AS e WHERE e.entityid=@entityid LIMIT 1; ";

            var sqlParameters = new List<DbParameter>();
            sqlParameters.Add(new NpgsqlParameter("entityid", entityId));
            var pagejson = ExecuteScalar(sql, sqlParameters.ToArray());
            ServicesJsonInfo result = null;
            if (pagejson != null && !string.IsNullOrEmpty(pagejson.ToString()))
            {
                result = JsonConvert.DeserializeObject<ServicesJsonInfo>(pagejson.ToString());
            }
            return result;
        }

        /// <summary>
        /// 保存实体页面入口信息
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public bool SaveEntryPagesInfo(Guid entityId, ServicesJsonInfo info, int userNumber)
        {

            var sql = @"UPDATE crm_sys_entity SET servicesjson =@servicesjson ,recupdator=@recupdator,recupdated=now() WHERE entityid=@entityid; ";

            var sqlParameters = new List<DbParameter>();
            sqlParameters.Add(new NpgsqlParameter("servicesjson", JsonConvert.SerializeObject(info)) { NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Jsonb });
            sqlParameters.Add(new NpgsqlParameter("recupdator", userNumber));
            sqlParameters.Add(new NpgsqlParameter("entityid", entityId));
            var result = ExecuteNonQuery(sql, sqlParameters.ToArray());
            return result > 0;
        }

        /// <summary>
        /// 保存功能按钮json配置
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public bool SaveFunctionJson(Guid entityId, FunctionJsonInfo info, int userNumber, DbTransaction trans = null)
        {

            var sql = @"UPDATE crm_sys_entity SET functionbuttons =@functionbuttons ,recupdator=@recupdator,recupdated=now() WHERE entityid=@entityid; ";

            var sqlParameters = new List<DbParameter>();
            sqlParameters.Add(new NpgsqlParameter("functionbuttons", JsonConvert.SerializeObject(info)) { NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Jsonb });
            sqlParameters.Add(new NpgsqlParameter("recupdator", userNumber));
            sqlParameters.Add(new NpgsqlParameter("entityid", entityId));
            var result = ExecuteNonQuery(sql, sqlParameters.ToArray(), trans);
            return result > 0;
        }


        #region 设置查重

        public List<EntityFieldProMapper> NeedCheckFieldRepeat(string entityId, int userId)
        {
            var procName = @"
                SELECT * FROM crm_func_need_checkrepeat_list(@entityid, @userno)
            ";
            var param = new DynamicParameters();
            param.Add("entityid", entityId);
            param.Add("userno", userId);
            var result = DataBaseHelper.QueryStoredProcCursor<EntityFieldProMapper>(procName, param, CommandType.Text);
            return result;
        }
        public Dictionary<string, List<IDictionary<string, object>>> SetRepeatList(string entityId, int userId)
        {
            var procName = @"
                SELECT * FROM crm_func_checkrepeat_list(@entityid, @userno)
            ";
            var dataNames = new List<string> { "RepeatList" };
            var param = new DynamicParameters();
            param.Add("entityid", entityId);
            param.Add("userno", userId);
            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }



        public OperateResult CheckFieldRepeat(string entityId, string fieldId, string dataId, string dataValue, int userId)
        {
            var sql = @"
                SELECT * FROM crm_func_field_checkrepeat(@entityid,@fieldid,@dataid,@datavalue, @userno)
            ";
            var param = new DynamicParameters();
            param.Add("entityid", entityId);
            param.Add("fieldid", fieldId);
            param.Add("dataid", dataId);
            param.Add("datavalue", dataValue);
            param.Add("userno", userId);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }
        public OperateResult SaveSetRepeat(string entityId, string fieldIds, int userId)
        {
            var sql = @"
                SELECT * FROM crm_func_setrepeat_save(@entityid,@fieldids, @userno)
            ";
            var param = new DynamicParameters();
            param.Add("entityid", entityId);
            param.Add("fieldids", fieldIds);
            param.Add("userno", userId);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }
        #endregion

        #region 入口分组设置
        public OperateResult SaveEntanceGroup(string entranceJson, int userId)
        {
            var sql = @"
                SELECT * FROM crm_fun_entrance_save(@entrance , @userno)
            ";
            var param = new DynamicParameters();
            param.Add("entrance", entranceJson);
            param.Add("userno", userId);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public Dictionary<string, List<IDictionary<string, object>>> EntranceListQuery(int userNumber)
        {

            var procName =
                "SELECT crm_func_entrance_list(@userno)";

            var dataNames = new List<string> { "Crm", "Office", "DynamicEntity", "StandbyChoose" };
            var param = new DynamicParameters();
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }

        #endregion



        #region 关联控件接口

        public Dictionary<string, List<IDictionary<string, object>>> RelContorlValueQuery(RelControlValueMapper entity, int userNumber)
        {
            var procName =
                "SELECT crm_func_relcontrol_value(@recid,@entity,@fieldid,@userno)";

            var dataNames = new List<string> { "RelControlValue" };
            var param = new DynamicParameters();
            param.Add("recid", entity.RecId);
            param.Add("entityid", entity.EntityId);
            param.Add("fieldid", entity.FieldId);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }
        #endregion


        #region 保存个人列表设置

        public Dictionary<string, List<IDictionary<string, object>>> PersonalSettingQuery(Guid entityId, int userNumber)
        {
            var procName =
                "SELECT crm_func_personal_view_set_list(@entityid,@userno)";

            var dataNames = new List<string> { "PersonalSetting" };
            var param = new DynamicParameters();
            param.Add("entityid", entityId);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }
        public OperateResult SavePersonalViewSet(List<PersonalViewSetMapper> entities, int userId)
        {
            var sql = @"
                SELECT * FROM crm_fun_personal_view_set_save(@setting , @userno)
            ";
            var param = new DynamicParameters();
            param.Add("setting", JsonHelper.ToJson(entities));
            param.Add("userno", userId);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }
        #endregion



        #region 设置实体基础资料字段

        public OperateResult SaveEntityBaseData(List<EntityBaseDataMapper> entity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_entity_field_add(@entityid,@fieldlabel, @displayname, @fieldname, @fieldconfig, @fieldtype, @status,@controltype, @userno)
            ";

            var sql_viewcol = @"
                SELECT * FROM crm_func_field_visible_web_save(@entityid,@viewtype,@fieldids, @userno)
            ";

            var sql_name = @"select
                            fieldid  from crm_sys_entity_fields where controltype=1012  and entityid=@relentityid and recstatus=1 and fieldname='recname';";

            var lst = entity.AsList();
            var conn = DataBaseHelper.GetDbConnect();
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            var param = new DynamicParameters();
            param.Add("entityid", entity.FirstOrDefault().EntityId);
            param.Add("relentityid", entity.FirstOrDefault().RelEntityId);
            param.Add("fieldidnames", string.Join(",", entity.Select(t => t.FieldName).ToArray()));

            var fields = conn.Query<dynamic>(@"
                             Select 
                            fieldname, 
                            @relentityid::uuid entityid,
                            fieldlabel,
                            displayname,
                            controltype,
                            fieldtype,
                            json_object_set_key(COALESCE(fieldconfig,'{}')::json,'readonly','1'::int)::jsonb as fieldconfig,
                            recorder,
                            recstatus,
                            reccreator,
                            recupdator from crm_sys_entity_fields Where controltype not in (20, 1001, 1002, 1003, 1004, 1005, 1006, 1007, 1008, 1009, 1010, 1011, 1012, 1013)  and entityid=@entityid and fieldname in( SELECT UNNEST(string_to_array(@fieldidnames,','))) and recstatus=1
                        ", param).ToList();
            var len = fields.Count();
            OperateResult result = new OperateResult();
            if (len == 0)
            {
                result.Msg = "没有需要保存的基础资料";
                return result;
            }

            var transaction = conn.BeginTransaction();
            param = new DynamicParameters();
            param.Add("entityid", entity.FirstOrDefault().RelEntityId);
            param.Add("fieldnames", string.Join(",", entity.Select(t => t.FieldName).ToArray()));
            conn.Execute("Update  crm_sys_entity_field_rules set recstatus=0 where fieldid in (SELECT fieldid From crm_sys_entity_fields Where controltype not in (20, 1001, 1002, 1003, 1004, 1005, 1006, 1007, 1008, 1009, 1010, 1011, 1012, 1013) and entityid=@entityid::uuid);" +
                " Update   crm_sys_entity_fields set recstatus=0 Where controltype not in (20, 1001, 1002, 1003, 1004, 1005, 1006, 1007, 1008, 1009, 1010, 1011, 1012, 1013) and entityid=@entityid::uuid;", param);
            try
            {

                ArrayList al = new ArrayList();
                for (var i = 0; i < len; i++)
                {

                    param = new DynamicParameters();
                    param.Add("entityid", fields[i].entityid.ToString());
                    param.Add("fieldname", fields[i].fieldname);
                    param.Add("fieldlabel", fields[i].fieldlabel);
                    param.Add("displayname", fields[i].displayname);
                    param.Add("fieldname", fields[i].fieldname);
                    param.Add("fieldconfig", fields[i].fieldconfig);
                    param.Add("fieldtype", fields[i].fieldtype);
                    param.Add("controltype", fields[i].controltype);
                    param.Add("status", fields[i].recstatus);
                    param.Add("orderby", i);
                    param.Add("userno", userNumber);
                    result = DataBaseHelper.QuerySingle<OperateResult>(conn, sql, param);
                    if (result.Flag == 0)
                        throw new Exception("保存配置异常");
                    al.Add(result.Id);
                }
                param = new DynamicParameters();
                param.Add("relentityid", entity.FirstOrDefault().RelEntityId);
                var recName = conn.Query<dynamic>(sql_name, param).FirstOrDefault();
                if (recName != null)
                {
                    al.Add(recName.fieldid);
                    param = new DynamicParameters();
                    param.Add("entityid", fields.FirstOrDefault().entityid.ToString());
                    param.Add("viewtype", 0);
                    param.Add("fieldids", string.Join(",", al.ToArray()));
                    param.Add("userno", userNumber);
                    result = DataBaseHelper.QuerySingle<OperateResult>(conn, sql_viewcol, param);

                    if (result.Flag == 0)
                        throw new Exception("保存配置异常");
                }
                transaction.Commit();
                result.Msg = "保存配置成功";
            }
            catch (Exception ex)
            {
                //出现异常，事务Rollback
                transaction.Rollback();
                result.Msg = "保存配置失败";
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
            result.Id = Guid.NewGuid().ToString();
            return result;
        }

        public Dictionary<string, List<IDictionary<string, object>>> EntityBaseDataFieldQuery(EntityBaseDataFieldMapper entity, int userNumber)
        {

            var procName =
                "SELECT crm_func_cust_common_field_list(@entityid,@commentityid,@userno)";

            var dataNames = new List<string> { "FieldNotVisible", "FieldVisible" };
            var param = new DynamicParameters();
            param.Add("entityid", entity.EntityId);
            param.Add("commentityid", entity.CommEntityId);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }
        #endregion

        /// <summary>
        /// 获取关联实体列表
        /// </summary>
        /// <param name="entityid"></param>
        /// <param name="usernumber"></param>
        /// <returns></returns>
        public List<RelateEntity> GetRelateEntityList(Guid entityid, int usernumber)
        {
            List<RelateEntity> resutl = new List<RelateEntity>();
            var sql = @"SELECT entityid AS EntityId,entityname AS EntityName,entitytable AS EntityTableName,modeltype AS ModelType 
                        FROM crm_sys_entity WHERE relentityid=@entityid AND recstatus=1
                        ";

            var sqlParameters = new List<DbParameter>();
            sqlParameters.Add(new NpgsqlParameter("entityid", entityid));

            using (var conn = myDBHelper.GetDbConnect())
            {
                conn.Open();
                var tran = conn.BeginTransaction();
                try
                {
                    resutl = myDBHelper.ExecuteQuery<RelateEntity>(tran, sql, sqlParameters.ToArray());
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return resutl;
        }

        /// <summary>
        /// 获取实体的菜单列表
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public List<EntityMenuInfo> GetEntityMenuInfoList(Guid entityId)
        {
            List<RelateEntity> resutl = new List<RelateEntity>();
            var sql = @" SELECT * FROM crm_sys_entity_menu WHERE entityid=@entityid";

            var sqlParameters = new List<DbParameter>();
            sqlParameters.Add(new NpgsqlParameter("entityid", entityId));

            return ExecuteQuery<EntityMenuInfo>(sql, sqlParameters.ToArray());


        }

        /// <summary>
        /// 获取实体的tabid
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public Guid GetEntityRelTabId(Guid entityId, string entitytaburl)
        {
            var sql = @"SELECT relid FROM crm_sys_entity_rel_tab WHERE entityid=@entityid AND entitytaburl=@entitytaburl AND recstatus=1 LIMIT 1;";

            var sqlParameters = new List<DbParameter>();
            sqlParameters.Add(new NpgsqlParameter("entityid", entityId));
            sqlParameters.Add(new NpgsqlParameter("entitytaburl", entitytaburl));
            var resutl = ExecuteScalar(sql, sqlParameters.ToArray());
            Guid relid = Guid.Empty;
            if (resutl != null && Guid.TryParse(resutl.ToString(), out relid))
            {
                return relid;
            }
            else throw new Exception(string.Format("crm_sys_entity_rel_tab不存在entityid={0}和entitytaburl={1}的记录", entityId, entitytaburl));
        }


        /// <summary>
        /// 获取实体的tabid
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public void SyncFunctionList(Guid entityId, List<FunctionInfo> webfuncs, List<FunctionInfo> mobilefuncs, int usernumber)
        {
            using (var conn = myDBHelper.GetDbConnect())
            {
                conn.Open();
                var tran = conn.BeginTransaction();
                try
                {

                    List<FunctionInfo> funclist = new List<FunctionInfo>();
                    if (webfuncs != null && webfuncs.Count > 0)
                        funclist.AddRange(webfuncs);
                    if (mobilefuncs != null && mobilefuncs.Count > 0)
                        funclist.AddRange(mobilefuncs);

                    #region --获取根节点的父节点--
                    var entrytype_sql = @"SELECT entrytype FROM crm_sys_entrance WHERE entityid =@entityid AND recstatus=1  LIMIT 1";
                    DbParameter[] entrytypeParms = new DbParameter[]
                    {
                        new NpgsqlParameter("entityid", entityId),
                    };
                    var entrytypeResult = ExecuteScalar(entrytype_sql, entrytypeParms, tran);
                    int entrytype = -1;
                    if (entrytypeResult != null)
                    {
                        int.TryParse(entrytypeResult.ToString(), out entrytype);
                    }
                    Guid webRootfuncid = Guid.Empty;
                    Guid mobileRootfuncid = Guid.Empty;
                    switch (entrytype)
                    {
                        case 0://crm的function根
                            webRootfuncid = Guid.Parse("c84ff512-fe7d-4d7e-8cf2-f0dc72ea9cb3");
                            mobileRootfuncid = Guid.Parse("da5fbce8-8c52-4d3c-8e44-8aa5bfdfaae1");
                            break;
                        case 1://办公的function根
                            webRootfuncid = Guid.Parse("7c927ecd-bdf9-424f-bed8-e577783c3922");
                            mobileRootfuncid = Guid.Parse("39287afa-8254-446b-960d-2924cebfc84b");
                            break;
                        default://其他实体的function根
                            webRootfuncid = Guid.Parse("26177703-12c4-46d8-8594-3372705524fb");
                            mobileRootfuncid = Guid.Parse("da67b7fc-a16e-4c74-bb44-c3d833416645");
                            break;
                    }

                    var rootfuncs = funclist.Where(m => m.ParentId == Guid.Empty);
                    foreach (var root in rootfuncs)
                    {
                        root.ParentId = root.DeviceType == 0 ? webRootfuncid : mobileRootfuncid;
                    }

                    #endregion

                    #region --获取关联的动态实体列表--
                    var entity_sql = @"SELECT entityid FROM crm_sys_entity WHERE relentityid =@entityid AND modeltype=3 AND recstatus=1";
                    DbParameter[] entityParms = new DbParameter[]
                    {
                        new NpgsqlParameter("entityid", entityId),
                    };
                    var dynamicEntityIds = ExecuteQuery(entity_sql, entityParms, tran);
                    List<Guid> entityids = new List<Guid>();
                    entityids.Add(entityId);
                    foreach (var m in dynamicEntityIds)
                    {
                        Guid dynamicEntityId = Guid.Empty;
                        if (m != null && m.ContainsKey("entityid") && m["entityid"] != null && Guid.TryParse(m["entityid"].ToString(), out dynamicEntityId))
                        {
                            entityids.Add(dynamicEntityId);
                        }
                    }
                    #endregion

                    #region --清空旧数据--
                    var deletedfuncid_sql = @"SELECT funcid FROM crm_sys_function  WHERE entityid=ANY(@entityids);";
                    DbParameter[] deleteFuncid_Parms = new DbParameter[]
                    {
                        new NpgsqlParameter("entityids", entityids.ToArray()),
                    };
                    var deleteFuncIdsResult = ExecuteQuery(deletedfuncid_sql, deleteFuncid_Parms, tran);
                    List<Guid> allDeleteFuncIds = new List<Guid>();
                    List<Guid> deleteFuncIds = new List<Guid>();
                    foreach (var m in deleteFuncIdsResult)
                    {
                        Guid funcid = Guid.Empty;
                        if (m != null && m.ContainsKey("funcid") && m["funcid"] != null && Guid.TryParse(m["funcid"].ToString(), out funcid))
                        {
                            allDeleteFuncIds.Add(funcid);
                            if (!funclist.Exists(a => a.FuncId == funcid))//如果当前function不在保存列表中，则表示永久删除
                            {
                                deleteFuncIds.Add(funcid);
                            }
                        }
                    }

                    var delete_sql = @"DELETE FROM crm_sys_function WHERE entityid=ANY(@entityids);
                                       DELETE FROM crm_sys_function_treepaths WHERE ancestor=ANY(@funcids) OR descendant=ANY(@funcids);
                                       DELETE FROM crm_sys_vocation_function_relation WHERE functionid=ANY(@deletedfuncids) ;
                                       DELETE FROM crm_sys_vocation_function_rule_relation WHERE functionid=ANY(@deletedfuncids) ;
                                    ";
                    DbParameter[] deleteParms = new DbParameter[]
                    {
                        new NpgsqlParameter("entityids", entityids.ToArray()),
                        new NpgsqlParameter("funcids", allDeleteFuncIds.ToArray()),
                        new NpgsqlParameter("deletedfuncids", deleteFuncIds.ToArray())
                     };
                    ExecuteNonQuery(delete_sql, deleteParms, tran);
                    #endregion

                    #region --插入新数据--
                    var insert_sql = @"INSERT INTO crm_sys_function (funcid,funcname,funccode,parentid,entityid,devicetype,reccreator,recupdator,rectype,relationvalue,routepath,islastchild) 
		                             VALUES (@funcid,@funcname,@funccode,@parentid,@entityid,@devicetype,@reccreator,@recupdator,@rectype,@relationvalue,@routepath,@islastchild);
                                     INSERT INTO crm_sys_function_treepaths(ancestor,descendant,nodepath)
		                                    SELECT t.ancestor,@funcid,nodepath+1
		                                    FROM crm_sys_function_treepaths AS t
		                                    WHERE t.descendant = @parentid
		                                    UNION ALL
		                                    SELECT @funcid,@funcid,0;
                                    ";
                    var insert_params = new List<DbParameter[]>();

                    foreach (var func in funclist)
                    {
                        insert_params.Add(new DbParameter[] {
                            new NpgsqlParameter("funcid", func.FuncId),
                            new NpgsqlParameter("funcname", func.FuncName),
                            new NpgsqlParameter("funccode", func.Funccode),
                            new NpgsqlParameter("parentid", func.ParentId),
                            new NpgsqlParameter("entityid", func.EntityId),
                            new NpgsqlParameter("devicetype", func.DeviceType),
                            new NpgsqlParameter("reccreator", usernumber),
                            new NpgsqlParameter("recupdator", usernumber),
                            new NpgsqlParameter("rectype", (int)func.RecType),
                            new NpgsqlParameter("relationvalue", func.RelationValue),
                            new NpgsqlParameter("routepath", func.RoutePath),
                            new NpgsqlParameter("islastchild", func.IsLastChild)
                        });
                    }

                    ExecuteNonQueryMultiple(insert_sql, insert_params, tran);
                    #endregion
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }



        /// <summary>
        /// 获取动态实体
        /// </summary>
        /// <param name="modelType"></param>
        /// <returns></returns>
        public List<EntityProMapper> GetDynamicEntityList(int modelType = 3)
        {
            var sql = @"select entityid::text,entityname,relentityid::text from crm_sys_entity where relentityid is not null and modeltype=@modeltype and recstatus=1";

            var param = new
            {
                ModelType = modelType
            };

            return DataBaseHelper.Query<EntityProMapper>(sql, param);
        }

        #region 扩展配置
        /// <summary>
        /// 获取事件表信息
        /// </summary>
        /// <param name="tran"></param>
        /// <param name="entityId"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public List<FuncEvent> GetFuncEvent(DbTransaction tran, Guid entityId, int userNumber)
        {
            var sql = @"select * from crm_sys_entity_func_event where typeid in 
(select ec.categoryid from crm_sys_entity_category as ec where entityid = @entityid)";

            var param = new DbParameter[] { new NpgsqlParameter("entityid", entityId) };

            return ExecuteQuery<FuncEvent>(sql, param, tran);
        }

        /// <summary>
        /// 获取配置表信息
        /// </summary>
        /// <param name="tran"></param>
        /// <param name="entityId"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public List<ActionExtConfig> GetActionExtConfig(DbTransaction tran, Guid entityId, int userNumber)
        {
            var sql = @"select * from crm_sys_actionext_config where entityid = @entityid";

            var param = new DbParameter[] { new NpgsqlParameter("entityid", entityId) };

            return ExecuteQuery<ActionExtConfig>(sql, param, tran);
        }

        /// <summary>
        /// 获取方法信息
        /// </summary>
        /// <param name="tran"></param>
        /// <param name="entityId"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public List<ExtFunction> GetExtFunction(DbTransaction tran, Guid entityId, int userNumber)
        {
            var sql = @"select * from crm_sys_entity_extfunction where entityid = @entityid";

            var param = new DbParameter[] { new NpgsqlParameter("entityid", entityId) };

            return ExecuteQuery<ExtFunction>(sql, param, tran);
        }

        public void UpdateFuncEvent(DbTransaction tran, Guid entityId, List<FuncEvent> data, int userNumber)
        {
            #region sql
            string delSQL = @"Delete from crm_sys_entity_func_event where typeid in 
(select ec.categoryid from crm_sys_entity_category as ec where entityid = @entityid)";
            string insertSql = string.Format(@"INSERT INTO crm_sys_entity_func_event(funceventid,typeid,operatetype,funcname)
                                   SELECT uuid_generate_v4(),typeid,operatetype,funcname
                                   FROM json_populate_recordset(null::crm_sys_entity_func_event,@condition) where funcname <> ''");
            #endregion
            #region 删除
            var param = new DbParameter[]
            {
                    new NpgsqlParameter("@entityid",entityId)
            };
            ExecuteNonQuery(delSQL, param, tran);
            #endregion

            #region 插入
            DbParameter[] rulesparams = new DbParameter[] { new NpgsqlParameter("condition", JsonConvert.SerializeObject(data)) { NpgsqlDbType = NpgsqlDbType.Json } };
            ExecuteNonQuery(insertSql, rulesparams, tran);
            #endregion
        }

        public void UpdateActionExt(DbTransaction tran, Guid entityId, List<ActionExtConfig> data, int userNumber)
        {
            #region sql
            string delSQL = @"Delete from crm_sys_actionext_config where entityid = @entityid";
            string insertSql = string.Format(@"INSERT INTO crm_sys_actionext_config(recid,routepath,implementtype,assemblyname,classtypename,funcname,operatetype,resulttype,recstatus,entityid)
                                   SELECT uuid_generate_v4(),routepath,implementtype,assemblyname,classtypename,funcname,operatetype,resulttype,1 recstatus,entityid
                                   FROM json_populate_recordset(null::crm_sys_actionext_config,@condition)");
            #endregion
            #region 删除
            var param = new DbParameter[]
            {
                    new NpgsqlParameter("@entityid",entityId)
            };
            ExecuteNonQuery(delSQL, param, tran);
            #endregion

            #region 插入
            DbParameter[] rulesparams = new DbParameter[] { new NpgsqlParameter("condition", JsonConvert.SerializeObject(data)) { NpgsqlDbType = NpgsqlDbType.Json } };
            ExecuteNonQuery(insertSql, rulesparams, tran);
            #endregion
        }

        public void UpdateExtFunction(DbTransaction tran, Guid entityId, List<ExtFunction> data, int userNumber)
        {
            foreach (ExtFunction item in data) {
                item.RecStatus = 1;
            }
            #region sql
            string delSQL = @"Delete from crm_sys_entity_extfunction where entityid = @entityid";
            string insertSql = string.Format(@"INSERT INTO crm_sys_entity_extfunction(dbfuncid,functionname,entityid,parameters,recorder,returntype,recstatus,remark,enginetype,uscript)
                                   SELECT uuid_generate_v4(),functionname,entityid,parameters,recorder,returntype,recstatus,remark,enginetype,uscript
                                   FROM json_populate_recordset(null::crm_sys_entity_extfunction,@condition)");
            #endregion
            #region 删除
            var param = new DbParameter[]
            {
                    new NpgsqlParameter("@entityid",entityId)
            };
            ExecuteNonQuery(delSQL, param, tran);
            #endregion

            #region 插入
            DbParameter[] rulesparams = new DbParameter[] { new NpgsqlParameter("condition", JsonConvert.SerializeObject(data)) { NpgsqlDbType = NpgsqlDbType.Json } };
            ExecuteNonQuery(insertSql, rulesparams, tran);
            #endregion
        }

        public List<Dictionary<string, object>> QueryEntityWithDataSource(DbTransaction tran, int userId)
        {
            try {
                string strSQL = @"select * from crm_sys_entity where
                             entityid in (
                            select entityid
                            from crm_sys_entity_datasource
                            where recstatus = 1 ) order by entityname ";
                return ExecuteQuery(strSQL, new DbParameter[] { }, tran);
            }
            catch (Exception ex)
            {

                return new List<Dictionary<string, object>>();
            }
        }

        public Dictionary<string, object> GetEntityInfoByEntityName(DbTransaction tran, string entityName, int userId)
        {
            try
            {
                string strSQL = "select* from crm_sys_entity where entityname =@entityname limit 1 ";
                DbParameter[] p = new DbParameter[] {
                    new Npgsql.NpgsqlParameter("@entityname",entityName)
                };
                List<Dictionary<string, object>> ret = ExecuteQuery(strSQL, p, tran);
                if (ret == null || ret.Count == 0) return null;
                return ret.ElementAt(0);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public Dictionary<string, object> GetEntityInfoByTableName(DbTransaction tran, string tablename, int userId)
        {
            try
            {
                string strSQL = "select* from crm_sys_entity where entitytable =@entitytable limit 1 ";
                DbParameter[] p = new DbParameter[] {
                    new Npgsql.NpgsqlParameter("@entitytable",tablename)
                };
                List<Dictionary<string, object>> ret = ExecuteQuery(strSQL, p, tran);
                if (ret == null || ret.Count == 0) return null;
                return ret.ElementAt(0);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public Dictionary<string, object> GetFieldInfoByFieldName(DbTransaction tran, string fieldName, Guid entityId, int userId)
        {
            try
            {
                string strSQL = @"select * from crm_sys_entity_fields 
                            where entityid = @entityid and fieldname = @displayname
                            limit 1 ";
                DbParameter[] p = new DbParameter[] {
                    new Npgsql.NpgsqlParameter("@entityid",entityId),
                    new Npgsql.NpgsqlParameter("@displayname",fieldName)
                };
                List<Dictionary<string, object>> ret = ExecuteQuery(strSQL, p, tran);
                if (ret == null || ret.Count == 0) return null;
                return ret.ElementAt(0);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public void DeleteEntityFieldRules(Guid catelogid, int userId)
        {
            try
            {
                string strSQL = "delete from crm_sys_entity_field_rules where typeid = @typeid";
                DbParameter[] param = new DbParameter[] {
                    new Npgsql.NpgsqlParameter("@typeid",catelogid)
                };
                ExecuteNonQuery(strSQL, param, null);
            }
            catch (Exception ex) { }
        }

        public void MapEntityType(Guid subTypeId, Guid mainTypeId)
        {

            try
            {
                string strSQL = "update  crm_sys_entity_category set relcategoryid = @relcategoryid where categoryid =@categoryid ";
                DbParameter[] param = new DbParameter[] {
                    new Npgsql.NpgsqlParameter("relcategoryid",mainTypeId),
                    new Npgsql.NpgsqlParameter("categoryid",subTypeId)
                };
                ExecuteNonQuery(strSQL, param, null);
            }
            catch (Exception ex) {
            }
        }

        public void UpdateEntityFieldConfig(Guid fieldId, Dictionary<string, object> fieldConfig, int v)
        {
            try
            {
                string strSQL = "update crm_sys_entity_fields set fieldconfig = @fieldconfig where fieldid = @fieldid";
                DbParameter[] p = new DbParameter[] {
                    new Npgsql.NpgsqlParameter("@fieldconfig",Newtonsoft.Json.JsonConvert.SerializeObject(fieldConfig)){ NpgsqlDbType = NpgsqlDbType.Jsonb},
                    new Npgsql.NpgsqlParameter("@fieldid",fieldId)
                };
                ExecuteNonQuery(strSQL, p, null);
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
        }

        public void UpdateEntityFieldName(DbTransaction tran, Guid fieldId, string displayname, int userId)
        {
            try
            {
                string strSQL = "update crm_sys_entity_fields set displayname=@displayname where fieldid=@fieldid";
                DbParameter[] p = new DbParameter[] {
                    new NpgsqlParameter("@fieldid",fieldId),
                    new NpgsqlParameter("@displayname",displayname)
                };
                ExecuteNonQuery(strSQL, p, tran);

            }
            catch (Exception ex) { }
        }

        public Dictionary<string, object> GetFieldInfoByDisplayName(DbTransaction tran, string displayName, Guid entityId, int userId)
        {
            try
            {
                string strSQL = @"select * from crm_sys_entity_fields where( fieldlabel = @displayname or  displayname = @displayname  )and entityid =@entityid
                            limit 1 ";
                DbParameter[] p = new DbParameter[] {
                    new Npgsql.NpgsqlParameter("@entityid",entityId),
                    new Npgsql.NpgsqlParameter("@displayname",displayName)
                };
                List<Dictionary<string, object>> ret = ExecuteQuery(strSQL, p, tran);
                if (ret == null || ret.Count == 0) return null;
                return ret.ElementAt(0);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public void SaveEntityInputMethod(DbTransaction tran, Guid entityId, List<EntityInputModeInfo> inputs, int userId)
        {
            try
            {
                string strSQL = "update crm_sys_entity set  inputmethod=@inputmethod where entityid=@entityid";
                DbParameter[] p = new DbParameter[] {
                    new Npgsql.NpgsqlParameter("@inputmethod",JsonConvert.SerializeObject(inputs)){
                        NpgsqlDbType = NpgsqlDbType.Jsonb
                    },
                    new Npgsql.NpgsqlParameter("@entityid",entityId)
                };
                ExecuteNonQuery(strSQL, p, tran);
            }
            catch (Exception ex) {
                throw (ex);
            }
        }
        #endregion


    }
}
