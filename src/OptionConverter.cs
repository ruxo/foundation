using System;
using Newtonsoft.Json;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace RZ.Foundation
{
    public class OptionConverter<T> : JsonConverter
    {
        JsonToken expectedToken;
        static readonly JToken NullToken = (string)null;
        static readonly Dictionary<Type, JsonToken> primitiveLookup = new Dictionary<Type, JsonToken>
            { { typeof(string), JsonToken.String }
            , { typeof(bool), JsonToken.Boolean }
            , { typeof(float), JsonToken.Float }
            , { typeof(double), JsonToken.Float }
            , { typeof(int), JsonToken.Integer }
            , { typeof(long), JsonToken.Integer }
            };

        public OptionConverter()
        {
            var t = typeof(T);

            if (primitiveLookup.TryGetValue(t, out var value))
                expectedToken = value;
            else
                expectedToken = JsonToken.StartObject;  // assume object
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Option<T>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == expectedToken)
                return Option<T>.From((T)Convert.ChangeType(reader.Value, typeof(T)));
            else if (reader.TokenType == JsonToken.Null)
                return Option<T>.None();
            else
                return serializer.Deserialize(reader, objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var opt = value as Option<T>?;
            Debug.Assert(opt != null);

            var v = opt.Value.GetOrDefault();
            var token = Equals(v, null) ? NullToken : JToken.FromObject(v); 
            token.WriteTo(writer);
        }
    }
}
