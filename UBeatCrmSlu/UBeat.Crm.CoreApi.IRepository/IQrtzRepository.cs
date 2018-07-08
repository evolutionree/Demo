using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.UkQrtz;

namespace UBeat.Crm.CoreApi.IRepository
{

    /// <summary>
    /// 调度事务的存储接口
    /// </summary>
    public interface IQrtzRepository
    {
        /// <summary>
        /// 获取所有的事务定义
        /// 按事务名称排序
        /// </summary>
        /// <param name="loadDeleted"></param>
        /// <param name="userid"></param>
        /// <param name="tran"></param>
        /// <returns></returns>
        List<TriggerDefineInfo> getAllTriggers(bool loadDeleted ,int userid,DbTransaction tran);
        /// <summary>
        /// 获取事务定义列表
        /// </summary>
        /// <param name="SearchKey"></param>
        /// <param name="LoadNormal"></param>
        /// <param name="LoadStop"></param>
        /// <param name="LoadDeleted"></param>
        /// <param name="PageIndex"></param>
        /// <param name="PageSize"></param>
        /// <param name="userid"></param>
        /// <param name="tran"></param>
        /// <returns></returns>
        PageDataInfo<TriggerDefineInfo> ListTriggers(string SearchKey, bool LoadNormal, bool LoadStop, bool LoadDeleted, int PageIndex, int PageSize, int userid, DbTransaction tran);
        /// <summary>
        /// 新增一个事务定义
        /// </summary>
        /// <param name="triggerInfo"></param>
        /// <param name="userid"></param>
        /// <param name="tran"></param>
        void AddTrigger(TriggerDefineInfo triggerInfo, int userid, DbTransaction tran);
        /// <summary>
        /// 更新一个事务定义
        /// </summary>
        /// <param name="triggerInfo"></param>
        /// <param name="userid"></param>
        /// <param name="tran"></param>
        void UpdateTrigger(TriggerDefineInfo triggerInfo, int userid, DbTransaction tran);
        /// <summary>
        /// 获取一个事务的详情
        /// </summary>
        /// <param name="recid"></param>
        /// <param name="userid"></param>
        /// <param name="tran"></param>
        /// <returns></returns>
        TriggerDefineInfo TriggerDetail(Guid recid, int userid, DbTransaction tran);
        /// <summary>
        /// 删除一个事务定义
        /// </summary>
        /// <param name="recid"></param>
        /// <param name="userid"></param>
        /// <param name="tran"></param>
        void DeleteTrigger(Guid recid, int userid, DbTransaction tran);
        /// <summary>
        /// 获取事务实例
        /// </summary>
        /// <param name="triggerid"></param>
        /// <param name="triggername"></param>
        /// <param name="dtFrom"></param>
        /// <param name="dtTo"></param>
        /// <param name="pageindex"></param>
        /// <param name="pagesize"></param>
        /// <param name="userid"></param>
        /// <param name="tran"></param>
        /// <returns></returns>
        PageDataInfo<TriggerInstanceInfo> ListTriggerInstances(Guid triggerid, string triggername,
                    DateTime dtFrom, DateTime dtTo, bool listArchived, int pageindex, int pagesize, int userid, DbTransaction tran);
        /// <summary>
                                                                                                                        /// 新增一个事务实例
                                                                                                                        /// </summary>
                                                                                                                        /// <param name="instanceInfo"></param>
                                                                                                                        /// <param name="userid"></param>
                                                                                                                        /// <param name="tran"></param>
        void AddTriggerInstance(TriggerInstanceInfo instanceInfo, int userid, DbTransaction tran);
        /// <summary>
        /// 更新一个事务实例
        /// </summary>
        /// <param name="instanceInfo"></param>
        /// <param name="userid"></param>
        /// <param name="tran"></param>
        void UpdateTriggerInstance(TriggerInstanceInfo instanceInfo, int userid, DbTransaction tran);
        /// <summary>
        /// 获取一个事务实例的详情
        /// </summary>
        /// <param name="recid"></param>
        /// <param name="userid"></param>
        /// <param name="tran"></param>
        /// <returns></returns>
        TriggerInstanceInfo InstanceDetail(Guid recid, int userid, DbTransaction tran);

        List<TriggerDefineInfo> ListNeedCheckTriggers(int userid, DbTransaction tran);
        void ExecuteSQL(string sql, int userid, DbTransaction tran);
        List<TriggerDefineInfo> ListNeedArchiveTriggers(int v1, int v2, DbTransaction dbTransaction);
        void ArchiveInstances(Guid recId, int maxInstancesCount, int userid, DbTransaction tran);
        void ClearTiggerRunningStatus(DbTransaction tran, Guid triggerid, string serverid, int userid);
        List<TriggerDefineInfo> ListDeadTriggers(string serverid, int userid, DbTransaction tran);
    }
}
