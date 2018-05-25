using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using UBeat.Crm.CoreApi.DomainModel.TransferScheme;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.TransferScheme;
using UBeat.Crm.CoreApi.Services.Services;

namespace UBeat.Crm.CoreApi.Controllers
{
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
        public OutputResult<object> SaveTransferScheme([FromBody]TransferSchemeParam body)
        {
            if (body == null) return ResponseError<object>("参数格式有误");
            if (string.IsNullOrWhiteSpace(body.TransSchemeName)) return ResponseError<object>("请填写转移方案名称");
            if (body.TargetTransferId == null) return ResponseError<object>("请选择目标对象");
            if (string.IsNullOrEmpty(body.AssociationTransfer)) return ResponseError<object>("请选择关联转移");
            return _transferScheme.SaveTransferScheme(body, UserId);
        }

        /// <summary>
        /// 查询转移方案
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
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
    }
}