using AutoMapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.ReportRelation;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.ReportRelation;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class ReportRelationServices : BasicBaseServices
    {

        private readonly IReportRelationRepository _reportRelationRepository;
        private readonly IMapper _mapper;


        public ReportRelationServices(IMapper mapper, IReportRelationRepository reportRelationRepository, IConfigurationRoot config)
        {
            _reportRelationRepository = reportRelationRepository;
            _mapper = mapper;
        }

        public OutputResult<object> GetReportRelationListData(QueryReportRelationModel model, int userId)
        {
            var mapper = _mapper.Map<QueryReportRelationModel, QueryReportRelationMapper>(model);

            return ExcuteAction((transaction, arg, userData) =>
            {
                var scripts = _reportRelationRepository.GetReportRelationListData(mapper, transaction, userId);
                return new OutputResult<object>(scripts);
            }, model, userId);
        }
        public OutputResult<object> AddReportRelation(AddReportRelationModel add, int userId)
        {
            var mapper = _mapper.Map<AddReportRelationModel, AddReportRelationMapper>(add);

            return ExcuteAction((transaction, arg, userData) =>
            {
                var data = _reportRelationRepository.AddReportRelation(mapper, transaction, userId);
                return HandleResult(data);
            }, add, userId, isolationLevel: IsolationLevel.ReadUncommitted);
        }
        public OutputResult<object> UpdateReportRelation(EditReportRelationModel edit, int userId)
        {
            var mapper = _mapper.Map<EditReportRelationModel, EditReportRelationMapper>(edit);

            return ExcuteAction((transaction, arg, userData) =>
            {
                var result = _reportRelationRepository.UpdateReportRelation(mapper, transaction, userId);
                return HandleResult(result);
            }, edit, userId, null, IsolationLevel.ReadUncommitted);
        }
        public OutputResult<object> GetReportRelDetailListData(QueryReportRelDetailModel model, int userId)
        {
            var mapper = _mapper.Map<QueryReportRelDetailModel, QueryReportRelDetailMapper>(model);

            return ExcuteAction((transaction, arg, userData) =>
            {
                var scripts = _reportRelationRepository.GetReportRelDetailListData(mapper, transaction, userId);
                return new OutputResult<object>(scripts);
            }, model, userId);
        }

        public OutputResult<object> AddReportRelDetail(AddReportRelDetailModel add, int userId)
        {
            var mapper = _mapper.Map<AddReportRelDetailModel, AddReportRelDetailMapper>(add);

            return ExcuteAction((transaction, arg, userData) =>
            {
                var data = _reportRelationRepository.AddReportRelDetail(mapper, transaction, userId);
                return HandleResult(data);
            }, add, userId, isolationLevel: IsolationLevel.ReadUncommitted);
        }
        public OutputResult<object> UpdateReportRelDetail(EditReportRelDetailModel edit, int userId)
        {
            var mapper = _mapper.Map<EditReportRelDetailModel, EditReportRelDetailMapper>(edit);

            return ExcuteAction((transaction, arg, userData) =>
            {
                var result = _reportRelationRepository.UpdateReportRelDetail(mapper, transaction, userId);
                return HandleResult(result);
            }, edit, userId, null, IsolationLevel.ReadUncommitted);
        }
        public OutputResult<object> DeleteReportRelDetail(DeleteReportRelDetailModel add, int userId)
        {
            var mapper = _mapper.Map<DeleteReportRelDetailModel, DeleteReportRelDetailMapper>(add);

            return ExcuteAction((transaction, arg, userData) =>
            {
                var data = _reportRelationRepository.DeleteReportRelDetail(mapper, transaction, userId);
                return HandleResult(data);
            }, add, userId, isolationLevel: IsolationLevel.ReadUncommitted);
        }

        public OutputResult<object> DeleteReportRelation(DeleteReportRelationModel add, int userId)
        {
            var mapper = _mapper.Map<DeleteReportRelationModel, DeleteReportRelationMapper>(add);

            return ExcuteAction((transaction, arg, userData) =>
            {
                var data = _reportRelationRepository.DeleteReportRelation(mapper, transaction, userId);
                return HandleResult(data);
            }, add, userId, isolationLevel: IsolationLevel.ReadUncommitted);
        }
    }
}
