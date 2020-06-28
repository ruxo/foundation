using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using LanguageExt;
using RZ.Foundation.Extensions;
using static LanguageExt.Prelude;

namespace RZ.Foundation
{
    static class OptionConverterHelper
    {
        internal static readonly Dictionary<Type, JsonToken> PrimitiveLookup = new Dictionary<Type, JsonToken>
        { { typeof(string), JsonToken.String }
          , { typeof(bool), JsonToken.Boolean }
          , { typeof(float), JsonToken.Float }
          , { typeof(double), JsonToken.Float }
          , { typeof(int), JsonToken.Integer }
          , { typeof(long), JsonToken.Integer }
        };
    }
    public class OptionConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(Option<T>);

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) =>
            reader.TokenType == JsonToken.Null ? Option<T>.None : Optional(serializer.Deserialize<T>(reader)!);

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) {
            if (value is Option<T> v && v.IsSome)
                serializer.Serialize(writer, v.Get(), typeof(T));
            else
                serializer.Serialize(writer, null);
        }
    }
}
