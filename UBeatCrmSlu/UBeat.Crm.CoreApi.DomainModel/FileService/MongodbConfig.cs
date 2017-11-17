using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.FileService
{
    public class MongodbConfig
    {
        /// <summary>
        /// mongodb连接字符串，标准格式： mongodb://[username:password@]host1[:port1][,host2[:port2],...[,hostN[:portN]]][/[database][?options]] 
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns></returns>
        public string GetConnectString(string dbName)
        {
            return string.Format(ConnTmp, dbName);
        }

        public string GetDefaultConnect()
        {
            return GetConnectString(DefaultDb);
        }

        public string ConnTmp { get; set; }
        public string DefaultDb { get; set; }

    }
}
