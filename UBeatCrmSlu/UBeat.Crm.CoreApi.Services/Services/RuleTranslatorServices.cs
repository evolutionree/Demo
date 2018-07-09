using AutoMapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.DomainMapper.Rule;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.DynamicEntity;
using UBeat.Crm.CoreApi.DomainModel.EntityPro;
using UBeat.Crm.CoreApi.DomainModel.SalesTarget;
using UBeat.Crm.CoreApi.DomainModel.Version;
using UBeat.Crm.CoreApi.DomainModel.Vocation;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.DynamicEntity;
using UBeat.Crm.CoreApi.Services.Models.EntityPro;
using UBeat.Crm.CoreApi.Services.Models.Rule;
using UBeat.Crm.CoreApi.Services.Models.SalesTarget;
using UBeat.Crm.CoreApi.Services.Models.Vocation;
using UBeat.Crm.CoreApi.Services.Utility;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class RuleTranslatorServices : BaseServices
    {
        private readonly IEntityProRepository _entityProRepository;
        private readonly IRuleRepository _ruleRepository;
        private readonly IMapper mapper;


        private readonly IVocationRepository _vocationRepository;
        private readonly ISalesTargetRepository _salesTargetRepository;
        private readonly IReminderRepository _reminderRepository;


        public RuleTranslatorServices(IMapper _mapper, IEntityProRepository entityProRepository, IRuleRepository ruleRepository, IVocationRepository vocationRepository, ISalesTargetRepository salesTargetRepository, IReminderRepository reminderRepository)
        {
            mapper = _mapper;
            _entityProRepository = entityProRepository;
            _ruleRepository = ruleRepository;
            _vocationRepository = vocationRepository;

            _salesTargetRepository = salesTargetRepository;
            _reminderRepository = reminderRepository;
        }

        public OutputResult<object> MenuRuleInfoQuery(MenuRuleModel entityModel, int userId)
        {
            var entity = mapper.Map<MenuRuleModel, MenuRuleMapper>(entityModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            var infoList = _ruleRepository.MenuRuleInfoQuery(entity.menuid, userId);
            var obj = infoList.GroupBy(t => new
            {
                t.RuleId,
                t.RuleName,
                t.RuleSet,
                t.MenuName
            }).Select(group => new RuleInfoModel
            {
                RuleId = group.Key.RuleId,
                RuleName = group.Key.RuleName,
                MenuName = group.Key.MenuName,
                RuleItems = group.Select(t => new RuleItemInfoModel
                {
                    ItemId = t.ItemId,

                    ItemName = t.ItemName,
                    FieldId = t.FieldId,
                    Operate = t.Operate,
                    UseType = t.UseType,
                    RuleData = t.RuleData,
                    RuleSql = t.RuleSql,
                    RuleType = t.RuleType
                }).ToList(),
                RuleSet = new RuleSetInfoModel
                {
                    RuleSet = group.Key.RuleSet
                }
            }).ToList();
            List<RuleInfoModel> tmp = (List<RuleInfoModel>)obj;
            foreach (RuleInfoModel i in tmp) {
                List<RuleItemInfoModel> tmpList = i.RuleItems.ToList();
                tmpList.Sort((x, y) => x.ItemName.CompareTo(y.ItemName));
                i.RuleItems = tmpList;
            }
            return new OutputResult<object>(obj);
        }

        public void SaveMenuOrder(List<EntityMenuOrderByModel> paramInfo, int userId)
        {
            try
            {
                DbTransaction trans = null;
                foreach (EntityMenuOrderByModel item in paramInfo) {
                    this._ruleRepository.SaveMenuOrder(item.MenuId,item.OrderBy, userId, trans);
                }
                

            }
            catch (Exception ex) {
                throw (ex);
            }
        }

        public OutputResult<object> EntityRuleMenuQuery(string entityId, int userNumber)
        {
            return new OutputResult<object>(_ruleRepository.MenuRuleQuery(entityId, userNumber));
        }
        public OutputResult<object> DisabledEntityRule(string entityId, int userNumber)
        {
            var res = HandleResult(_ruleRepository.DisabledEntityRule(entityId, userNumber));
            if (res.Status == 0)
            {

                RemoveCommonCache();
                RemoveAllUserCache();

                IncreaseDataVersion(DataVersionType.EntityData);
                IncreaseDataVersion(DataVersionType.PowerData);
            }

            return res;
        }
        public OutputResult<object> SaveRule(RuleModel entityModel, int userId)
        {

            var ruleSetEntity = mapper.Map<RuleSetModel, RuleSetMapper>(entityModel.RuleSet);
            if (ruleSetEntity == null || !ruleSetEntity.IsValid())
            {
                return HandleValid(ruleSetEntity);
            }
            List<EntityFieldProMapper> fields;
            if (entityModel.TypeId == 2)
            {
                fields = _entityProRepository.FieldQuery(entityModel.RelEntityId, userId);
            }
            else
            {
                fields = _entityProRepository.FieldQuery(entityModel.EntityId, userId);
            }
            foreach (var entity in entityModel.RuleItems)
            {
                var itemEntity = mapper.Map<RuleItemModel, RuleItemMapper>(entity);
                if (itemEntity == null || !itemEntity.IsValid())
                {
                    return HandleValid(itemEntity);
                }
                switch (entity.RuleType)
                {
                    case 0:
                    case 1:
                        {
                            var entityField = fields.SingleOrDefault(t => t.FieldId == entity.FieldId);
                            if (entityField.ControlType != entity.ControlType) throw new Exception("配置字段类型不匹配");
                            entity.RuleSql = TranslateRuleConditionSql(entity.Operate, entity.RuleType, entity.RuleData, userId, entityField);
                            break;
                        }
                    case 2:
                        {
                            var data = JObject.Parse(entity.RuleData);
                            entity.RuleSql = string.Format(@"({0})", data["dataVal"].ToString());
                            break;
                        }
                    case 10://分支流程条件--实体字段
                        {
                            List<EntityFieldProMapper> entityfields= _entityProRepository.FieldQuery(entityModel.EntityId, userId);
                            
                            var entityField = entityfields.SingleOrDefault(t => t.FieldId == entity.FieldId);
                            if (entityField == null||entityField.ControlType != entity.ControlType)
                                throw new Exception("配置字段类型不匹配");
                            entity.RuleSql = TranslateRuleConditionSql(entity.Operate, entity.RuleType, entity.RuleData, userId, entityField);
                            
                        }
                        break;
                    case 11://分支流程条件--rel实体字段
                        {
                           
                            List<EntityFieldProMapper> relentityfields = _entityProRepository.FieldQuery(entityModel.RelEntityId, userId);
                            var entityField = relentityfields.SingleOrDefault(t => t.FieldId == entity.FieldId);
                            if (entityField == null||entityField.ControlType != entity.ControlType)
                                throw new Exception("配置字段类型不匹配");
                            entity.RuleSql = TranslateRuleConditionSql(entity.Operate, entity.RuleType, entity.RuleData, userId, entityField, "rel");

                        }
                        break;
                    case 2001://分支流程条件--发起人
                        {
                            entity.RuleSql = UserIdFormatInOrNotIn(entity.Operate, "flowluancher", entity.RuleData);
                        }
                        break;
                    case 2002://分支流程条件--发起人部门 
                        {
                            entity.RuleSql = FormatInOrNotIn(entity.Operate, "flowluancherdeptid", entity.RuleData);
                        }
                        break;
                    case 2003://分支流程条件--发起人上级部门 
                        {
                            entity.RuleSql = FormatInOrNotIn(entity.Operate, "flowluancherpredeptid", entity.RuleData);
                        }
                        break;
                    case 2004://分支流程条件--发起人角色
                        {
                            entity.RuleSql = FormatInOrNotIn(entity.Operate, "flowluancherroleid", entity.RuleData);
                            
                        }
                        break;
                    case 2005://分支流程条件--是否是领导
                        {
                            var data = JObject.Parse(entity.RuleData);
                            var dataValue = data["dataVal"].ToString();
                            entity.RuleSql = string.Format(@"(flowluancherisleader = {0})",  dataValue);
                        }
                        break;

                    default: throw new Exception("尚未实现的规则类型");
                }
                entity.Relation.ParamIndex = -1;
                entity.ItemId = Guid.NewGuid().ToString();
                entity.Relation.ItemId = entity.ItemId;
                entity.Relation.RuleId = entityModel.RuleId;
            }
            var ruleItems = entityModel.RuleItems.ToList();
            entityModel.RuleSet.RuleFormat = TranslateRuleSet(entityModel.RuleSet.RuleSet, ref ruleItems);
            var mapperEntity = mapper.Map<RuleModel, RuleMapper>(entityModel);
            if (mapperEntity == null || !mapperEntity.IsValid())
            {
                return HandleValid(mapperEntity);
            }
            string id = mapperEntity.id;
            string ruleJson = JsonConvert.SerializeObject(mapperEntity);
            string ruleItemJson = JsonConvert.SerializeObject(mapperEntity.RuleItems);
            string ruleItemRel = JsonConvert.SerializeObject(mapperEntity.RuleItems.Where(t => t.Relation != null).Select(t => t.Relation));
            string ruleSet = JsonConvert.SerializeObject(mapperEntity.RuleSet);
            var res = HandleResult(_ruleRepository.SaveRule(mapperEntity.id, mapperEntity.typeid, ruleJson, ruleItemJson, ruleSet, ruleItemRel, userId));
            if (res.Status == 0)
            {
                //RemoveUserDataCache(userId);
                RemoveCommonCache();
                RemoveAllUserCache();
                //GetCommonCacheData(userId, true);
                IncreaseDataVersion(DataVersionType.EntityData);
                IncreaseDataVersion(DataVersionType.PowerData);

            }

            return res;
        }
        private string FormatInOrNotIn(string operate, string fieldName, string strData)
        {
            var data = JObject.Parse(strData);
            string ruleSql = null;
            var dataValue = data["dataVal"].ToString();

            switch (operate)
            {
                case "in":
                    ruleSql = string.Format(@"({0} IN (SELECT unnest(string_to_array('{1}', ','))::uuid AS tempid))", fieldName, dataValue);
                    break;
                case "not in":
                    ruleSql = string.Format(@"({0} NOT IN (SELECT unnest(string_to_array('{1}', ','))::uuid AS tempid))", fieldName, dataValue);
                    break;
            }
            return ruleSql;
        }



        private string UserIdFormatInOrNotIn(string operate, string fieldName,string strData)
        {
            var data = JObject.Parse(strData);
            string ruleSql = null;
            //var dataValue = data["dataVal"].ToString();
            // string ruleSql=null;
            // switch (operate)
            // {
            //     case "in":
            //         ruleSql = string.Format(@"({0} {1} ('{2}'))", fieldName, Condition.In.GetSqlOperate(), dataValue);
            //         break;
            //     case "not in":
            //         ruleSql = string.Format(@"({0} {1} ('{2}'))", fieldName, Condition.NotIn.GetSqlOperate(), dataValue);
            //         break;
            //     case "=":
            //         ruleSql = string.Format(@"({0} {1} {2})", fieldName, Condition.Equal.GetSqlOperate(), dataValue);
            //         break;
            // }
            // return ruleSql;


            var dataValue = data["dataVal"].ToString();
            string userIds = string.Empty;
            string deptIds = string.Empty;
            JToken value;
            if (data.TryGetValue("users", out value))
            {
                userIds = value.ToString();
            }
            if (data.TryGetValue("deptids", out value))
            {
                deptIds = value.ToString();
            }

            StringBuilder sb = new StringBuilder();
            switch (operate)
            {
                case "in":
                    sb.Append("(");
                    if (!string.IsNullOrEmpty(dataValue) && dataValue != "{}")
                    {
                        var strSql = string.Format(@" (EXISTS( SELECT * FROM (select regexp_split_to_table({0}::text,',')::int4 AS sel ) AS t WHERE t.sel {1} ({2}))) ", fieldName, Condition.In.GetSqlOperate(), dataValue);
                        sb.Append(strSql);
                    }
                    else
                    {
                        var strSql = "1=1";
                        sb.Append(strSql);
                    }
                    if (!string.IsNullOrEmpty(userIds))
                    {
                       var strSql = string.Format(@" AND (EXISTS( SELECT * FROM (select regexp_split_to_table({0}::text,',')::int4 AS sel ) AS t WHERE t.sel {1} ({2}))) ", fieldName, Condition.In.GetSqlOperate(), userIds);
                        sb.Append(strSql);
                    }
                    if (!string.IsNullOrEmpty(deptIds))
                    {
                        string departmentSql = "SELECT userid FROM crm_sys_account_userinfo_relate WHERE recstatus = 1 AND deptid IN (SELECT deptid FROM crm_func_department_tree('{0}', 1))  ";
                        ArrayList ar = new ArrayList();
                        foreach (var depid in deptIds.Split(','))
                        {
                            ar.Add(string.Format(departmentSql, depid));
                        }
                        departmentSql = " ( " + string.Join(" UNION ALL ", ar.ToArray()) + " ) ";
                        var strSql = string.Format(@" AND (EXISTS( SELECT * FROM (select regexp_split_to_table({0}::text,',')::int4 AS sel ) AS t WHERE t.sel {1} ({2})))", fieldName, Condition.In.GetSqlOperate(), departmentSql);
                        sb.Append(strSql);
                    }
                    sb.Append(")");
                    ruleSql = sb.ToString();
                    break;
                case "not in":
                    sb.Append("(");
                    if (!string.IsNullOrEmpty(dataValue) && dataValue != "{}")
                    {
                        string strSql = "";
                        if (dataValue == "{currentUser}")
                        {
                             strSql = string.Format(@"  (NOT EXISTS ( SELECT * FROM (SELECT UNNEST(ARRAY[{0}]) userid) AS arr INNER JOIN (SELECT splitable.regexp_split_to_table::int4 as splitid FROM  (select * FROM  regexp_split_to_table({1}::text,',')) as splitable ) AS tmp ON tmp.splitid=arr.userid))", dataValue, fieldName);
                        }
                        else if (dataValue == "{currentDepartment}" || dataValue == "{subDepartment}")
                        {
                             strSql = string.Format(@"  (NOT EXISTS ( SELECT * FROM ({0}) AS arr INNER JOIN (SELECT splitable.regexp_split_to_table::int4 as splitid FROM  (select * FROM  regexp_split_to_table({1}::text,',')) as splitable ) AS tmp ON tmp.splitid=arr.userid))", dataValue, fieldName);
                        }
                        sb.Append(strSql);
                    }
                    else
                    {
                       var strSql = "1=1";
                        sb.Append(strSql);
                    }
                    if (!string.IsNullOrEmpty(userIds))
                    {
                        var strSql = string.Format(@" AND (NOT EXISTS ( SELECT * FROM (SELECT UNNEST(ARRAY[{0}]) userid) AS arr INNER JOIN (SELECT splitable.regexp_split_to_table::int4 as splitid FROM  (select * FROM  regexp_split_to_table({1}::text,',')) as splitable ) AS tmp ON tmp.splitid=arr.userid))", userIds, fieldName);
                        sb.Append(strSql);
                    }
                    if (!string.IsNullOrEmpty(deptIds))
                    {
                        string departmentSql = "SELECT userid FROM crm_sys_account_userinfo_relate WHERE recstatus = 1 AND deptid IN (SELECT deptid FROM crm_func_department_tree('{0}', 1))  ";
                        ArrayList ar = new ArrayList();
                        foreach (var depid in deptIds.Split(','))
                        {
                            ar.Add(string.Format(departmentSql, depid));
                        }
                        departmentSql = " ( " + string.Join(" UNION ALL ", ar.ToArray()) + " ) ";
                        var strSql = string.Format(@" AND  ( NOT EXISTS(SELECT * FROM ({0} ) AS arr INNER JOIN (SELECT splitable.regexp_split_to_table::int4 as splitid FROM  (select * FROM  regexp_split_to_table({1}::text,',')) as splitable ) AS tmp ON tmp.splitid=arr.userid))", departmentSql, fieldName);
                        sb.Append(strSql);
                    }
                    sb.Append(")");
                    ruleSql = sb.ToString();
                    break;
                case "=":
                    ruleSql = string.Format(@"(COALESCE({0},'0')::text {1} {2}::text)", fieldName, Condition.Equal.GetSqlOperate(), dataValue);
                    break;
                case "!=":
                    ruleSql = string.Format(@"(COALESCE({0},'0')::text {1} {2}::text)", fieldName, Condition.NotEqual.GetSqlOperate(), dataValue);
                    break;
            }

            return ruleSql;

        }

        public OutputResult<object> GetRule(GetRuleInfoModel entityModel, int userId)
        {
            Guid ruleid;
            Guid.TryParse(entityModel.RuleId, out ruleid);
            return new OutputResult<object>(_ruleRepository.GetRule(ruleid, userId));
        }

        public OutputResult<object> RoleRuleInfoQuery(RoleRuleModel entityModel, int userId)
        {
            var entity = mapper.Map<RoleRuleModel, RoleRuleMapper>(entityModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            var infoList = _ruleRepository.RoleRuleInfoQuery(entity.roleid, entity.entityid, userId);
            var obj = infoList.GroupBy(t => new
            {
                t.RuleId,
                t.RuleName,
                t.RuleSet,
            }).Select(group => new RoleRuleInfoModel
            {
                RuleId = group.Key.RuleId,
                RuleName = group.Key.RuleName,
                RuleItems = group.Select(t => new RuleItemInfoModel
                {
                    ItemId = t.ItemId,
                    ItemName = t.ItemName,
                    FieldId = t.FieldId,
                    Operate = t.Operate,
                    UseType = t.UseType,
                    RuleData = t.RuleData,
                    RuleType = t.RuleType
                }).ToList(),
                RuleSet = new RuleSetInfoModel
                {
                    RuleSet = group.Key.RuleSet
                }
            }).ToList();
            return new OutputResult<object>(obj);
        }

        public OutputResult<object> WorkFlowRuleInfoQuery(FlowRuleModel entityModel, int userId)
        {
            var entity = new FlowRuleMapper
            {
                ruleid= entityModel.RuleId,
                flowid= entityModel.FlowId
            };
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            var infoList = _ruleRepository.WorkFlowRuleInfoQuery(entity.flowid, userId);
            var obj = infoList.GroupBy(t => new
            {
                t.RuleId,
                t.RuleName,
                t.RuleSet,
            }).Select(group => new RoleRuleInfoModel
            {
                RuleId = group.Key.RuleId,
                RuleName = group.Key.RuleName,
                RuleItems = group.Select(t => new RuleItemInfoModel
                {
                    ItemId = t.ItemId,
                    ItemName = t.ItemName,
                    FieldId = t.FieldId,
                    Operate = t.Operate,
                    UseType = t.UseType,
                    RuleData = t.RuleData,
                    RuleType = t.RuleType
                }).ToList(),
                RuleSet = new RuleSetInfoModel
                {
                    RuleSet = group.Key.RuleSet
                }
            }).ToList();
            return new OutputResult<object>(obj);
        }

            public OutputResult<object> DynamicRuleInfoQuery(DynamicRuleModel entityModel, int userId)
        {
            var entity = mapper.Map<DynamicRuleModel, DynamicRuleMapper>(entityModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            var infoList = _ruleRepository.DynamicRuleInfoQuery(entity.entityid, userId);
            var obj = infoList.GroupBy(t => new
            {
                t.RuleId,
                t.RuleName,
                t.RuleSet,
            }).Select(group => new RoleRuleInfoModel
            {
                RuleId = group.Key.RuleId,
                RuleName = group.Key.RuleName,
                RuleItems = group.Select(t => new RuleItemInfoModel
                {
                    ItemId = t.ItemId,
                    ItemName = t.ItemName,
                    FieldId = t.FieldId,
                    Operate = t.Operate,
                    UseType = t.UseType,
                    RuleData = t.RuleData,
                    RuleType = t.RuleType
                }).ToList(),
                RuleSet = new RuleSetInfoModel
                {
                    RuleSet = group.Key.RuleSet
                }
            }).ToList();
            return new OutputResult<object>(obj);
        }
        //public OutputResult<object> SaveRoleRule(RoleRuleModel entityModel, int userId)
        //{
        //    var fields = _entityProRepository.FieldQuery(entityModel.EntityId, userId);

        //    foreach (var entity in entityModel.RuleItems)
        //    {
        //        switch (entity.RuleType)
        //        {
        //            case 0:
        //            case 1:
        //                {
        //                    var entityField = fields.SingleOrDefault(t => t.FieldId == entity.FieldId);
        //                    if (entityField.ControlType != entity.ControlType) throw new Exception("配置字段类型不匹配");
        //                    entity.RuleSql = TranslateRuleConditionSql(entity.Operate, entityField.FieldName, entity.ControlType, entity.RuleType, entity.RuleData);
        //                    break;
        //                }
        //            case 2:
        //                {
        //                    var data = JObject.Parse(entity.RuleData);
        //                    entity.RuleSql = string.Format(@"()", data["dataval"].ToString());
        //                    break;
        //                }
        //            default: throw new Exception("尚未实现的规则类型");
        //        }
        //        entity.ItemId = Guid.NewGuid().ToString();
        //        entity.Relation.ItemId = entity.ItemId;
        //    }
        //    entityModel.RuleSet.RuleFormat = TranslateRuleSet(entityModel.RuleSet.RuleSet);
        //    var mapperEntity = mapper.Map<RoleRuleModel, RoleRuleMapper>(entityModel);
        //    string ruleId = mapperEntity.ruleid;
        //    string ruleJson = JsonConvert.SerializeObject(mapperEntity);
        //    string ruleItemJson = JsonConvert.SerializeObject(mapperEntity.RuleItems);
        //    string ruleItemRel = JsonConvert.SerializeObject(mapperEntity.RuleItems.Select(t => t.Relation));
        //    string ruleSet = JsonConvert.SerializeObject(mapperEntity.RuleSet);
        //    return HandleResult(_ruleRepository.SaveRoleRule(ruleId, ruleJson, ruleItemJson, ruleSet, ruleItemRel, userId));
        //}

        private string TranslateRuleSet(string ruleSet, ref List<RuleItemModel> rel)
        {

            string[] splitSet = ruleSet.Split(' ');
            int indexPara = 0;
            foreach (var tmp in splitSet)
            {
                if (!tmp.Contains("$")) continue;
                ruleSet = ruleSet.Replace(tmp, "%s");
                string paraIndex = tmp.Replace("$", "");
                if (Int32.TryParse(paraIndex, out indexPara))
                {
                    if (indexPara > rel.Count) throw new Exception("规则明细索引溢出");
                    if (indexPara < 0) throw new Exception("规则明细索引不能为0");
                    rel[indexPara - 1].Relation.ParamIndex = indexPara - 1;
                }
                else
                {
                    throw new Exception("规则参数格式异常");
                }
            }
            IncreaseDataVersion(DataVersionType.EntityData);
            return ruleSet;
        }
        /// <summary>
        /// 处理不同的操作符
        /// </summary>
        /// <param name="operate"></param>
        /// <param name="fieldName"></param>
        /// <param name="strData"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        private string TranslateRuleConditionSql(string operate, int ruleType, string strData, int userId, EntityFieldProMapper entityField = null,string prestring=null)
        {
            string fieldName = string.Empty;
            int controlType = -1;

            if (entityField != null)
            {
                if(string.IsNullOrEmpty(prestring))
                {
                    fieldName = entityField.FieldName;
                }
                else fieldName = prestring + entityField.FieldName;

                controlType = entityField.ControlType;
            }

            string strSql = string.Empty;
            var data = JObject.Parse(strData);
            string dataValue = string.Empty;
            operate = operate.ToLower();
            if ((int)DynamicProtocolControlType.Text == controlType || (int)DynamicProtocolControlType.TextArea == controlType || (int)DynamicProtocolControlType.PhoneNum == controlType || (int)DynamicProtocolControlType.EmailAddr == controlType || (int)DynamicProtocolControlType.Telephone == controlType || (int)DynamicProtocolControlType.RecId == controlType || (int)DynamicProtocolControlType.RecName == controlType || (int)DynamicProtocolControlType.RecType == controlType)
            {
                dataValue = data["dataVal"].ToString();
                switch (operate)
                {
                    case "in":
                        strSql = string.Format(@"({0} {1} ('{2}'))", fieldName, Condition.In.GetSqlOperate(), dataValue);
                        break;
                    case "not in":
                        strSql = string.Format(@"({0} {1} ('{2}'))", fieldName, Condition.NotIn.GetSqlOperate(), dataValue);
                        break;
                    case "=":
                        strSql = string.Format(@"({0} {1} '{2}')", fieldName, Condition.Equal.GetSqlOperate(), dataValue);
                        break;
                    case "!=":
                        strSql = string.Format(@"({0} {1} '{2}')", fieldName, Condition.NotEqual.GetSqlOperate(), dataValue);
                        break;
                    case "ilike":
                        strSql = string.Format(@"({0} {1} '%{2}%')", fieldName, Condition.Like.GetSqlOperate(), dataValue);
                        break;
                    case "!*"://空
                        strSql = string.Format(@"({0} {1} )", fieldName, Condition.IsNull.GetSqlOperate());
                        break;
                    case "*"://非空
                        strSql = string.Format(@"({0} {1} )", fieldName, Condition.IsNotNull.GetSqlOperate());
                        break;
                }
            }
            else if ((int)DynamicProtocolControlType.NumberInt == controlType || (int)DynamicProtocolControlType.NumberDecimal == controlType || (int)DynamicProtocolControlType.RecAudits == controlType || (int)DynamicProtocolControlType.RecStatus == controlType)
            {
                dataValue = data["dataVal"].ToString();
                string[] splitVal = null;
                if (dataValue.Contains(':'))
                {
                    splitVal = dataValue.Split(':');
                    foreach (var tmp in splitVal)
                    {
                        if (!CommonHelper.IsMatchNumber(dataValue))
                            throw new Exception("参数类型出错");
                    }
                }
                switch (operate)
                {
                    case "in":
                        strSql = string.Format(@"({0} {1} ({2}))", fieldName, Condition.In.GetSqlOperate(), dataValue);
                        break;
                    case "not in":
                        strSql = string.Format(@"({0} {1} ({2}))", fieldName, Condition.NotIn.GetSqlOperate(), dataValue);
                        break;
                    case "=":
                        strSql = string.Format(@"({0} {1} {2})", fieldName, Condition.Equal.GetSqlOperate(), dataValue);
                        break;
                    case "!=":
                        strSql = string.Format(@"({0} {1} {2})", fieldName, Condition.NotEqual.GetSqlOperate(), dataValue);
                        break;
                    case ">=":
                        strSql = string.Format(@"({0} {1} {2})", fieldName, Condition.GreaterEqual.GetSqlOperate(), dataValue);
                        break;
                    case "<=":
                        strSql = string.Format(@"({0} {1} {2})", fieldName, Condition.LessEqual.GetSqlOperate(), dataValue);
                        break;
                    case ">":
                        strSql = string.Format(@"({0} {1} {2})", fieldName, Condition.Greater.GetSqlOperate(), dataValue);
                        break;
                    case "<":
                        strSql = string.Format(@"({0} {1} {2})", fieldName, Condition.Less.GetSqlOperate(), dataValue);
                        break;
                    case "!*"://空
                        strSql = string.Format(@"({0} {1} )", fieldName, Condition.IsNull.GetSqlOperate());
                        break;
                    case "*"://非空
                        strSql = string.Format(@"({0} {1} )", fieldName, Condition.IsNotNull.GetSqlOperate());
                        break;
                    case "between":
                        if (splitVal == null || splitVal.Length < 2)
                            throw new Exception("规则明细数字格式异常");
                        strSql = string.Format(@"({0} {1} {2} and {0} {3} {4})", fieldName, Condition.GreaterEqual.GetSqlOperate(), splitVal[0], Condition.LessEqual.GetSqlOperate(), splitVal[1]);
                        break;
                }
            }
            #region 合并到选人控件的逻辑中
            //else if ((int)DynamicProtocolControlType.RecCreator == controlType || (int)DynamicProtocolControlType.RecUpdator == controlType || (int)DynamicProtocolControlType.RecManager == controlType)
            //{
            //    dataValue = data["dataVal"].ToString();
            //    StringBuilder sb = new StringBuilder();
            //    switch (operate)
            //    {
            //        case "in":
            //            strSql = string.Format(@"({0} {1} ({2})) ", fieldName, Condition.In.GetSqlOperate(), dataValue);
            //            break;
            //        case "not in":
            //            strSql = string.Format(@"({0} {1} ({2})) ", fieldName, Condition.NotIn.GetSqlOperate(), dataValue);
            //            break;
            //        case "=":
            //            strSql = string.Format(@"({0} {1} {2})", fieldName, Condition.Equal.GetSqlOperate(), dataValue);
            //            break;
            //        case "!=":
            //            strSql = string.Format(@"({0} {1} {2})", fieldName, Condition.NotEqual.GetSqlOperate(), dataValue);
            //            break;
            //    }
            //} 
            #endregion
            else if ((int)DynamicProtocolControlType.TimeDate == controlType || (int)DynamicProtocolControlType.TimeStamp == controlType || (int)DynamicProtocolControlType.RecUpdated == controlType || (int)DynamicProtocolControlType.RecCreated == controlType || (int)DynamicProtocolControlType.RecOnlive == controlType)
            {
                string date = string.Empty;
                switch (operate)
                {
                    case "between":
                        string startDate = data["startdate"].ToString();
                        string endDate = data["enddate"].ToString();
                        if (!CommonHelper.IsMatchDateTime(startDate))
                            throw new Exception("参数类型出错");
                        if (!CommonHelper.IsMatchDateTime(endDate))
                            throw new Exception("参数类型出错");
                        strSql = string.Format(@"({0} between '{1}' and '{2}')", fieldName, operate, startDate, endDate);
                        break;
                    case "=":
                        date = data["dataVal"].ToString();
                        if (!CommonHelper.IsMatchDateTime(date))
                            throw new Exception("参数类型出错");
                        strSql = string.Format(@"({0}  {1}  '{2}')", fieldName, Condition.GreaterEqual.GetSqlOperate(), date);
                        break;
                    case "!=":
                        date = data["dataVal"].ToString();
                        if (!CommonHelper.IsMatchDateTime(date))
                            throw new Exception("参数类型出错");
                        strSql = string.Format(@"({0}  {1}  '{2}')", fieldName, Condition.GreaterEqual.GetSqlOperate(), date);
                        break;
                    case ">=":
                        date = data["dataVal"].ToString();
                        if (!CommonHelper.IsMatchDateTime(date))
                            throw new Exception("参数类型出错");
                        strSql = string.Format(@"({0}  {1}  '{2}')", fieldName, Condition.GreaterEqual.GetSqlOperate(), date);
                        break;
                    case "<=":
                        date = data["dataVal"].ToString();
                        if (!CommonHelper.IsMatchDateTime(date))
                            throw new Exception("参数类型出错");
                        strSql = string.Format(@"({0}  {1}  '{2}')", fieldName, Condition.LessEqual.GetSqlOperate(), date);
                        break;
                    case ">":
                        date = data["dataVal"].ToString();
                        if (!CommonHelper.IsMatchDateTime(date))
                            throw new Exception("参数类型出错");
                        strSql = string.Format(@"({0}  {1}  '{2}')", fieldName, Condition.Greater.GetSqlOperate(), date);
                        break;
                    case "<":
                        date = data["dataVal"].ToString();
                        if (!CommonHelper.IsMatchDateTime(date))
                            throw new Exception("参数类型出错");
                        strSql = string.Format(@"({0}  {1}  '{2}')", fieldName, Condition.Less.GetSqlOperate(), date);
                        break;
                    case ">=now":
                        string dayGreaterEqual = data["dataVal"].ToString();
                        int days = 0;
                        if (!Int32.TryParse(dayGreaterEqual, out days))
                            throw new Exception("参数类型出错");
                        strSql = string.Format(@"date_part('day', {0}::timestamp - now()) {1} {2}", fieldName, Condition.GreaterEqual.GetSqlOperate(), days);
                        break;
                    case "<=now":
                        string dayLessEqual = data["dataVal"].ToString();
                        int daysLessEqual = 0;
                        if (!Int32.TryParse(dayLessEqual, out daysLessEqual))
                            throw new Exception("规则明细格式化时间出错");
                        strSql = string.Format(@"date_part('day', {0}::timestamp - now()) {1} {2}", fieldName, Condition.LessEqual.GetSqlOperate(), daysLessEqual);
                        break;
                }
            }
            else if ((int)DynamicProtocolControlType.DataSourceSingle == controlType || (int)DynamicProtocolControlType.DataSourceMulti == controlType)
            {
                dataValue = data["dataVal"].ToString();
                dataValue = "'" + dataValue.Replace(",", "','") + "'";
                switch (operate)
                {
                    case "in":
                        strSql = string.Format(@"(({0}->>'id') {1} ({2}))", fieldName, Condition.In.GetSqlOperate(), dataValue);
                        break;
                    case "not in":
                        strSql = string.Format(@"(({0}->>'id') {1} ({2}))", fieldName, Condition.NotIn.GetSqlOperate(), dataValue);
                        break;
                    default:
                        throw new Exception("规则明细字段缺少该操作符");
                }
            }
            else if ((int)DynamicProtocolControlType.Department == controlType || (int)DynamicProtocolControlType.Product == controlType || (int)DynamicProtocolControlType.ProductSet == controlType)
            {
                dataValue = data["dataVal"].ToString();
                dataValue = "'" + dataValue.Replace(",", "','") + "'";
                switch (operate)
                {
                    case "in":
                        strSql = string.Format(@"({0} {1} ({2}))", fieldName, Condition.In.GetSqlOperate(), dataValue);
                        break;
                    case "not in":
                        strSql = string.Format(@"({0} {1} ({2}))", fieldName, Condition.NotIn.GetSqlOperate(), dataValue);
                        break;
                    default:
                        throw new Exception("规则明细字段缺少该操作符");
                }
            }
            else if ((int)DynamicProtocolControlType.SelectMulti == controlType)
            {
                dataValue = data["dataVal"].ToString();
                switch (operate)
                {
                    case "in":
                        strSql = string.Format(@"(EXISTS( SELECT * FROM (select regexp_split_to_table({0},',')::int4 AS sel ) AS t WHERE t.sel {1} ({2})))", fieldName, Condition.In.GetSqlOperate(), dataValue);
                        break;
                    case "not in":
                        strSql = string.Format(@" (NOT EXISTS ( SELECT * FROM (SELECT UNNEST(ARRAY[{0}]) ID) AS arr INNER JOIN (SELECT splitable.regexp_split_to_table::int4 as splitid FROM  (select * FROM  regexp_split_to_table({1},',')) as splitable ) AS tmp ON tmp.splitid=arr.id))", dataValue, fieldName);
                        break;
                    default:
                        throw new Exception("规则明细字段缺少该操作符");
                }
            }
            else if ((int)DynamicProtocolControlType.SelectSingle == controlType)
            {
                dataValue = data["dataVal"].ToString();
                switch (operate)
                {
                    case "in":
                        strSql = string.Format(@"(EXISTS( SELECT * FROM (select regexp_split_to_table({0}::text,',')::int4 AS sel ) AS t WHERE t.sel {1} ({2})))", fieldName, Condition.In.GetSqlOperate(), dataValue);
                        break;
                    case "not in":
                        strSql = string.Format(@" (NOT EXISTS ( SELECT * FROM (SELECT UNNEST(ARRAY[{0}]) ID) AS arr INNER JOIN (SELECT splitable.regexp_split_to_table::int4 as splitid FROM  (select * FROM  regexp_split_to_table({1}::text,',')) as splitable ) AS tmp ON tmp.splitid=arr.id))", dataValue, fieldName);
                        break;
                    default:
                        throw new Exception("规则明细字段缺少该操作符");
                }
            }
            else if ((int)DynamicProtocolControlType.FileAttach == controlType || (int)DynamicProtocolControlType.TakePhoto == controlType)
            {
                dataValue = data["dataVal"].ToString();
                switch (operate)
                {
                    case "=":
                        strSql = string.Format(@"({0} is {1})", fieldName, dataValue);
                        break;
                    default:
                        throw new Exception("规则明细字段缺少该操作符");
                }
            }
            else if ((int)DynamicProtocolControlType.Address == controlType || (int)DynamicProtocolControlType.Location == controlType)
            {
                dataValue = data["dataVal"].ToString();
                switch (operate)
                {
                    case "ilike":
                        strSql = string.Format(@"({0}->>'address' {1} '%{2}%')", fieldName, Condition.Like.GetSqlOperate(), dataValue);
                        break;
                    case "=":
                        strSql = string.Format(@"({0}->>'address' {1} '{2}')", fieldName, Condition.Equal.GetSqlOperate(), dataValue);
                        break;
                    default:
                        throw new Exception("规则明细字段缺少该操作符");
                }
            }

            else if (((int)DynamicProtocolControlType.PersonSelectMulti == controlType || (int)DynamicProtocolControlType.PersonSelectSingle == controlType)
                || ((int)DynamicProtocolControlType.RecCreator == controlType || (int)DynamicProtocolControlType.RecUpdator == controlType || (int)DynamicProtocolControlType.RecManager == controlType))
            {
                dataValue = data["dataVal"].ToString();
                string userIds = string.Empty;
                string deptIds = string.Empty;
                JToken value;
                if (data.TryGetValue("users", out value))
                {
                    userIds = value.ToString();
                }
                if (data.TryGetValue("deptids", out value))
                {
                    deptIds = value.ToString();
                }

                StringBuilder sb = new StringBuilder();
                switch (operate)
                {
                    case "in":
                        sb.Append("(");
                        if (!string.IsNullOrEmpty(dataValue) && dataValue != "{}")
                        {
                            strSql = string.Format(@" (EXISTS( SELECT * FROM (select regexp_split_to_table({0}::text,',')::int4 AS sel ) AS t WHERE t.sel {1} ({2}))) ", fieldName, Condition.In.GetSqlOperate(), dataValue);
                            sb.Append(strSql);
                        }
                        else
                        {
                            strSql = "1=1";
                            sb.Append(strSql);
                        }
                        if (!string.IsNullOrEmpty(userIds))
                        {
                            strSql = string.Format(@" AND (EXISTS( SELECT * FROM (select regexp_split_to_table({0}::text,',')::int4 AS sel ) AS t WHERE t.sel {1} ({2}))) ", fieldName, Condition.In.GetSqlOperate(), userIds);
                            sb.Append(strSql);
                        }
                        if (!string.IsNullOrEmpty(deptIds))
                        {
                            string departmentSql = "SELECT userid FROM crm_sys_account_userinfo_relate WHERE recstatus = 1 AND deptid IN (SELECT deptid FROM crm_func_department_tree('{0}', 1))  ";
                            ArrayList ar = new ArrayList();
                            foreach (var depid in deptIds.Split(','))
                            {
                                ar.Add(string.Format(departmentSql, depid));
                            }
                            departmentSql = " ( " + string.Join(" UNION ALL ", ar.ToArray()) + " ) ";
                            strSql = string.Format(@" AND (EXISTS( SELECT * FROM (select regexp_split_to_table({0}::text,',')::int4 AS sel ) AS t WHERE t.sel {1} ({2})))", fieldName, Condition.In.GetSqlOperate(), departmentSql);
                            sb.Append(strSql);
                        }
                        sb.Append(")");
                        strSql = sb.ToString();
                        break;
                    case "not in":
                        sb.Append("(");
                        if (!string.IsNullOrEmpty(dataValue) && dataValue != "{}")
                        {
                            if (dataValue == "{currentUser}")
                            {
                                strSql = string.Format(@"  (NOT EXISTS ( SELECT * FROM (SELECT UNNEST(ARRAY[{0}]) userid) AS arr INNER JOIN (SELECT splitable.regexp_split_to_table::int4 as splitid FROM  (select * FROM  regexp_split_to_table({1}::text,',')) as splitable ) AS tmp ON tmp.splitid=arr.userid))", dataValue, fieldName);
                            }
                            else if (dataValue == "{currentDepartment}" || dataValue == "{subDepartment}")
                            {
                                strSql = string.Format(@"  (NOT EXISTS ( SELECT * FROM ({0}) AS arr INNER JOIN (SELECT splitable.regexp_split_to_table::int4 as splitid FROM  (select * FROM  regexp_split_to_table({1}::text,',')) as splitable ) AS tmp ON tmp.splitid=arr.userid))", dataValue, fieldName);
                            }
                            sb.Append(strSql);
                        }
                        else
                        {
                            strSql = "1=1";
                            sb.Append(strSql);
                        }
                        if (!string.IsNullOrEmpty(userIds))
                        {
                            strSql = string.Format(@" AND (NOT EXISTS ( SELECT * FROM (SELECT UNNEST(ARRAY[{0}]) userid) AS arr INNER JOIN (SELECT splitable.regexp_split_to_table::int4 as splitid FROM  (select * FROM  regexp_split_to_table({1}::text,',')) as splitable ) AS tmp ON tmp.splitid=arr.userid))", userIds, fieldName);
                            sb.Append(strSql);
                        }
                        if (!string.IsNullOrEmpty(deptIds))
                        {
                            string departmentSql = "SELECT userid FROM crm_sys_account_userinfo_relate WHERE recstatus = 1 AND deptid IN (SELECT deptid FROM crm_func_department_tree('{0}', 1))  ";
                            ArrayList ar = new ArrayList();
                            foreach (var depid in deptIds.Split(','))
                            {
                                ar.Add(string.Format(departmentSql, depid));
                            }
                            departmentSql = " ( " + string.Join(" UNION ALL ", ar.ToArray()) + " ) ";
                            strSql = string.Format(@" AND  ( NOT EXISTS(SELECT * FROM ({0} ) AS arr INNER JOIN (SELECT splitable.regexp_split_to_table::int4 as splitid FROM  (select * FROM  regexp_split_to_table({1}::text,',')) as splitable ) AS tmp ON tmp.splitid=arr.userid))", departmentSql, fieldName);
                            sb.Append(strSql);
                        }
                        sb.Append(")");
                        strSql = sb.ToString();
                        break;
                    case "=":
                        strSql = string.Format(@"(COALESCE({0},'0')::text {1} {2}::text)", fieldName, Condition.Equal.GetSqlOperate(), dataValue);
                        break;
                    case "!=":
                        strSql = string.Format(@"(COALESCE({0},'0')::text {1} {2}::text)", fieldName, Condition.NotEqual.GetSqlOperate(), dataValue);
                        break;
                }
            }

            else if ((int)DynamicProtocolControlType.QuoteControl == controlType)
            {
                if (entityField == null) throw new Exception("缺少字段信息");

                dataValue = data["dataVal"].ToString();

                switch (operate)
                {
                    case "=":
                        if (entityField.FieldName == "deptgroup")
                        {
                            strSql = string.Format("crm_func_entity_protocol_format_belongdepartment(e.recmanager) = '{0}'", dataValue);
                        }
                        else if (entityField.FieldName == "predeptgroup")
                        {
                            strSql = string.Format("crm_func_entity_protocol_format_predepartment(e.recmanager) = '{0}'", dataValue);
                        }
                        else
                        {
                            strSql = string.Format("{0} = '{1}'", tryParseFieldString(entityField), dataValue);
                        }
                        break;

                    case "ilike":
                        if (entityField.FieldName == "deptgroup")
                        {
                            strSql = string.Format("crm_func_entity_protocol_format_belongdepartment(e.recmanager) ilike '%{0}%'", dataValue);
                        }
                        else if (entityField.FieldName == "predeptgroup")
                        {
                            strSql = string.Format("crm_func_entity_protocol_format_predepartment(e.recmanager) ilike  '%{0}%'", dataValue);
                        }
                        else
                        {
                            strSql = string.Format("{0} ilike '%{1}%'", tryParseFieldString(entityField), dataValue);
                        }
                        break;
                }

            }
            return strSql;
        }

        private static string tryParseFieldString(EntityFieldProMapper field)
        {
            if ((DynamicProtocolControlType)field.ControlType == DynamicProtocolControlType.QuoteControl)
            {
                if (field.FieldName == "deptgroup")
                {
                    return string.Format("crm_func_entity_protocol_format_belongdepartment(e.recmanager) ");
                }
                else if (field.FieldName == "predeptgroup")
                {
                    return string.Format("crm_func_entity_protocol_format_predepartment(e.recmanager) ");
                }
                else
                {
                    return string.Format("crm_func_entity_protocol_format_quote_control(row_to_json(e),'{0}','{1}') ", field.EntityId, field.FieldId);
                }
            }
            else if ((DynamicProtocolControlType)field.ControlType == DynamicProtocolControlType.RecCreator)
            {
                return string.Format("crm_func_entity_protocol_format_userinfo(e.{0}) ", field.FieldName);
            }
            else if ((DynamicProtocolControlType)field.ControlType == DynamicProtocolControlType.SalesStage)
            {
                return string.Format("crm_func_entity_protocol_format_salesstage(e.{0}) ", field.FieldName);
            }
            else if ((DynamicProtocolControlType)field.ControlType == DynamicProtocolControlType.RecUpdator)
            {
                return string.Format("crm_func_entity_protocol_format_userinfo(e.{0}) ", field.FieldName);
            }
            else if ((DynamicProtocolControlType)field.ControlType == DynamicProtocolControlType.RecManager)
            {
                return string.Format("crm_func_entity_protocol_format_userinfo(e.{0}) ", field.FieldName);
            }
            else if ((DynamicProtocolControlType)field.ControlType == DynamicProtocolControlType.DataSourceSingle)
            {
                return string.Format("crm_func_entity_protocol_format_ds(e.{0}) ", field.FieldName);
            }
            else if ((DynamicProtocolControlType)field.ControlType == DynamicProtocolControlType.PersonSelectSingle)
            {
                return string.Format("crm_func_entity_protocol_format_userinfo_multi(e.{0}) ", field.FieldName);
            }
            else if ((DynamicProtocolControlType)field.ControlType == DynamicProtocolControlType.PersonSelectMulti)
            {
                return string.Format("crm_func_entity_protocol_format_userinfo_multi(e.{0}) ", field.FieldName);
            }
            else if ((DynamicProtocolControlType)field.ControlType == DynamicProtocolControlType.RecStatus)
            {
                return string.Format("crm_func_entity_protocol_format_recstatus(e.{0}) ", field.FieldName);
            }
            else if ((DynamicProtocolControlType)field.ControlType == DynamicProtocolControlType.TimeDate)
            {
                return string.Format("crm_func_entity_protocol_format_time(e.{0},'YYYY-MM-DD') ", field.FieldName);
            }
            else if ((DynamicProtocolControlType)field.ControlType == DynamicProtocolControlType.TimeStamp)
            {
                return string.Format("crm_func_entity_protocol_format_time(e.{0},'YYYY-MM-DD HH24:MI:SS') ", field.FieldName);
            }
            else if ((DynamicProtocolControlType)field.ControlType == DynamicProtocolControlType.RecCreated)
            {
                return string.Format("crm_func_entity_protocol_format_time(e.{0},'YYYY-MM-DD HH24:MI:SS') ", field.FieldName);
            }
            else if ((DynamicProtocolControlType)field.ControlType == DynamicProtocolControlType.RecUpdated)
            {
                return string.Format("crm_func_entity_protocol_format_time(e.{0},'YYYY-MM-DD HH24:MI:SS') ", field.FieldName);
            }
            else if ((DynamicProtocolControlType)field.ControlType == DynamicProtocolControlType.Department)
            {
                return string.Format("crm_func_entity_protocol_format_dept_multi(e.{0}::TEXT) ", field.FieldName);
            }
            else if ((DynamicProtocolControlType)field.ControlType == DynamicProtocolControlType.Address)
            {
                return string.Format("crm_func_entity_protocol_format_address(e.{0}) ", field.FieldName);
            }
            else if ((DynamicProtocolControlType)field.ControlType == DynamicProtocolControlType.Location)
            {
                return string.Format("crm_func_entity_protocol_format_address(e.{0}) ", field.FieldName);
            }
            else if ((DynamicProtocolControlType)field.ControlType == DynamicProtocolControlType.RecType)
            {
                return string.Format("crm_func_entity_protocol_format_rectype(e.{0}) ", field.FieldName);
            }
            else if ((DynamicProtocolControlType)field.ControlType == DynamicProtocolControlType.Product)
            {
                return string.Format("crm_func_entity_protocol_format_product_multi(e.{0}) ", field.FieldName);
            }

            else if ((DynamicProtocolControlType)field.ControlType == DynamicProtocolControlType.ProductSet)
            {
                return string.Format("crm_func_entity_protocol_format_productserial_multi(e.{0}) ", field.FieldName);
            }
            else if ((DynamicProtocolControlType)field.ControlType == DynamicProtocolControlType.SelectSingle || (DynamicProtocolControlType)field.ControlType == DynamicProtocolControlType.SelectMulti)
            {
                if (!string.IsNullOrEmpty(field.FieldConfig))
                {
                    JObject jo = JObject.Parse(field.FieldConfig);
                    if (jo["dataSource"] == null) throw new Exception("字段FieldConfig异常");
                    jo = JObject.Parse(jo["dataSource"].ToString());
                    int dataId = Convert.ToInt32(jo["sourceId"].ToString());
                    return string.Format("crm_func_entity_protocol_format_dictionary({0},e.{1}::text) ", dataId, field.FieldName);
                }

                return field.FieldName;
            }
            else if ((DynamicProtocolControlType)field.ControlType == DynamicProtocolControlType.AreaRegion)
            {
                return string.Format("crm_func_entity_protocol_format_region(e.{0}) ", field.FieldName);
            }
            return string.Format("e.{0}", field.FieldName);
        }


        /// <summary>
        /// 保存职能模块用到的保存规则方法
        /// </summary>
        /// <param name="entityModel"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public OutputResult<object> SaveRuleForVocation(FunctionRuleAddModel entityModel, int userId)
        {

            List<EntityFieldProMapper> fields = _entityProRepository.FieldQuery(entityModel.EntityId.ToString(), userId);
            foreach (var entity in entityModel.RuleItems)
            {
                switch (entity.RuleType)
                {
                    case 0:
                    case 1:
                        {
                            var entityField = fields.SingleOrDefault(t => t.FieldId == entity.FieldId);
                            if (entityField.ControlType != entity.ControlType) throw new Exception("配置字段类型不匹配");
                            entity.RuleSql = TranslateRuleConditionSql(entity.Operate, entity.RuleType, entity.RuleData, userId, entityField);
                            break;
                        }
                    case 2:
                        {
                            var data = JObject.Parse(entity.RuleData);
                            entity.RuleSql = string.Format(@"({0})", data["dataVal"].ToString());
                            break;
                        }
                    default: throw new Exception("尚未实现的规则类型");
                }
                entity.Relation.ParamIndex = -1;
            }
            var ruleItemList = entityModel.RuleItems.ToList();
            entityModel.RuleSet.RuleFormat = TranslateRuleSet(entityModel.RuleSet.RuleSet, ref ruleItemList);


            var serializerSettings = new JsonSerializerSettings()
            {
                ContractResolver = new LowercaseContractResolver()
            };


            OperateResult result = new OperateResult();

            if (entityModel.Rule.RuleId.HasValue)
            {
                List<FunctionRuleEdit> lst = new List<FunctionRuleEdit>();

                foreach (var tmp in entityModel.RuleItems)
                {
                    tmp.ItemId = Guid.NewGuid().ToString();
                    tmp.Relation.ItemId = tmp.ItemId;
                }
                var crmData = new FunctionRuleEdit()
                {
                    FunctionId = entityModel.FunctionId,
                    VocationId = entityModel.VocationId,
                    Rule = JsonConvert.SerializeObject(entityModel.Rule, serializerSettings),
                    RuleItem = JsonConvert.SerializeObject(entityModel.RuleItems, serializerSettings),
                    RuleSet = JsonConvert.SerializeObject(entityModel.RuleSet, serializerSettings),
                    RuleRelation = JsonConvert.SerializeObject(entityModel.RuleItems.Select(t => t.Relation), serializerSettings)
                };

                lst.Add(crmData);
                if (entityModel.SyncDevice == 1)
                {
                    foreach (var tmp in entityModel.RuleItems)
                    {
                        tmp.ItemId = Guid.NewGuid().ToString();
                        tmp.Relation.ItemId = tmp.ItemId;
                    }

                    var dyn = _vocationRepository.GetFunctionRule(entityModel.VocationId, entityModel.EntityId, entityModel.FunctionId);//查询web和mob对应的节点的 例如web端的新增和
                    if (dyn == null) throw new Exception(DeviceClassic.ToString() + "缺少对应的职能节点");
                    if (dyn.funcid == null) throw new Exception("职能Id不能为空");
                    if (dyn.ruleid == null)
                    {
                        crmData = new FunctionRuleEdit()
                        {
                            FunctionId = dyn.funcid,
                            VocationId = entityModel.VocationId,
                            Rule = JsonConvert.SerializeObject(entityModel.Rule, serializerSettings),
                            RuleItem = JsonConvert.SerializeObject(entityModel.RuleItems, serializerSettings),
                            RuleSet = JsonConvert.SerializeObject(entityModel.RuleSet, serializerSettings),
                            RuleRelation = JsonConvert.SerializeObject(entityModel.RuleItems.Select(t => t.Relation), serializerSettings),
                            IsAdd = true
                        };
                    }
                    else
                    {
                        entityModel.Rule.RuleId = dyn.ruleid;
                        crmData = new FunctionRuleEdit()
                        {
                            FunctionId = dyn.funcid,
                            VocationId = entityModel.VocationId,
                            Rule = JsonConvert.SerializeObject(entityModel.Rule, serializerSettings),
                            RuleItem = JsonConvert.SerializeObject(entityModel.RuleItems, serializerSettings),
                            RuleSet = JsonConvert.SerializeObject(entityModel.RuleSet, serializerSettings),
                            RuleRelation = JsonConvert.SerializeObject(entityModel.RuleItems.Select(t => t.Relation), serializerSettings),
                            IsAdd = false
                        };
                    }
                    lst.Add(crmData);
                }
                result = _vocationRepository.EditFunctionRule(lst, userId);

            }
            else
            {
                List<FunctionRuleAdd> lst = new List<FunctionRuleAdd>();

                foreach (var tmp in entityModel.RuleItems)
                {
                    tmp.ItemId = Guid.NewGuid().ToString();
                    tmp.Relation.ItemId = tmp.ItemId;
                }
                var crmData = new FunctionRuleAdd()
                {
                    VocationId = entityModel.VocationId,
                    FunctionId = entityModel.FunctionId,
                    Rule = JsonConvert.SerializeObject(entityModel.Rule, serializerSettings),
                    RuleItem = JsonConvert.SerializeObject(entityModel.RuleItems, serializerSettings),
                    RuleSet = JsonConvert.SerializeObject(entityModel.RuleSet, serializerSettings),
                    RuleRelation = JsonConvert.SerializeObject(entityModel.RuleItems.Select(t => t.Relation), serializerSettings),
                    IsAdd = true
                };
                lst.Add(crmData);
                if (entityModel.SyncDevice == 1)
                {
                    foreach (var tmp in entityModel.RuleItems)
                    {
                        tmp.ItemId = Guid.NewGuid().ToString();
                        tmp.Relation.ItemId = tmp.ItemId;
                    }
                    var dyn = _vocationRepository.GetFunctionRule(entityModel.VocationId, entityModel.EntityId, entityModel.FunctionId);
                    if (dyn == null) throw new Exception(DeviceClassic.ToString() + "缺少对应的职能节点");
                    if (dyn.funcid == null) throw new Exception("职能Id不能为空");
                    if (dyn.ruleid == null)
                    {
                        crmData = new FunctionRuleAdd()
                        {
                            VocationId = entityModel.VocationId,
                            FunctionId = dyn.funcid,
                            Rule = JsonConvert.SerializeObject(entityModel.Rule, serializerSettings),
                            RuleItem = JsonConvert.SerializeObject(entityModel.RuleItems, serializerSettings),
                            RuleSet = JsonConvert.SerializeObject(entityModel.RuleSet, serializerSettings),
                            RuleRelation = JsonConvert.SerializeObject(entityModel.RuleItems.Select(t => t.Relation), serializerSettings),
                            IsAdd = true
                        };
                    }
                    else
                    {
                        entityModel.Rule.RuleId = dyn.ruleid;
                        crmData = new FunctionRuleAdd()
                        {
                            VocationId = entityModel.VocationId,
                            FunctionId = dyn.funcid,
                            Rule = JsonConvert.SerializeObject(entityModel.Rule, serializerSettings),
                            RuleItem = JsonConvert.SerializeObject(entityModel.RuleItems, serializerSettings),
                            RuleSet = JsonConvert.SerializeObject(entityModel.RuleSet, serializerSettings),
                            RuleRelation = JsonConvert.SerializeObject(entityModel.RuleItems.Select(t => t.Relation), serializerSettings),
                            IsAdd = false
                        };
                    }
                    lst.Add(crmData);
                }
                result = _vocationRepository.AddFunctionRule(lst, userId);
                //return HandleResult(result);
            }
            IncreaseDataVersion(DataVersionType.EntityData);
            return HandleResult(result);


        }



        /// <summary>
        /// 保存销售指标规则
        /// </summary>
        /// <param name="entityModel"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public OutputResult<object> SaveRuleForSalesTargetNorm(SaleTargetNormTypeRuleSaveModel entityModel, int userId)
        {
            if (string.IsNullOrEmpty(entityModel.RoleId))
            {
                entityModel.RoleId = Guid.Empty.ToString();
            }

            var _ruleMapper = new RuleMapper()
            {
                typeid = entityModel.TypeId,
                id = entityModel.Id,
                roleid = entityModel.RoleId,
                menuname = entityModel.MenuName,
                ruleid = entityModel.RuleId,
                rulename = entityModel.RuleName,
                entityid = entityModel.EntityId.ToString(),
                rulesql = entityModel.Rulesql,
            };



            string ruleJson = JsonConvert.SerializeObject(_ruleMapper);


            List<EntityFieldProMapper> fields = _entityProRepository.FieldQuery(entityModel.EntityId.ToString(), userId);
            foreach (var entity in entityModel.RuleItems)
            {
                switch (entity.RuleType)
                {
                    case 0:
                    case 1:
                        {
                            var entityField = fields.SingleOrDefault(t => t.FieldId == entity.FieldId);
                            if (entityField.ControlType != entity.ControlType) throw new Exception("配置字段类型不匹配");
                            entity.RuleSql = TranslateRuleConditionSql(entity.Operate, entity.RuleType, entity.RuleData, userId, entityField);
                            break;
                        }
                    case 2:
                        {
                            var data = JObject.Parse(entity.RuleData);
                            entity.RuleSql = string.Format(@"({0})", data["dataVal"].ToString());
                            break;
                        }
                    default: throw new Exception("尚未实现的规则类型");
                }

                entity.ItemId = Guid.NewGuid().ToString();
                entity.Relation.ItemId = entity.ItemId;
            }

            var ruleItemList = entityModel.RuleItems.ToList();
            entityModel.RuleSet.RuleFormat = TranslateRuleSet(entityModel.RuleSet.RuleSet, ref ruleItemList);
            //entityModel.RuleSet.RuleFormat = TranslateRuleSet(entityModel.RuleSet.RuleSet);


            var serializerSettings = new JsonSerializerSettings()
            {
                ContractResolver = new LowercaseContractResolver()
            };



            var normType = new SalesTargetNormTypeMapper()
            {
                Id = entityModel.NormId,
                Name = entityModel.NormTypeName,
                EntityId = entityModel.EntityId,
                FieldName = entityModel.FieldName,
                CaculateType = entityModel.CalcuteType,
                BizDateFieldName = entityModel.BizDateFieldName
            };


            if (!string.IsNullOrEmpty(entityModel.RuleId))
            {
                var crmData = new SalesTargetNormRuleInsertMapper()
                {
                    Rule = ruleJson, //JsonConvert.SerializeObject(entityModel.Rule, serializerSettings),
                    RuleItem = JsonConvert.SerializeObject(entityModel.RuleItems, serializerSettings),
                    RuleSet = JsonConvert.SerializeObject(entityModel.RuleSet, serializerSettings),
                    RuleRelation = JsonConvert.SerializeObject(entityModel.RuleItems.Select(t => t.Relation), serializerSettings)
                };

                //编辑销售指标规则
                return HandleResult(_salesTargetRepository.EditSalesTargetNormTypeRule(normType, crmData, userId));
            }
            else
            {
                var crmData = new SalesTargetNormRuleInsertMapper()
                {
                    Rule = ruleJson, //JsonConvert.SerializeObject(entityModel.Rule, serializerSettings),
                    RuleItem = JsonConvert.SerializeObject(entityModel.RuleItems, serializerSettings),
                    RuleSet = JsonConvert.SerializeObject(entityModel.RuleSet, serializerSettings),
                    RuleRelation = JsonConvert.SerializeObject(entityModel.RuleItems.Select(t => t.Relation), serializerSettings)
                };

                if (!crmData.IsValid())
                {
                    return HandleValid(crmData);
                }

                //新增销售指标规则
                return HandleResult(_salesTargetRepository.InsertSalesTargetNormTypeRule(normType, crmData, userId));
            }
        }

        public static string SqlRuleFormat(string sql, int userNo)
        {
            sql = sql.Replace("{UserNo}", userNo.ToString());

            sql = sql.Replace("{currentUser}", userNo.ToString());

            sql = sql.Replace("{UserDeptPeople}", "(SELECT userid FROM crm_sys_account_userinfo_relate WHERE recstatus = 1 AND deptid =(SELECT deptid FROM crm_sys_account_userinfo_relate WHERE recstatus = 1 AND userid = _userno LIMIT 1))");

            sql = sql.Replace("{currentDepartment}", "(SELECT userid FROM crm_sys_account_userinfo_relate WHERE recstatus = 1 AND deptid =(SELECT deptid FROM crm_sys_account_userinfo_relate WHERE recstatus = 1 AND userid = _userno LIMIT 1))");

            sql = sql.Replace("{UserDeptPeopleWithSub}", "(SELECT userid FROM crm_sys_account_userinfo_relate WHERE recstatus = 1 AND deptid IN (SELECT deptid FROM crm_func_department_tree((SELECT deptid FROM crm_sys_account_userinfo_relate WHERE recstatus = 1 AND userid = _userno LIMIT 1), 1)))");

            sql = sql.Replace("{subDepartment}", "(SELECT userid FROM crm_sys_account_userinfo_relate WHERE recstatus = 1 AND deptid IN (SELECT deptid FROM crm_func_department_tree((SELECT deptid FROM crm_sys_account_userinfo_relate WHERE recstatus = 1 AND userid = _userno LIMIT 1), 1)) )");
            sql = sql.Replace("{noLeaderDepartment}", @")select userid  from crm_sys_account_userinfo_relate  
                where recstatus= 1 and 
	                deptid not in (
	                select a.deptid
	                from crm_sys_account_userinfo_relate a
				                inner join crm_sys_userinfo b on a.userid= b.userid 
	                where a.recstatus =1  and b.recstatus =1 and b.isleader = 1 
               ) )");

            return sql;
        }

        public DomainModel.Reminder.RuleInsertMapper GetRuleSaveMapper(Models.Reminder.RuleSaveModel entityModel, int userId)
        {

            if (string.IsNullOrEmpty(entityModel.RoleId))
            {
                entityModel.RoleId = Guid.Empty.ToString();
            }

            var _ruleMapper = new RuleMapper()
            {
                typeid = entityModel.TypeId,
                id = entityModel.Id,
                roleid = entityModel.RoleId,
                menuname = entityModel.MenuName,
                ruleid = entityModel.RuleId,
                rulename = entityModel.RuleName,
                entityid = entityModel.EntityId.ToString(),
                rulesql = entityModel.Rulesql,
            };



            string ruleJson = JsonConvert.SerializeObject(_ruleMapper);


            List<EntityFieldProMapper> fields = _entityProRepository.FieldQuery(entityModel.EntityId.ToString(), userId);
            foreach (var entity in entityModel.RuleItems)
            {
                switch (entity.RuleType)
                {
                    case 0:
                    case 1:
                        {
                            var entityField = fields.SingleOrDefault(t => t.FieldId == entity.FieldId);
                            if (entityField.ControlType != entity.ControlType) throw new Exception("配置字段类型不匹配");
                            entity.RuleSql = TranslateRuleConditionSql(entity.Operate, entity.RuleType, entity.RuleData, userId, entityField);
                            break;
                        }
                    case 2:
                        {
                            var data = JObject.Parse(entity.RuleData);
                            entity.RuleSql = string.Format(@"({0})", data["dataVal"].ToString());
                            break;
                        }
                    default: throw new Exception("尚未实现的规则类型");
                }

                entity.ItemId = Guid.NewGuid().ToString();
                entity.Relation.ItemId = entity.ItemId;
            }

            var ruleItemList = entityModel.RuleItems.ToList();
            entityModel.RuleSet.RuleFormat = TranslateRuleSet(entityModel.RuleSet.RuleSet, ref ruleItemList);
            //entityModel.RuleSet.RuleFormat = TranslateRuleSet(entityModel.RuleSet.RuleSet);


            //设置json序列化为小写
            var serializerSettings = new JsonSerializerSettings()
            {
                ContractResolver = new LowercaseContractResolver()
            };



            var ruleData = new DomainModel.Reminder.RuleInsertMapper()
            {
                Rule = ruleJson,
                RuleItem = JsonConvert.SerializeObject(entityModel.RuleItems, serializerSettings),
                RuleSet = JsonConvert.SerializeObject(entityModel.RuleSet, serializerSettings),
                RuleRelation = JsonConvert.SerializeObject(entityModel.RuleItems.Select(t => t.Relation), serializerSettings)

            };

            return ruleData;
        }



        //end of the class
    }




    public class LowercaseContractResolver : DefaultContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            return propertyName.ToLower();
        }
    }


}
