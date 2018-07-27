using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using UBeat.Crm.CoreApi.Core.Utility;

namespace UBeat.Crm.CoreApi.Repository.Utility
{
    /// <summary>
    /// 数据库操作基类(for PostgreSQL)
    /// </summary>
    public class PostgreHelper : IDBHelper
    {
        private string _connectString;
        public DbConnection GetDbConnect(string connectStr = null)
        {
            if (string.IsNullOrEmpty(connectStr))
            {
                if (_connectString == null)
                {
                    IConfigurationRoot config = ServiceLocator.Current.GetInstance<IConfigurationRoot>();
                    _connectString = config.GetConnectionString("DefaultDB");
                }

                connectStr = _connectString;
            }

            return new NpgsqlConnection(connectStr);
        }

        /// <summary>
        /// 得到数据条数
        /// </summary>
        public int GetCount(string connectionString, string tblName, string condition, DbParameter[] cmdParms)
        {
            StringBuilder sql = new StringBuilder("select count(1) from " + tblName);
            if (!string.IsNullOrEmpty(condition))
                sql.Append(" where " + condition);

            object count = ExecuteScalar(connectionString, sql.ToString(), cmdParms, CommandType.Text);
            return int.Parse(count.ToString());
        }

        public int GetCount(DbTransaction trans, string tblName, string condition, DbParameter[] cmdParms)
        {
            StringBuilder sql = new StringBuilder("select count(1) from " + tblName);
            if (!string.IsNullOrEmpty(condition))
                sql.Append(" where " + condition);

            object count = ExecuteScalar(trans, sql.ToString(), cmdParms, CommandType.Text);
            return int.Parse(count.ToString());
        }


        #region --执行查询--
        /// <summary>
        /// 在事务中执行查询，返回单结果集
        /// </summary>
        public List<Dictionary<string, object>> ExecuteQuery(DbTransaction trans, string cmdText, DbParameter[] cmdParms, CommandType cmdType = CommandType.Text)
        {

            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
            NpgsqlDataReader reader = (NpgsqlDataReader)ExecuteReader(trans, cmdText, cmdParms, cmdType);

            while (reader.Read())
            {
                Dictionary<string, object> rowData = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var value = reader.GetValue(i);
                    value = (value == DBNull.Value) ? null : value;
                    rowData.Add(reader.GetName(i), value);
                }
                rows.Add(rowData);

            }
            reader.Close();

            return rows;


        }

        /// <summary>
        /// 执行查询，返回单结果集
        /// </summary>
        public List<Dictionary<string, object>> ExecuteQuery(string connectionString, string cmdText, DbParameter[] cmdParms, CommandType cmdType = CommandType.Text)
        {
            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
            using (DbConnection conn = GetDbConnect(connectionString))
            {
                NpgsqlDataReader reader = (NpgsqlDataReader)ExecuteReader(conn, cmdText, cmdParms, cmdType);

                while (reader.Read())
                {
                    Dictionary<string, object> rowData = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var value = reader.GetValue(i);
                        value = (value == DBNull.Value) ? null : value;
                        rowData.Add(reader.GetName(i), value);
                    }
                    rows.Add(rowData);

                }
                reader.Close();
                conn.Close();
                return rows;
            }
        }

        /// <summary>
        /// 执行查询，返回单结果集
        /// </summary>
        public List<T> ExecuteQuery<T>(string connectionString, string cmdText, DbParameter[] cmdParms, CommandType cmdType = CommandType.Text) where T : new()
        {

            using (DbConnection conn = GetDbConnect(connectionString))
            {
                NpgsqlDataReader reader = (NpgsqlDataReader)ExecuteReader(conn, cmdText, cmdParms, cmdType);

                List<T> rows = ExecuteNpgsqlDataReader<T>(reader);

                reader.Close();
                conn.Close();
                return rows;
            }

        }



        /// <summary>
        /// 在事务中执行查询，返回单结果集
        /// </summary>
        public List<T> ExecuteQuery<T>(DbTransaction trans, string cmdText, DbParameter[] cmdParms, CommandType cmdType = CommandType.Text) where T : new()
        {

            NpgsqlDataReader reader = (NpgsqlDataReader)ExecuteReader(trans, cmdText, cmdParms, cmdType);

            List<T> rows = ExecuteNpgsqlDataReader<T>(reader);
            reader.Close();

            return rows;
        }

        /// <summary>
        /// 在事务中执行查询，返回多结果集
        /// </summary>
        public List<List<Dictionary<string, object>>> ExecuteQueryMultiple(DbTransaction trans, string cmdText, DbParameter[] cmdParms, CommandType cmdType = CommandType.Text)
        {
            List<List<Dictionary<string, object>>> tables = new List<List<Dictionary<string, object>>>();

            NpgsqlDataReader reader = (NpgsqlDataReader)ExecuteReader(trans, cmdText, cmdParms, cmdType);

            do
            {
                List<Dictionary<string, object>> tableData = new List<Dictionary<string, object>>();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {

                        Dictionary<string, object> rowData = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var value = reader.GetValue(i);
                            value = (value == DBNull.Value) ? null : value;
                            rowData.Add(reader.GetName(i), value);
                        }
                        tableData.Add(rowData);

                    }

                }
                tables.Add(tableData);
            }
            while (reader.NextResult());
            reader.Close();
            return tables;
        }

        /// <summary>
        /// 执行查询，返回多结果集
        /// </summary>
        public List<List<Dictionary<string, object>>> ExecuteQueryMultiple(string connectionString, string cmdText, DbParameter[] cmdParms, CommandType cmdType = CommandType.Text)
        {
            List<List<Dictionary<string, object>>> tables = new List<List<Dictionary<string, object>>>();

            using (DbConnection conn = GetDbConnect(connectionString))
            {
                NpgsqlDataReader reader = (NpgsqlDataReader)ExecuteReader(conn, cmdText, cmdParms, cmdType);

                do
                {
                    List<Dictionary<string, object>> tableData = new List<Dictionary<string, object>>();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {

                            Dictionary<string, object> rowData = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                var value = reader.GetValue(i);
                                value = (value == DBNull.Value) ? null : value;
                                rowData.Add(reader.GetName(i), value);
                            }
                            tableData.Add(rowData);

                        }
                    }
                    tables.Add(tableData);
                }
                while (reader.NextResult());
                reader.Close();
                conn.Close();
                return tables;
            }
        }
        /// <summary>
        /// 在事务中执行查询，返回多结果集
        /// </summary>
        public List<List<T>> ExecuteQueryMultiple<T>(DbTransaction trans, string cmdText, DbParameter[] cmdParms, CommandType cmdType = CommandType.Text) where T : new()
        {
            List<List<T>> tables = new List<List<T>>();


            NpgsqlDataReader reader = (NpgsqlDataReader)ExecuteReader(trans, cmdText, cmdParms, cmdType);

            do
            {
                List<T> tableData = new List<T>();
                if (reader.HasRows)
                {
                    tableData = ExecuteNpgsqlDataReader<T>(reader);

                }
                tables.Add(tableData);
            }
            while (reader.NextResult());
            reader.Close();
            return tables;
        }

        /// <summary>
        /// 执行查询，返回多结果集
        /// </summary>
        public List<List<T>> ExecuteQueryMultiple<T>(string connectionString, string cmdText, DbParameter[] cmdParms, CommandType cmdType = CommandType.Text) where T : new()
        {
            List<List<T>> tables = new List<List<T>>();


            using (DbConnection conn = GetDbConnect(connectionString))
            {

                NpgsqlDataReader reader = (NpgsqlDataReader)ExecuteReader(conn, cmdText, cmdParms, cmdType);
                do
                {
                    List<T> tableData = new List<T>();
                    if (reader.HasRows)
                    {
                        tableData = ExecuteNpgsqlDataReader<T>(reader);

                    }
                    tables.Add(tableData);
                }
                while (reader.NextResult());
                reader.Close();
                conn.Close();
                return tables;

            }
        }

        #endregion

        #region --执行游标查询--
        /// <summary>
        /// 在事务中执行游标查询，返回多结果集
        /// </summary>
        public Dictionary<string, List<Dictionary<string, object>>> ExecuteQueryRefCursor(DbTransaction trans, string cmdText, DbParameter[] cmdParms, CommandType cmdType = CommandType.Text)
        {
            var resultList = new Dictionary<string, List<Dictionary<string, object>>>();
            var resultsReferences = ExecuteQuery(trans, cmdText, cmdParms, cmdType);
            for (var index = 0; index < resultsReferences.Count; index++)
            {
                var resultsReference = resultsReferences[index];
                var resultSetName = resultsReference.Values.FirstOrDefault();
                var resultSetReferenceCmdText = string.Format("FETCH ALL IN \"{0}\";", resultSetName);

                var dataKey = resultSetName.ToString().Replace("cursor", "");

                var result = ExecuteQuery(trans, resultSetReferenceCmdText, null, CommandType.Text);
                var closeCursorSQL = string.Format("close \"{0}\";", resultSetName);
                ExecuteNonQuery(trans, closeCursorSQL, new DbParameter[] { });
                resultList.Add(dataKey, result);
            }
            return resultList;
        }

        /// <summary>
        /// 执行游标查询，返回多结果集
        /// </summary>
        public Dictionary<string, List<Dictionary<string, object>>> ExecuteQueryRefCursor(string connectionString, string cmdText, DbParameter[] cmdParms, CommandType cmdType = CommandType.Text)
        {
            var resultList = new Dictionary<string, List<Dictionary<string, object>>>();

            using (DbConnection conn = GetDbConnect(connectionString))
            {
                conn.Open();
                var tran = conn.BeginTransaction();
                var resultsReferences = ExecuteQuery(tran, cmdText, cmdParms, cmdType);

                for (var index = 0; index < resultsReferences.Count; index++)
                {
                    var resultsReference = resultsReferences[index];
                    var resultSetName = resultsReference.Values.FirstOrDefault();
                    var resultSetReferenceCmdText = string.Format("FETCH ALL IN \"{0}\";", resultSetName);

                    var dataKey = resultSetName.ToString().Replace("cursor", "");

                    var result = ExecuteQuery(tran, resultSetReferenceCmdText, null, CommandType.Text);
                    var closeCursorSQL = string.Format("close \"{0}\";", resultSetName);
                    ExecuteNonQuery(tran, closeCursorSQL, new DbParameter[] { });
                    resultList.Add(dataKey, result);
                }
                conn.Close();
                return resultList;
            }
        }

        /// <summary>
        /// 在事务中执行游标查询，返回多结果集
        /// </summary>
        public Dictionary<string, List<T>> ExecuteQueryRefCursor<T>(DbTransaction trans, string cmdText, DbParameter[] cmdParms, CommandType cmdType = CommandType.Text) where T : new()
        {
            var resultList = new Dictionary<string, List<T>>();
            var resultsReferences = ExecuteQuery(trans, cmdText, cmdParms, cmdType);
            for (var index = 0; index < resultsReferences.Count; index++)
            {
                var resultsReference = resultsReferences[index];
                var resultSetName = resultsReference.Values.FirstOrDefault();
                var resultSetReferenceCmdText = string.Format("FETCH ALL IN \"{0}\";", resultSetName);

                var dataKey = resultSetName.ToString().Replace("cursor", "");

                var result = ExecuteQuery<T>(trans, resultSetReferenceCmdText, null, CommandType.Text);

                var closeCursorSQL = string.Format("close \"{0}\";", resultSetName);
                ExecuteNonQuery(trans, closeCursorSQL, new DbParameter[] { });
                resultList.Add(dataKey, result);
            }
            return resultList;
        }

        /// <summary>
        /// 执行游标查询，返回多结果集
        /// </summary>
        public Dictionary<string, List<T>> ExecuteQueryRefCursor<T>(string connectionString, string cmdText, DbParameter[] cmdParms, CommandType cmdType = CommandType.Text) where T : new()
        {
            var resultList = new Dictionary<string, List<T>>();
            NpgsqlCommand cmd = new NpgsqlCommand();

            using (DbConnection conn = GetDbConnect(connectionString))
            {
                conn.Open();
                var tran = conn.BeginTransaction();
                var resultsReferences = ExecuteQuery(tran, cmdText, cmdParms, cmdType);

                for (var index = 0; index < resultsReferences.Count; index++)
                {
                    var resultsReference = resultsReferences[index];
                    var resultSetName = resultsReference.Values.FirstOrDefault();
                    var resultSetReferenceCmdText = string.Format("FETCH ALL IN \"{0}\";", resultSetName);

                    var dataKey = resultSetName.ToString().Replace("cursor", "");

                    var result = ExecuteQuery<T>(tran, resultSetReferenceCmdText, null, CommandType.Text);
                    var closeCursorSQL = string.Format("close \"{0}\";", resultSetName);
                    ExecuteNonQuery(tran, closeCursorSQL, new DbParameter[] { });
                    resultList.Add(dataKey, result);
                }
                conn.Close();
                return resultList;
            }

        }


        #endregion



        /// <summary>
        /// 执行 Transact-SQL 语句并返回受影响的行数。
        /// </summary>
        public int ExecuteNonQuery(string connectionString, string cmdText, DbParameter[] cmdParms, CommandType cmdType = CommandType.Text)
        {
            NpgsqlCommand cmd = new NpgsqlCommand();

            using (DbConnection conn = GetDbConnect(connectionString))
            {
                PrepareCommand(cmd, conn, null, cmdType, cmdText, cmdParms);

                int val = cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
                conn.Close();
                return val;
            }
        }

        /// <summary>
        /// 在事务中执行 Transact-SQL 语句并返回受影响的行数。
        /// </summary>
        public int ExecuteNonQuery(DbTransaction trans, string cmdText, DbParameter[] cmdParms, CommandType cmdType = CommandType.Text)
        {
            NpgsqlCommand cmd = new NpgsqlCommand();
            PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdText, cmdParms);
            int val = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            return val;
        }
       


        /// <summary>
        /// 执行查询，返回DataReader
        /// </summary>
        public DbDataReader ExecuteReader(DbConnection conn, string cmdText, DbParameter[] cmdParms, CommandType cmdType = CommandType.Text)
        {
            NpgsqlCommand cmd = new NpgsqlCommand();
            NpgsqlDataReader rdr=null;
            {
                try
                {
                    PrepareCommand(cmd, conn, null, cmdType, cmdText, cmdParms);
                    rdr = cmd.ExecuteReader();
                    cmd.Parameters.Clear();
                    return rdr;
                }
                catch(Exception ex)
                {
                    if (rdr != null)
                        rdr.Close();
                    throw ex;
                }
               
            }
        }

        /// <summary>
        /// 在事务中执行查询，返回DataReader
        /// </summary>
        public DbDataReader ExecuteReader(DbTransaction trans, string cmdText, DbParameter[] cmdParms, CommandType cmdType = CommandType.Text)
        {
            NpgsqlCommand cmd = new NpgsqlCommand();
            PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdText, cmdParms);
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            cmd.Parameters.Clear();
            return rdr;
        }

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列。忽略其他列或行。
        /// </summary>
        public object ExecuteScalar(string connectionString, string cmdText, DbParameter[] cmdParms, CommandType cmdType = CommandType.Text)
        {
            NpgsqlCommand cmd = new NpgsqlCommand();

            using (DbConnection conn = GetDbConnect(connectionString))
            {
                PrepareCommand(cmd, conn, null, cmdType, cmdText, cmdParms);
                object val = cmd.ExecuteScalar();
                cmd.Parameters.Clear();
                conn.Close();
                return val;
            }
        }

        /// <summary>
        /// 在事务中执行查询，并返回查询所返回的结果集中第一行的第一列。忽略其他列或行。
        /// </summary>
        public object ExecuteScalar(DbTransaction trans, string cmdText, DbParameter[] cmdParms, CommandType cmdType = CommandType.Text)
        {
            NpgsqlCommand cmd = new NpgsqlCommand();
            PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdText, cmdParms);
            object val = cmd.ExecuteScalar();
            cmd.Parameters.Clear();
            return val;
        }

        /// <summary>
        /// 生成要执行的命令
        /// </summary>
        /// <remarks>参数的格式：冒号+参数名</remarks>
        private static void PrepareCommand(DbCommand cmd, DbConnection conn, DbTransaction trans, CommandType cmdType,
            string cmdText, DbParameter[] cmdParms,bool isReplaceSpecChar=true)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();

            cmd.Connection = conn;
            //cmd.CommandText = cmdText.Replace("@", ":").Replace("?", ":").Replace("[", "\"").Replace("]", "\"");
            if (isReplaceSpecChar)
                cmd.CommandText = cmdText.Replace("@", ":").Replace("?", ":");
            if (CheckSqlInjection(cmd.CommandText))
                throw new Exception("SQL语句不可包含系统关键字");
            if (trans != null)
            {
                cmd.Transaction = trans;
            }


            cmd.CommandType = cmdType;

            if (cmdParms != null)
            {
                foreach (NpgsqlParameter parm in cmdParms)
                {
                    parm.ParameterName = parm.ParameterName.Replace("@", ":").Replace("?", ":");
                    if (parm.NpgsqlValue == null)
                    {
                        parm.NpgsqlValue = DBNull.Value;
                    }
                    if (parm.NpgsqlValue != DBNull.Value&&( parm.NpgsqlDbType == NpgsqlTypes.NpgsqlDbType.Text || parm.NpgsqlDbType == NpgsqlTypes.NpgsqlDbType.Varchar))
                    {
                        
                        //if (CheckDbParameterInjection(parm.NpgsqlValue.ToString()))
                        //   throw new Exception("SQL参数不可包含系统关键字");
                        
                    }
                    cmd.Parameters.Add(parm);
                }
            }
        }

        /// <summary>
        /// 检查是否包含注入的关键字
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static bool CheckSqlInjection(string s)
        {

            if (string.IsNullOrEmpty(s)) return false;
            //s = s.Replace("'", "");
            // return s.Contains("--");
            return false;
        }


        /// <summary>
        /// 检查sql参数是否包含注入的关键字，以防存储过程中传入字符串拼接导致注入
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static bool CheckDbParameterInjection(string s)
        {
            
            bool res = false;
            if (string.IsNullOrEmpty(s))
                return false;
            //if (s.Contains("--") || s.Contains("'"))
            if (s.Contains("--") )
            {
                res = true;
            }

            return res;
        }



        #region --执行解析NpgsqlDataReader对象--
        private List<T> ExecuteNpgsqlDataReader<T>(NpgsqlDataReader reader) where T : new()
        {
            List<T> rows = new List<T>();
            while (reader.Read())
            {
                var md = new T();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var filed = reader.GetName(i);
                    var value = reader.GetValue(i);

                    var pgsqlType = reader.GetPostgresType(i);
                    value = (value == DBNull.Value) ? null : value;
                    var property = md.GetType().GetProperty(filed, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    if (property != null)
                    {
                        if (value != null && property.PropertyType != typeof(string) && property.PropertyType != typeof(object) && (pgsqlType.NpgsqlDbType == NpgsqlDbType.Json || pgsqlType.NpgsqlDbType == NpgsqlDbType.Jsonb))
                        {
                            value = JsonConvert.DeserializeObject(value.ToString(), property.PropertyType);
                        }
                        if (property.PropertyType == typeof(System.Boolean))
                        {
                            if (value == null)
                            {
                                property.SetValue(md, false);
                            }
                            else {
                                if (value.GetType() == typeof(System.Boolean))
                                    property.SetValue(md, value);
                                else
                                {
                                    string tmp = value.ToString().ToLower();
                                    if (tmp == "true" || tmp != "0")
                                    {
                                        property.SetValue(md, true);
                                    }
                                    else {
                                        property.SetValue(md, false);
                                    }
                                }
                            }
                            
                        }
                        else {
                            property.SetValue(md, value);
                        }
                    }
                }
                rows.Add(md);

            }
            return rows;
        }
        #endregion

    }
}
