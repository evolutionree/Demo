using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.DomainModel.UkQrtz;
using UBeat.Crm.CoreApi.Services.Services;
using System.Reflection;
using UBeat.Crm.CoreApi.Services.Utility;

namespace UBeat.Crm.CoreApi.Utility
{
    public class MainSchedulerJobImp : IJob
    {
        public static MainSchedulerJobImp instance = null;
        private QrtzServices _qrtzServices;
        private bool IsClearStatus = false;
        private string ServerName = null;
        private static NLog.ILogger _logger = NLog.LogManager.GetLogger(typeof(MainSchedulerJobImp).FullName);
        public void clearRunningStatus (){
            if (IsClearStatus == true) return;
            string serverid = ServerFingerPrintUtils.getInstance().CurrentFingerPrint.ServerId.ToString();
            _logger.Trace("开始重置服务器上次关闭后的未处理完毕的调度任务,serverid="+ serverid);
            int clearCount = GetQrtzServices().ClearRunningStatus(serverid);
            _logger.Trace(string.Format("调度任务的实例已经清理完毕，工清理了{0}个任务", clearCount));
        }
        public Task Execute(IJobExecutionContext context)
        {
            clearRunningStatus();
            DateTime dt = DateTime.Now;
            if (context.ScheduledFireTimeUtc != null && context.ScheduledFireTimeUtc.HasValue) {
                context.ScheduledFireTimeUtc.Value.ToLocalTime();
            }
            else {
                dt = context.FireTimeUtc.DateTime.ToLocalTime();
            }
            List<TriggerDefineInfo> list = GetQrtzServices().ListNeedTriggers(0, dt);
            Task task =  new Task(() =>
            {
                List<UKJobTaskInfo> ts = new List<UKJobTaskInfo>();
                foreach (TriggerDefineInfo triggerInfo in list) {
                    if (triggerInfo.ActionType == TriggerActionType.ActionType_DbFunc)
                    {
                        UKJobTaskInfo t = StartDbFunctionJob(triggerInfo);
                        if (t != null) ts.Add(t);
                    }
                    else if (triggerInfo.ActionType == TriggerActionType.ActionType_Service) {
                        UKJobTaskInfo t = StartServiceJob(triggerInfo);
                        if (t != null) ts.Add(t);
                    }

                }
                foreach (UKJobTaskInfo subTask in ts) {
                    subTask.Task.Wait();
                }
            });
            task.Start();
            return task;
        }
        private QrtzServices GetQrtzServices() {
            if (_qrtzServices == null)
                _qrtzServices = ServiceLocator.Current.GetInstance<QrtzServices>();
            return _qrtzServices;
        }
        private UKJobTaskInfo StartDbFunctionJob(TriggerDefineInfo triggerInfo) {
            UKJobTaskInfo retInfo = new UKJobTaskInfo();
            retInfo.TriggerInfo = triggerInfo;
            retInfo.InstanceInfo = new TriggerInstanceInfo();
            retInfo.InstanceInfo.BeginTime = DateTime.Now;
            retInfo.InstanceInfo.RecId = Guid.NewGuid();
            retInfo.InstanceInfo.Status = TriggerInstanceStatusEnum.Running;
            retInfo.InstanceInfo.RunServer = CurrentServerName();
            retInfo.InstanceInfo.TriggerId = triggerInfo.RecId;
            triggerInfo.InBusy = 1;
            triggerInfo.RunningServer = CurrentServerName();
            triggerInfo.StartRunTime = retInfo.InstanceInfo.BeginTime;
            try
            {
                GetQrtzServices().UpdateRunningTrigerInfo(triggerInfo, UserId());
                GetQrtzServices().AddTrigerInstance(retInfo.InstanceInfo, UserId());
            }
            catch (Exception ex) {
            }
            retInfo.Task = new Task(() => {
                try
                {
                    this._qrtzServices.ExecuteSQL(triggerInfo.ActionCmd, UserId());
                    retInfo.InstanceInfo.EndTime = DateTime.Now;
                    retInfo.InstanceInfo.Status = TriggerInstanceStatusEnum.Completed;
                }
                catch (Exception ex) {
                    retInfo.InstanceInfo.EndTime = DateTime.Now;
                    retInfo.InstanceInfo.ErrorMsg = ex.Message;
                    retInfo.InstanceInfo.Status = TriggerInstanceStatusEnum.CompletedWithError;
                }
                EndDbFunctionJob(retInfo);
            });
            retInfo.Task.Start();
            return retInfo;
        }
        private void EndDbFunctionJob(UKJobTaskInfo taskInfo) {
            try
            {
                taskInfo.TriggerInfo.EndRunTime = DateTime.Now;
                taskInfo.TriggerInfo.InBusy = 0;
                GetQrtzServices().UpdateRunningTrigerInfo(taskInfo.TriggerInfo, UserId());
                GetQrtzServices().UpdateTrigerInstance(taskInfo.InstanceInfo,UserId());
            }
            catch (Exception ex) {
            }
        }
        private UKJobTaskInfo StartServiceJob(TriggerDefineInfo triggerInfo) {
            UKJobTaskInfo retInfo = new UKJobTaskInfo();
            retInfo.TriggerInfo = triggerInfo;
            retInfo.InstanceInfo = new TriggerInstanceInfo();
            retInfo.InstanceInfo.BeginTime = DateTime.Now;
            retInfo.InstanceInfo.RecId = Guid.NewGuid();
            retInfo.InstanceInfo.Status = TriggerInstanceStatusEnum.Running;
            retInfo.InstanceInfo.RunServer = CurrentServerName();
            retInfo.InstanceInfo.TriggerId = triggerInfo.RecId;
            triggerInfo.InBusy = 1;
            triggerInfo.RunningServer = CurrentServerName();
            triggerInfo.StartRunTime = retInfo.InstanceInfo.BeginTime;
            try
            {
                GetQrtzServices().UpdateRunningTrigerInfo(triggerInfo, UserId());
                GetQrtzServices().AddTrigerInstance(retInfo.InstanceInfo, UserId());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            retInfo.Task = new Task(() => {
                try
                {
                    string fullpath = triggerInfo.ActionCmd;
                    int index = fullpath.LastIndexOf('.');
                    if (index < 0) {
                        throw (new Exception("系统配置错误"));
                    }
                    string className = fullpath.Substring(0, index);
                    string funName = fullpath.Substring(index + 1, fullpath.Length - index - 1);
                    BaseServices bs = (BaseServices)ServiceLocator.Current.GetInstanceWithName(className);
                    if (bs == null) {
                        throw (new Exception("服务创建失败"));
                    }
                    MethodInfo method = bs.GetType().GetMethod(funName);
                    if (method == null) {
                        throw (new Exception("获取执行方法异常"));
                    }
                    object[] obj = new object[] { };
                    ParameterInfo[] parameters = method.GetParameters();
                    if (parameters != null && parameters.Length > 0) {
                        List<object> r = new List<object>();
                        foreach (ParameterInfo p in parameters) {
                            //什么都不干，现在支持Dictionary<string,object>
                        }

                        if (retInfo.TriggerInfo.ActionParameters != null && retInfo.TriggerInfo.ActionParameters.Length > 0)
                        {
                            Dictionary<string, object> actionParameters = null;
                            try
                            {
                                actionParameters = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(retInfo.TriggerInfo.ActionParameters);

                            }
                            catch (Exception ex)
                            {
                            }
                            r.Add(actionParameters);
                        }
                        else {
                            r.Add(null);
                        }
                        obj = r.ToArray();
                    }
                    method.Invoke(bs, obj);
                   
                    retInfo.InstanceInfo.EndTime = DateTime.Now;
                    retInfo.InstanceInfo.Status = TriggerInstanceStatusEnum.Completed;
                }
                catch (Exception ex)
                {
                    retInfo.InstanceInfo.EndTime = DateTime.Now;
                    retInfo.InstanceInfo.ErrorMsg = ex.Message;
                    retInfo.InstanceInfo.Status = TriggerInstanceStatusEnum.CompletedWithError;
                    Console.WriteLine(ex.Message);
                }
                EndServiceJob(retInfo);
            });
            retInfo.Task.Start();
            return retInfo;
        }
        private void EndServiceJob(UKJobTaskInfo taskInfo) {
            try
            {
                taskInfo.TriggerInfo.EndRunTime = DateTime.Now;
                taskInfo.TriggerInfo.InBusy = 0;
                GetQrtzServices().UpdateRunningTrigerInfo(taskInfo.TriggerInfo, UserId());
                GetQrtzServices().UpdateTrigerInstance(taskInfo.InstanceInfo, UserId());
            }
            catch (Exception ex)
            {
            }
        }
        private string CurrentServerName() {
            if (ServerName == null || ServerName.Length == 0) {
                ServerName = System.Net.Dns.GetHostName();
            }
            return ServerName;
        }
        private int UserId() {
            return 0;
        }

    }
    public class UKJobTaskInfo {
        public Task Task { get; set; }
        public TriggerDefineInfo TriggerInfo { get; set; }
        public TriggerInstanceInfo InstanceInfo { get; set; }
    }
}
