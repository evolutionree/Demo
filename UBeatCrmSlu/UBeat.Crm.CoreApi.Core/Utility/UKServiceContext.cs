using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;

namespace UBeat.Crm.CoreApi.Core.Utility
{
    public class UKServiceContext
    {
        public DbTransaction Transaction { get; set; }

    }
}
