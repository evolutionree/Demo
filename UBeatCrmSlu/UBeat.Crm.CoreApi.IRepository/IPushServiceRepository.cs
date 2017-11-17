using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IPushServiceRepository
    {
        dynamic PushSingleAccount(string account, string message, int message_type, string send_time, int environment = 0);

        dynamic PushAccountList(List<string> account_list, string message, int message_type, int environment = 0);

        dynamic PushMultiAccounts(List<string> account_list, string message, int message_type, int environment = 0);
    }
}