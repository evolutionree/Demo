using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace UBeat.Crm.CoreApi.DomainModel.DJCloud
{
    public class DJCloudCallMapper : BaseEntity
    {
        public string CallId { set; get; }

        public string SessionId { set; get; }

        public string Caller { set; get; }

        public string Called { set; get; }

        public int IsSeccess { set; get; }

        public string FailMsg { set; get; }

        public DateTime CallTime { set; get; }

        public string Description { set; get; }

        public DateTime Reccreated { set; get; }

        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<DJCloudCallMapper>
        {
            public Validator()
            {
            }
        }
    }


    public class CallInfo
    {
        public string appID { set; get; }

        public string callID { set; get; }

        public string sessionID { set; get; }
    }

    public class CallResultModel
    {
        public int code { set; get; }

        public string msg { set; get; }

        public string suggest { set; get; }

        public int IsSeccess { set; get; }

        public CallInfo Info { set; get; }
    }
}
