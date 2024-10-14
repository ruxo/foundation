using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using JetBrains.Annotations;
using LanguageExt.UnitsOfMeasure;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MongoDBMigrations;
using RZ.AspNet;
using Version = MongoDBMigrations.Version;

namespace RZ.Foundation.MongoDb.Migration;

[PublicAPI, ExcludeFromCodeCoverage]
public static class MongoMigration
{
    const string DelayExitEnv = "DelayExit";
    const string UpgradeVersionEnv = "UpgradeVersion";

    public static void Start(string connection, Version? version = null) {
        var mcs = MongoConnectionString.From(connection) ?? throw new ArgumentException("Invalid Mongo connection string", connection);
        Start(mcs, version);
    }

    public static void Start(MongoConnectionString mcs, Version? version = null) {
        var connectionSettings = AppSettings.From(mcs) ?? AppSettings.FromEnvironment();

        Console.WriteLine("Database  : {0}", connectionSettings.DatabaseName);
        Console.WriteLine("Migrating to version: {0}", version?.ToString() ?? "latest");

        var client = new MongoClient(connectionSettings.ConnectionString);

        var migration = new MigrationEngine()
                       .UseDatabase(client, connectionSettings.DatabaseName)
                       .UseAssembly(Assembly.GetEntryAssembly()!)
                       .UseSchemeValidation(false);

        if (version is null)
            migration.Run();
        else
            migration.Run(version);
    }

    public static void Start(IEnumerable<string> args, string connectionName = "MongoDb") {
        var config = AspHost.CreateDefaultConfigurationSettings();
        var version = (args.FirstOrDefault() ?? config[UpgradeVersionEnv])?.Apply(ParseVersion);

        var cs = config.GetConnectionString(connectionName) ?? throw new ArgumentException("Invalid connection string name", connectionName);
        Start(cs, version);

        var delay = Optional(config[DelayExitEnv]).Bind(s => int.TryParse(s, out var v) ? Some(v) : None);
        delay.Iter(d => {
            Console.WriteLine("Delay for {0} seconds...", d);
            Thread.Sleep(d.Seconds());
        });
        Console.WriteLine("End migration.");
    }

    static Version ParseVersion(string s) {
        var parts = s.Split('.');
        return new(NumAt(0), NumAt(1), NumAt(2));

        int NumAt(int pos) => int.Parse(parts[pos]);
    }
}