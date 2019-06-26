using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace UBeat.Crm.CoreApi.DomainModel.ReportRelation
{
    public class AddReportRelationMapper : BaseEntity
    {
        // public Guid AnaFuncId { get; set; }
        public String ReportRelationName { get; set; }
        public String ReportreMark { get; set; }
        protected override IValidator GetValidator()
        {
            return new ReportRelationMapperVal();
        }
        class ReportRelationMapperVal : AbstractValidator<AddReportRelationMapper>
        {
            public ReportRelationMapperVal()
            {
            }
        }
    }

    public class EditReportRelationMapper : BaseEntity
    {
        public Guid ReportRelationId { get; set; }
        public String ReportRelationName { get; set; }
        public String ReportreMark { get; set; }
        protected override IValidator GetValidator()
        {
            return new EditReportRelationMapperVal();
        }
        class EditReportRelationMapperVal : AbstractValidator<EditReportRelationMapper>
        {
            public EditReportRelationMapperVal()
            {
            }
        }
    }


    public class DeleteReportRelationMapper : BaseEntity
    {
        public List<Guid> ReportRelationIds { get; set; }
        public int RecStatus { get; set; }
        protected override IValidator GetValidator()
        {
            return new DeleteReportRelationMapperVal();
        }
        class DeleteReportRelationMapperVal : AbstractValidator<DeleteReportRelationMapper>
        {
            public DeleteReportRelationMapperVal()
            {
            }
        }
    }

    public class AddReportRelDetailMapper : BaseEntity
    {
        public Guid ReportRelationId { get; set; }
        public String ReportUser { get; set; }
        public String ReportLeader { get; set; }
        protected override IValidator GetValidator()
        {
            return new ReportRelDetailationMapperVal();
        }
        class ReportRelDetailationMapperVal : AbstractValidator<AddReportRelDetailMapper>
        {
            public ReportRelDetailationMapperVal()
            {
            }
        }
    }

    public class EditReportRelDetailMapper : BaseEntity
    {
        public Guid ReportRelDetailId { get; set; }
        public Guid ReportRelationId { get; set; }
        public String ReportUser { get; set; }
        public String ReportLeader { get; set; }
        protected override IValidator GetValidator()
        {
            return new EditReportRelDetailationMapperVal();
        }
        class EditReportRelDetailationMapperVal : AbstractValidator<EditReportRelDetailMapper>
        {
            public EditReportRelDetailationMapperVal()
            {
            }
        }
    }


    public class DeleteReportRelDetailMapper : BaseEntity
    {      public List<Guid> ReportRelationIds { get; set; }
        public List<Guid> ReportRelDetailIds { get; set; }
        public int RecStatus { get; set; }
        protected override IValidator GetValidator()
        {
            return new DeleteReportRelDetailationMapperVal();
        }
        class DeleteReportRelDetailationMapperVal : AbstractValidator<DeleteReportRelDetailMapper>
        {
            public DeleteReportRelDetailationMapperVal()
            {
            }
        }
    }

    public class QueryReportRelationMapper
    {
        public String Name { get; set; }

        public int? UserId { get; set; }

        public Guid? ReportRelationId { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }
        public string SearchOrder { get; set; }
        public Dictionary<string, object> ColumnFilter { get; set; }
    }

    public class QueryReportRelDetailMapper
    {
        public String Name { get; set; }

        public int? UserId { get; set; }
        public Guid? ReportRelationId { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }
        public string SearchOrder { get; set; }
        public Dictionary<string, object> ColumnFilter { get; set; }
    }


}