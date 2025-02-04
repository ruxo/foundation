using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using JetBrains.Annotations;

namespace RZ.Foundation.Helpers;

[PublicAPI]
public static class ClaimCollectionExtensions
{
    public static string? FindValueByPriority(this IEnumerable<Claim> claims, params string[] types)
        => types.Select(claims.FindFirstValue).FirstOrDefault(v => v is not null);

    public static string? FindFirstValue(this IEnumerable<Claim> claims, Func<Claim, bool> predicate)
        => claims.FirstOrDefault(predicate)?.Value;

    public static string? FindFirstValue(this IEnumerable<Claim> claims, string type)
        => claims.FirstOrDefault(c => c.Type == type)?.Value;

    public static string FirstValue(this IEnumerable<Claim> claims, Func<Claim, bool> predicate)
        => claims.First(predicate).Value;

    public static string FirstValue(this IEnumerable<Claim> claims, string type)
        => claims.First(c => c.Type == type).Value;
}