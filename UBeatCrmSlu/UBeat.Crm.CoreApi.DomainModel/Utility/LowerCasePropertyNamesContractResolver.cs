using Newtonsoft.Json.Serialization;

namespace UBeat.Crm.CoreApi.DomainModel.Utility
{
    public class LowerCasePropertyNamesContractResolver: DefaultContractResolver
    {
        protected override string ResolvePropertyName(string propertyName) => propertyName.ToLowerInvariant();
    }
}
