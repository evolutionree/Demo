using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Dynamics
{
    public class DynamicListParameter
    {
        public Guid EntityId { set; get; }

        public Guid Businessid { set; get; }
        
        public List<DynamicType> DynamicTypes { set; get; }

    }
}
