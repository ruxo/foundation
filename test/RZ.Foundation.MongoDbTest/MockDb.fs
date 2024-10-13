module RZ.Foundation.MongoDbTest.MockDb

open System
open System.Threading
open Mongo2Go

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
