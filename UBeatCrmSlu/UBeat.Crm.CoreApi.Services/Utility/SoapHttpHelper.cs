using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml;
using UBeat.Crm.CoreApi.Core.Utility;

namespace UBeat.Crm.CoreApi.Services.Utility
{
    public class SoapRequestStatus
    {
        public bool IsSuccess { get; set; }
        public string Msg { get; set; }
    }

    public static class SoapHttpHelper
    {
        public static string SOAPTEMPLATE =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
 "<soap12:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap12=\"http://www.w3.org/2003/05/soap-envelope\">" +
 "  <soap12:Body>  " +
 "    <{0} xmlns=\"http://tempuri.org/\">" +
            "{1}" +
 "    </{0}> " +
 "  </soap12:Body>" +
 "</soap12:Envelope>";
        private static readonly IConfigurationRoot configurationRoot = ServiceLocator.Current.GetInstance<IConfigurationRoot>();

        public static SoapRequestStatus SendSoapRequest(string soapUrl, string xmlParam)
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
                }
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    XmlDocument doc = new XmlDocument();
                    StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                    String retXml = sr.ReadToEnd();
                    sr.Close();
                    doc.LoadXml(retXml);
                    XmlNamespaceManager mgr = new XmlNamespaceManager(doc.NameTable);
                    //此处是soap1.2，如果是soap1.1就应该如下：mgr.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");
                    mgr.AddNamespace("soap12", "http://www.w3.org/2003/05/soap-envelope");
                    //解析XML
                    var xmlNode = doc.SelectSingleNode("//soap12:Body/*", mgr);
                    Console.WriteLine(xmlNode.FirstChild.InnerText);
                }
            }
            catch (Exception ex)
            {
                return new SoapRequestStatus { IsSuccess = false, Msg = "【请求异常】：" + ex.Message };
            }
            return new SoapRequestStatus { IsSuccess = true, Msg = "请求:" + soapInterface + "成功" };
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
            return Type.GetType("UBeat.Crm.CoreApi.Services.Models.SoapErp."+ className);
        }
    }
}
