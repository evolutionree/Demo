using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.TransferScheme;
using System.Data.Common;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface ITransferSchemeRepository
    {
        bool AddTransferScheme(TransferSchemeModel data, DbTransaction tran);

        bool UpdateTransferScheme(TransferSchemeModel data, DbTransaction tran);

        TransferSchemeModel GetTransferScheme(Guid TransSchemeId, DbTransaction tran, int userNumber);

        bool SetTransferSchemeStatus(List<Guid> list, int status, DbTransaction tran, int userNumber);

        List<Dictionary<string, object>> TransferSchemeList(int recStatus, string searchName, int userNumber);
    }
}
