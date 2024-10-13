module MongoCRUD

open System
open System.Runtime.CompilerServices
open System.Threading
open FluentAssertions
open Mongo2Go
open Moq
open RZ.Foundation.MongoDb
open Xunit

MongoHelper.SetupMongoStandardMappings()

module MockDb =
    let mutable private locker = SpinLock()
    let mutable private mock_db = lazy MongoDbRunner.Start()
    let mutable private db_ref_count = 0

    type ITracker =
        inherit IDisposable
        abstract ConnectionString: string

    let private release() =
        let got_lock = ref false
        try
            db_ref_count <- db_ref_count - 1
            if db_ref_count = 0 then
                mock_db.Value.Dispose()
                mock_db <- lazy MongoDbRunner.Start()
        finally
            if got_lock.Value then locker.Exit()

    let get() =
        let got_lock = ref false
        try
            locker.Enter(got_lock)
            db_ref_count <- db_ref_count + 1
            { new ITracker with
                member this.ConnectionString = mock_db.Value.ConnectionString
                override this.Dispose() = release()
            }
        finally
            if got_lock.Value then locker.Exit()

[<Struct; IsReadOnly>]
type Address = { Country: string; Zip: string }

[<CLIMutable>]
type Customer = {
    mutable Id: Guid
    Name: string
    Address: Address
    Updated: DateTimeOffset
    Version: uint
}
with
    interface IHaveKey<Guid> with
        member this.Id with get() = this.Id and set v = this.Id <- v
    interface IHaveVersion with
        member this.Updated = this.Updated
        member this.Version = this.Version
    interface ICanUpdateVersion<Customer> with
        member this.WithVersion(updated, next) = { this with Updated = updated; Version = next }

type TestDbContext(connection, db_name) =
    inherit RzMongoDbContext(connection, db_name)

[<Fact>]
let ``Add single row and query`` () = task {
    let person = {
        Id=Guid.NewGuid()
        Name="John Doe"
        Address = { Country = "TH"; Zip="10000" }
        Updated = DateTimeOffset.MinValue
        Version = 0u
    }

    let mocked_now = DateTimeOffset(2024, 1, 31, 17, 0, 0, TimeSpan.Zero)
    let time_provider = Mock<TimeProvider>()
    time_provider.Setup(_.GetUtcNow()).Returns(mocked_now) |> ignore

    let expected = { person with Updated = mocked_now; Version = 1u }

    // when
    use mock_db = MockDb.get()
    let db = TestDbContext(MongoConnectionString.From(mock_db.ConnectionString).Value, "test1")
    let coll = db.GetCollection<Customer>()
    let! _ = coll.Add(person, time_provider.Object)

    let! result = coll.GetById person.Id

    result.Should().BeEquivalentTo(expected) |> ignore
}
