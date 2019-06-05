using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.EntityPro;

namespace UBeat.Crm.CoreApi.Services.Models.EntityPro
{
    public class EntityProQueryModel
    {
        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public int Status { get; set; }

        public string EntityName { get; set; }

        public int TypeId { get; set; }
        /// <summary>
        /// 类型列表，typeid字段主要兼容原来的接口
        /// </summary>
        public string TypeIds { get; set; }

    }
    public class EntityProInfoModel
    {
        public string EntityId { get; set; }
    }

    public class OrderByEntityProModel
    {
        public string EntityIds { get; set; }

    }

    public class EntityOrderbyModel
    {
        public int ModelType { get; set; }

        public string RelEntityId { get; set; }
    }
    public class EntityProModel
    {
        public string EntityId { get; set; }
        public string EntityName { get; set; }
        public string EntityTable { get; set; }
        public int TypeId { get; set; }
        public string Remark { get; set; }
        public string Styles { get; set; }
        public string Icons { get; set; }
        public string RelEntityId { get; set; }
        public int Relaudit { get; set; }
        public string Perfix { get; set; }
        public int RecStatus { get; set; }

        public Guid RelFieldId { get; set; }

        public Dictionary<string, string> EntityName_Lang { get; set; }

        public Dictionary<string, string> ServicesJson { get; set; }

    }


    public class EntityFieldProModel
    {
        public string FieldId { get; set; }
        public string FieldName { get; set; }
        public string EntityId { get; set; }
        public string FieldLabel { get; set; }
        public string DisplayName { get; set; }
        public int ControlType { get; set; }
        public int FieldType { get; set; }
        public string DataSource { get; set; }

        public int RecStatus { get; set; }

        public int RecOrder { get; set; }
        public string FieldConfig { get; set; }
        public Dictionary<string, string> DisplayName_Lang { get; set; }
        public Dictionary<string, string> FieldLabel_Lang { get; set; }

        // [JsonProperty("FieldConfig")]
        //   public JObject FieldConfigJson => FieldConfig.ToJsonObject();
    }

    public class EntityFieldExpandJSModel
    {
        public string FieldId { get; set; }
        public string ExpandJS { get; set; }

        public string Remark { get; set; }
    }

    public class EntityFieldFilterJSModel
    {
        public string FieldId { get; set; }
        public string FilterJS { get; set; }
        public string Remark { get; set; }
    }

    /// <summary>
    /// 快速修改字段名称
    /// </summary>
    public class EntityFieldUpdateDisplayNameParamInfo
    {
        public Guid FieldId { get; set; }
        public Dictionary<string, string> DisplayName_Lang { get; set; }
    }
    public class DeleteEntityDataModel
    {
        public string EntityId { get; set; }

    }

    public class SimpleSearchModel
    {
        public int ViewType { get; set; }
        public string EntityId { get; set; }
        public string SearchField { get; set; }
        public int IsComQuery { get; set; }

        public ICollection<AdvanceSearchModel> AdvanceSearch { get; set; }

    }
    public class AdvanceSearchModel
    {

        public int IsLike { get; set; }

        public string EntityId { get; set; }

        public string FieldId { get; set; }

        public int ControlType { get; set; }

        public int RecOrder { get; set; }
    }

    public class EntityTypeQueryModel
    {
        public string CategoryId { get; set; }

        public string CategoryName { get; set; }

        public string EntityId { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }
    }

    public class EntityTypeModel
    {
        public string CategoryId { get; set; }

        public string CategoryName { get; set; }

        public string EntityId { get; set; }

        public int RecOrder { get; set; }

        public int RecStatus { get; set; }

        public Dictionary<string, string> CategoryName_Lang { get; set; }
    }

    public class EntityFieldRulesSaveModel
    {
        public EntityFieldRulesSaveModel()
        {
            Rules = new List<FieldRulesDetailModel>();
        }
        public string FieldLabel { get; set; }
        public string FieldId { get; set; }

        public string TypeId { get; set; }
        public ICollection<FieldRulesDetailModel> Rules { get; set; }
        public int RecStatus { get; set; }
    }

    public class FieldRulesDetailModel
    {
        public string FieldRulesId { get; set; }

        public int OperateType { get; set; }

        public int IsVisible { get; set; }

        public int IsRequired { get; set; }

        public int IsReadOnly { get; set; }

        public JObject ViewRule { get; set; }
        public JObject ValidRule { get; set; }
        public string ViewRuleStr { get; set; }

        public string ValidRuleStr { get; set; }

        public int UserId { get; set; }

    }
    public class FileldRulesVocationQueryModel
    {

        public string EntityId { get; set; }

        public string VocationId { get; set; }

        public int UseType { get; set; }
    }
    public class EntityFieldRulesVocationSaveModelList
    {
        public EntityFieldRulesVocationSaveModelList()
        {
            FieldRules = new List<EntityFieldRulesVocationSaveModel>();
        }
        public ICollection<EntityFieldRulesVocationSaveModel> FieldRules { set; get; }

        public string EntityId { set; get; }
    }

    public class EntityFieldRulesVocationSaveModel
    {
        public EntityFieldRulesVocationSaveModel()
        {
            Rules = new List<FieldRulesVocationDetailModel>();
        }
        public string FieldLabel { get; set; }
        public string VocationId { set; get; }
        public string FieldId { get; set; }

        public ICollection<FieldRulesVocationDetailModel> Rules { get; set; }
        public int RecStatus { get; set; }
    }

    public class FieldRulesVocationDetailModel
    {
        public string FieldRulesId { get; set; }
        public int OperateType { get; set; }
        public int IsVisible { get; set; }

        // public int IsRequire { get; set; }
        public int IsReadOnly { get; set; }
        //public int UserId { get; set; }



    }

    public class FileldRulesQueryModel
    {

        public string EntityId { get; set; }

        public string TypeId { get; set; }

        public int UseType { get; set; }
    }

    public class ListViewModel
    {
        public string ViewConfId { get; set; }
        public int ViewStyleId { get; set; }
        public string EntityId { get; set; }
        public string FieldKeys { get; set; }
        public string Fonts { get; set; }
        public string Colors { get; set; }
        public int ViewType { get; set; }

        public string FieldIds { get; set; }

    }
    public class ListViewColumnModel
    {
        public string ViewColumnId { get; set; }
        public string EntityId { get; set; }
        public string FieldId { get; set; }
        public int ViewType { get; set; }

    }
    public class SaveListViewColumnModel
    {
        public string EntityId { get; set; }
        public string FieldIds { get; set; }
        public int ViewType { get; set; }

    }

    public class EntityPageConfigModel
    {
        public string EntityId { get; set; }
        public string TitlefieldId { get; set; }
        public string TitlefieldName { get; set; }
        public string SubfieldIds { get; set; }
        public string SubfieldNames { get; set; }
        public string Modules { get; set; }
        public string RelentityId { get; set; }
        public string RelfieldId { get; set; }
        public string RelfieldName { get; set; }
    }

    public class SetRepeatModel
    {
        public string EntityId { get; set; }
    }

    public class SaveSetRepeatModel
    {
        public string EntityId { get; set; }

        public string Fieldids { get; set; }
    }

    public class SaveEntranceGroupModel
    {
        public string entranceid { get; set; }
        public string entryname { get; set; }
        public int entrytype { get; set; }
        public string entityid { get; set; }
        public int isgroup { get; set; }
        public int recorder { get; set; }

        public int IsDefaultGroup { get; set; }

    }


    public class RelControlValueModel
    {
        public string RecId { get; set; }
        public string EntityId { get; set; }

        public string FieldId { get; set; }

    }
    public class PersonalSettingModel
    {
        public Guid EntityId { get; set; }

    }
    /// <summary>
    /// 个人列表字段设置
    /// </summary>

    public class PersonalViewSetModel
    {
        /// <summary>
        /// userid
        /// </summary>

        public int UserId { get; set; }
        /// <summary>
        /// 字段所属实体Id
        /// </summary>

        public Guid EntityId { get; set; }
        /// <summary>
        /// 字段Id
        /// </summary>

        public Guid FieldId { get; set; }
        /// <summary>
        /// 排序号
        /// </summary>
        public int RecOrder { get; set; }


    }

    public class EntityBaseDataModel
    {
        public Guid EntityId { get; set; }

        public Guid FieldId { get; set; }

        public Guid RelEntityId { get; set; }

        public string FieldName { get; set; }
    }
    public class EntityBaseDataFieldModel
    {
        public Guid EntityId { get; set; }

        public Guid CommEntityId { get; set; }
    }

    public class EntityGlobalJsModel
    {
        public EntityGlobalJsModel()
        {
            Details = new List<DetailModel>();
        }
        public Guid EntityId { get; set; }
        public ICollection<DetailModel> Details { get; set; }
    }

    public class DetailModel
    {
        public string Load { get; set; }
        public string Remark { get; set; }
        public int Type { get; set; }
    }
    public class EntityInputMethodParamInfo
    {
        public Guid EntityId { get; set; }
        public List<EntityInputModeInfo> InputMethods { get; set; }
    }

    public class UCodeModel
    {
        public Guid? RecId { get; set; }
        public Guid? Id { get; set; }
        //public string RecCode { get; set; }
        //public string CommitDate { get; set; }
        //public string UserName { get; set; }
        //public string CommitUserName { get; set; }
        public string CommitRemark { get; set; }
        //public string CommitRemarkDate { get; set; }
        public int PageIndex { get; set; }

        public int PageSize { get; set; }
        public string SearchOrder { get; set; }
        public Dictionary<string, object> ColumnFilter { get; set; }

    }

    public class PgCodeModel
    {
        public Guid? Id { get; set; }
        public int PageIndex { get; set; }
        public string Remark { get; set; }
        public int PageSize { get; set; }
        public string SearchOrder { get; set; }
        public Dictionary<string, object> ColumnFilter { get; set; }
    }
}
