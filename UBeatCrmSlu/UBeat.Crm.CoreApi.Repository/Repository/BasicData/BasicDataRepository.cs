using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.BasicData;
using UBeat.Crm.CoreApi.DomainModel.Utility;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Utility;

namespace UBeat.Crm.CoreApi.Repository.Repository.BasicData
{
    public class BasicDataRepository : IBasicDataRepository
    {
        public Dictionary<string, List<IDictionary<string, object>>> GetMessageList(PageParam pageParam,
            NotifyMessageMapper searchParam, int userNumber)
        {
            var procName =
                "SELECT crm_func_notify_message_list(@recversion, @logintype, @pageindex, @pagesize, @userno)";

            var dataNames = new List<string> { "Messages" };
            var param = new
            {
                RecVersion = searchParam.RecVersion,
                LoginType = searchParam.LoginType,
                PageIndex = 1,
                PageSize = 1,
                UserNo = userNumber
            };

            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }

        public Dictionary<string, List<IDictionary<string, object>>> SyncData(SyncDataMapper versionMapper,
            int userNumber)
        {
            //获取第一个基础数据
            var procName =
                "SELECT crm_func_basicdata_sync(@config, @userno)";

            var dataNames = new List<string> { "dept", "region", "version", "datadic" };

            var versionObj = versionMapper.VersionKey;
            var versionJson = JsonHelper.ToJson(versionObj);

            var param = new
            {
                Config = versionJson,
                UserNo = userNumber
            };

            var dataDic = new Dictionary<string, List<IDictionary<string, object>>>();
            var resultOne = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            foreach (KeyValuePair<string, List<IDictionary<string, object>>> pair in resultOne)
            {
                if (!dataDic.ContainsKey(pair.Key))
                {
                    dataDic.Add(pair.Key, pair.Value);
                }
            }

            //获取第二个基础数据
            procName =
                "SELECT crm_func_basicdata_sync_field(@config, @userno)";

            dataNames = new List<string> { "entity", "entityfield", "entitycategory", "entityfieldrule" };

            var resultTwo = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            foreach (KeyValuePair<string, List<IDictionary<string, object>>> pair in resultTwo)
            {
                if (!dataDic.ContainsKey(pair.Key))
                {
                    dataDic.Add(pair.Key, pair.Value);
                }
            }

            //获取第三个基础数据
            procName =
                "SELECT crm_func_basicdata_sync_field_view(@config, @userno)";

            dataNames = new List<string> { "entitymenu", "entitysearch", "entitylistview", "entitypageconf", "version", "menuentry" };

            var resultTrd = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            foreach (KeyValuePair<string, List<IDictionary<string, object>>> pair in resultTrd)
            {
                if (!dataDic.ContainsKey(pair.Key))
                {
                    dataDic.Add(pair.Key, pair.Value);
                }
            }

            return dataDic;
        }

        public List<IDictionary<string, object>> DeptData(DeptDataMapper deptMapper, int userNumber)
        {
            var procName =
                "SELECT * FROM crm_func_department_tree(@deptId,@direction)";

            var dataNames = new List<string> { "Dept" };

            var param = new
            {
                DeptId = deptMapper.DeptId,
                Direction = deptMapper.Direction
            };

            var result = DataBaseHelper.Query(procName, param);

            return result;
        }
        public List<IDictionary<string, object>> DeptPowerData(DeptDataMapper deptMapper, int userNumber)
        {
            var procName =
                "SELECT * FROM crm_func_department_tree_power(@deptId,@status,@direction,@userno)";

            var dataNames = new List<string> { "Dept" };

            var param = new
            {
                DeptId = deptMapper.DeptId,
                Status = deptMapper.Status,
                Direction = deptMapper.Direction,
                UserNo = userNumber
            };

            var result = DataBaseHelper.Query(procName, param);

            return result;
        }

        public Dictionary<string, List<IDictionary<string, object>>> UserContactList(PageParam pageParam,
            BasicDataUserContactListMapper searchParm, int userNumber)
        {
            var procName =
                "SELECT crm_func_account_userinfo_contact_list(@versionId,@pageIndex,@pageSize,@userNo)";

            var dataNames = new List<string> { "PageData", "PageCount", "Version" };

            var param = new
            {
                VersionId = searchParm.RecVersion,
                PageIndex = pageParam.PageIndex,
                PageSize = pageParam.PageSize,
                UserNo = userNumber
            };

            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }

        public Dictionary<string, List<IDictionary<string, object>>> SyncDataBasic(SyncDataMapper versionMapper,
            int userNumber)
        {
            //获取第一个基础数据
            var procName =
                "SELECT crm_func_basicdata_sync_basic(@config, @userno)";

            var dataNames = new List<string> { "dept", "region", "datadic", "version" };

            var versionObj = versionMapper.VersionKey;
            var versionJson = JsonHelper.ToJson(versionObj);

            var param = new
            {
                Config = versionJson,
                UserNo = userNumber
            };

            var resultOne = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);

            return resultOne;
        }

        public Dictionary<string, List<IDictionary<string, object>>> SyncDataEntity(SyncDataMapper versionMapper,
            int userNumber)
        {
            var procName =
                "SELECT crm_func_basicdata_sync_field(@config, @userno)";

            var dataNames = new List<string> { "entity", "entityfield", "entitycategory", "entityfieldrule", "entityfieldrulevo", "version" };

            //获取第一个基础数据
            var versionObj = versionMapper.VersionKey;
            var versionJson = JsonHelper.ToJson(versionObj);

            var param = new
            {
                Config = versionJson,
                UserNo = userNumber
            };

            //获取第二个基础数据

            var resultTwo = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);

            return resultTwo;
        }
        public List<IDictionary<string, object>> SyncDelDataEntity( int userNumber)
        {
            var procName =
                "SELECT crm_func_del_entity_sync(@userno)";

            var param = new
            {
                UserNo = userNumber
            };

            var resultTwo = DataBaseHelper.QueryStoredProcCursor(procName, param, CommandType.Text);

            return resultTwo;
        }
        public Dictionary<string, List<IDictionary<string, object>>> SyncDataView(SyncDataMapper versionMapper,
            int userNumber)
        {
            //获取第三个基础数据
            var procName =
                "SELECT crm_func_basicdata_sync_field_view(@config, @userno)";

            var dataNames = new List<string> { "entitymenu", "entitysearch", "entitylistview", "entitypageconf", "version", "menuentry", "entitycompoment", "entityrelate" };

            var versionObj = versionMapper.VersionKey;
            var versionJson = JsonHelper.ToJson(versionObj);

            var param = new
            {
                Config = versionJson,
                UserNo = userNumber
            };

            var resultTrd = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);

            return resultTrd;
        }

        public Dictionary<string, List<IDictionary<string, object>>> SyncDataTemplate(SyncDataMapper versionMapper, int userNumber)
        {
            //获取第三个基础数据
            var procName =
                "SELECT crm_func_basicdata_sync_template(@config, @userno)";

            var dataNames = new List<string> { "template", "msggroup", "yearweek", "version", "funcactive" };

            var versionObj = versionMapper.VersionKey;
            var versionJson = JsonHelper.ToJson(versionObj);

            var param = new
            {
                Config = versionJson,
                UserNo = userNumber
            };

            var resultTrd = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);

            return resultTrd;
        }

        public dynamic FuncCount(int userNumber)
        {
            var procName =
             "SELECT crm_func_analyse_func_countdata(@userno)";

            var dataNames = new List<string> { "CountData" };

            var param = new
            {
                UserNo = userNumber
            };

            var resultTrd = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            object[] array = { };
            if (resultTrd["CountData"].FirstOrDefault() != null)
                array = resultTrd["CountData"].FirstOrDefault().Values.ToArray();
            return new { CountData = array };
        }

        public Dictionary<string, List<IDictionary<string, object>>> FuncCountList(PageParam pageParam, Guid anaFuncId, int userNumber)
        {
            var procName =
           "SELECT crm_func_analyse_func_count_list_data(@anaFuncId,@pageIndex,@pageSize,@userno)";

            var dataNames = new List<string> { "PageData", "PageCount", "Columns" };

            var param = new
            {
                AnaFuncId = anaFuncId,
                PageIndex = pageParam.PageIndex,
                PageSize = pageParam.PageSize,
                UserNo = userNumber
            };

            var resultTrd = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);

            return resultTrd;
        }


        #region 统计指标
        public Dictionary<string, List<IDictionary<string, object>>> AnalyseFuncQuery(AnalyseListMapper entity, int userNumber)
        {
            var procName =
                "SELECT crm_func_analyse_func_list(@anafuncname,@pageindex,@pagesize,@userno)";

            var dataNames = new List<string> { "AnalyseFuncList" };
            var param = new DynamicParameters();
            param.Add("anafuncname", entity.AnafuncName);
            param.Add("pageindex", entity.PageIndex);
            param.Add("pagesize", entity.PageSize);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }

        public OperateResult InsertAnalyseFunc(AddAnalyseMapper entity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_analyse_func_add(@anafuncname,@moreflag,@countfunc,@morefunc, @userno)
            ";
            var param = new DynamicParameters();
            param.Add("anafuncname", entity.AnafuncName);
            param.Add("moreflag", entity.MoreFlag);
            param.Add("countfunc", entity.CountFunc);
            param.Add("morefunc", entity.MoreFunc);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult UpdateAnalyseFunc(EditAnalyseMapper entity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_analyse_func_edit(@anafuncid,@anafuncname,@moreflag,@countfunc,@morefunc, @userno)
            ";
            var param = new DynamicParameters();
            param.Add("anafuncid", entity.AnafuncId);
            param.Add("anafuncname", entity.AnafuncName);
            param.Add("moreflag", entity.MoreFlag);
            param.Add("countfunc", entity.CountFunc);
            param.Add("morefunc", entity.MoreFunc);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult DisabledAnalyseFunc(DisabledOrOderbyAnalyseMapper entity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_analyse_func_disabled(@anafuncids, @userno)
            ";
            var param = new DynamicParameters();
            param.Add("anafuncids", entity.AnafuncIds);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult OrderByAnalyseFunc(DisabledOrOderbyAnalyseMapper entity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_analyse_func_orderby(@anafuncids,@userno)
            ";

            var param = new DynamicParameters();
            param.Add("anafuncids", entity.AnafuncIds);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }
        #endregion
    }
}