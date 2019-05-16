using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace UBeat.Crm.CoreApi.DomainModel.StatisticsSetting
{
    public class AddStatisticsSettingMapper : BaseEntity
    {
        // public Guid AnaFuncId { get; set; }
        public String AnaFuncName { get; set; }
        public int MoreFlag { get; set; }
        public String CountFunc { get; set; }
        public String MoreFunc { get; set; }
        public Guid EntityId { get; set; }
        public int AllowInto { get; set; }
        public string AnaFuncName_Lang { get; set; }
        protected override IValidator GetValidator()
        {
            return new AddStatisticsSettingMapperVal();
        }
        class AddStatisticsSettingMapperVal : AbstractValidator<AddStatisticsSettingMapper>
        {
            public AddStatisticsSettingMapperVal()
            {
            }
        }
    }

    public class EditStatisticsSettingMapper : BaseEntity
    {
        public Guid AnaFuncId { get; set; }
        public String AnaFuncName { get; set; }
        public int MoreFlag { get; set; }
        public String CountFunc { get; set; }
        public String MoreFunc { get; set; }
        public Guid EntityId { get; set; }
        public int AllowInto { get; set; }
        public string AnaFuncName_Lang { get; set; }
        protected override IValidator GetValidator()
        {
            return new EditStatisticsSettingMapperVal();
        }
        class EditStatisticsSettingMapperVal : AbstractValidator<EditStatisticsSettingMapper>
        {
            public EditStatisticsSettingMapperVal()
            {
            }
        }
    }


    public class DeleteStatisticsSettingMapper : BaseEntity
    {
        public List<Guid> AnaFuncIds { get; set; }
        public int RecStatus { get; set; }
        protected override IValidator GetValidator()
        {
            return new DeleteStatisticsSettingMapperVal();
        }
        class DeleteStatisticsSettingMapperVal : AbstractValidator<DeleteStatisticsSettingMapper>
        {
            public DeleteStatisticsSettingMapperVal()
            {
            }
        }
    }

    public class QueryStatisticsSettingMapper
    {
        public String AnaFuncName { get; set; }
    }

    public class QueryStatisticsMapper
    {
        public String AnaFuncName { get; set; }

        public String GroupName { get; set; }
    }


    public class EditStatisticsGroupMapper
    {
        public String NewGroupName { get; set; }

        public String GroupName { get; set; }
        public String NewGroupName_Lang { get; set; }
    }

    public class SaveStatisticsGroupMapper
    {
        public SaveStatisticsGroupMapper()
        {
            Data = new List<SaveStatisticsGroupSumMapper>();
        }
        public List<SaveStatisticsGroupSumMapper> Data { get; set; }

        public int IsDel { get; set; }
    }
    public class SaveStatisticsGroupSumMapper
    {
        public Guid? AnafuncId { get; set; }

        public int RecOrder { get; set; }

        public string GroupName_Lang { get; set; }
        public String GroupName { get; set; }
    }
}