namespace RZ.Foundation.MongoDbTest.MigrationTestScript

open MongoDBMigrations
open RZ.Foundation.Functional
open RZ.Foundation.MongoDb.Migration
open RZ.Foundation.MongoDbTest.TestSample

type TestMigration() =
    interface IMigration with
        member this.Version = Version(0,0,1)
        member this.Name = "Test migration"
        member this.Up db =
            let name = FSharp.ToExpression<Customer, obj>(_.Name)
            db.Build<Customer>()
              .WithSchema(Migration.Validation.Requires<Customer>())
              .Index("Name", _.Ascending(name))
              .Run()
        member this.Down db =
            db.DropCollection(nameof Customer)

type MigrationStep2() =
    interface IMigration with
        member _.Version = Version(0,0,2)
        member _.Name = "Migration step 2"
        member _.Up db =
            db.Collection<Customer>().DropIndex("Name") |> ignore
        member _.Down db =
            db.Collection<Customer>().CreateUniqueIndex("name", _.Ascending(FSharp.ToExpression<Customer, obj>(_.Name))) |> ignore
