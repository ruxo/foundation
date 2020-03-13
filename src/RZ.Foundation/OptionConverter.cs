using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace RZ.Foundation
{
    static class OptionConverterHelper
    {
        internal static readonly JToken NullToken = (string)null;
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
        readonly JsonToken expectedToken;

        public OptionConverter()
        {
            var t = typeof(T);

            if (OptionConverterHelper.PrimitiveLookup.TryGetValue(t, out var value))
                expectedToken = value;
            else
                expectedToken = JsonToken.StartObject;  // assume object
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Option<T>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) =>
            reader.TokenType == JsonToken.Null ? Option<T>.None() : serializer.Deserialize<T>(reader).ToOption();

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            var v = (Option<T>) value;
            if (v.IsSome)
                serializer.Serialize(writer, v.Get(), typeof(T));
            else
                serializer.Serialize(writer, null);
        }
    }
}
