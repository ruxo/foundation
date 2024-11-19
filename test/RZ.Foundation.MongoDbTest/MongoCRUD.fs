namespace MongoDbTest.MongoCRUD

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
open Xunit.Abstractions

(************************* TEST STARTS HERE *****************************)
type Add(output) =
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
        use mdb = startDb output
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
        use mdb = startDb output
        let coll = mdb.Db.GetCollection<Customer>()
        let! _ = coll.Add(person)

        // then when inserting the same record the second time
        let result = Func<Task>(fun() -> coll.Add(person) :> Task)

        let! ``exception`` = result.Should().ThrowAsync<ErrorInfoException>()

        ``exception``.Which.Code.Should().Be(StandardErrorCodes.Duplication) |> ignore
    }

    [<Fact>]
    let ``Capture duplicated add error with TryAdd`` () = task {
        use mdb = startWithSample output

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
        use mdb = startWithSample output

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

type Retrieval(output: ITestOutputHelper) =
    [<Fact>]
    let ``Get the first customer who has Zip code = 11111`` () = task {
        use mdb = startDb output

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
        use mdb = startDb output

        let coll = mdb.Db.GetCollection<Customer>().ImportSamples()

        // when
        let! result = coll.FindAsync(fun x -> x.Address.Country = "TH").Retrieve(_.ToListAsync())

        // then
        result.Count.Should().Be(2) |> ignore

        let names = result |> Seq.map _.Name
        names.Should().BeEquivalentTo([JohnDoe.Name; JaneDoe.Name]) |> ignore
    }

type Update(output) =
    [<Fact>]
    let ``Update Jane Zip code`` () = task {
        use mdb = startWithSample output
        let customer = mdb.Db.GetCollection<Customer>()

        let time = Mock<TimeProvider>()
        time.Setup(fun x -> x.GetUtcNow()).Returns(NewYear2024) |> ignore

        // when
        let! jane = customer.GetById JaneDoe.Id
        let updated_jane = { jane with Customer.Address.Zip = "22222" }
        let! _ = customer.Update(updated_jane, clock = time.Object)

        // then
        let expected = { updated_jane with Updated = NewYear2024; Version = 3u }
        let! jane = customer.GetById JaneDoe.Id
        jane.Should().BeEquivalentTo(expected) |> ignore
    }

    [<Fact>]
    let ``Try updating Jane Zip code must succeed`` () = task {
        use mdb = startWithSample output
        let customer = mdb.Db.GetCollection<Customer>()

        let time = Mock<TimeProvider>()
        time.Setup(fun x -> x.GetUtcNow()).Returns(NewYear2024) |> ignore

        // when
        let! jane = customer.GetById JaneDoe.Id
        let updated_jane = { jane with Customer.Address.Zip = "22222" }
        let! result = customer.TryUpdate(updated_jane, clock = time.Object)

        // then
        result.IsSuccess.Should().BeTrue() |> ignore

        let expected = { updated_jane with Updated = NewYear2024; Version = 3u }
        let! jane = customer.GetById JaneDoe.Id
        jane.Should().BeEquivalentTo(expected) |> ignore
    }

    [<Fact>]
    let ``Update Jane Zip code with explicit version number`` () = task {
        use mdb = startWithSample output
        let customer = mdb.Db.GetCollection<Customer>()

        // when
        let updated_jane = { JaneDoe with Customer.Address.Zip = "22222" }
        let! _ = customer.Update(JaneDoe.Id, updated_jane, JaneDoe.Version)

        // then
        let! jane = customer.GetById JaneDoe.Id
        jane.Should().BeEquivalentTo(updated_jane) |> ignore
    }

    [<Fact>]
    let ``Update Jane Zip code with outdated explicit version number, results in race condition`` () = task {
        use mdb = startWithSample output
        let customer = mdb.Db.GetCollection<Customer>()

        // when
        let updated_jane = { JaneDoe with Customer.Address.Zip = "22222" }
        let action = Func<Task>(fun _ -> customer.Update(JaneDoe.Id, updated_jane, 123u))

        // then
        let! error = action.Should().ThrowAsync<ErrorInfoException>()

        error.Which.Code.Should().Be(StandardErrorCodes.RaceCondition) |> ignore
    }

    [<Fact>]
    let ``Try updating Jane Zip code with outdated explicit version number, results in race condition`` () = task {
        use mdb = startWithSample output
        let customer = mdb.Db.GetCollection<Customer>()

        let updated_jane = { JaneDoe with Customer.Address.Zip = "22222" }
        let! result = customer.TryUpdate(JaneDoe.Id, updated_jane, 123u)

        let is_failed, error, _ = result.IfFail()
        is_failed.Should().BeTrue() |> ignore
        error.Code.Should().Be(StandardErrorCodes.RaceCondition) |> ignore
    }

    [<Fact>]
    let ``Update Jane Zip code with the explicit (new) key and data's key mismatch, results in race condition error`` () = task {
        use mdb = startWithSample output
        let customer = mdb.Db.GetCollection<Customer>()

        let updated_jane = { JaneDoe with Customer.Address.Zip = "22222" }
        let action = Func<Task>(fun _ -> customer.Update(UnusedGuid1, updated_jane))

        let! error = action.Should().ThrowAsync<ErrorInfoException>()

        error.Which.Code.Should().Be(StandardErrorCodes.RaceCondition) |> ignore
    }

    [<Fact>]
    let ``Update Jane Zip code with the explicit (valid) key and data's key mismatch, results in database transaction error`` () = task {
        use mdb = startWithSample output
        let customer = mdb.Db.GetCollection<Customer>()

        let updated_jane = { JaneDoe with Customer.Address.Zip = "22222" }
        let action = Func<Task>(fun _ -> customer.Update(JohnDoe.Id, updated_jane))

        let! error = action.Should().ThrowAsync<ErrorInfoException>()

        error.Which.Code.Should().Be(StandardErrorCodes.DatabaseTransactionError) |> ignore
    }

    [<Fact>]
    let ``Update John zip code with his *unique* zip`` () = task {
        use mdb = startWithSample output
        let customer = mdb.Db.GetCollection<Customer>()

        let updated_john = { JohnDoe with Customer.Address.Zip = "22222" }
        let! _ = customer.Update(updated_john, fun x -> x.Address.Zip = "11111")

        let! john = customer.GetById JohnDoe.Id
        john.Should().BeEquivalentTo(updated_john) |> ignore
    }

    [<Fact>]
    let ``Update with multiple matches will result in ID overwritten which will fail`` () = task {
        use mdb = startWithSample output
        let customer = mdb.Db.GetCollection<Customer>()

        let action = Func<Task>(fun _ -> customer.Update(NewKid, fun x -> x.Address.Country = "TH"))

        let! error = action.Should().ThrowAsync<ErrorInfoException>()

        error.Which.Code.Should().Be(StandardErrorCodes.DatabaseTransactionError, "someone's ID was overwritten") |> ignore
    }

    [<Fact>]
    let ``Try updating with multiple matches will result in ID overwritten which will fail`` () = task {
        use mdb = startWithSample output
        let customer = mdb.Db.GetCollection<Customer>()

        let! result = customer.TryUpdate(NewKid, fun x -> x.Address.Country = "TH")

        let is_failed, error, _ = result.IfFail()
        is_failed.Should().BeTrue() |> ignore
        error.Code.Should().Be(StandardErrorCodes.DatabaseTransactionError, "someone's ID was overwritten") |> ignore
    }

type Upsert(output) =
    [<Fact>]
    let ``Upsert New Kid`` () = task {
        use mdb = startWithSample output
        let customer = mdb.Db.GetCollection<Customer>()

        let time = Mock<TimeProvider>()
        time.Setup(_.GetUtcNow()).Returns(NewYear2024) |> ignore

        // when
        let! result = customer.Upsert(NewKid, clock = time.Object)

        // then
        let expect = { NewKid with Updated = NewYear2024; Version = 2u }
        let! db = customer.GetById NewKid.Id
        let! cursor = customer.FindAsync(fun x -> x.Address.Country = "US")
        let! all_us_people = cursor.Retrieve(_.ToListAsync())
        result.Should().BeEquivalentTo(expect) |> ignore
        db.Should().BeEquivalentTo(expect) |> ignore
        all_us_people.Count.Should().Be(2) |> ignore
        all_us_people.Should().Contain(expect) |> ignore
    }

    [<Fact>]
    let ``Try upsert the existing Jane won't have any change and no error`` () = task {
        use mdb = startWithSample output
        let customer = mdb.Db.GetCollection<Customer>()

        // when
        let! result = customer.TryUpsert(JaneDoe)

        // then
        let is_success, _, _ = result.IfSuccess()
        is_success.Should().BeTrue() |> ignore

        let! all_th_people = customer.FindAsync(fun x -> x.Address.Country = "TH").Retrieve(_.ToListAsync())
        all_th_people.Count.Should().Be(2, "no new record was added") |> ignore
    }

    [<Fact>]
    let ``Upsert Jane Zip code`` () = task {
        use mdb = startWithSample output
        let customer = mdb.Db.GetCollection<Customer>()

        // when
        let! result = customer.Upsert(JaneDoe.Id, { JaneDoe with Customer.Address.Zip = "22222" })

        // then
        let expect = { JaneDoe with Customer.Address.Zip = "22222" }
        let! db = customer.GetById JaneDoe.Id
        result.Should().BeEquivalentTo(expect) |> ignore
        db.Should().BeEquivalentTo(expect) |> ignore
    }

    [<Fact>]
    let ``Upsert Jane Zip code with outdated explicit version number, results in **duplication**`` () = task {
        use mdb = startWithSample output
        let customer = mdb.Db.GetCollection<Customer>()

        // when
        let updated_jane = { JaneDoe with Customer.Address.Zip = "22222" }
        let action = Func<Task>(fun _ -> customer.Upsert(JaneDoe.Id, updated_jane, 123u))

        // then
        let! error = action.Should().ThrowAsync<ErrorInfoException>()

        error.Which.Code.Should().Be(StandardErrorCodes.Duplication) |> ignore // note that this is different from Update where it gets Race Condition!
    }

    [<Fact>]
    let ``Try upsert Jane Zip code with outdated explicit version number, results in **duplication**`` () = task {
        use mdb = startWithSample output
        let customer = mdb.Db.GetCollection<Customer>()

        // when
        let updated_jane = { JaneDoe with Customer.Address.Zip = "22222" }
        let! result = customer.TryUpsert(JaneDoe.Id, updated_jane, 123u)

        // then
        let is_failed, error, _ = result.IfFail()
        is_failed.Should().BeTrue() |> ignore
        error.Code.Should().Be(StandardErrorCodes.Duplication) |> ignore
    }

    [<Fact>]
    let ``Upsert John zip code with his *unique* zip must succeed`` () = task {
        use mdb = startWithSample output
        let customer = mdb.Db.GetCollection<Customer>()

        // when
        let updated_john = { JohnDoe with Customer.Address.Zip = "22222" }
        let! result = customer.Upsert(updated_john, fun x -> x.Address.Zip = "11111")

        let! john = customer.GetById JohnDoe.Id
        john.Should().BeEquivalentTo(updated_john) |> ignore
        result.Should().BeEquivalentTo(updated_john) |> ignore
    }

    [<Fact>]
    let ``Try upsert John zip code with his invalid zip will fail from inserting a duplicated record`` () = task {
        use mdb = startWithSample output
        let customer = mdb.Db.GetCollection<Customer>()

        // when
        let updated_john = { JohnDoe with Customer.Address.Zip = "22222" }
        let! result = customer.TryUpsert(updated_john, fun x -> x.Address.Zip = "99999")

        // then
        let is_failed, error, _ = result.IfFail()
        is_failed.Should().BeTrue() |> ignore
        error.Code.Should().Be(StandardErrorCodes.Duplication) |> ignore
    }

type Deletion (output) =
    [<Fact>]
    let ``Delete all customers!`` () = task {
        use mdb = startWithSample output
        let customer = mdb.Db.GetCollection<Customer>()

        // when
        do! customer.DeleteAll(fun _ -> true)

        // then
        let! people = customer.FindAsync(fun _ -> true).Retrieve(_.ToListAsync())
        people.Count.Should().Be(0) |> ignore
    }

    [<Fact>]
    let ``Delete Jane`` () = task {
        use mdb = startWithSample output
        let customer = mdb.Db.GetCollection<Customer>()

        // when
        do! customer.Delete(JaneDoe)

        // then
        let! jane = customer.GetById JaneDoe.Id
        jane.Should().BeNull() |> ignore
    }

    [<Fact>]
    let ``Delete with unique zip condition`` () = task {
        use mdb = startWithSample output
        let customer = mdb.Db.GetCollection<Customer>()

        // when
        do! customer.Delete(fun x -> x.Address.Zip = UniqueZip)

        // then
        let! john = customer.GetById JohnDoe.Id
        john.Should().BeNull() |> ignore
    }

    [<Fact>]
    let ``Delete with a specific key`` () = task {
        use mdb = startWithSample output
        let customer = mdb.Db.GetCollection<Customer>()

        // when
        do! customer.Delete(JohnDoe.Id)

        // then
        let! john = customer.GetById JohnDoe.Id
        john.Should().BeNull() |> ignore
    }

    [<Fact>]
    let ``Delete with a key and an invalid version, should have no effect`` () = task {
        use mdb = startWithSample output
        let customer = mdb.Db.GetCollection<Customer>()
        let customer_count = customer.CountDocuments(fun _ -> true)

        // when
        do! customer.Delete(JohnDoe.Id, 123u)

        // then
        let current_count = customer.CountDocuments(fun _ -> true)
        current_count.Should().Be(customer_count) |> ignore
    }

    [<Fact>]
    let ``Try deleting all customers!`` () = task {
        use mdb = startWithSample output
        let customer = mdb.Db.GetCollection<Customer>()

        // when
        let! result = customer.TryDeleteAll(fun _ -> true)

        // then
        result.IsSuccess.Should().BeTrue() |> ignore

        let! people = customer.FindAsync(fun _ -> true).Retrieve(_.ToListAsync())
        people.Count.Should().Be(0) |> ignore
    }

    [<Fact>]
    let ``Try deleting Jane`` () = task {
        use mdb = startWithSample output
        let customer = mdb.Db.GetCollection<Customer>()

        // when
        let! result = customer.TryDelete(JaneDoe)

        // then
        result.IsSuccess.Should().BeTrue() |> ignore

        let! jane = customer.GetById JaneDoe.Id
        jane.Should().BeNull() |> ignore
    }

    [<Fact>]
    let ``Try deleting with multiple matches, only (random) one is removed`` () = task {
        use mdb = startWithSample output
        let customer = mdb.Db.GetCollection<Customer>()

        // when
        let! result = customer.TryDelete(fun x -> x.Address.Zip = "10000")

        // then
        result.IsSuccess.Should().BeTrue() |> ignore

        let! people = customer.FindAsync(fun x -> x.Address.Zip = "10000").Retrieve(_.ToListAsync())
        people.Count.Should().Be(1) |> ignore
    }

    [<Fact>]
    let ``Try deleting with a specific key`` () = task {
        use mdb = startWithSample output
        let customer = mdb.Db.GetCollection<Customer>()

        // when
        let! result = customer.TryDelete(JohnDoe.Id)

        // then
        result.IsSuccess.Should().BeTrue() |> ignore
        let! john = customer.GetById JohnDoe.Id
        john.Should().BeNull() |> ignore
    }

    [<Fact>]
    let ``Try deleting with a key and an invalid version, should have no effect`` () = task {
        use mdb = startWithSample output
        let customer = mdb.Db.GetCollection<Customer>()
        let customer_count = customer.CountDocuments(fun _ -> true)

        // when
        let! result = customer.TryDelete(JohnDoe.Id, 123u)

        // then
        result.IsSuccess.Should().BeTrue() |> ignore
        let current_count = customer.CountDocuments(fun _ -> true)
        current_count.Should().Be(customer_count) |> ignore
    }