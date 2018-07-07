using Quartz;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.DomainModel.BaseSys;
using UBeat.Crm.CoreApi.Repository.Utility;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.CoreApi.Services.Utility;

namespace UBeat.Crm.CoreApi.Utility
{
    /// <summary>
    /// 用于定时检查redis上的状态， 并把心跳信息加入到redis中，以便其他服务器感知本服务器的存在
    /// 服务于同一个数据库（租户）的服务器群组必须连接到同一个redis（可以是Redis集群的不同Redis实例）。
    /// 在Redis中增加一个key为ServerList_{dbname}的可以，value是一个服务器列表，列表项是1个字段，叫Serverfinger信息（未来会增加其他信息）
    /// 然后在服务器上增加一个ServerFinger_{serverfinger},value是最后的心跳时间
    /// 其他服务，如消息服务，将会检测ServerList_{dbname}，以便获取服务器的存货状态
    /// </summary>
    public class HeartBeatWithRedisJob : IJob
    {
        private CacheServices cacheServices;
        public Task Execute(IJobExecutionContext context)
        {
            Task task = new Task(() =>
            {
                string dbname = DataBaseHelper.getDbName();
                if (cacheServices == null) {
                    cacheServices = ServiceLocator.Current.GetInstance<CacheServices>();
                }
                List<ServerListInfo>  serverList = cacheServices.Repository.Get<List<ServerListInfo>>("ServerList_" + dbname) ;
                if (serverList == null) {
                    serverList = new List<ServerListInfo>();
                }
                DateTime dt = System.DateTime.Now;

                string thisServerId = ServerFingerPrintUtils.getInstance().CurrentFingerPrint.ServerId.ToString();
                bool isFound = false;
                foreach (ServerListInfo item in serverList) {
                    if (item.ServerFinger.Equals(item.ServerFinger)) {
                        isFound = true;
                        item.LasHeartTime = dt;
                        item.FingerDetail = ServerFingerPrintUtils.getInstance().CurrentFingerPrint;
                    }
                }
                if (!isFound) {
                    ServerListInfo  item = new ServerListInfo()
                    {
                        ServerFinger = thisServerId,
                        LasHeartTime = dt,
                        FingerDetail = ServerFingerPrintUtils.getInstance().CurrentFingerPrint

                    };
                    serverList.Add(item);
                }
                cacheServices.Repository.Add("ServerList_" + dbname, serverList);
                cacheServices.Repository.Add("ServerFinger_" + thisServerId, dt);
            });
            task.Start();
            return task;
        }
    }
}
