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

namespace UBeat.Crm.CoreApi.Services.Utility
{
    public class SoapRequestStatus
    {
        public bool IsSuccess { get; set; }
        public string Msg { get; set; }
    }
    public enum DataTypeEnum
    {
        SingleChoose = 1,
        MultiChoose = 2,
        ChoosePerson = 3
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
        public static string SOAPTEMPLATE =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
 "<soap12:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap12=\"http://www.w3.org/2003/05/soap-envelope\">" +
 "  <soap12:Body>  " +
 "    <{0} xmlns=\"http://tempuri.org/\">" +
            "ns2:updateCustomerFromCrm xmlns:ns2="http://erp.service.ceews.ceepcb.com/"+
            "{1}" +
 "    </{0}> " +
 "  </soap12:Body>" +
 "</soap12:Envelope>";
        private static readonly IConfigurationRoot configurationRoot = ServiceLocator.Current.GetInstance<IConfigurationRoot>();

        public static SoapRequestStatus SendSoapRequest(string soapUrl, string xmlParam, string logId, int userId)
        {
            WebRequest request = HttpWebRequest.Create(soapUrl);
            byte[] bs = Encoding.UTF8.GetBytes(xmlParam);
            request.Method = "POST";
            request.Timeout = 6000;
            //SOAP1.1调用时需加上头部信息request.Headers.Add("SOAPAction", "http://tempuri.org/HelloWorld");
            request.ContentType = "application/soap+xml; charset=UTF-8";
            request.ContentLength = bs.Length;
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
                    Log(new List<string> { "soapresstatus" }, new List<string> { response.StatusCode == HttpStatusCode.OK ? "200:返回成功" : (int)response.StatusCode + ":返回异常" }, 0, userId, logId);
                    XmlNamespaceManager mgr = new XmlNamespaceManager(doc.NameTable);
                    //此处是soap1.2，如果是soap1.1就应该如下：mgr.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");
                    mgr.AddNamespace("soap12", "http://www.w3.org/2003/05/soap-envelope");
                    //解析XML
                    var xmlNode = doc.SelectSingleNode("//soap12:Body/*", mgr);
                    Console.WriteLine(xmlNode.FirstChild.InnerText);
                    Log(new List<string> { "soapresresult" }, new List<string> { xmlNode.FirstChild.InnerText }, 0, userId, logId);
                }
            }
            catch (Exception ex)
            {
                Log(new List<string> { "soapexceptionmsg" }, new List<string> { ex.Message + " || " + ex.InnerException.Message }, 0, userId, logId);
                return new SoapRequestStatus { IsSuccess = false, Msg = "【请求异常】：" + ex.Message };
            }
            return new SoapRequestStatus { IsSuccess = true, Msg = "请求:" + soapUrl + "成功" };
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
            IDynamicEntityRepository dynamicEntityRepository = ServiceLocator.Current.GetInstance<DynamicEntityRepository>();
            OperateResult result;
            var fielddata = new Dictionary<string, object>();
            for (int i = 0; i < fields.Count; i++)
            {
                fielddata.Add(fields[i], contents[i]);
            }
            if (isUpdate == 0)
            {
                result = dynamicEntityRepository.DynamicAdd(null, Guid.Parse("22e0700c-e829-4c1b-bb4a-ec5282d359b7"), fielddata, null, userId);
                return Guid.Parse(result.Codes);
            }
            else
            {
                result = dynamicEntityRepository.DynamicEdit(null, Guid.Parse("22e0700c-e829-4c1b-bb4a-ec5282d359b7"), Guid.Parse(recId), fielddata, userId);
                return Guid.Parse(recId);
            }

        }

        public static object ValueConvert(string key, Dictionary<string, object> detail, Type type)
        {
            if (detail[key] == null) return string.Empty;
            var property = type.GetProperty(key);
            var customPro = property.GetCustomAttribute<DataTypeAttribute>();
            var customCla = type.GetCustomAttribute<EntityInfoAttribute>();
            if (customPro == null) return detail[key];
            if (customCla == null) throw new Exception("Soap的DTO实体没有配置EntityInfo");
            var dynamicRepository = ServiceLocator.Current.GetInstance<IDynamicEntityRepository>();
            var accountRepository = ServiceLocator.Current.GetInstance<IAccountRepository>();
            switch (customPro.type)
            {
                case DataTypeEnum.SingleChoose:
                case DataTypeEnum.MultiChoose:
                    var dicRepository = ServiceLocator.Current.GetInstance<IDataSourceRepository>();
                    var fields = dynamicRepository.GetEntityFields(Guid.Parse(customCla.EntityId), 1);
                    var field = fields.FirstOrDefault(t => t.FieldName == key);
                    if (field == null) return detail[key];
                    DynamicProtocolFieldConfig config = Newtonsoft.Json.JsonConvert.DeserializeObject<DynamicProtocolFieldConfig>(field.FieldConfig);
                    var dicValues = dicRepository.SelectFieldDicVaue(Convert.ToInt32(config.DataSource.SourceId), 1);
                    var ids = detail[key].ToString().Split(",");
                    string s = string.Empty;
                    foreach (var id in ids)
                    {
                        var dicValue = dicValues.FirstOrDefault(t => t.DataId == Convert.ToInt32(id));
                        s += dicValue.ExtField1 + ",";
                    }
                    detail[key] = s.Substring(0, s.Length - 2);
                    break;
                case DataTypeEnum.ChoosePerson:
                    var userInfos = accountRepository.GetAllUserInfoList();
                    break;
            }
            return detail[key];
        }
    }
}