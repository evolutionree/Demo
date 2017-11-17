using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.Repository.Utility;

namespace UBeat.Crm.CoreApi.Repository.Repository
{
    public class RepositoryBase
    {
        protected IDBHelper DBHelper = new PostgreHelper();
        


        /// <summary>
        /// 批量执行 Transact-SQL 语句
        /// </summary>
        public void ExecuteNonQueryMultiple(string cmdText, List<DbParameter[]> cmdParms, DbTransaction trans = null, CommandType cmdType = CommandType.Text)
        {
            
            int result = 0;
            if (trans != null)
            {
                foreach (var parms in cmdParms)
                {
                    result = DBHelper.ExecuteNonQuery(trans, cmdText, parms, cmdType);
                    if (result <= 0)
                    {
                        throw new Exception("批量执行 Transact-SQL 语句失败");
                    }
                }
            }
            else
            {
                using (DbConnection conn = DBHelper.GetDbConnect())
                {
                    conn.Open();
                    var tran = conn.BeginTransaction();

                    try
                    {
                        foreach (var parms in cmdParms)
                        {
                            result = DBHelper.ExecuteNonQuery(tran, cmdText, parms, cmdType);
                            if (result <= 0)
                            {
                                throw new Exception("批量执行 Transact-SQL 语句失败");
                            }
                        }
                        tran.Commit();
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
        }

        /// <summary>
        /// 执行 Transact-SQL 语句并返回受影响的行数。
        /// </summary>
        public int ExecuteNonQuery(string cmdText, DbParameter[] cmdParms, DbTransaction trans = null, CommandType cmdType = CommandType.Text)
        {
            if (trans == null)
                return DBHelper.ExecuteNonQuery("", cmdText, cmdParms, cmdType);
            else return DBHelper.ExecuteNonQuery(trans, cmdText, cmdParms, cmdType);
        }

        /// <summary>
        /// 执行查询，返回分页数据
        /// </summary>
        public PageDataInfo<T> ExecuteQueryByPaging<T>(string cmdText, DbParameter[] cmdParms, int pageSize, long pageIndex, DbTransaction trans = null, CommandType cmdType = CommandType.Text) where T : new()
        {
            string pagingSql = cmdText;
            string countSql = string.Format("SELECT COUNT(1) FROM ({0}) t", cmdText.Trim().Trim(';'));
            if (pageSize > 0 && pageIndex > 0)
            {
                long offset = (pageIndex - 1) * pageSize;
                pagingSql = string.Format("{0} LIMIT {1} OFFSET {2}", cmdText.Trim().Trim(';'), pageSize, offset);
            }

            var result = new PageDataInfo<T>();
            result.PageInfo.PageSize = pageSize;

            if (trans == null)
            {
                object count = DBHelper.ExecuteScalar("", countSql, cmdParms, CommandType.Text);
                result.PageInfo.TotalCount = long.Parse(count.ToString());
                result.DataList = DBHelper.ExecuteQuery<T>("", pagingSql, cmdParms, cmdType);
            }
            else
            {
                object count = DBHelper.ExecuteScalar(trans, countSql, cmdParms, CommandType.Text);
                result.PageInfo.TotalCount = long.Parse(count.ToString());
                result.DataList = DBHelper.ExecuteQuery<T>(trans, pagingSql, cmdParms, cmdType);
            }

            return result;
        }

        /// <summary>
        /// 执行查询，返回分页数据
        /// </summary>
        public PageDataInfo<Dictionary<string, object>> ExecuteQueryByPaging(string cmdText, DbParameter[] cmdParms, int pageSize, long pageIndex, DbTransaction trans = null, CommandType cmdType = CommandType.Text)
        {
            string pagingSql = cmdText;
            string countSql= string.Format("SELECT COUNT(1) FROM ({0}) t", cmdText.Trim().Trim(';'));
            if (pageSize > 0 && pageIndex > 0)
            {
                long offset = (pageIndex - 1) * pageSize;
                pagingSql = string.Format("{0} LIMIT {1} OFFSET {2}", cmdText.Trim().Trim(';'), pageSize, offset);
            }

            var result = new PageDataInfo<Dictionary<string, object>>();
            result.PageInfo.PageSize = pageSize;

            if (trans == null)
            {
                object count = DBHelper.ExecuteScalar("", countSql, cmdParms, CommandType.Text);
                result.PageInfo.TotalCount = long.Parse(count.ToString());
                result.DataList = DBHelper.ExecuteQuery("", pagingSql, cmdParms, cmdType);
            }
            else
            {
                object count = DBHelper.ExecuteScalar(trans, countSql, cmdParms, CommandType.Text);
                result.PageInfo.TotalCount = long.Parse(count.ToString());
                result.DataList = DBHelper.ExecuteQuery(trans, pagingSql, cmdParms, cmdType);
            }

            return result;
        }
        ///// <summary>
        ///// 执行游标查询，返回分页数据
        ///// </summary>
        //public PageDataInfo<Dictionary<string, object>> ExecuteQueryRefCursorByPaging(string cmdText, DbParameter[] cmdParms, int pageSize, long pageIndex, DbTransaction trans = null, CommandType cmdType = CommandType.Text)
        //{
        //    long offset = (pageIndex - 1) * pageSize;
        //    var pagingSql = string.Format("{0} LIMIT {1} OFFSET {2}", cmdText.Trim().Trim(';'), pageSize, offset);
        //    var countSql = string.Format("SELECT COUNT(1) FROM ({0}) t", cmdText.Trim().Trim(';'));

        //    var result = new PageDataInfo<Dictionary<string, object>>();
        //    result.PageInfo.PageSize = pageSize;

        //    if (trans == null)
        //    {
        //        object count = DBHelper.ExecuteScalar("", countSql, cmdParms, CommandType.Text);
        //        result.PageInfo.TotalCount = long.Parse(count.ToString());
        //        result.DataList = DBHelper.ExecuteQuery("", pagingSql, cmdParms, cmdType);
        //    }
        //    else
        //    {
        //        object count = DBHelper.ExecuteScalar(trans, countSql, cmdParms, CommandType.Text);
        //        result.PageInfo.TotalCount = long.Parse(count.ToString());
        //        result.DataList = DBHelper.ExecuteQuery(trans, pagingSql, cmdParms, cmdType);
        //    }

        //    return result;
        //}


        /// <summary>
        /// 执行查询，返回单结果集
        /// </summary>
        public List<Dictionary<string, object>> ExecuteQuery(string cmdText, DbParameter[] cmdParms, DbTransaction trans = null, CommandType cmdType = CommandType.Text)
        {
            if (trans == null)
                return DBHelper.ExecuteQuery("", cmdText, cmdParms, cmdType);
            else return DBHelper.ExecuteQuery(trans, cmdText, cmdParms, cmdType);
        }

        /// <summary>
        /// 执行查询，返回单结果集
        /// </summary>
        public List<T> ExecuteQuery<T>(string cmdText, DbParameter[] cmdParms, DbTransaction trans = null, CommandType cmdType = CommandType.Text) where T : new()
        {
            if (trans == null)
                return DBHelper.ExecuteQuery<T>("", cmdText, cmdParms, cmdType);
            else return DBHelper.ExecuteQuery<T>(trans, cmdText, cmdParms, cmdType);
        }

        /// <summary>
        /// 执行查询，返回多结果集
        /// </summary>
        public List<List<Dictionary<string, object>>> ExecuteQueryMultiple(string cmdText, DbParameter[] cmdParms, DbTransaction trans = null, CommandType cmdType = CommandType.Text)
        {
            if (trans == null)
                return DBHelper.ExecuteQueryMultiple("", cmdText, cmdParms, cmdType);
            else return DBHelper.ExecuteQueryMultiple(trans, cmdText, cmdParms, cmdType);
        }

        /// <summary>
        /// 执行查询，返回多结果集
        /// </summary>
        public List<List<T>> ExecuteQueryMultiple<T>(string cmdText, DbParameter[] cmdParms, DbTransaction trans = null, CommandType cmdType = CommandType.Text) where T : new()
        {
            if (trans == null)
                return DBHelper.ExecuteQueryMultiple<T>("", cmdText, cmdParms, cmdType);
            else return DBHelper.ExecuteQueryMultiple<T>(trans, cmdText, cmdParms, cmdType);
        }

        /// <summary>
        /// 执行游标查询，返回多结果集
        /// </summary>
        public Dictionary<string, List<Dictionary<string, object>>> ExecuteQueryRefCursor(string cmdText, DbParameter[] cmdParms, DbTransaction trans = null, CommandType cmdType = CommandType.Text)
        {
            if (trans == null)
                return DBHelper.ExecuteQueryRefCursor("", cmdText, cmdParms, cmdType);
            else return DBHelper.ExecuteQueryRefCursor(trans, cmdText, cmdParms, cmdType);
        }

        /// <summary>
        /// 执行游标查询，返回多结果集
        /// </summary>
        public Dictionary<string, List<T>> ExecuteQueryRefCursor<T>(string cmdText, DbParameter[] cmdParms, DbTransaction trans = null, CommandType cmdType = CommandType.Text) where T : new()
        {
            if (trans == null)
                return DBHelper.ExecuteQueryRefCursor<T>("", cmdText, cmdParms, cmdType);
            else return DBHelper.ExecuteQueryRefCursor<T>(trans, cmdText, cmdParms, cmdType);
        }

        /// <summary>
        /// 在事务中执行查询，返回DataReader
        /// </summary>
        public DbDataReader ExecuteReader(string cmdText, DbParameter[] cmdParms, DbTransaction trans = null, CommandType cmdType = CommandType.Text)
        {
            if (trans == null)
                throw new Exception("事务不可为空");
            else return DBHelper.ExecuteReader(trans, cmdText, cmdParms, cmdType);
        }

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列。忽略其他列或行。
        /// </summary>
        public object ExecuteScalar(string cmdText, DbParameter[] cmdParms, DbTransaction trans = null, CommandType cmdType = CommandType.Text)
        {
            if (trans == null)
                return DBHelper.ExecuteScalar("", cmdText, cmdParms, cmdType);
            else return DBHelper.ExecuteScalar(trans, cmdText, cmdParms, cmdType);
        }

        /// <summary>
        /// 得到数据条数
        /// </summary>
        /// <param name="tblName">表名</param>
        /// <param name="condition">条件(不需要where)</param>
        /// <returns>数据条数</returns>
        public int GetCount(string tblName, string condition, DbParameter[] cmdParms, DbTransaction trans = null)
        {
            if (trans == null)
                return DBHelper.GetCount("", tblName, condition, cmdParms);
            else return DBHelper.GetCount(trans, tblName, condition, cmdParms);
        }

    }
}
