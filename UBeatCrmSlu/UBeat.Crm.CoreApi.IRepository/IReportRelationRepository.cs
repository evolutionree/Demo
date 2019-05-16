using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.ReportRelation;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IReportRelationRepository
    {
        OperateResult AddReportRelation(AddReportRelationMapper add, DbTransaction dbTran, int userId);


        OperateResult UpdateReportRelation(EditReportRelationMapper edit, DbTransaction dbTran, int userId);


        OperateResult DeleteReportRelation(DeleteReportRelationMapper delete, DbTransaction dbTran, int userId);

        PageDataInfo<Dictionary<string, object>> GetReportRelationListData(QueryReportRelationMapper mapper, DbTransaction dbTran, int userId);


        OperateResult AddReportRelDetail(AddReportRelDetailMapper add, DbTransaction dbTran, int userId);


        OperateResult UpdateReportRelDetail(EditReportRelDetailMapper edit, DbTransaction dbTran, int userId);


        OperateResult DeleteReportRelDetail(DeleteReportRelDetailMapper delete, DbTransaction dbTran, int userId);

        PageDataInfo<Dictionary<string, object>> GetReportRelDetailListData(QueryReportRelDetailMapper mapper, DbTransaction dbTran, int userId);

        List<EditReportRelDetailMapper> GetReportRelDetail(QueryReportRelDetailMapper mapper, DbTransaction dbTran, int userId);
    }
}
