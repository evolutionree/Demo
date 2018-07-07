using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UBeat.Crm.CoreApi.Models
{
    public class AppMetricsModel
    {
        /// <summary>
        /// 是否启用，1=启用，其他=关闭
        /// </summary>
        public int IsEnable { set; get; }
        public string DataBase { set; get; }
        public string DbUserName { set; get; }

        public string DbPassword { set; get; }

        public string Uri { set; get; }

        public string AppName { set; get; }

        public string EnvName { set; get; }

        public long PhysicalMemoryHealthCheck { set; get; }
    }
}
