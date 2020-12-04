using System;
using System.Collections.Generic;
using LanguageExt;
using Newtonsoft.Json;
using RZ.Foundation.Extensions;
using static LanguageExt.Prelude;

namespace RZ.Foundation.NewtonsoftJson
{
    static class OptionConverterHelper
    {
        internal static readonly Dictionary<Type, JsonToken> PrimitiveLookup = new()
        {
            {typeof(string), JsonToken.String},
            {typeof(bool), JsonToken.Boolean},
            {typeof(float), JsonToken.Float},
            {typeof(double), JsonToken.Float},
            {typeof(int), JsonToken.Integer},
            {typeof(long), JsonToken.Integer}
        };
    }
    public sealed class OptionConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(Option<T>);

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) =>
            reader.TokenType == JsonToken.Null ? Option<T>.None : Optional(serializer.Deserialize<T>(reader)!);

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) {
            if (value is Option<T> {IsSome: true} v)
                serializer.Serialize(writer, v.Get(), typeof(T));
            else
                serializer.Serialize(writer, null);
        }
    }
}
