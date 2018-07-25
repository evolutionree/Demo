using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.DynamicEntity;

namespace UBeat.Crm.CoreApi.Services.Models.DynamicEntity
{
    public class DynamicEntityAddModel
    {
        public Guid TypeId { get; set; }
        public Dictionary<string, object> FieldData { get; set; }
        public Dictionary<string, object> ExtraData { get; set; }
        public List<Dictionary<string, object>> WriteBackData { get; set; }
        public Guid? FlowId { get; set; }
        public Guid? RelEntityId { get; set; }
        public Guid? RelRecId { get; set; }
        public Guid CacheId { get; set; }
    }


    public class DynamicEntityAddListModel
    {
        public List<DynamicEntityFieldDataModel> EntityFields { get; set; }

    }

    public class DynamicEntityFieldDataModel
    {
        public Guid TypeId { get; set; }
        public Dictionary<string, object> FieldData { get; set; }
        public Dictionary<string, object> ExtraData { get; set; }
    }

    public class DynamicEntityEditModel
    {
        public Guid TypeId { get; set; }
        public Guid RecId { get; set; }
        public Dictionary<string, object> FieldData { get; set; }

    }

    public class DynamicEntityListModel
    {
        public Guid EntityId { get; set; }
        public string MenuId { get; set; }
        public int ViewType { get; set; }
        public Dictionary<string, object> SearchData { get; set; }
        public Dictionary<string, object> ExtraData { get; set; }

        public Dictionary<string, object> SearchDataXOR { get; set; }
        public string SearchOrder { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int IsAdvanceQuery { get; set; }
        public int? NeedPower { get; set; }
        /// <summary>
        /// 用于关联列表查询
        /// 包含两个字段
        /// RelFieldName 和RelRecId
        /// </summary>
        public Dictionary<string, object> RelInfo { get; set; }
        public Dictionary<string, string> ColumnFilter { get; set; }
        /// <summary>
        ///  精确查询的条件，不受帅选项控制
        /// </summary>
        public Dictionary<string, object> ExactFieldOrFilter { get; set; }
        /// <summary>
        /// 专用于获取嵌套表格列表时，通过对应的主实体id列表获取其所有的嵌套表格的记录
        /// 
        /// </summary>
        public List<Guid> MainIds { get; set; }
    }


    public class DynamicEntityDetailModel
    {
        public Guid EntityId { get; set; }
        public Guid RecId { get; set; }
        public int? NeedPower { get; set; }

    }

    public class DynamicEntityDetaillistModel
    {
        public Guid EntityId { get; set; }
        /// <summary>
        /// 通过逗号分隔','
        /// </summary>
        public string RecIds { get; set; }
        public int? NeedPower { get; set; }

    }

    public class DynamicEntityGeneralModel
    {
        public Guid TypeId { get; set; }
        public int OperateType { get; set; }
    }
    /**
     * 
     * 用于web查询分录的type
     * **/
    public class DynamicGridEntityGeneralModel
    {
        public Guid TypeId { get; set; }
        public Guid EntityId { get; set; }
        public int OperateType { get; set; }
    }

    public class DynamicEntityListViewColumnModel
    {
        public Guid EntityId { get; set; }
        public int ViewType { get; set; }
    }

    public class DynamicEntityGeneralDicModel
    {
        public string DicKeys { get; set; }
    }

    public class DynamicPluginVisibleModel
    {
        public Guid EntityId { get; set; }
        public Guid RecId { get; set; }
    }

    public class DynamicPageVisibleModel
    {
        public Guid EntityId { get; set; }
        public Guid RecId { get; set; }
        public int PageType { get; set; }
        public string PageCode { get; set; }
    }

    public class DynamicEntityTransferModel
    {
        public Guid EntityId { get; set; }
        public string RecId { get; set; }
        public int Manager { get; set; }

    }
    public class DynamicEntityTransferUser2UserModel {
        public int OldUserId { get; set; }
        public int NewUserId { get; set; }

        public int IgnoreMsg { get; set; }

        public List<DynamicEntityTransferUser2User_EntityFieldsModel> Entities { get; set; }
    }
    public class DynamicEntityTransferUser2User_EntityFieldsModel {
        public Guid EntityId { get; set; }
        public string FieldIds { get; set; }
    }

    public class DynamicEntityDeleteModel
    {
        public Guid EntityId { get; set; }
        //暂时没有用
        public Guid TypeId { set; get; }
        public string RecId { get; set; }

        public Guid RelRecId { set; get; }

        public int PageType { get; set; }

        public string PageCode { get; set; }

    }
    public class DataSrcDeleteRelationModel
    {
        public Guid RelId { get; set; }
        //暂时没有用
        public Guid RecId { get; set; }

        public Guid RelRecId { set; get; }

    }
    public class DynamicEntityAddConnectModel
    {
        public Guid EntityIdUp { get; set; }
        public Guid EntityIdTo { get; set; }
        public Guid RecIdUp { get; set; }
        public Guid RecIdTo { get; set; }
        public string Remark { get; set; }
    }

    public class DynamicEntityEditConnectModel
    {
        public Guid ConnectId { get; set; }
        public Guid EntityIdTo { get; set; }
        public Guid RecIdTo { get; set; }
    }

    public class DynamicEntityDeleteConnectModel
    {
        public Guid ConnectId { get; set; }
    }

    public class DynamicEntityConnectListModel
    {
        public Guid EntityId { get; set; }
        public Guid RecId { get; set; }
    }


    public class DynamicEntitySearchListModel
    {
        public int ModelType { get; set; }
        public Dictionary<string, object> SearchData { get; set; }
    }

    public class DynamicEntitySearchRepeatModel
    {
        public Guid EntityId { get; set; }
        public string CheckField { get; set; }
        public string CheckName { get; set; }
        public int Exact { get; set; }
        public Dictionary<string, object> SearchData { get; set; }
    }

    public class DynamicRelTabModel
    {
        public Guid RecId { get; set; }
        public Guid RelId { get; set; }
        public Guid RelEntityId { get; set; }
        public string keyWord { get; set; }
        public int ViewType { get; set; }

        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }

    public class RelTabSrcModel
    {
        public Guid RelId { get; set; }
        public Guid EntityId { get; set; }
        public int IsManyToMany { get; set; }
        public string SrcSql { get; set; }
    }
    public class DynamicRelTabModel1
    {
        public Guid RecId { get; set; }
        public Guid EntityId { get; set; }
        public Guid RelEntityId { get; set; }
        public string keyWord { get; set; }
        public int ViewType { get; set; }

        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }


    public class RelTabListModel
    {
        public Guid EntityId { get; set; }
    }

    public class GetEntityFieldsModel
    {
        public Guid EntityId { get; set; }
        public Guid RelEntityId { get; set; }
    }

    public class RelTabInfoModel
    {
        public Guid RelId { get; set; }
    }
    public class AddRelTabModel
    {
        public Guid EntityId { get; set; }

        public Guid FieldId { get; set; }
        public Guid RelEntityId { get; set; }
        public string RelName { get; set; }
        public Guid ICon { get; set; }
        public int IsManyToMany { get; set; }
        public string SrcSql { get; set; }

        public string SrcTitle { get; set; }
        public Dictionary<string,string> RelName_Lang { get; set; }
    }
    public class SaveRelConfigModel
    {
        public Guid RelId { get; set; }
        public List<RelConfig> Configs { get; set; }
        public List<RelConfigSet> ConfigSets { get; set; }
    }

    public class RelQueryDataModel
    {
        public Guid RecId { get; set; }
        public Guid RelId { get; set; }

    }
    public class RelConfigModel
    {
        public Guid RelId { get; set; }
        public List<RelConfig> Configs { get; set; }
        public List<RelConfigSet> ConfigSets { get; set; }
    }
    /// <summary>
    /// 用于独立实体页签下，新增关联数据时，需要调用相应接口去获取id和name字段信息
    /// </summary>
    public class RelTabQueryDataSourceModel {
        /// <summary>
        /// 当前实体定义ID
        /// </summary>
        public Guid EntityId { get; set; }
        /// <summary>
        /// 当前实体数据id
        /// </summary>
        public Guid RecId { get; set; }
        /// <summary>
        /// 要新增实体的字段的id
        /// 
        /// </summary>
        public Guid FieldId { get; set; }
    }
    public class UpdateRelTabModel
    {
        public Guid RelId { get; set; }
        public Guid EntityId { get; set; }

        public Guid FieldId { get; set; }

        public Guid RelEntityId { get; set; }
        public string RelName { get; set; }
        public string ICon { get; set; }

        public int type { get; set; }

        public int IsManyToMany { get; set; }
        public string SrcSql { get; set; }
        public string SrcTitle { get; set; }
        public Dictionary<string, string> RelName_Lang { get; set; }
    }
    public class DisabledRelTabModel
    {
        public Guid RelId { get; set; }
    }
    public class OrderbyRelTabModel
    {
        public string RelIds { get; set; }
    }

    public class AddRelTabRelationDataSrcModel
    {
        public Guid RelId { get; set; }
        public Guid RecId { get; set; }
        public string IdStr { get; set; }


    }


    public class PermissionModel
    {
        public Guid RecId { get; set; }
        public Guid EntityId { get; set; }
        public Guid RelEntityId { get; set; }
        public Guid RelRecId { get; set; }
    }


    public class FollowModel
    {
        public Guid RecId { get; set; }
        public Guid EntityId { get; set; }
        public bool IsFollow { get; set; }
        public Guid RecType { get; set; }
    }
    /// <summary>
    /// 用于执行自定义函数的入口参数
    /// </summary>
    public class UKExtExecuteFunctionModel {
        /// <summary>
        /// 执行自定义函数的记录ids
        /// </summary>
        public string[] RecIds { get; set; }
        /// <summary>
        /// 属于哪个实体的函数
        /// </summary>
        public Guid? EntityId { get; set; }
        /// <summary>
        /// 额外的参数
        /// </summary>
        public Dictionary<string, object> OtherParams { get; set; }
    }

    /// <summary>
    /// 获取服务器函数扩展的列表的入口参数
    /// </summary>
    public class UKExtConfigListModel {
        public Guid EntityId { get; set; }
    }

    #region 查重条件实体
    /// <summary>
    /// 查重条件-----
    /// </summary>
    public class EntityCondition
    {
        /// <summary>
        /// 实体ID
        /// </summary>
        public Guid EntityId { get; set; }
        /// <summary>
        /// 字段ID
        /// </summary>
        public Guid FieldId { get; set; }
        /// <summary>
        /// 条件类型 0是查重
        /// </summary>
        public int FuncType { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int RecOrder { get; set; }

        /// <summary>
        /// 字段ID集合字符串
        /// </summary>
        public string FieldIds { get; set; }
    }

    public enum FuncType
    {
        /// <summary>
        /// 查重
        /// </summary>
        Repeat = 0
    }
    #endregion

    public class MarkCompleteModel
    {
        public Guid RecId { get; set; }

    }
    /// <summary>
    /// 用于获取个人实体WEB列定义的详情的参数
    /// </summary>
    public class WebListColumnsForPersonalParamInfo {
        public Guid EntityId { get; set; }
    }
    public class SaveWebListColumnsForPersonalParamInfo {
        public Guid EntityId { get; set; }
        public int UserId { get; set; }
        public WebListPersonalViewSettingInfo ViewConfig { get; set; }

    }

    /// <summary>
    /// 新版本的实体数据转移接口
    /// </summary>
    public class EntityTransferParamInfo {
        public DynamicEntityListModel DataFilter { get; set; }
        //优先recids
        public string RecIds { get; set; }
        public Guid SchemeId { get; set; }
        public Guid FieldId { get; set; }
        public Guid EntityId { get; set; }
        public int NewUserId { get; set; }
        public int OUserId { get; set; }
        public int InsertIfNo { get; set; }
    }

    public class TemporarySaveModel
    {
        public int RecManager { get; set; }
        public DateTime CreatedTime { get; set; }
        public string DataJson { get; set; }
        public string FieldJson { get; set; }
        public Guid TypeId { get; set; }
        public Guid CacheId { get; set; }
        public string InputJson { get; set; }
        public string Title { get; set; }
        public string FieldName { get; set; }
        public string RecRelateId { get; set; }
        public string RelateEntityId { get; set; }
        public string RelateTypeId { get; set; }
        public string EntityId { get; set; }
    }

    public class TemporaryDetailMode
    {
       public string CacheId { get; set; }
    }
    public class DelTempListModel
    {
        public string CacheIds { get; set; }
    }


    /// <summary>
    /// 嵌套表格导入（新增+编辑）时下载导出模板的接口参数
    /// </summary>
    public class DetailImportTemplateParamInfo {
        public Guid MainEntityId { get; set; }
        public Guid MainTypeId { get; set; }
        public Guid DetailEntityId { get; set; }
    }
    public class DetailImportParamInfo {
        /// <summary>
        /// 主实体的EntityId
        /// </summary>
        public Guid MainEntityId { get; set; }
        /// <summary>
        /// 主实体的RecID
        /// </summary>
        public Guid MainRecId { get; set; }
        public Guid MainTypeId { get; set; }
        public Guid DetailEntityId { get; set; }
        /// <summary>
        /// 当前实体数据的信息，包括所有嵌套实体的信息
        /// </summary>
        public Dictionary<string, object> MainDetail { get; set; }
        /// <summary>
        /// 当前界面上的当前分录的过滤信息，主要用于清理数据时考虑过滤条件问题(暂时不考虑此问题)
        /// </summary>
        public Dictionary<string,Dictionary<string, object>> DetailFieldsFilter { get; set; }

        /// <summary>
        /// Excel文件
        /// </summary>
        public IFormFile Data { set; get; }
        public OutputResult<object> ValidateData() {
            try
            {
                if (MainTypeId == null || MainTypeId == Guid.Empty) throw (new Exception("主实体类型不存在"));
                if (MainEntityId == null || MainEntityId == Guid.Empty) throw (new Exception("主实体EntityId不存在"));
                if (DetailEntityId == null || DetailEntityId == Guid.Empty) throw (new Exception("嵌套实体的EntityId"));
                return null;
            }
            catch (Exception ex) {
                return new OutputResult<object>(null, ex.Message, -1);
            }
        }
    }
}
