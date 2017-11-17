using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace UBeat.Crm.CoreApi.Repository.Utility
{
    public interface IDBHelper
    {
        DbConnection GetDbConnect(string connectStr = null);

        /// <summary>
        /// 执行 Transact-SQL 语句并返回受影响的行数。
        /// </summary>
        int ExecuteNonQuery(string connectionString, string cmdText, DbParameter[] cmdParms, CommandType cmdType = CommandType.Text);

        /// <summary>
        /// 在事务中执行 Transact-SQL 语句并返回受影响的行数。
        /// </summary>
        int ExecuteNonQuery(DbTransaction trans, string cmdText, DbParameter[] cmdParms, CommandType cmdType = CommandType.Text);

        #region --执行查询--
        /// <summary>
        /// 在事务中执行查询，返回单结果集
        /// </summary>
        List<Dictionary<string, object>> ExecuteQuery(DbTransaction trans, string cmdText, DbParameter[] cmdParms, CommandType cmdType = CommandType.Text);

        /// <summary>
        /// 执行查询，返回单结果集
        /// </summary>
        List<Dictionary<string, object>> ExecuteQuery(string connectionString, string cmdText, DbParameter[] cmdParms, CommandType cmdType = CommandType.Text);

        /// <summary>
        /// 执行查询，返回单结果集
        /// </summary>
        List<T> ExecuteQuery<T>(string connectionString, string cmdText, DbParameter[] cmdParms, CommandType cmdType = CommandType.Text) where T : new();

        /// <summary>
        /// 在事务中执行查询，返回单结果集
        /// </summary>
        List<T> ExecuteQuery<T>(DbTransaction trans, string cmdText, DbParameter[] cmdParms, CommandType cmdType = CommandType.Text) where T : new();

        /// <summary>
        /// 在事务中执行查询，返回多结果集
        /// </summary>
        List<List<Dictionary<string, object>>> ExecuteQueryMultiple(DbTransaction trans, string cmdText, DbParameter[] cmdParms, CommandType cmdType = CommandType.Text);

        /// <summary>
        /// 执行查询，返回多结果集
        /// </summary>
        List<List<Dictionary<string, object>>> ExecuteQueryMultiple(string connectionString, string cmdText, DbParameter[] cmdParms, CommandType cmdType = CommandType.Text);

        /// <summary>
        /// 在事务中执行查询，返回多结果集
        /// </summary>
        List<List<T>> ExecuteQueryMultiple<T>(DbTransaction trans, string cmdText, DbParameter[] cmdParms, CommandType cmdType = CommandType.Text) where T : new();

        /// <summary>
        /// 执行查询，返回多结果集
        /// </summary>
        List<List<T>> ExecuteQueryMultiple<T>(string connectionString, string cmdText, DbParameter[] cmdParms, CommandType cmdType = CommandType.Text) where T : new();



        #endregion

        #region --执行游标查询--
        /// <summary>
        /// 在事务中执行游标查询，返回多结果集
        /// </summary>
        Dictionary<string, List<Dictionary<string, object>>> ExecuteQueryRefCursor(DbTransaction trans, string cmdText, DbParameter[] cmdParms, CommandType cmdType = CommandType.Text);

        /// <summary>
        /// 执行游标查询，返回多结果集
        /// </summary>
        Dictionary<string, List<Dictionary<string, object>>> ExecuteQueryRefCursor(string connectionString, string cmdText, DbParameter[] cmdParms, CommandType cmdType = CommandType.Text);

        /// <summary>
        /// 执行游标查询，返回多结果集
        /// </summary>
        Dictionary<string, List<T>> ExecuteQueryRefCursor<T>(string connectionString, string cmdText, DbParameter[] cmdParms, CommandType cmdType = CommandType.Text) where T : new();

        /// <summary>
        /// 在事务中执行游标查询，返回多结果集
        /// </summary>
        Dictionary<string, List<T>> ExecuteQueryRefCursor<T>(DbTransaction trans, string cmdText, DbParameter[] cmdParms, CommandType cmdType = CommandType.Text) where T : new();
        #endregion

        /// <summary>
        /// 在事务中执行查询，返回DataReader
        /// </summary>
        DbDataReader ExecuteReader(DbTransaction trans, string cmdText, DbParameter[] cmdParms, CommandType cmdType = CommandType.Text);

       

        /// <summary>
        /// 在事务中执行查询，并返回查询所返回的结果集中第一行的第一列。忽略其他列或行。
        /// </summary>
        object ExecuteScalar(DbTransaction trans, string cmdText, DbParameter[] cmdParms, CommandType cmdType = CommandType.Text);

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列。忽略其他列或行。
        /// </summary>
        object ExecuteScalar(string connectionString, string cmdText, DbParameter[] cmdParms, CommandType cmdType = CommandType.Text);

        /// <summary>
        /// 得到数据条数
        /// </summary>
        /// <param name="tblName">表名</param>
        /// <param name="condition">条件(不需要where)</param>
        /// <returns>数据条数</returns>
        int GetCount(string connectionString, string tblName, string condition, DbParameter[] cmdParms);

        /// <summary>
        /// 得到数据条数
        /// </summary>
        /// <param name="tblName">表名</param>
        /// <param name="condition">条件(不需要where)</param>
        /// <returns>数据条数</returns>
        int GetCount(DbTransaction trans, string tblName, string condition, DbParameter[] cmdParms);
    }
}
