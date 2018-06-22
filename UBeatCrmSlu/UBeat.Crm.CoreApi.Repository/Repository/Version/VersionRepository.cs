using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.Version;
using UBeat.Crm.CoreApi.IRepository;

namespace UBeat.Crm.CoreApi.Repository.Repository.Version
{
    public class VersionRepository : RepositoryBase, IVersionRepository
    {
        int limitCount = 5000;

        /// <summary>
        /// 获取数据版本的信息
        /// </summary>
        /// <returns></returns>
        public List<DataVersionInfo> GetDataVersions()
        {
            var sql = @"SELECT datatype,userid,maxversion FROM crm_sys_dataversion;";

            return DBHelper.ExecuteQuery<DataVersionInfo>("", sql, null);
        }

        /// <summary>
        /// 递增数据大版本号
        /// </summary>
        /// <param name="versionType"></param>
        /// <param name="usernumber"></param>
        /// <returns></returns>
        public bool IncreaseDataVersion(DataVersionType versionType, List<int> usernumbers)
        {
            var insert_sql = @"INSERT INTO crm_sys_dataversion (datatype, maxversion, userid) VALUES (@datatype, 1, @userid);";
            var update_sql = string.Format(@"UPDATE crm_sys_dataversion SET maxversion=(
                                            SELECT maxversion+1 FROM crm_sys_dataversion 
                                            WHERE datatype=@datatype AND userid=@userid AND recstatus=1
                                            )
                                            WHERE datatype=@datatype AND userid=@userid AND recstatus=1;");
            //如果是权限数据，且没有传usernumbers ，则全部人的权限版本号都加+1
            if (versionType == DataVersionType.PowerData && (usernumbers == null || usernumbers.Count == 0))
            {
                var powerUserSql = @"SELECT DISTINCT userid FROM crm_sys_dataversion WHERE datatype=6;";
                var powerUsers = ExecuteQuery(powerUserSql, null);
                usernumbers = powerUsers.Select(m => int.Parse(m["userid"].ToString())).ToList();
                if (usernumbers == null || usernumbers.Count == 0)
                {
                    return false;
                }
            }
            if (usernumbers == null || usernumbers.Count == 0)
            {
                usernumbers = new List<int>();
                usernumbers.Add(0);
            }
            using (var conn = DBHelper.GetDbConnect())
            {
                conn.Open();
                var tran = conn.BeginTransaction();
                try
                {
                    bool operate = false;
                    foreach (var userid in usernumbers)
                    {
                        var sqlParameters = new List<DbParameter>();
                        sqlParameters.Add(new NpgsqlParameter("datatype", (int)versionType));
                        sqlParameters.Add(new NpgsqlParameter("userid", userid));
                        if (DBHelper.GetCount(tran, "crm_sys_dataversion", "datatype=@datatype AND userid=@userid", sqlParameters.ToArray()) <= 0)
                        {
                            operate = DBHelper.ExecuteNonQuery(tran, insert_sql, sqlParameters.ToArray()) > 0;
                        }
                        else
                        {
                            operate = DBHelper.ExecuteNonQuery(tran, update_sql, sqlParameters.ToArray()) > 0;
                        }
                        if (operate == false)
                            throw new Exception("保存大版本数据失败");

                    }
                    tran.Commit();
                    return operate;
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

        private List<Dictionary<string, object>> GetDatasTabsByVersion(string tableName, string selectFieldsSql, long recVersion, int userNumber, out long maxVersion, out bool hasMoreData)
        {
            if (string.IsNullOrEmpty(selectFieldsSql))
                selectFieldsSql = "*";
            else
            {
                if (!selectFieldsSql.Contains("recversion"))
                    selectFieldsSql += ",recversion";
                if (!selectFieldsSql.Contains("recstatus"))
                    selectFieldsSql += ",recstatus";
            }
            var whereSql = recVersion < 0 ? "1=1 and mob=1" : "recversion>@recversion and mob=1 ";
            var limitSql = recVersion < 0 ? "" : "LIMIT @limitCount";
            var sql = string.Format(@"
                      SELECT {0} FROM {1} WHERE {2} ORDER BY recversion ASC {3} ;
                      SELECT MAX(recversion) FROM {1} ;", selectFieldsSql, tableName, whereSql, limitSql);

            var param = new DbParameter[]
                    {
                        new NpgsqlParameter("recversion", recVersion),
                        new NpgsqlParameter("limitCount", limitCount),
                    };
            var result = DBHelper.ExecuteQueryMultiple("", sql, param);

            long totalMaxVersion = recVersion;
            if (result.Count == 2)
            {
                totalMaxVersion = long.Parse(result[1].FirstOrDefault().FirstOrDefault().Value.ToString());
            }
            maxVersion = totalMaxVersion;
            if (result[0].Count > 0)
            {
                maxVersion = long.Parse(result[0][result[0].Count - 1]["recversion"].ToString());
            }

            hasMoreData = maxVersion < totalMaxVersion;
            return result[0];
        }


        private List<Dictionary<string, object>> GetDatasByVersion(string tableName, string selectFieldsSql, long recVersion, int userNumber, out long maxVersion, out bool hasMoreData, string whereSql = "")
        {
            if (string.IsNullOrEmpty(selectFieldsSql))
                selectFieldsSql = "*";
            else
            {
                if (!selectFieldsSql.Contains("recversion"))
                    selectFieldsSql += ",recversion";
                if (!selectFieldsSql.Contains("recstatus"))
                    selectFieldsSql += ",recstatus";
            }
            string execSql = recVersion < 0 ? "1=1" : "recversion>@recversion";
            if (!string.IsNullOrEmpty(whereSql))
                execSql += whereSql;

            var limitSql = recVersion < 0 ? "" : "LIMIT @limitCount";
            var sql = string.Format(@"
                      SELECT {0} FROM {1} WHERE {2} ORDER BY recversion ASC {3} ;
                      SELECT MAX(recversion) FROM {1} Where 1=1  {4} ;", selectFieldsSql, tableName, execSql, limitSql, whereSql);

            var param = new DbParameter[]
                    {
                        new NpgsqlParameter("recversion", recVersion),
                        new NpgsqlParameter("limitCount", limitCount),
                    };
            var result = DBHelper.ExecuteQueryMultiple("", sql, param);

            long totalMaxVersion = recVersion;
            if (result.Count == 2)
            {
                var frow = result[1].FirstOrDefault();
                if (frow != null)
                {
                    var fField = frow.FirstOrDefault();
                    if( fField.Value!=null)
                        totalMaxVersion = long.Parse(fField.Value.ToString());
                }
                
            }
            maxVersion = totalMaxVersion;
            if (result[0].Count > 0)
            {
                maxVersion = long.Parse(result[0][result[0].Count - 1]["recversion"].ToString());
            }

            hasMoreData = maxVersion < totalMaxVersion;
            return result[0];
        }




        #region --行政区域--
        /// <summary>
        /// 通过版本号获取行政区域
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetRegionsByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData)
        {
            return GetDatasByVersion("crm_func_region_tree(100000,1)", null, recVersion, userNumber, out maxVersion, out hasMoreData);

        }
        #endregion

        #region --团队组织--
        /// <summary>
        /// 通过版本号获取团队组织
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetDepartmentsByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData)
        {
            var tempTable = string.Format(@"(
                SELECT c.deptid,c.deptname,c.recorder,c.recversion,c.recstatus,c.pdeptid AS ancestor,t.descendant,t.nodepath,(SELECT COUNT(1)-1 FROM crm_sys_department_treepaths AS s WHERE s.ancestor = t.descendant AND EXISTS(SELECT 1 FROM crm_sys_department AS n WHERE n.deptid = s.descendant AND n.recstatus = 1 LIMIT 1))::INT AS nodes
                FROM crm_sys_department AS c
                INNER JOIN crm_sys_department_treepaths t on c.deptid = t.descendant
                WHERE t.ancestor = '7f74192d-b937-403f-ac2a-8be34714278b'
                ) AS t");

            return GetDatasByVersion(tempTable, null, recVersion, userNumber, out maxVersion, out hasMoreData);
        }
        #endregion

        #region --年次周期--
        /// <summary>
        /// 通过版本号获取年次周期
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetWeekInfoByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData)
        {
            var selectFields = "weekid,EXTRACT(YEAR FROM weekstart) AS weekyear,weeknum,weekstart,weekend,recstatus,recversion";
            return GetDatasByVersion("crm_sys_week_info", selectFields, recVersion, userNumber, out maxVersion, out hasMoreData);

        }
        #endregion


        #region --指标系数(CRM统计指标)--
        /// <summary>
        /// 通过版本号获取指标系数(CRM统计指标)
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetAnalyseFuncActiveByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData)
        {
            var tempTable = @"
            (SELECT a.recversion,a.recstatus, a.anafuncid,a.recorder,a.groupmark,f.anafuncname,f.moreflag,f.entityid,f.allowinto 
            FROM crm_sys_analyse_func_active AS a
            LEFT JOIN crm_sys_analyse_func AS f ON a.anafuncid = f.anafuncid  ) AS t";
            return GetDatasByVersion(tempTable, null, recVersion, userNumber, out maxVersion, out hasMoreData);
        }
        #endregion


        #region --系统模版配置--
        /// <summary>
        /// 通过版本号获取系统模版配置(周报日报模板)
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetTemplateByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData)
        {
            var selectFields = "tmpid,tmpname,tmpcon";
            return GetDatasByVersion("crm_sys_template", selectFields, recVersion, userNumber, out maxVersion, out hasMoreData);

        }

        #endregion


        /// <summary>
        /// 通过版本号获取用户信息配置(通讯录)
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetUserInfoByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData)
        {
            var selectFields = "";

            var tempTable = @"
            (SELECT u.userid,u.username,u.usericon,u.userphone,u.userjob,u.usertel,u.workcode,u.useremail,u.usersex,u.recstatus,u.recversion,u.namepinyin,ur.deptid,d.deptname,
				(SELECT enterprisename FROM crm_sys_enterprise LIMIT 1) AS enterprise, a.accountname
        FROM crm_sys_userinfo AS u
				LEFT JOIN crm_sys_account_userinfo_relate AS ur ON ur.userid = u.userid AND ur.recstatus = 1
				left join crm_sys_account a on a.accountid = ur.accountid
        LEFT JOIN crm_sys_department AS d ON ur.deptid = d.deptid) AS t";

            return GetDatasByVersion(tempTable, selectFields, recVersion, userNumber, out maxVersion, out hasMoreData);

        }


        /// <summary>
        /// 通过版本号获取推送消息配置
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetNotifyMessageByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData)
        {
            var selectFields = "";
            return GetDatasByVersion("crm_sys_notify_message", selectFields, recVersion, userNumber, out maxVersion, out hasMoreData);
        }

        #region --消息分组配置--
        /// <summary>
        /// 通过版本号获取消息分组配置
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetNotifyGroupByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData)
        {
            var selectFields = "msggroupid,msggroupname,msggroupicon,recorder,recstatus";
            return GetDatasByVersion("crm_sys_notify_group", selectFields, recVersion, userNumber, out maxVersion, out hasMoreData);
        }
        #endregion


        #region --数据字典配置--
        /// <summary>
        /// 通过版本号获取数据字典配置
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetDictionaryByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData)
        {
            var selectFields = "dicid,dictypeid,dataid,dataval,recorder,recstatus";
            return GetDatasByVersion("crm_sys_dictionary", selectFields, recVersion, userNumber, out maxVersion, out hasMoreData);
        }
        #endregion


        /// <summary>
        /// 通过版本号获取已删除实体列表(实体注册表配置)
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetDeleteedEntityListByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData)
        {
            string selectFields = "entityid";
            return GetDatasByVersion("crm_sys_del_version", selectFields, recVersion, userNumber, out maxVersion, out hasMoreData);
        }


        #region --实体列表(实体注册表配置)--
        /// <summary>
        /// 通过版本号获取实体列表(实体注册表配置)
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetEntityListByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData)
        {
            var selectFields = "entityid,entityname,modeltype,styles,icons,relentityid,relaudit,recorder,recstatus,newload,editload,checkload,servicesjson,relfieldid,relfieldname,inputmethod";
            return GetDatasByVersion("crm_sys_entity", selectFields, recVersion, userNumber, out maxVersion, out hasMoreData);
        }
        #endregion

        #region --实体入口--
        /// <summary>
        /// 通过版本号获取实体入口
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetEntityEntranceByVersion(long recVersion, int userNumber, int deviceType, out long maxVersion, out bool hasMoreData)
        {
            var selectFields = "entranceid,entryname,entrytype,entityid,isgroup,recorder,recstatus ";
            string whereSql = (deviceType == 0 ? " and web=1 " : " and mob=1 ");
            return GetDatasByVersion("crm_sys_entrance", selectFields, recVersion, userNumber, out maxVersion, out hasMoreData, whereSql);
        }
        #endregion

        #region --实体字段表配置--
        /// <summary>
        /// 通过版本号获取实体字段表配置
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetEntityFieldsByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData)
        {
            var selectFields = "fieldid,fieldname,entityid,fieldlabel,displayname,controltype,fieldtype,fieldconfig,recorder,recstatus,expandjs,filterjs";
            return GetDatasByVersion("crm_sys_entity_fields", selectFields, recVersion, userNumber, out maxVersion, out hasMoreData);
        }
        #endregion


        #region --实体分类表配置--
        /// <summary>
        /// 通过版本号获取实体分类表配置
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetEntityCategoryByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData)
        {
            var selectFields = "categoryid,categoryname,entityid,recorder,recstatus,relcategoryid";
            return GetDatasByVersion("crm_sys_entity_category", selectFields, recVersion, userNumber, out maxVersion, out hasMoreData);
        }
        #endregion

        #region --实体规则表配置--
        /// <summary>
        /// 通过版本号获取实体规则表配置
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetEntityFieldRulesByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData)
        {
            var selectFields = "fieldrulesid,typeid,fieldid,operatetype,viewrules,validrules,relaterules,recorder,recstatus";
            return GetDatasByVersion("crm_sys_entity_field_rules", selectFields, recVersion, userNumber, out maxVersion, out hasMoreData);
        }
        #endregion


        #region --实体职能规则表配置--
        /// <summary>
        /// 通过版本号获取实体职能规则表配置
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetEntityFieldVocationRulesByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData)
        {
            var selectFields = "fieldrulesid,entityid,vocationid,fieldid,operatetype,viewrules,recorder,recstatus ";
            return GetDatasByVersion("crm_sys_entity_field_rules_vocation", selectFields, recVersion, userNumber, out maxVersion, out hasMoreData);
        }
        #endregion


        #region --实体菜单配置--
        /// <summary>
        /// 通过版本号获取菜单配置
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetEntityMenuByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData)
        {
            var selectFields = "menuid,menuname,menutype,entityid,ruleid,recorder,recstatus ";
            return GetDatasByVersion("crm_sys_entity_menu", selectFields, recVersion, userNumber, out maxVersion, out hasMoreData);
        }
        #endregion

        #region --高级搜索配置--
        /// <summary>
        /// 通过版本号获取高级搜索配置
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetEntitySearchByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData)
        {
            var selectFields = "searchid,entityid,fieldid,recorder,recstatus,islike as controltype ";
            return GetDatasByVersion("crm_sys_entity_search", selectFields, recVersion, userNumber, out maxVersion, out hasMoreData);
        }

        #endregion

        #region --实体列表显示配置（设置手机端列表显示）--
        /// <summary>
        /// 通过版本号获取实体列表显示配置（设置手机端列表显示）
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetEntityListViewByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData)
        {
            var selectFields = "viewconfid,entityid,viewstyleid,fieldkeys,fonts,colors,recorder,recstatus";
            return GetDatasByVersion("crm_sys_entity_listview_config", selectFields, recVersion, userNumber, out maxVersion, out hasMoreData);
        }
        #endregion

        #region --实体主页显示配置--
        /// <summary>
        /// 通过版本号获取实体主页显示配置
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetEntityPageByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData)
        {
            var selectFields = "entityid,titlefieldid,subfieldids,(SELECT entityid FROM crm_sys_entity_datasource WHERE datasrcid=(select (fieldconfig#>>'{dataSource,sourceId}')::uuid from crm_sys_entity_fields WHERE fieldid=relfieldid limit 1))relentityid,modules,relfieldid,recstatus";
            return GetDatasByVersion("crm_sys_entity_page_config", selectFields, recVersion, userNumber, out maxVersion, out hasMoreData);
        }
        #endregion


        #region --实体功能按钮配置--
        /// <summary>
        /// 通过版本号获取实体功能按钮配置
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetEntityCompomentByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData)
        {
            var selectFields = "comptid,entityid,comptname,comptaction,icon,recorder,recstatus ";
            return GetDatasByVersion("crm_sys_entity_compoment_config", selectFields, recVersion, userNumber, out maxVersion, out hasMoreData);
        }
        #endregion


        #region --实体销售阶段高级设置配置--
        /// <summary>
        /// 通过版本号获取实体销售阶段高级设置配置
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetEntitySalessTagetByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData)
        {
            var selectFields = "isopenhighsetting,salesstagetypeid typeid,recorder,recstatus";
            return GetDatasByVersion("crm_sys_salesstage_type_setting", selectFields, recVersion, userNumber, out maxVersion, out hasMoreData);
        }
        #endregion


        #region --实体关系页签配置--
        /// <summary>
        /// 通过版本号获取实体关系页签配置
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetEntityRelateTabByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData)
        {
            var tempTable = string.Format(@"(
                SELECT t.*,f.fieldname AS fieldname
                FROM crm_sys_entity_rel_tab AS t
                LEFT JOIN crm_sys_entity_fields f on f.fieldid=t.fieldid
                ) AS t");
            var selectFields = "relid,entityid,relentityid,relname,icon,recorder,recstatus,tabtype,fieldid,fieldname,ismanytomany,srcsql,srctitle";
            return GetDatasTabsByVersion(tempTable, selectFields, recVersion, userNumber, out maxVersion, out hasMoreData);
        }
        #endregion

        #region --实体查重配置--
        /// <summary>
        /// 通过版本号获取实体查重配置
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetEntityConditionByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData)
        {
            string selectFields = null;
            return GetDatasByVersion("crm_sys_entity_condition", selectFields, recVersion, userNumber, out maxVersion, out hasMoreData);
        }
        #endregion
       


        #region --流程审批配置--
        /// <summary>
        /// 通过版本号获取流程审批配置
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetWorkflowListByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData)
        {
            var selectFields = "flowid,flowtype,backflag,resetflag,flowname,entityid,vernum,recorder,recstatus";
            return GetDatasByVersion("crm_sys_workflow", selectFields, recVersion, userNumber, out maxVersion, out hasMoreData);
        }
        #endregion

        /// <summary>
        /// 通过版本号获取职能功能表配置
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetVocationFunctionByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData)
        {
            maxVersion = 0;
            hasMoreData = false;
            return null;
        }


        #region --产品信息--
        /// <summary>
        /// 通过版本号获取产品信息
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetProductsByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData)
        {
            var selectFields = "recid AS productid,productname,0 AS iscommon,productsetid,recorder,recstatus,recversion ";
            return GetDatasByVersion("crm_sys_product", selectFields, recVersion, userNumber, out maxVersion, out hasMoreData);

        }
        #endregion

        #region --产品系列--
        /// <summary>
        /// 通过版本号获取产品系列
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetProductSeriesByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData)
        {
            var tempTable = string.Format(@"(
                SELECT c.productsetid,c.productsetname,c.recorder,c.recversion,c.recstatus,c.pproductsetid AS ancestor,t.descendant,t.nodepath,(SELECT COUNT(1)-1 FROM crm_sys_products_series_treepaths AS s WHERE s.ancestor = t.descendant AND EXISTS(SELECT 1 FROM crm_sys_products_series AS n WHERE n.productsetid = s.descendant AND n.recstatus = 1 LIMIT 1))::INT AS nodes 
                FROM crm_sys_products_series AS c
                INNER JOIN crm_sys_products_series_treepaths t on c.productsetid = t.descendant
                WHERE t.ancestor = '7f74192d-b937-403f-ac2a-8be34714278b'
                ) AS t");

            return GetDatasByVersion(tempTable, null, recVersion, userNumber, out maxVersion, out hasMoreData);
        }
        #endregion


        #region --邮箱信息--
        /// <summary>
        /// 通过版本号获取邮箱信息
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetMailboxDataByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData)
        {

            //var selectFields = "comptid,entityid,comptname,comptaction,icon,recorder,recstatus ";
            return GetDatasByVersion("crm_sys_mail_mailbox", null, recVersion, userNumber, out maxVersion, out hasMoreData);
        }
        #endregion

        #region 定位策略信息
        /// <summary>
        /// 根据版本获取定位策略信息
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <param name="hasMoreData"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetTrackSettingData(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData)
        {

            //var selectFields = "comptid,entityid,comptname,comptaction,icon,recorder,recstatus ";
            return GetDatasByVersion("crm_sys_track_strategy_allocation", null, recVersion, userNumber, out maxVersion, out hasMoreData);
        }
        #endregion
    }
}
