module MongoCRUD

open System
open System.Runtime.CompilerServices
open System.Threading.Tasks
open FluentAssertions
open Mongo2Go
open MongoDB.Driver
open Moq
open RZ.Foundation
open RZ.Foundation.MongoDb
open RZ.Foundation.Types
open Xunit

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

let startDb db_name
    = let server = MongoDbRunner.Start()
      let db = TestDbContext(MongoConnectionString.From(server.ConnectionString).Value, db_name)
      { Server = server; Db = db }

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
    use mdb = startDb "test1"
    let coll = mdb.Db.GetCollection<Customer>()
    let! _ = coll.Add(person, time_provider.Object)

    let! result = coll.GetById person.Id

    result.Should().BeEquivalentTo(expected) |> ignore
}

[<Fact>]
let ``Repeatedly add the same single row will throw`` () = task {
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

    // when
    use mdb = startDb "test2"
    let coll = mdb.Db.GetCollection<Customer>()
    let! _ = coll.Add(person, time_provider.Object)

    // then when inserting the same record the second time
    let result = Func<Task>(fun() -> coll.Add(person, time_provider.Object) :> Task)

    let! ``exception`` = result.Should().ThrowAsync<ErrorInfoException>()

    ``exception``.Which.Code.Should().Be(StandardErrorCodes.Duplication) |> ignore
}

[<Fact>]
let ``Get the first customer who has Zip code = 11111`` () = task {
    use mdb = startDb "test3"
    mdb.Server.Import("test3", "Customer", "customer.jsonrow", drop=true)

    let coll = mdb.Db.GetCollection<Customer>()

    // when
    let! result = coll.Get(fun x -> x.Address.Zip = "11111")

    // then
    result.Should().BeEquivalentTo({
        Id=Guid("0B8D9631-720A-46B7-8C95-F55B4EC520A4")
        Name="John Doe"
        Address = { Country = "TH"; Zip="11111" }
        Updated = DateTimeOffset(2020, 1, 1, 17, 0, 0, TimeSpan.Zero)
        Version = 1u
    }) |> ignore
}

[<Fact>]
let ``Get all customers who has country = 'TH'`` () = task {
    use mdb = startDb "test4"
    mdb.Server.Import("test4", "Customer", "customer.jsonrow", drop=true)

    let coll = mdb.Db.GetCollection<Customer>()

    // when
    let! result = coll.FindAsync(fun x -> x.Address.Country = "TH").Retrieve(_.ToListAsync())

    // then
    result.Count.Should().Be(2) |> ignore

    let names = result |> Seq.map _.Name
    names.Should().BeEquivalentTo(["John Doe"; "Jane Doe"]) |> ignore
}