using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using LanguageExt;
using static LanguageExt.Prelude;

namespace RZ.Foundation
{
    class OptionConverterHelper
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

        public OptionConverter() {
            var t = typeof(T);

            expectedToken = OptionConverterHelper.PrimitiveLookup.TryGetValue(t, out var value) ? value : JsonToken.StartObject;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Option<T>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == expectedToken)
                return Some((T)Convert.ChangeType(reader.Value, typeof(T)));
            else if (reader.TokenType == JsonToken.Null)
                return None;
            else
                return serializer.Deserialize(reader, objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var opt = (Option<T>) value;
            var v = opt.IfNone(default(T));
            var token = Equals(v, null) ? OptionConverterHelper.NullToken : JToken.FromObject(v); 
            token.WriteTo(writer);
        }
    }
}
