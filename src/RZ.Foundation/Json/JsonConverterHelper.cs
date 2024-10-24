using System;
using System.Runtime.CompilerServices;
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
}