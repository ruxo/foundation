using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RZ.Foundation.Json;

public class SetJsonConverter : JsonConverterFactory
{
    public static readonly SetJsonConverter Default = new();

    static readonly Type SetType = typeof(Set<>);
    public override bool CanConvert(Type typeToConvert) =>
        JsonConverterHelper.CanConvert(SetType, typeToConvert);

    static readonly Type SetSerializerType = typeof(SetSerializer<>);
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
        JsonConverterHelper.CreateConverter(SetSerializerType, typeToConvert);

    sealed class SetSerializer<T> : JsonConverter<Set<T>>
    {
        public override Set<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            var v = JsonSerializer.Deserialize<T[]>(ref reader, options);
            return toSet(v);
        }

        public override void Write(Utf8JsonWriter writer, Set<T> value, JsonSerializerOptions options) {
            JsonSerializer.Serialize<IEnumerable<T>>(writer, value, options);
        }
    }
}