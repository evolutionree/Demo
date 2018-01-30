using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Contact;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IContactRepository : IBaseRepository
    {
        PageDataInfo<LinkManMapper> GetFlagLinkman(ContactMapper paramInfo, int userId);

        PageDataInfo<LinkManMapper> GetRecentCall(ContactMapper paramInfo, int userId);

        OperateResult FlagLinkman(ContactMapper paramInfo, int userId);
        OperateResult AddRecentCall(ContactMapper paramInfo, int userId);
    }
}
