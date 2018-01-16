using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.menus;
namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IWebMenuRepository: IBaseRepository
    {

        /***
         * 获取结构化后的WEB菜单树 
         */
        List<WebMenuItem> getAllMenu(int type, int userNumber);
        WebMenuItem getMenuInfo(string id);//获取菜单详情，但不包括子菜单
        List<WebMenuItem> getSubMenus(string id);

        bool updateMenuInfo(WebMenuItem item);//保存菜单详情
        Guid insertMennInfo(WebMenuItem item);//插入菜单详情
        bool deleteMenuInfo(string id);//删除菜单详情

        bool deleteByParentId(Guid parentid);//删除所有的子菜单
        bool deleteDynamicByParentID(Guid parentid);//删除所有动态子菜单

    }
}
