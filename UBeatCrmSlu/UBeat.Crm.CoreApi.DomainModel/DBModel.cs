using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel
{
    public class DBModel
    {
        public DBModel(DbTransaction dbTran)
        {
            DbTransaction = dbTran;
        }
        public DbConnection DbConnection { set; get; }
        public DbTransaction DbTransaction { set; get; }
    }
}
