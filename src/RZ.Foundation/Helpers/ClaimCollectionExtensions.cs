using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using JetBrains.Annotations;

namespace RZ.Foundation.Helpers;

[PublicAPI]
public static class ClaimCollectionExtensions
{
    extension(IEnumerable<Claim> claims)
    {
        public string? FindValueByPriority(params string[] types)
            => types.Select(claims.FindFirstValue).FirstOrDefault(v => v is not null);

        public string? FindFirstValue(Func<Claim, bool> predicate)
            => claims.FirstOrDefault(predicate)?.Value;

        public string? FindFirstValue(string type)
            => claims.FirstOrDefault(c => c.Type == type)?.Value;

        public string FirstValue(Func<Claim, bool> predicate)
            => claims.First(predicate).Value;

        public string FirstValue(string type)
            => claims.First(c => c.Type == type).Value;
    }
}