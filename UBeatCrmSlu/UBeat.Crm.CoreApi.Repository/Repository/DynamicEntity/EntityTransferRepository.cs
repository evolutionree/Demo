using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.DynamicEntity;
using UBeat.Crm.CoreApi.IRepository;
using System.Data.Common;
namespace UBeat.Crm.CoreApi.Repository.Repository.DynamicEntity
{
    public class EntityTransferRepository : RepositoryBase, IEntityTransferRepository
    {
        /// <summary>
        /// 
        /// 根据RuleID获得转换规则的详情
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userNum"></param>
        /// <returns></returns>
        public EntityTransferRuleInfo getById(string id, int userNum, DbTransaction transaction = null)
        {
            try
            {
                string cmdText = "Select * from crm_sys_entity_transfer_rule where recstatus = 1  and recid = @recid";
                var param = new DbParameter[] {
                    new Npgsql.NpgsqlParameter("recid",Guid.Parse(id))
                };
                List<EntityTransferRuleInfo> list = ExecuteQuery<EntityTransferRuleInfo>(cmdText, param, transaction);
                if (list != null && list.Count > 0)
                {
                    return list[0].parseAllJsonString();
                }
                return null;

            }
            catch (Exception ex)
            {

            }
            return null;
        }

        /// <summary>
        /// 根据查询条件，查询符合的转换规则列表
        /// </summary>
        /// <param name="queryModel"></param>
        /// <param name="userNum"></param>
        /// <returns></returns>
        public List<EntityTransferRuleInfo> queryRules(EntityTransferRuleQueryModel queryModel, int userNum, DbTransaction transaction = null)
        {
            try
            {
                if (queryModel.SrcEntityId != null && queryModel.SrcEntityId.Length > 0 && queryModel.SrcEntityId != Guid.Empty.ToString()
                    && queryModel.DstEntityId != null && queryModel.DstEntityId.Length > 0 && queryModel.DstEntityId != Guid.Empty.ToString())
                {
                    constructOtherCategoryRule(queryModel.SrcEntityId, queryModel.DstEntityId);
                }
                string cmdText = @"
                                    Select rules.* ,srcentity.entityname SrcEntityName,
		                                    dstentity.entityname DstEntityName,
		                                    srccategory.categoryname SrcCategoryName,
		                                    dstcategory.categoryname DstCategoryName
                                    from crm_sys_entity_transfer_rule rules 
	                                    left outer join crm_sys_entity srcEntity 
			                                    on rules.srcentity = srcentity.entityid::text 
	                                    left outer join crm_sys_entity dstEntity 
			                                    on rules.dstentity = dstEntity.entityid::text
	                                    left outer join crm_sys_entity_category srcCategory  
			                                    on rules.srccategory = srcCategory.categoryid::text
	                                    left outer join crm_sys_entity_category dstCategory 
			                                    on rules.dstcategory = dstCategory.categoryid::text 
                                    where rules.recstatus = 1  
                                        and dstCategory.recstatus = 1 
                                        and dstEntity.recstatus = 1 ";
                if (queryModel.SrcEntityId != null && queryModel.SrcEntityId.Length != 0 && queryModel.SrcEntityId != Guid.Empty.ToString())
                {
                    cmdText = cmdText + " and rules.srcentity='" + queryModel.SrcEntityId.Replace("'", "'' ") + "'";
                }
                if (queryModel.SrcCategoryId != null && queryModel.SrcCategoryId.Length != 0 && queryModel.SrcCategoryId != Guid.Empty.ToString())
                {
                    cmdText = cmdText + " and  (rules.srccategory is null or rules.srccategory = '' or  rules.srccategory='" + queryModel.SrcCategoryId.Replace("'", "''") + "') ";
                }
                if (queryModel.DstEntityId != null && queryModel.DstEntityId.Length != 0 && queryModel.DstEntityId != Guid.Empty.ToString())
                {
                    cmdText = cmdText + " and rules.dstentity ='" + queryModel.DstEntityId.Replace("'", "''") + "' ";
                }
                if (queryModel.DstCategoryId != null && queryModel.DstCategoryId.Length != 0 && queryModel.DstCategoryId != Guid.Empty.ToString())
                {
                    cmdText = cmdText + " and rules.dstcategory ='" + queryModel.DstCategoryId.Replace("'", "''") + "'";
                }
                if (queryModel.IsInner == 1)
                {
                    cmdText = cmdText + " And rules.isuseforinner = 1";
                }
                cmdText = cmdText + " order by dstCategory.recorder ";
                List<EntityTransferRuleInfo> data = ExecuteQuery<EntityTransferRuleInfo>(cmdText, new DbParameter[] { }, transaction);
                foreach (EntityTransferRuleInfo item in data)
                {
                    item.RecName = "转为" + item.DstEntityName + "(" + item.DstCategoryName + ")";
                }
                return data;
            }
            catch (Exception ex)
            {

            }
            return new List<EntityTransferRuleInfo>();
        }
        private void constructOtherCategoryRule(string srcentityid, string dstentityid, DbTransaction transaction = null)
        {
            string cmdText = string.Format(@"
                                INSERT INTO crm_sys_entity_transfer_rule (
	                                recname, reccode, recaudits, recstatus, reccreator,
	                                recupdator, recmanager, reccreated, recupdated, recversion, 
	                                srcentity, srccategory, dstentity, dstcategory, ruleid, 
	                                transferjson, isautosave, dealwithotherservice, isuseforinner, checknexttransfer, writeback) 

                                select needcategory.entityname  || '('||needcategory.categoryname||')'  recname , 
	                                 baserule.reccode, baserule.recaudits, baserule.recstatus, baserule.reccreator,
	                                baserule.recupdator, baserule.recmanager, baserule.reccreated, baserule.recupdated, baserule.recversion, 
	                                baserule.srcentity, baserule.srccategory, baserule.dstentity, needcategory.categoryid dstcategory, baserule.ruleid, 
	                                baserule.transferjson, baserule.isautosave, baserule.dealwithotherservice, baserule.isuseforinner, baserule.checknexttransfer, baserule.writeback
                                FROM
                                (
	                                Select b.categoryid,a.entityname ,b.categoryname
	                                from  crm_sys_entity  a  
	                                left outer join crm_sys_entity_category b on a.entityid = b.entityid 
	                                left outer join  crm_sys_entity_transfer_rule c on (c.dstentity = a.entityid::text  
                                        and c.dstcategory = b.categoryid::text and c.srcentity ='{0}')
	                                where b.recstatus = 1 and c.recid is null 
	                                and a.entityid = '{1}'
                                ) needcategory ,
                                (
                                select reccode, recaudits, recstatus, reccreator,
	                                recupdator, recmanager, reccreated, recupdated, recversion, 
	                                srcentity, srccategory, dstentity,  ruleid, 
	                                transferjson, isautosave, dealwithotherservice, isuseforinner, checknexttransfer, writeback
                                from crm_sys_entity_transfer_rule a 
                                where  srcentity = '{0}' and dstentity ='{1}' and (srccategory is null or  srccategory = '')
                                limit 1
                                ) baserule  ", srcentityid, dstentityid);
            try
            {
                ExecuteNonQuery(cmdText, new DbParameter[] { }, transaction);
            }
            catch (Exception ex)
            {

            }
        }
        /// <summary>
        /// 根据分类id获取分类名称
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="userNum"></param>
        /// <returns></returns>
        public string getCategoryName(string categoryId, int userNum, DbTransaction transaction = null)
        {
            string cmdText = @"SELECT categoryname  FROM crm_sys_entity_category WHERE categoryid = " + categoryId + " LIMIT 1;";
            try
            {
                return (string)ExecuteScalar(cmdText, new DbParameter[] { }, transaction);
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        /// <summary>
        /// 根据字典类型ID和字典数据ID获取字典数的名称
        /// </summary>
        /// <param name="dataid"></param>
        /// <param name="typeid"></param>
        /// <param name="userNum"></param>
        /// <returns></returns>
        public string getDictNameByDataId(int dataid, int typeid, int userNum, DbTransaction transaction = null)
        {
            string cmdText = string.Format(@"SELECT dataval
                        FROM crm_sys_dictionary WHERE dictypeid ={0} and dataid={1} LIMIT 1", typeid, dataid);
            try
            {
                return (string)ExecuteScalar(cmdText, new DbParameter[] { }, transaction);
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        /// <summary>
        /// 根据字典类型ID和字典数据,获取字典数的值
        /// </summary>
        /// <param name="vals"></param>
        /// <param name="userNum"></param>
        /// <returns></returns>
        public string getDictByVal(int typeid, string vals)
        {
            string cmdText = string.Format(@" SELECT string_agg(dataval,',')  FROM crm_sys_dictionary 
                  WHERE dictypeid = {0} AND dataid IN (
							            SELECT dataid::INT from (
										            SELECT UNNEST( string_to_array({1}::text, ',')) as dataid 
							            ) as r WHERE dataid!=''
                  )", typeid, vals);
            try
            {
                return (string)ExecuteScalar(cmdText, new DbParameter[] { }, null);
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        /// <summary>
        /// 
        /// 根据客户的名称获取客户的ID
        /// </summary>
        /// <param name="CustName"></param>
        /// <param name="userNum"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public Guid getCustomIdByName(string CustName, int userNum, string userName, DbTransaction transaction = null)
        {
            try
            {
                string RuleSQL = " 1=1 ";
                string whereSQL = " e.recname ='" + CustName.Replace("''", "''") + "'";
                string cmdText = string.Format(" select recid from crm_sys_customer e where 1=1 and {0} And {1}", RuleSQL, whereSQL);
                return (Guid)ExecuteScalar(cmdText, new DbParameter[] { }, transaction);
            }
            catch (Exception ex)
            {
            }
            return Guid.Empty;
        }

        public Dictionary<string, object> getCustomerDataSourceByClue(string clueid, int userNum, string userName, DbTransaction transaction)
        {
            try
            {
                string RuleSQL = " 1=1 ";
                string whereSQL = " jsonb_extract_path_text(e.saleclue,'id') ='" + clueid + "'";
                string cmdText = string.Format(" select recid as  id  ,recname  as name from crm_sys_customer e where 1=1 and {0} And {1}", RuleSQL, whereSQL);
                List<Dictionary<string, object>> list = ExecuteQuery(cmdText, new DbParameter[] { }, transaction);

                if (list != null && list.Count > 0) return list[0];
                return null;
            }
            catch (Exception ex)
            {
            }
            return null;
        }

        public bool CheckHasContact(string custid, string phone, DbTransaction transaction = null)
        {
            try
            {
                string RuleSQL = " 1=1 ";
                string whereSQL = " jsonb_extract_path_text(e.belcust,'id') ='" + custid + "'";
                string phoneMobileSQL = " e.mobilephone ='" + phone.Replace("'", "''") + "'";
                string cmdText = string.Format(" select* from crm_sys_contact e where 1=1 and {0} And {1} and {2}", RuleSQL, whereSQL, phoneMobileSQL);
                List<Dictionary<string, object>> list = ExecuteQuery(cmdText, new DbParameter[] { }, transaction);

                if (list != null && list.Count > 0) return true;
                return false;
            }
            catch (Exception ex)
            {
            }
            return false;
        }

        public string getProductName(string productid, int userNum)
        {
            try
            {
                string RuleSQL = " 1=1 ";
                string whereSQL = " recid ='" + productid + "'";
                string cmdText = string.Format(" select productname   from crm_sys_product e where 1=1 and {0} And {1} ", RuleSQL, whereSQL);
                List<Dictionary<string, object>> list = ExecuteQuery(cmdText, new DbParameter[] { }, null);

                if (list != null && list.Count > 0) return list[0]["productname"].ToString();
                return "";
            }
            catch (Exception ex)
            {
            }
            return "";
        }

        public string getBaseCustomIdByName(string custName, int userNum, DbTransaction transaction = null)
        {
            try
            {
                string cmdText = string.Format(@"Select recid from crm_sys_custcommon where recname like '{0}' ", custName.Replace("'", "''"));
                object obj = ExecuteScalar(cmdText, new DbParameter[] { }, transaction);
                if (obj != null) return obj.ToString();
            }
            catch (Exception ex)
            {

            }
            return null;
        }

    }
}
