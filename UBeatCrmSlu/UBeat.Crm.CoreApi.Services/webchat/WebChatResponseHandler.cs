using Microsoft.Extensions.Logging;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.DomainModel.Account;
using UBeat.Crm.CoreApi.DomainModel.BaseSys;
using UBeat.Crm.CoreApi.DomainModel.FileService;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Utility;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.CoreApi.Services.Utility;

namespace UBeat.Crm.CoreApi.Services.webchat
{
    public  class WebChatResponseHandler
    {

        private Dictionary<int, UserInfo> UserList = new Dictionary<int, UserInfo>();
        private static object lockInstance = new object();
        private Queue<WebResponsePackage> queue = new Queue<WebResponsePackage>();
        private static WebChatResponseHandler instance = null;
        private object operatorLock = new object();
        private Task FetchNextServerMessageTask;
        private System.Threading.Thread RunningThread = null;
        private AutoResetEvent _wh = new AutoResetEvent(false);
        private CacheServices cacheServices;
        private Dictionary<string, int> ServerLastMsgId = new Dictionary<string, int>();
        private readonly FileServices _fileService;
        private int CurrentMsgId = 1;
        private IAccountRepository _accountRepository;

        private WebChatResponseHandler() {
            cacheServices = ServiceLocator.Current.GetInstance<CacheServices>();
            _accountRepository = ServiceLocator.Current.GetInstance<IAccountRepository>();
            _fileService = ServiceLocator.Current.GetInstance<FileServices>();
            StartTask();

        }
        
        public static WebChatResponseHandler getInstance() {
            lock (lockInstance) {
                if (instance == null) {
                    instance = new WebChatResponseHandler();
                }
                return instance;
            }
        }
        private void StartTask()
        {
            RunningThread = new System.Threading.Thread( WaitAndSendResponse);
            RunningThread.Start();
            this.FetchNextServerMessageTask = new Task(this.FetchOtherServerMessage);
            this.FetchNextServerMessageTask.Start();


        }
        public  void WaitAndSendResponse() {
            while (true) {
                try
                {
                    WebResponsePackage pkg = null;
                    try
                    {
                        pkg = queue.Dequeue();
                    }
                    catch (Exception ex)
                    {

                    }
                    if (pkg == null)
                    {
                        _wh.WaitOne(10 * 1000);//最多等待10秒钟
                        continue;
                    }
                    List<WebSocket> sockets = new List<WebSocket>();
                    if (pkg.WebSock != null)
                    {
                        sockets.Add(pkg.WebSock);
                    }
                    else
                    {
                        sockets = WebSockChatSocketManager.getInstance().getSocketsByUserId(pkg.ReceiverId);
                    }
                    List<Task> ts = new List<Task>();
                    foreach (WebSocket socket in sockets)
                    {
                        if (pkg.MessageType == WebChatMsgType.ChatMessage)
                        {
                            pkg.ChatMsg.MessageType = WebChatMsgType.ChatMessage;
                            //需要处理文件的情况
                            if (pkg.ChatMsg.CustomContent != null && pkg.ChatMsg.CustomContent.ContainsKey("ct") && pkg.ChatMsg.CustomContent["ct"] != null)
                            {
                                int ct = 0;
                                ct = int.Parse(pkg.ChatMsg.CustomContent["ct"].ToString());
                                if (ct == 5) //文件才处理
                                {
                                    try
                                    {

                                        FileInfoModel fileInfo = _fileService.GetOneFileInfo(null, pkg.ChatMsg.CustomContent["ct"].ToString());
                                        if (fileInfo != null)
                                        {
                                            pkg.ChatMsg.CustomContent.Add("file", fileInfo);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                    }
                                }
                            }
                            Task task = SendStringAsync(socket, pkg.ChatMsg);
                            if (task != null)
                                ts.Add(task);
                        }
                        else
                        {
                            pkg.CmdMsg.MessageType = WebChatMsgType.Command;
                            Task task = SendStringAsync(socket, pkg.CmdMsg);
                            if (task != null)
                                ts.Add(task);
                        }

                    }
                    foreach (Task t in ts)
                    {
                        try
                        {
                            t.Wait(1000);
                        }
                        catch (Exception ex) { }
                    }
                }
                catch (Exception ex) {
                    try
                    {
                        System.Threading.Thread.Sleep(1000 * 10);
                    }
                    catch (Exception ex2) {

                    }
                }
                
            }
        }
        private  Task SendStringAsync(System.Net.WebSockets.WebSocket socket, object data, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var buffer = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(data));
                var segment = new ArraySegment<byte>(buffer);
                return socket.SendAsync(segment, WebSocketMessageType.Text, true, ct);
            }
            catch (Exception ex) {
                Console.WriteLine(ex.StackTrace);
                WebSockChatSocketManager.getInstance().UnbindSocketWithUser(socket,-1);
            }
            return null;
        }
        public void Enqueue(WebResponsePackage msg) {
            lock (operatorLock) {
                this.queue.Enqueue(msg);
                this._wh.Set();
            }
        }
        private UserInfo GetUserDetail(int uid) {
            if (UserList == null) UserList = new Dictionary<int, UserInfo>();
            if (UserList.ContainsKey(uid)) {
                return UserList[uid];
            }
            UserInfo userInfo =  _accountRepository.GetUserInfoById(uid);
            UserList.Add(uid, userInfo);
            return userInfo;
        }
        /// <summary>
        /// 用于移动端向web端发送消息，
        /// </summary>
        /// <param name="accountList"></param>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="customContent"></param>
        public void addMessages(string accountList,string title,string message, Dictionary<string, object> customContent)
        {
            if (accountList == null) return;
            try
            {
                string[] tmp = accountList.Split(",");
                foreach (string account in tmp)
                {
                    WebResponsePackage msg = new WebResponsePackage();
                    msg.ReceiverId = int.Parse(account);
                    msg.MessageType = WebChatMsgType.ChatMessage;
                    msg.ChatMsg = new WebChatMsgInfo()
                    {
                        Title = title,
                        Message = message,
                        CustomContent = customContent,
                        ReceiverId = int.Parse(account)
                    };
                    Enqueue(msg);

                    #region 获取客户详情
                    if (customContent.ContainsKey("s") && customContent["s"] != null ) {
                        int u = 0;
                        if (int.TryParse(customContent["s"].ToString(), out u)) {
                            customContent.Add("ud", GetUserDetail(u));
                        }
                    }
                    #endregion
                    int msgid = CurrentMsgId;
                    CurrentMsgId++;
                    cacheServices.Repository.Add("chatmsgid_" + ServerFingerPrintUtils.getInstance().CurrentFingerPrint.ServerId + "_" + msgid.ToString(), msg, new TimeSpan(0, 5, 0));

                }
                cacheServices.Repository.Add("chatmsgids_" + ServerFingerPrintUtils.getInstance().CurrentFingerPrint.ServerId,(CurrentMsgId-1).ToString());//把最新的

            }
            catch (Exception ex) {

            }
        }

        public void FetchOtherServerMessage() {
            int totalMinSecond = 5000;
            DateTime lastDateTime = System.DateTime.MinValue;
            while (true) {
                try
                {

                    double totalmin = (System.DateTime.Now - lastDateTime).TotalMilliseconds;
                    if (totalmin < totalMinSecond)
                    {
                        System.Threading.Thread.Sleep((int)(totalMinSecond - totalmin));
                    }
                    lastDateTime = System.DateTime.Now;
                    string dbname = DataBaseHelper.getDbName();
                    List<ServerListInfo>  serverList = cacheServices.Repository.Get<List<ServerListInfo>>("ServerList_" + dbname);
                    if (serverList == null || serverList.Count == 0) continue;
                    foreach (ServerListInfo server in serverList) {
                        if (server.ServerFinger.Equals(ServerFingerPrintUtils.getInstance().CurrentFingerPrint.ServerId)) {
                            continue;
                        }
                        if ((System.DateTime.Now - server.LasHeartTime).TotalMinutes > 10) {
                            //十分钟没有心跳，则不考虑处理
                            continue;
                        }
                        string msgid =  cacheServices.Repository.Get<string>("chatmsgids_" + server.ServerFinger);
                        if (msgid == null || msgid.Length == 0) continue;
                        int imsgid = 0;
                        if (int.TryParse(msgid,out imsgid) ==false )  { continue; }
                        int lastid = 0;
                        if (this.ServerLastMsgId.ContainsKey(server.ServerFinger)) {
                             lastid = ServerLastMsgId[server.ServerFinger];
                        }
                        else {
                            lastid = imsgid;
                        }
                        if (lastid < imsgid)
                        {
                            for (int id = lastid + 1; id < imsgid; id++)
                            {
                                WebResponsePackage pkg = cacheServices.Repository.Get<WebResponsePackage>("chatmsgid_" + server.ServerFinger + "_" + id.ToString());
                                if (pkg == null) continue;
                                Enqueue(pkg);
                            }
                            
                        }
                        ServerLastMsgId[server.ServerFinger] = imsgid;

                    }
                }
                catch (Exception ex) {
                }

            }
        }

    }
}
