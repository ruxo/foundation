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
}