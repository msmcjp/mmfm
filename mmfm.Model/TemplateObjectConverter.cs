using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mmfm.Model
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
            int depth = reader.CurrentDepth;
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    
                    var propertyName = reader.GetString();
                    if (template.ContainsKey(propertyName))
                    {
                        var type = template[propertyName].GetType();
                        deserialized[propertyName] = JsonSerializer.Deserialize(ref reader, type, options) ?? template[propertyName];
                        continue;
                    }
                }

                if (reader.TokenType == JsonTokenType.EndObject && reader.CurrentDepth == depth)
                {
                    break;
                }
            }

            // in case of deserialized object does not have template member
            foreach(var key in template.Keys)
            {
                if (deserialized.ContainsKey(key) == false)
                {
                    deserialized.Add(key, template[key]);
                }
            }

            return (ExpandoObject)deserialized;
        }

        public override void Write(Utf8JsonWriter writer, ExpandoObject value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize<ExpandoObject>(writer, value, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
