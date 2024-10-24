using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
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
            var result = ReadOutcome(ref reader, options);
            // ensure the reader is at the end
            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject){}
            return result;
        }

        static Outcome<T> ReadOutcome(ref Utf8JsonReader reader, JsonSerializerOptions options) {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new ErrorInfoException(StandardErrorCodes.InvalidRequest, "Deserialize to OutcomeSerializer<T> failed: not a start object");
            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject){
                var key = reader.GetString()!;
                Trace.Assert(reader.Read());
                if (key.Equals("data", StringComparison.OrdinalIgnoreCase)){
                    var value = JsonSerializer.Deserialize<T>(ref reader, options) ?? throw new ErrorInfoException(StandardErrorCodes.InvalidRequest, "Deserialize to OutcomeSerializer<T> failed: possibly type mismatched");
                    return SuccessOutcome(value);
                }
                if (key.Equals("error", StringComparison.OrdinalIgnoreCase)){
                    var error = JsonSerializer.Deserialize<ErrorInfo>(ref reader, options) ?? throw new ErrorInfoException(StandardErrorCodes.InvalidRequest, "Deserialize to OutcomeSerializer<T> failed: possibly error type mismatched");
                    return FailedOutcome<T>(error);
                }
                reader.Skip();
            }
            return new ErrorInfo(StandardErrorCodes.InvalidRequest, "Deserialize to OutcomeSerializer<T> failed: not malformed Outcome JSON");
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