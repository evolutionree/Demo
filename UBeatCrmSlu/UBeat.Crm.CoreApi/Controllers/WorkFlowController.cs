﻿using Microsoft.AspNetCore.Mvc;
using System;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.WorkFlow;
using UBeat.Crm.CoreApi.Services.Services;

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class WorkFlowController : BaseController
    {
        private readonly WorkFlowServices _workFlowServices;

        public WorkFlowController(WorkFlowServices workFlowServices) : base(workFlowServices)
        {
            _workFlowServices = workFlowServices;
        }

        /// <summary>
        /// 获取流程审批详情
        /// </summary>
        /// <param name="detailModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("casedetail")]
        public OutputResult<object> CaseDetail([FromBody] CaseDetailModel detailModel = null)
        {
            if (detailModel == null) return ResponseError<object>("参数格式错误");
            return _workFlowServices.CaseDetail(detailModel, UserId);
        }


        /// <summary>
        /// 发起审批接口
        /// </summary>
        [HttpPost]
        [Route("addcase")]
        public OutputResult<object> AddCase([FromBody] WorkFlowAddCaseModel caseModel = null)
        {
            if (caseModel == null) return ResponseError<object>("参数格式错误");
            WriteOperateLog("提交审批数据", caseModel);
            return _workFlowServices.AddCase(caseModel, UserId);
        }

        //获取下一个节点，以及节点选人数据
        [HttpPost]
        [Route("nextnodedata")]
        public OutputResult<object> NextNodeData_old([FromBody] WorkFlowNextNodeModel caseModel = null)
        {
            if (caseModel == null) return ResponseError<object>("参数格式错误");
            return _workFlowServices.NextNodeData(caseModel, UserId);
        }
        //获取下一个节点，以及节点选人数据
        [HttpPost]
        [Route("getnextnode")]
        public OutputResult<object> GetNextNodeData([FromBody] WorkFlowNextNodeModel caseModel = null)
        {
            if (caseModel == null) return ResponseError<object>("参数格式错误");
            return _workFlowServices.GetNextNodeData(caseModel, UserId);
        }

        //选人之后，提交审批明细数据
        [HttpPost]
        [Route("addcaseitem")]
        public OutputResult<object> AddCaseItem([FromBody] WorkFlowAddCaseItemModel caseItemModel = null)
        {
            if (caseItemModel == null) return ResponseError<object>("参数格式错误");
            WriteOperateLog("提交审批步骤数据", caseItemModel);
            return _workFlowServices.AddCaseItem(caseItemModel, UserId);
        }
        //审批流程
        [HttpPost]
        [Route("auditcaseitem")]
        public OutputResult<object> AuditCaseItem([FromBody] WorkFlowAuditCaseItemModel caseItemModel = null)
        {
            if (caseItemModel == null) return ResponseError<object>("参数格式错误");
            WriteOperateLog("提交审核意见", caseItemModel);
            return _workFlowServices.AuditCaseItem(caseItemModel, UserId);
        }

        //提交审批预处理(新审批接口)
        [HttpPost]
        [Route("submitpreaudit")]
        public OutputResult<object> SubmitPretreatAudit([FromBody] WorkFlowAuditCaseItemModel caseItemModel = null)
        {
            if (caseItemModel == null) return ResponseError<object>("参数格式错误");
            return _workFlowServices.SubmitPretreatAudit(caseItemModel, LoginUser);
        }

        //审批流程(新审批接口)
        [HttpPost]
        [Route("submitaudit")]
        public OutputResult<object> SubmitWorkFlowAudit([FromBody] WorkFlowAuditCaseItemModel caseItemModel = null)
        {
            if (caseItemModel == null) return ResponseError<object>("参数格式错误");
            WriteOperateLog("提交流程审批", caseItemModel);
            return _workFlowServices.SubmitWorkFlowAudit(caseItemModel, LoginUser);
        }


        //选人之后，提交审批明细数据
        [HttpPost]
        [Route("caseitemlist")]
        public OutputResult<object> CaseItemList([FromBody] WorkFlowAuditCaseItemListModel listModel = null)
        {
            if (listModel == null) return ResponseError<object>("参数格式错误");
            return _workFlowServices.CaseItemList(listModel, UserId);
        }

        [HttpPost]
        [Route("nodelinesinfo")]
        public OutputResult<object> NodeLinesInfo([FromBody] WorkFlowNodeLinesInfoModel nodeLineModel = null)
        {
            if (nodeLineModel == null) return ResponseError<object>("参数格式错误");
            return _workFlowServices.NodeLineInfo(nodeLineModel, UserId);
        }

        //
        [HttpPost]
        [Route("getnodelinesinfo")]
        public OutputResult<object> GetNodeLinesInfo([FromBody] WorkFlowNodeLinesInfoModel nodeLineModel = null)
        {
            if (nodeLineModel == null) return ResponseError<object>("参数格式错误");
            return _workFlowServices.GetNodeLinesInfo(nodeLineModel, UserId);
        }

        #region --旧代码--
        [HttpPost]
        [Route("nodelinesconfig")]
        public OutputResult<object> NodeLinesConfig([FromBody] WorkFlowNodeLinesConfigModel configModel = null)
        {
            if (configModel == null) return ResponseError<object>("参数格式错误");
            WriteOperateLog("修改审批节点配置", configModel);
            return _workFlowServices.NodeLinesConfig(configModel, UserId);
        } 
        #endregion

        [HttpPost]
        [Route("savenodesconfig")]
        public OutputResult<object> SaveNodeLinesConfig([FromBody] WorkFlowNodeLinesConfigModel configModel = null)
        {
            if (configModel == null) return ResponseError<object>("参数格式错误");
            WriteOperateLog("修改审批节点配置", configModel);
            return _workFlowServices.SaveNodeLinesConfig(configModel, UserId);
        }

        [HttpPost]
        [Route("flowlist")]
        public OutputResult<object> FlowList([FromBody] WorkFlowListModel listModel = null)
        {
            if (listModel == null) return ResponseError<object>("参数格式错误");
            return _workFlowServices.FlowList(listModel, UserId);
        }

        [HttpPost]
        [Route("detail")]
        public OutputResult<object> Detail([FromBody] WorkFlowDetailModel detailModel = null)
        {
            if (detailModel == null) return ResponseError<object>("参数格式错误");
            return _workFlowServices.Detail(detailModel, UserId);
        }

        [HttpPost]
        [Route("add")]
        public OutputResult<object> AddFlow([FromBody] WorkFlowAddModel flowModel = null)
        {
            if (flowModel == null) return ResponseError<object>("参数格式错误");
            WriteOperateLog("新增审批流程", flowModel);
            return _workFlowServices.AddFlow(flowModel, UserId);
        }

        [HttpPost]
        [Route("update")]
        public OutputResult<object> UpdateFlow([FromBody] WorkFlowUpdateModel flowModel = null)
        {
            if (flowModel == null) return ResponseError<object>("参数格式错误");
            WriteOperateLog("修改流程信息", flowModel);
            return _workFlowServices.UpdateFlow(flowModel, UserId);
        }

        [HttpPost]
        [Route("delete")]
        public OutputResult<object> DeleteFlow([FromBody] WorkFLowDeleteModel flowModel = null)
        {
            if (flowModel == null) return ResponseError<object>("参数格式错误");
            WriteOperateLog("停用流程信息", flowModel);
            return _workFlowServices.DeleteFlow(flowModel, UserId);
        }
        [HttpPost]
        [Route("undelete")]
        public OutputResult<object> UnDeleteFlow([FromBody] WorkFLowDeleteModel flowModel = null) {
            if (flowModel == null) return ResponseError<object>("参数格式错误");
            WriteOperateLog("启用流程信息", flowModel);
            return _workFlowServices.UnDeleteFlow(flowModel, UserId);

        }


        /// <summary>
        /// 发起多个审批
        /// </summary>
        [HttpPost]
        [Route("addmultiplecase")]
        public OutputResult<object> AddMultipleCase([FromBody] WorkFlowAddMultipleCaseModel caseModel = null)
        {
            if (caseModel == null) return ResponseError<object>("参数格式错误");
            WriteOperateLog("提交审批数据", caseModel);
            return _workFlowServices.AddMultipleCase(caseModel, UserId);
        }


        /// <summary>
        /// 发起多个审批
        /// </summary>
        [HttpPost]
        [Route("addmultiplecaseitem")]
        public OutputResult<object> AddMultipleCaseItem([FromBody] WorkFlowAddMultipleCaseItemModel caseModel = null)
        {
            if (caseModel == null) return ResponseError<object>("参数格式错误");
            WriteOperateLog("提交审批数据", caseModel);
            return _workFlowServices.AddMultipleCaseItem(caseModel, UserId);
        }

        /// <summary>
        /// 保存工作流Rule
        /// </summary>
        /// <param name="paramInfo"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("saverule")]
        public OutputResult<object> SaveRule([FromBody]WorkFlowRuleSaveParamInfo paramInfo) {
            if (paramInfo == null) return ResponseError<object>("参数异常");
            return this._workFlowServices.SaveWorkflowRule(paramInfo, UserId);
        }
        [HttpPost]
        [Route("getrule")]
        public OutputResult<object> GetRule([FromBody] WorkFlowRuleQueryParamInfo paramInfo ) {
            if (paramInfo == null || paramInfo.FlowId == null || paramInfo.FlowId == Guid.Empty) {
                return ResponseError<object>("参数异常");
            }
            return _workFlowServices.GetRules(paramInfo, UserId);
        }
    }
}
 