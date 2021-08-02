using Dapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.menus;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Utility;

namespace UBeat.Crm.CoreApi.Repository.Repository.menus
{
    public class WebMenuRepository : RepositoryBase, IWebMenuRepository
    {
        public bool deleteByParentId(Guid parentid)
        {
            string cmdText = "delete from crm_sys_webmenu where parentid=@parentid";
            var param = new
            {
                parentid
            };
            DataBaseHelper.ExecuteNonQuery(cmdText, param);
            return true;

        }
        public bool deleteDynamicByParentID(Guid parentid)
        {
            var sql = @"WITH RECURSIVE T1 as(
                                SELECT id,name,parentid  FROM crm_sys_webmenu WHERE isdynamic=1 AND parentid=@parentid
                                UNION
                                SELECT wm.id,wm.name,wm.parentid  FROM crm_sys_webmenu wm INNER JOIN T1 ON T1.id=wm.parentid WHERE isdynamic=1
                            )
                            DELETE FROM crm_sys_webmenu WHERE  id IN (SELECT id FROM T1);";
            var param = new
            {
                parentid
            };
            DataBaseHelper.ExecuteNonQuery(sql, param);
            return true;
        }

        public bool deleteMenuInfo(string id)
        {
            throw new NotImplementedException();
        }

        public WebMenuItem getMenuInfo(string id)
        {
            string cmdText = "select * from crm_sys_webmenu where id=@guid";
            Guid guid = Guid.Parse(id);
            var param = new
            {
                guid
            };
            List<IDictionary<string, object>> result = DataBaseHelper.Query(cmdText, param);
            if (result == null || result.Count == 0) return null;
            IDictionary<string, object> item = result[0];
            return WebMenuItem.parseFromDict(item);
        }



        public List<WebMenuItem> getSubMenus(string id)
        {
            string cmdText = "";
            cmdText = "select * from crm_sys_webmenu where parentid=@parentid order by index";

            Guid parentid = Guid.Empty;
            if (id == null || id.Length == 0)
            {
            }
            else
            {
                parentid = Guid.Parse(id);
            }
            var param = new
            {
                parentid
            };
            List<IDictionary<string, object>> result = DataBaseHelper.Query(cmdText, param);
            List<WebMenuItem> retList = new List<WebMenuItem>();
            foreach (IDictionary<string, object> item in result)
            {
                WebMenuItem menu = WebMenuItem.parseFromDict(item);
                if (menu != null)
                {
                    retList.Add(menu);
                }
            }
            return retList;
        }

        public WebMenuItem getMenuInfoByParentMenuIdAndName(Guid parentId, string name)
        {
            string cmdText = "select * from crm_sys_webmenu where parentId = @parentId and name = @name";
            var param = new
            {
                parentId,
                name
            };
            List<IDictionary<string, object>> result = DataBaseHelper.Query(cmdText, param);
            if (result == null || result.Count == 0) return null;
            IDictionary<string, object> item = result[0];
            return WebMenuItem.parseFromDict(item);
        }


        public Guid insertMennInfo(WebMenuItem item)
        {
            if (!string.IsNullOrEmpty(item.FuncID) && !string.IsNullOrEmpty(item.Name))
            {
                var menu = getMenuInfoByParentMenuIdAndName(item.ParentId, item.Name);
                if (menu != null)
                {
                    return item.Id;
                }
            }

            if (item.Id == null || item.Id == Guid.Empty)
            {
                return Guid.Empty;
            }

            string cmdText = @"insert into crm_sys_webmenu(id,index,name,icon,path,funcid,parentid,isdynamic,name_lang)values(@id,@index,@name,@icon,@path,@funcid,@parentid,@isdynamic,@name_lang::jsonb) returning id";
            var param = new
            {
                item.Id,
                item.Index,
                item.Name,
                item.Icon,
                item.path,
                item.FuncID,
                item.ParentId,
                item.IsDynamic,
                Name_Lang = JsonConvert.SerializeObject(item.Name_Lang)
            };
            return DataBaseHelper.ExecuteScalar<Guid>(cmdText, param);
        }

        public bool updateMenuInfo(WebMenuItem item)
        {
            if (item.Id == null || item.Id == Guid.Empty)
            {
                return false;
            }
            string cmdText = @"update crm_sys_webmenu set index=@index,
                                name=@name,icon=@icon,path=@path,funcid=@funcid,parentid=@parentid,isdynamic=@isdynamic,name_lang =@name_lang::jsonb
                                where id=@id";
            var param = new
            {
                item.Index,
                item.Name,
                item.Icon,
                item.path,
                item.FuncID,
                item.ParentId,
                item.IsDynamic,
                item.Id,
                Name_Lang = JsonConvert.SerializeObject(item.Name_Lang)
            };
            DataBaseHelper.ExecuteNonQuery(cmdText, param);
            return true;
        }

        List<WebMenuItem> IWebMenuRepository.getAllMenu(int type, int userNumber)
        {
            string cmdText = "with RECURSIVE  tmp as   ( \n" +
"                                                                select w.*," +
"(select 1 from crm_sys_reportdefine where recid::text =\n" +
" ltrim(rtrim(replace(w.path, '/reportform/', ''))) limit 1) as reporttype,\n" +
"(select recname from crm_sys_reportdefine where recid::text =\n" +
" ltrim(rtrim(replace(w.path, '/reportform/', ''))) limit 1) as reportname \n"+
"from crm_sys_webmenu w  \n" +
"                                                                WHERE parentid='00000000-0000-0000-0000-000000000000' and islogicmenu=@type \n" +
"                                                                union all  \n" +
"                                                                select e.*," +
"(select 1 from crm_sys_reportdefine where recid::text =\n" +
" ltrim(rtrim(replace(e.path, '/reportform/', ''))) limit 1) as reporttype,\n" +
"(select recname from crm_sys_reportdefine where recid::text =\n" +
" ltrim(rtrim(replace(e.path, '/reportform/', ''))) limit 1) as reportname \n" +
"from crm_sys_webmenu e    \n" +
"                                                                inner join tmp t on t.id = e.parentid \n" +
"                                                                ) \n" +
"                                                                select id, index, name, \n" +
"                                                                (select icons from crm_sys_entity where entityid::text = ltrim(rtrim(replace(path, '/entcomm-list/', ''))) limit 1) as icon,\n" +
"                                                                path, funcid, parentid, isdynamic, islogicmenu, \n" +
"                                                                isleaf, name_lang,1 reporttype,reportname from tmp where path like '/entcomm-list/%'\n" +
"                                                                union\n" +
"                                                                select id, index, name, \n" +
"                                                                (select icons from crm_sys_entity where entityid::text = ltrim(rtrim(replace(path, '/entcomm-application/', ''))) limit 1) as icon,\n" +
"                                                                path, funcid, parentid, isdynamic, islogicmenu, \n" +
"                                                                isleaf, name_lang,1 reporttype,reportname from tmp where path like '/entcomm-application/%'" +
"                                                                union\n" +
"                                                                select * from tmp where (path is null or path not like '/entcomm-list/%') and (path is null or path not like '/entcomm-application/%') \n" +
"                                                                order by index";
            var param = new DynamicParameters();
            param.Add("type", type);
            List<IDictionary<string, object>> result = DataBaseHelper.Query(cmdText, param);
            List<WebMenuItem> retList = new List<WebMenuItem>();
            foreach (IDictionary<string, object> item in result)
            {
                WebMenuItem menu = WebMenuItem.parseFromDict(item);
                if (menu != null)
                {
                    retList.Add(menu);
                }
            }
            return retList;
        }
    }
}
