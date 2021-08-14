using System;
using System.Collections.Generic;
using System.Text;

using System.Data.Common;
using System.Data;
using UBeat.Crm.CoreApi.DomainModel.Reports;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IReportEngineRepository
    {
        Dictionary<string, List<Dictionary<string, object>>> queryDataFromDataSource_CommonSQL(DbTransaction transaction, string sql, Dictionary<string, object> param);
        Dictionary<string, List<Dictionary<string, object>>> queryDataFromDataSource_FuncSQL(DbTransaction transaction, string funcName, string paramDefined,Dictionary<string, object> param);
        List<Dictionary<string, object>> ExecuteSQL(string cmdText, DbParameter[] dbParam);
        List<ReportFolderInfo> queryWebReportList();
        List<ReportFolderInfo> queryMobileReportList();
        string getEntityIdByDataSourceId(string datasourceid, int userNum);
        void repairWebReportFunctions(DbTransaction trans, int userNum);
        void repairMobReportFunctions(DbTransaction trans, int userNum);
        void repairWebMenuForReport(DbTransaction tran, int userNum);
        string getRuleSQLByRuleId(string entityid, string ruleid, int userNum, DbTransaction tran);
        string getRuleSQLByUserId(string entityid, int userNum, DbTransaction tran);
        /// <summary>
        /// 获取指定用户的报表默认查询范围，如果是部门领导，则返回
        /// rangetype=1,range=我负责的部门id
        /// 如果非部门领导，则返回：
        /// rangetype=2,range=我的id
        /// </summary>
        /// <param name="userNum"></param>
        /// <param name="tran"></param>
        /// <returns></returns>
        Dictionary<string, object> getMyRangeWithType(int userNum, DbTransaction tran);

        string getTopDeptId(int userNum);
        string getTopDeptName(int userNum);
        string getMyDeptId(int userNum);
        string getMyDeptName(int userNum);
    }
}
