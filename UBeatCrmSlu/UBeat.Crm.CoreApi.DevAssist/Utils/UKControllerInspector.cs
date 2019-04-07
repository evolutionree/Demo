using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.DevAssist.Models;

namespace UBeat.Crm.CoreApi.DevAssist.Utils
{
    public class UKControllerInspector
    {
        private static UKControllerInspector instance = null;
        private static List<UKWebApiInfo> ApisFromControllers = null;
        private UKControllerInspector() {
            ApisFromControllers = _listApi();
        }
        public static UKControllerInspector getInstance(bool ForceRefresh = false) {
            if (instance == null || ForceRefresh) {
                instance = new UKControllerInspector();
            }
            return instance;
        }
        public List<UKWebApiInfo> ListAllApi() {
            List<UKWebApiInfo> ret = MergeWithLocal(ApisFromControllers);
            return ret;
        }
        /// <summary>
        /// 通过Controller获取所有接口列表
        /// </summary>
        /// <returns></returns>
        private List<UKWebApiInfo> _listApi()
        {
            List<UKWebApiInfo> ret = new List<UKWebApiInfo>();
            #region 获取所有的dll
            dynamic type = this.GetType();
            string currentDirectory = Path.GetDirectoryName(type.Assembly.Location);
            System.IO.DirectoryInfo dir = new DirectoryInfo(currentDirectory);
            FileInfo[] files = dir.GetFiles("UBeat.Crm.CoreApi.*.dll");
            foreach (FileInfo f in files)
            {
                string assemblename = f.Name.Substring(0, f.Name.Length - 4);
                Assembly assembly = Assembly.Load(new AssemblyName(assemblename));
                string DllName = f.Name;
                Type[] types = assembly.GetTypes();
                IEnumerable<Attribute> attrs = null;
                foreach (Type t in types)
                {
                    if (t.FullName.EndsWith("Controller") == false) continue;
                    attrs = t.GetCustomAttributes(typeof(RouteAttribute));
                    string baseroutepath = "";
                    if (attrs != null)
                    {
                        IEnumerator<Attribute> it = attrs.GetEnumerator();
                        if (it.MoveNext())
                        {
                            RouteAttribute at = (RouteAttribute)it.Current;
                            if (at.Template != null && at.Template.Length > 0)
                            {
                                baseroutepath = at.Template;
                                string controllername = t.Name.Replace("Controller", "");
                                baseroutepath = baseroutepath.Replace("[controller]", controllername);
                                baseroutepath = baseroutepath.ToLower();
                            }
                        }
                    }
                    string className = t.FullName;
                    MethodInfo[] methods = t.GetMethods();
                    foreach (MethodInfo method in methods)
                    {
                        UKWebApiInfo item = new UKWebApiInfo();
                        string methodName = method.Name;
                        bool bWebApi = false;
                        #region 处理名称
                        attrs = method.GetCustomAttributes(typeof(HttpPostAttribute));
                        if (attrs != null)
                        {
                            IEnumerator<Attribute> it = attrs.GetEnumerator();
                            if (it.MoveNext())
                            {
                                HttpPostAttribute at = (HttpPostAttribute)it.Current;
                                if (at.Template != null && at.Template.Length > 0)
                                {
                                    item.FullPath = baseroutepath + "/" + at.Template;
                                    if ((item.ApiName == null || item.ApiName.Length == 0)
                                              && at.Name != null && at.Name.Length > 0)
                                    {
                                        item.ApiName = at.Name;
                                    }
                                    bWebApi = true;
                                }
                            }
                        }
                        if (!bWebApi)
                        {
                            attrs = method.GetCustomAttributes(typeof(RouteAttribute));
                            if (attrs != null)
                            {
                                IEnumerator<Attribute> it = attrs.GetEnumerator();
                                if (it.MoveNext())
                                {
                                    RouteAttribute at = (RouteAttribute)it.Current;
                                    if (at.Template != null && at.Template.Length > 0)
                                    {
                                        item.FullPath = baseroutepath + "/" + at.Template;
                                        if ((item.ApiName == null || item.ApiName.Length == 0)
                                              && at.Name != null && at.Name.Length > 0)
                                        {
                                            item.ApiName = at.Name;
                                        }
                                        bWebApi = true;
                                    }
                                }
                            }
                        }
                        attrs = method.GetCustomAttributes(typeof(UKWebApiAttribute));
                        if (attrs != null)
                        {
                            IEnumerator<Attribute> it = attrs.GetEnumerator();
                            if (it.MoveNext())
                            {
                                UKWebApiAttribute at = (UKWebApiAttribute)it.Current;
                                if ((item.ApiName == null || item.ApiName.Length == 0)
                                              && at.Name != null && at.Name.Length > 0)
                                {
                                    item.ApiName = at.Name;
                                }
                                if (at.Description != null && at.Description.Length > 0)
                                {
                                    item.SelfDescription = at.Description;
                                }
                            }
                        }
                        attrs = method.GetCustomAttributes(typeof(AllowAnonymousAttribute));
                        if (attrs != null)
                        {
                            IEnumerator<Attribute> it = attrs.GetEnumerator();
                            if (it.MoveNext())
                            {
                                item.NeedAuth = false;
                            }
                        }
                        #endregion
                        #region 处理参数 ，仅处理一层
                        if (bWebApi)
                        {

                            ParameterInfo[] mps = method.GetParameters();
                            if (mps != null && mps.Length > 0) {
                                foreach (ParameterInfo p in mps) {
                                    FromBodyAttribute pa = p.GetCustomAttribute<FromBodyAttribute>();
                                    if (pa != null)
                                    {
                                        //这有这种可能才考虑
                                        Type pt = p.ParameterType;
                                        PropertyInfo[] props = pt.GetProperties();
                                        foreach (PropertyInfo prop in props) {
                                            UKApiRequestParameterInfo thisitem = new UKApiRequestParameterInfo();
                                            thisitem.ParameterName = prop.Name;
                                            thisitem.ParameterType = prop.PropertyType.ToString();
                                            item.Parameters.Add(thisitem);
                                        }
                                    }


                                }
                            }
                        }
                        #endregion 
                        if (bWebApi)
                        {
                            item.DllName = DllName;
                            item.ClassName = className;
                            item.MethodName = methodName;
                            ret.Add(item);
                        }

                    }
                }
            }
            #endregion 
            return ret;
        }

        private List<UKWebApiInfo> MergeWithLocal(List<UKWebApiInfo> apiFromDll) {
            List<UKWebApiInfo> ret = new List<UKWebApiInfo>();
            foreach (UKWebApiInfo item in apiFromDll) {
                UKWebApiInfo retItem = DealWithOneApi(item);
                if (retItem != null) ret.Add(retItem);
            }
            return ret;
        }
        private UKWebApiInfo DealWithOneApi(UKWebApiInfo srcItem) {
            if (srcItem == null) return null;
            UKWebApiInfo itemInFile = null;
            UKWebApiInfo itemRet = Newtonsoft.Json.JsonConvert.DeserializeObject<UKWebApiInfo>(
                JsonConvert.SerializeObject(srcItem)
                );
            string fullPath = srcItem.FullPath;
            if (fullPath == null || fullPath.Length == 0) return null;
            string[] subPaths = fullPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (subPaths == null || subPaths.Length == 0) return null;
            string CurrentDir = System.IO.Directory.GetCurrentDirectory();
            CurrentDir = CurrentDir + Path.DirectorySeparatorChar + "apijson";            
            for (int i = 0; i < subPaths.Length-1; i++) {
                CurrentDir = CurrentDir + Path.DirectorySeparatorChar + subPaths[i];
            }
            CurrentDir = CurrentDir + Path.DirectorySeparatorChar + subPaths[subPaths.Length - 1] +".json";
            FileInfo file = new FileInfo(CurrentDir);
            if (File.Exists(CurrentDir)) {
                try
                {
                    StreamReader r = new StreamReader(CurrentDir);
                    string tmp = r.ReadToEnd();
                    itemInFile = Newtonsoft.Json.JsonConvert.DeserializeObject<UKWebApiInfo>(tmp);
                    itemRet = MergeFileItemToDllItem(srcItem, itemInFile);
                    r.Close();
                }
                catch (Exception ex) {

                }
            }
            return itemRet;
        }
        private UKWebApiInfo MergeFileItemToDllItem(UKWebApiInfo localItem, UKWebApiInfo fileItem) {
            if (fileItem == null) return localItem;
            localItem.MoreName = fileItem.MoreName;
            localItem.MoreDescription = fileItem.MoreDescription;
            localItem.RequestSample = fileItem.RequestSample;
            localItem.ResponseSample = fileItem.ResponseSample;
            DealWithParameter(localItem.Parameters, fileItem.Parameters);
            return localItem;
        }
        private void DealWithParameter(List<UKApiRequestParameterInfo> localParameters, List<UKApiRequestParameterInfo> fileParameters) {
            if (fileParameters == null) return;
            foreach (UKApiRequestParameterInfo item in localParameters) {
                      UKApiRequestParameterInfo newItem = fileParameters.Find((UKApiRequestParameterInfo i) =>
                {
                    return i.ParameterName == item.ParameterName;
                });
                if (newItem != null) {
                    if (newItem.ParameterCNName != null && newItem.ParameterCNName.Length > 0) {
                        item.ParameterCNName = newItem.ParameterCNName;
                    }
                    if (newItem.Description != null && newItem.Description.Length > 0) {
                        item.Description = newItem.Description;
                    }
                    if (item.SubParameters != null && newItem.SubParameters != null) {
                        DealWithParameter(item.SubParameters, newItem.SubParameters);
                    }
                }
            }
        }
    }
}
