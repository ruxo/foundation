using System;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RZ.Foundation.Json;

static class JsonConverterHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CanConvert(Type functor, Type typeToConvert) =>
        typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == functor;

    public static JsonConverter CreateConverter(Type functor, Type typeToConvert) {
        var t = functor.MakeGenericType(typeToConvert.GetGenericArguments());
        return (JsonConverter)Activator.CreateInstance(t)!;
    }
}

public sealed class OptionJsonConverter : JsonConverterFactory
{
    public static readonly OptionJsonConverter Default = new();
    
    static readonly Type OptionType = typeof(Option<>);

    public override bool CanConvert(Type typeToConvert) =>
        JsonConverterHelper.CanConvert(OptionType, typeToConvert);

    static readonly Type OptionSerializerType = typeof(OptionSerializer<>);
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) => 
        JsonConverterHelper.CreateConverter(OptionSerializerType, typeToConvert);

    sealed class OptionSerializer<T> : JsonConverter<Option<T>>
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