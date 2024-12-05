// ReSharper disable CheckNamespace

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using LanguageExt.Common;
using RZ.Foundation.Types;
using PureAttribute = System.Diagnostics.Contracts.PureAttribute;

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

[PublicAPI]
public readonly struct Outcome<T> : IEquatable<Outcome<T>>
{
    readonly EitherStatus status = EitherStatus.IsBottom;

    Outcome(EitherStatus status, T? data, ErrorInfo? error)
        => (this.status, Data, Error) = (status, data, error);

    public Outcome(T data) : this(EitherStatus.IsRight, data, default) { }
    public Outcome(ErrorInfo error) : this(EitherStatus.IsLeft, default, error) { }

    public ErrorInfo? Error { get; init; }
    public T? Data { get; init; }

    public string State
    {
        get => status switch {
            EitherStatus.IsRight => Outcome.SuccessState,
            EitherStatus.IsLeft  => Outcome.FailState,
            _                    => Outcome.BottomState
        };

        init
        {
            status = value switch {
                Outcome.SuccessState => EitherStatus.IsRight,
                Outcome.FailState    => EitherStatus.IsLeft,
                Outcome.BottomState  => EitherStatus.IsBottom,
                _                    => throw new ArgumentOutOfRangeException(nameof(value), "Invalid state")
            };
        }
    }

    [Pure] public static implicit operator Outcome<T>(T value)         => new(value);
    [Pure] public static implicit operator Outcome<T>(ErrorInfo value) => new(value);
    [Pure] public static implicit operator Outcome<T>(Error value)     => new(ErrorFrom.Exception(value));

    [Pure]
    public static implicit operator Outcome<T>(Either<ErrorInfo, T> value)
        => value.Match(v => new Outcome<T>(v), e => new Outcome<T>(e));

    [Pure, JsonIgnore]
    public bool IsFail => status == EitherStatus.IsLeft;

    [Pure, JsonIgnore]
    public bool IsSuccess => status == EitherStatus.IsRight;

    [Pure]
    public Outcome<B> Bind<B>(Func<T, Outcome<B>> bind) =>
        status == EitherStatus.IsRight ? bind(Data!) : new Outcome<B>(Error!);

    [Pure]
    public Outcome<B> Map<B>(Func<T, B> map)
        => status == EitherStatus.IsRight ? new Outcome<B>(map(Data!)) : new Outcome<B>(Error!);

    [Pure]
    public Outcome<T> MapFailure(Func<ErrorInfo, ErrorInfo> map)
        => status == EitherStatus.IsRight ? Data! : map(Error!);

    [Pure]
    public Outcome<B> BiMap<B>(Func<T, B> success, Func<ErrorInfo, ErrorInfo> fail)
        => status == EitherStatus.IsRight ? new Outcome<B>(success(Data!)) : new Outcome<B>(fail(Error!));

    [Pure]
    public Either<E,B> MapToEither<B,E>(Func<T, B> success, Func<ErrorInfo, E> fail)
        => status == EitherStatus.IsRight ? success(Data!) : fail(Error!);

    [Pure]
    public Either<ErrorInfo, T> ToEither()
        => MapToEither(identity, identity);

    #region Pipe operators

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Outcome<T> operator |(Outcome<T> ma, in Outcome<T> mb) =>
        ma.status == EitherStatus.IsRight ? ma : mb;

    public static Outcome<T> operator |(Outcome<T> ma, OutcomeCatch<T> mb) =>
        ma.status == EitherStatus.IsRight ? ma : mb.Run(ma.Error!);

    public static Outcome<T> operator |(Outcome<T> ma, OutcomeSideEffect<T> sideEffect) =>
        ma.status == EitherStatus.IsRight ? ma.Map(sideEffect.Run) : ma;

    public static Outcome<T> operator |(Outcome<T> ma, OutcomeSideEffect sideEffect) =>
        ma.status == EitherStatus.IsRight ? ma : new(SideEffect<ErrorInfo>(v => sideEffect.Run(v))(ma.Error!));

    #endregion

    [Pure]
    public Outcome<T> Catch(Func<ErrorInfo, ErrorInfo> handler) =>
        status == EitherStatus.IsRight ? this : new Outcome<T>(handler(Error!));

    [Pure]
    public Outcome<T> Catch(Func<ErrorInfo, T> handler) =>
        status == EitherStatus.IsRight ? this : new Outcome<T>(handler(Error!));

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T IfFail(Func<ErrorInfo, T> mapper) => status == EitherStatus.IsRight ? Data! : mapper(Error!);

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T IfFail(T defaultValue) => status == EitherStatus.IsRight ? Data! : defaultValue;

    public Unit IfFail(Action<ErrorInfo> fail) {
        if (Error is not null) fail(Error);
        return unit;
    }

    public bool IfFail(out ErrorInfo e, out T v) {
        if (status == EitherStatus.IsRight){
            (e, v) = (default!, Data!);
            return false;
        }
        else{
            (e, v) = (Error!, default!);
            return true;
        }
    }

    public bool IfSuccess(out T v, out ErrorInfo e) {
        if (status == EitherStatus.IsRight){
            (e, v) = (default!, Data!);
            return true;
        }
        else{
            (e, v) = (Error!, default!);
            return false;
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public V Match<V>(Func<T, V> success, Func<ErrorInfo, V> fail) =>
        status == EitherStatus.IsRight ? success(Data!) : fail(Error!);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Unwrap() =>
        status == EitherStatus.IsRight ? Data! : JustThrow(Error!);

    [ExcludeFromCodeCoverage, MethodImpl(MethodImplOptions.AggressiveInlining)]
    static T JustThrow(ErrorInfo e) => e.Throw<T>();

    public ErrorInfo UnwrapError() =>
        status == EitherStatus.IsRight ? throw new InvalidOperationException("Outcome state is success") : Error!;

    [Pure, ExcludeFromCodeCoverage]
    public override string ToString() =>
        new StringBuilder(128)
           .Append("Outcome<").Append(typeof(T).Name).Append(">(")
           .Append(status).Append(": ")
           .Append(status == EitherStatus.IsRight ? Data : Error).Append(')')
           .ToString();

    #region Equality

    public bool Equals(Outcome<T> other)
        => status == other.status && Equals(Error, other.Error) && EqualityComparer<T?>.Default.Equals(Data, other.Data);

    public override bool Equals(object? obj)
        => obj is Outcome<T> other && Equals(other);

    [ExcludeFromCodeCoverage]
    public override int GetHashCode()
        => HashCode.Combine((int)status, Error, Data);

    [MethodImpl(MethodImplOptions.AggressiveInlining), ExcludeFromCodeCoverage]
    public static bool operator ==(Outcome<T> left, Outcome<T> right)
        => left.Equals(right);

    [MethodImpl(MethodImplOptions.AggressiveInlining), ExcludeFromCodeCoverage]
    public static bool operator !=(Outcome<T> left, Outcome<T> right)
        => !left.Equals(right);

    #endregion
}

public static class Outcome
{
    public const string SuccessState = "success";
    public const string FailState = "fail";
    public const string BottomState = "bottom";
}