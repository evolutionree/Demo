using System;
using System.Linq;
using Newtonsoft.Json;

namespace UBeat.Crm.CoreApi.Utility
{
    public class JsonCodeConverter : JsonConverter
    {
        private readonly Type[] _types;

        public JsonCodeConverter(params Type[] types)
        {
            _types = types;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if ((value as  string == null ) && IsJsonObjectString(value.ToString()))
            {
                writer.WriteRawValue(value.ToString());
            }
            else
            {
                writer.WriteValue(value);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
        }

        public override bool CanConvert(Type objectType)
        {
            return _types.Any(t => t == objectType);
        }

        public override bool CanRead => false;

        public bool IsJsonObjectString(string input)
        {
            return input.Trim().StartsWith("{") && input.EndsWith("}");
        }
    }
}