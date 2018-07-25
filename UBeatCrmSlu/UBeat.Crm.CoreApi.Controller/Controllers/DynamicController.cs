using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.CoreApi.Services.Models.Dynamics;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.DomainModel.Dynamics;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class DynamicController : BaseController
    {
        private readonly ILogger<DynamicController> _logger;

        private readonly DynamicServices _service;


        public DynamicController(ILogger<DynamicController> logger, DynamicServices service) : base(service)
        {
            _logger = logger;
            _service = service;
        }

        #region --动态摘要--


        /// <summary>
        /// 保存摘要设置
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("savedynamicabstract")]
        public OutputResult<object> SaveDynamicAbstract([FromBody] SaveDynamicAbstractModel body)
        {
            if (body == null)
                return ResponseError<object>("参数错误");
            WriteOperateLog("保存摘要设置", body);
            return _service.SaveDynamicAbstract(body, UserId);
        }
        /// <summary>
        /// 查询动态摘要
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("selectdynamicabstract")]
        public OutputResult<object> SelectDynamicAbstract([FromBody] SelectDynamicAbstractModel body)
        {
            if (body == null)
                return ResponseError<object>("参数错误");
            return _service.SelectDynamicAbstract(body, UserId);
        }
        #endregion

        #region --动态详情--

        [HttpPost("adddefaultdynamic")]
        public OutputResult<object> AddDynamic([FromBody] AddDynamicModel body)
        {
            if (body == null)
                return ResponseError<object>("参数错误");
           
            var result = _service.AddDynamic(body, UserId);
            if(result.Status==0)
            {
                WriteOperateLog("发布默认动态", body);
            }
            return result;
        }

        [HttpPost("deletedynamic")]
        public OutputResult<object> DeleteDynamic([FromBody] DeleteDynamicModel body)
        {
            if (body == null)
                return ResponseError<object>("参数错误");

            var result= _service.DeleteDynamic(body, UserId);
            if (result.Status == 0)
            {
                WriteOperateLog("删除动态", body);
            }
            return result;
        }

        [HttpPost("getdynamiclist")]
        public OutputResult<object> SelectDynamicList([FromBody] SelectDynamicListModel body)
        {
            if (body == null)
                return ResponseError<object>("参数错误");
            return _service.SelectDynamicList(body, UserId,GetAnalyseHeader());
        }
        [HttpPost("getdynamicdetail")]
        public OutputResult<object> SelectDynamic([FromBody] SelectDynamicModel body)
        {
            if (body == null)
                return ResponseError<object>("参数错误");
            return _service.SelectDynamic(body, UserId);
        }

        [HttpPost("dynamiclist")]
        public OutputResult<object> GetDynamicInfoList([FromBody] DynamicListModel body)
        {
            if (body == null)
                return ResponseError<object>("参数错误");
            return _service.GetDynamicInfoList(body, UserId);
        }
        [HttpPost("dynamicdetail")]
        public OutputResult<object> GetDynamicInfo([FromBody] SelectDynamicModel body)
        {
            if (body == null)
                return ResponseError<object>("参数错误");
            return _service.GetDynamicInfo(body, UserId);
        }
        /// <summary>
        /// 根据实体EntityId和RecId获取数据
        /// </summary>
        /// <returns></returns>
        [HttpPost("dynamicdetailbybizid")]
        public OutputResult<object> GetDynamicInfoByBizId([FromBody] SelectDynamicByBizIdParamInfo paramInfo) {
            if (paramInfo == null 
                || paramInfo.EntityId == null || paramInfo.EntityId == Guid.Empty
                || paramInfo.RecId == null || paramInfo.RecId == Guid.Empty)
            {
                return ResponseError<object>("参数异常");
            }
            try
            {

                DynamicInfo ret =   this._service.GetDynamicInfoByBizId(paramInfo.EntityId, paramInfo.RecId, UserId);
                return new OutputResult<object>(ret);
            }
            catch (Exception ex) {
                return ResponseError<object>(ex.Message);
            }

        }

        #endregion

        #region --动态评论--

        #region --新增动态评论-- POST: /adddynamiccomments 

        [HttpPost("adddynamiccomments")]
        public OutputResult<object> AddDynamicComments_old([FromBody] AddDynamicCommentsModel body)
        {
            if (body == null)
                return ResponseError<object>("参数错误");

            var result = _service.AddDynamicComments_old(body, UserId);
            if (result.Status == 0)
            {
                WriteOperateLog("发表评论", body);
            }
            return result;
        }
        [HttpPost("addcomments")]
        public OutputResult<object> AddDynamicComments([FromBody] AddDynamicCommentsModel body)
        {
            if (body == null)
                return ResponseError<object>("参数错误");

            var result = _service.AddDynamicComments(body, UserId);
            if (result.Status == 0)
            {
                WriteOperateLog("发表评论", body);
            }
            return result;
        }

        #endregion

        #region --(暂时)删除动态评论-- POST: /deletedynamiccomments 

        [HttpPost("deletedynamiccomments")]
        public OutputResult<object> DeleteDynamicComments([FromBody] DeleteDynamicCommentsModel body)
        {
            if (body == null)
                return ResponseError<object>("参数错误");

            var result = _service.DeleteDynamicComments(body, UserId);
            if (result.Status == 0)
            {
                WriteOperateLog("删除动态评论", body);
            }
            return result;
        }
        #endregion

        #endregion


        #region --动态点赞--

        #region --新增动态点赞-- POST: /adddynamicpraise 

        [HttpPost("adddynamicpraise")]
        public OutputResult<object> AddDynamicPraise_old([FromBody] DynamicPraiseModel body)
        {
            if (body == null)
                return ResponseError<object>("参数错误");

            var result = _service.AddDynamicPraise_old(body, UserId);
            if (result.Status == 0)
            {
                WriteOperateLog("点赞动态", body);
            }
            return result;
        }


        [HttpPost("addpraise")]
        public OutputResult<object> AddDynamicPraise([FromBody] DynamicPraiseModel body)
        {
            if (body == null)
                return ResponseError<object>("参数错误");

            var result = _service.AddDynamicPraise(body, UserId);
            if (result.Status == 0)
            {
                WriteOperateLog("点赞动态", body);
            }
            return result;
        }

        #endregion

        #region --(暂时)删除动态点赞-- POST: /deletedynamicpraise 

        [HttpPost("deletedynamicpraise")]
        public OutputResult<object> DeleteDynamicPraise([FromBody] DynamicPraiseModel body)
        {
            if (body == null)
                return ResponseError<object>("参数错误");

            var result = _service.DeleteDynamicPraise(body, UserId);
            if (result.Status == 0)
            {
                WriteOperateLog("取消点赞", body);
            }
            return result;

        }
        #endregion

        #endregion

    }
}
