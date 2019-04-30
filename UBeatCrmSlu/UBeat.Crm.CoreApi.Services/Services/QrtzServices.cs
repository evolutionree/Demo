using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.UkQrtz;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Utility;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class QrtzServices: BaseServices
    {
        private readonly IQrtzRepository _qrtzRepository;
        
        public QrtzServices(IQrtzRepository qrtzRepository) {
            _qrtzRepository = qrtzRepository;
        }
        /// <summary>
        /// 获取本次需要触发的事务信息
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        public List<TriggerDefineInfo> ListNeedTriggers(int userid,DateTime scheduleTime) {
            List<TriggerDefineInfo> triggers = _qrtzRepository.ListNeedCheckTriggers(userid, null);
            List<TriggerDefineInfo> retList = new List<TriggerDefineInfo>();
            DateTime now = scheduleTime;
            foreach(TriggerDefineInfo trigger in  triggers){
                if (trigger.TriggerTime != null && trigger.TriggerTime.Length > 0) {
                    if (TriggerCronbCheckUtils.Match(now,trigger.TriggerTime)) {
                        retList.Add(trigger);
                    }
                }
            }
            return retList;
        }

        public int ClearRunningStatus(string serverid)
        {
            int count = 0;
            List<TriggerDefineInfo> triggers =  this._qrtzRepository.ListDeadTriggers(serverid, 0, null);
            foreach (TriggerDefineInfo triggerInfo in triggers) {
                this._qrtzRepository.ClearTiggerRunningStatus(null, triggerInfo.RecId, serverid, 0);
                count++;
            }
            return count;
        }

        public void ExecuteSQL(string strSQL, int userid) {
            this._qrtzRepository.ExecuteSQL(strSQL,0, null);
        }
        public void AddTrigerInstance(TriggerInstanceInfo instanceInfo, int userid) {
            if (instanceInfo == null) return;
            this._qrtzRepository.AddTriggerInstance(instanceInfo, userid, null);
        }
        public void UpdateTrigerInstance(TriggerInstanceInfo instanceInfo, int userid) {
            if (instanceInfo == null) return;
            this._qrtzRepository.UpdateTriggerInstance(instanceInfo, userid, null);
        }
        /// <summary>
        /// 更新事务定义的运行信息
        /// </summary>
        /// <param name="triggerInfo"></param>
        /// <param name="userid"></param>
        public void UpdateRunningTrigerInfo(TriggerDefineInfo triggerInfo, int userid) {
            if (triggerInfo == null) return;
            TriggerDefineInfo oldInfo = this._qrtzRepository.TriggerDetail(triggerInfo.RecId, userid, null);
            if (oldInfo == null) return;
            oldInfo.InBusy = triggerInfo.InBusy;
            oldInfo.EndRunTime = triggerInfo.EndRunTime;
            oldInfo.StartRunTime = triggerInfo.StartRunTime;
            oldInfo.RunningServer = ServerFingerPrintUtils.getInstance().CurrentFingerPrint.ServerId.ToString();
            oldInfo.ErrorCount = triggerInfo.ErrorCount;
            oldInfo.LastErrorTime = triggerInfo.LastErrorTime;
            this._qrtzRepository.UpdateTrigger(oldInfo, userid, null);
        }

        public TriggerDefineInfo TriggerDetail(Guid recId, int userId)
        {
            return this._qrtzRepository.TriggerDetail(recId, userId, null);
        }

        /// <summary>
        /// 更新事务定义的基本信息
        /// </summary>
        /// <param name="triggerInfo"></param>
        /// <param name="userid"></param>
        public TriggerDefineInfo UpdateTriggerBaseInfo(TriggerDefineInfo triggerInfo, int userid) {
            if (triggerInfo == null || triggerInfo.RecId == null) return null;
            TriggerDefineInfo oldInfo = this._qrtzRepository.TriggerDetail(triggerInfo.RecId, userid, null);
            if (oldInfo == null) return null;
            oldInfo.RecName = triggerInfo.RecName;
            oldInfo.TriggerTime = triggerInfo.TriggerTime;
            oldInfo.Remark = triggerInfo.Remark;
            oldInfo.SingleRun = triggerInfo.SingleRun;
            oldInfo.ActionCmd = triggerInfo.ActionCmd;
            oldInfo.ActionType = triggerInfo.ActionType;
            oldInfo.ActionParameters = triggerInfo.ActionParameters;
            this._qrtzRepository.UpdateTrigger(oldInfo, userid, null);
            return this._qrtzRepository.TriggerDetail(oldInfo.RecId, userid, null);
        }
        /// <summary>
        /// 增加事务定义
        /// </summary>
        /// <param name="triggerDefineInfo"></param>
        /// <param name="userid"></param>
        public TriggerDefineInfo AddTriggerDefineInfo(TriggerDefineInfo triggerDefineInfo, int userid) {
            if (triggerDefineInfo == null) throw(new Exception("参数异常")); 
			triggerDefineInfo.RecId = Guid.NewGuid();
            this._qrtzRepository.AddTrigger(triggerDefineInfo, userid, null);
            return this._qrtzRepository.TriggerDetail(triggerDefineInfo.RecId, userid, null);
        }

        public PageDataInfo<TriggerInstanceInfo> ListInstances(Guid triggerId, DateTime searchFrom, DateTime searchTo, bool listArchived, int pageIndex, int pageSize, int userId)
        {
			TriggerDefineInfo triggerInfo = this._qrtzRepository.TriggerDetail(triggerId, userId, null);
			if (triggerInfo.TriggerType == TriggerType.TriggerType_System)
				throw (new Exception("不支持系统事务类型操作（triggertype）"));

			return this._qrtzRepository.ListTriggerInstances(triggerId, "", searchFrom, searchTo, listArchived, pageIndex, pageSize, userId, null);

        }

        /// <summary>
        /// 禁用或者启用事务定义
        /// </summary>
        /// <param name="recid"></param>
        /// <param name="status"></param>
        /// <param name="userid"></param>
        public TriggerDefineInfo ForbitTrigger(Guid recid, int status, int userid) {
            if(recid == null || recid.Equals(Guid.Empty)) throw(new Exception("参数异常"));
            if (status != 1 && status != 2 && status != 0) throw (new Exception("参数异常"));
            TriggerDefineInfo triggerInfo = this._qrtzRepository.TriggerDetail(recid, userid, null);
            if (triggerInfo == null) throw (new Exception("无法找到事务定义"));
			if (triggerInfo.TriggerType == TriggerType.TriggerType_System)
				throw (new Exception("不支持系统事务类型操作（triggertype）"));
			triggerInfo.RecStatus = status;
            this._qrtzRepository.UpdateTrigger(triggerInfo, userid, null);
            return this._qrtzRepository.TriggerDetail(triggerInfo.RecId, userid, null);

        }

		public TriggerDefineInfo StopTrigger(Guid recid, int userid, string userName)
		{
			if (recid == null || recid.Equals(Guid.Empty)) throw (new Exception("参数异常")); 
			TriggerDefineInfo triggerInfo = this._qrtzRepository.TriggerDetail(recid, userid, null);
			if (triggerInfo == null) throw (new Exception("无法找到事务定义"));
			if (triggerInfo.InBusy != 1) throw (new Exception("事务必须是运行中才能终止实例"));
			if (triggerInfo.TriggerType == TriggerType.TriggerType_System)
				throw (new Exception("不支持系统事务类型操作（triggertype）"));
			triggerInfo.InBusy = 0;//0空闲 1运行中
			this._qrtzRepository.UpdateTrigger(triggerInfo, userid, null);

			TriggerInstanceInfo instanceInfo = new TriggerInstanceInfo();
			instanceInfo.BeginTime = DateTime.Now;
			instanceInfo.RecId = Guid.NewGuid();
			instanceInfo.Status = TriggerInstanceStatusEnum.Completed;
			instanceInfo.RunServer = System.Net.Dns.GetHostName();
			instanceInfo.TriggerId = triggerInfo.RecId;
			instanceInfo.ErrorMsg = string.Format(@"【{0}】用户终止了实例", userName);
			this._qrtzRepository.AddTriggerInstance(instanceInfo, userid, null); 

			return this._qrtzRepository.TriggerDetail(triggerInfo.RecId, userid, null); 
		}

		public void TestForCallService(Dictionary<string,object> paramInfo ) {
            Console.WriteLine("Test For Call Service :" + DateTime.Now.ToString());
        }
        /// <summary>
        /// 用于定时事务
        /// 对于过多的定时事务实例，对实例进行归档
        /// 归档规则：
        /// 对于每个事务定义，不超过200条实例，多余的将进行归档
        /// 为了避免过多的归档操作，当超过300条开始归档，归档减少到200条。
        /// </summary>
        public void TriggerInstanceArchived(Dictionary<string, object> paramInfo)
        {
            int userid = 0;
            DbTransaction tran = null;
            int MaxInstancesCount = 200;
            int MaxFireCount = 300;
            #region 获取从事务中传输的参数
            if (paramInfo != null)
            {
                if (paramInfo.ContainsKey("firecount") && paramInfo["firecount"] != null)
                {
                    int tmp = 0;
                    if (Int32.TryParse(paramInfo["firecount"].ToString(), out tmp))
                    {
                        MaxFireCount = tmp;
                    }
                }
                if (paramInfo.ContainsKey("leftcount") && paramInfo["leftcount"] != null)
                {
                    int tmp = 0;
                    if (Int32.TryParse(paramInfo["leftcount"].ToString(), out tmp))
                    {
                        MaxInstancesCount = tmp;
                    }
                }
            } 
            #endregion
            List<TriggerDefineInfo> needArchiveTriggers = this._qrtzRepository.ListNeedArchiveTriggers((int)300, (int)0,(DbTransaction) null);
            foreach (TriggerDefineInfo triggerInfo in needArchiveTriggers) {
                Console.WriteLine("开始归档事务:" + triggerInfo.RecId.ToString());
                this._qrtzRepository.ArchiveInstances(triggerInfo.RecId, MaxInstancesCount, userid, tran);
                Console.WriteLine("事务归档结束");
            }
        }
        public PageDataInfo<TriggerDefineInfo> ListTriggers(string SearchKey, bool SearchDeleted, bool SearchNormal, bool SearchStop, int PageIndex, int PageSize, int userid) {
           return this._qrtzRepository.ListTriggers(SearchKey, SearchNormal, SearchStop, SearchDeleted, PageIndex, PageSize, userid, null);
        }
    }
}
