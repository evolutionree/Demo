
using AutoMapper;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.Account;
using UBeat.Crm.CoreApi.DomainModel.ReportRelation;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.Excels;
using UBeat.Crm.CoreApi.Services.Models.ReportRelation;
using UBeat.Crm.CoreApi.Services.Utility.ExcelUtility;
using System.Linq;
using System.Linq.Expressions;
using UBeat.Crm.CoreApi.DomainModel;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class ReportRelationServices : BasicBaseServices
    {

        private readonly IReportRelationRepository _reportRelationRepository;
        private readonly IAccountRepository _iAccountRepository;
        private readonly IMapper _mapper;


        public ReportRelationServices(IMapper mapper, IReportRelationRepository reportRelationRepository, IConfigurationRoot config, IAccountRepository iAccountRepository)
        {
            _reportRelationRepository = reportRelationRepository;
            _iAccountRepository = iAccountRepository;
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

        public OutputResult<object> ImportReportRelation(System.IO.Stream r, bool isConvertImport, int userId)
        {
            var sheetDefine = new List<SheetDefine>();
            using (var db = GetDbConnect())
            {
                db.Open();
                var tran = db.BeginTransaction();
                try
                {
                    List<Dictionary<string, object>> realDatas = new List<Dictionary<string, object>>();
                    var users = _iAccountRepository.GetUserList(new DomainModel.PageParam
                    {
                        PageIndex = 1,
                        PageSize = int.MaxValue
                    }, new AccountUserQueryMapper()
                    {
                        DeptId = Guid.Empty,
                        RecStatus = 1,
                        UserName = "",
                        UserPhone = ""
                    }, userId);
                    var reportRelationDetail = _reportRelationRepository.GetReportRelDetailListData(new QueryReportRelDetailMapper
                    {
                        PageIndex = 1,
                        PageSize = int.MaxValue,
                        ColumnFilter = new Dictionary<string, object>()
                    }, tran, userId);
                    List<string> errorTips = new List<string>();
                    var data = OXSExcelReader.ReadExcelFirstSheet(r);
                    data.RemoveAt(0);
                    List<Guid> containKeys = new List<Guid>();
                    List<Guid> reportRelationIds;
                    int index = 0;
                    foreach (var curRow in data)
                    {
                        if (curRow["0"] != null && string.IsNullOrEmpty(curRow["0"].ToString()))
                            continue;
                        if (curRow["1"] != null && string.IsNullOrEmpty(curRow["1"].ToString()))
                            continue;
                        if (curRow["2"] != null && string.IsNullOrEmpty(curRow["2"].ToString()))
                            continue;
                        int totalRowCount = data.Count;
                        Guid reportRelationId = Guid.Parse(reportRelationDetail.DataList.FirstOrDefault(t => t["reportrelationname"].ToString() == curRow["0"].ToString())["reportrelationid"].ToString());
                        if (isConvertImport)//覆盖
                        {
                            if (!containKeys.Contains(reportRelationId))
                            {
                                reportRelationIds = new List<Guid>();
                                containKeys.Add(reportRelationId);
                                reportRelationIds.Add(reportRelationId);
                                _reportRelationRepository.DeleteReportRelDetailData(new DeleteReportRelDetailMapper
                                {
                                    ReportRelationIds = reportRelationIds,
                                    RecStatus = 0
                                }, tran, userId);
                            }
                        }
                        List<string> result = new List<string>();
                        List<string> result1 = new List<string>();
                        curRow["1"].ToString().Split(",").ToList().ForEach(t =>
                        {
                            var dic = users["PageData"].FirstOrDefault(t1 => t1["username"].ToString() == t);
                            result.Add(dic["userid"].ToString());
                        });
                        curRow["2"].ToString().Split(",").ToList().ForEach(t =>
                        {
                            var dic = users["PageData"].FirstOrDefault(t1 => t1["username"].ToString() == t);
                            result1.Add(dic["userid"].ToString());
                        });

                        var operateResult = _reportRelationRepository.AddReportRelDetail(new AddReportRelDetailMapper
                        {
                            ReportRelationId = reportRelationId,
                            ReportUser = string.Join(",", result),
                            ReportLeader = string.Join(",", result1),
                        }, tran, userId);
                        if (operateResult.Flag == 0)
                        {
                            errorTips.Add("第" + (index + 1).ToString() + "行，" + operateResult.Msg);
                        }
                        index++;
                    }

                    if (errorTips.Count > 0)
                    {
                        tran.Rollback();
                        return HandleResult(new OperateResult
                        {
                            Msg = string.Join(",", errorTips)
                        });
                    }

                    tran.Commit();
                    return HandleResult(new OperateResult
                    {
                        Flag = 1,
                        Msg = "导入成功"
                    });
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw new Exception("汇报关系导入异常");
                }
            }
        }
    }
}
