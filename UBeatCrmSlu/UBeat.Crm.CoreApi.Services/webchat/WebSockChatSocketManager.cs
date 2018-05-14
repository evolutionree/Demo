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
        private Dictionary<int, List<WebSocket>> UserSockets = new Dictionary<int, List<WebSocket>>();
        private Dictionary<WebSocket, int> SocketUserMap = new Dictionary<WebSocket, int>();
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
        /// <summary>
        /// socket绑定用户
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="UserId"></param>
        public void MapSocketToUser(WebSocket socket, int UserId) {
            lock (operatorobject) {
                //先判断Socket是否已经在别的用户中绑定了
                if (SocketUserMap.ContainsKey(socket)) {
                    int oldUserId = SocketUserMap[socket];
                    if (UserSockets.ContainsKey(oldUserId)) {
                        List<WebSocket> list = UserSockets[oldUserId];
                        list.Remove(socket);
                    }
                }
                if (UserSockets.ContainsKey(UserId))
                {
                    List<WebSocket> list = UserSockets[UserId];
                    list.Add(socket);
                    SocketUserMap.Add(socket, UserId);
                }
                else {
                    List<WebSocket> list = new List<WebSocket>();
                    list.Add(socket);
                    UserSockets.Add(UserId, list);
                    SocketUserMap.Add(socket, UserId);
                }
            }
        }

        /// <summary>
        /// 解绑用户
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="UserId"></param>
        public void UnbindSocketWithUser(WebSocket socket, int UserId) {
            lock (operatorobject) {
                if (SocketUserMap.ContainsKey(socket)) {
                    int oldUserId = SocketUserMap[socket];
                    if (oldUserId != UserId) {
                        if (UserSockets.ContainsKey(oldUserId)) {
                            List<WebSocket> list = UserSockets[oldUserId];
                            list.Remove(socket);
                        }
                    }
                }
                if (UserSockets.ContainsKey(UserId)) {
                    List<WebSocket> list = UserSockets[UserId];
                    list.Remove(socket);
                }
            }
        }
    }
}
