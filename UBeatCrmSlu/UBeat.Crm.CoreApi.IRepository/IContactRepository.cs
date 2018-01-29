using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Contact;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IContactRepository : IBaseRepository
    {
        PageDataInfo<LinkManMapper> GetFlagLinkman(LinkManMapper paramInfo, int userId);

        PageDataInfo<LinkManMapper> GetRecentCall(LinkManMapper paramInfo, int userId);

        OperateResult FlagLinkman(LinkManMapper paramInfo, int userId);
        OperateResult AddRecentCall(LinkManMapper paramInfo, int userId);
    }
}
