module RZ.Foundation.MongoDbTest.MockDb

open System
open Mongo2Go
open RZ.Foundation.MongoDb
open TestSample

MongoHelper.SetupMongoStandardMappings()

type TestDbContext(connection, db_name) =
    inherit RzMongoDbContext(connection, db_name)

type MockedDatabase = {
    Server: MongoDbRunner
    Db: TestDbContext
}
with
    interface IDisposable with
        member this.Dispose() = this.Server.Dispose()

let startTransactDb db_name
    = let server = MongoDbRunner.Start(singleNodeReplSet = true)
      let db = TestDbContext(MongoConnectionString.From(server.ConnectionString).Value, db_name)
      { Server = server; Db = db }

let startDb db_name
    = let server = MongoDbRunner.Start()
      let db = TestDbContext(MongoConnectionString.From(server.ConnectionString).Value, db_name)
      { Server = server; Db = db }

let inline startWithSample() =
    let x = startDb "test"
    x.Db.GetCollection<Customer>().ImportSamples() |> ignore
    x
