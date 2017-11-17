using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.Dynamics
{
    public class AddDynamicCommentsModel
    {
        public Guid DynamicId { set; get; }

        public Guid? PCommentsid { set; get; }

        public string Comments { set; get; }

    }
    public class SelectDynamicCommentsModel
    {
        public Guid DynamicId { set; get; }

    }
    public class DeleteDynamicCommentsModel
    {
        public Guid Commentsid { set; get; }

    }
}
