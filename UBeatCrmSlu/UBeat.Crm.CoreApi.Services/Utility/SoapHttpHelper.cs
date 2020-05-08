﻿using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Repository.DynamicEntity;
using System.Linq;
using UBeat.Crm.CoreApi.Services.Models.DynamicEntity;
using System.Collections;
using System.Xml.Serialization;
using Newtonsoft.Json.Linq;

namespace UBeat.Crm.CoreApi.Services.Utility
{
    public class SoapRequestStatus
    {
        public bool IsSuccess { get; set; }
        public string Msg { get; set; }
        public object Data { get; set; }
    }
    public enum DataTypeEnum
    {
        SingleChoose = 1,
        MultiChoose = 2,
        ChoosePerson = 3,
        AttachFile = 4,
        Address = 5
    }
    public class DataTypeAttribute : Attribute
    {

        /// <summary>
        /// 1单选，2多选 ，3选人.....
        /// </summary>
        public DataTypeEnum type { get; set; }
        public DataTypeAttribute(DataTypeEnum type)
        {
            this.type = type;
        }
    }
    public class EntityInfoAttribute : Attribute
    {
        public string EntityId { get; set; }
        public EntityInfoAttribute(string entityId)
        {
            this.EntityId = entityId;
        }
    }
    public static class SoapHttpHelper
    {

        private static readonly IConfigurationRoot configurationRoot = ServiceLocator.Current.GetInstance<IConfigurationRoot>();
        private static readonly string contentType = "text/xml; charset=UTF-8";

        private static void SetWebRequest(HttpWebRequest request)
        {
            request.Credentials = CredentialCache.DefaultCredentials;
            request.Timeout = 10000;
        }
        public static SoapRequestStatus SendSoapRequest(string soapUrl, string xmlParam, string xmlPath, string logId, int userId)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(soapUrl);
            byte[] bs = Encoding.UTF8.GetBytes(xmlParam);
            request.Method = "POST";
            request.ContentType = contentType;
            request.ProtocolVersion = HttpVersion.Version11;
            request.ContentLength = bs.Length;
            SetWebRequest(request);
            try
            {
                using (Stream reqStream = request.GetRequestStream())
                {
                    reqStream.Write(bs, 0, bs.Length);
                    reqStream.Close();
                    Log(new List<string> { "soapreqstatus" }, new List<string> { "请求成功" }, 0, userId, logId);
                }
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    XmlDocument doc = new XmlDocument();
                    StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                    String retXml = sr.ReadToEnd();
                    sr.Close();
                    doc.LoadXml(retXml);
                    var node = doc.SelectSingleNode(xmlPath, GetNameSpaceManager(doc));
                    Log(new List<string> { "soapresstatus", "soapresresult" }, new List<string> { response.StatusCode == HttpStatusCode.OK ? "200:返回成功" : (int)response.StatusCode + ":返回异常", doc.OuterXml }, 0, userId, logId);
                    return new SoapRequestStatus { IsSuccess = true, Msg = "请求:" + soapUrl + "成功", Data = node };
                }
            }
            catch (Exception ex)
            {
                Log(new List<string> { "soapexceptionmsg" }, new List<string> { ex.Message + " || " + ex.InnerException.Message }, 0, userId, logId);
                return new SoapRequestStatus { IsSuccess = false, Msg = "【请求异常】：" + ex.Message };
            }
        }
        static XmlNamespaceManager GetNameSpaceManager(XmlDocument Document)
        {
            XmlNamespaceManager objXmlNamespaceManager = new XmlNamespaceManager(Document.NameTable);
            objXmlNamespaceManager.AddNamespace("soap", Document.DocumentElement.GetNamespaceOfPrefix("soap"));
            return objXmlNamespaceManager;
        }

        public static Dictionary<string, string> GetParamValueKV(Type t)
        {
            PropertyInfo[] peroperties = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (PropertyInfo property in peroperties)
            {
                object[] objs = property.GetCustomAttributes(typeof(JsonPropertyAttribute), true);
                if (objs.Length > 0)
                {
                    dic.Add(property.Name.ToLower(), ((JsonPropertyAttribute)objs[0]).PropertyName);
                }
            }
            return dic;
        }
        public static Type GetType(string className)
        {
            return Type.GetType("UBeat.Crm.CoreApi.Services.Models.SoapErp." + className);
        }
        public static Guid Log(List<string> fields, List<string> contents, int isUpdate, int userId, string recId = "")
        {
            IDynamicEntityRepository dynamicEntityRepository = ServiceLocator.Current.GetInstance<IDynamicEntityRepository>();
            OperateResult result;
            var fielddata = new Dictionary<string, object>();
            for (int i = 0; i < fields.Count; i++)
            {
                fielddata.Add(fields[i], contents[i]);
            }
            if (isUpdate == 0)
            {
                result = dynamicEntityRepository.DynamicAdd(null, Guid.Parse("22e0700c-e829-4c1b-bb4a-ec5282d359b7"), fielddata, null, userId);
                return Guid.Parse(result.Id);
            }
            else
            {
                result = dynamicEntityRepository.DynamicEdit(null, Guid.Parse("22e0700c-e829-4c1b-bb4a-ec5282d359b7"), Guid.Parse(recId), fielddata, userId);
                return Guid.Parse(recId);
            }

        }

        public static void ValueConvert(KeyValuePair<string, string> kv, IDictionary<string, object> detail, Type type)
        {
            if (detail[kv.Key] == null) return;
            var property = type.GetProperties().FirstOrDefault(t => t.Name.ToLower() == kv.Key);
            var customPro = property.GetCustomAttribute<DataTypeAttribute>();
            var customCla = type.GetCustomAttribute<EntityInfoAttribute>();
            if (customPro == null) return;
            if (customCla == null) throw new Exception("Soap的DTO实体没有配置EntityInfo");
            var dynamicRepository = ServiceLocator.Current.GetInstance<IDynamicEntityRepository>();
            var accountRepository = ServiceLocator.Current.GetInstance<IAccountRepository>();
            switch (customPro.type)
            {
                case DataTypeEnum.SingleChoose:
                case DataTypeEnum.MultiChoose:
                    var dicRepository = ServiceLocator.Current.GetInstance<IDataSourceRepository>();
                    var fields = dynamicRepository.GetEntityFields(Guid.Parse(customCla.EntityId), 1);
                    var field = fields.FirstOrDefault(t => t.FieldName == kv.Key);
                    if (field == null) return;
                    DynamicProtocolFieldConfig config = Newtonsoft.Json.JsonConvert.DeserializeObject<DynamicProtocolFieldConfig>(field.FieldConfig);
                    var dicValues = dicRepository.SelectFieldDicVaue(Convert.ToInt32(config.DataSource.SourceId), 1);
                    var ids = detail[kv.Key].ToString().Split(",");
                    string s = string.Empty;
                    foreach (var id in ids)
                    {
                        var dicValue = dicValues.FirstOrDefault(t => t.DataId == Convert.ToInt32(id));
                        s += dicValue.ExtField1 + ",";
                    }
                    detail[kv.Key] = s.Substring(0, s.Length - 1);
                    break;
                case DataTypeEnum.ChoosePerson:
                    var userInfos = accountRepository.GetAllUserInfoList();
                    var userInfo = userInfos.FirstOrDefault(t => t.UserId == Convert.ToInt32((detail[kv.Key] ?? 0)));
                    detail[kv.Key] = userInfo == null ? string.Empty : userInfo.RelateErpUserId;
                    break;
                case DataTypeEnum.AttachFile:
                    var files = new List<string>();
                    var attach = JArray.Parse((detail[kv.Key] ?? "{}").ToString());
                    foreach (var t in attach)
                    {
                        files.Add(string.Format(configurationRoot.GetSection("FileServiceSetting").GetValue<string>("ReadUrl"), t["fileid"]));
                    }
                    detail[kv.Key] = files;
                    break;
                case DataTypeEnum.Address:
                    var addr = JObject.Parse((detail[kv.Key] ?? "{}").ToString());
                    detail[kv.Key] = addr.HasValues ? addr["address"].ToString() : string.Empty;
                    break;
            }
            return;
        }
        public static T OutPutERPData<T>(IDictionary<string, object> detail)
        {
            var instance = CreateInstance<T>(typeof(T));
            var properties = typeof(T).GetProperties();
            foreach (var pro in properties)
            {
                if (pro.GetCustomAttribute<JsonPropertyAttribute>() == null) continue;
                pro.SetValue(instance, detail[pro.Name.ToLower()] ?? string.Empty);
            }
            return instance;
        }

        /// <summary>
        /// 创建对象实例
        /// </summary>
        /// <typeparam name="T">要创建对象的类型</typeparam>
        /// <param name="assemblyName">类型所在程序集名称</param>
        /// <param name="nameSpace">类型所在命名空间</param>
        /// <param name="className">类型名</param>
        /// <returns></returns>
        static T CreateInstance<T>(Type t)
        {
            try
            {
                object obj = Activator.CreateInstance(t, true);//根据类型创建实例
                return (T)obj;//类型转换并返回
            }
            catch
            {
                //发生异常，返回类型的默认值
                return default(T);
            }
        }
    }
}
