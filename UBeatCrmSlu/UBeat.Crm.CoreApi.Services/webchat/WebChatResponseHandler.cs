using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UBeat.Crm.CoreApi.Services.webchat
{
    public  class WebChatResponseHandler
    {
        private static object lockInstance = new object();
        private Queue<WebResponsePackage> queue = new Queue<WebResponsePackage>();
        private static WebChatResponseHandler instance = null;
        private object operatorLock = new object();
        private System.Threading.Thread RunningThread = null;
        private AutoResetEvent _wh = new AutoResetEvent(false);
        private WebChatResponseHandler() {
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


        }
        public  void WaitAndSendResponse() {
            while (true) {
                WebResponsePackage pkg = queue.Dequeue();
                if (pkg == null) {
                    _wh.WaitOne(10 * 1000);//最多等待10秒钟
                    continue;
                }
                List<WebSocket> sockets = new List<WebSocket>();
                if (pkg.WebSock != null)
                {
                    sockets.Add(pkg.WebSock);
                }
                else {
                    sockets = WebSockChatSocketManager.getInstance().getSocketsByUserId(pkg.ReceiverId);
                }
                List<Task> ts = new List<Task>();
                foreach (WebSocket socket in sockets)
                {
                    Task task = SendStringAsync(socket, pkg.msg);
                    if (task != null )
                        ts.Add(task);
                }
                foreach (Task t in ts) {
                    try
                    {
                        t.Wait(1000);
                    }
                    catch (Exception ex) { } 
                }

            }
        }
        private  Task SendStringAsync(System.Net.WebSockets.WebSocket socket, WebChatResponseMsg data, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var buffer = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(data));
                var segment = new ArraySegment<byte>(buffer);
                return socket.SendAsync(segment, WebSocketMessageType.Text, true, ct);
            }
            catch (Exception ex) {
            }
            return null;
        }
        public void Enqueue(WebResponsePackage msg) {
            lock (operatorLock) {
                this.queue.Enqueue(msg);
                this._wh.Set();
            }
        }

    }
}
