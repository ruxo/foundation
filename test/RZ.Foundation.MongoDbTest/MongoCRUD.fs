module MongoCRUD

open System
open System.Threading.Tasks
open FluentAssertions
open MongoDB.Driver
open Moq
open RZ.Foundation
open RZ.Foundation.MongoDb
open RZ.Foundation.MongoDbTest
open RZ.Foundation.Types
open Xunit
open MockDb
open TestSample

(************************* TEST STARTS HERE *****************************)
[<Fact>]
let ``Add single row and query`` () = task {
    let person = {
        Id=JohnDoe.Id
        Name="John Doe"
        Address = { Country = "TH"; Zip="10000" }
        Updated = DateTimeOffset(2024, 1, 31, 17, 0, 0, TimeSpan.Zero)
        Version = 0u
    }

    // when
    use mdb = startDb "test1"
    let coll = mdb.Db.GetCollection<Customer>()
    let! _ = coll.Add(person)

    let! result = coll.GetById person.Id

    result.Should().BeEquivalentTo(person) |> ignore
}

[<Fact>]
let ``Repeatedly add the same single row will throw`` () = task {
    let person = {
        Id=JohnDoe.Id
        Name="John Doe"
        Address = { Country = "TH"; Zip="10000" }
        Updated = DateTimeOffset(2024, 1, 31, 17, 0, 0, TimeSpan.Zero)
        Version = 0u
    }

    // when
    use mdb = startDb "test2"
    let coll = mdb.Db.GetCollection<Customer>()
    let! _ = coll.Add(person)

    // then when inserting the same record the second time
    let result = Func<Task>(fun() -> coll.Add(person) :> Task)

    let! ``exception`` = result.Should().ThrowAsync<ErrorInfoException>()

    ``exception``.Which.Code.Should().Be(StandardErrorCodes.Duplication) |> ignore
}

[<Fact>]
let ``Capture duplicated add error with TryAdd`` () = task {
    use mdb = startWithSample()

    let coll = mdb.Db.GetCollection<Customer>()

    // when
    let! result = coll.TryAdd({
        Id = Guid("711CA94D-239C-4E67-81C9-1F2F155B3F43")
        Name = "Example Name"
        Address = { Country = "TH"; Zip = "10000" }
        Updated = DateTimeOffset(2020, 1, 1, 17, 0, 0, TimeSpan.Zero)
        Version = 0u
    })

    // then
    let is_failed, error, _ = result.IfFail()
    is_failed.Should().BeTrue() |> ignore
    error.Code.Should().Be(StandardErrorCodes.Duplication) |> ignore
}

[<Fact>]
let ``Simple add with TryAdd`` () = task {
    use mdb = startWithSample()

    let coll = mdb.Db.GetCollection<Customer>()

    // when
    let! result = coll.TryAdd({
        Id = UnusedGuid1
        Name = "Testla Namera"
        Address = { Country = "XY"; Zip = "10000" }
        Updated = DateTimeOffset(2020, 1, 1, 17, 0, 0, TimeSpan.Zero)
        Version = 0u
    })

    // then
    let is_success, _, _ = result.IfSuccess()
    is_success.Should().BeTrue() |> ignore
}

[<Fact>]
let ``Get the first customer who has Zip code = 11111`` () = task {
    use mdb = startDb "test3"

    let coll = mdb.Db.GetCollection<Customer>().ImportSamples()

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

    let coll = mdb.Db.GetCollection<Customer>().ImportSamples()

    // when
    let! result = coll.FindAsync(fun x -> x.Address.Country = "TH").Retrieve(_.ToListAsync())

    // then
    result.Count.Should().Be(2) |> ignore

    let names = result |> Seq.map _.Name
    names.Should().BeEquivalentTo(["John Doe"; "Jane Doe"]) |> ignore
}

[<Fact>]
let ``Update the first customer who has lives in "TH"`` () = task {
    use mdb = startWithSample()
    let customer = mdb.Db.GetCollection<Customer>()

    let time = Mock<TimeProvider>()
    time.Setup(fun x -> x.GetUtcNow()).Returns(NewYear2024) |> ignore

    let! jane = customer.GetById JaneDoe.Id
    let updated_jane = { jane with Customer.Address.Zip = "22222" }
    let! _ = customer.Update(updated_jane, clock = time.Object)

    let expected = { updated_jane with Updated = NewYear2024; Version = 3u }
    let! jane = customer.GetById JaneDoe.Id
    jane.Should().BeEquivalentTo(expected) |> ignore
}