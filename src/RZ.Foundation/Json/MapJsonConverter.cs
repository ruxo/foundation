using System;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using LanguageExt;
using Map = LanguageExt.Map;

namespace RZ.Foundation.Json;

public sealed class MapJsonConverter : JsonConverterFactory
{
    public static readonly MapJsonConverter Default = new();
    
    static readonly Type MapType = typeof(Map<,>);
    public override bool CanConvert(Type typeToConvert) => 
        JsonConverterHelper.CanConvert(MapType, typeToConvert) && typeToConvert.GetGenericArguments().First() == typeof(string);

    static readonly Type MapSerializerType = typeof(MapSerializer<>);
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) {
        var t = MapSerializerType.MakeGenericType(typeToConvert.GetGenericArguments().Skip(1).ToArray());
        return (JsonConverter)Activator.CreateInstance(t)!;
    }

    sealed class MapSerializer<T> : JsonConverter<Map<string,T>>
    {
        public override Map<string,T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            Debug.Assert(reader.TokenType == JsonTokenType.StartObject);

            var map = Map.empty<string, T>();
            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject) {
                var key = reader.GetString()!;
                Debug.Assert(reader.Read());
                var value = JsonSerializer.Deserialize<T>(ref reader, options)!;
                map = map.Add(key, value);
            }
            return map;
        }
        
        public override void Write(Utf8JsonWriter writer, Map<string,T> value, JsonSerializerOptions options) {
            writer.WriteStartObject();
            foreach (var kv in value) {
                writer.WritePropertyName(kv.Key);
                JsonSerializer.Serialize(writer, kv.Value, options);
            }
            writer.WriteEndObject();
        }
    }
}