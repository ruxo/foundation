module RZ.Foundation.MongoDbTest.TestSample

open System
open System.Runtime.CompilerServices
open MongoDB.Driver
open RZ.Foundation.MongoDb

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

(****************************** DATA ******************************)
let UnusedGuid1 = Guid "503461E9-969B-4847-8CC7-F920370C39AB"
let UnusedGuid2 = Guid "1F9E1596-3484-44C2-B0C3-B55CF69CAAD1"

let NewYear2024 = DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero)

let UniqueZip = "11111"

let JohnDoe = {
     Id= Guid "0B8D9631-720A-46B7-8C95-F55B4EC520A4"
     Name= "John Doe"
     Address= { Country= "TH"; Zip=UniqueZip }   // His Zip must be unique, test cases assume it!
     Updated= DateTimeOffset(2020, 1, 1, 17, 0, 0, TimeSpan.Zero)
     Version= 1u
}

let JaneDoe = {
     Id= Guid "B823FD8C-C995-4B64-96FB-D83BEBAAD21D"
     Name= "Jane Doe"
     Address= { Country= "TH"; Zip= "10000" }
     Updated= DateTimeOffset(2020, 1, 31, 17, 0, 0, TimeSpan.Zero)
     Version= 2u
}

let HelloWorld = {
    Id= Guid "711ca94d-239c-4e67-81c9-1f2f155b3f43"
    Name = "Hello World"
    Address = { Country = "US"; Zip = "10000" }
    Updated = DateTimeOffset(2020, 2, 13, 17, 0, 0, TimeSpan.Zero)
    Version = 1u
}

/// New kid on the block, never be in the database before
let NewKid = {
    Id = Guid "BADA86E1-5EAD-4FAE-BDA6-D2C108A7BD9B"
    Name = "New Kid"
    Address = { Country = "US"; Zip = "10000" }
    Updated = DateTimeOffset(2020, 2, 13, 17, 0, 0, TimeSpan.Zero)
    Version = 1u
}

type TestSampleHelpers =
    [<Extension>]
    static member ImportSamples(collection: IMongoCollection<Customer>) =
        collection.InsertMany([ JohnDoe; JaneDoe; HelloWorld ])
        collection

    [<Extension>]
    static member ImportSamples(database: IMongoDatabase) =
        database.GetCollection<Customer>(nameof Customer).ImportSamples() |> ignore
        database
