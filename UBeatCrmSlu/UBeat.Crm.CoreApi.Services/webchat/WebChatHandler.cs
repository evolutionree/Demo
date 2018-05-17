using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.Services.Models.Chat;
using UBeat.Crm.CoreApi.Services.Services;

namespace UBeat.Crm.CoreApi.Services.webchat
{
    public class WebChatHandler
    {

        private readonly RequestDelegate _next;

        private AccountServices _accountService = null;
        private ChatServices _chatService = null;
        private int UserId = 0;
        public WebChatHandler(RequestDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(HttpContext context)
        {

            if (!context.WebSockets.IsWebSocketRequest)
            {
                await _next.Invoke(context);
                return;
            }
            if (_accountService == null)
            {
                _accountService = ServiceLocator.Current.GetInstance<AccountServices>();
            }
            if (_chatService == null) {
                _chatService = ServiceLocator.Current.GetInstance<ChatServices>();
            }
            CancellationToken ct = context.RequestAborted;
            var currentSocket = await context.WebSockets.AcceptWebSocketAsync();

            WebChatResponseMsg responseMsg = null;
            WebResponsePackage pkg = null;
            responseMsg = new WebChatResponseMsg()
            {
                ResultCode = -1,
                ErrorMsg = "请登录"
            };
            pkg = new WebResponsePackage()
            {
                WebSock = currentSocket,
                CmdMsg = responseMsg,
                MessageType = WebChatMsgType.Command//命令回复数据
            };

            WebChatResponseHandler.getInstance().Enqueue(pkg);
            while (true)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }


                string response = null;
                try
                {
                    response = await ReceiveStringAsync(currentSocket, ct);
                    if (response != null && response.StartsWith("@heart")) continue;
                }
                catch (Exception ex) {
                    //已经连接异常了，需要把服务删除
                    WebSockChatSocketManager.getInstance().UnbindSocketWithUser(currentSocket, -1);
                    break;
                }

                WebChatMsgTemplate msg = null;
                try
                {
                    msg = JsonConvert.DeserializeObject<WebChatMsgTemplate>(response);
                } catch (Exception ex) {
                }
                if (msg == null) {
                    //发送格式错误的回复消息
                    responseMsg = new WebChatResponseMsg()
                    {
                        ResultCode = -2,
                        ErrorMsg = "消息格式异常"
                    };
                    pkg = new WebResponsePackage()
                    {
                        WebSock = currentSocket,
                        CmdMsg = responseMsg,
                        MessageType = WebChatMsgType.Command//命令回复数据
                    };

                    WebChatResponseHandler.getInstance().Enqueue(pkg);
                    continue;
                };
                switch (msg.Cmd) {
                    case WebChatCommandType.WebChatLogin:
                        Login(msg, currentSocket);
                        break;
                    case WebChatCommandType.WebChatLogout:
                        ReceiveMsg(msg, currentSocket);
                        break;
                    case WebChatCommandType.WebChatListMsg:
                        break;
                    case WebChatCommandType.WebChatSendMsg:
                        break;
                }
                responseMsg = new WebChatResponseMsg()
                {
                    ResultCode = -1,
                    ErrorMsg = "请登录"
                };
                pkg = new WebResponsePackage()
                {
                    WebSock = currentSocket,
                    CmdMsg = responseMsg,
                    MessageType = WebChatMsgType.Command//命令回复数据
                };
                WebChatResponseHandler.getInstance().Enqueue(pkg);

            }
            try
            {

                await currentSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", ct);
            }
            catch (Exception ex) {
            }
            try
            {
                currentSocket.Dispose();
            }
            catch (Exception ex) {
            }
        }

        /// <summary>
        /// 登陆处理方法
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="websock"></param>
        public void Login(WebChatMsgTemplate cmd, WebSocket websock)
        {
            WebChatResponseMsg responseMsg = null;
            WebResponsePackage pkg = null;
            try
            {
                Dictionary<string, object> data = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(cmd.Data));
                int userid = 0;
                string authcode = "";
                if (data.ContainsKey("userid") && data["userid"] != null) {
                    userid = int.Parse(data["userid"].ToString());
                }
                if (data.ContainsKey("authorizedcode") && data["authorizedcode"] != null)
                {
                    authcode = data["authorizedcode"].ToString();
                }
                if (userid <= 0 || authcode.Length == 0) {
                    //发送登陆失败的回复
                    responseMsg = new WebChatResponseMsg()
                    {
                        ResultCode = -2,
                        ErrorMsg = "登陆失败(code:-2)"
                    };
                    pkg = new WebResponsePackage()
                    {
                        WebSock = websock,
                        CmdMsg = responseMsg,
                        MessageType = WebChatMsgType.Command//命令回复数据
                    };
                    WebChatResponseHandler.getInstance().Enqueue(pkg);
                    return;
                }
                if (_accountService.CheckAuthorizedCodeValid(userid, authcode))
                {
                    WebSockChatSocketManager.getInstance().MapSocketToUser(websock, userid);
                    UserId = userid;
                    //发送登陆成功的消息
                    responseMsg = new WebChatResponseMsg()
                    {
                        ResultCode = 0,
                        ErrorMsg = "登陆成功"
                    };
                    pkg = new WebResponsePackage()
                    {
                        WebSock = websock,
                        CmdMsg = responseMsg,
                        MessageType = WebChatMsgType.Command//命令回复数据
                    };
                    WebChatResponseHandler.getInstance().Enqueue(pkg);
                }
                else {
                    //发送登陆失败的回复
                    responseMsg = new WebChatResponseMsg()
                    {
                        ResultCode = -2,
                        ErrorMsg = "登陆失败(code:-3)"
                    };
                    pkg = new WebResponsePackage()
                    {
                        WebSock = websock,
                        CmdMsg = responseMsg,
                        MessageType = WebChatMsgType.Command//命令回复数据
                    };
                    WebChatResponseHandler.getInstance().Enqueue(pkg);
                }
            }
            catch (Exception ex)
            {
                responseMsg = new WebChatResponseMsg()
                {
                    ResultCode = -2,
                    ErrorMsg = "登陆失败(code:-4)"
                };
                pkg = new WebResponsePackage()
                {
                    WebSock = websock,
                    CmdMsg = responseMsg,
                    MessageType = WebChatMsgType.Command//命令回复数据
                };
                WebChatResponseHandler.getInstance().Enqueue(pkg);
            }
        }

        /// <summary>
        /// 发送消息功能
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="webSock"></param>
        public void ReceiveMsg(WebChatMsgTemplate cmd, WebSocket webSock) {
            WebChatResponseMsg responseMsg = null;
            WebResponsePackage pkg = null;
            try
            {
                SendChatModel model = Newtonsoft.Json.JsonConvert.DeserializeObject<SendChatModel>(JsonConvert.SerializeObject(cmd.Data));
                if (model != null)
                {
                    _chatService.SendChat(model, UserId);
                }
                else {
                    //发送消息异常的回复
                    responseMsg = new WebChatResponseMsg()
                    {
                        ResultCode = -10,
                        ErrorMsg = "发送消息失败"
                    };
                    pkg = new WebResponsePackage()
                    {
                        ReceiverId = UserId,
                        CmdMsg = responseMsg,
                        MessageType = WebChatMsgType.Command//命令回复数据
                    };
                    WebChatResponseHandler.getInstance().Enqueue(pkg);
                }
            }
            catch (Exception ex) {
                //发送消息异常的回复
                responseMsg = new WebChatResponseMsg()
                {
                    ResultCode = -10,
                    ErrorMsg = "发送消息失败"
                };
                pkg = new WebResponsePackage()
                {
                    ReceiverId = UserId,
                    CmdMsg = responseMsg,
                    MessageType = WebChatMsgType.Command//命令回复数据
                };
                WebChatResponseHandler.getInstance().Enqueue(pkg);
            }
        }
        private static async Task<string> ReceiveStringAsync(System.Net.WebSockets.WebSocket socket, CancellationToken ct = default(CancellationToken))
        {
            var buffer = new ArraySegment<byte>(new byte[8192]);
            using (var ms = new MemoryStream())
            {
                WebSocketReceiveResult result;
                do
                {
                    ct.ThrowIfCancellationRequested();

                    result = await socket.ReceiveAsync(buffer, ct);
                    ms.Write(buffer.Array, buffer.Offset, result.Count);
                }
                while (!result.EndOfMessage);

                ms.Seek(0, SeekOrigin.Begin);
                if (result.MessageType != WebSocketMessageType.Text)
                {
                    return null;
                }

                using (var reader = new StreamReader(ms, Encoding.UTF8))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }
    }
    
}
