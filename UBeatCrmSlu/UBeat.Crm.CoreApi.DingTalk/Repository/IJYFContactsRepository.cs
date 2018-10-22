using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DingTalk.Models;

namespace UBeat.Crm.CoreApi.DingTalk.Repository
{
    public interface IJYFContactsRepository
    {
        List<Dictionary<string, object>> GetAllCompanys(Dictionary<string, string>.KeyCollection.Enumerator enumerator);
        List<ContactPositionInfo> getContactsByCompanys(string companyids);
        Dictionary<Guid, string> GetContactsName(Dictionary<string, string>.KeyCollection keys);
        List<ContactsRelationInfo> GetOtherContactRelations(Dictionary<string, string>.KeyCollection.Enumerator enumerator);
        Dictionary<string, object> GetDeptInfo(Guid parentId, string deptname);
    }
}
