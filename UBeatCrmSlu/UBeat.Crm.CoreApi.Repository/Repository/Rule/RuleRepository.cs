using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using UBeat.Crm.CoreApi.DomainMapper.Rule;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Utility;
using System.Linq;
using UBeat.Crm.CoreApi.DomainModel.Rule;

namespace UBeat.Crm.CoreApi.Repository.Repository.Rule
{
    public class RuleRepository : RepositoryBase, IRuleRepository
    {

        public List<RuleQueryMapper> MenuRuleInfoQuery(string menuId, int userNumber)
        {
            var procName =
                "SELECT crm_func_menu_rule_info(@menuid,@userno)";
            var param = new DynamicParameters();
            param.Add("menuid", menuId);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QueryStoredProcCursor<RuleQueryMapper>(procName, param, CommandType.Text);
            return result;
        }
        public List<RoleRuleQueryMapper> RoleRuleInfoQuery(string roleId, string entityId, int userNumber)
        {
            var procName =
                "SELECT crm_func_role_rule_info(@roleid,@entityid,@userno)";
            var param = new DynamicParameters();
            param.Add("roleid", roleId);
            param.Add("entityid", entityId);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QueryStoredProcCursor<RoleRuleQueryMapper>(procName, param, CommandType.Text);
            return result;
        }
        public List<DynamicRuleQueryMapper> DynamicRuleInfoQuery(string entityId, int userNumber)
        {
            var procName =
                "SELECT crm_func_dynamic_rule_info(@entityid,@userno)";
            var param = new DynamicParameters();
            param.Add("entityid", entityId);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QueryStoredProcCursor<DynamicRuleQueryMapper>(procName, param, CommandType.Text);
            return result;
        }
        public Dictionary<string, List<IDictionary<string, object>>> MenuRuleQuery(string entityId, int userNumber)
        {
            var procName =
                "SELECT crm_func_menu_rule_list(@entityid,@userno)";
            var dataNames = new List<string> { "RuleMenu" };
            var param = new DynamicParameters();
            param.Add("entityid", entityId);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }

        public OperateResult SaveRuleWithoutRelation(string Id, string rule, string ruleItem, string ruleSet,int userId) {
            OperateResult result = new OperateResult();
            string sql = string.Empty;
            var param = new DynamicParameters();
            if (string.IsNullOrEmpty(Id))
            {
                //新建模式
                sql = @"SELECT * FROM crm_func_rule_insert(@rule,@ruleitem,@ruleset,@userno)";
            }
            else {
                //修改模式
                sql = @"SELECT * FROM crm_func_rule_update(@rule,@ruleitem,@ruleset,@userno)";
            }
            param.Add("ruleitem", ruleItem);
            param.Add("ruleset", ruleSet);
            param.Add("userno", userId);
            result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);

            return result;
        }
        public OperateResult SaveRule(string Id, int typeId, string rule, string ruleItem, string ruleSet, string ruleRelation, int userId)
        {
            OperateResult result = new OperateResult();
            string sql = string.Empty;
            var param = new DynamicParameters();
            if (string.IsNullOrEmpty(Id))
            {
                sql = @"
                SELECT * FROM crm_func_rule_add(@isdefaultrule,@typeid,@rule,@ruleitem,@ruleset,@rulerelation,@userno)
            ";
                param.Add("isdefaultrule", 0);//新建的规则默认是非默认规则 默认规则需要手动在数据库添加
            }
            else
            {
                sql = @"
                SELECT * FROM crm_func_rule_edit(@typeid,@rule,@ruleitem,@ruleset,@rulerelation,@userno)
            ";
            }
            param.Add("typeid", typeId);
            param.Add("rule", rule);
            param.Add("ruleitem", ruleItem);
            param.Add("ruleset", ruleSet);
            param.Add("rulerelation", ruleRelation);
            param.Add("userno", userId);
            result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);

            return result;
        }


        public List<RuleDataInfo> GetRule(Guid ruleid, int userNumber, DbTransaction tran = null)
        {
            var executeSql = @" 
                                SELECT r.ruleid::text,r.rulename,r.entityid::text,f.entityid::text AS itemIdEntityId,r.recstatus,i.itemid::text,i.fieldid::text,i.itemname,i.operate,i.usetype,i.ruletype,i.ruledata,s.ruleset,r.rulesql 
                                FROM crm_sys_rule r
                                Left JOIN crm_sys_rule_item_relation ir ON r.ruleid=ir.ruleid 
                                Left JOIN crm_sys_rule_item  i on i.itemid=ir.itemid
                                Left JOIN crm_sys_entity_fields  f on f.fieldid=i.fieldid
                                Left JOIN crm_sys_rule_set  s on s.ruleid=r.ruleid 
                                where r.recstatus=1 and r.ruleid=@ruleid order by i.recorder asc";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("ruleid", ruleid),
            };
           var infoList= ExecuteQuery<RoleRuleQueryMapper>(executeSql, param, tran);
            var obj = infoList.GroupBy(t => new
            {
                t.RuleId,
                t.RuleName,
                t.RuleSet,
                t.RuleSql,
                t.EntityId,
            }).Select(group => new RuleDataInfo
            {
                RuleId = group.Key.RuleId,
                RuleName = group.Key.RuleName,
                Rulesql=group.Key.RuleSql,
                EntityId=group.Key.EntityId,
                RuleItems = group.Select(t => new RuleItemInfo
                {
                    ItemId = t.ItemId,
                    ItemName = t.ItemName,
                    FieldId = t.FieldId,
                    Operate = t.Operate,
                    UseType = t.UseType,
                    RuleData = t.RuleData,
                    RuleType = t.RuleType,
                    EntityId=t.ItemIdEntityId,

                }).ToList(),
                RuleSet = new RuleSetInfo
                {
                    RuleSet = group.Key.RuleSet
                }
            }).ToList();
            return obj;
        }

        public OperateResult DisabledEntityRule(string menuId, int userId)
        {
            OperateResult result = new OperateResult();
            string sql = @"
                SELECT * FROM crm_func_rule_disabled(@menuid,@userno)
            ";
            var param = new DynamicParameters();
            param.Add("menuid", menuId);
            param.Add("userno", userId);
            result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);

            return result;
        }

        /// <summary>
        /// 判断是否有数据权限
        /// </summary>
        /// <param name="tran"></param>
        /// <param name="ruleSql"></param>
        /// <param name="entityid"></param>
        /// <param name="recids"></param>
        /// <returns></returns>
        public bool HasDataAccess(DbTransaction tran, string ruleSql, Guid entityid, List<Guid> recids, string recidFieldName = "recid")
        {
            var entitySql = "SELECT entitytable FROM crm_sys_entity WHERE entityid=@entityid";
            var entitySqlParameters = new List<DbParameter>();
            entitySqlParameters.Add(new NpgsqlParameter("entityid", entityid));
            object entitytableResult = null;
            if (tran == null)
                entitytableResult = DBHelper.ExecuteScalar("", entitySql, entitySqlParameters.ToArray());
            else
                entitytableResult = DBHelper.ExecuteScalar(tran, entitySql, entitySqlParameters.ToArray());
            if (entitytableResult == null || string.IsNullOrEmpty(entitytableResult.ToString()))
            {
                throw new Exception("该实体不存在有效的业务表");
            }
            string entityTableName = entitytableResult.ToString();

            string whereSql = string.Format("{0} AND {1} = ANY(@recids)",( string.IsNullOrEmpty(ruleSql) ? "1=1" : ruleSql), recidFieldName);
            string sql = string.Format("SELECT COUNT(1) FROM {0} AS e WHERE  {1}", entityTableName, whereSql);

            var sqlParameters = new List<DbParameter>();
            sqlParameters.Add(new NpgsqlParameter("recids", recids.ToArray()));
            object result = null;
            if (tran == null)
                result = DBHelper.ExecuteScalar("", sql, sqlParameters.ToArray());
            else result = DBHelper.ExecuteScalar(tran, sql, sqlParameters.ToArray());
            int isAccess = 0;
            if (result != null)
                int.TryParse(result.ToString(), out isAccess);
            return isAccess == recids.Count;
        }


      

    }
}
