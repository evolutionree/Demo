using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public OutputResult<object> AddCase([FromBody] WorkFlowCaseAddModel caseModel = null)
        {
            if (caseModel == null) return ResponseError<object>("参数格式错误");
            Guid g;
            if (!string.IsNullOrEmpty(caseModel.CacheId) && !Guid.TryParse(caseModel.CacheId, out g))
                return ResponseError<object>("CacheId格式错误");
            WriteOperateLog("发起审批数据", caseModel);
            return _workFlowServices.AddWorkflowCase(caseModel, LoginUser);
        }

        /// <summary>
        /// 预发起审批接口
        /// </summary>
        [HttpPost]
        [Route("preaddcase")]
        public OutputResult<object> PreAddCase([FromBody] WorkFlowCaseAddModel caseModel = null)
        {
            if (caseModel == null) return ResponseError<object>("参数格式错误");
            return _workFlowServices.PreAddWorkflowCase(caseModel, LoginUser);
        }
		 
        //获取下一个节点，以及节点选人数据
        [HttpPost]
        [Route("getnextnode")]
        public OutputResult<object> GetNextNodeData([FromBody] WorkFlowNextNodeModel caseModel = null)
        {
            if (caseModel == null) return ResponseError<object>("参数格式错误");
            return _workFlowServices.GetNextNodeData(caseModel, UserId);
        }
        
        //提交审批预处理(新审批接口)
        [HttpPost]
        [Route("submitpreaudit")]
        public OutputResult<object> SubmitPretreatAudit([FromBody] WorkFlowAuditCaseItemModel caseItemModel = null)
        {
            if (caseItemModel == null) return ResponseError<object>("参数格式错误");
            return _workFlowServices.SubmitPretreatAudit(caseItemModel, LoginUser );
        }

        //审批流程(新审批接口)
        [HttpPost]
        [Route("submitaudit")]
        public OutputResult<object> SubmitWorkFlowAudit([FromBody] WorkFlowAuditCaseItemModel caseItemModel = null)
        {
            if (caseItemModel == null) return ResponseError<object>("参数格式错误");
            WriteOperateLog("提交流程审批", caseItemModel);
            return _workFlowServices.SubmitWorkFlowAudit(caseItemModel, LoginUser,isNeedToSendMsg:1);
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
		 
        [HttpPost]
        [Route("getnodelinesinfo")]
        public OutputResult<object> GetNodeLinesInfo([FromBody] WorkFlowNodeLinesInfoModel nodeLineModel = null)
        {
            if (nodeLineModel == null) return ResponseError<object>("参数格式错误");
            return _workFlowServices.GetNodeLinesInfo(nodeLineModel, UserId);
        }
		 
        [HttpPost]
        [Route("savenodesconfig")]
        public OutputResult<object> SaveNodeLinesConfig([FromBody] WorkFlowNodeLinesConfigModel configModel = null)
        {
            if (configModel == null) return ResponseError<object>("参数格式错误");
            WriteOperateLog("修改审批节点配置", configModel);
            return _workFlowServices.SaveNodeLinesConfig(configModel, UserId);
        }
        [HttpPost]
        [Route("savefreeflowevent")]
        public OutputResult<object> SaveFreeFlowNodeEvents([FromBody] FreeFlowEventModel configModel = null)
        {
            if (configModel == null) return ResponseError<object>("参数格式错误");
            WriteOperateLog("修改自由流程配置", configModel);
            return _workFlowServices.SaveFreeFlowNodeEvents(configModel, UserId);
        }

        [HttpPost]
        [Route("getfreeflowevent")]
        public OutputResult<object> GetFreeFlowNodeEvents([FromBody] GetFreeFlowEventModel configModel = null)
        {
            if (configModel == null) return ResponseError<object>("参数格式错误");
            WriteOperateLog("修改自由流程配置", configModel);
            return _workFlowServices.GetFreeFlowNodeEvents(configModel, UserId);
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
		 
        ///// <summary>
        ///// 发起多个审批
        ///// </summary>
        //[HttpPost]
        //[Route("addmultiplecase")]
        //public OutputResult<object> AddMultipleCase([FromBody] WorkFlowAddMultipleCaseModel caseModel = null)
        //{
        //    if (caseModel == null) return ResponseError<object>("参数格式错误");
        //    WriteOperateLog("提交审批数据", caseModel);
        //    return _workFlowServices.AddMultipleCase(caseModel, UserId);
        //}


        ///// <summary>
        ///// 发起多个审批
        ///// </summary>
        //[HttpPost]
        //[Route("addmultiplecaseitem")]
        //public OutputResult<object> AddMultipleCaseItem([FromBody] WorkFlowAddMultipleCaseItemModel caseModel = null)
        //{
        //    if (caseModel == null) return ResponseError<object>("参数格式错误");
        //    WriteOperateLog("提交审批数据", caseModel);
        //    return _workFlowServices.AddMultipleCaseItem(caseModel, UserId);
        //}

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
        #region 处理工作流主题问题
        [HttpPost("titlefields")]
        public OutputResult<object> GetTitleFieldList([FromBody] WorkFlowDetailModel paramInfo) {
            if (paramInfo == null || paramInfo.FlowId == null || paramInfo.FlowId == Guid.Empty) {
                return ResponseError<object>("参数异常");
            }
            return _workFlowServices.GetTitleFieldList(paramInfo, UserId);
        }
        [HttpPost("savetitleconfig")]
        [AllowAnonymous]
        public OutputResult<object> SaveTitleConfig([FromBody] WorkFlowTitleConfigModel paramInfo)
        {
            if (paramInfo == null || paramInfo.FlowId == null || paramInfo.FlowId == Guid.Empty)
            {
                return ResponseError<object>("参数异常");
            }
            try
            {

                return _workFlowServices.SaveTitleConfig(paramInfo, UserId);
            }
            catch (Exception ex) {
                return ResponseError<object>(ex.Message);
            }
        }


        #endregion
        #region 获取简单实体的对应的工作流定义id
        [HttpPost("workflowidbyentityid")]
        public OutputResult<object> GetWorkflowIdByEntityId([FromBody] WorkflowIdByEntityIdParamInfo paramInfo) {
            try
            {

                if (paramInfo == null) return ResponseError<object>("参数异常");
                if (paramInfo.EntityId == null || paramInfo.EntityId == Guid.Empty) return ResponseError<object>("参数异常");
                return new OutputResult<object>(this._workFlowServices.GetWorkFlowIdByEntityId(paramInfo.EntityId, UserId));
            }
            catch (Exception ex) {
                return ResponseError<object>(ex.Message);
            }
        }
		#endregion

		#region 流程超期检查：审批设置了超时自动中止之后，系统根据设置的超时时间自动中止审批
		[HttpPost]
		[Route("autostopworkflow")]
		public OutputResult<object> AutoStopWorkflow()
		{ 
			var res = true;
			_workFlowServices.AutoTerminateWorkflowCases();

			return new OutputResult<object>(res);
		}
        #endregion


        [HttpPost]
        [Route("transfer")]
        public OutputResult<object> TransferToOtherUser([FromBody]CaseItemTransfer transfer)
        {
            if (transfer == null) return ResponseError<object>("参数异常");
            return _workFlowServices.TransferToOther(transfer, UserId, ControllerContext.HttpContext.RequestServices);
        }

        [HttpPost]
        [Route("repeatapprove")]
        public OutputResult<object> NeedToRepeatApprove([FromBody]WorkFlowRepeatApproveModel workFlow)
        {
            if (workFlow == null) return ResponseError<object>("参数异常");
            return _workFlowServices.NeedToRepeatApprove(workFlow, UserId);
        }

        [HttpPost]
        [Route("checkrepeatapprove")]
        public OutputResult<object> CheckIfExistNeedToRepeatApprove([FromBody]WorkFlowRepeatApproveModel workFlow)
        {
            if (workFlow == null) return ResponseError<object>("参数异常");
            return _workFlowServices.CheckIfExistNeedToRepeatApprove(workFlow, UserId);
        }
        [HttpPost]
        [Route("saveinformer")]
        public OutputResult<object> SaveWorkflowInformer([FromBody]InformerModel informerModel)
        {
            if (informerModel == null) return ResponseError<object>("参数异常");
            if (informerModel.InformerRules.Count == 0) return ResponseError<object>("参数异常");
            if (informerModel.InformerRules.Exists(t => t.Rule == null)) return ResponseError<object>("规则不能为空");
            return _workFlowServices.SaveWorkflowInformer(informerModel, UserId);
        }
        [HttpPost]
        [Route("getinformerrule")]
        public OutputResult<object> GetWorkflowInformerRules([FromBody]InformerRuleModel informerRuleModel)
        {
            if (informerRuleModel == null) return ResponseError<object>("参数异常");
            return _workFlowServices.GetWorkflowInformerRules(informerRuleModel, UserId);
        }
        [HttpPost]
        [Route("jumptorejectnode")]
        public OutputResult<object> RejectToOrginalNode([FromBody]RejectToOrginalNodeModel rejectToOrginalNodeModel)
        {
            if (rejectToOrginalNodeModel == null) return ResponseError<object>("参数异常");
            return _workFlowServices.RejectToOrginalNode(rejectToOrginalNodeModel, UserId);
        }
        //是否可以撤回
        [HttpPost]
        [Route("canwithdraw")]
        public OutputResult<object> CanWithDraw([FromBody]WithDrawRequestModel withDraw)
        {
            if (withDraw == null) return ResponseError<object>("参数异常");
            return _workFlowServices.CanWorkFlowWithDraw(withDraw, UserId);
        }

        //撤回
        [HttpPost]
        [Route("withdraw")]
        public OutputResult<object> WithDraw([FromBody]WithDrawRequestModel withDraw)
        {
            if (withDraw == null) return ResponseError<object>("参数异常");
            return _workFlowServices.WorkFlowWithDraw(withDraw, UserId);
        }

    }
}
 