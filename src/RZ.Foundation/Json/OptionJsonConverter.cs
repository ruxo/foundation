using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace RZ.Foundation.Json;

[PublicAPI]
public class OptionJsonConverter : JsonConverterFactory
{
    public static readonly OptionJsonConverter Default = new();

    static readonly Type OptionType = typeof(Option<>);

    public override bool CanConvert(Type typeToConvert) =>
        JsonConverterHelper.CanConvert(OptionType, typeToConvert);

    static readonly Type OptionSerializerType = typeof(OptionSerializer<>);
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
        JsonConverterHelper.CreateConverter(OptionSerializerType, typeToConvert);

    public class OptionSerializer<T> : JsonConverter<Option<T>>
    {
        public override Option<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            reader.TokenType == JsonTokenType.Null
                ? Option<T>.None
                : JsonSerializer.Deserialize<T>(ref reader, options);

        public override void Write(Utf8JsonWriter writer, Option<T> value, JsonSerializerOptions options) {
            if (value.IfSome(out var v))
                JsonSerializer.Serialize(writer, v, options);
            else
                writer.WriteNullValue();
        }
    }
}