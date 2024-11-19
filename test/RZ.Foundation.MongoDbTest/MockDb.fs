module RZ.Foundation.MongoDbTest.MockDb

open System
open System.Diagnostics
open System.Threading
open MongoSandbox
open RZ.Foundation.MongoDb
open TestSample
open Xunit.Abstractions

MongoHelper.SetupMongoStandardMappings()

let private mongo_options = MongoRunnerOptions(
    UseSingleNodeReplicaSet = true,
    StandardOuputLogger = (sprintf "| %s" >> Trace.WriteLine),
    StandardErrorLogger = (sprintf "ERR: %s" >> Trace.WriteLine),
    KillMongoProcessesWhenCurrentProcessExits = true
)
let private server = MongoRunner.Run mongo_options

type TestDbContext(connection, db_name) =
    inherit RzMongoDbContext(connection, db_name)

type MockedDatabase = {
    ConnectionString: string
    Db: TestDbContext
}
with
    interface IDisposable with
        member this.Dispose() = ()

let mutable db_count = 0

let startDb (_: ITestOutputHelper) =
    let id = Interlocked.Increment(&db_count)
    let db = TestDbContext(MongoConnectionString.From(server.ConnectionString).Value, $"test{id}")
    { ConnectionString=server.ConnectionString; Db = db }

let inline startTransactDb output =
    startDb output

let inline startWithSample output =
    let x = startDb output
    x.Db.GetCollection<Customer>().ImportSamples() |> ignore
    x
