using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.EntityPro;
using UBeat.Crm.CoreApi.DomainModel.Vocation;
using UBeat.Crm.CoreApi.Services.Models.EntityPro;

namespace UBeat.Crm.CoreApi.Services.Models.DynamicEntity
{
    public class FunctionBtnsModel
    {
        /// <summary>
        /// 实体id（必填）
        /// </summary>
        public Guid EntityId { set; get; }

        /// <summary>
        /// 关联的记录id（可空）
        /// </summary>
        public List<Guid> RecIds { set; get; }
    }

    public class FunctionBtnListModel
    {
        /// <summary>
        /// 实体id（必填）
        /// </summary>
        public Guid EntityId { set; get; }
    }

    public class SaveFuncsModel
    {
        /// <summary>
        /// 实体id（必填）
        /// </summary>
        public Guid EntityId { set; get; }

        public List<FunctionModel> WebFuncs { set; get; }

        public List<FunctionModel> MobileFuncs { set; get; }

    }


    public class SyncFuncListModel
    {
        /// <summary>
        /// 实体id（必填）
        /// </summary>
        public Guid EntityId { set; get; }


    }
   

    public class ServiceJsonModel
    {
        /// <summary>
        /// 实体id（必填）
        /// </summary>
        public Guid EntityId { set; get; }
    }

    public class ServiceJsonDetailModel
    {
        public Guid EntityId { set; get; }
        /// <summary>
        /// WEB实体列表页面
        /// </summary>
        public string WebListPage { set; get; }
        /// <summary>
        /// WEB实体主页
        /// </summary>
        public string WebIndexPage { set; get; }
        /// <summary>
        /// WEB实体编辑页
        /// </summary>
        public string WebEditPage { set; get; }
        /// <summary>
        /// WEB实体查看页
        /// </summary>
        public string WebViewPage { set; get; }
        /// <summary>
        /// 安卓实体列表页
        /// </summary>
        public string AndroidListPage { set; get; }
        /// <summary>
        /// 安卓实体主页
        /// </summary>
        public string AndroidIndexPage { set; get; }
        /// <summary>
        /// 安卓实体编辑页
        /// </summary>
        public string AndroidEditPage { set; get; }
        /// <summary>
        /// 安卓实体查看页
        /// </summary>
        public string AndroidViewPage { set; get; }
        /// <summary>
        /// 苹果实体列表页
        /// </summary>
        public string IOSListPage { set; get; }
        /// <summary>
        /// 苹果实体主页
        /// </summary>
        public string IOSIndexpage { set; get; }
        /// <summary>
        /// 苹果实体编辑页
        /// </summary>
        public string IOSEditPage { set; get; }
        /// <summary>
        /// 苹果实体查看页
        /// </summary>
        public string IOSViewPage { set; get; }
    }

    public class FunctionBtnDetailModel
    {
        public Guid EntityId { set; get; }
        /// <summary>
        /// id
        /// </summary>
        public Guid Id { set; get; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { set; get; }
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { set; get; }

        /// <summary>
        /// 功能编码
        /// </summary>
        public string ButtonCode { set; get; }

        /// <summary>
        /// 图标
        /// </summary>
        public string Icon { set; get; }
        /// <summary>
        /// 显示位置
        /// </summary>
        public List<DisplayPositionType> DisplayPosition { set; get; }
        /// <summary>
        /// 数据选择类型
        /// </summary>
        public DataSelectType SelectType { set; get; }
        /// <summary>
        /// 是否自动刷新页面,1为刷新。0为不刷新
        /// </summary>
        public int IsRefreshPage { set; get; }
        /// <summary>
        /// 按钮的请求地址
        /// </summary>
        public string RoutePath { set; get; }

        /// <summary>
        /// 顺序序号
        /// </summary>
        public int RecOrder { set; get; }

        public object extradata { set; get; }

    }

    public class DeleteFunctionBtnModel
    {
        public Guid EntityId { set; get; }

        public Guid Id { set; get; }
    }

    public class SortFunctionBtnModel
    {
        public Guid EntityId { set; get; }
        public  Dictionary<Guid,int> OrderMapper { set; get; }


    }

}
