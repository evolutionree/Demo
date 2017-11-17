using Dapper;
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
            string cmdText = "delete from crm_sys_webmenu where parentid=@parentid and isdynamic = 1";
            var param = new
            {
                parentid
            };
            DataBaseHelper.ExecuteNonQuery(cmdText, param);
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

        public bool insertMennInfo(WebMenuItem item)
        {
            if (item.Id == null || item.Id == Guid.Empty)
            {
                return false;
            }
            string cmdText = @"insert into crm_sys_webmenu(id,index,name,icon,path,funcid,parentid,isdynamic)values(@id,@index,@name,@icon,@path,@funcid,@parentid,@isdynamic)";
            var param = new
            {
                item.Id,
                item.Index,
                item.Name,
                item.Icon,
                item.path,
                item.FuncID,
                item.ParentId,
                item.IsDynamic
            };
            DataBaseHelper.ExecuteNonQuery(cmdText, param);
            return true;
        }

        public bool updateMenuInfo(WebMenuItem item)
        {
            if (item.Id == null || item.Id == Guid.Empty)
            {
                return false;
            }
            string cmdText = @"update crm_sys_webmenu set index=@index,
                                name=@name,icon=@icon,path=@path,funcid=@funcid,parentid=@parentid,isdynamic=@isdynamic
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
                item.Id
            };
            DataBaseHelper.ExecuteNonQuery(cmdText, param);
            return true;
        }

        List<WebMenuItem> IWebMenuRepository.getAllMenu(int type, int userNumber)
        {
            string cmdText = "with RECURSIVE  tmp as \n" +
" ( \n" +
" select w.* from crm_sys_webmenu w  WHERE parentid='00000000-0000-0000-0000-000000000000' and islogicmenu=@type \n" +
" union all  \n" +
" select e.*  from crm_sys_webmenu e    inner join tmp t on t.id = e.parentid \n" +
" )select * from tmp order by index";
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
