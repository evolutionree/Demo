using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface ICustomerVisitRepository
    {
        List<string> GetDictionaryDataValues(int dicTypeid, List<int> dicIds);

    }
}
