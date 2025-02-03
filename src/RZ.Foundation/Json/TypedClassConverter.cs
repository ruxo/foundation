using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace RZ.Foundation.Json;

[AttributeUsage(AttributeTargets.Class)]
public class JsonDerivedTypeAttribute(object value, string? propertyName = null) : Attribute
{
    public object Value => value;
    public string PropertyName => propertyName ?? "Type";
}

public class TypedClassConverter : JsonConverterFactory
{
    public readonly record struct PropInfo(string PropertyName, object Value, Func<object?, object?> Converter, Type TargetType);

    readonly FrozenDictionary<Type, PropInfo[]> derivedTypes;

    public TypedClassConverter(IEnumerable<Assembly>? assemblies = null) {
        var allTypes = (assemblies ?? [Assembly.GetEntryAssembly()!]).SelectMany(a => a.GetTypes());
        derivedTypes = (from t in allTypes
                        let attr = t.GetCustomAttribute<JsonDerivedTypeAttribute>()
                        where attr is not null

                        from @base in GetBaseTypes(t)

                        group new PropInfo(attr.PropertyName, attr.Value, GetConverter(attr.Value.GetType()), t) by @base into g
                        select g
                       ).ToFrozenDictionary(k => k.Key, v => v.ToArray());
    }

    public override bool CanConvert(Type typeToConvert)
        => derivedTypes.ContainsKey(typeToConvert);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) {
        var dt = derivedTypes[typeToConvert];
        return (JsonConverter)Activator.CreateInstance(typeof(JsonDerivedTypeConverter<>).MakeGenericType(typeToConvert), dt)!;
    }

    static IEnumerable<Type> GetBaseTypes(Type type) {
        var baseType = type.BaseType;
        if (baseType == typeof(object))
            yield return type;
        else
            do{
                Debug.Assert(baseType is not null);

                yield return baseType;
                baseType = baseType.BaseType;
            } while (baseType != typeof(object));
    }

    static Func<object?,object?> GetConverter(Type type) {
        // currently support only enum
        if (!type.IsEnum) return identity;

        return x => x switch {
            string s => Enum.TryParse(type, s, ignoreCase: true, out var v)? v : x,
            int i    => Enum.ToObject(type, i),

            _ => x
        };
    }

    public class JsonDerivedTypeConverter<TBase>(PropInfo[] derivedTypes) : JsonConverter<TBase>
    {
        public override TBase Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
           var json = JsonSerializer.Deserialize<JsonObject>(ref reader, options)!;
            var target = derivedTypes.First(x => {
                var propName = options.PropertyNamingPolicy?.ConvertName(x.PropertyName) ?? x.PropertyName;
                var jsonValue = json[propName]!.AsValue();
                var value = jsonValue.GetObjectValue();

                return x.Value.Equals(x.Converter(value));
            }).TargetType;
            if (target == typeof(TBase))
                throw new JsonException("Deserializing a base class is not supported!");

            return (TBase) json.Deserialize(target, options)!;
        }

        public override void Write(Utf8JsonWriter writer, TBase value, JsonSerializerOptions options) {
            throw new JsonException("Serializing a base class is not supported!");
        }
    }
}
