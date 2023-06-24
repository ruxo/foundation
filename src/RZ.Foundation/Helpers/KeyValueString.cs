using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;

// ReSharper disable UnusedType.Global

namespace RZ.Foundation.Helpers;

/// <summary>
/// Represent a key/pair string.
///
/// <p>A key/pair string is a semi-colon separated of <c>key=value</c> strings. The value must be JSON encoded.</p>
/// </summary>
public static class KeyValueString
{
    /// <summary>
    /// Parse a string <paramref name="keyValueString"/> into a dictionary of key/value pairs.
    /// </summary>
    /// <param name="keyValueString"></param>
    /// <returns>A dictionary that contains key/value pairs of the given string.</returns>
    /// <exception cref="ArgumentException">Malformed key/value string</exception>
    public static IDictionary<string, string> Parse(string keyValueString) =>
        keyValueString.Split(';')
                      .ToSeq()
                      .Map(SplitPairs)
                      .ToDictionary(k => k.Item1, v => v.Item2);

    public static (string, string) SplitPairs(string s) {
        var separator = s.IndexOf('=');
        try {
            return separator == -1 ? (s,string.Empty) : (s[..separator], JsonNode.Parse($"\"{s[(separator+1)..]}\"")!.ToString());
        }
        catch (JsonException e) {
            throw new ArgumentException("Malformed key/JSON-value pairs", nameof(s), e);
        }
    }
}