using System.Reflection;
using JetBrains.Annotations;
using LanguageExt.UnitsOfMeasure;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MongoDBMigrations;
using RZ.AspNet;
using TCRB.Database.Mongo;
using Version = MongoDBMigrations.Version;

namespace RZ.Foundation.MongoDb.Migration;

[PublicAPI]
public static class MongoMigration
{
    const string DelayExitEnv = "DelayExit";
    const string UpgradeVersionEnv = "UpgradeVersion";

    public static void Start(IEnumerable<string> args) => Start(Seq(args));
    public static void Start(Seq<string> args) => Start(args, "MongoDb");

    public static void Start(Seq<string> args, string? connectionName) {
        var config = AspHost.CreateDefaultConfigurationSettings();
        var version = args.HeadOrNone().OrElse(() => Optional(config[UpgradeVersionEnv])).Map(ParseVersion);

        var verbose = args.Contains("-v");

        var connectionSettings =
            (from cn in Optional(connectionName)
             from cs in Optional(config.GetConnectionString(cn))
             let mcs = MongoConnectionString.From(cs) ?? throw new ArgumentException("Invalid connection string", cn)
             from settings in Optional(AppSettings.From(mcs))
             select settings
            ).IfNone(AppSettings.GetConnectionSettings);

        if (verbose)
            Console.WriteLine("Connection: {0}", connectionSettings.ConnectionString);

        Console.WriteLine("Database  : {0}", connectionSettings.DatabaseName);
        Console.WriteLine("Migrating to version: {0}", version.Map(v => v.ToString()).IfNone("latest"));

        var client = new MongoClient(connectionSettings.ConnectionString);

        var migration = new MigrationEngine()
                       .UseDatabase(client, connectionSettings.DatabaseName)
                       .UseAssembly(Assembly.GetEntryAssembly()!)
                       .UseSchemeValidation(false);

        version.Then(v => migration.Run(v), () => migration.Run());

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