using FluentValidation;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;

namespace UBeat.Crm.CoreApi.Desktop
{
    public class DesktopMapper : BaseEntity
    {
        public DesktopMapper() {
            LeftDesktopComponents = new List<DesktopComponentMapper>();
            RightDesktopComponents = new List<DesktopComponentMapper>();
        }
        public Guid DesktopId { get; set; }

        public String DesktopName { get; set; }

        public int DesktopType { get; set; }
        public String LeftItems { get; set; }
        public String RightItems { get; set; }

        public Guid BaseDeskId { get; set; }

        public List<DesktopComponentMapper> LeftDesktopComponents { get; set; }

        public List<DesktopComponentMapper> RightDesktopComponents { get; set; }

        protected override IValidator GetValidator()
        {
            return new DesktopMapperValidator();
        }

        class DesktopMapperValidator : AbstractValidator<DesktopMapper>
        {
            public DesktopMapperValidator()
            {
                //          RuleFor(d => d.DatasourceName).NotNull().WithMessage("数据源名称不能为空");

            }
        }
    }

    public class DesktopComponentMapper : BaseEntity
    {
        public Guid DsComponetId { get; set; }

        public String ComName { get; set; }

        public int ComType { get; set; }

        public Decimal ComWidth { get; set; }

        public int ComHeightType { get; set; }
        public Decimal MinComHeight { get; set; }
        public Decimal MaxComHeight { get; set; }
        public String ComUrl { get; set; }
        public String ComArgs { get; set; }
        public String ComDesciption { get; set; }

        protected override IValidator GetValidator()
        {
            return new DesktopComponentMapperValidator();
        }

        class DesktopComponentMapperValidator : AbstractValidator<DesktopComponentMapper>
        {
            public DesktopComponentMapperValidator()
            {
                //          RuleFor(d => d.DatasourceName).NotNull().WithMessage("数据源名称不能为空");

            }
        }
    }

    public class DesktopRelationMapper : BaseEntity
    {

        public int UserId { get; set; }

        public Guid DesktopId { get; set; }

        protected override IValidator GetValidator()
        {
            return new DesktopRelationMapperValidator();
        }

        class DesktopRelationMapperValidator : AbstractValidator<DesktopRelationMapper>
        {
            public DesktopRelationMapperValidator()
            {
                //          RuleFor(d => d.DatasourceName).NotNull().WithMessage("数据源名称不能为空");

            }
        }
    }

    public class DesktopRunTimeMapper : BaseEntity
    {
        public Guid DesktopId { get; set; }

        public Guid DsComponentId { get; set; }

        public int UserId { get; set; }

        public JObject ComArgs { get; set; }

        protected override IValidator GetValidator()
        {
            return new DesktopRunTimeMapperValidator();
        }

        class DesktopRunTimeMapperValidator : AbstractValidator<DesktopRunTimeMapper>
        {
            public DesktopRunTimeMapperValidator()
            {
                //          RuleFor(d => d.DatasourceName).NotNull().WithMessage("数据源名称不能为空");

            }
        }
    }
}
