// ReSharper disable UnusedType.Global

namespace RZ.Foundation.Helpers;

/// <summary>
/// Represent a key/pair string.
///
/// <p>A key/pair string is a semi-colon separated of <c>key=value</c> strings. The value is taken
/// literally (everything after the first <c>=</c>), so it round-trips verbatim.</p>
/// </summary>
[PublicAPI]
public static class KeyValueString
{
    /// <summary>
    /// Parse a string <paramref name="keyValueString"/> into a dictionary of key/value pairs.
    /// </summary>
    /// <param name="keyValueString"></param>
    /// <returns>A dictionary that contains key/value pairs of the given string.</returns>
    public static IDictionary<string, string> Parse(string keyValueString) =>
        keyValueString.Split(';')
                      .ToSeq()
                      .Map(SplitPairs)
                      .ToDictionary(k => k.Item1, v => v.Item2);

    public static (string, string) SplitPairs(string s) {
        var separator = s.IndexOf('=');
        return separator == -1
                   ? (s, string.Empty)
                   : (s[..separator], s[(separator + 1)..]);
    }
}