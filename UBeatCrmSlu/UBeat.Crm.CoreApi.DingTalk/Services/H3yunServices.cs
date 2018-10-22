using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using UBeat.Crm.CoreApi.DingTalk.Repository;
using UBeat.Crm.CoreApi.DingTalk.Utils;
using UBeat.Crm.CoreApi.DomainModel.DynamicEntity;
using UBeat.Crm.CoreApi.DomainModel.WorkFlow;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Services;

namespace UBeat.Crm.CoreApi.DingTalk.Services
{
    public class H3yunServices : EntityBaseServices
    {
        private readonly IWorkFlowRepository _workFlowRepository;
        private readonly IDynamicEntityRepository _dynamicEntityRepository;
        private readonly IH3YunRepository _iH3YunRepository;
        public H3yunServices(IWorkFlowRepository workFlowRepository
            , IDynamicEntityRepository dynamicEntityRepository
            , IH3YunRepository iH3YunRepository )
        {
            _workFlowRepository = workFlowRepository;
            _dynamicEntityRepository = dynamicEntityRepository;
            _iH3YunRepository = iH3YunRepository;
        }
        public void SendToH3Yun(DbTransaction tran ,Guid caseid,  int nodenum, int userno) {
            //根据CaseId获取单据单据id
            WorkFlowCaseInfo caseInfo = _workFlowRepository.GetWorkFlowCaseInfo(tran, caseid);
            if (caseInfo == null) return;

            string code = _iH3YunRepository.GetH3Code(caseInfo.FlowId.ToString());
            if (code == null || code.Length == 0) {
                throw (new Exception("氚云工作流未定义"));
            }
            DynamicEntityDetailtMapper mapper = new DynamicEntityDetailtMapper() {
                RecId = caseInfo.RecId,
                EntityId = caseInfo.EntityId,
                NeedPower = 0
            };
            IDictionary<string, object> entityData = _dynamicEntityRepository.Detail(mapper, 1, tran);
            Hashtable data2 = new Hashtable();
            foreach (string key in entityData.Keys) {
                if (key.EndsWith("_name")) continue;
                if (entityData.ContainsKey(key + "_name"))
                {
                    data2.Add(key, entityData[key + "_name"]);
                }
                else
                {
                    data2.Add(key, entityData[key]);
                }
            }

            H3ConfigInfo config = new H3ConfigInfo();
            Hashtable real = new Hashtable();
            real.Add("schemaCode", code);
            real.Add("objData", JsonConvert.SerializeObject(data2));
            real.Add("submit", true);
            SoapV1_1WebService(config, "https://www.h3yun.com/Webservices/BizObjectService.asmx", "CreateBizObject", real, "http://tempuri.org/");
        }
        public static string SoapV1_1WebService(H3ConfigInfo config,String URL, String MethodName, Hashtable Pars, string XmlNs)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(URL);
            request.Method = "POST";
            request.ContentType = "text/xml; charset=utf-8";
            request.Headers.Add("SOAPAction", "\"" + XmlNs + (XmlNs.EndsWith("/") ? "" : "/") + MethodName + "\"");
            request.Headers.Add("CorpId", config.DingDingCode);
            request.Headers.Add("EngineCode", config.EngineCode);
            request.Headers.Add("Secret", config.Secret);
            // 凭证
            request.Credentials = CredentialCache.DefaultCredentials;
            //超时时间
            request.Timeout = 10000;
            byte[] data = HashtableToSoap(Pars, XmlNs, MethodName);
            request.ContentLength = data.Length;
            Stream writer = request.GetRequestStream();
            writer.Write(data, 0, data.Length);
            writer.Close();
            var response = request.GetResponse();
            XmlDocument doc = new XmlDocument();
            StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            String retXml = sr.ReadToEnd();
            sr.Close();
            doc.LoadXml(retXml);
            XmlNamespaceManager mgr = new XmlNamespaceManager(doc.NameTable);
            mgr.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");
            String xmlStr = doc.SelectSingleNode("//soap:Body/*/*", mgr).InnerXml;

            return xmlStr;
        }
        private static string ObjectToSoapXml(object o)
        {
            XmlSerializer mySerializer = new XmlSerializer(o.GetType());
            MemoryStream ms = new MemoryStream();
            mySerializer.Serialize(ms, o);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(Encoding.UTF8.GetString(ms.ToArray()));
            if (doc.DocumentElement != null)
            {
                return doc.DocumentElement.InnerXml;
            }
            else
            {
                return o.ToString();
            }
        }
        private static byte[] HashtableToSoap(Hashtable ht, String XmlNs, String MethodName)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\"></soap:Envelope>");
            XmlDeclaration decl = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            doc.InsertBefore(decl, doc.DocumentElement);
            XmlElement header = doc.CreateElement("soap", "Header", "http://schemas.xmlsoap.org/soap/envelope/");
            XmlElement auth = doc.CreateElement("Authentication");
            auth.SetAttribute("xmlns", XmlNs);
            H3ConfigInfo config = new H3ConfigInfo();
            XmlElement item1 = doc.CreateElement("CorpId");
            item1.InnerXml = ObjectToSoapXml(config.DingDingCode);
            auth.AppendChild(item1);
            item1 = doc.CreateElement("EngineCode");
            item1.InnerXml = ObjectToSoapXml(config.EngineCode);
            auth.AppendChild(item1);
            item1 = doc.CreateElement("Secret");
            item1.InnerXml = ObjectToSoapXml(config.Secret);
            auth.AppendChild(item1);
            header.AppendChild(auth);
            doc.DocumentElement.AppendChild(header);

            XmlElement soapBody = doc.CreateElement("soap", "Body", "http://schemas.xmlsoap.org/soap/envelope/");

            XmlElement soapMethod = doc.CreateElement(MethodName);
            soapMethod.SetAttribute("xmlns", XmlNs);
            foreach (string k in ht.Keys)
            {

                XmlElement soapPar = doc.CreateElement(k);
                soapPar.InnerXml = ObjectToSoapXml(ht[k]);
                soapMethod.AppendChild(soapPar);
            }
            soapBody.AppendChild(soapMethod);
            doc.DocumentElement.AppendChild(soapBody);
            return Encoding.UTF8.GetBytes(doc.OuterXml);
        }
    }
}
