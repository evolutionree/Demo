using System.Collections.Generic;

namespace UBeat.Crm.CoreApi.DomainModel
{
    public class ValidationState
    {
        public ValidationState()
        {

        }
        public ValidationState(IEnumerable<string> errors, bool isvalid)
        {

            this.Errors = errors;
            this.IsValid = isvalid;
        }
        public IEnumerable<string> Errors { get; private set; }
        public bool IsValid { get; private set; }
    }
}
