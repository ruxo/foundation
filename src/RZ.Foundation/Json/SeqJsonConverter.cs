using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RZ.Foundation.Json;

public class SeqJsonConverter : JsonConverterFactory
{
    public static readonly SeqJsonConverter Default = new();

    static readonly Type SeqType = typeof(Seq<>);
    public override bool CanConvert(Type typeToConvert) =>
        JsonConverterHelper.CanConvert(SeqType, typeToConvert);

    static readonly Type SeqSerializerType = typeof(SeqSerializer<>);
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
        JsonConverterHelper.CreateConverter(SeqSerializerType, typeToConvert);

    sealed class SeqSerializer<T> : JsonConverter<Seq<T>>
    {
        public override Seq<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            var v = JsonSerializer.Deserialize<T[]>(ref reader, options);
            return v.ToSeq();
        }

        public override void Write(Utf8JsonWriter writer, Seq<T> value, JsonSerializerOptions options) {
            JsonSerializer.Serialize<IEnumerable<T>>(writer, value, options);
        }
    }
}