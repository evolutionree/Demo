using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using UBeat.Crm.CoreApi.DomainModel.DynamicEntity;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.DynamicEntity;
using UBeat.Crm.CoreApi.Services.Services;

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class DynamicEntityController : BaseController
    {
        private readonly DynamicEntityServices _dynamicEntityServices;
        private readonly EntityExcelImportServices _entityExcelImportServices;

        public DynamicEntityController(DynamicEntityServices dynamicEntityServices,
            EntityExcelImportServices entityExcelImportServices) : base(dynamicEntityServices)
        {
            _dynamicEntityServices = dynamicEntityServices;
            _entityExcelImportServices = entityExcelImportServices;
        }

        [HttpPost]
        [Route("add")]
        public OutputResult<object> Add([FromBody] DynamicEntityAddModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            WriteOperateLog("提交动态数据", dynamicModel);
            var header = GetAnalyseHeader();
            return _dynamicEntityServices.Add(dynamicModel, header, UserId);
        }

        [HttpPost]
        [Route("addlist")]
        public OutputResult<object> AddList([FromBody] DynamicEntityAddListModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            WriteOperateLog("提交动态数据", dynamicModel);
            var header = GetAnalyseHeader();
            return _dynamicEntityServices.AddList(dynamicModel, header, UserId);
        }

        [HttpPost]
        [Route("generalprotocol")]
        public OutputResult<object> GeneralProtocolResult([FromBody] DynamicEntityGeneralModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _dynamicEntityServices.GeneralProtocol(dynamicModel, UserId);
        }
        [HttpPost]
        [Route("generalprotocolforgrid")]
        public OutputResult<object> GeneralGridProtocolResult([FromBody] DynamicGridEntityGeneralModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _dynamicEntityServices.GeneralGridProtocol(dynamicModel, UserId);
        }

        [HttpPost]
        [Route("generalwebprotocol")]
        public OutputResult<object> ListViewColumns([FromBody] DynamicEntityGeneralModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _dynamicEntityServices.ListViewColumns(dynamicModel, UserId);
        }

        [HttpPost]
        [Route("generaldynwebprotocol")]
        public OutputResult<object> DynamicListViewColumns([FromBody] DynamicEntityGeneralModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _dynamicEntityServices.DynamicListViewColumns(dynamicModel, UserId);
        }
        [HttpPost]
        [Route("generaldic")]
        public OutputResult<object> GeneralDictionaryResult([FromBody] DynamicEntityGeneralDicModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _dynamicEntityServices.GeneralDictionary(dynamicModel, UserId);
        }

        [HttpPost]
        [Route("edit")]
        public OutputResult<object> Edit([FromBody] DynamicEntityEditModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            WriteOperateLog("修改动态数据", dynamicModel);
            var header = GetAnalyseHeader();
            return _dynamicEntityServices.Edit(dynamicModel, header, UserId);
        }

        [HttpPost]
        [Route("list")]
        public OutputResult<object> DataList([FromBody] DynamicEntityListModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            var isAdvance = dynamicModel.IsAdvanceQuery == 1;
            return _dynamicEntityServices.DataList2(dynamicModel, isAdvance, UserId);
        }
        [HttpPost("listcount")]
        public OutputResult<object> ListStatistic([FromBody] DynamicEntityListViewColumnModel paramInfo ) {
            if (paramInfo == null || paramInfo.EntityId == null || paramInfo.EntityId == Guid.Empty) {
                return ResponseError<object>("参数异常");
            }
            return _dynamicEntityServices.CalcMenuDataCount(paramInfo.EntityId,UserId);
        }
        /// <summary>
        /// 此接口已废弃
        /// </summary>
        /// <param name="dynamicModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("advance")]
        public OutputResult<object> AdvanceQuery([FromBody] DynamicEntityListModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _dynamicEntityServices.DataList2(dynamicModel, true, UserId);
        }

        [HttpPost]
        [Route("detail")]
        public OutputResult<object> Detail([FromBody] DynamicEntityDetailModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _dynamicEntityServices.Detail(dynamicModel, UserId);
        }
        [HttpPost]
        [Route("detailList")]
        public OutputResult<object> DetailList([FromBody] DynamicEntityDetaillistModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _dynamicEntityServices.DetailList(dynamicModel, UserId);
        }


        [HttpPost]
        [Route("pluginvisible")]
        public OutputResult<object> PluginCheckVisible([FromBody] DynamicPluginVisibleModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _dynamicEntityServices.PluginCheckVisible(dynamicModel, UserId);
        }

        /// <summary>
        /// 获取功能按钮列表
        /// </summary>
        /// <param name="dynamicModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("functionbutton")]
        public OutputResult<object> GetFunctionBtns([FromBody] FunctionBtnsModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _dynamicEntityServices.GetFunctionBtns(dynamicModel, UserId);
        }



        [HttpPost]
        [Route("pagevisible")]
        public OutputResult<object> PageCheckVisible([FromBody] DynamicPageVisibleModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _dynamicEntityServices.PageCheckVisible(dynamicModel, UserId);
        }

        [HttpPost]
        [Route("transfer")]
        public OutputResult<object> Transfer([FromBody] DynamicEntityTransferModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            WriteOperateLog("提交转移负责人数据", dynamicModel);
            return _dynamicEntityServices.Transfer(dynamicModel, UserId);
        }
        /// <summary>
        /// 用于一键转移所有的数据
        /// </summary>
        /// <param name="paramInfo"></param>
        /// <returns></returns>
        [HttpPost("transfer_v2_user2user")]
        public OutputResult<object> Tansfer_V2_User2User([FromBody] DynamicEntityTransferUser2UserModel paramInfo = null) {
            if (paramInfo == null ) return ResponseError<object>("参数格式错误");

            return _dynamicEntityServices.TransferUser2User(paramInfo,UserId);
        }

        [HttpPost]
        [Route("delete")]
        public OutputResult<object> Delete([FromBody] DynamicEntityDeleteModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            WriteOperateLog("提交删除数据", dynamicModel);
            return _dynamicEntityServices.Delete(dynamicModel, UserId);
        }
        [HttpPost]
        [Route("deleterelation")]
        public OutputResult<object> DeleteDataSrcRelation([FromBody] DataSrcDeleteRelationModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            WriteOperateLog("提交删除数据", dynamicModel);
            return _dynamicEntityServices.DeleteDataSrcRelation(dynamicModel, UserId);
        }

        [HttpPost]
        [Route("addconnect")]
        public OutputResult<object> AddConnect([FromBody] DynamicEntityAddConnectModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            WriteOperateLog("提交实体关系数据", dynamicModel);
            return _dynamicEntityServices.AddConnect(dynamicModel, UserId);
        }

        [HttpPost]
        [Route("editconnect")]
        public OutputResult<object> EditConnect([FromBody] DynamicEntityEditConnectModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            WriteOperateLog("修改实体关系数据", dynamicModel);
            return _dynamicEntityServices.EditConnect(dynamicModel, UserId);
        }

        [HttpPost]
        [Route("deleteconnect")]
        public OutputResult<object> DeleteConnect([FromBody] DynamicEntityDeleteConnectModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            WriteOperateLog("删除实体关系数据", dynamicModel);
            return _dynamicEntityServices.DeleteConnect(dynamicModel, UserId);
        }

        [HttpPost]
        [Route("connectlist")]
        public OutputResult<object> ConnectList([FromBody] DynamicEntityConnectListModel listModel = null)
        {
            if (listModel == null) return ResponseError<object>("参数格式错误");
            return _dynamicEntityServices.ConnectList(listModel, UserId);
        }

        [HttpPost]
        [Route("entitylist")]
        public OutputResult<object> EntitySearchList([FromBody] DynamicEntitySearchListModel listModel = null)
        {
            if (listModel == null) return ResponseError<object>("参数格式错误");
            return _dynamicEntityServices.EntitySearchList(listModel, UserId);
        }

        [HttpPost]
        [Route("searchrepeat")]
        public OutputResult<object> SearchRepeat([FromBody] DynamicEntitySearchRepeatModel listModel = null)
        {
            if (listModel == null) return ResponseError<object>("参数格式错误");
            return _dynamicEntityServices.EntitySearchRepeat(listModel, UserId);
        }
        /// <summary>
        /// 此接口已经废弃，合并至list接口
        /// </summary>
        /// <param name="dynamicModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("reltablist")]
        public OutputResult<object> ConnectList([FromBody] DynamicRelTabModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _dynamicEntityServices.RelTabList(dynamicModel, UserId);
        }

        [HttpPost]
        [Route("reltabsrclist")]
        public OutputResult<object> RelTabSrcSqlList([FromBody] DynamicRelTabModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _dynamicEntityServices.RelTabSrcSqlList(dynamicModel, UserId);
        }


        [HttpPost]
        [Route("queryreltablist")]
        public OutputResult<object> RelTabListQuery([FromBody] RelTabListModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _dynamicEntityServices.RelTabListQuery(dynamicModel, UserId);
        }

        [HttpPost]
        [Route("queryreltablistbyrole")]
        public OutputResult<object> RelTabListQueryByRole([FromBody] RelTabListModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _dynamicEntityServices.RelTabListQueryByRole(dynamicModel, UserId);
        }

        [HttpPost]
        [Route("queryreltabinfo")]
        public OutputResult<object> RelTabInfoQuery([FromBody] RelTabInfoModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _dynamicEntityServices.RelTabInfoQuery(dynamicModel, UserId);
        }

        [HttpPost]
        [Route("getreltabentity")]
        public OutputResult<object> GetRelTabEntity([FromBody] RelTabListModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _dynamicEntityServices.GetRelTabEntity(dynamicModel, UserId);
        }

        [HttpPost]
        [Route("getrelentityfields")]
        public OutputResult<object> GetRelEntityFields([FromBody] GetEntityFieldsModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _dynamicEntityServices.GetRelEntityFields(dynamicModel, UserId);
        }

        [HttpPost]
        [Route("getrelconfigfields")]
        public OutputResult<object> GetRelConfigFields([FromBody] GetEntityFieldsModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _dynamicEntityServices.GetRelConfigFields(dynamicModel, UserId);
        }

        [HttpPost]
        [Route("addreltab")]
        public OutputResult<object> AddRelTab([FromBody] AddRelTabModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _dynamicEntityServices.AddRelTab(dynamicModel, UserId);
        }

        [HttpPost]
        [Route("saverelconfig")]
        public OutputResult<object> SaveRelConfig([FromBody] SaveRelConfigModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _dynamicEntityServices.SaveRelConfig(dynamicModel, UserId);
        }

        [HttpPost]
        [Route("getrelconfig")]
        public OutputResult<object> GetRelConfig([FromBody] RelConfigModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _dynamicEntityServices.GetRelConfig(dynamicModel, UserId);
        }

        /// <summary>
        /// 返回页签统计信息
        /// </summary>
        /// <param name="queryModel"></param>>
        /// <returns></returns>
        [HttpPost]
        [Route("queryreldatasource")]
        public OutputResult<object> queryDataForDataSource([FromBody]RelQueryDataModel queryModel)
        {
            if (queryModel == null || queryModel.RelId == null || queryModel.RelId== new Guid("00000000-0000-0000-0000-000000000000")
                || queryModel.RecId == null || queryModel.RecId== new Guid("00000000-0000-0000-0000-000000000000"))
                return new OutputResult<object>(null, "参数异常", 1);
           
            try
            {

                return _dynamicEntityServices.queryDataForDataSource(ControllerContext.HttpContext.RequestServices,queryModel, UserId); ;
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    return new OutputResult<object>(null, ex.InnerException.Message, -1);

                }
                else
                {

                    return new OutputResult<object>(null, ex.Message, -1);
                }
            }

        }
        [HttpPost("queryvaluefornewdata")]
        [AllowAnonymous]
        public OutputResult<object> QueryValueForReltabAddNewData([FromBody] RelTabQueryDataSourceModel paramInfo ) {
            if (paramInfo == null || paramInfo.EntityId == Guid.Empty || paramInfo.FieldId == Guid.Empty || paramInfo.RecId == Guid.Empty) {
                return ResponseError<object>("参数异常");
            }
            return this._dynamicEntityServices.QueryValueForRelTabAddNew(paramInfo, UserId);
        }

        [HttpPost]
        [Route("editreltab")]
        public OutputResult<object> UpdateRelTab([FromBody] UpdateRelTabModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _dynamicEntityServices.UpdateRelTab(dynamicModel, UserId);
        }

        [HttpPost]
        [Route("disabledreltab")]
        public OutputResult<object> DisabledRelTab([FromBody] DisabledRelTabModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _dynamicEntityServices.DisabledRelTab(dynamicModel, UserId);
        }


        [HttpPost]
        [Route("orderbyreltab")]
        public OutputResult<object> OrderbyRelTab([FromBody] OrderbyRelTabModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _dynamicEntityServices.OrderbyRelTab(dynamicModel, UserId);
        }

        [HttpPost]
        [Route("addreltabreldatasrc")]
        public OutputResult<object> AddRelTabRelationDataSrc([FromBody] AddRelTabRelationDataSrcModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _dynamicEntityServices.AddRelTabRelationDataSrc(dynamicModel, UserId);
        }

        [HttpPost]
        [Route("ishaspermission")]
        public OutputResult<object> IsHasPerssion([FromBody] PermissionModel dynamicModel = null)
        {
            if (dynamicModel == null) throw new System.Exception("参数格式错误");
            return _dynamicEntityServices.IsHasPerssion(dynamicModel, UserId);
        }


        [HttpPost]
        [Route("follow")]
        public OutputResult<object> Follow([FromBody] FollowModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _dynamicEntityServices.FollowRecord(dynamicModel, UserId);
        }



        [HttpPost]
        [Route("markcomplete")]
        public OutputResult<object> MarkComplete([FromBody] MarkCompleteModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _dynamicEntityServices.MarkRecordComplete(dynamicModel.RecId, UserId);
        }
        /// <summary>
        /// U客100引擎的服务端扩展，主要用于项目
        /// 产品开发请勿使用此函数。
        /// 执行此函数将不纳入系统的数据权限范围，数据权限校验由数据库函数处理。
        /// 但服务端会校验：
        /// 1、该实体是否定义了这个函数
        /// 2、此函数是否对应有权限项，以及当前用户是否有该权限。
        /// 函数可以返回
        /// </summary>
        /// <param name="functionname"></param>
        /// <param name="paramInfo"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ukextengine/{functionname}")]
        public OutputResult<object> UKExtExecuteFunction([FromRoute] string functionname, [FromBody] UKExtExecuteFunctionModel paramInfo)
        {
            if (paramInfo == null) return ResponseError<object>("参数格式错误");
            if (functionname == null || functionname.Length == 0) return ResponseError<object>("参数格式错误");
            OutputResult<object> ret = _dynamicEntityServices.ExecuteExtFunction(functionname, paramInfo, UserId);
            return ret;
        }

        /// <summary>
        /// 获取实体服务器扩展的函数列表及状态信息 
        /// </summary>
        /// <param name="paramInfo"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ukextconf/list")]
        public OutputResult<object> ListExtFunction([FromBody] UKExtConfigListModel paramInfo) {
            return null;

        }
        /// <summary>
        /// 新增服务器扩展函数
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("ukextconf/add")]
        public OutputResult<object> AddExtFunction() {
            return null;
        }
        /// <summary>
        /// 编辑保存服务器扩展函数 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("ukextconf/edit")]
        public OutputResult<object> EditExtFunction() {
            return null;
        }
        /// <summary>
        /// 删除服务器扩展函数
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("ukextconf/delete")]
        public OutputResult<object> DeleteExtFunction()
        {
            return null;
        }
        /// <summary>
        /// 启用服务器扩展函数
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("ukextconf/enable")]
        public OutputResult<object> EnableExtFunction()
        {
            return null;
        }
        /// <summary>
        /// 禁用服务器扩展函数
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("ukextconf/disable")]
        public OutputResult<object> DisableExtFunction()
        {
            return null;
        }

        #region 查重
        /// <summary>
        /// 查重
        /// </summary>
        /// <param name="body">条件对象</param>
        /// <returns></returns>
        [HttpPost]
        [Route("queryentitycondition")]
        public OutputResult<object> QueryEntityCondition([FromBody] EntityCondition body)
        {
            if (body == null || body.EntityId.Equals(Guid.Empty))
                return ResponseError<object>("参数格式错误");
            body.FuncType = (int)FuncType.Repeat;
            return _dynamicEntityServices.QueryEntityCondition(body);
        }

        /// <summary>
        /// 查重修改
        /// </summary>
        /// <param name="body">参数对象</param>
        /// <returns></returns>
        [HttpPost]
        [Route("updateentitycondition")]
        public OutputResult<object> UpdateEntityCondition([FromBody] EntityCondition body)
        {
            if (body == null || body.EntityId.Equals(Guid.Empty))
                return ResponseError<object>("参数格式错误");
            Guid g = Guid.Empty;
            if (!string.IsNullOrEmpty(body.FieldIds))
            {
                var list = body.FieldIds.Split(',');
                for (int i = 0; i < list.Count(); i++)
                {
                    if (!Guid.TryParse(list[i], out g))
                        return ResponseError<object>("GUID格式有误！");
                }
            }
            return _dynamicEntityServices.UpdateEntityCondition(body, UserId);
        }
        #endregion

        #region 通用实体（独立实体、简单实体、动态实体）WEB列表中，显示字段的个人配置相关接口
        /// <summary>
        /// 获取实体的个人web列表定义
        /// </summary>
        /// <param name="paramInfo"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("getweblistcolumnsforpersonal")]
        public OutputResult<object> WebListColumnsForPersonal([FromBody] WebListColumnsForPersonalParamInfo paramInfo)
        {
            if (paramInfo == null
                || paramInfo.EntityId == null
                || paramInfo.EntityId.Equals(Guid.Empty))
            {
                return ResponseError<object>("参数异常");
            }
            WebListPersonalViewSettingInfo columnsSetting = this._dynamicEntityServices.GetPersonalWebListColumnsSettting(paramInfo, UserId);
            return new OutputResult<object>(columnsSetting);
        }
        /// <summary>
        /// 保存实体的个人web列表定义
        /// </summary>
        /// <param name="paramInfo"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("saveweblistcolumnsforpersonal")]
        public OutputResult<object> SaveWebListColumnsForPersonal([FromBody] SaveWebListColumnsForPersonalParamInfo paramInfo)
        {
            if (paramInfo == null
                || paramInfo.EntityId == null
                || paramInfo.EntityId.Equals(Guid.Empty))
            {
                return ResponseError<object>("参数异常");
            }
            try
            {
                this._dynamicEntityServices.SavePersonalWebListColumnsSetting(paramInfo, UserId);
            }
            catch (Exception ex) {
                return ResponseError<object>(ex.Message);
            }
            return new OutputResult<object>("success");
        }
        #endregion

        [HttpPost("test")]
        [AllowAnonymous]
        public OutputResult<object> test() {
            try
            {

                Dictionary<string,object>  listEntity = _entityExcelImportServices.ImportEntityFromExcel();
                listEntity.Add("allmessage", this._entityExcelImportServices.GenerateTotalMessage(listEntity));
                return new OutputResult<object>(listEntity);
            }
            catch (Exception ex) {
                return ResponseError<object>(ex.Message);
            }
        }
        [Route("sendtomule")]

        #region 用于测试安居宝，过后会删除
        public OutputResult<object> SendToMule([FromBody] MuleSendParamInfo paramInfo)
        {
            return this._dynamicEntityServices.SendToMule(paramInfo, UserId);

        } 
        #endregion
    }
    
}
