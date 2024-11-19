namespace MongoDbTest

open System
open System.Reflection
open FluentAssertions
open MongoDB.Driver
open MongoDBMigrations
open RZ.Foundation
open RZ.Foundation.MongoDb
open RZ.Foundation.MongoDb.Migration
open RZ.Foundation.MongoDbTest
open RZ.Foundation.Types
open Xunit
open MockDb

type ``Migration tests``(output) =
    [<Fact>]
    let ``Initialize migration`` () =
        use mdb = startTransactDb output

        let client = MongoClient(mdb.ConnectionString)
        let migration = MigrationEngine()
                            .UseDatabase(client, "test")
                            .UseAssembly(Assembly.GetExecutingAssembly())
                            .UseSchemeValidation(enabled = false)

        migration.Run() |> ignore

        migration.Run(Version(0,0,1)) |> ignore

    [<Fact>]
    let ``Get ConnectionSettings from MongoConnectionString`` () =
        let mcs = MongoConnectionString.From "mongodb://localhost:27017/?database=test"
        let cs = AppSettings.From mcs.Value

        // then
        cs.Should().Be(ConnectionSettings("mongodb://localhost:27017", "test")) |> ignore

    [<Fact>]
    let ``Get ConnectionSettings from environment where nothing is set, will throw exception`` () =
        let action = Func<ConnectionSettings>(AppSettings.FromEnvironment)

        // then
        let error = action.Should().Throw<ErrorInfoException>()
        error.Which.Code.Should().Be(StandardErrorCodes.MissingConfiguration) |> ignore
