using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;


namespace UBeat.Crm.CoreApi.DomainModel.SalesTarget
{
    public class SalesTargetMapper : BaseEntity
    {

        public string RecId { get; set; }
        public string TypeId { get; set; }
        protected override IValidator GetValidator()
        {
            return new SalesStageRestartMapperValidator();
        }
        class SalesStageRestartMapperValidator : AbstractValidator<SalesTargetMapper>
        {
            public SalesStageRestartMapperValidator()
            {
                RuleFor(d => d.RecId).NotNull().WithMessage("商机Id不能为空");
            }
        }
    }


    public class SalesTargetSelectMapper : BaseEntity
    {

        public int Year { get; set; }
        public Guid NormTypeId { get; set; }
        public Guid DepartmentId { get; set; }

        public string SearchName { get; set; }



        protected override IValidator GetValidator()
        {
            return new SalesTargetSelectMapperValidator();
        }
        class SalesTargetSelectMapperValidator : AbstractValidator<SalesTargetSelectMapper>
        {
            public SalesTargetSelectMapperValidator()
            {
                RuleFor(d => d.Year).NotNull().WithMessage("年份不能为空");
                RuleFor(d => d.NormTypeId).NotNull().WithMessage("指标类型不能为空");
                RuleFor(d => d.DepartmentId).NotNull().WithMessage("部门Id不能为空");

            }
        }
    }

    public class SalesTargetEditMapper : BaseEntity
    {


        public Guid? TargetId { get; set; }


        /// <summary>
        ///  年份
        /// </summary>
        public int Year { get; set; }


        /// <summary>
        /// 全年目标
        /// </summary>
        public decimal YearCount { get; set; }

        /// <summary>
        /// 一月
        /// </summary>
        public decimal JanCount { get; set; }


        /// <summary>
        /// 二月
        /// </summary>
        public decimal FebCount { get; set; }

        /// <summary>
        /// 三月
        /// </summary>
        public decimal MarCount { get; set; }

        /// <summary>
        /// 四月
        /// </summary>
        public decimal AprCount { get; set; }

        /// <summary>
        /// 五月
        /// </summary>
        public decimal MayCount { get; set; }

        /// <summary>
        /// 六月
        /// </summary>
        public decimal JunCount { get; set; }

        /// <summary>
        /// 七月
        /// </summary>
        public decimal JulCount { get; set; }

        /// <summary>
        /// 八月
        /// </summary>
        public decimal AugCount { get; set; }

        /// <summary>
        /// 九月
        /// </summary>
        public decimal SepCount { get; set; }

        /// <summary>
        /// 十月
        /// </summary>
        public decimal OctCount { get; set; }

        /// <summary>
        ///十一 月
        /// </summary>
        public decimal NovCount { get; set; }

        /// <summary>
        /// 十二月
        /// </summary>
        public decimal DecCount { get; set; }

        /// <summary>
        /// 用户id
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 部门id
        /// </summary>
        public Guid DepartmentId { get; set; }

        /// <summary>
        /// 是否团队销售目标
        /// </summary>
        public bool IsGroupTarget { get; set; }

        /// <summary>
        /// 销售目标指标类型
        /// </summary>
        public int TargetType { get; set; }


        protected override IValidator GetValidator()
        {
            return new SalesTargetEditMapperValidator();
        }
        class SalesTargetEditMapperValidator : AbstractValidator<SalesTargetEditMapper>
        {
            public SalesTargetEditMapperValidator()
            {
                RuleFor(d => d.Year).NotNull().WithMessage("年份不能为空");
                RuleFor(d => d.TargetType).NotNull().WithMessage("指标类型不能为空");
            }
        }
    }

    public class SalesTargetInsertMapper : BaseEntity
    {
        /// <summary>
        ///  年份
        /// </summary>
        public int Year { get; set; }


        /// <summary>
        /// 全年目标
        /// </summary>
        public decimal YearCount { get; set; }

        /// <summary>
        /// 一月
        /// </summary>
        public decimal JanCount { get; set; }


        /// <summary>
        /// 二月
        /// </summary>
        public decimal FebCount { get; set; }

        /// <summary>
        /// 三月
        /// </summary>
        public decimal MarCount { get; set; }

        /// <summary>
        /// 四月
        /// </summary>
        public decimal AprCount { get; set; }

        /// <summary>
        /// 五月
        /// </summary>
        public decimal MayCount { get; set; }

        /// <summary>
        /// 六月
        /// </summary>
        public decimal JunCount { get; set; }

        /// <summary>
        /// 七月
        /// </summary>
        public decimal JulCount { get; set; }

        /// <summary>
        /// 八月
        /// </summary>
        public decimal AugCount { get; set; }

        /// <summary>
        /// 九月
        /// </summary>
        public decimal SepCount { get; set; }

        /// <summary>
        /// 十月
        /// </summary>
        public decimal OctCount { get; set; }

        /// <summary>
        ///十一 月
        /// </summary>
        public decimal NovCount { get; set; }

        /// <summary>
        /// 十二月
        /// </summary>
        public decimal DecCount { get; set; }

        /// <summary>
        /// 用户id
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// 部门id
        /// </summary>
        public Guid DepartmentId { get; set; }

        /// <summary>
        /// 是否团队销售目标
        /// </summary>
        public bool IsGroupTarget { get; set; }

        /// <summary>
        /// 销售目标指标类型
        /// </summary>
        public Guid NormTypeId { get; set; }


        protected override IValidator GetValidator()
        {
            return new SalesTargetEditMapperValidator();
        }
        class SalesTargetEditMapperValidator : AbstractValidator<SalesTargetInsertMapper>
        {
            public SalesTargetEditMapperValidator()
            {
                RuleFor(d => d.Year).NotNull().WithMessage("年份不能为空");
                RuleFor(d => d.NormTypeId).NotNull().WithMessage("指标不能为空");
            }
        }
    }

    public class SalesTargetSelectDetailMapper : BaseEntity
    {
        public Guid DepartmentId { get; set; }
        public int UserId { get; set; }

        public Guid NormTypeId { get; set; }

        public bool IsGroupTarget { get; set; }
        public int Year { get; set; }


        protected override IValidator GetValidator()
        {
            return new SalesStageRestartMapperValidator();
        }
        class SalesStageRestartMapperValidator : AbstractValidator<SalesTargetSelectDetailMapper>
        {
            public SalesStageRestartMapperValidator()
            {
                RuleFor(d => d.DepartmentId).NotNull().WithMessage("部门Id不能为空");
                RuleFor(d => d.NormTypeId).NotNull().WithMessage("指标类型不能为空");
                RuleFor(d => d.Year).NotNull().WithMessage("年份不能为空");
            }
        }
    }


    public class VisibleItem
    {

        public string Name { get; set; }

        public string Value { get; set; }

    }


    public class SalesTargetSetBeginMothMapper : BaseEntity
    {
        public Guid DepartmentId { get; set; }

        public int BeginYear { get; set; }
        public int BeginMonth { get; set; }

        public int UserId { get; set; }

        protected override IValidator GetValidator()
        {
            return new SalesTargetSetBeginMothMapperValidator();
        }

        class SalesTargetSetBeginMothMapperValidator : AbstractValidator<SalesTargetSetBeginMothMapper>
        {
            public SalesTargetSetBeginMothMapperValidator()
            {
                RuleFor(d => d.BeginYear).NotNull().WithMessage("生效年份不能为空");
                RuleFor(d => d.BeginMonth).NotNull().WithMessage("生效月份不能为空");
                RuleFor(d => d.DepartmentId).NotNull().WithMessage("部门Id不能为空");
            }
        }
    }


    public class SalesTargetNormTypeMapper : BaseEntity
    {
        public Guid? Id { get; set; }

        public string Name { get; set; }

        public Guid EntityId { get; set; }

        public string FieldName { get; set; }
        public string BizDateFieldName { get; set; }

        public int CaculateType { get; set; }

        public Dictionary<string, string> NormTypeName_Lang { get; set; }

        protected override IValidator GetValidator()
        {
            return new SalesTargetNormTypeMapperValidator();
        }

        class SalesTargetNormTypeMapperValidator : AbstractValidator<SalesTargetNormTypeMapper>
        {
            public SalesTargetNormTypeMapperValidator()
            {
                RuleFor(d => d.Name).NotNull().WithMessage("销售指标名称不能为空");
            }
        }
    }


    public class SalesTargetNormTypeDeleteMapper : BaseEntity
    {
        public Guid Id { get; set; }


        protected override IValidator GetValidator()
        {
            return new SalesTargetNormTypeDeleteMapperValidator();
        }

        class SalesTargetNormTypeDeleteMapperValidator : AbstractValidator<SalesTargetNormTypeDeleteMapper>
        {
            public SalesTargetNormTypeDeleteMapperValidator()
            {
                RuleFor(d => d.Id).NotNull().WithMessage("销售指标Id不能为空");
            }
        }
    }




    public class SalesTargetNormRuleInsertMapper : BaseEntity
    {
        public string Rule { get; set; }
        public string RuleItem { get; set; }
        public string RuleSet { get; set; }

        public bool IsAdd { get; set; }
        public string RuleRelation { get; set; }

        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<SalesTargetNormRuleInsertMapper>
        {
            public Validator()
            {
                RuleFor(d => d.Rule).NotNull().WithMessage("规则不能为空");
                RuleFor(d => d.RuleItem).NotNull().WithMessage("规则明细不能为空");
                RuleFor(d => d.RuleSet).NotNull().WithMessage("规则设置不能为空");


            }
        }
    }



    public class SalesTargetNormTypeDetailMapper : BaseEntity
    {
        public Guid Id { get; set; }


        protected override IValidator GetValidator()
        {
            return new SalesTargetNormTypeDeleteMapperValidator();
        }

        class SalesTargetNormTypeDeleteMapperValidator : AbstractValidator<SalesTargetNormTypeDeleteMapper>
        {
            public SalesTargetNormTypeDeleteMapperValidator()
            {
                RuleFor(d => d.Id).NotNull().WithMessage("销售指标Id不能为空");
            }
        }
    }


    public class SalesTargetNormRuleMapper
    {
        public Guid NormTypeId { get; set; }
        public string EntityId { get; set; }
        public string FieldName { get; set; }
        public string BizDateFieldName { get; set; }
        public int? CaculateType { get; set; }

        public string RuleId { get; set; }
        public string RuleName { get; set; }
        public int? RecStatus { get; set; }
        public string ItemId { get; set; }
        public string FieldId { get; set; }
        public string ItemName { get; set; }
        public string Operate { get; set; }
        public string RuleSql { get; set; }
        public int? UseType { get; set; }
        public int? RuleType { get; set; }
        public string RuleData { get; set; }
        public string RuleSet { get; set; }
    }




    public class YearSalesTargetSaveMapper : BaseEntity
    {
        public string Id { get; set; }
        public int IsGroup { get; set; }
        public decimal YearCount { get; set; }
        public int Year { get; set; }
        public Guid NormTypeId { get; set; }

        public string DepartmentId { get; set; }

        protected override IValidator GetValidator()
        {
            return new YearSalesTargetSsaveMapperValidator();
        }

        class YearSalesTargetSsaveMapperValidator : AbstractValidator<YearSalesTargetSaveMapper>
        {
            public YearSalesTargetSsaveMapperValidator()
            {
                RuleFor(d => d.Id).NotNull().WithMessage("年度销售目标Id不能为空");
                RuleFor(d => d.IsGroup).NotNull().WithMessage("是否为团队不能为空");
                RuleFor(d => d.YearCount).NotNull().WithMessage("年度销售目标不能为空");
                RuleFor(d => d.Year).NotNull().WithMessage("年份不能为空");
                RuleFor(d => d.NormTypeId).NotNull().WithMessage("销售指标Id不能为空");

            }
        }
    }






}



