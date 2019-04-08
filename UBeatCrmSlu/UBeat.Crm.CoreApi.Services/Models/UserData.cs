using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.Account;
using UBeat.Crm.CoreApi.DomainModel.Role;
using UBeat.Crm.CoreApi.DomainModel.Rule;
using UBeat.Crm.CoreApi.DomainModel.Version;
using UBeat.Crm.CoreApi.DomainModel.Vocation;

using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Repository.Rule;
using UBeat.Crm.CoreApi.Services.Utility;
using UBeat.Crm.CoreApi.Repository.Repository.Role;
using UBeat.Crm.CoreApi.Repository.Repository.Vocation;
using UBeat.Crm.CoreApi.Services.Services;

namespace UBeat.Crm.CoreApi.Services.Models
{
    public class UserData
    {
        public int UserId { set; get; }

        /// <summary>
        /// 个人账号信息
        /// </summary>
        public AccountUserInfo AccountUserInfo { set; get; }

        public List<VocationInfo> Vocations { set; get; } = new List<VocationInfo>();

        public List<RoleInfo> Roles { set; get; } = new List<RoleInfo>();


        #region --获取用户大版本号的数据--
        /// <summary>
        /// 获取用户大版本号的数据
        /// </summary>
        /// <param name="allList"></param>
        /// <returns></returns>
        public VersionData GetUserVersionData(List<DataVersionInfo> allList)
        {
            if (allList == null)
                return null;

            var userList = allList.Where(m => m.UserId == 0 || m.UserId == UserId);
            var versionData = new VersionData();
            foreach (var m in userList)
            {
                switch (m.DataType)
                {
                    case DataVersionType.BasicData:
                        versionData.BasicData = m.MaxVersion;
                        break;
                    case DataVersionType.MsgData:
                        versionData.MsgData = m.MaxVersion;
                        break;
                    case DataVersionType.DicData:
                        versionData.DicData = m.MaxVersion;
                        break;
                    case DataVersionType.EntityData:
                        versionData.EntityData = m.MaxVersion;
                        break;
                    case DataVersionType.FlowData:
                        versionData.FlowData = m.MaxVersion;
                        break;
                    case DataVersionType.PowerData:
                        versionData.PowerData = m.MaxVersion;
                        break;
                    case DataVersionType.ProductData:
                        versionData.ProductData = m.MaxVersion;
                        break;
                    case DataVersionType.TrackSettingData:
                        versionData.TrackSettingData = m.MaxVersion;
                        break;
                }
            }
            return versionData;


        }


        #endregion



        #region --判断是否拥有某个功能--
        /// <summary>
        /// 判断是否拥有某个功能
        /// </summary>
        /// <param name="routePath"></param>
        /// <param name="entityid"></param>
        /// <returns></returns>
        public bool HasFunction(string routePath, Guid entityid, DeviceClassic deviceClassic)
        {

            if (routePath == null)
                return false;

            bool result = false;
            if (Vocations != null && Vocations.Count > 0)
            {
                if (entityid == Guid.Empty)
                {
                    result = Vocations.Exists(m => m.Functions != null && m.Functions.Exists(a => a.RoutePath != null && a.RoutePath.Trim().Trim('/').Equals(routePath)));
                }
                else
                {
                    result = Vocations.Exists(m => m.Functions != null && m.Functions.Exists(a => a.RoutePath != null && a.DeviceType == (int)deviceClassic && a.RoutePath.Trim().Trim('/').Equals(routePath) && a.EntityId == entityid));
                }
            }
            return result;
        }
        #endregion
        /***
         * 根据Funcid判断有没有权限
        **/
        public bool HasFunctionById(string funcid)
        {

            if (funcid == null)
                return false;

            bool result = false;
            if (Vocations != null && Vocations.Count > 0)
            {
                result = Vocations.Exists(m => m.Functions != null && m.Functions.Exists(a => a.FuncId != null && a.FuncId.Equals(funcid)));
            }
            return result;
        }



        #region --判断是否有某些数据的操作权限--
        /// <summary>
        /// 判断是否有某些数据的操作权限
        /// </summary>
        /// <param name="tran"></param>
        /// <param name="routePath"></param>
        /// <param name="entityid"></param>
        /// <param name="deviceClassic"></param>
        /// <param name="recids"></param>
        /// <returns></returns>
        public bool HasDataAccess(DbTransaction tran, string routePath, Guid entityid, DeviceClassic deviceClassic, List<Guid> recids, string recidFieldName = "recid")
        {
            if (routePath == null)
                return false;
            string ruleSql = string.Empty;
            if (routePath.StartsWith("/")) routePath = routePath.Substring(1);
            ruleSql = BasicRuleSqlFormat(routePath, entityid, deviceClassic);

            IRuleRepository repository = new RuleRepository();
            bool result = repository.HasDataAccess(tran, ruleSql, entityid, recids, recidFieldName);
            if (result)
            {
                if (routePath == null)
                    return false;
                ruleSql = string.Empty;
                if (routePath.StartsWith("/")) routePath = routePath.Substring(1);
                ruleSql = RuleSqlFormat(routePath, entityid, deviceClassic);

                repository = new RuleRepository();
                return repository.HasDataAccess(tran, ruleSql, entityid, recids, recidFieldName);
            }
            return result;
        }


        #endregion

        #region --格式化RuleSql--
        /// <summary>
        /// 格式化RuleSql
        /// </summary>
        /// <param name="routePath"></param>
        /// <param name="entityid"></param>
        /// <param name="deviceClassic"></param>
        /// <returns></returns>
        public string RuleSqlFormat(string routePath, Guid entityid, DeviceClassic deviceClassic)
        {
            string functionRuleSql = null;
            StringBuilder roleRuleSql = new StringBuilder();
            if (Vocations != null && Vocations.Count > 0)
            {
                FunctionInfo functionInfo = null;
                foreach (var vocation in Vocations)
                {
                    if (vocation.Functions != null)
                    {
                        if (entityid == Guid.Empty)
                            functionInfo = vocation.Functions.Find(a => a.RoutePath != null && a.RoutePath.Trim().Trim('/').Equals(routePath)
                            && a.DeviceType == (int)deviceClassic);
                        else
                            functionInfo = vocation.Functions.Find(a => a.RoutePath != null && a.RoutePath.Trim().Trim('/').Equals(routePath)
                             && a.DeviceType == (int)deviceClassic && a.EntityId == entityid);

                        if (functionInfo != null)
                        {
                            string temp = string.Empty;
                            if (functionInfo.Rule != null && !string.IsNullOrEmpty(functionInfo.Rule.Rulesql))
                                temp = functionInfo.Rule.Rulesql;
                            else temp = "1=1";
                            if (string.IsNullOrEmpty(functionRuleSql))
                                functionRuleSql = temp;
                            else
                                functionRuleSql = string.Format("{0} OR {1}", functionRuleSql, temp);
                        }
                    }
                }


            }
            //获取角色权限，取最大权限的角色rule，假如有个角色没有rule或者rule的sql为空，则没有角色权限限制
            if (Roles != null && Roles.Count > 0)
            {

                foreach (var role in Roles)
                {
                    if (roleRuleSql.Length > 0)
                        roleRuleSql.Append(" OR ");
                    var rule = role.Rules.Find(a => a.EntityId == entityid);
                    // 如果 没有rule，或者rule的sql为空，则表示角色不过滤权限
                    if (rule == null || string.IsNullOrEmpty(rule.Rulesql))
                    {
                        roleRuleSql.Append("1=1");
                    }
                    else roleRuleSql.Append(rule.Rulesql);
                }
            }
            functionRuleSql = string.IsNullOrEmpty(functionRuleSql) ? "1=1" : string.Format("({0})", functionRuleSql);
            var roleRuleSqlString = roleRuleSql.ToString();
            roleRuleSqlString = string.IsNullOrEmpty(roleRuleSqlString) ? "1=1" : string.Format("({0})", roleRuleSqlString);

            var sql = string.Format("({0}) AND ({1})", functionRuleSql, roleRuleSqlString);

            if (AccountUserInfo == null)
                return null;
            return RuleSqlHelper.FormatRuleSql(sql, AccountUserInfo.UserId, AccountUserInfo.DepartmentId);
        }

        /// <summary>
        /// 格式化RuleSql
        /// </summary>
        /// <param name="routePath"></param>
        /// <param name="entityid"></param>
        /// <param name="deviceClassic"></param>
        /// <returns></returns>
        public string BasicRuleSqlFormat(string routePath, Guid entityid, DeviceClassic deviceClassic)
        {
            string functionRuleSql = null;
            StringBuilder roleRuleSql = new StringBuilder();
            if (Vocations != null && Vocations.Count > 0)
            {
                FunctionInfo functionInfo = null;
                foreach (var vocation in Vocations)
                {
                    if (vocation.Functions != null)
                    {
                        if (entityid == Guid.Empty)
                            functionInfo = vocation.Functions.Find(a => a.RoutePath != null && a.RoutePath.Trim().Trim('/').Equals(routePath)
                            && a.DeviceType == (int)deviceClassic);
                        else
                            functionInfo = vocation.Functions.Find(a => a.RoutePath != null && a.RoutePath.Trim().Trim('/').Equals(routePath)
                             && a.DeviceType == (int)deviceClassic && a.EntityId == entityid);

                        if (functionInfo != null)
                        {
                            string temp = string.Empty;
                            if (functionInfo.BasicRule != null && !string.IsNullOrEmpty(functionInfo.BasicRule.Rulesql))
                                temp = functionInfo.BasicRule.Rulesql;
                            else temp = "1=1";
                            if (string.IsNullOrEmpty(functionRuleSql))
                                functionRuleSql = temp;
                            else
                                functionRuleSql = string.Format("{0} OR {1}", functionRuleSql, temp);
                        }
                    }
                }
            }
            functionRuleSql = string.IsNullOrEmpty(functionRuleSql) ? "1=1" : string.Format("({0})", functionRuleSql);
            var sql = string.Format("({0})", functionRuleSql);
        
            if (AccountUserInfo == null)
                return null;
            return RuleSqlHelper.FormatRuleSql(sql, AccountUserInfo.UserId, AccountUserInfo.DepartmentId);
        }
		#endregion

		public string RuleSqlFormatForFunction(FunctionInfo functionInfo)
		{
			if (functionInfo == null) return string.Empty;

			var functionRuleSql = string.Empty;
			if (functionInfo.Rule != null && !string.IsNullOrEmpty(functionInfo.Rule.Rulesql))
				functionRuleSql = functionInfo.Rule.Rulesql;
			else
				return string.Empty;

			var sql = string.Format("({0})", functionRuleSql);
			if (AccountUserInfo == null)
				return string.Empty;
			return RuleSqlHelper.FormatRuleSql(sql, AccountUserInfo.UserId, AccountUserInfo.DepartmentId);
		}

		public string RuleSqlFormatForRelTab(FunctionInfo functionInfo)
		{
			var functionRuleSql = string.Empty;
			string temp = string.Empty;
			if (functionInfo.Rule != null && !string.IsNullOrEmpty(functionInfo.Rule.Rulesql))
				temp = functionInfo.Rule.Rulesql;
			else temp = "1=1";
			if (string.IsNullOrEmpty(functionRuleSql))
				functionRuleSql = temp;
			else
				functionRuleSql = string.Format("{0} OR {1}", functionRuleSql, temp);


			var sql = string.Format("({0})", functionRuleSql);
			if (AccountUserInfo == null)
				return null;
			return RuleSqlHelper.FormatRuleSql(sql, AccountUserInfo.UserId, AccountUserInfo.DepartmentId);
		}
	}   
}
