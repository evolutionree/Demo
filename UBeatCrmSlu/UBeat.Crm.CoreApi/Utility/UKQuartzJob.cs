using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UBeat.Crm.CoreApi.Utility
{
    public class UKQuartzJob : Task
    {
        public UKQuartzJob(Action action) : base(action)
        {
        }
        
    }
}
