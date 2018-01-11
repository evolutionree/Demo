using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.Vocation;

namespace UBeat.Crm.CoreApi.DomainModel.EntityPro
{
    public class FunctionJsonInfo
    {
        /// <summary>
        /// 页面功能按钮定义
        /// </summary>
        public List<FunctionBtnInfo> FuncBtns { set; get; }

        /// <summary>
        /// 实体拥有的功能列表，若为null，表示未设置数据，若为空list，则说明用户自己去除配置
        /// </summary>
        public List<FunctionInfo> WebFunctions { set; get; }

        /// <summary>
        /// 实体拥有的功能列表，若为null，表示未设置数据，若为空list，则说明用户自己去除配置
        /// </summary>
        public List<FunctionInfo> MobileFunctions { set; get; }
       
    }

    #region --FunctionBtnInfo定义--

    public class FunctionBtnInfo
    {
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

        public object extraData { set; get; }

        /// <summary>
        /// 顺序序号
        /// </summary>
        public int RecOrder { set; get; }

        public Guid WebFuncId { set; get; }

        public Guid MobileFuncId { set; get; }

    }

    /// <summary>
    /// 显示位置类型
    /// </summary>
    public enum DisplayPositionType
    {
        /// <summary>
        /// web列表
        /// </summary>
        WebList = 0,
        /// <summary>
        /// web详情
        /// </summary>
        WebDetail = 1,


        /// <summary>
        /// 手机列表
        /// </summary>
        MobileList = 100,
        /// <summary>
        /// 手机详情
        /// </summary>
        MobileDetail = 101
    }

    public enum DataSelectType
    {
        /// <summary>
        /// 默认类型，不关联数据
        /// </summary>
        Default = 0,
        /// <summary>
        /// 单选
        /// </summary>
        Single = 1,
        /// <summary>
        /// 多选
        /// </summary>
        Multiple = 2
    }
    #endregion

    #region --Function

    #endregion

}
