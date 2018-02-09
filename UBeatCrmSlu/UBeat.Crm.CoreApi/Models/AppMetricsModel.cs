using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UBeat.Crm.CoreApi.Models
{
    public class AppMetricsModel
    {
        public string DataBase { set; get; }
        public string DbUserName { set; get; }

        public string DbPassword { set; get; }

        public string Uri { set; get; }

        public string AppName { set; get; }

        public string EnvName { set; get; }

        public long PhysicalMemoryHealthCheck { set; get; }
    }
}
