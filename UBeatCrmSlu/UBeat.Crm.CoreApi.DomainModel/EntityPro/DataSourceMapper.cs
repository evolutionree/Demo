using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.EntityPro
{
    public class DataSourceListMapper : BaseEntity
    {
        public string DatasourceName { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int RecStatus { get; set; }

        protected override IValidator GetValidator()
        {
            return new DataSourceListMapperValidator();
        }

        class DataSourceListMapperValidator : AbstractValidator<DataSourceListMapper>
        {
            public DataSourceListMapperValidator()
            {
                RuleFor(d => d.DatasourceName).NotNull().WithMessage("数据源名称不能为空");

            }
        }
    }
    public class DataSourceMapper : BaseEntity
    {
        public string DatasourceId { get; set; }
        public string DatasourceName { get; set; }
        public string DataSrcKey { get; set; }
        public int SrcType { get; set; }
        public string EntityId { get; set; }
        public string Srcmark { get; set; }

        public int IsRelatePower { get; set; }
        public string Rulesql { get; set; }
        public int RecStatus { get; set; }

        public int IsPro { get; set; }

        protected override IValidator GetValidator()
        {
            return new DataSourceMapperValidator();
        }

        class DataSourceMapperValidator : AbstractValidator<DataSourceMapper>
        {
            public DataSourceMapperValidator()
            {
                RuleFor(d => d.DatasourceName).NotNull().WithMessage("数据源名称不能为空");

            }
        }
    }

    public class DataSourceDetailMapper : BaseEntity
    {
        public string DatasourceId { get; set; }
        protected override IValidator GetValidator()
        {
            return new DataSourceDetailMapperValidator();
        }

        class DataSourceDetailMapperValidator : AbstractValidator<DataSourceDetailMapper>
        {
            public DataSourceDetailMapperValidator()
            {
                RuleFor(d => d.DatasourceId).NotNull().WithMessage("数据源Id不能为空");

            }
        }
    }
    public class InsertDataSourceConfigMapper : BaseEntity
    {
        public string DataSourceId { get; set; }
        public string EntityId { get; set; }
        public int CssTypeId { get; set; }
        public int ViewStyleId { get; set; }
        public string ColNames { get; set; }
        public string Fonts { get; set; }
        public string Colors { get; set; }
        public string RuleSql { get; set; }

        protected override IValidator GetValidator()
        {
            return new InsertDataSourceConfigMapperValidator();
        }

        class InsertDataSourceConfigMapperValidator : AbstractValidator<InsertDataSourceConfigMapper>
        {
            public InsertDataSourceConfigMapperValidator()
            {
                RuleFor(d => d.DataSourceId).NotNull().WithMessage("数据源Id不能为空");
            }
        }
    }

    public class UpdateDataSourceConfigMapper : BaseEntity
    {
        public string DataConfigId { get; set; }
        public int ViewStyleId { get; set; }
        public string ColNames { get; set; }
        public string Fonts { get; set; }
        public string Colors { get; set; }
        public string RuleSql { get; set; }

        protected override IValidator GetValidator()
        {
            return new UpdateDataSourceConfigMapperValidator();
        }

        class UpdateDataSourceConfigMapperValidator : AbstractValidator<UpdateDataSourceConfigMapper>
        {
            public UpdateDataSourceConfigMapperValidator()
            {
                RuleFor(d => d.DataConfigId).NotNull().WithMessage("数据源配置Id不能为空");
            }
        }
    }

    public class DataSrcDeleteMapper : BaseEntity
    {
        public Guid DataSrcId { get; set; }
        protected override IValidator GetValidator()
        {
            return new DataSrcDeleteMapperValidator();
        }
        class DataSrcDeleteMapperValidator : AbstractValidator<DataSrcDeleteMapper>
        {
            public DataSrcDeleteMapperValidator()
            {
                RuleFor(d => d.DataSrcId).NotNull().WithMessage("数据源Id不能为空");
            }
        }
    }




    public class DynamicDataSrcMapper : BaseEntity
    {
        public string SourceId { get; set; }
        public string KeyWord { get; set; }

        public string SqlWhere { get; set; }
        public List<Dictionary<string, object>> QueryData { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        protected override IValidator GetValidator()
        {
            return new DynamicDataSrcMapperValidator();
        }

        class DynamicDataSrcMapperValidator : AbstractValidator<DynamicDataSrcMapper>
        {
            public DynamicDataSrcMapperValidator()
            {
                RuleFor(d => d.SourceId).NotNull().WithMessage("数据源配置Id不能为空");
            }
        }
    }

    public class DynamicDataSrcQueryDataMapper
    {
        public string FieldName { get; set; }

        public int IsLike { get; set; }
    }

    public class DictionaryTypeMapper : BaseEntity
    {
        public string DicTypeId { get; set; }
        public string DicTypeName { get; set; }
        public string DicRemark { get; set; }
        public string FieldConfig { get; set; }
        public int RecStatus { get; set; }
        public int? RelateDicTypeId { get; set; }
        public string RecOrder { get; set; }
        /// <summary>
        /// 0:使用自定义 1:使用全局
        /// </summary>
        public int IsConfig { get; set; }
        protected override IValidator GetValidator()
        {
            return new DictionaryEntityMapperValidator();
        }

        class DictionaryEntityMapperValidator : AbstractValidator<DictionaryTypeMapper>
        {
            public DictionaryEntityMapperValidator()
            {
                RuleFor(d => d.DicTypeName).NotNull().WithMessage("字典Key不能为空");
            }
        }
    }

    public class DictionaryMapper : BaseEntity
    {
        public string DicId { get; set; }

        public int DicTypeId { get; set; }

        public string DataId { get; set; }

        public string DataValue { get; set; }

        protected override IValidator GetValidator()
        {
            return new DictionaryMapperValidator();
        }

        class DictionaryMapperValidator : AbstractValidator<DictionaryMapper>
        {
            public DictionaryMapperValidator()
            {
                RuleFor(d => d.DataValue).NotEmpty().WithMessage("字段选项值不能为空");
            }
        }
    }

}
