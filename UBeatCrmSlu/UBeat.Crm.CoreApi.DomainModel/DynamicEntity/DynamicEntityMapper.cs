using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using UBeat.Crm.CoreApi.DomainModel.Account;

namespace UBeat.Crm.CoreApi.DomainModel.DynamicEntity
{
    public class DynamicEntityAddMapper : BaseEntity
    {
        public Guid TypeId { get; set; }
        public Dictionary<string, object> FieldData { get; set; }

        public Dictionary<string, object> ExtraData { get; set; }
        public List<Dictionary<string, object>> WriteBackData { get; set; }
        public Guid? FlowId { get; set; }
        public Guid? RelEntityId { get; set; }
        public Guid? RelRecId { get; set; }
        protected override IValidator GetValidator()
        {
            return new DynamicEntityAddMapperValidator();
        }
    }

    public class DynamicEntityAddListMapper
    {
        public Guid TypeId { get; set; }
        public Dictionary<string, object> FieldData { get; set; }
        public Dictionary<string, object> ExtraData { get; set; }

    }


    public class DynamicEntityAddMapperValidator : AbstractValidator<DynamicEntityAddMapper>
    {
        public DynamicEntityAddMapperValidator()
        {
            RuleFor(d => d.TypeId).NotNull().WithMessage("类型ID不能为空");
            RuleFor(d => d.FieldData).NotEmpty().WithMessage("字段数据不能为空");
            RuleFor(d => d).Must(x => ValidWorkFlow(x.FlowId, x.RelEntityId, x.RelRecId)).WithMessage("关联实体数据必须同时有值");
        }

        public static bool ValidWorkFlow(Guid? flowId, Guid? relEntityId, Guid? relRecId)
        {
            if (!relEntityId.HasValue && !relRecId.HasValue) return true;
            if (relEntityId.HasValue && relRecId.HasValue) return true;
            return false;
        }
    }

    public class DynamicEntityEditMapper : BaseEntity
    {
        public Guid TypeId { get; set; }
        public Dictionary<string, object> FieldData { get; set; }
        public Guid RecId { get; set; }
        protected override IValidator GetValidator()
        {
            return new DynamicEntityEditMapperValidator();
        }
    }

    public class DynamicEntityEditMapperValidator : AbstractValidator<DynamicEntityEditMapper>
    {
        public DynamicEntityEditMapperValidator()
        {
            RuleFor(d => d.TypeId).NotNull().WithMessage("类型ID不能为空");
            RuleFor(d => d.FieldData).NotEmpty().WithMessage("字段数据不能为空");
            RuleFor(d => d.RecId).NotNull().WithMessage("数据ID不能为空");
        }
    }

    public class DataSrcDeleteRelationMapper:BaseEntity
    {
        public Guid RelId { get; set; }
        public Guid RecId { get; set; }

        public Guid RelRecId { set; get; }

        protected override IValidator GetValidator()
        {
            return new DataSrcDeleteRelationMapperValidator();
        }

        class DataSrcDeleteRelationMapperValidator : AbstractValidator<DataSrcDeleteRelationMapper>
        {
            public DataSrcDeleteRelationMapperValidator()
            {
                RuleFor(d => d.RelId).NotNull().WithMessage("页签配置ID不能为空");
                RuleFor(d => d.RecId).NotNull().WithMessage("数据ID不能为空");
                RuleFor(d => d.RelRecId).NotNull().WithMessage("关联数据ID不能为空");
            }
        }
    }

    public class DynamicEntityListMapper : BaseEntity
    {
        public Guid EntityId { get; set; }
        public string MenuId { get; set; }
        public int ViewType { get; set; }
        public Dictionary<string, object> SearchData { get; set; }
        public Dictionary<string, object> ExtraData { get; set; }
        public string SearchQuery { get; set; }
        public string SearchOrder { get; set; }
        public int? NeedPower { get; set; }
        public Dictionary<string,object> RelInfo { get; set; }
        protected override IValidator GetValidator()
        {
            return new DynamicEntityListMapperValidator();
        }
        
    }

    public class DynamicEntityListMapperValidator : AbstractValidator<DynamicEntityListMapper>
    {
        public DynamicEntityListMapperValidator()
        {
            RuleFor(d => d.EntityId).NotNull().WithMessage("实体ID不能为NULL");
            RuleFor(d => d.MenuId).NotNull().WithMessage("菜单ID不能为NULL");
            RuleFor(d => d.ViewType).NotNull().WithMessage("视图类型不能为NULL");
        }
    }

    public class DynamicEntityDetailtMapper : BaseEntity
    {
        public Guid EntityId { get; set; }
        public Guid RecId { get; set; }
        public int? NeedPower { get; set; }
        protected override IValidator GetValidator()
        {
            return new DynamicEntityDetailtMapperValidator();
        }
    }

    public class DynamicEntityDetailtListMapper : BaseEntity
    {

        public Guid EntityId { get; set; }
        /// <summary>
        /// 通过逗号分隔','
        /// </summary>
        public string RecIds { get; set; }
        public int? NeedPower { get; set; }
        protected override IValidator GetValidator()
        {
            return new MapperValidator();
        }
        class MapperValidator : AbstractValidator<DynamicEntityDetailtListMapper>
        {
            public MapperValidator()
            {
                RuleFor(d => d.EntityId).NotNull().WithMessage("实体ID不能为空");
                RuleFor(d => d.RecIds).NotNull().WithMessage("数据ID不能为空");
            }
        }
    }



    public class DynamicEntityDetailtMapperValidator : AbstractValidator<DynamicEntityDetailtMapper>
    {
        public DynamicEntityDetailtMapperValidator()
        {
            RuleFor(d => d.EntityId).NotNull().WithMessage("实体ID不能为空");
            RuleFor(d => d.RecId).NotNull().WithMessage("数据ID不能为空");
        }
    }

    public class DynamicPluginVisibleMapper : BaseEntity
    {
        public int DeviceClassic { get; set; }
        public Guid EntityId { get; set; }
        public Guid RecId { get; set; }
        protected override IValidator GetValidator()
        {
            return new DynamicPluginVisibleMapperValidator();
        }
    }

    public class DynamicPluginVisibleMapperValidator : AbstractValidator<DynamicPluginVisibleMapper>
    {
        public DynamicPluginVisibleMapperValidator()
        {
            RuleFor(d => d.EntityId).NotNull().WithMessage("实体ID不能为空");
            RuleFor(d => d.RecId).NotNull().WithMessage("数据ID不能为空");
        }
    }

    public class DynamicPageVisibleMapper : BaseEntity
    {
        public Guid EntityId { get; set; }
        public Guid RecId { get; set; }
        public int PageType { get; set; }
        public string PageCode { get; set; }
        protected override IValidator GetValidator()
        {
            return new DynamicPageVisibleMapperValidator();
        }
    }

    public class DynamicPageVisibleMapperValidator : AbstractValidator<DynamicPageVisibleMapper>
    {
        public DynamicPageVisibleMapperValidator()
        {
            RuleFor(d => d.EntityId).NotNull().WithMessage("实体ID不能为空");
            RuleFor(d => d.RecId).NotNull().WithMessage("数据ID不能为空");
            RuleFor(d => d.PageType).NotNull().WithMessage("页面类型不能为空");
            RuleFor(d => d.PageCode).NotEmpty().WithMessage("页面编码不能为空");
        }
    }

    public class DynamicEntityTransferMapper : BaseEntity
    {
        public Guid EntityId { get; set; }
        public string RecId { get; set; }
        public int Manager { get; set; }
        protected override IValidator GetValidator()
        {
            return new DynamicEntityTransferMapperValidator();
        }
    }

    public class DynamicEntityTransferMapperValidator : AbstractValidator<DynamicEntityTransferMapper>
    {
        public DynamicEntityTransferMapperValidator()
        {
            RuleFor(d => d.EntityId).NotNull().WithMessage("实体ID不能为空");
            RuleFor(d => d.RecId).NotEmpty().WithMessage("数据ID不能为空");
            RuleFor(d => d.Manager).NotNull().GreaterThan(0).WithMessage("新的负责人不能为空");
        }
    }

    public class DynamicEntityAddConnectMapper : BaseEntity
    {
        public Guid EntityIdUp { get; set; }
        public Guid EntityIdTo { get; set; }
        public Guid RecIdUp { get; set; }
        public Guid RecIdTo { get; set; }
        public string Remark { get; set; }
        protected override IValidator GetValidator()
        {
            return new DynamicEntityAddConnectMapperValidator();
        }
    }

    public class DynamicEntityAddConnectMapperValidator : AbstractValidator<DynamicEntityAddConnectMapper>
    {
        public DynamicEntityAddConnectMapperValidator()
        {
            RuleFor(d => d.EntityIdUp).NotNull().WithMessage("Up实体ID不能为空");
            RuleFor(d => d.EntityIdTo).NotNull().WithMessage("To实体ID不能为空");
            RuleFor(d => d.RecIdUp).NotNull().WithMessage("Up记录ID不能为空");
            RuleFor(d => d.RecIdTo).NotNull().WithMessage("To记录ID不能为空");
        }
    }

    public class DynamicEntityEditConnectMapper : BaseEntity
    {
        public Guid ConnectId { get; set; }
        public Guid EntityIdTo { get; set; }
        public Guid RecIdTo { get; set; }
        protected override IValidator GetValidator()
        {
            return new DynamicEntityEditConnectMapperValidator();
        }
    }

    public class DynamicEntityEditConnectMapperValidator : AbstractValidator<DynamicEntityEditConnectMapper>
    {
        public DynamicEntityEditConnectMapperValidator()
        {
            RuleFor(d => d.ConnectId).NotNull().WithMessage("关系ID不能为空");
            RuleFor(d => d.EntityIdTo).NotNull().WithMessage("To实体ID不能为空");
            RuleFor(d => d.RecIdTo).NotNull().WithMessage("To记录ID不能为空");
        }
    }

    public class DynamicEntityConnectListMapper : BaseEntity
    {
        public Guid EntityId { get; set; }
        public Guid RecId { get; set; }
        protected override IValidator GetValidator()
        {
            return new DynamicEntityConnectListMapperValidator();
        }
    }

    public class DynamicEntityConnectListMapperValidator : AbstractValidator<DynamicEntityConnectListMapper>
    {
        public DynamicEntityConnectListMapperValidator()
        {
            RuleFor(d => d.EntityId).NotNull().WithMessage("实体ID不能为空");
            RuleFor(d => d.RecId).NotNull().WithMessage("实体数据ID不能为空");
        }
    }

    public class RelConfig : BaseEntity
    {
        public Guid RecId { get; set; }
        public int Index { get; set; }
        //0配置1函数2服务
        public int Type { get; set; }

        public Guid RelId { get; set; }
        public Guid RelentityId { get; set; }
        public Guid FieldId { get; set; }
        //0直接取值1求和2求平均3计数
        public int CalcuteType { get; set; }
        public Guid EntityId { get; set; }
        protected override IValidator GetValidator()
        {
            return new RelConfigValidator();
        }
        class RelConfigValidator : AbstractValidator<RelConfig>
        {
            public RelConfigValidator()
            {
                RuleFor(d => d.EntityId).NotNull().WithMessage("实体ID不能为空");
            }
        }
    }

    public class RelConfigSet : BaseEntity
    {
        public Guid RecId { get; set; }
        public Guid RelId { get; set; }
        public string ConfigSet1 { get; set; }
        public string title1 { get; set; }
        public string ConfigSet2 { get; set; }
        public string title2 { get; set; }
        public string ConfigSet3 { get; set; }
        public string title3 { get; set; }
        public string ConfigSet4 { get; set; }
        public string title4 { get; set; }
        protected override IValidator GetValidator()
        {
            return new RelConfigSetValidator();
        }
        class RelConfigSetValidator : AbstractValidator<RelConfigSet>
        {
            public RelConfigSetValidator()
            {
                RuleFor(d => d.RelId).NotNull().WithMessage("RelID不能为空");
            }
        }
    }

    public class RelTabListMapper : BaseEntity
    {
        public int DeviceType { get; set; }
        public Guid EntityId { get; set; }
        protected override IValidator GetValidator()
        {
            return new RelTabListMapperValidator();
        }
        class RelTabListMapperValidator : AbstractValidator<RelTabListMapper>
        {
            public RelTabListMapperValidator()
            {
                RuleFor(d => d.EntityId).NotNull().WithMessage("实体ID不能为空");
            }
        }
    }

    public class RelTabInfoMapper : BaseEntity
    {
        public Guid RelId { get; set; }
        protected override IValidator GetValidator()
        {
            return new RelTabInfoMapperValidator();
        }
        class RelTabInfoMapperValidator : AbstractValidator<RelTabInfoMapper>
        {
            public RelTabInfoMapperValidator()
            {
                RuleFor(d => d.RelId).NotNull().WithMessage("页签ID不能为空");
            }
        }
    }

    public class RelTabSrcMapper
    {
        public Guid RelId { get; set; }
        public Guid EntityId { get; set; }
        public int IsManyToMany { get; set; }
        public string SrcSql { get; set; }
    }

    public class RelTabSrcValListMapper
    {
        public Guid ID { get; set; }

        public string Name { get; set; }

        public Guid FieldId { get; set; }

        public Guid EntityId { get; set; }

    }

    public class AddRelTabRelationMapper
    {
        public Guid EntityId { get; set; }

        public Guid FieldId { get; set; }
    }
    public class AddRelTabMapper : BaseEntity
    {
        public Guid EntityId { get; set; }
        public Guid RelEntityId { get; set; }
        public Guid FieldId { get; set; }
        public string RelName { get; set; }
        public Guid ICon { get; set; }

        public int IsManyToMany { get; set; }
        public string SrcSql { get; set; }

        public string SrcTitle { get; set; }
        protected override IValidator GetValidator()
        {
            return new AddRelTabMapperValidator();
        }
        class AddRelTabMapperValidator : AbstractValidator<AddRelTabMapper>
        {
            public AddRelTabMapperValidator()
            {
                RuleFor(d => d.EntityId).NotNull().WithMessage("实体ID不能为空");
                RuleFor(d => d.RelEntityId).NotNull().WithMessage("引用实体ID不能为空");
                RuleFor(d => d.FieldId).NotNull().WithMessage("引用实体ID不能为空");
                RuleFor(d => d.RelName).NotNull().WithMessage("页签名称不能为空");
                RuleFor(d => d.ICon).NotNull().WithMessage("页签图标不能为空");
            }
        }
    }
    public class UpdateRelTabMapper : BaseEntity
    {
        public Guid RelId { get; set; }
        public Guid RelEntityId { get; set; }
        public Guid FieldId { get; set; }
        public string RelName { get; set; }
        public string ICon { get; set; }

        public int IsManyToMany { get; set; }
        public string SrcSql { get; set; }
        public string SrcTitle { get; set; }
        protected override IValidator GetValidator()
        {
            return new UpdateRelTabMapperValidator();
        }
        class UpdateRelTabMapperValidator : AbstractValidator<UpdateRelTabMapper>
        {
            public UpdateRelTabMapperValidator()
            {
                RuleFor(d => d.RelId).NotNull().WithMessage("页签ID不能为空");
                RuleFor(d => d.RelEntityId).NotNull().WithMessage("引用实体ID不能为空");
                RuleFor(d => d.RelName).NotNull().WithMessage("页签名称不能为空");
                RuleFor(d => d.ICon).NotNull().WithMessage("页签图标不能为空");
            }
        }
    }
    public class DisabledRelTabMapper : BaseEntity
    {
        public Guid RelId { get; set; }

        protected override IValidator GetValidator()
        {
            return new DisabledRelTabMapperValidator();
        }
        class DisabledRelTabMapperValidator : AbstractValidator<DisabledRelTabMapper>
        {
            public DisabledRelTabMapperValidator()
            {
                RuleFor(d => d.RelId).NotNull().WithMessage("页签ID不能为空");
            }
        }
    }

    public class AddRelTabRelationDataSrcMapper : BaseEntity
    {
        public Guid RelId { get; set; }
        public Guid RecId { get; set; }
        public string IdStr { get; set; }

        protected override IValidator GetValidator()
        {
            return new AddRelTabRelationDataSrcMapperValidator();
        }
        class AddRelTabRelationDataSrcMapperValidator : AbstractValidator<AddRelTabRelationDataSrcMapper>
        {
            public AddRelTabRelationDataSrcMapperValidator()
            {
                RuleFor(d => d.RelId).NotNull().WithMessage("实体ID不能为空");

                RuleFor(d => d.RecId).NotNull().WithMessage("记录ID不能为空");
                RuleFor(d => d.IdStr).NotEmpty().WithMessage("记录ID不能为空");
            }
        }
    }

    public class GetEntityFieldsMapper : BaseEntity
    {
        public Guid EntityId { get; set; }
        public Guid RelEntityId { get; set; }

        protected override IValidator GetValidator()
        {
            return new GetEntityFieldsMapperValidator();
        }
        class GetEntityFieldsMapperValidator : AbstractValidator<GetEntityFieldsMapper>
        {
            public GetEntityFieldsMapperValidator()
            {
                RuleFor(d => d.EntityId).NotNull().WithMessage("当前实体ID不能为空");
                RuleFor(d => d.RelEntityId).NotNull().WithMessage("关联实体ID不能为空");
            }
        }
    }

    public class OrderbyRelTabMapper : BaseEntity
    {
        public string RelIds { get; set; }

        protected override IValidator GetValidator()
        {
            return new OrderbyRelTabMapperValidator();
        }
        class OrderbyRelTabMapperValidator : AbstractValidator<OrderbyRelTabMapper>
        {
            public OrderbyRelTabMapperValidator()
            {
                RuleFor(d => d.RelIds).NotNull().WithMessage("页签ID不能为空");
            }
        }
    }


    public class FollowRecordMapper : BaseEntity
    {
        public Guid RelId { get; set; }
        public Guid EntityId { get; set; }

        public bool IsFollow { get; set; }

        protected override IValidator GetValidator()
        {
            return new FollowRecordMapperValidator();
        }
        class FollowRecordMapperValidator : AbstractValidator<FollowRecordMapper>
        {
            public FollowRecordMapperValidator()
            {
                RuleFor(d => d.RelId).NotNull().WithMessage("ID不能为空");
                RuleFor(d => d.EntityId).NotNull().WithMessage("EntityId不能为空");
                RuleFor(d => d.IsFollow).NotNull().WithMessage("IsFollow不能为空");
            }
        }
    }

    /// <summary>
    /// 用于WEB列表字段个性化设置
    /// </summary>
    public class WebListPersonalViewSettingInfo
    {
        public int FixedColumnCount { get; set; }
        public List<WebListPersonalViewColumnSettingInfo> Columns { get; set; }
    }
    /// <summary>
    /// 用于WEB列表字段个性化设置
    /// </summary>
    public class WebListPersonalViewColumnSettingInfo
    {
        public int Seq { get; set; }
        public Guid FieldId { get; set; }
        public int Width { get; set; }
        public int IsDisplay { get; set; }
    }
}
