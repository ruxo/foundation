using System;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace RZ.Foundation.Json;

static class JsonConverterHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CanConvert(Type functor, Type typeToConvert) =>
        typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == functor;

    public static JsonConverter CreateConverter(Type functor, Type typeToConvert) {
        var t = functor.MakeGenericType(typeToConvert.GetGenericArguments());
        return (JsonConverter)Activator.CreateInstance(t)!;
    }

    public static object? GetObjectValue(this JsonValue node) =>
        node.GetValueKind() switch {
            JsonValueKind.String => node.GetValue<string>(),
            JsonValueKind.Number => IsInteger(node.GetValue<double>())? node.GetValue<int>() : node.GetValue<double>(),
            JsonValueKind.True   => true,
            JsonValueKind.False  => false,
            JsonValueKind.Undefined or
                JsonValueKind.Null => null,
            _ => throw new NotSupportedException($"Unsupported JsonValueKind: {node.GetValueKind()}")
        };

    static bool IsInteger(double x) => Math.Abs(x - Math.Floor(x)) < 1e-9;
}