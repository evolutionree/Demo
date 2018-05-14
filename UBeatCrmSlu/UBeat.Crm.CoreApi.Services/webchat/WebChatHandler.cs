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

namespace UBeat.Crm.CoreApi.Services.webchat
{
    public class WebChatHandler
    {
      
        private readonly RequestDelegate _next;
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
            CancellationToken ct = context.RequestAborted;
            var currentSocket = await context.WebSockets.AcceptWebSocketAsync();
            while (true)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }

                string response = await ReceiveStringAsync(currentSocket, ct);
                WebChatMsgTemplate msg = JsonConvert.DeserializeObject<WebChatMsgTemplate>(response);
                if (msg == null) {
                    //发送格式错误的回复消息
                    continue;
                };
                WebChatResponseMsg responseMsg  = null;
                switch (msg.Cmd) {
                    case WebChatCommandType.WebChatLogin:
                        break;
                    case WebChatCommandType.WebChatLogout:
                        break;
                    case WebChatCommandType.WebChatListMsg:
                        break;
                    case WebChatCommandType.WebChatSendMsg:
                        break;
                }
                responseMsg = new WebChatResponseMsg() {
                    ResultCode = -1,
                    ErrorMsg = "请登录"
                };
                WebResponsePackage pkg = new WebResponsePackage()
                {
                    WebSock = currentSocket,
                    CmdMsg = responseMsg,
                    MessageType = WebChatMsgType.Command//命令回复数据
                };
                WebChatResponseHandler.getInstance().Enqueue(pkg);

            }

            //_sockets.TryRemove(socketId, out dummy);

            await currentSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", ct);
            currentSocket.Dispose();
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
