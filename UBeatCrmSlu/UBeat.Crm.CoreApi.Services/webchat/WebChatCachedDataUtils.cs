using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.DomainModel.Account;
using UBeat.Crm.CoreApi.DomainModel.Chat;
using UBeat.Crm.CoreApi.IRepository;

namespace UBeat.Crm.CoreApi.Services.webchat
{
    public class WebChatCachedDataUtils
    {
        private static WebChatCachedDataUtils instance = null;
        private static object lockobject = new object();
        private Dictionary<int, WebChatUserCacheInfo> UserList = new Dictionary<int, WebChatUserCacheInfo>();
        private Dictionary<Guid, WebChatGroupCacheInfo> GroupList = new Dictionary<Guid, WebChatGroupCacheInfo>();
        private IAccountRepository _accountRepository;
        private IChatRepository _chatRepository;

        private WebChatCachedDataUtils() {
            _accountRepository = ServiceLocator.Current.GetInstance<IAccountRepository>();
            _chatRepository = ServiceLocator.Current.GetInstance<IChatRepository>();

        }
        public static WebChatCachedDataUtils getInstance() {
            lock (lockobject) {
                if (instance == null) {
                    instance = new WebChatCachedDataUtils();
                }
                return instance;
            }
        }
        /// <summary>
        /// 根据情况，获取用户信息
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public UserInfo getUserInfo(int uid) {
            WebChatUserCacheInfo us = null;
            if (UserList.ContainsKey(uid)) {
                us =UserList[uid];
            }
            if (us == null)
            {
                //强制更新，且需要等待返回
                try
                {
                    UserInfo userInfo = _accountRepository.GetUserInfoById(uid);
                    if (userInfo != null) {
                        WebChatUserCacheInfo tmp = new WebChatUserCacheInfo()
                        {
                            UserId = uid,
                            User = userInfo,
                            IsUpdating = false,
                            ExpiredTime = System.DateTime.Now + new TimeSpan(1,0,0)
                        };
                        lock (lockobject)
                        {
                            if (UserList.ContainsKey(uid) ==false)
                                UserList.Add(uid, tmp);
                        }
                        us = tmp;
                    }

                }
                catch (Exception ex) {

                }
            }
            else {
                if (us.ExpiredTime > DateTime.Now && (us.IsUpdating ==false || (us.IsUpdating  ==true && (DateTime.Now - us.UpdatingTime).TotalMinutes > 2.0))) {
                    //已经超时，且未在更新状态，或者虽然在更新状态，但是启动更新时间到目前为止已经超过了2分钟，认为更新失败，可以重新启动更新了
                    us.UpdatingTime = System.DateTime.Now;
                    us.IsUpdating = true;
                    Task.Run(() => {
                        try
                        {
                            UserInfo userInfo = _accountRepository.GetUserInfoById(uid);
                            if (userInfo != null)
                            {
                               
                                us.User = userInfo;
                                us.IsUpdating = false;
                                us.ExpiredTime = System.DateTime.Now + new TimeSpan(1, 0, 0);
                            }
                        }
                        catch (Exception ex) {
                        }
                    });
                }
            }
            if (us == null) return null; 
            return us.User;
        }
        public ChatGroupModel GetGroupInfo(Guid gid) {
            WebChatGroupCacheInfo tmp = null;
            if (GroupList.ContainsKey(gid)) {
                tmp = GroupList[gid];
            }
            if (tmp == null)
            {
                try
                {
                    ChatGroupModel groupInfo = _chatRepository.GetGroupInfo(gid);
                    if (groupInfo != null)
                    {
                        WebChatGroupCacheInfo  grouptmp = new WebChatGroupCacheInfo()
                        {
                            GroupId = gid,
                            GroupInfo = groupInfo,
                            IsUpdating = false,
                            ExpiredTime = System.DateTime.Now + new TimeSpan(0, 10, 0)
                        };
                        lock (lockobject)
                        {
                            if (GroupList.ContainsKey(gid) == false)
                                GroupList.Add(gid, grouptmp);
                        }
                        tmp = grouptmp;
                    }

                }
                catch (Exception ex)
                {

                }
            }
            else {
                if (tmp.ExpiredTime > DateTime.Now && (tmp.IsUpdating == false || (tmp.IsUpdating == true && (DateTime.Now - tmp.UpdatingTime).TotalMinutes > 2.0))) {
                    tmp.UpdatingTime = System.DateTime.Now;
                    tmp.IsUpdating = true;
                    Task.Run(() => {
                        try
                        {
                            ChatGroupModel groupInfo = _chatRepository.GetGroupInfo(gid);
                            if (groupInfo != null)
                            {

                                tmp.GroupInfo = groupInfo;
                                tmp.IsUpdating = false;
                                tmp.ExpiredTime = System.DateTime.Now + new TimeSpan(0, 10, 0);
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    });
                }
            }

            if (tmp == null) return null;
            return tmp.GroupInfo;

        }
    }
    public class WebChatUserCacheInfo {
        public int UserId { get; set; }
        public UserInfo User { get; set; }
        public DateTime ExpiredTime { get; set; }
        public bool IsUpdating { get; set; }
        public DateTime UpdatingTime { get; set; }
    }
    public class WebChatGroupCacheInfo {
        public Guid GroupId { get; set; }
        public ChatGroupModel GroupInfo { get; set; }
        public DateTime ExpiredTime { get; set; }
        public bool IsUpdating { get; set; }
        public DateTime UpdatingTime { get; set; }
    }
}
