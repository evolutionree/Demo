using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Account
{
    public class DeviceBindInfo
    {
        public Guid RecId { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; }

        public string DeviceModel { get; set; }

        public string OsVersion { get; set; }

        public string UniqueId { get; set; }

        public int RecStatus { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }
    }
}