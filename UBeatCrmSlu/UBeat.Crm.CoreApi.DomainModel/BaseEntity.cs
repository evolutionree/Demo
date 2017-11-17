using System.Linq;
using FluentValidation;
using FluentValidation.Results;

namespace UBeat.Crm.CoreApi.DomainModel
{
    public abstract class BaseEntity
    {
        private ValidationState _validationState;
        protected abstract IValidator GetValidator();

        public ValidationState ValidationState
        {
            get
            {
                if (_validationState == null)
                {
                    IsValid();
                }
                return _validationState;
            }
        }


        public bool IsValid()
        {
            if (_validationState == null)
            {
                ValidationResult result = GetValidator().Validate(this);
                _validationState =
                    new ValidationState(
                        result.Errors.Select(d => d.ErrorMessage),
                        result.IsValid);
            }
            return _validationState.IsValid;
        }
    }
}