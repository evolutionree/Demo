using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.EntityPro
{
    public class RelateEntity
    {
        //entityid,entityname,entitytable,modeltype

        public Guid EntityId { set; get; }

        public string EntityName { set; get; }

        public string EntityTableName { set; get; }

        public EntityModelType ModelType { set; get; }

    }
}
