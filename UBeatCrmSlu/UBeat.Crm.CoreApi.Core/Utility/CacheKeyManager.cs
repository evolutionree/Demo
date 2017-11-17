using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Core.Utility
{
    public static class CacheKeyManager
    {
        /// <summary>
        /// 用户个人数据的缓存key
        /// </summary>
        public const string UserData_Profix = "UserData_";
        /// <summary>
        /// 用户个人数据有效期
        /// </summary>
        public static TimeSpan UserDataExpires = new TimeSpan(1, 0, 0, 0);


        /// <summary>
        /// 数据版本缓存key
        /// </summary>
        public static string DataVersionKey = "DataVersion_Everyone";
        /// <summary>
        /// 数据版本缓存有效期
        /// </summary>
        public static TimeSpan DataVersionExpires = new TimeSpan(0, 0, 30, 0);


        /// <summary>
        /// 所有的功能信息缓存key
        /// </summary>
        public static string TotalFunctionsDataKey = "TotalFunctionsData_Everyone";
        /// <summary>
        /// 所有的功能信息缓存有效期
        /// </summary>
        public static TimeSpan TotalFunctionsDataExpires = new TimeSpan(1, 0, 0, 0);



        /// <summary>
        /// 扩展二开方法的缓存key
        /// </summary>
        public static string ActionExtDataKey = "ActionExtData_Everyone";
        /// <summary>
        /// 扩展二开方法的缓存有效期
        /// </summary>
        public static TimeSpan ActionExtDataExpires = new TimeSpan(1, 0, 0, 0);

        /// <summary>
        /// 消息配置缓存key
        /// </summary>
        public static string MessageConfigKey = "MessageConfig_Everyone";
        /// <summary>
        /// 消息配置的缓存有效期
        /// </summary>
        public static TimeSpan MessageConfigExpires = new TimeSpan(1, 0, 0, 0);


    }
}
