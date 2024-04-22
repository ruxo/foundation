using System;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
// ReSharper disable CheckNamespace

namespace RZ.Foundation;

public static partial class Prelude
{
    public static OutcomeAsync<T> TryCatch<T>(Func<Task<Outcome<T>>> handler) =>
        from v in TryAsync(handler).ToOutcome()
        from result in v.ToAsync()
        select result;

    public static OutcomeAsync<T> TryCatch<T>(Func<Task<Either<Error, T>>> handler) =>
        from v in TryAsync(handler).ToEither()
        from result in v.ToAsync()
        select result;

    public static OutcomeAsync<T> TryCatch<T>(Func<Task<T>> handler) =>
        TryAsync(handler).ToOutcome();

    public static OutcomeAsync<Unit> TryCatch(Func<Task> handler) =>
        TryAsync(async () => {
                     await handler();
                     return Unit.Default;
                 }).ToEither();

    public static Outcome<T> TryCatch<T>(Func<Outcome<T>> handler) =>
        Try(handler).ToOutcome().Either.Bind(x => x.Either);

    public static Outcome<T> TryCatch<T>(Func<Either<Error, T>> handler) =>
        Try(handler).ToOutcome().Either.Bind(identity);

    public static Outcome<T> TryCatch<T>(Func<T> handler) =>
        Try(handler).ToEither(Error.New);

    public static Outcome<Unit> TryCatch(Action handler) =>
        Try(() => {
                handler();
                return unit;
            }).ToEither(Error.New);
}