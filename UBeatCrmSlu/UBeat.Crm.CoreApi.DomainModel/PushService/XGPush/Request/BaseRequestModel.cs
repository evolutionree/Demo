using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.PushService.XGPush
{
    public class BaseRequestModel
    {
        /// <summary>
        /// 应用的唯一标识符，在提交应用时管理系统返回。可在xg.qq.com管理台查看
        /// </summary>
        public long access_id { set; get; }

        /// <summary>
        /// 本请求的unix时间戳，用于确认请求的有效期。默认情况下，请求时间戳与服务器时间（北京时间）偏差大于600秒则会被拒绝
        /// </summary>
        public long timestamp { set; get; }

        /// <summary>
        /// 配合timestamp确定请求的有效期，单位为秒，最大值为600。若不设置此参数或参数值非法，则按默认值600秒计算有效期
        /// </summary>
        public int valid_time { set; get; } = 600;

        /// <summary>
        /// 内容签名
        /// </summary>
        public string sign { set; get; }

        private Dictionary<String, object> map;

        public Dictionary<string, object> ToDictionary()
        {
            //因为反射影响效率，因此第一次调用后保存到全局变量
            if (map == null)
            {
                map = new Dictionary<string, object>();

                Type t = this.GetType();

                PropertyInfo[] pi = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (PropertyInfo p in pi)
                {
                    MethodInfo mi = p.GetGetMethod();
                    var value = p.GetValue(this);

                    if (mi != null && mi.IsPublic)
                    {
                        if (value == null|| value.ToString().Equals(string.Empty))
                            continue;
                        map.Add(p.Name, value);
                        //map.Add(p.Name, mi.Invoke(this, new Object[] { }));
                    }
                }
            }
            return map;
        }


    }
}
