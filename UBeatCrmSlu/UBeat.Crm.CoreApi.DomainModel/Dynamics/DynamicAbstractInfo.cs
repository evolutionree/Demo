using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.EntityPro;

namespace UBeat.Crm.CoreApi.DomainModel.Dynamics
{
    public class DynamicAbstractInfo
    {
        public Guid Id { set; get; }

        public Guid TypeId { set; get; }

        public Guid EntityId { set; get; }

        public Guid FieldId { set; get; }

        public string FieldName { set; get; }

        public EntityFieldControlType ControlType { set; get; }

    }
}
