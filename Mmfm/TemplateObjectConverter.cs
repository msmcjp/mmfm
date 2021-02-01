using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mmfm
{
    public class TemplateObjectConverter : JsonConverter<ExpandoObject>
    {
        private IDictionary<string, object> template;
        public TemplateObjectConverter(ExpandoObject template)
        {
            this.template = template;
        }

        public override ExpandoObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            IDictionary<string, object> deserialized = new ExpandoObject();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propertyName = reader.GetString();
                    if (template.ContainsKey(propertyName))
                    {
                        var type = template[propertyName].GetType();
                        deserialized[propertyName] = JsonSerializer.Deserialize(ref reader, type, options) ?? template[propertyName];
                    }
                }
            }

            return (ExpandoObject)deserialized;
        }

        public override void Write(Utf8JsonWriter writer, ExpandoObject value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize<ExpandoObject>(writer, value, options);
        }
    }
}
