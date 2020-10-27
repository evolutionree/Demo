using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.Services.Models.WJXModel;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IWJXRepository
    {
        void SaveWXJAnswer(WJXCallBack callBack, int userId, DbTransaction tran = null);
        List<WJXCallBack> GetWXJAnswerList(WJXCallBack callBack, int userId, DbTransaction tran = null);
    }
}
