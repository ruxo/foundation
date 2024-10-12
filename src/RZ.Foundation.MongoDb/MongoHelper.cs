using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using MongoDB.Driver;
using RZ.Foundation.Types;

namespace RZ.Foundation.MongoDb;

[PublicAPI]
public static class MongoHelper
{
    [Pure]
    public static ErrorInfo? TryInterpretDatabaseError(Exception e)
        => e is MongoWriteException mongoException && mongoException.WriteError.Category == ServerErrorCategory.DuplicateKey
               ? new ErrorInfo(StandardErrorCodes.Duplication, "Either data identity, name, or both are already existed", e.ToString())
               : e is MongoException
                   ? new ErrorInfo(StandardErrorCodes.DatabaseTransactionError, e.Message, e.ToString())
                   : null;

    [Pure]
    public static ErrorInfo InterpretDatabaseError(Exception e)
        => TryInterpretDatabaseError(e) ?? ErrorFrom.Exception(e);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Outcome<T> Try<T>(Func<T> f) {
        try{
            return f();
        }
        catch (Exception e){
            return InterpretDatabaseError(e);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<Outcome<T>> TryAsync<T>(Func<ValueTask<T>> f) {
        try{
            return await f();
        }
        catch (Exception e){
            return InterpretDatabaseError(e);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<Outcome<Unit>> TryAsync(Func<ValueTask> f) {
        try{
            await f();
            return unit;
        }
        catch (Exception e){
            return InterpretDatabaseError(e);
        }
    }

    /// Get database from the given <see cref="MongoConnectionString"/>, our explicit option (i.e. "database" option) comes first.
    /// Otherwise, we pick authentication source as the database.<br/>
    /// This method does not support different options in different connections. That is if the connection has multiple MongoDB nodes,
    /// and one or more node connections gives different database / authentication source, the picked database name cannot be determined.
    [Pure]
    public static (MongoConnectionString Connection, string Database)? GetDatabaseNameFrom(MongoConnectionString cs) {
        var db = cs.Options.Find(DatabaseOption).ToNullable() ?? cs.AuthDatabase ?? cs.AuthSource;
        var fixedConnection = cs with {
            Options = cs.Options.Remove(DatabaseOption) // remove non-standard option from connections
        };
        return db is null ? null : (fixedConnection, db);
    }

    static readonly CaseInsensitiveString DatabaseOption = "database";
}