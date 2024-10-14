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

    public string? DatabaseName => Options.Find(DatabaseOption).ToNullable() ?? AuthDatabase ?? AuthSource;

    public MongoConnectionString GetValidConnectionString()
        => this with { Options = Options.Remove(DatabaseOption) }; // remove non-standard option from connections

    public override string ToString() {
        var sb = new StringBuilder(256);
        sb.Append(Scheme);
        sb.Append("://");
        sb.Append(HostAndPort);
        if (AuthDatabase is not null)
            sb.Append($"/{AuthDatabase}");
        var options = Options.ToSeq().Map(kv => $"{kv.Key}={kv.Value}").Join('&');
        if (options.Length > 0) sb.Append($"?{options}");
        return sb.ToString();
    }

    /// Get database from the given <see cref="MongoConnectionString"/>, our explicit option (i.e. "database" option) comes first.
    /// Otherwise, we pick authentication source as the database.<br/>
    /// This method does not support different options in different connections. That is if the connection has multiple MongoDB nodes,
    /// and one or more node connections gives different database / authentication source, the picked database name cannot be determined.
    [Pure]
    public void Deconstruct(out MongoConnectionString connection, out string? database) {
        connection = GetValidConnectionString();
        database = DatabaseName;
    }

    static readonly CaseInsensitiveString DatabaseOption = "database";

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