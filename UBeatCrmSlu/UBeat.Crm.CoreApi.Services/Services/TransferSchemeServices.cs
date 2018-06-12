using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.TransferScheme;
using UBeat.Crm.CoreApi.DomainModel.TransferScheme;
using System.Data.Common;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class TransferSchemeServices : EntityBaseServices
    {
        private readonly ITransferSchemeRepository _transferSchemeRepository;
        public TransferSchemeServices(ITransferSchemeRepository transferSchemeRepository)
        {
            _transferSchemeRepository = transferSchemeRepository;
        }

        public OutputResult<object> SaveTransferScheme(TransferSchemeParam data, int userNumber)
        {
            DbTransaction tran = null;
            bool flag, isAdd;
            isAdd = data.TransSchemeId == Guid.Empty;
            if (isAdd)
                data.TransSchemeId = Guid.NewGuid();

            var model = new TransferSchemeModel
            {
                RecId = data.TransSchemeId,
                RecName = data.TransSchemeName,
                EntityId = data.TargetTransferId,
                Association = data.AssociationTransfer,
                RecCreator = userNumber,
                RecStatus = 1,
                RecCreated = DateTime.Now,
                Remark = data.Remark,
                FieldId = data.FieldId
            };

            if (isAdd)
                flag = _transferSchemeRepository.AddTransferScheme(model, tran);
            else
                flag = _transferSchemeRepository.UpdateTransferScheme(model, tran);

            if (flag)
                return new OutputResult<object>(null, "操作成功");
            else
                return new OutputResult<object>(null, "操作失败", 1);
        }

        public OutputResult<TransferSchemeModel> GetTransferScheme(Guid TransSchemeId, int userNumber)
        {
            DbTransaction tran = null;
            var data = _transferSchemeRepository.GetTransferScheme(TransSchemeId, tran, userNumber);
            return new OutputResult<TransferSchemeModel>(data);

        }

        public OutputResult<object> SetTransferSchemeStatus(List<Guid> list, int status, int userNumber)
        {
            DbTransaction tran = null;
            var flag = _transferSchemeRepository.SetTransferSchemeStatus(list, status, tran, userNumber);
            if (flag)
                return new OutputResult<object>(null, "操作成功");
            else
                return new OutputResult<object>(null, "操作失败", 1);
        }

        public OutputResult<object> TransferSchemeList(ListModel model, int userNumber)
        {
            var data = _transferSchemeRepository.TransferSchemeList(model.RecStatus, model.SearchName, userNumber);
            return new OutputResult<object>(data);
        }

    }
}
