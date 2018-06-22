using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using UBeat.Crm.CoreApi.DomainModel.TransferScheme;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.TransferScheme;
using UBeat.Crm.CoreApi.Services.Services;

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class TransferSchemeController : BaseController
    {
        private readonly TransferSchemeServices _transferScheme;
        public TransferSchemeController(TransferSchemeServices transferSchemeServices) : base(transferSchemeServices)
        {
            _transferScheme = transferSchemeServices;
        }

        /// <summary>
        /// 保存转移方案
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("savetransferscheme")]
        public OutputResult<object> SaveTransferScheme([FromBody]TransferSchemeParam body)
        {
            if (body == null) return ResponseError<object>("参数格式有误");
            if (string.IsNullOrWhiteSpace(body.TransSchemeName)) return ResponseError<object>("请填写转移方案名称");
            if (body.TargetTransferId == null) return ResponseError<object>("请选择目标对象");
           // if (string.IsNullOrEmpty(body.AssociationTransfer)) return ResponseError<object>("请选择关联转移");
            return _transferScheme.SaveTransferScheme(body, UserId);
        }

        /// <summary>
        /// 查询转移方案
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("getdetail")]
        public OutputResult<TransferSchemeModel> GetTransferScheme([FromBody]GetTransParam body)
        {
            if (body == null || body.TransSchemeId == Guid.Empty) return ResponseError<TransferSchemeModel>("参数格式有误");
            return _transferScheme.GetTransferScheme(body.TransSchemeId, UserId);
        }

        /// <summary>
        /// 设置转移方案状态
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("setstatus")]
        public OutputResult<object> SetTransferSchemeStatus([FromBody] TransStatus body)
        {
            if (body == null) return ResponseError<object>("参数格式有误");
            if (string.IsNullOrEmpty(body.RecIds)) return ResponseError<object>("方案转移id未传递");
            var ids = body.RecIds.Split(',');
            List<Guid> list = new List<Guid>();
            Guid guid;
            foreach (var item in ids)
            {
                if (Guid.TryParse(item, out guid))
                    list.Add(guid);
                else
                    return ResponseError<object>("方案转移id格式有误");
            }
            return _transferScheme.SetTransferSchemeStatus(list, body.Status, UserId);
        }

        /// <summary>
        /// 方案转移列表
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("transferschemelist")]
        public OutputResult<object> TransferSchemeList([FromBody] ListModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");
            return _transferScheme.TransferSchemeList(body, UserId);
        }
        [HttpPost("listschemebyentity")]
        public OutputResult<object> ListTransferSchemesByEntity([FromBody] SearchEntitySchemeParamInfo paramInfo) {
            if (paramInfo == null) return ResponseError<object>("参数异常");
            return _transferScheme.ListTransferSchemesByEntity(paramInfo.EntityId, UserId);
        }

    }
}