using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Dapper;

namespace UBeat.Crm.CoreApi.Repository.Utility
{
    public static class DapperExtensions
    {
        public static IEnumerable<T> Query<T>(this IDbConnection connection, Func<T> typeBuilder, string sql)
        {
            return connection.Query<T>(sql);
        }
    }

    /*
     * var data = connection.Query(() => new 
        {
            ContactId = default(int),
            Name = default(string),
        }, "SELECT ContactId, Name FROM Contact");
     */
}
