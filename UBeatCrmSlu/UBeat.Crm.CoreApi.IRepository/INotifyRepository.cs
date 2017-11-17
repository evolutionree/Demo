using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Notify;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface INotifyRepository : IBaseRepository
    {
        Dictionary<string, List<IDictionary<string, object>>> FetchMessage(NotifyFetchMessageMapper versionMapper, int userNumber);

        OperateResult WriteReadStatus(string msgIds, int userNumber);

        Dictionary<string, object> GetMessageList(DbTransaction tran, PageParam pageParam,int msgType, int userNumber);
        OperateResult WriteMessage( NotifyEntity readModel,bool isAutoSend, int userNumber);
    }
}