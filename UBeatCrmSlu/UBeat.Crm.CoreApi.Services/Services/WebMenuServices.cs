using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.EntityPro;
using UBeat.Crm.CoreApi.DomainModel.menus;
using UBeat.Crm.CoreApi.DomainModel.Vocation;
using UBeat.Crm.CoreApi.IRepository;
using System.Linq;
namespace UBeat.Crm.CoreApi.Services.Services
{
    public class WebMenuServices : BasicBaseServices
    {
        private static Guid CrmMenuId = Guid.Parse("10000000-0000-0000-0001-000000000001");
        private static Guid OfficeMenuId = Guid.Parse("10000000-0000-0000-0001-000000000002");
        private static Guid DynamicEntityId = Guid.Parse("10000000-0000-0000-0001-000000000005");
        private readonly IWebMenuRepository _webMenuRepository;
        private readonly IEntityProRepository _entryProRepository;
        private readonly IVocationRepository _vocationRepository;
        public WebMenuServices(IWebMenuRepository webMenuRepository, IEntityProRepository entryProRepository, IVocationRepository vocationRepository)
        {
            _webMenuRepository = webMenuRepository;
            _entryProRepository = entryProRepository;
            _vocationRepository = vocationRepository;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="accessType">0是业务菜单1是设置菜单</param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public List<WebMenuItem> getAllWebMenus(int type, int userNumber)
        {
            List<WebMenuItem> retList = _webMenuRepository.getAllMenu(type, userNumber);
            List<WebMenuItem> topList = new List<WebMenuItem>();
            WebMenuItem defaultPage = null;
            Dictionary<Guid, WebMenuItem> allMenus = new Dictionary<Guid, WebMenuItem>();
            Dictionary<string, FunctionInfo> allMyFuncs = AllMyFunctionIds(userNumber);
            foreach (WebMenuItem item in retList)
            {
                if (allMenus.ContainsKey(item.Id)) continue;
                allMenus.Add(item.Id, item);
            }
            foreach (WebMenuItem item in retList)
            {
                if (item.FuncID != null && item.FuncID != Guid.Empty.ToString() && item.FuncID != "")
                {
                    //需要校验有没有功能权限
                    if (allMyFuncs.ContainsKey(item.FuncID) == false)
                    {
                        continue;
                    }
                }
                if (defaultPage == null)
                {
                    if (item.path != null && item.path.Length > 0)
                    {
                        defaultPage = item;
                    }
                }
                if (item.ParentId == Guid.Empty)
                {
                    topList.Add(item);
                }
                else
                {
                    if (allMenus.ContainsKey(item.ParentId))
                    {
                        WebMenuItem parentItem = allMenus[item.ParentId];
                        if (!parentItem.ChildRen.Select(t => t.Id).Contains(item.Id))
                            parentItem.ChildRen.Add(item);
                    }
                }

            }
            //这里要删除没有子目录的
            removeEmptyDir(topList);
            if (defaultPage != null)
            {
                defaultPage.IsDefaultPage = 1;
            }
            return topList;
        }
        private void removeEmptyDir(List<WebMenuItem> topList)
        {
            List<WebMenuItem> needRemove = new List<WebMenuItem>();
            foreach (WebMenuItem item in topList)
            {
                if (item.ChildRen != null && item.ChildRen.Count > 0)
                {
                    removeEmptyDir(item.ChildRen);
                }
                if (item.ChildRen == null || item.ChildRen.Count == 0)
                {
                    if (item.path == null || item.path.Length == 0)
                    {
                        needRemove.Add(item);
                    }
                }

            }
            foreach (WebMenuItem item in needRemove)
            {
                topList.Remove(item);
            }
        }
        public bool addMenu(WebMenuItem item)
        {
            if (item.Id == null || item.Id == Guid.Empty)
            {
                item.Id = Guid.NewGuid();
            }
            if (item.Index == -1)
            {
                List<WebMenuItem> subItems = null;
                if (item.ParentId != null && item.ParentId != Guid.Empty)
                {
                    subItems = _webMenuRepository.getSubMenus(item.ParentId.ToString());

                }
                else
                {
                    subItems = _webMenuRepository.getSubMenus(null);
                }
                int maxIndex = -1;
                foreach (WebMenuItem item1 in subItems)
                {
                    if (maxIndex < item1.Index)
                    {
                        maxIndex = item1.Index;
                    }
                }
                maxIndex++;
                item.Index = maxIndex;
            }
            var uuid = _webMenuRepository.insertMennInfo(item);
            if (uuid != Guid.Empty)
            {
                return true;
            }
            return false;
        }

        public bool updateMenu(WebMenuItem item)
        {
            return _webMenuRepository.updateMenuInfo(item);
        }

        private string getFunctionIDByEntity(Guid entityid, List<FunctionInfo> funcs, int modeltype = 1)
        {
            foreach (FunctionInfo fun in funcs)
            {
                switch (modeltype)
                {
                    case 1:
                        if (entityid.Equals(fun.EntityId) && (fun.RoutePath == null || fun.RoutePath.Length == 0) && fun.DeviceType == 0)
                        {
                            if (fun.RecType == FunctionType.Entity)
                            {
                                return fun.FuncId.ToString();
                            }
                        }
                        break;
                    case 3:
                        if (entityid.Equals(fun.EntityId) && !string.IsNullOrEmpty(fun.RoutePath) && fun.DeviceType == 0)
                        {
                            if (fun.RecType == FunctionType.Function)
                            {
                                return fun.FuncId.ToString();
                            }
                        }
                        break;
                }
            }
            return null;
        }
        public bool synchCRMAndOfficeMenus()
        {
            //检查是否存在CRM的目录
            WebMenuItem crmDir = _webMenuRepository.getMenuInfo(CrmMenuId.ToString());
            if (crmDir == null)
            {
                crmDir = new WebMenuItem();
                crmDir.Index = 0;
                crmDir.Name = "CRM";
                crmDir.Id = CrmMenuId;
                crmDir.ParentId = Guid.Empty;
                crmDir.Icon = "";
                crmDir.path = "";
                crmDir.FuncID = "";
                crmDir.IsDynamic = 0;
                crmDir.IsLogicMenu = 0;
                _webMenuRepository.insertMennInfo(crmDir);
            }
            WebMenuItem officeDir = _webMenuRepository.getMenuInfo(OfficeMenuId.ToString());
            if (officeDir == null)
            {
                officeDir = new WebMenuItem();
                officeDir.Index = 1;
                officeDir.Name = "办公";
                officeDir.Id = OfficeMenuId;
                officeDir.ParentId = Guid.Empty;
                officeDir.Icon = "";
                officeDir.path = "";
                officeDir.FuncID = "";
                officeDir.IsDynamic = 0;
                officeDir.IsLogicMenu = 0;
                _webMenuRepository.insertMennInfo(officeDir);
            }
            List<WebMenuItem> allCrmSubMenus = _webMenuRepository.getSubMenus(CrmMenuId.ToString());
            List<WebMenuItem> allOfficeSubMenus = _webMenuRepository.getSubMenus(OfficeMenuId.ToString());
            List<WebMenuItem> allDynamicEntityMenus = _webMenuRepository.getSubMenus(OfficeMenuId.ToString());
            List<WebMenuItem> fixedCrmSubMenus = new List<WebMenuItem>();
            List<WebMenuItem> fixedOfficeSubMenus = new List<WebMenuItem>();
            List<WebMenuItem> fixedDynamicEntitySubMenus = new List<WebMenuItem>();
            List<FunctionInfo> allFunctions = _vocationRepository.GetTotalFunctionsWithStatus0();
            foreach (WebMenuItem item in allCrmSubMenus)
            {
                if (item.IsDynamic == 0)
                {
                    fixedCrmSubMenus.Add(item);
                }
            }
            foreach (WebMenuItem item in allOfficeSubMenus)
            {
                if (item.IsDynamic == 0)
                {
                    fixedOfficeSubMenus.Add(item);
                }
            }
            foreach (WebMenuItem item in allDynamicEntityMenus)
            {
                if (item.IsDynamic == 0)
                {
                    fixedDynamicEntitySubMenus.Add(item);
                }
            }
            //删除没有用的
            _webMenuRepository.deleteByParentId(CrmMenuId);
            _webMenuRepository.deleteByParentId(OfficeMenuId);
            _webMenuRepository.deleteDynamicByParentID(DynamicEntityId);
            Dictionary<string, List<IDictionary<string, object>>> sysMenus = _entryProRepository.EntranceListQuery(0);
            Dictionary<string, string> allids = new Dictionary<string, string>();
            if (sysMenus.ContainsKey("Crm"))
            {
                List<IDictionary<string, object>> crmMenus = (List<IDictionary<string, object>>)sysMenus["Crm"];
                foreach (IDictionary<string, object> item in crmMenus)
                {
                    string entryid = (string)item["entranceid"].ToString();
                    if (!allids.ContainsKey(entryid)) { allids.Add(entryid, entryid); }

                }
                crmMenus = (List<IDictionary<string, object>>)sysMenus["Office"];
                foreach (IDictionary<string, object> item in crmMenus)
                {
                    string entryid = (string)item["entranceid"].ToString();
                    if (!allids.ContainsKey(entryid)) { allids.Add(entryid, entryid); }

                }

            }
            if (sysMenus.ContainsKey("Crm"))
            {
                List<IDictionary<string, object>> crmMenus = (List<IDictionary<string, object>>)sysMenus["Crm"];
                int index = 0;
                foreach (IDictionary<string, object> item in crmMenus)
                {
                    if (((int)item["isgroup"]) == 1) continue;

                    WebMenuItem webMenu = new WebMenuItem();
                    webMenu.Name = (string)item["entryname"];
                    webMenu.IsLogicMenu = (int)item["islogicmenu"];
                    webMenu.ParentId = CrmMenuId;
                    webMenu.Id = (Guid)item["entranceid"];
                    webMenu.Index = index;
                    bool isCustPage = false;
                    if (item.ContainsKey("servicesjson") && item["servicesjson"] != null)
                    {
                        string serviceJson = item["servicesjson"].ToString();
                        try
                        {
                            ServicesJsonInfo serviceJsonInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<ServicesJsonInfo>(serviceJson);
                            if (serviceJsonInfo != null
                                && serviceJsonInfo.EntryPages != null
                                && serviceJsonInfo.EntryPages.WebListPage != null
                                && serviceJsonInfo.EntryPages.WebListPage.Length > 0)
                            {
                                webMenu.path = serviceJsonInfo.EntryPages.WebListPage;
                                isCustPage = true;
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                    if (isCustPage == false)
                    {
                        if (item["modeltype"] == null || (int)item["modeltype"] == 0)
                        {
                            webMenu.path = "/entcomm-list/" + ((Guid)item["entityid"]).ToString();
                        }
                        else
                        {
                            webMenu.path = "/entcomm-application/" + ((Guid)item["entityid"]).ToString();
                        }
                    }

                    webMenu.Icon = item["icons"] == null ? "" : item["icons"].ToString();
                    webMenu.FuncID = "";
                    webMenu.IsDynamic = 1;
                    webMenu.FuncID = getFunctionIDByEntity(((Guid)item["entityid"]), allFunctions);
                    _webMenuRepository.insertMennInfo(webMenu);
                    index++;
                }
                //开始处理静态数据
                foreach (WebMenuItem item in fixedCrmSubMenus)
                {
                    if (allids.ContainsKey(item.Id.ToString())) continue;
                    item.Index = index;
                    _webMenuRepository.insertMennInfo(item);

                    index++;
                }
            }
            if (sysMenus.ContainsKey("Office"))
            {
                List<IDictionary<string, object>> officeMenus = (List<IDictionary<string, object>>)sysMenus["Office"];
                int index = 0;
                foreach (IDictionary<string, object> item in officeMenus)
                {
                    if (((int)item["isgroup"]) == 1) continue;
                    WebMenuItem webMenu = new WebMenuItem();
                    webMenu.Name = (string)item["entryname"];
                    webMenu.IsLogicMenu = (int)item["islogicmenu"];
                    webMenu.ParentId = OfficeMenuId;
                    webMenu.Id = (Guid)item["entranceid"];
                    webMenu.Index = index;
                    bool isCustPage = false;
                    if (item.ContainsKey("servicesjson") && item["servicesjson"] != null)
                    {
                        string serviceJson = item["servicesjson"].ToString();
                        try
                        {
                            ServicesJsonInfo serviceJsonInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<ServicesJsonInfo>(serviceJson);
                            if (serviceJsonInfo != null
                                && serviceJsonInfo.EntryPages != null
                                && serviceJsonInfo.EntryPages.WebListPage != null
                                && serviceJsonInfo.EntryPages.WebListPage.Length > 0)
                            {
                                webMenu.path = serviceJsonInfo.EntryPages.WebListPage;
                                isCustPage = true;
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                    if (isCustPage == false)
                    {
                        if (item["modeltype"] == null || (int)item["modeltype"] == 0)
                        {
                            webMenu.path = "/entcomm-list/" + ((Guid)item["entityid"]).ToString();
                        }
                        else
                        {
                            webMenu.path = "/entcomm-application/" + ((Guid)item["entityid"]).ToString();
                        }
                    }
                    webMenu.Icon = item["icons"] == null ? "" : item["icons"].ToString();
                    webMenu.FuncID = "";
                    webMenu.IsDynamic = 1;
                    webMenu.FuncID = getFunctionIDByEntity(((Guid)item["entityid"]), allFunctions);
                    _webMenuRepository.insertMennInfo(webMenu);
                    index++;
                }
                //开始处理静态数据
                foreach (WebMenuItem item in fixedOfficeSubMenus)
                {
                    if (allids.ContainsKey(item.Id.ToString())) continue;
                    item.Index = index;
                    _webMenuRepository.insertMennInfo(item);
                    index++;
                }
            }


            if (sysMenus.ContainsKey("DynamicEntity"))
            {
                List<IDictionary<string, object>> dynamicEntity = (List<IDictionary<string, object>>)sysMenus["DynamicEntity"];
                var tmpLst = _entryProRepository.GetDynamicEntityList();
                int index = 0;
                int secIndex = 0;
                foreach (IDictionary<string, object> item in dynamicEntity)
                {
                    if (((int)item["isgroup"]) == 1) continue;
                    var subMenus = tmpLst.Where(t => t.RelEntityId == item["entityid"].ToString());
                    if (subMenus.Count() <= 0) continue;
                    WebMenuItem webMenu = new WebMenuItem();
                    webMenu.Name = (string)item["entryname"];
                    webMenu.IsLogicMenu = (int)item["islogicmenu"];
                    webMenu.ParentId = DynamicEntityId;
                    webMenu.Id = Guid.NewGuid();
                    webMenu.Index = index;
                    webMenu.Icon = item["icons"] == null ? "" : item["icons"].ToString();
                    webMenu.IsDynamic = 1;
                    webMenu.FuncID = getFunctionIDByEntity(((Guid)item["entityid"]), allFunctions);
                    var pid = _webMenuRepository.insertMennInfo(webMenu);
                    index++;

                    foreach (var subMenu in subMenus)
                    {
                        WebMenuItem webSubMenu = new WebMenuItem();
                        webSubMenu.Name = subMenu.EntityName;
                        webSubMenu.IsLogicMenu = 1;
                        webSubMenu.ParentId = pid;
                        webSubMenu.Id = Guid.NewGuid();
                        webSubMenu.Index = secIndex;
                        webSubMenu.Icon = subMenu.Icons;
                        webSubMenu.IsDynamic = 1;
                        webSubMenu.path = "/entcomm-dynamic/" + subMenu.EntityId;
                        webSubMenu.FuncID = getFunctionIDByEntity(Guid.Parse(subMenu.EntityId), allFunctions, 3);
                        _webMenuRepository.insertMennInfo(webSubMenu);
                        secIndex++;
                    }
                }
            }

            return true;
        }
    }
}
