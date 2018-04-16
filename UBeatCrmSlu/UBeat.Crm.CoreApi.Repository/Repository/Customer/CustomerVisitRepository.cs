using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.IRepository;

namespace UBeat.Crm.CoreApi.Repository.Repository.Customer
{
    public class CustomerVisitRepository : RepositoryBase, ICustomerVisitRepository
    {

        public List<string> GetDictionaryDataValues(int dicTypeid, List<int> dicIds)
        {

            var executeSql = "SELECT dataval FROM crm_sys_dictionary WHERE dictypeid=@dictypeid AND dataid =ANY(@dicids) AND recstatus=1";
            var param = new DbParameter[]
           {
                new NpgsqlParameter("dictypeid",dicTypeid),
                new NpgsqlParameter("dicids",dicIds.ToArray())
           };

            var result = ExecuteQuery(executeSql, param);
            List<string> dataidlist = new List<string>();

            foreach (var m in result)
            {
                object value = null;
                if (m.TryGetValue("dataval", out value))
                {
                    if (value != null)
                    {
                        dataidlist.Add(value.ToString());
                    }
                }
            }
            return dataidlist;
        }
    }
}
