using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace UBeat.Crm.CoreApi.DomainModel.Products
{
    public class ProductSeriesInsert : BaseEntity
    {

        public Guid TopSeriesId { get; set; }

        /// <summary>
        /// 产品系列名称
        /// </summary>
        public string SeriesName { get; set; }

        /// <summary>
        /// 父品系列名称
        /// </summary>
        public string ParentSeriesName { get; set; }

        /// <summary>
        /// 系列编码
        /// </summary>
        public string SeriesCode { get; set; }


        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<ProductSeriesInsert>
        {
            public Validator()
            {
                RuleFor(d => d.SeriesName).NotNull().WithMessage("SeriesName不能为空");
                RuleFor(d => d.SeriesCode).NotNull().WithMessage("SeriesCode不能为空");
            }
        }

    }


    public class ProductSeriesEdit : BaseEntity
    {
        /// <summary>
        /// 产品系列id
        /// </summary>
        public Guid ProductsetId { get; set; }

        /// <summary>
        /// 产品系列名称
        /// </summary>
        public string SeriesName { get; set; }

        /// <summary>
        /// 产品系列编码
        /// </summary>
        public string SeriesCode { get; set; }

        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<ProductSeriesEdit>
        {
            public Validator()
            {
                RuleFor(d => d.ProductsetId).NotNull().WithMessage("ProductsetId不能为空");
                RuleFor(d => d.SeriesName).NotNull().WithMessage("SeriesName不能为空");
                RuleFor(d => d.SeriesCode).NotNull().WithMessage("SeriesCode不能为空");
            }
        }

    }


    public class ProductInsert : BaseEntity
    {

        /// <summary>
        /// 产品名称
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 产品系列名称
        /// </summary>
        public string ProductSeriesName { get; set; }

        /// <summary>
        /// 产品描述
        /// </summary>
        public string ProductFeatures { get; set; }


        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<ProductInsert>
        {
            public Validator()
            {
                RuleFor(d => d.ProductName).NotNull().WithMessage("ProductName不能为空");
                RuleFor(d => d.ProductSeriesName).NotNull().WithMessage("ProductSeriesName不能为空");
            }
        }

    }

    public class ProductUpdate : BaseEntity
    {
        public Guid ProductId { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 产品系列名称
        /// </summary>
        public string ProductSeriesName { get; set; }

        /// <summary>
        /// 产品描述
        /// </summary>
        public string ProductFeatures { get; set; }


        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<ProductUpdate>
        {
            public Validator()
            {
                RuleFor(d => d.ProductId).NotNull().WithMessage("ProductId不能为空");
                RuleFor(d => d.ProductName).NotNull().WithMessage("ProductName不能为空");
            }
        }

    }


    public class ProductList : BaseEntity
    {

        /// <summary>
        /// 产品系列id
        /// </summary>
        public Guid ProductSeriesId { get; set; }


        /// <summary>
        /// 产品version
        /// </summary>
        public Int64 RecVersion { get; set; }

        /// <summary>
        /// 产品状态
        /// </summary>
        public int RecStatus { get; set; }


        /// <summary>
        /// 是否包括子系列产品
        /// </summary>
        public bool IsAllProduct { get; set; }


        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<ProductList>
        {
            public Validator()
            {

            }
        }
    }





}
