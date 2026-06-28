using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using RZ.Foundation.Types;

namespace RZ.Foundation.Json;

[PublicAPI]
public class OutcomeConverter : JsonConverterFactory
{
    static readonly Type OutcomeType = typeof(Outcome<>);

    public override bool CanConvert(Type typeToConvert)
        => JsonConverterHelper.CanConvert(OutcomeType, typeToConvert);

    static readonly Type OutcomeSerializerType = typeof(OutcomeSerializer<>);
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        => JsonConverterHelper.CreateConverter(OutcomeSerializerType, typeToConvert);

    public class OutcomeSerializer<T> : JsonConverter<Outcome<T>>
    {
        public override Outcome<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new ErrorInfoException(InvalidRequest, "Deserialize to OutcomeSerializer<T> failed: not a start object");

            var found = false;
            Outcome<T> result = new ErrorInfo(InvalidRequest, "Deserialize to OutcomeSerializer<T> failed: malformed Outcome JSON");

            // Read every property of THIS object; deserialize data/error, skip the rest. The loop
            // exits on this object's own EndObject, leaving the reader exactly where the serializer
            // expects it — so trailing properties (even nested objects) are handled correctly.
            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject){
                var key = reader.GetString()!;
                reader.Read(); // move to the property value
                if (!found && key.Equals("data", StringComparison.OrdinalIgnoreCase)){
                    result = JsonSerializer.Deserialize<T>(ref reader, options) ?? throw new ErrorInfoException(InvalidRequest, "Deserialize to OutcomeSerializer<T> failed: possibly type mismatched");
                    found = true;
                }
                else if (!found && key.Equals("error", StringComparison.OrdinalIgnoreCase)){
                    result = JsonSerializer.Deserialize<ErrorInfo>(ref reader, options) ?? throw new ErrorInfoException(InvalidRequest, "Deserialize to OutcomeSerializer<T> failed: possibly error type mismatched");
                    found = true;
                }
                else
                    reader.Skip(); // skip an unknown property's value, including nested objects/arrays
            }
            return result;
        }

        public override void Write(Utf8JsonWriter writer, Outcome<T> value, JsonSerializerOptions options) {
            writer.WriteStartObject();
            if (value.IfSuccess(out var v, out var e)){
                writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName("Data") ?? "Data");
                JsonSerializer.Serialize(writer, v, options);
            }
            else{
                writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName("Error") ?? "Error");
                JsonSerializer.Serialize(writer, e, options);
            }
            writer.WriteEndObject();
        }
    }
}