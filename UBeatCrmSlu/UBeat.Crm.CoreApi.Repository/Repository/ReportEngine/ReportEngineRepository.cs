using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.DomainModel.Reports;

namespace UBeat.Crm.CoreApi.Repository.Repository.ReportEngine
{
    public class ReportEngineRepository :RepositoryBase, IReportEngineRepository
    {
        public static string WebReportRootFolderId = "10000000-1000-1000-1000-100000000001";
        public static string MobileReportRootFolderId = "10000000-1000-1000-1000-100000000002";
        public static string FunctionReportRootIdForWeb = "10000000-1000-1000-1000-100000000001";
        public static string FunctionReportNotDeployIdForWeb = "5040da80-bc62-4f71-b64f-945830cfa400";
        public static string FunctionReportRootIdForMob = "10000000-1000-1000-1000-100000000002";
        public static string FunctionReportNotDeployIdForMob = "af153e75-4946-40e5-b663-1a5be208bac9";
        public static string ReportMenuEntityId = "10000000-1000-1000-1000-100000000001";
        public static string FunctionWebRootId = "1f9a7c10-0a22-4ef0-825e-c98d4503c600";
        public static string FunctionMobRootId = "d90680f9-5cf3-49c2-a83e-8ab267ff094a";
        public static string WebMenu_ReportRootId = "10000000-0000-0000-0001-000000000003";
        public static string WebMenu_Root = "00000000-0000-0000-0000-000000000000";
        public Dictionary<string, List<Dictionary<string, object>>> queryDataFromDataSource_CommonSQL(DbTransaction transaction, string sql, Dictionary<string, object> param)
        {
            try
            {
                var cmdParams = new DbParameter[param.Count];
                int index = 0;
                foreach(string key in param.Keys)
                {
                    cmdParams[index] = new NpgsqlParameter(key, param[key]);
                }
                List<Dictionary<string, object>>  ret = DBHelper.ExecuteQuery(transaction, sql, cmdParams);
                        Dictionary<string, List<Dictionary<string, object>>> retData = new Dictionary<string, List<Dictionary<string, object>>>();
                        retData.Add("data", ret);
                        return retData;
                    }
                    catch (Exception ex) {
            }
            return null;
        }

        public Dictionary<string, List<Dictionary<string, object>>> 
            queryDataFromDataSource_FuncSQL(DbTransaction transaction, string funcName, string paramDefined, Dictionary<string, object> param)
        {
            try {
                DbParameter[] p = new NpgsqlParameter[param.Count];
                Dictionary<string, string> hasParams = new Dictionary<string, string>();
                int index = 0;
                foreach (string item in param.Keys) {
                    string newItem = item;
                    if (item.StartsWith("@")) {
                        newItem = item.Substring(1);
                    }
                    if (item == "pageindex" || item == "@pageindex" || item == "pagesize" || item == "@pagesize" || item == "userno" || item == "@userno")
                    {
                        p[index] = new NpgsqlParameter(newItem, NpgsqlTypes.NpgsqlDbType.Integer);
                        p[index].Value =  param[item];
                        hasParams.Add(newItem,newItem);
                        hasParams.Add("@" + newItem, newItem);
                    }
                    else
                    {

                        p[index] = new NpgsqlParameter(newItem, param[item]); ;
                        hasParams.Add(newItem, newItem);
                        hasParams.Add("@" + newItem, newItem);
                    }
                    index++;
                }
                #region 检查并补全参数
                if (paramDefined != null && paramDefined.Length > 0)
                {
                    List<DbParameter> needAddP = new List<DbParameter>();
                    string[] definedParams = paramDefined.Split(',');
                    foreach (string pn in definedParams) {
                        if (hasParams.ContainsKey(pn) == false) {
                            needAddP.Add(new NpgsqlParameter(pn,""));
                        }
                    }
                    DbParameter[] tmpp = p;
                    p = new DbParameter[tmpp.Length + needAddP.Count];
                    for (int i = 0; i < tmpp.Length; i++) {
                        p[i] = tmpp[i];
                    }
                    int index1 = tmpp.Length;
                    foreach (DbParameter item in needAddP) {
                        p[index1] = item;
                        index1++;
                    }
                }
                #endregion
                return this.ExecuteQueryRefCursor(funcName, p, transaction);
            } catch (Exception ex) {
                throw (ex);
            }
        }
        public  List<Dictionary<string, object>> ExecuteSQL(string cmdText,DbParameter[] dbParam) {
            try
            {
                return ExecuteQuery(cmdText, dbParam);
            }
            catch (Exception ex)    {
            }
            return null;
        }



        public List<ReportFolderInfo> queryWebReportList()
        {
            return getReportFolderList(WebReportRootFolderId);
        }

        public List<ReportFolderInfo> queryMobileReportList()
        {
            return getReportFolderList(MobileReportRootFolderId);
        }

        /// <summary>
        /// 根据根目录（WEB or mobile）获取所有的已发布报表目录结构
        /// </summary>
        /// <param name="rootid"></param>
        /// <returns></returns>
        private List<ReportFolderInfo> getReportFolderList(string rootid) {
            try
            {
                string cmdText = "Select * from crm_sys_reportfolder order by level,index";
                List<ReportFolderInfo> orgFolderList = ExecuteQuery<ReportFolderInfo>(cmdText, new DbParameter[] { });
                if (orgFolderList == null || orgFolderList.Count == 0) {
                    return new List<ReportFolderInfo>();
                }
                Dictionary<Guid, ReportFolderInfo> allFolders = new Dictionary<Guid, ReportFolderInfo>();
                Dictionary<Guid, ReportFolderInfo> RootFolders = new Dictionary<Guid, ReportFolderInfo>();
                foreach (ReportFolderInfo folderInfo in orgFolderList) {
                    if (allFolders.ContainsKey(folderInfo.ParentId))
                    {
                        allFolders[folderInfo.ParentId].SubFolders.Add(folderInfo);
                        allFolders.Add(folderInfo.Id, folderInfo);
                    }
                    else {
                        if (folderInfo.ParentId == Guid.Empty) {
                            RootFolders.Add(folderInfo.Id, folderInfo);
                            allFolders.Add(folderInfo.Id, folderInfo);
                        }
                    }
                }
                Guid needId = Guid.Parse(rootid);
                if (RootFolders.ContainsKey(needId)) {
                    return RootFolders[needId].SubFolders;
                }
            }
            catch (Exception ex) {

            }
            return new List<ReportFolderInfo>();
        }

        /// <summary>
        /// 根据datasourceid获取entityid
        /// </summary>
        /// <param name="datasourceid"></param>
        /// <param name="userNum"></param>
        /// <returns></returns>
        public string getEntityIdByDataSourceId(string datasourceid, int userNum)
        {
            try
            {
                string cmdText = string.Format("select  entityid::text  from crm_sys_entity_datasource  where datasrcid = '{0}' ", datasourceid);
                string ret = (string)ExecuteScalar(cmdText, new DbParameter[] { });
                return ret;
            }
            catch (Exception ex) {
                
            }
            return null;
        }

        /// <summary>
        /// 根据报表定义表和报表发布目录表来重新生成web权限项
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="userNum"></param>
        public void repairWebReportFunctions(DbTransaction trans, int userNum) {
            List<string> functionItems = new List<string>();
            string subIds = "";
            #region 查询第一级权限项
            try
            {
                string cmdText = string.Format("Select funcid from crm_sys_function where  parentid  = '" + FunctionReportRootIdForWeb + "'");
                List<Dictionary<string, object>> rootData = ExecuteQuery(cmdText, new DbParameter[] { }, trans);
                foreach (Dictionary<string, object> item in rootData)
                {
                    functionItems.Add(item["funcid"].ToString());
                    subIds = subIds + ",'" + item["funcid"].ToString() + "'";
                }
                if (subIds.Length > 0)
                {
                    subIds = subIds.Substring(1);
                }
            }
            catch (Exception ex)
            {
            }
            #endregion
            #region 查询第二级权限项
            try
            {
                if (subIds.Length > 0)
                {
                    string cmdText = string.Format("Select funcid from crm_sys_function where  parentid  in(" + subIds + ")");
                    List<Dictionary<string, object>> rootData = ExecuteQuery(cmdText, new DbParameter[] { }, trans);
                    foreach (Dictionary<string, object> item in rootData)
                    {
                        functionItems.Add(item["funcid"].ToString());
                        subIds = subIds + ",'" + item["funcid"].ToString() + "'";
                    }
                }
            }
            catch (Exception ex)
            {
            }
            if (subIds.Length > 0 && subIds.StartsWith(","))
            {
                subIds = subIds.Substring(1);
            }
            #endregion
            #region 删除权限项及treepath
            if (subIds.Length > 0)
            {
                try
                {
                    string cmdText = "delete from crm_sys_function where funcid in (" + subIds + ")";
                    ExecuteNonQuery(cmdText, new DbParameter[] { }, trans);
                }
                catch (Exception ex)
                {

                }
                try
                {
                    string cmdText = "delete from crm_sys_function_treepaths where descendant in (" + subIds + ")";
                    ExecuteNonQuery(cmdText, new DbParameter[] { }, trans);
                }
                catch (Exception ex)
                {

                }
            } 
            #endregion
            #region 检查是否存在web report root
            try
            {
                string cmdText = "Select count(*) from crm_sys_function where funcid = '" + FunctionReportRootIdForWeb + "'";
                int count = int.Parse(ExecuteScalar(cmdText, new DbParameter[] { }, trans).ToString());
                if (count == 0)
                {
                    cmdText = string.Format(@"INSERT INTO crm_sys_function(
	                                                funcid,	funcname,	funccode,	parentid,	entityid,
	                                                devicetype,	recorder,	recstatus,	reccreated,	recupdated,
	                                                reccreator,	recupdator,		rectype,	relationvalue,
	                                                routepath,	islastchild,	childtype
                                                )
                                                VALUES
	                                                (
		                                                '{0}'::uuid,'报表','sss','{1}'::uuid,'{2}'::uuid,
		                                                '0','0','1',now(),now(),
		                                                '7','7','0','',
		                                                '',	'0',NULL
	                                                )", FunctionReportRootIdForWeb, FunctionWebRootId, ReportMenuEntityId);
                    ExecuteNonQuery(cmdText, new DbParameter[] { }, trans);
                    cmdText = string.Format("select crm_func_repairefunctiontreepath('{0}')", FunctionReportRootIdForWeb);
                    ExecuteNonQuery(cmdText, new DbParameter[] { }, trans);
                }

            }
            catch (Exception ex)
            {

            }
            #endregion

            #region 插入未发布的报表的权限项
            try
            {
                string cmdText = string.Format(@"INSERT INTO crm_sys_function(
	                                                funcid,	funcname,	funccode,	parentid,	entityid,
	                                                devicetype,	recorder,	recstatus,	reccreated,	recupdated,
	                                                reccreator,	recupdator,		rectype,	relationvalue,
	                                                routepath,	islastchild,	childtype
                                                )
                                                VALUES
	                                                (
		                                                '{0}'::uuid,'未发布报表','sss','{1}'::uuid,'{2}'::uuid,
		                                                '0','0','1',now(),now(),
		                                                '7','7','0','',
		                                                '',	'0',NULL
	                                                )", FunctionReportNotDeployIdForWeb, FunctionReportRootIdForWeb, ReportMenuEntityId);
                ExecuteNonQuery(cmdText, new DbParameter[] { }, trans);
                cmdText = string.Format("select crm_func_repairefunctiontreepath('{0}')", FunctionReportNotDeployIdForWeb);
                ExecuteNonQuery(cmdText, new DbParameter[] { }, trans);
            }
            catch (Exception ex)
            {
            }
            #endregion

            transferReportFolder2Function(trans, WebReportRootFolderId, userNum);
            #region 处理未发布的表表
            try
            {
                string cmdText = string.Format(@"select recid as id ,recname as name  
                                                from crm_sys_reportdefine 
                                               where recid not in (select reportid from crm_sys_reportfolder )
                                                     and recstatus = 1  order by reccreated");
                List<Dictionary<string, object>> data = ExecuteQuery(cmdText, new DbParameter[] { }, trans);
                int index = 0;
                foreach (Dictionary<string, object> item in data) {
                    transferReport2Function(trans, item, index, FunctionReportNotDeployIdForWeb, userNum);
                    index++;
                }
            }
            catch (Exception ex) {
            }
            #endregion 
        }
        private void transferReportFolder2Function(DbTransaction trans ,string parentfolderid, int userNum) {
            try
            {
                string cmdText = "select * from crm_sys_reportfolder where parentid = '" + parentfolderid + "' order by index ";
                List<Dictionary<string, object>> data = ExecuteQuery(cmdText, new DbParameter[] { }, trans);
                int index = 0;
                foreach (Dictionary<string, object> item in data) {
                    transferReport2Function(trans, item, index, parentfolderid, userNum);
                    if ((bool)item["isfolder"])
                    {
                        transferReportFolder2Function(trans, item["id"].ToString(), userNum);
                    }
                    index++;
                }
            }
            catch (Exception ex) {
            }
        }
        private void transferReport2Function(DbTransaction trans, Dictionary<string, object> item,int index, string parentfolderid, int userNum) {
            try
            {
                string cmdText = string.Format(@"INSERT INTO crm_sys_function(
	                                                funcid,	funcname,	funccode,	parentid,	entityid,
	                                                devicetype,	recorder,	recstatus,	reccreated,	recupdated,
	                                                reccreator,	recupdator,		rectype,	relationvalue,
	                                                routepath,	islastchild,	childtype
                                                )
                                                VALUES
	                                                (
		                                                '{0}'::uuid,'{3}','report','{1}'::uuid,'{2}'::uuid,
		                                                '0','{4}','1',now(),now(),
		                                                '7','7','0','',
		                                                '',	'0',NULL
	                                                )", item["id"].ToString(), parentfolderid, ReportMenuEntityId,item["name"].ToString(), index);
                ExecuteNonQuery(cmdText, new DbParameter[] { }, trans);
                cmdText = string.Format("select crm_func_repairefunctiontreepath('{0}')", item["id"].ToString());
                ExecuteNonQuery(cmdText, new DbParameter[] { }, trans);
            }
            catch (Exception ex) {
            }
        }
        /// <summary>
        /// 根据报表定义表和报表发布目录表来重新生成移动权限项
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="userNum"></param>
        public void repairMobReportFunctions(DbTransaction trans, int userNum)
        {
            List<string> functionItems = new List<string>();
            string subIds = "";
            #region 查询第一级权限项
            try
            {
                string cmdText = string.Format("Select funcid from crm_sys_function where  parentid  = '" + FunctionReportRootIdForMob + "'");
                List<Dictionary<string, object>> rootData = ExecuteQuery(cmdText, new DbParameter[] { }, trans);
                foreach (Dictionary<string, object> item in rootData)
                {
                    functionItems.Add(item["funcid"].ToString());
                    subIds = subIds + ",'" + item["funcid"].ToString() + "'";
                }
                if (subIds.Length > 0)
                {
                    subIds = subIds.Substring(1);
                }
            }
            catch (Exception ex)
            {
            }
            #endregion
            #region 查询第二级权限项
            try
            {
                if (subIds.Length > 0)
                {
                    string cmdText = string.Format("Select funcid from crm_sys_function where  parentid  in(" + subIds + ")");
                    List<Dictionary<string, object>> rootData = ExecuteQuery(cmdText, new DbParameter[] { }, trans);
                    foreach (Dictionary<string, object> item in rootData)
                    {
                        functionItems.Add(item["funcid"].ToString());
                        subIds = subIds + ",'" + item["funcid"].ToString() + "'";
                    }
                }
            }
            catch (Exception ex)
            {
            }
            if (subIds.Length > 0 && subIds.StartsWith(","))
            {
                subIds = subIds.Substring(1);
            }
            #endregion
            #region 删除权限项及treepath
            if (subIds.Length > 0)
            {
                try
                {
                    string cmdText = "delete from crm_sys_function where funcid in (" + subIds + ")";
                    ExecuteNonQuery(cmdText, new DbParameter[] { }, trans);
                }
                catch (Exception ex)
                {

                }
                try
                {
                    string cmdText = "delete from crm_sys_function_treepaths where descendant in (" + subIds + ")";
                    ExecuteNonQuery(cmdText, new DbParameter[] { }, trans);
                }
                catch (Exception ex)
                {

                }
            }
            #endregion
            #region 检查是否存在web report root
            try
            {
                string cmdText = "Select count(*) from crm_sys_function where funcid = '" + FunctionReportRootIdForMob + "'";
                int count = int.Parse(ExecuteScalar(cmdText, new DbParameter[] { }, trans).ToString());
                if (count == 0)
                {
                    cmdText = string.Format(@"INSERT INTO crm_sys_function(
	                                                funcid,	funcname,	funccode,	parentid,	entityid,
	                                                devicetype,	recorder,	recstatus,	reccreated,	recupdated,
	                                                reccreator,	recupdator,		rectype,	relationvalue,
	                                                routepath,	islastchild,	childtype
                                                )
                                                VALUES
	                                                (
		                                                '{0}'::uuid,'报表','sss','{1}'::uuid,'{2}'::uuid,
		                                                '0','0','1',now(),now(),
		                                                '7','7','0','',
		                                                '',	'0',NULL
	                                                )", FunctionReportRootIdForMob, FunctionMobRootId, ReportMenuEntityId);
                    ExecuteNonQuery(cmdText, new DbParameter[] { }, trans);
                    cmdText = string.Format("select crm_func_repairefunctiontreepath('{0}')", FunctionReportRootIdForMob);
                    ExecuteNonQuery(cmdText, new DbParameter[] { }, trans);
                }

            }
            catch (Exception ex)
            {

            }
            #endregion

            #region 插入未发布的报表的权限项
            try
            {
                string cmdText = string.Format(@"INSERT INTO crm_sys_function(
	                                                funcid,	funcname,	funccode,	parentid,	entityid,
	                                                devicetype,	recorder,	recstatus,	reccreated,	recupdated,
	                                                reccreator,	recupdator,		rectype,	relationvalue,
	                                                routepath,	islastchild,	childtype
                                                )
                                                VALUES
	                                                (
		                                                '{0}'::uuid,'未发布报表','sss','{1}'::uuid,'{2}'::uuid,
		                                                '0','0','1',now(),now(),
		                                                '7','7','0','',
		                                                '',	'0',NULL
	                                                )", FunctionReportNotDeployIdForMob, FunctionReportRootIdForMob, ReportMenuEntityId);
                ExecuteNonQuery(cmdText, new DbParameter[] { }, trans);
                cmdText = string.Format("select crm_func_repairefunctiontreepath('{0}')", FunctionReportNotDeployIdForMob);
                ExecuteNonQuery(cmdText, new DbParameter[] { }, trans);
            }
            catch (Exception ex)
            {
            }
            #endregion

            transferReportFolder2Function(trans, MobileReportRootFolderId, userNum);

            #region 处理未发布的表表
            try
            {
                string cmdText = string.Format(@"select recid as id ,recname as name  
                                                from crm_sys_reportdefine 
                                               where recid not in (select reportid from crm_sys_reportfolder )
                                                     and recstatus = 1
                                                order by reccreated");
                List<Dictionary<string, object>> data = ExecuteQuery(cmdText, new DbParameter[] { }, trans);
                int index = 0;
                foreach (Dictionary<string, object> item in data)
                {
                    transferReport2Function(trans, item, index, FunctionReportNotDeployIdForMob, userNum);
                    index++;
                }
            }
            catch (Exception ex)
            {
            }
            #endregion 

        }

        public void repairWebMenuForReport(DbTransaction tran, int userNum) {
            try
            {
                getAndDeleteWebMenu(tran, WebMenu_ReportRootId, userNum);
                #region 检查并生成报表主入口
                string cmdText = "Select id::text from crm_sys_webmenu where id='" + WebMenu_ReportRootId + "'";
                string id = (string)ExecuteScalar(cmdText, new DbParameter[] { }, tran);
                if (id == null || id.Length == 0) {
                    cmdText = string.Format(@"INSERT INTO crm_sys_webmenu (
	                                                id,index,	name,path,funcid,parentid,isdynamic
                                                )values('{0}',{1},'{2}','{3}','{4}',1)", WebMenu_ReportRootId, 9, "统计报表", "", "");
                    ExecuteNonQuery(cmdText, new DbParameter[] { }, tran);
                }
                #endregion

                checkAndInsertWebMenu(tran, WebMenu_ReportRootId, WebReportRootFolderId, userNum);
            }
            catch (Exception ex) {

            }
        }
        private void checkAndInsertWebMenu(DbTransaction trans, string menuParentId, string reportFolderID, int userNum) {
            try
            {
                string cmdText = "select * from crm_sys_reportfolder where parentid = '" + reportFolderID + "' order by index ";
                List<Dictionary<string, object>> data = ExecuteQuery(cmdText, new DbParameter[] { }, trans);
                int index = 0;
                foreach (Dictionary<string, object> item in data)
                {
                    InsertWebMenu(trans, item, index, menuParentId, userNum);
                    if ((bool)item["isfolder"])
                    {
                        checkAndInsertWebMenu(trans, item["id"].ToString(), item["id"].ToString(), userNum);
                    }
                    index++;
                }
            }
            catch (Exception ex)
            {
            }
        }
        private void InsertWebMenu(DbTransaction trans, Dictionary<string, object> item, int index,string parentid, int userNum) {
            string funcid = "";
            string path = "";
            if (!(bool)item["isfolder"])
            {
                funcid = item["id"].ToString();
                path = string.Format("/reportform/{0}", funcid);
            }
            try
            {
                string cmdText = string.Format(@"INSERT INTO crm_sys_webmenu (
	                                                id,index,	name,path,funcid,parentid,isdynamic
                                                )values('{0}',{1},'{2}','{3}','{4}','{5}',1)", item["id"].ToString(), index, item["name"].ToString(), path, funcid, parentid);
                ExecuteNonQuery(cmdText, new DbParameter[] { }, trans);
            }
            catch (Exception ex) {


            }

        }
        private void getAndDeleteWebMenu(DbTransaction trans, string parentFolder, int userNum) {
            try
            {
                string cmdText = string.Format(@"select * from crm_sys_webmenu where parentid = '{0}'", parentFolder);
                string ids = "";
                List<Dictionary<string, object>> data = ExecuteQuery(cmdText, new DbParameter[] { }, trans);
                foreach (Dictionary<string, object> item in data) {
                    getAndDeleteWebMenu(trans, item["id"].ToString(), userNum);
                    ids = ids + ",'" + item["id"].ToString() + "'";
                }
                if (ids.Length > 0) {
                    ids = ids.Substring(1);
                    cmdText = string.Format(@"delete from  crm_sys_webmenu where id in({0})", ids);
                    ExecuteNonQuery(cmdText, new DbParameter[] { }, trans);
                }
            }
            catch (Exception ex) {
            }
        }

        public string getRuleSQLByRuleId(string entityid, string ruleid, int userNum, DbTransaction tran)
        {
            try
            {
                string cmdText = string.Format(@"select crm_func_rule_fetch_sql('{0}'::uuid,'{1}'::uuid,{2})", entityid, ruleid, userNum);
                return (string)ExecuteScalar(cmdText, new DbParameter[] { }, tran);
            }
            catch (Exception ex) {
            }
            return null;
        }

        public string getRuleSQLByUserId(string entityid, int userNum, DbTransaction tran)
        {
            try
            {
                try
                {
                    string cmdText = string.Format(@"select crm_func_role_rule_fetch_sql_withalias('{0}'::uuid,'rulecheck',{1})", entityid, userNum);
                    return (string)ExecuteScalar(cmdText, new DbParameter[] { }, tran);
                }
                catch (Exception ex)
                {
                }
            }
            catch (Exception ex)
            {
            }
            return null;
        }

        public Dictionary<string, object> getMyRangeWithType(int userNum, DbTransaction tran)
        {
            try
            {
                Dictionary<string, object> ret = new Dictionary<string, object>();
                string strSQL = @"select userinfo.userid::text,userInfo.isleader::int4 ,userDept.deptid::text
                                from (
                                select * from crm_sys_userinfo where recstatus = 1 
                                ) userInfo 
                                inner join (
	                                select * from 
	                                crm_sys_account_userinfo_relate 
	                                where recstatus = 1 
                                ) userdept on userdept.userid = userInfo.userid 
                                where  userInfo.userid = " + userNum.ToString();
                List<Dictionary<string, object>> detailList = this.ExecuteQuery(strSQL, new DbParameter[] { }, tran);
                if (detailList != null && detailList.Count > 0) {
                    Dictionary<string, object> detail = detailList[0];
                    if (detail.ContainsKey("isleader") == false || detail["isleader"] == null) {
                        detail["isleader"] = 0;
                    }
                    if (detail.ContainsKey("deptid") == false || detail["deptid"] == null)
                    {
                        detail["deptid"] = "";
                    }
                    return detail;
                }
            }
            catch (Exception ex) {
            }
            return null;
        }
    }
}