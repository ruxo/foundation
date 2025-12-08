using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RZ.Foundation.Types;

// ReSharper disable InconsistentNaming

// ReSharper disable CheckNamespace

namespace RZ.Foundation;

public static partial class Prelude
{
    #region ToOutcome

    [Pure]
    [PublicAPI]
    public static Outcome<T> ToOutcome<T>(this Option<T> opt, ErrorInfo? error = default)
        => opt.Match(v => (Outcome<T>)v, () => error ?? new(StandardErrorCodes.NotFound));

    [Pure]
    [PublicAPI]
    public static Outcome<T> ToOutcome<T>(this Either<ErrorInfo, T> opt) => opt.Match(v => (Outcome<T>)v, e => e);

    [Pure]
    [PublicAPI]
    public static Outcome<T> ToOutcome<T>(this Try<T> self) => self.ToEither(e => ErrorFrom.Exception(e)).ToOutcome();

    #endregion

    #region Try/Catch

    [PublicAPI]
    public static async ValueTask<Outcome<T>> TryCatch<T>([InstantHandle] Func<ValueTask<Outcome<T>>> handler) {
        try{
            return await handler();
        }
        catch (Exception e){
            return ErrorFrom.Exception(e);
        }
    }

    [PublicAPI]
    public static async ValueTask<Outcome<T>> TryCatch<T>([InstantHandle] Func<ValueTask<T>> handler) {
        try{
            return await handler();
        }
        catch (Exception e){
            return ErrorFrom.Exception(e);
        }
    }

    [PublicAPI]
    public static async ValueTask<Outcome<Unit>> TryCatch([InstantHandle] Func<ValueTask> handler) {
        try{
            await handler();
            return unit;
        }
        catch (Exception e){
            return ErrorFrom.Exception(e);
        }
    }

    [PublicAPI]
    public static Outcome<T> TryCatch<T>([InstantHandle] Func<Outcome<T>> handler) {
        try{
            return handler();
        }
        catch (Exception e){
            return ErrorFrom.Exception(e);
        }
    }

    public static Outcome<T> TryCatch<T>([InstantHandle] Func<T> handler) {
        try{
            return handler();
        }
        catch (Exception e){
            return ErrorFrom.Exception(e);
        }
    }

    public static Outcome<Unit> TryCatch([InstantHandle] Action handler) {
        try{
            handler();
            return unit;
        }
        catch (Exception e){
            return ErrorFrom.Exception(e);
        }
    }

    #endregion
}