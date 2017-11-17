using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Role
{
    public class RolesUsersInfo
    {
        public Guid RoleId { set; get; }

        public int UserId { set; get; }

        public string UserName { set; get; }
    }
}
