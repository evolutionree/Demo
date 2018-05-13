using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.webchat
{
    public class WebSockChatSocketManager
    {
        public static WebSockChatSocketManager instance = null;
        public static object instanceObject = new object();
        private Dictionary<string,WebSocket> AllSockets = new Dictionary<string, WebSocket>();
        private Dictionary<int, List<WebSocket>> UserSockets = new Dictionary<int, List<WebSocket>>();
        private HashSet<string> UnLoginSockets = new HashSet<string>();
        private Dictionary<string, int> SocketUserMap = new Dictionary<string, int>();
        private object operatorobject = new object();
        private WebSockChatSocketManager() {

        }
        public static WebSockChatSocketManager getInstance() {
            lock (instanceObject) {
                if (instance == null) instance = new WebSockChatSocketManager();
                return instance;
            }
        }
        public List<WebSocket> getSocketsByUserId(int userId) {
            lock (operatorobject) {
                if (userId <= 0) return null;
                if (UserSockets.ContainsKey(userId))
                    return UserSockets[userId];
                return null;
            }
        }
    }
}
