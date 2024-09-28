// ReSharper disable CheckNamespace

using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Text;
using LanguageExt.Common;
using RZ.Foundation.Types;

namespace RZ.Foundation;

public readonly struct OutcomeCatch<T>(Func<ErrorInfo, Outcome<T>> fail)
{
    public OutcomeCatch(Func<ErrorInfo, bool> predicate, Func<ErrorInfo, Outcome<T>> fail)
        : this(e => predicate(e) ? fail(e) : e) {
    }

    public Outcome<T> Run(ErrorInfo error) => fail(error);
}

public readonly struct OutcomeSideEffect<T>(Func<T, Unit> sideEffect)
{
    public T Run(T data) {
        sideEffect(data);
        return data;
    }
}

public readonly struct OutcomeSideEffect(Func<ErrorInfo, Unit> sideEffect)
{
    public Unit Run(ErrorInfo error) => sideEffect(error);
}

public readonly struct Outcome<T>
{
    readonly ErrorInfo? error;
    readonly T? data;
    readonly EitherStatus status = EitherStatus.IsBottom;

    Outcome(EitherStatus status, T? data, ErrorInfo? error)
        => (this.status, this.data, this.error) = (status, data, error);

    Outcome(T data) : this(EitherStatus.IsRight, data, default) { }
    Outcome(ErrorInfo error) : this(EitherStatus.IsLeft, default, error) { }

    [Pure]
    public static implicit operator Outcome<T>(T value)         => new(value);

    [Pure]
    public static implicit operator Outcome<T>(ErrorInfo value) => new(value);

    [Pure]
    public static implicit operator Outcome<T>(Error value)     => new(ErrorFrom.Exception(value));

    [Pure]
    public static implicit operator Outcome<T>(Either<ErrorInfo, T> value)
        => value.Match(v => new Outcome<T>(v), e => new Outcome<T>(e));

    [Pure]
    public bool IsFail => status == EitherStatus.IsLeft;

    [Pure]
    public bool IsSuccess => status == EitherStatus.IsRight;

    [Pure]
    public Outcome<B> Bind<B>(Func<T, Outcome<B>> bind) =>
        status == EitherStatus.IsRight ? bind(data!) : new Outcome<B>(error!);

    [Pure]
    public Outcome<B> Map<B>(Func<T, B> map)
        => status == EitherStatus.IsRight ? new Outcome<B>(map(data!)) : new Outcome<B>(error!);

    [Pure]
    public Outcome<T> MapFailure(Func<ErrorInfo, ErrorInfo> map)
        => status == EitherStatus.IsRight ? data! : map(error!);

    [Pure]
    public Outcome<B> BiMap<B>(Func<T, B> success, Func<ErrorInfo, ErrorInfo> fail)
        => status == EitherStatus.IsRight ? new Outcome<B>(success(data!)) : new Outcome<B>(fail(error!));

    [Pure]
    public Either<E,B> MapToEither<B,E>(Func<T, B> success, Func<ErrorInfo, E> fail)
        => status == EitherStatus.IsRight ? success(data!) : fail(error!);

    [Pure]
    public Either<ErrorInfo, T> ToEither()
        => MapToEither(identity, identity);

    #region Pipe operators

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Outcome<T> operator |(Outcome<T> ma, in Outcome<T> mb) =>
        ma.status == EitherStatus.IsRight ? ma : mb;

    public static Outcome<T> operator |(Outcome<T> ma, OutcomeCatch<T> mb) =>
        ma.status == EitherStatus.IsRight ? ma : mb.Run(ma.error!);

    public static Outcome<T> operator |(Outcome<T> ma, OutcomeSideEffect<T> sideEffect) =>
        ma.status == EitherStatus.IsRight ? ma.Map(sideEffect.Run) : ma;

    public static Outcome<T> operator |(Outcome<T> ma, OutcomeSideEffect sideEffect) =>
        ma.status == EitherStatus.IsRight ? ma : new(SideEffect<ErrorInfo>(v => sideEffect.Run(v))(ma.error!));

    #endregion

    [Pure]
    public Outcome<T> Catch(Func<ErrorInfo, ErrorInfo> handler) =>
        status == EitherStatus.IsRight ? this : new Outcome<T>(handler(error!));

    [Pure]
    public Outcome<T> Catch(Func<ErrorInfo, T> handler) =>
        status == EitherStatus.IsRight ? this : new Outcome<T>(handler(error!));

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T IfFail(Func<ErrorInfo, T> mapper) => status == EitherStatus.IsRight ? data! : mapper(error!);

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T IfFail(T defaultValue) => status == EitherStatus.IsRight ? data! : defaultValue;

    public Unit IfFail(Action<ErrorInfo> fail) {
        if (error is not null) fail(error);
        return unit;
    }

    public bool IfFail(out ErrorInfo e, out T v) {
        if (status == EitherStatus.IsRight){
            (e, v) = (default!, data!);
            return false;
        }
        else{
            (e, v) = (error!, default!);
            return true;
        }
    }

    public bool IfSuccess(out T v, out ErrorInfo e) {
        if (status == EitherStatus.IsRight){
            (e, v) = (default!, data!);
            return true;
        }
        else{
            (e, v) = (error!, default!);
            return false;
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public V Match<V>(Func<T, V> success, Func<ErrorInfo, V> fail) =>
        status == EitherStatus.IsRight ? success(data!) : fail(error!);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Unwrap() =>
        status == EitherStatus.IsRight ? data! : JustThrow(error!);

    [ExcludeFromCodeCoverage, MethodImpl(MethodImplOptions.AggressiveInlining)]
    static T JustThrow(ErrorInfo e) => e.Throw<T>();

    public ErrorInfo UnwrapError() =>
        status == EitherStatus.IsRight ? throw new InvalidOperationException("Outcome state is success") : error!;

    [Pure, ExcludeFromCodeCoverage]
    public override string ToString() =>
        new StringBuilder(128)
           .Append("Outcome<").Append(typeof(T).Name).Append(">(")
           .Append(status).Append(": ")
           .Append(status == EitherStatus.IsRight ? data : error).Append(')')
           .ToString();
}