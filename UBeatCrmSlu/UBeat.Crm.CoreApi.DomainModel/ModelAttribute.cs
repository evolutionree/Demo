using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel
{
    [AttributeUsage(AttributeTargets.All)]
    public sealed class SqlTypeAttribute : Attribute
    {
        private readonly NpgsqlDbType _dbtype;

        public NpgsqlDbType NpgsqlDbType
        {
            get { return _dbtype; }
        }

        public SqlTypeAttribute(NpgsqlDbType dbtype)
        {
            _dbtype = dbtype;
        }
    }


}
