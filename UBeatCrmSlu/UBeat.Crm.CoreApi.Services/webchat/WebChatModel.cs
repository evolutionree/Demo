using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.webchat
{
    //
    public class WebChatMsgTemplate
    {
        public WebChatCommandType Cmd { get; set; }
        public object Data { get; set; }
    }
    public enum WebChatCommandType
    {
        WebChatLogin = 1,
        WebChatLogout = 2,
        WebChatSendMsg = 3,
        WebChatListMsg = 4
    }
    public class WebChatResponseMsg
    {
        public WebChatMsgType MessageType { get; set;  }
        public int ResultCode { get; set; }
        public string ErrorMsg { get; set; }
        public object Data { get; set; }
        public WebChatResponseMsg() {
            MessageType = WebChatMsgType.Command;
        }
    }
    public class WebResponsePackage
    {
        public int ReceiverId { get; set; }
        public WebSocket WebSock { get; set; }
        public WebChatResponseMsg CmdMsg { get; set; }
        public WebChatMsgType MessageType { get; set; }//1=命令返回结果，2=消息数据
        public WebChatMsgInfo ChatMsg { get; set; }

    }
    public class WebChatMsgInfo {

        public WebChatMsgType MessageType { get; set; }//1=命令返回结果，2=消息数据
        public string Title { get; set; }
        public string Message { get; set; }
        public Dictionary<string, object> CustomContent { get; set; }
        public int ReceiverId { get; set; }
        public WebChatMsgInfo() {
            MessageType = WebChatMsgType.ChatMessage;
        }
    }
    public enum WebChatMsgType {
        Command = 1,
        ChatMessage = 2
    }
}
