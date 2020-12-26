using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Dapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NLog;
using Npgsql;
using UBeat.Crm.CoreApi.Core.Utility;
using static Dapper.SqlMapper;

namespace UBeat.Crm.CoreApi.Repository.Utility
{
    public static class DataBaseHelper
    {
        private static readonly Logger Logger = LogManager.GetLogger(typeof(DataBaseHelper).FullName);

        private static string _connectString;

        public static IDbConnection GetDbConnect(string connectStr = null)
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

        public static string getDbName()
        {
            if (_connectString == null)
            {
                IConfigurationRoot config = ServiceLocator.Current.GetInstance<IConfigurationRoot>();
                _connectString = config.GetConnectionString("DefaultDB");
            }
            NpgsqlConnection conn = new NpgsqlConnection(_connectString); ;
            return conn.Database;
        }
        public static int ExecuteNonQuery(string commandText, object parameters = null,
            CommandType commandType = CommandType.Text, string connectString = null)
        {
            int result;
            using (var conn = GetDbConnect(connectString))
            {
                conn.Open();
                var tran = conn.BeginTransaction();
                try
                {
                    result = conn.Execute(commandText, parameters, commandType: commandType);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    Logger.Error(ex, "数据库执行出错");
                    throw;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }

        public static int ExecuteNonQuery(string commandText, IDbConnection connection, IDbTransaction transaction,
            object parameters = null, CommandType commandType = CommandType.Text, string connectString = null)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            return connection.Execute(commandText, parameters, commandType: commandType, transaction: transaction);
        }

        public static TDataType ExecuteScalar<TDataType>(string commandText, object parameters = null,
            CommandType commandType = CommandType.Text, string connectString = null)
        {
            TDataType result;
            using (var conn = GetDbConnect(connectString))
            {
                conn.Open();
                var tran = conn.BeginTransaction();
                try
                {
                    result = conn.ExecuteScalar<TDataType>(commandText, parameters, commandType: commandType);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    Logger.Error(ex, "数据库执行出错");
                    throw;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return result;
        }

        public static TDataType ExecuteScalar<TDataType>(string commandText, IDbConnection connection,
            IDbTransaction transaction, object parameters = null,
            CommandType commandType = CommandType.Text, string connectString = null)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            return connection.ExecuteScalar<TDataType>(commandText, parameters, commandType: commandType);
        }

        /// <summary>
        /// 该方法是用于带Cursor的函数查询
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="dataNames"></param>
        /// <param name="parameters"></param>
        /// <param name="commandType"></param>
        /// <param name="connectString"></param>
        /// <returns></returns>
        public static Dictionary<string, List<IDictionary<string, object>>> QueryStoredProcCursor(string procName,
            List<string> dataNames,
            object parameters = null, CommandType commandType = CommandType.StoredProcedure, string connectString = null, IDbTransaction touterran = null)
        {

            var resultList = new Dictionary<string, List<IDictionary<string, object>>>();
            IDbConnection conn = null;
            IDbTransaction tran = null;
            try
            {
                if (touterran == null)
                {
                    conn = GetDbConnect(connectString);
                    conn.Open();
                    tran = conn.BeginTransaction();
                }
                else
                {
                    tran = touterran;
                    conn = tran.Connection;
                }
                var resultsReferences =
                            conn.Query(procName, parameters, transaction: tran, commandType: commandType)
                                .Cast<IDictionary<string, object>>()
                                .ToList();
                for (var index = 0; index < resultsReferences.Count; index++)
                {
                    var resultsReference = resultsReferences[index];
                    var resultSetName = resultsReference.Values.FirstOrDefault();
                    var resultSetReferenceCommand = string.Format("FETCH ALL IN \"{0}\";", resultSetName);

                    var dataKey = index >= dataNames.Count ? resultSetName.ToString().Replace("cursor", "") : dataNames[index];

                    var result =
                        conn.Query(resultSetReferenceCommand, null, commandType: CommandType.Text, transaction: tran)
                            .Cast<IDictionary<string, object>>()
                            .ToList();
                    var closeCursorSQL = string.Format("close \"{0}\";", resultSetName);
                    ExecuteNonQuery(closeCursorSQL, conn, tran);
                    resultList.Add(dataKey, result);
                }
                if (touterran == null)
                {
                    tran.Commit();
                    tran = null;
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                try
                {
                    if (touterran == null)
                    {
                        if (tran != null)
                        {
                            tran.Rollback();
                            tran = null;
                        }
                        if (conn != null)
                        {
                            conn.Close();
                            conn = null;
                        }
                    }
                }
                catch (Exception ex1)
                {
                }
            }
            return resultList;
        }

        /// <summary>
        /// 该方法是用于带Cursor的函数查询
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="dataNames"></param>
        /// <param name="parameters"></param>
        /// <param name="commandType"></param>
        /// <param name="connectString"></param>
        /// <returns></returns>
        public static List<TDataType> QueryStoredProcCursor<TDataType>(string procName,
            object parameters = null, CommandType commandType = CommandType.StoredProcedure, string connectString = null)
        {
            List<TDataType> result;
            using (var conn = GetDbConnect(connectString))
            {
                conn.Open();
                var tran = conn.BeginTransaction();
                try
                {
                    var resultsReferences =
                        conn.Query(procName, parameters, commandType: commandType)
                            .Cast<IDictionary<string, object>>()
                            .ToList();
                    var resultsReference = resultsReferences.FirstOrDefault();
                    var resultSetName = resultsReference.Values.FirstOrDefault();
                    var resultSetReferenceCommand = string.Format("FETCH ALL IN \"{0}\";", resultSetName);
                    var tmp = conn.Query(resultSetReferenceCommand, null, commandType: CommandType.Text,
                            transaction: tran).ToList();
                    result = JsonConvert.DeserializeObject<List<TDataType>>(JsonConvert.SerializeObject(tmp));
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    Logger.Error(ex, "数据库执行出错");
                    throw;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }

        public static List<IDictionary<string, object>> QueryStoredProcCursor(string procName,
      object parameters = null, CommandType commandType = CommandType.StoredProcedure, string connectString = null)
        {
            List<IDictionary<string, object>> result;
            using (var conn = GetDbConnect(connectString))
            {
                conn.Open();
                var tran = conn.BeginTransaction();
                try
                {
                    var resultsReferences =
                        conn.Query(procName, parameters, commandType: commandType)
                            .Cast<IDictionary<string, object>>()
                            .ToList();
                    var resultsReference = resultsReferences.FirstOrDefault();
                    var resultSetName = resultsReference.Values.FirstOrDefault();
                    var resultSetReferenceCommand = string.Format("FETCH ALL IN \"{0}\";", resultSetName);

                    result =
                        conn.Query(resultSetReferenceCommand, null, commandType: CommandType.Text,
                            transaction: tran).Cast<IDictionary<string, object>>().ToList();
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    Logger.Error(ex, "数据库执行出错");
                    throw;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }


        public static Dictionary<string, object> QueryStoredProcCursor<TDataType, TOtherType, TFirstType, TSecondType>(
            string procName,
            List<string> dataNames,
            object parameters = null, CommandType commandType = CommandType.StoredProcedure, string connectString = null)
        {
            var resultList = new Dictionary<string, object>();
            using (var conn = GetDbConnect(connectString))
            {
                conn.Open();
                var tran = conn.BeginTransaction();
                try
                {
                    var resultsReferences =
                        conn.Query(procName, parameters, commandType: commandType)
                            .Cast<IDictionary<string, object>>()
                            .ToList();
                    for (var index = 0; index < resultsReferences.Count; index++)
                    {
                        if (index >= dataNames.Count) break;
                        var resultsReference = resultsReferences[index];
                        var resultSetName = resultsReference.Values.FirstOrDefault();
                        var resultSetReferenceCommand = string.Format("FETCH ALL IN \"{0}\";", resultSetName);

                        object result;
                        switch (index)
                        {
                            case 0:
                                {
                                    result =
                                        conn.Query<TDataType>(resultSetReferenceCommand, null, commandType: CommandType.Text,
                                                transaction: tran)
                                            .ToList();
                                    break;
                                }
                            case 1:
                                {
                                    result =
                                        conn.Query<TOtherType>(resultSetReferenceCommand, null,
                                                commandType: CommandType.Text, transaction: tran)
                                            .ToList();
                                    break;
                                }
                            case 2:
                                {
                                    result =
                                        conn.Query<TFirstType>(resultSetReferenceCommand, null,
                                                commandType: CommandType.Text, transaction: tran)
                                            .ToList();
                                    break;
                                }
                            case 3:
                                {
                                    result =
                                        conn.Query<TSecondType>(resultSetReferenceCommand, null,
                                                commandType: CommandType.Text, transaction: tran)
                                            .ToList();
                                    break;
                                }
                            default:
                                continue;
                        }

                        var dataKey = dataNames[index];
                        resultList.Add(dataKey, result);
                    }
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    Logger.Error(ex, "数据库执行出错");
                    throw;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return resultList;
        }


        public static List<T> Query<T>(IDbConnection connection, string commandText, object parameters = null,
            CommandType commandType = CommandType.Text, IDbTransaction tran = null)
        {
            return connection.Query<T>(commandText, parameters, commandType: commandType).ToList();
        }

        public static List<IDictionary<string, object>> Query(IDbConnection connection, string commandText, object parameters = null,
           CommandType commandType = CommandType.Text)
        {
            return connection.Query(commandText, parameters, commandType: commandType)
                            .Cast<IDictionary<string, object>>()
                            .ToList();
        }


        public static List<IDictionary<string, object>> Query(string commandText, object parameters = null,
            CommandType commandType = CommandType.Text, string connectString = null)
        {
            List<IDictionary<string, object>> result;
            using (var conn = GetDbConnect(connectString))
            {
                conn.Open();
                var tran = conn.BeginTransaction();
                try
                {
                    result =
                        conn.Query(commandText, parameters, commandType: commandType)
                            .Cast<IDictionary<string, object>>()
                            .ToList();
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    Logger.Error(ex, "数据库执行出错");
                    throw;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }



        public static List<TDataType> Query<TDataType>(string commandText, object parameters = null,
            CommandType commandType = CommandType.Text, string connectString = null)
        {
            List<TDataType> result;
            using (var conn = GetDbConnect(connectString))
            {
                conn.Open();
                var tran = conn.BeginTransaction();
                try
                {
                    result =
                        conn.Query<TDataType>(commandText, parameters, commandType: commandType).ToList();
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    Logger.Error(ex, "数据库执行出错");
                    throw;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }

        public static TDataType QuerySingle<TDataType>(IDbConnection connection, string commandText, object parameters = null,
           CommandType commandType = CommandType.Text, string connectString = null)
        {
            TDataType result;
            result =
                connection.Query<TDataType>(commandText, parameters, commandType: commandType)
                    .FirstOrDefault();

            return result;
        }

        public static TDataType QuerySingle<TDataType>(string commandText, object parameters = null,
            CommandType commandType = CommandType.Text, string connectString = null)
        {
            TDataType result;
            using (var conn = GetDbConnect(connectString))
            {
                conn.Open();
                var tran = conn.BeginTransaction();
                try
                {
                    result =
                        conn.Query<TDataType>(commandText, parameters, commandType: commandType)
                            .FirstOrDefault();
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    Logger.Error(ex, "数据库执行出错");
                    throw;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }


        public static GridReader QueryMultiple(string sql, object parameters = null,
            CommandType commandType = CommandType.Text, string connectString = null)
        {
            GridReader result;
            using (var conn = GetDbConnect(connectString))
            {
                conn.Open();
                var tran = conn.BeginTransaction();
                try
                {

                    result = conn.QueryMultiple(sql, parameters, commandType: commandType);

                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    Logger.Error(ex, "数据库执行出错");
                    throw;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }




    }
}