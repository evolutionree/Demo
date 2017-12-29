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

        public void SaveRuleInfoList(List<DbRuleInfo> ruleInfos,int userNum, DbTransaction trans = null)
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
            List<DbParameter[]> ruleParams = GetDbParameters(rules.Distinct(new CrmSysRuleComparer()).ToList());
            List<DbParameter[]> ruleItemParams = GetDbParameters(ruleItems.Distinct(new CrmSysRuleItemComparer()).ToList());
            List<DbParameter[]> ruleItemRelationsParams = GetDbParameters(ruleItemRelations.Distinct(new CrmSysRuleItemRelationComparer()).ToList());
            List<DbParameter[]> ruleSetParams = GetDbParameters(ruleSet.Distinct(new CrmSysRuleSetComparer()).ToList());

            DbConnection conn = null;
            if (trans == null)
            {
                conn = DBHelper.GetDbConnect();
                conn.Open();
                trans = conn.BeginTransaction();
            }

            try
            {
                var ruleExecuteSql = string.Format(@"INSERT INTO crm_sys_rule(ruleid,rulename,entityid,rulesql,reccreator,recupdator)
                                   VALUES(@ruleid,@rulename,@entityid,@rulesql,{0},{0})", userNum);
                ruleParams = ruleParams.Distinct().ToList();
                ExecuteNonQueryMultiple(ruleExecuteSql, ruleParams, trans);

                var ruleItemExecuteSql = string.Format(@"INSERT INTO crm_sys_rule_item(itemid,itemname,fieldid,operate,ruledata,ruletype,rulesql,usetype,reccreator,recupdator)
                                   VALUES(@itemid,@itemname,@fieldid,@operate,@ruledata,@ruletype,@rulesql,@usetype,{0},{0})", userNum);
                ExecuteNonQueryMultiple(ruleItemExecuteSql, ruleItemParams, trans);

                var ruleItemRelationsExecuteSql = string.Format(@"INSERT INTO crm_sys_rule_item_relation(ruleid,itemid,userid,rolesub,paramindex)
                                   VALUES(@ruleid,@itemid,@userid,@rolesub,@paramindex)");
                ExecuteNonQueryMultiple(ruleItemRelationsExecuteSql, ruleItemRelationsParams, trans);

                var ruleSetExecuteSql = string.Format(@"INSERT INTO crm_sys_rule_set(ruleid,ruleset,userid,ruleformat)
                                   VALUES(@ruleid,@ruleset,@userid,@ruleformat)");
                
                ExecuteNonQueryMultiple(ruleSetExecuteSql, ruleSetParams, trans);

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
