using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Dynamics
{
    public class DynamicCommentParameter
    {
        public Guid PcommentsId { set; get; }

        public Guid DynamicId { set; get; }

        public string Comments { set; get; }

    }
}
