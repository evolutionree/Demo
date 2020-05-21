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
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Repository.DynamicEntity;
using System.Linq;
using UBeat.Crm.CoreApi.Services.Models.DynamicEntity;
using System.Collections;
using System.Xml.Serialization;
using Newtonsoft.Json.Linq;
using UBeat.Crm.CoreApi.Services.Models.SoapErp;
using UBeat.Crm.CoreApi.Repository.Utility;
using System.Data.Common;

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
        Address = 5,
        DataSouce = 6,
        RelateEntity = 7,
        DateTime = 8,
        Region = 9,
        Default = 0
    }
    public enum FieldTypeEnum
    {
        Int = 1,
        Text = 2,
        Jsonb = 3
    }
    public class DataTypeAttribute : Attribute
    {

        /// <summary>
        /// 1单选，2多选 ，3选人.....
        /// </summary>
        public DataTypeEnum type { get; set; }
        public Type relateEntity { get; set; }
        public string bindingMethod { get; set; }
        public DataTypeAttribute(DataTypeEnum type)
        {
            this.type = type;
        }
        public DataTypeAttribute(DataTypeEnum type, Type relateEntity)
        {
            this.type = type;
            this.relateEntity = relateEntity;
        }
        public DataTypeAttribute(DataTypeEnum type, string bindingMethod)
        {
            this.type = type;
            this.bindingMethod = bindingMethod;
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
    public class EntityFieldAttribute : Attribute
    {
        public string FieldName { get; set; }
        public FieldTypeEnum FieldType { get; set; }
        public EntityFieldAttribute(string fieldName)
        {
            this.FieldName = fieldName;
        }
        public EntityFieldAttribute(string fieldName, FieldTypeEnum fieldType)
        {
            this.FieldName = fieldName;
            this.FieldType = fieldType;
        }
    }
    public static class SoapHttpHelper
    {
        private static readonly IConfigurationRoot configurationRoot = ServiceLocator.Current.GetInstance<IConfigurationRoot>();
        private static readonly string contentType = "text/xml; charset=UTF-8";
        private static readonly string[] custEntityIds = new string[] {
            "59cf141c-4d74-44da-bca8-3ccf8582a1f2",//产品
            "b56a7264-46b2-43d2-b22e-e5d777fb00db",//发货单
            "0d6d41d5-f913-4ccf-8ffd-1414fd9ed736"
        };
        private static readonly string[] productEntityIds = new string[] {
            "0d6d41d5-f913-4ccf-8ffd-1414fd9ed736"
        };
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
            if (detail[kv.Key] == null) { detail[kv.Key] = detail[kv.Key] ?? string.Empty; }
            var property = type.GetProperties().FirstOrDefault(t => t.Name.ToLower() == kv.Key);
            var customPro = property.GetCustomAttribute<DataTypeAttribute>();
            var customCla = type.GetCustomAttribute<EntityInfoAttribute>();
            if (customPro == null)
            {
                customPro = new DataTypeAttribute(DataTypeEnum.Default);
            }
            if (customCla == null) throw new Exception("Soap的DTO实体没有配置EntityInfo");
            var dynamicRepository = ServiceLocator.Current.GetInstance<IDynamicEntityRepository>();
            var accountRepository = ServiceLocator.Current.GetInstance<IAccountRepository>();
            switch (customPro.type)
            {
                case DataTypeEnum.Region:
                    IBasicDataRepository _basicDataRepository = ServiceLocator.Current.GetInstance<IBasicDataRepository>();
                    var dic = new Dictionary<string, Int64>();
                    dic.Add("regionsync", 0);
                    var dataList = _basicDataRepository.SyncDataBasic(new DomainModel.BasicData.SyncDataMapper
                    {
                        VersionKey = dic
                    }, 0);
                    var pageData = dataList["region"];
                    IDictionary<string, object> data = new Dictionary<string, object>();
                    if (pageData != null)
                    {
                        if (detail[kv.Key] == null) return;
                        var countryVal = detail["country"];
                        if (countryVal == null && string.IsNullOrEmpty(countryVal.ToString())) return;
                        IDataSourceRepository _dataSourceRepository = ServiceLocator.Current.GetInstance<IDataSourceRepository>();
                        var dicDetail = _dataSourceRepository.SelectFieldDicVaue(53, 0);
                        if (dicDetail == null && dicDetail.Count == 0) return;
                        var dicVal = dicDetail.FirstOrDefault(t => t.DataId == Convert.ToInt32(countryVal.ToString()));
                        if (dic == null) return;
                        if (string.IsNullOrEmpty(dicVal.ExtField1)) return;
                        if (dicVal.ExtField1 == "G001")
                        {
                            data = (pageData as List<IDictionary<string, object>>).FirstOrDefault(t => (t["regionid"] == null ? string.Empty : t["regionid"].ToString()) == detail[kv.Key].ToString());
                        }
                        else
                        {
                            data = new Dictionary<string, object>();
                            data.Add("regioncode", dicVal.ExtField1);
                        }
                    }
                    if (data.Keys.Count==0) return;
                    detail[kv.Key] = data == null ? string.Empty : data["regioncode"].ToString();
                    return;
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
                    TypeConvert(property, detail, kv);
                    return;
                case DataTypeEnum.ChoosePerson:
                    var userInfos = accountRepository.GetAllUserInfoList();
                    var userInfo = userInfos.FirstOrDefault(t => t.UserId == (string.IsNullOrEmpty(detail[kv.Key].ToString()) ? 0 : Convert.ToInt32(detail[kv.Key].ToString())));
                    detail[kv.Key] = userInfo == null ? string.Empty : userInfo.RelateErpUserId;
                    return;
                case DataTypeEnum.AttachFile:
                    var files = new List<string>();
                    var attach = JArray.Parse((detail[kv.Key] ?? "{}").ToString());
                    foreach (var t in attach)
                    {
                        files.Add(string.Format(configurationRoot.GetSection("FileServiceSetting").GetValue<string>("ReadUrl"), t["fileid"]));
                    }
                    detail[kv.Key] = files;
                    return;
                case DataTypeEnum.Address:
                    var addr = JObject.Parse((detail[kv.Key] ?? "{}").ToString());
                    detail[kv.Key] = addr.HasValues ? addr["address"].ToString() : string.Empty;
                    break;
                case DataTypeEnum.DataSouce:
                    var ds = JObject.Parse((detail[kv.Key] ?? "{}").ToString());
                    detail[kv.Key] = ds.HasValues ? ds["name"].ToString() : string.Empty;
                    break;
                case DataTypeEnum.RelateEntity:
                    var proType = property.GetCustomAttribute<DataTypeAttribute>();
                    if (proType != null)
                    {
                        Type genericArgTypes = proType.relateEntity;
                        var t = GetParamValueKV(genericArgTypes);
                        if (detail[kv.Key] is List<IDictionary<string, object>>)
                        {
                            var p = detail[kv.Key] as List<IDictionary<string, object>>;
                            for (int i = 0; i < p.Count; i++)
                            {
                                IDictionary<string, object> dicVal = new Dictionary<string, object>();
                                foreach (var k in t)
                                {
                                    ValueConvert(k, p[i], genericArgTypes);
                                    dicVal.Add(k.Value, p[i][k.Key]);
                                }
                                p[i] = dicVal;
                            }
                        }
                        else if (detail[kv.Key] is IDictionary<string, object>)
                        {
                            var p = detail[kv.Key] as IDictionary<string, object>;
                            foreach (var k in t)
                            {
                                ValueConvert(k, p, genericArgTypes);
                            }
                        }
                    }
                    return;
                case DataTypeEnum.Default:
                    TypeConvert(property, detail, kv);
                    break;
            }

        }
        static void TypeConvert(PropertyInfo property, IDictionary<string, object> detail, KeyValuePair<string, string> kv)
        {
            var entityField = property.GetCustomAttribute<EntityFieldAttribute>();
            if (entityField != null)
            {
                switch (entityField.FieldType)
                {
                    case FieldTypeEnum.Int:
                        detail[kv.Key] = (string.IsNullOrEmpty(detail[kv.Key].ToString())) ? 0 : Convert.ToInt32(detail[kv.Key].ToString());
                        return;
                    case FieldTypeEnum.Text:
                        detail[kv.Key] = (string.IsNullOrEmpty(detail[kv.Key].ToString())) ? string.Empty : detail[kv.Key].ToString();
                        return;
                    default:

                        return;
                }

            }
        }
        public static T OutPutERPData<T>(IDictionary<string, object> detail)
        {
            var instance = CreateInstance<T>(typeof(T));
            var properties = typeof(T).GetProperties();
            foreach (var pro in properties)
            {
                if (pro.GetCustomAttribute<JsonIgnoreAttribute>() != null) continue;
                if (pro.GetCustomAttribute<JsonPropertyAttribute>() == null && !detail.Keys.Contains(pro.Name.ToLower())) continue;
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
        public static List<T> ConvertDataFormat<T>(string data, int userId)
        {
            var dto = JsonConvert.DeserializeObject<List<T>>(data);
            var properties = typeof(T).GetProperties();
            var customCla = typeof(T).GetCustomAttribute<EntityInfoAttribute>();
            List<T> actData = new List<T>();
            foreach (var t in dto)
            {
                var instance = CreateInstance<T>(typeof(T));
                foreach (var p in properties)
                {
                    var customPro = p.GetCustomAttribute<DataTypeAttribute>();
                    if (customPro == null)
                    {
                        p.SetValue(instance, p.GetValue(t));
                        continue;
                    }
                    var dynamicRepository = ServiceLocator.Current.GetInstance<IDynamicEntityRepository>();
                    switch (customPro.type)
                    {
                        case DataTypeEnum.DateTime:
                            p.SetValue(instance, Convert.ToDateTime(p.GetValue(t)));
                            break;
                        case DataTypeEnum.SingleChoose:
                            var dicRepository = ServiceLocator.Current.GetInstance<IDataSourceRepository>();
                            var fields = dynamicRepository.GetEntityFields(Guid.Parse(customCla.EntityId), 1);
                            var field = fields.FirstOrDefault(t1 => t1.FieldName == p.GetCustomAttribute<EntityFieldAttribute>().FieldName);
                            DynamicProtocolFieldConfig config = Newtonsoft.Json.JsonConvert.DeserializeObject<DynamicProtocolFieldConfig>(field.FieldConfig);
                            var dicValues = dicRepository.SelectFieldDicVaue(Convert.ToInt32(config.DataSource.SourceId), 1);
                            var value = p.GetValue(t);
                            if (value != null)
                            {
                                var dicValue = dicValues.FirstOrDefault(t1 => t1.ExtField1 == value.ToString());
                                p.SetValue(instance, dicValue == null ? "0" : dicValue.DataId.ToString());
                            }
                            break;
                        case DataTypeEnum.DataSouce:
                            var thisType = typeof(SoapHttpHelper);
                            var methods = thisType.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Where(m => !m.IsSpecialName);
                            var method = methods.FirstOrDefault(t1 => t1.Name == customPro.bindingMethod);
                            if (method == null) throw new Exception("数据源配置为空");
                            var genMethod = method.MakeGenericMethod(t.GetType());
                            genMethod.Invoke(thisType, new object[5] { p, t, instance, dynamicRepository, userId });
                            break;
                        case DataTypeEnum.RelateEntity:
                            p.SetValue(instance, p.GetValue(t));
                            break;
                    }
                }
                actData.Add(instance);
            }
            return actData;
        }
        #region   
        static void CustDataSource<T>(PropertyInfo p, T oldinstance, T newinstance, IDynamicEntityRepository dynamicRepository, int userId)
        {
            var dataList = dynamicRepository.DataList(new PageParam { PageIndex = 1, PageSize = int.MaxValue }, null, new DomainModel.DynamicEntity.DynamicEntityListMapper
            {
                EntityId = Guid.Parse("f9db9d79-e94b-4678-a5cc-aa6e281c1246"),
                ExtraData = null,
                ViewType = 0,
                MenuId = "f3219ea4-7701-4a19-87d8-1ecf1d27ca38",
                NeedPower = 0,
                SearchQuery = " AND custcode='" + p.GetValue(oldinstance) + "'"
            }, userId);
            var pageData = dataList["PageData"];
            if (pageData != null && pageData.Count > 0)
            {
                var tmpData = pageData.FirstOrDefault();
                var detail = dynamicRepository.Detail(new DomainModel.DynamicEntity.DynamicEntityDetailtMapper
                {
                    EntityId = Guid.Parse("f9db9d79-e94b-4678-a5cc-aa6e281c1246"),
                    NeedPower = 0,
                    RecId = Guid.Parse(tmpData["recid"].ToString())
                }, userId);
                p.SetValue(newinstance, "{\"id\":\"" + detail["recid"].ToString() + "\",\"name\":\"" + detail["recname"].ToString() + "\"}");
            }
        }
        static void ProductDataSource<T>(PropertyInfo p, T oldinstance, T newinstance, IDynamicEntityRepository dynamicRepository, int userId)
        {
            IProductsRepository _productRepository = ServiceLocator.Current.GetInstance<IProductsRepository>();
            var dataList = _productRepository.GetNewProducts(null, " 1=1 ", new PageParam { PageIndex = 1, PageSize = 10 }, new DomainModel.Products.ProductList
            {
                IsAllProduct = true,
                ProductSeriesId = Guid.Parse("7f74192d-b937-403f-ac2a-8be34714278b"),
                RecStatus = 1,
                RecVersion = 0
            }, string.Empty, userId);
            var pageData = dataList["data"];
            if (pageData != null)
            {
                var oldVal = p.GetValue(oldinstance) == null ? string.Empty : p.GetValue(oldinstance).ToString();
                var tmpData = (pageData as List<Dictionary<string, object>>).FirstOrDefault(t => (t["productcode"] == null ? string.Empty : t["productcode"].ToString()) == oldVal);

                p.SetValue(newinstance, tmpData == null ? string.Empty : tmpData["recid"].ToString());
            }
        }
        static void OrderDataSource<T>(PropertyInfo p, T oldinstance, T newinstance, IDynamicEntityRepository dynamicRepository, int userId)
        {
            var dataList = dynamicRepository.DataList(new PageParam { PageIndex = 1, PageSize = int.MaxValue }, null, new DomainModel.DynamicEntity.DynamicEntityListMapper
            {
                EntityId = Guid.Parse("af949c2d-a101-46d5-a125-a9d0659959f0"),
                ExtraData = null,
                ViewType = 0,
                MenuId = "8886abf5-003b-4ee1-849c-79eeaa9f6ccb",
                NeedPower = 0,
                SearchQuery = " AND orderno='" + p.GetValue(oldinstance) + "'"
            }, userId);
            var pageData = dataList["PageData"];
            if (pageData != null && pageData.Count > 0)
            {
                var tmpData = pageData.FirstOrDefault();
                var detail = dynamicRepository.Detail(new DomainModel.DynamicEntity.DynamicEntityDetailtMapper
                {
                    EntityId = Guid.Parse("af949c2d-a101-46d5-a125-a9d0659959f0"),
                    NeedPower = 0,
                    RecId = Guid.Parse(tmpData["recid"].ToString())
                }, userId);
                p.SetValue(newinstance, "{\"id\":\"" + detail["recid"].ToString() + "\",\"name\":\"" + detail["recname"].ToString() + "\"}");
            }
        }
        static void RegionDataSource<T>(PropertyInfo p, T oldinstance, T newinstance, IDynamicEntityRepository dynamicRepository, int userId)
        {
            IBasicDataRepository _basicDataRepository = ServiceLocator.Current.GetInstance<IBasicDataRepository>();
            var dic = new Dictionary<string, Int64>();
            dic.Add("regionsync", 0);
            var dataList = _basicDataRepository.SyncDataBasic(new DomainModel.BasicData.SyncDataMapper
            {
                VersionKey = dic
            }, userId);
            var pageData = dataList["region"];
            if (pageData != null)
            {
                IDictionary<string, object> tmpData;
                var country = oldinstance.GetType().GetProperty("country");
                if (country == null)
                {
                    var oldVal = p.GetValue(oldinstance) == null ? string.Empty : p.GetValue(oldinstance).ToString();
                    tmpData = (pageData as List<IDictionary<string, object>>).FirstOrDefault(t => (t["regionid"] == null ? string.Empty : t["regionid"].ToString()) == oldVal);
                }
                else
                {
                    var countryVal = country.GetValue(oldinstance);
                    if (countryVal == null && string.IsNullOrEmpty(countryVal.ToString())) return;
                    IDataSourceRepository _dataSourceRepository = ServiceLocator.Current.GetInstance<IDataSourceRepository>();
                    var dicDetail = _dataSourceRepository.SelectFieldDicVaue(53, userId);
                    if (dicDetail == null && dicDetail.Count == 0) return;
                    var dicVal = dicDetail.FirstOrDefault(t => t.DataId == Convert.ToInt32(countryVal.ToString()));
                    if (dic == null) return;
                    if (string.IsNullOrEmpty(dicVal.ExtField1)) return;
                    if (dicVal.ExtField1 == "G001")
                    {
                        var oldVal = p.GetValue(oldinstance) == null ? string.Empty : p.GetValue(oldinstance).ToString();
                        tmpData = (pageData as List<IDictionary<string, object>>).FirstOrDefault(t => (t["regionid"] == null ? string.Empty : t["regionid"].ToString()) == oldVal);
                    }
                    else
                    {
                        tmpData = new Dictionary<string, object>();
                        tmpData.Add("regioncode", dicVal.ExtField1);
                    }
                }

                p.SetValue(newinstance, tmpData == null ? string.Empty : tmpData["regioncode"].ToString());
            }
        }
        #endregion

        static object ConvertFieldValue<T>(PropertyInfo p, T data)
        {
            var fieldType = p.GetCustomAttribute<EntityFieldAttribute>();
            if (fieldType == null) return null;
            switch (fieldType.FieldType)
            {
                case FieldTypeEnum.Int:
                    var val = p.GetValue(data);
                    if (val == null)
                        return (object)0;
                    return (object)(Convert.ToInt32(val));
                case FieldTypeEnum.Jsonb:
                    val = p.GetValue(data);
                    if (val == null)
                        return JObject.Parse("{}");
                    return (object)(val.ToString());
                default:
                    val = p.GetValue(data);
                    return val;
            }
        }
        public static List<Dictionary<string, object>> PersistenceEntityData<T>(string data, int userId, string logId)
        {
            var tData = ConvertDataFormat<T>(data, userId);
            var dynamicRepository = ServiceLocator.Current.GetInstance<IDynamicEntityRepository>();
            var customCla = typeof(T).GetCustomAttribute<EntityInfoAttribute>();
            if (customCla == null) throw new Exception("Soap的DTO实体没有配置EntityInfo");
            Dictionary<string, object> fieldData;
            var properties = typeof(T).GetProperties();
            List<Dictionary<string, object>> dic = new List<Dictionary<string, object>>();
            foreach (var t in tData)
            {
                fieldData = new Dictionary<string, object>();
                foreach (var p in properties)
                {
                    fieldData.Add(p.GetCustomAttribute<EntityFieldAttribute>().FieldName, ConvertFieldValue(p, t));
                }
                dic.Add(fieldData);
            }
            return dic;
        }
        public static List<Dictionary<string, object>> PersistenceEntityData<T, T1>(string data, int userId, string logId)
        {
            var tData = ConvertDataFormat<T>(data, userId);
            var propertyInfos = GetSubDetailPros<T, T1>();
            foreach (var p in propertyInfos)
            {
                foreach (var t in tData)
                {
                    var proData = p.GetValue(t);
                    p.SetValue(t, ConvertDataFormat<T1>(Newtonsoft.Json.JsonConvert.SerializeObject(proData), userId));
                }
            }
            var properties = typeof(T).GetProperties();
            List<Dictionary<string, object>> dic = RecircleData<T, T1>(tData, propertyInfos);
            return dic;
        }
        static List<PropertyInfo> GetSubDetailPros<T, T1>()
        {
            var pros = typeof(T).GetProperties();
            List<PropertyInfo> propertyInfos = new List<PropertyInfo>();
            foreach (var d in pros)
            {
                var customAttr = d.GetCustomAttribute<DataTypeAttribute>();
                if (customAttr == null) continue;
                var isAdapter = customAttr.type == DataTypeEnum.RelateEntity && customAttr.relateEntity == typeof(T1);
                if (!isAdapter) continue;
                propertyInfos.Add(d);
            }
            return propertyInfos;
        }
        static List<Dictionary<string, object>> RecircleData<T, T1>(List<T> tData, List<PropertyInfo> innerPros)
        {
            var properties = typeof(T).GetProperties();
            Dictionary<string, object> fieldData;
            List<Dictionary<string, object>> dic = new List<Dictionary<string, object>>();
            foreach (var t in tData)
            {
                fieldData = new Dictionary<string, object>();
                foreach (var p in properties)
                {
                    var entityField = p.GetCustomAttribute<EntityFieldAttribute>();
                    if (entityField == null) continue;
                    if (innerPros.Contains(p))
                    {
                        var pData = p.GetValue(t);
                        if (pData is List<T1>)
                        {
                            var t1Data = pData as List<T1>;
                            var subDicVal = RecircleData<T1, T>(t1Data, new List<PropertyInfo>());
                            fieldData.Add(entityField.FieldName, subDicVal);
                        }
                    }
                    else
                        fieldData.Add(entityField.FieldName, ConvertFieldValue(p, t));
                }
                dic.Add(fieldData);
            }
            return dic;
        }
    }
}
