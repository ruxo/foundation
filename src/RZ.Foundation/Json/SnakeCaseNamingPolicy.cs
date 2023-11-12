using System.Text.Json;
using RZ.Foundation.Helpers;

namespace RZ.Foundation.Json;

/// <summary>
/// Allow JSON serialization/deserialization in snake_case
/// </summary>
public sealed class SnakeCaseNamingPolicy : JsonNamingPolicy
{
    public static readonly SnakeCaseNamingPolicy Instance = new();

    public override string ConvertName(string name) => 
        SnakeCase.ToSnakeCase(name);
}