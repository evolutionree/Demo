using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.SalesStage
{
    public class SalesStageMapper
    {

    }

    public class SalesStageRestartMapper : BaseEntity
    {
        public string RecId { get; set; }
        public string TypeId { get; set; }
        protected override IValidator GetValidator()
        {
            return new SalesStageRestartMapperValidator();
        }
        class SalesStageRestartMapperValidator : AbstractValidator<SalesStageRestartMapper>
        {
            public SalesStageRestartMapperValidator()
            {
                RuleFor(d => d.RecId).NotEmpty().WithMessage("商机Id不能为空");
            }
        }
    }

    public class SalesstageTypeMapper : BaseEntity
    {
        public string SalesstageTypeId { get; set; }
        public int ForAdmin { get; set; }
        public SalesstageTypeMapper() {
            ForAdmin = 0;
        }
        protected override IValidator GetValidator()
        {
            return new SalesstageTypeMapperValidator();
        }
        class SalesstageTypeMapperValidator : AbstractValidator<SalesstageTypeMapper>
        {
            public SalesstageTypeMapperValidator()
            {
                RuleFor(d => d.SalesstageTypeId).NotEmpty().WithMessage("商机类型Id不能为空");
            }
        }
    }

    public class SaveSalesStageMapper : BaseEntity
    {
        public string SalesStageTypeId { get; set; }
        public string SalesStageId { get; set; }
        public string StageName { get; set; }
        public decimal WinRate { get; set; }
        public Dictionary<string, string> StageName_Lang { get; set; }
        protected override IValidator GetValidator()
        {
            return new SaveSalesStageMapperValidator();
        }
        class SaveSalesStageMapperValidator : AbstractValidator<SaveSalesStageMapper>
        {
            public SaveSalesStageMapperValidator()
            {
                RuleFor(d => d.StageName).NotEmpty().WithMessage("销售阶段名称不能为空");
                RuleFor(d => d.WinRate).NotEmpty().WithMessage("销售阶段赢率不能为空");
                RuleFor(d => d.SalesStageTypeId).NotNull().WithMessage("商机类型不能为空");
            }
        }
    }

    public class DisabledSalesStageMapper : BaseEntity
    {
        public string SalesStageId { get; set; }

        public int RecStatus { get; set; }
        protected override IValidator GetValidator()
        {
            return new DisabledSalesStageMaperValidator();
        }
        class DisabledSalesStageMaperValidator : AbstractValidator<DisabledSalesStageMapper>
        {
            public DisabledSalesStageMaperValidator()
            {
                RuleFor(d => d.SalesStageId).NotEmpty().WithMessage("销售阶段Id不能为空");
            }
        }
    }
    public class OrderBySalesStageMapper : BaseEntity
    {
        public string SalesStageIds { get; set; }
        protected override IValidator GetValidator()
        {
            return new OrderBySalesStageMapperValidator();
        }
        class OrderBySalesStageMapperValidator : AbstractValidator<OrderBySalesStageMapper>
        {
            public OrderBySalesStageMapperValidator()
            {
                RuleFor(d => d.SalesStageIds).NotEmpty().WithMessage("销售阶段Id不能为空");
            }
        }
    }

    public class OpenHighSettingMapper : BaseEntity
    {
        public string TypeId { get; set; }
        public int IsOpenHighSetting { get; set; }
        protected override IValidator GetValidator()
        {
            return new OpenHighSettingMapperValidator();
        }
        class OpenHighSettingMapperValidator : AbstractValidator<OpenHighSettingMapper>
        {
            public OpenHighSettingMapperValidator()
            {
                RuleFor(d => d.TypeId).NotEmpty().WithMessage("实体类型Id不能为空");
            }
        }
    }

    public class AddSalesStageEventSetMapper : BaseEntity
    {
        public string EventName { get; set; }

        public int IsNeedUpFile { get; set; }

        public string SalesStageId { get; set; }

        protected override IValidator GetValidator()
        {
            return new AddSalesStageEventSetMapperValidator();
        }
        class AddSalesStageEventSetMapperValidator : AbstractValidator<AddSalesStageEventSetMapper>
        {
            public AddSalesStageEventSetMapperValidator()
            {
                RuleFor(d => d.EventName).NotEmpty().WithMessage("关键事件名称不能为空");
                RuleFor(d => d.SalesStageId).NotEmpty().WithMessage("销售阶段Id不能为空");
            }
        }
    }

    public class EditSalesStageEventSetMapper : BaseEntity
    {
        public string EventSetId { get; set; }
        public string EventName { get; set; }

        public int IsNeedUpFile { get; set; }

        public string SalesStageId { get; set; }

        protected override IValidator GetValidator()
        {
            return new EditSalesStageEventSetMapperValidator();
        }
        class EditSalesStageEventSetMapperValidator : AbstractValidator<EditSalesStageEventSetMapper>
        {
            public EditSalesStageEventSetMapperValidator()
            {
                RuleFor(d => d.EventName).NotEmpty().WithMessage("关键事件名称不能为空");
                RuleFor(d => d.SalesStageId).NotEmpty().WithMessage("销售阶段Id不能为空");
                RuleFor(d => d.EventSetId).NotEmpty().WithMessage("关键事件Id不能为空");
            }
        }
    }
    public class SalesStageSetLstMapper : BaseEntity
    {
        public string SalesStageId { get; set; }
        protected override IValidator GetValidator()
        {
            return new SalesStageSetLstMapperValidator();
        }
        class SalesStageSetLstMapperValidator : AbstractValidator<SalesStageSetLstMapper>
        {
            public SalesStageSetLstMapperValidator()
            {
                RuleFor(d => d.SalesStageId).NotEmpty().WithMessage("销售阶段Id不能为空");
            }
        }
    }
    public class DisabledSalesStageEventSetMapper : BaseEntity
    {
        public string EventSetId { get; set; }
        protected override IValidator GetValidator()
        {
            return new DisabledSalesStageEventSetMapperValidator();
        }
        class DisabledSalesStageEventSetMapperValidator : AbstractValidator<DisabledSalesStageEventSetMapper>
        {
            public DisabledSalesStageEventSetMapperValidator()
            {
                RuleFor(d => d.EventSetId).NotEmpty().WithMessage("关键事件Id不能为空");
            }
        }
    }



    //public class SalesStageOppInfoSetLstMapper : BaseEntity
    //{
    //    public string SalesStageId { get; set; }
    //    protected override IValidator GetValidator()
    //    {
    //        return new SalesStageEventSetLstMapperValidator();
    //    }
    //    class SalesStageEventSetLstMapperValidator : AbstractValidator<SalesStageEventSetLstMapper>
    //    {
    //        public SalesStageEventSetLstMapperValidator()
    //        {
    //            RuleFor(d => d.SalesStageId).NotEmpty().WithMessage("销售阶段Id不能为空");
    //        }
    //    }
    //}

    public class SalesStageOppInfoFieldsMapper : BaseEntity
    {
        public string EntityId { get; set; }
        public string SalesStageId { get; set; }
        public string SalesStageTypeId { get; set; }
        protected override IValidator GetValidator()
        {
            return new SalesStageOppInfoFieldsMapperValidator();
        }
        class SalesStageOppInfoFieldsMapperValidator : AbstractValidator<SalesStageOppInfoFieldsMapper>
        {
            public SalesStageOppInfoFieldsMapperValidator()
            {
                RuleFor(d => d.SalesStageId).NotEmpty().WithMessage("销售阶段Id不能为空");
                RuleFor(d => d.EntityId).NotEmpty().WithMessage("实体Id不能为空");
                RuleFor(d => d.SalesStageTypeId).NotEmpty().WithMessage("实体类型Id不能为空");
            }
        }
    }


    public class SaveSalesStageOppInfoSetMapper : BaseEntity
    {
        public string EntityId { get; set; }

        public string FieldIds { get; set; }

        public string SalesStageId { get; set; }
        protected override IValidator GetValidator()
        {
            return new SaveSalesStageOppInfoSetMapperrValidator();
        }
        class SaveSalesStageOppInfoSetMapperrValidator : AbstractValidator<SaveSalesStageOppInfoSetMapper>
        {
            public SaveSalesStageOppInfoSetMapperrValidator()
            {
                RuleFor(d => d.SalesStageId).NotEmpty().WithMessage("销售阶段Id不能为空");
                RuleFor(d => d.EntityId).NotEmpty().WithMessage("实体Id不能为空");
            }
        }
    }


    public class AddSalesStageDynEntitySetMapper : BaseEntity
    {
        public string RelEntityId { get; set; }
        public string SalesStageId { get; set; }

        protected override IValidator GetValidator()
        {
            return new AddSalesStageDynEntitySetMapperValidator();
        }
        class AddSalesStageDynEntitySetMapperValidator : AbstractValidator<AddSalesStageDynEntitySetMapper>
        {
            public AddSalesStageDynEntitySetMapperValidator()
            {
                RuleFor(d => d.RelEntityId).NotEmpty().WithMessage("关联实体Id不能为空");
                RuleFor(d => d.SalesStageId).NotEmpty().WithMessage("销售阶段Id不能为空");
            }
        }
    }


    public class DelSalesStageDynEntitySetMapper : BaseEntity
    {
        public string DynEntityId { get; set; }

        protected override IValidator GetValidator()
        {
            return new DelSalesStageDynEntitySetMapperValidator();
        }
        class DelSalesStageDynEntitySetMapperValidator : AbstractValidator<DelSalesStageDynEntitySetMapper>
        {
            public DelSalesStageDynEntitySetMapperValidator()
            {
                RuleFor(d => d.DynEntityId).NotEmpty().WithMessage("关联自定义表单Id不能为空");
            }
        }
    }




    #region 销售阶段推进
    public class SalesStageStepInfoMapper : BaseEntity
    {
        public string RecId { get; set; }

        public string SalesStageId { get; set; }

        public string SalesStageTypeId { get; set; }

        protected override IValidator GetValidator()
        {
            return new SalesStageStepInfoMapperValidator();
        }
        class SalesStageStepInfoMapperValidator : AbstractValidator<SalesStageStepInfoMapper>
        {
            public SalesStageStepInfoMapperValidator()
            {
                RuleFor(d => d.RecId).NotEmpty().WithMessage("商机Id不能为空");
                RuleFor(d => d.SalesStageTypeId).NotEmpty().WithMessage("实体类型Id不能为空");
                RuleFor(d => d.SalesStageId).NotEmpty().WithMessage("销售阶段Id不能为空");
            }
        }
    }

    public class PushSalesStageStepInfoMapper : BaseEntity
    {
        public string RecId { get; set; }

        public string SalesStageIds { get; set; }


        protected override IValidator GetValidator()
        {
            return new PushSalesStageStepInfoMapperValidator();
        }
        class PushSalesStageStepInfoMapperValidator : AbstractValidator<PushSalesStageStepInfoMapper>
        {
            public PushSalesStageStepInfoMapperValidator()
            {
                RuleFor(d => d.RecId).NotEmpty().WithMessage("商机Id不能为空");
                RuleFor(d => d.SalesStageIds).NotEmpty().WithMessage("销售阶段Id不能为空");
            }
        }
    }

    public class ReturnSalesStageStepInfoMapper : BaseEntity
    {
        public string RecId { get; set; }

        public string TypeId { get; set; }
        public string SalesStageId { get; set; }


        protected override IValidator GetValidator()
        {
            return new ReturnSalesStageStepInfoMapperValidator();
        }
        class ReturnSalesStageStepInfoMapperValidator : AbstractValidator<ReturnSalesStageStepInfoMapper>
        {
            public ReturnSalesStageStepInfoMapperValidator()
            {
                RuleFor(d => d.RecId).NotEmpty().WithMessage("商机Id不能为空");
                RuleFor(d => d.SalesStageId).NotEmpty().WithMessage("销售阶段Id不能为空");
            }
        }
    }

    public class SaveSalesStageStepInfoMapper : BaseEntity
    {
        public int SalesStageFlag { get; set; }
        public string RecId { get; set; }
        public string TypeId { get; set; }
        public string SalesStageId { get; set; }
        public string RelRecId { get; set; }
        public string SalesStageIds { get; set; }
        public int IsWeb { get; set; }

        public ICollection<EventSetMapper> Event { get; set; }

        protected override IValidator GetValidator()
        {
            return new SaveSalesStageStepInfoMapperValidator();
        }
        class SaveSalesStageStepInfoMapperValidator : AbstractValidator<SaveSalesStageStepInfoMapper>
        {
            public SaveSalesStageStepInfoMapperValidator()
            {
                RuleFor(d => d.RecId).NotEmpty().WithMessage("商机Id不能为空");
                RuleFor(d => d.SalesStageId).NotEmpty().WithMessage("销售阶段Id不能为空");
            }
        }
    }

    public class SaveDynEntityMapper : BaseEntity
    {
        public string RecId { get; set; }
        public string DynRecId { get; set; }
        public string SalesStageId { get; set; }

        protected override IValidator GetValidator()
        {
            return new SaveDynEntityMapperValidator();
        }
        class SaveDynEntityMapperValidator : AbstractValidator<SaveDynEntityMapper>
        {
            public SaveDynEntityMapperValidator()
            {
                RuleFor(d => d.RecId).NotEmpty().WithMessage("商机Id不能为空");
                RuleFor(d => d.SalesStageId).NotEmpty().WithMessage("销售阶段Id不能为空");
            }
        }
    }

    public class UpdateOpportunityStatusMapper : BaseEntity
    {
        public string RecId { get; set; }

        public string SalesStageId { get; set; }

        protected override IValidator GetValidator()
        {
            return new UpdateOpportunityStatusMapperValidator();
        }
        class UpdateOpportunityStatusMapperValidator : AbstractValidator<UpdateOpportunityStatusMapper>
        {
            public UpdateOpportunityStatusMapperValidator()
            {
                RuleFor(d => d.RecId).NotEmpty().WithMessage("商机Id不能为空");
                RuleFor(d => d.SalesStageId).NotEmpty().WithMessage("销售阶段Id不能为空");
            }
        }
    }

    public class DynEntitySetMapper : BaseEntity
    {
        public string SalesStageId { get; set; }

        public int IsFinish { get; set; }
        protected override IValidator GetValidator()
        {
            return new DynEntitySetMapperValidator();
        }
        class DynEntitySetMapperValidator : AbstractValidator<DynEntitySetMapper>
        {
            public DynEntitySetMapperValidator()
            {
                RuleFor(d => d.SalesStageId).NotEmpty().WithMessage("销售阶段Id不能为空");
            }
        }
    }
    public class EventSetMapper
    {
        public int isfinish { get; set; }

        public int isuploadfile { get; set; }

        public string fileid { get; set; }

        public string targetid
        {
            get;
            set;
        }


        public string eventsetid { get; set; }
    }

    public class OppInfoSetMapper : BaseEntity
    {
        public string fieldid { get; set; }
        public string fieldvalue { get; set; }
        public int isfinish { get; set; }
        protected override IValidator GetValidator()
        {
            return new OppInfoSetMapperValidator();
        }
        class OppInfoSetMapperValidator : AbstractValidator<OppInfoSetMapper>
        {
            public OppInfoSetMapperValidator()
            {
                RuleFor(d => d.fieldid).NotEmpty().WithMessage("字段Id不能为空");
            }
        }
    }

    public class LoseOrderMapper : BaseEntity
    {
        public string LoseOrderId { get; set; }
        public string LoseReason { get; set; }

        public string ReasonSupplement { get; set; }

        public string OpportunityId { get; set; }

        protected override IValidator GetValidator()
        {
            return new LoseOrderMapperValidator();
        }
        class LoseOrderMapperValidator : AbstractValidator<LoseOrderMapper>
        {
            public LoseOrderMapperValidator()
            {
                RuleFor(d => d.OpportunityId).NotEmpty().WithMessage("商机Id不能为空");
                RuleFor(d => d.LoseReason).NotEmpty().WithMessage("输单原因不能为空");
            }
        }
    }

    public class WinOrderMapper : BaseEntity
    {
        public string WinOrderId { get; set; }
        public string IncomeType { get; set; }

        public string Remark { get; set; }
        public DateTime SigneDate { get; set; }

        public string OpportunityId { get; set; }

        protected override IValidator GetValidator()
        {
            return new WinOrderMapperValidator();
        }
        class WinOrderMapperValidator : AbstractValidator<WinOrderMapper>
        {
            public WinOrderMapperValidator()
            {
                RuleFor(d => d.OpportunityId).NotEmpty().WithMessage("商机Id不能为空");
                RuleFor(d => d.SigneDate).NotEmpty().WithMessage("签订日期不能为空");
                RuleFor(d => d.IncomeType).NotEmpty().WithMessage("收入类型不能为空");
            }
        }
    }

    public class OrderInfoMapper : BaseEntity
    {
        public string OpportunityId { get; set; }
        protected override IValidator GetValidator()
        {
            return new OrderInfoMapperValidator();
        }
        class OrderInfoMapperValidator : AbstractValidator<OrderInfoMapper>
        {
            public OrderInfoMapperValidator()
            {
                RuleFor(d => d.OpportunityId).NotEmpty().WithMessage("商机Id不能为空");
            }
        }
    }
    #endregion
}
