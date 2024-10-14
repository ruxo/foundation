using System.Text.Json;
using RZ.Foundation.Types;

namespace RZ.Foundation.MongoDb.Migration;

public record struct ConnectionSettings(string ConnectionString, string DatabaseName);

public static class AppSettings
{
    const string EnvConnectionString = "CS_CONNECTION";
    const string EnvDatabaseName = "CS_DATABASE";
    const string EnvFileConfig = "CS_CONFIGFILE";

    public static ConnectionSettings FromEnvironment() {
        var connection = GetEnv(EnvConnectionString);
        var dbName = GetEnv(EnvDatabaseName);
        var settings = from c in connection
                       from db in dbName
                       select new ConnectionSettings(c, db);
        var final = settings.OrElse(() => GetEnv(EnvFileConfig).Map(GetFromFile));
        return final
           .GetOrThrow(() => new ErrorInfoException(StandardErrorCodes.MissingConfiguration,
                                                    $"No connection settings in {EnvConnectionString}, {EnvDatabaseName}, or" +
                                                    $" {EnvFileConfig}"));
    }

    public static ConnectionSettings? From(MongoConnectionString connectionString) {
        var (connection, dbName) = connectionString;
        return dbName.ApplyValue(databaseName => new ConnectionSettings(connection.ToString(), databaseName));
    }

    static ConnectionSettings GetFromFile(string filename) =>
        JsonSerializer.Deserialize<ConnectionSettings>(File.ReadAllText(filename));

    static Option<string> GetEnv(string key) => Optional(Environment.GetEnvironmentVariable(key));
}