using System.Text;
using JetBrains.Annotations;
using RZ.Foundation.Types;

namespace RZ.Foundation.MongoDb;

[PublicAPI]
public readonly record struct MongoConnectionString(
    string Scheme,
    string HostAndPort,
    string? AuthDatabase,
    Map<CaseInsensitiveString, string> Options)
{
    static readonly CaseInsensitiveString AuthSourceOption = "authSource";

    public static MongoConnectionString? From(string connectionString)
        // ref: https://www.mongodb.com/docs/manual/reference/connection-string/
        => FindStartIndex(connectionString, "://").Select(schemaSeparator => {
            var scheme = connectionString[..schemaSeparator];
            var hostBegin = schemaSeparator + 3;
            var pathStart = FindStartIndex(connectionString, "/", hostBegin);
            var optionStart = FindStartIndex(connectionString, "?", hostBegin);
            var hostEnd = Math.Min(pathStart ?? connectionString.Length, optionStart ?? connectionString.Length);
            var host = connectionString[hostBegin..hostEnd];
            var authDatabaseStart = (pathStart ?? hostEnd - 1) + 1;
            var authDatabase = connectionString.Substring(authDatabaseStart, (optionStart ?? connectionString.Length) - authDatabaseStart);
            var validOptions = optionStart.Select(i => ExtractOptionString(connectionString[(i + 1)..]))
                            ?? LanguageExt.Map.empty<CaseInsensitiveString, string>();
            return new MongoConnectionString(scheme, host, string.IsNullOrEmpty(authDatabase) ? null : authDatabase, validOptions);
        });

    public string? AuthSource => Options.Find(AuthSourceOption).ToNullable();

    public override string ToString() {
        var sb = new StringBuilder(256);
        sb.Append(Scheme);
        sb.Append("://");
        sb.Append(HostAndPort);
        AuthDatabase.Iter(db => sb.Append($"/{db}"));
        var options = Options.ToSeq().Map(kv => $"{kv.Key}={kv.Value}").Join('&');
        if (options.Length > 0) sb.Append($"?{options}");
        return sb.ToString();
    }

    static Map<CaseInsensitiveString, string> ExtractOptionString(string options) =>
        options.Split('&')
               .Fold(LanguageExt.Map.empty<CaseInsensitiveString, string>(),
                     (last, current) => ExtractOption(current).Select(opt => last.Add(new(opt.Key), opt.Value)) ?? last);

    static (string Key, string Value)? ExtractOption(string s)
        => from separator in FindStartIndex(s, "=")
           select (s[..separator], s[(separator + 1)..]);

    [Pure]
    static int? FindStartIndex(string s, string text, int start = 0) {
        var i = s.IndexOf(text, start, StringComparison.Ordinal);
        return i == -1 ? null : i;
    }
}