using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using UBeat.Crm.CoreApi.DomainModel.WorkFlow;

namespace UBeat.Crm.CoreApi.Services.Models.WorkFlow
{
    public class WorkFlowProfile:Profile
    {
        public WorkFlowProfile()
        {
            CreateMap<WorkFlowAddCaseModel, WorkFlowAddCaseMapper>();
            CreateMap<WorkFlowAddCaseItemModel, WorkFlowAddCaseItemMapper>();
            CreateMap<WorkFlowAuditCaseItemModel, WorkFlowAuditCaseItemMapper>();
            CreateMap<WorkFlowNodeLinesConfigModel, WorkFlowNodeLinesConfigMapper>();
            CreateMap<WorkFlowNodeModel, WorkFlowNodeMapper>();
            CreateMap<WorkFlowLineModel, WorkFlowLineMapper>();
            CreateMap<WorkFlowAddModel, WorkFlowAddMapper>();
            CreateMap<WorkFlowUpdateModel, WorkFlowUpdateMapper>();
            CreateMap<CaseItemFileAttachModel, CaseItemFileAttach>();
            CreateMap<CaseItemTransfer, CaseItemTransferMapper>();
            CreateMap<WorkFlowRepeatApproveModel, WorkFlowRepeatApprove>();
            CreateMap<InformerRuleModel, InformerRuleMapper>();
            CreateMap<InformerModel, InformerMapper>();
            CreateMap<RejectToOrginalNodeModel, RejectToOrginalNode>();
        }
    }
}
