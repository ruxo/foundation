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

/// <summary>
/// Marks a concrete (derived) type as a polymorphic deserialization target for <see cref="TypedClassConverter"/>.
/// </summary>
/// <remarks>
/// <para>
/// Unlike <c>System.Text.Json</c>'s built-in <c>[JsonDerivedType]</c> (which is declared on the <em>base</em> type),
/// this attribute is placed on each <em>derived</em> type, so a hierarchy can be extended from any assembly without
/// modifying the base type. <see cref="TypedClassConverter"/> discovers every type carrying this attribute when it
/// scans the assemblies it is given.
/// </para>
/// <para>
/// The discriminator is an ordinary property already present on the model (for example an <c>enum</c> kind field),
/// rather than a synthetic <c>$type</c> metadata field, so it round-trips as normal data.
/// </para>
/// </remarks>
/// <param name="value">
/// The discriminator value that identifies this derived type. It is compared against the value of the JSON property
/// named by <paramref name="propertyName"/>. Enums (including members renamed with <c>[JsonStringEnumMemberName]</c>),
/// strings and integral numbers are supported.
/// </param>
/// <param name="propertyName">
/// The name of the JSON property that holds the discriminator. Defaults to <c>"Type"</c>. The active
/// <see cref="JsonNamingPolicy"/> (if any) is applied to this name when matching.
/// </param>
[AttributeUsage(AttributeTargets.Class)]
public class RzJsonDerivedTypeAttribute(object value, string? propertyName = null) : Attribute
{
    /// <summary>The discriminator value that identifies the annotated derived type.</summary>
    public object Value => value;

    /// <summary>The name of the JSON property holding the discriminator. Defaults to <c>"Type"</c>.</summary>
    public string PropertyName => propertyName ?? "Type";
}

/// <summary>
/// A <see cref="JsonConverterFactory"/> that deserializes polymorphic type hierarchies by matching a discriminator
/// property in the JSON against the concrete types marked with <see cref="RzJsonDerivedTypeAttribute"/>.
/// </summary>
/// <remarks>
/// <para>
/// Registration is <em>derived-side</em> and open: each concrete type self-registers via
/// <see cref="RzJsonDerivedTypeAttribute"/>, and the converter resolves it for every base type in its inheritance
/// chain (up to, but excluding, <see cref="object"/>). This supports hierarchies whose members live in different
/// assemblies.
/// </para>
/// <para>
/// The converter is read-only: serializing through a base type throws (write the concrete type instead, whose own
/// discriminator property round-trips as ordinary data).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var options = new JsonSerializerOptions {
///     Converters = { new TypedClassConverter([typeof(MyBase).Assembly]) }
/// };
/// var value = JsonSerializer.Deserialize&lt;MyBase&gt;(json, options); // returns the matching derived type
/// </code>
/// </example>
public class TypedClassConverter : JsonConverterFactory
{
    /// <summary>
    /// Discriminator metadata for a single registered derived type.
    /// </summary>
    /// <param name="PropertyName">Name of the JSON discriminator property.</param>
    /// <param name="Value">The discriminator value identifying <paramref name="TargetType"/>.</param>
    /// <param name="Converter">
    /// Normalises a raw JSON value before it is compared with <paramref name="Value"/> (for example, maps a JSON
    /// string or number to an enum). For non-enum discriminators this is the identity function.
    /// </param>
    /// <param name="TargetType">The concrete type to instantiate when the discriminator matches.</param>
    public readonly record struct PropInfo(string PropertyName, object Value, Func<object?, object?> Converter, Type TargetType);

    readonly FrozenDictionary<Type, PropInfo[]> derivedTypes;

    /// <summary>
    /// Scans the given assemblies for types marked with <see cref="RzJsonDerivedTypeAttribute"/> and builds the
    /// base-type to derived-type lookup used during conversion.
    /// </summary>
    /// <param name="assemblies">
    /// The assemblies to scan. When <c>null</c>, the entry assembly (<see cref="Assembly.GetEntryAssembly"/>) is used.
    /// </param>
    public TypedClassConverter(IEnumerable<Assembly>? assemblies = null) {
        var allTypes = (assemblies ?? [Assembly.GetEntryAssembly()!]).SelectMany(a => a.GetTypes());
        derivedTypes = (from t in allTypes
                        let attr = t.GetCustomAttribute<RzJsonDerivedTypeAttribute>()
                        where attr is not null

                        from @base in GetBaseTypes(t)

                        group new PropInfo(attr.PropertyName, attr.Value, GetConverter(attr.Value.GetType()), t) by @base into g
                        select g
                       ).ToFrozenDictionary(k => k.Key, v => v.ToArray());
    }

    /// <summary>Returns <c>true</c> for any base type that has at least one registered derived type.</summary>
    public override bool CanConvert(Type typeToConvert)
        => derivedTypes.ContainsKey(typeToConvert);

    /// <summary>Creates the polymorphic converter for <paramref name="typeToConvert"/>.</summary>
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

    /// <summary>
    /// The per-base-type converter that resolves and instantiates the concrete derived type for
    /// <typeparamref name="TBase"/> based on the JSON discriminator property.
    /// </summary>
    /// <typeparam name="TBase">The base type being deserialized.</typeparam>
    public class JsonDerivedTypeConverter<TBase>(PropInfo[] derivedTypes) : JsonConverter<TBase>
    {
        /// <summary>
        /// Reads the discriminator property and deserializes the JSON into the matching derived type.
        /// </summary>
        /// <exception cref="JsonException">
        /// No registered subtype matches the discriminator value, or the resolved type is the base type itself
        /// (deserializing a base class is not supported).
        /// </exception>
        public override TBase Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
           var json = JsonSerializer.Deserialize<JsonObject>(ref reader, options)!;
            var target = derivedTypes.FirstOrDefault(x => {
                var propName = options.PropertyNamingPolicy?.ConvertName(x.PropertyName) ?? x.PropertyName;
                if (json[propName] is not JsonValue jsonValue) // absent / non-scalar discriminator
                    return false;
                var value = jsonValue.GetObjectValue();

                return x.Value.Equals(x.Converter(value));
            }).TargetType;
            if (target is null)
                throw new JsonException("No registered subtype matches the discriminator value.");
            if (target == typeof(TBase))
                throw new JsonException("Deserializing a base class is not supported!");

            return (TBase) json.Deserialize(target, options)!;
        }

        /// <summary>Not supported — serialize the concrete derived type instead of the base type.</summary>
        /// <exception cref="JsonException">Always thrown.</exception>
        public override void Write(Utf8JsonWriter writer, TBase value, JsonSerializerOptions options) {
            throw new JsonException("Serializing a base class is not supported!");
        }
    }
}
