// ReSharper disable CheckNamespace

using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;

namespace RZ.Foundation;

public readonly struct OutcomeAsync<T>
{
    readonly EitherAsync<Error, T> value;

    OutcomeAsync(EitherAsync<Error, T> value) => this.value = value;

    public static implicit operator OutcomeAsync<T>(T value) => new(value);
    public static implicit operator OutcomeAsync<T>(Error value) => new(value);
    public static implicit operator OutcomeAsync<T>(EitherAsync<Error, T> value) => new(value);

    internal EitherAsync<Error, T> Either => value;

    #region Pipe operators

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeAsync<T> operator |(OutcomeAsync<T> ma, Outcome<T> mb) =>
        ma.value.BindLeft(_ => mb.value.ToAsync());

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeAsync<T> operator |(OutcomeAsync<T> ma, OutcomeAsync<T> mb) =>
        // ma.value | mb.value; // bug in LanguageExt library, which has inconsistent behavior with Either
        ma.value.BindLeft(_ => mb.Either);

    public static OutcomeAsync<T> operator |(OutcomeAsync<T> ma, OutcomeAsyncCatch<T> mb) =>
        ma.value.BindLeft(e => mb.Run(e).Either);

    public static OutcomeAsync<T> operator |(OutcomeAsync<T> ma, OutcomeCatch<T> mb) =>
        ma.value.BindLeft(e => mb.Run(e).Either.ToAsync());

    public static OutcomeAsync<T> operator |(OutcomeAsync<T> ma, OutcomeSideEffect<T> sideEffect) =>
        ma.value.Map(e => {
                         sideEffect.Run(e);
                         return e;
                     });

    public static OutcomeAsync<T> operator |(OutcomeAsync<T> ma, OutcomeSideEffect sideEffect) =>
        ma.value.MapLeft(e => {
                             sideEffect.Run(e);
                             return e;
                         });

    public static OutcomeAsync<T> operator |(OutcomeAsync<T> ma, OutcomeAsyncSideEffect sideEffect) =>
        ma.value.MapLeftAsync(async e => {
                                  await sideEffect.Run(e);
                                  return e;
                              });

    public static OutcomeAsync<T> operator | (OutcomeAsync<T> ma, CatchValue<T> mb) =>
        ma.value.BindLeft(e => mb.Match(e)? SuccessOutcome(mb.Value(e)).Either.ToAsync() : e);

    public static OutcomeAsync<T> operator | (OutcomeAsync<T> ma, CatchError mb) =>
        ma.value.BindLeft(e => mb.Match(e)? FailedOutcome<T>(mb.Value(e)).Either.ToAsync() : e);

    #endregion

    public async Task<Outcome<T>> AsTask() => await value;

    public OutcomeAwaiter GetAwaiter() => new(value);

    #region Catch

    [Pure]
    public OutcomeAsync<T> Catch(Func<Error, T> handler) =>
        RightAsync<Error,T>(value.Match(identity, handler));

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public OutcomeAsync<T> Catch(Func<Error, Error> handler) =>
        value.BiMap(identity, handler);

    #endregion

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public OutcomeAsync<R> Map<R>(Func<T, R> map) =>
        value.Map(map);

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public OutcomeAsync<V> Select<V>(Func<T, V> map) =>
        value.Select(map);

    [Pure]
    public OutcomeAsync<V> SelectMany<U, V>(Func<T, OutcomeAsync<U>> bind, Func<T, U, V> project) =>
        value.SelectMany(x => bind(x).Either, project);

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<T> IfFail(Func<Error, T> mapper) => value.IfLeft(mapper);

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<T> IfFail(T defaultValue) => value.IfLeft(defaultValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<Unit> IfFail(Action<Error> fail) => value.IfLeft(fail);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<T> Unwrap() =>
        value.Match(identity, JustThrow);

    [ExcludeFromCodeCoverage, MethodImpl(MethodImplOptions.AggressiveInlining)]
    static T JustThrow(Error e) {
        e.Throw();
        return default!;
    }

    public Task<Error> UnwrapError() =>
        value.Match(_ => throw new InvalidOperationException("Outcome state is success"), identity);

    public readonly struct OutcomeAwaiter(EitherAsync<Error, T> source) : INotifyCompletion
    {
        readonly TaskAwaiter<Either<Error, T>> source = source.GetAwaiter();
        public bool IsCompleted => source.IsCompleted;

        public Outcome<T> GetResult() => source.GetResult();

        public void OnCompleted(Action continuation) =>
            source.OnCompleted(continuation);
    }
}

public readonly struct OutcomeAsyncCatch<T>(Func<Error, OutcomeAsync<T>> fail)
{
    public OutcomeAsyncCatch(Func<Error, bool> predicate, Func<Error, OutcomeAsync<T>> fail)
        : this(e => predicate(e) ? fail(e) : e){}

    public OutcomeAsync<T> Run(Error error) => fail(error);
}

public readonly struct OutcomeAsyncSideEffect(Func<Error, Task<Unit>> sideEffect)
{
    public Task<Unit> Run(Error error) => sideEffect(error);
}