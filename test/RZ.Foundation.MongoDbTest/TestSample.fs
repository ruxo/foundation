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

let JohnDoe = {
     Id= Guid "0B8D9631-720A-46B7-8C95-F55B4EC520A4"
     Name= "John Doe"
     Address= { Country= "TH"; Zip= "11111" }
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

type TestSampleHelpers =
    [<Extension>]
    static member ImportSamples(collection: IMongoCollection<Customer>) =
        collection.InsertMany([ JohnDoe; JaneDoe; HelloWorld ])
