using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.DbManage.RuleInfo;
using UBeat.Crm.CoreApi.DomainModel.DbManage.WorkFlow;
using UBeat.Crm.CoreApi.IRepository;

namespace UBeat.Crm.CoreApi.Repository.Repository.DbManage
{
    public class DbRuleRepository : RepositoryBase, IDbRuleRepository
    {
        public List<DbRuleInfo> GetRuleInfoList(List<Guid> ruleids, DbTransaction trans = null)
        {

            var whereSql = string.Empty;
            var sqlParameters = new List<DbParameter>();

            if (ruleids != null && ruleids.Count > 0)
            {
                whereSql = " AND r.ruleid=ANY(@ruleids)";
                sqlParameters.Add(new NpgsqlParameter("ruleids", ruleids));
            }

            var executeSql = string.Format(@"SELECT (row_to_json(r,true)) AS RuleInfo,
                                                    array_to_json(array_agg(row_to_json(ri,true))) AS RuleItems,
                                                    array_to_json(array_agg(row_to_json(rir,true)))  AS RuleItemRelations,
                                                    array_to_json(array_agg(row_to_json(rs,true)))  AS RuleSet
                                             FROM crm_sys_rule r
                                             LEFT JOIN crm_sys_rule_item_relation rir ON rir.ruleid=r.ruleid
                                             LEFT JOIN crm_sys_rule_item ri ON ri.itemid=rir.itemid
                                             LEFT JOIN crm_sys_rule_set rs ON rs.ruleid=r.ruleid
                                             WHERE r.recstatus=1  {0}
                                             GROUP BY r.ruleid ", whereSql);
            var result = ExecuteQuery<DbRuleInfo>(executeSql, sqlParameters.ToArray(), trans);

            return result;
        }

        #region --获取rule相关表的所有数据--
        public List<CrmSysRule> GetAllCrmSysRuleList(DbTransaction trans = null)
        {
            var sqlParameters = new List<DbParameter>();

            var executeSql = @"SELECT * FROM crm_sys_rule ";
            var result = ExecuteQuery<CrmSysRule>(executeSql, sqlParameters.ToArray(), trans);

            return result;
        }
        public List<CrmSysRuleItem> GetAllCrmSysRuleItemList(DbTransaction trans = null)
        {
            var sqlParameters = new List<DbParameter>();

            var executeSql = @"SELECT * FROM crm_sys_rule_item ";
            var result = ExecuteQuery<CrmSysRuleItem>(executeSql, sqlParameters.ToArray(), trans);

            return result;
        }
        public List<CrmSysRuleItemRelation> GetAllCrmSysRuleItemRelationList(DbTransaction trans = null)
        {
            var sqlParameters = new List<DbParameter>();

            var executeSql = @"SELECT * FROM crm_sys_rule_item_relation ";
            var result = ExecuteQuery<CrmSysRuleItemRelation>(executeSql, sqlParameters.ToArray(), trans);

            return result;
        }

        public List<CrmSysRuleSet> GetAllCrmSysRuleSetList(DbTransaction trans = null)
        {
            var sqlParameters = new List<DbParameter>();

            var executeSql = @"SELECT * FROM crm_sys_rule_set ";
            var result = ExecuteQuery<CrmSysRuleSet>(executeSql, sqlParameters.ToArray(), trans);

            return result;
        }
        #endregion

        public void SaveRuleInfoList(List<DbRuleInfo> ruleInfos, int userNum, DbTransaction trans = null)
        {
            List<CrmSysRule> rules = new List<CrmSysRule>();
            List<CrmSysRuleItem> ruleItems = new List<CrmSysRuleItem>();
            List<CrmSysRuleItemRelation> ruleItemRelations = new List<CrmSysRuleItemRelation>();
            List<CrmSysRuleSet> ruleSet = new List<CrmSysRuleSet>();


            if (ruleInfos == null || ruleInfos.Count == 0)
            {
                return;
            }
            foreach (var rule in ruleInfos)
            {
                if (rule.RuleInfo != null)
                    rules.Add(rule.RuleInfo);
                if (rule.RuleItems != null)
                    ruleItems.AddRange(rule.RuleItems);
                if (rule.RuleItemRelations != null)
                    ruleItemRelations.AddRange(rule.RuleItemRelations);
                if (rule.RuleSet != null)
                    ruleSet.AddRange(rule.RuleSet);
            }
            rules = rules.Distinct(new CrmSysRuleComparer()).ToList();
            ruleItems = ruleItems.Distinct(new CrmSysRuleItemComparer()).ToList();
            ruleItemRelations = ruleItemRelations.Distinct(new CrmSysRuleItemRelationComparer()).ToList();
            ruleSet = ruleSet.Distinct(new CrmSysRuleSetComparer()).ToList();



            DbConnection conn = null;
            if (trans == null)
            {
                conn = DBHelper.GetDbConnect();
                conn.Open();
                trans = conn.BeginTransaction();
            }

            try
            {
                #region 过滤已经存在数据库的数据
                var allrules = GetAllCrmSysRuleList(trans);
                rules.RemoveAll(m => allrules.Exists(a => a.RuleId == m.RuleId));
                var allruleItems = GetAllCrmSysRuleItemList(trans);
                ruleItems.RemoveAll(m => allruleItems.Exists(a => a.ItemId == m.ItemId));
                var allruleItemRelations = GetAllCrmSysRuleItemRelationList(trans);
                ruleItemRelations.RemoveAll(m => allruleItemRelations.Exists(a => a.ItemId == m.ItemId && a.RuleId == m.RuleId && a.UserId == m.UserId));
                var ruleSetItems = GetAllCrmSysRuleSetList(trans);
                ruleSet.RemoveAll(m => ruleSetItems.Exists(a => a.RuleId == m.RuleId && a.RuleSet == m.RuleSet && a.UserId == m.UserId)); 
                #endregion


                var ruleExecuteSql = string.Format(@"INSERT INTO crm_sys_rule(ruleid,rulename,entityid,rulesql,reccreator,recupdator)
                                   SELECT ruleid,rulename,entityid,rulesql,{0},{0}
                                   FROM jsonb_populate_recordset(null::crm_sys_rule,@rules)", userNum);
                DbParameter[] rulesparams = new DbParameter[] { new NpgsqlParameter("rules", JsonConvert.SerializeObject(rules)) { NpgsqlDbType = NpgsqlDbType.Jsonb } };
                ExecuteNonQuery(ruleExecuteSql, rulesparams, trans);

                var ruleItemExecuteSql = string.Format(@"INSERT INTO crm_sys_rule_item(itemid,itemname,fieldid,operate,ruledata,ruletype,rulesql,usetype,reccreator,recupdator)
                                   SELECT itemid,itemname,fieldid,operate,ruledata,ruletype,rulesql,usetype,{0},{0}
                                   FROM jsonb_populate_recordset(null::crm_sys_rule_item,@ruleItems)", userNum);
                DbParameter[] ruleItemparams = new DbParameter[] { new NpgsqlParameter("ruleItems", JsonConvert.SerializeObject(ruleItems)) { NpgsqlDbType = NpgsqlDbType.Jsonb } };
                ExecuteNonQuery(ruleItemExecuteSql, ruleItemparams, trans);

                var ruleItemRelationsExecuteSql = string.Format(@"INSERT INTO crm_sys_rule_item_relation(ruleid,itemid,userid,rolesub,paramindex)
                                   SELECT ruleid,itemid,userid,rolesub,paramindex
                                   FROM jsonb_populate_recordset(null::crm_sys_rule_item_relation,@ruleitemrels)", userNum);
                DbParameter[] ruleItemRelationsParams = new DbParameter[] { new NpgsqlParameter("ruleitemrels", JsonConvert.SerializeObject(ruleItemRelations)) { NpgsqlDbType = NpgsqlDbType.Jsonb } };
                ExecuteNonQuery(ruleItemRelationsExecuteSql, ruleItemRelationsParams, trans);

                var ruleSetExecuteSql = string.Format(@"INSERT INTO crm_sys_rule_set(ruleid,ruleset,userid,ruleformat)
                                   SELECT ruleid,ruleset,userid,ruleformat
                                   FROM jsonb_populate_recordset(null::crm_sys_rule_set,@rulesets)", userNum);
                DbParameter[] ruleSetParams = new DbParameter[] { new NpgsqlParameter("rulesets", JsonConvert.SerializeObject(ruleSet)) { NpgsqlDbType = NpgsqlDbType.Jsonb } };
                ExecuteNonQuery(ruleSetExecuteSql, ruleSetParams, trans);

                if (conn != null)
                    trans.Commit();
            }
            catch (Exception ex)
            {
                trans.Rollback();
                throw ex;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                    conn.Dispose();
                }

            }


        }






    }
}
