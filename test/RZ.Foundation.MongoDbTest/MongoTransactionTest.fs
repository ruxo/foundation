namespace MongoDbTest

open FluentAssertions
open MongoDB.Driver
open Xunit
open RZ.Foundation.MongoDbTest
open TestSample
open MockDb
open RZ.Foundation.MongoDb
open Xunit.Abstractions


type ``Mongo transaction tests`` (output: ITestOutputHelper) =

    [<Fact>]
    let ``Add people with transaction`` () = task {
        use mdb = startTransactDb output

        let addCustomer() = task {
            use transaction = mdb.Db.CreateTransaction()

            let customer = transaction.GetCollection<Customer>()

            // when
            let! _ = customer.Add(JohnDoe)
            let! _ = customer.Add(JaneDoe)
            do! transaction.Commit()
        }
        // when
        do! addCustomer()

        // then
        let! people = mdb.Db.GetCollection<Customer>().FindAsync(fun _ -> true).Retrieve(_.ToListAsync())
        people.Should().BeEquivalentTo([ JohnDoe; JaneDoe ]) |> ignore
    }

    [<Fact>]
    let ``Auto rollback if no explicit commit`` () = task {
        use mdb = startTransactDb output

        let addCustomer() = task {
            use transaction = mdb.Db.CreateTransaction()

            let customer = transaction.GetCollection<Customer>()

            // when
            let! _ = customer.Add(JohnDoe)
            let! _ = customer.Add(JaneDoe)
            ()
        }
        // when
        do! addCustomer()

        // then
        let! people = mdb.Db.GetCollection<Customer>().FindAsync(fun _ -> true).Retrieve(_.ToListAsync())
        people.Count.Should().Be(0) |> ignore
    }