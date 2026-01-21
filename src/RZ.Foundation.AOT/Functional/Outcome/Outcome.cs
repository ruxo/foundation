// ReSharper disable CheckNamespace

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using LanguageExt.Common;

namespace RZ.Foundation;

public readonly struct OutcomeCatch<T>(Func<ErrorInfo, Outcome<T>> fail)
{
    public OutcomeCatch(Func<ErrorInfo, bool> predicate, Func<ErrorInfo, Outcome<T>> fail)
        : this(e => predicate(e) ? fail(e) : e) { }

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

    public string State {
        get => status switch {
            EitherStatus.IsRight => Outcome.SuccessState,
            EitherStatus.IsLeft  => Outcome.FailState,
            _                    => Outcome.BottomState
        };

        init {
            status = value switch {
                Outcome.SuccessState => EitherStatus.IsRight,
                Outcome.FailState    => EitherStatus.IsLeft,
                Outcome.BottomState  => EitherStatus.IsBottom,
                _                    => throw new ArgumentOutOfRangeException(nameof(value), "Invalid state")
            };
        }
    }

    [Pure]
    public static implicit operator Outcome<T>(T value) => new(value);

    [Pure]
    public static implicit operator Outcome<T>(ErrorInfo value) => new(value);

    [Pure]
    public static implicit operator Outcome<T>(Error value) => new(ErrorFrom.Exception(value));

    public void Deconstruct(out ErrorInfo? error, out T? data) => (error, data) = (Error, Data);

    [Pure]
    public static implicit operator Outcome<T>(Either<ErrorInfo, T> value)
        => value.Match(v => new Outcome<T>(v), e => new Outcome<T>(e));

    [Pure, JsonIgnore] public bool IsFail => status == EitherStatus.IsLeft;

    [Pure, JsonIgnore] public bool IsSuccess => status == EitherStatus.IsRight;

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
    public Either<E, B> MapToEither<B, E>(Func<T, B> success, Func<ErrorInfo, E> fail)
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

    #region If Fail & Success

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T IfFail(Func<ErrorInfo, T> mapper) => status == EitherStatus.IsRight ? Data! : mapper(Error!);

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T IfFail(T defaultValue) => status == EitherStatus.IsRight ? Data! : defaultValue;

    public Unit IfFail(Action<ErrorInfo> fail) {
        if (Error is not null) fail(Error);
        return unit;
    }

    public bool IfFail([NotNullWhen(true)] out ErrorInfo? e) {
        e = status == EitherStatus.IsRight ? null : Error;
        return status == EitherStatus.IsLeft;
    }

    public bool UnlessSuccess([NotNullWhen(false)] out T? v) {
        v = status == EitherStatus.IsRight ? Data : default;
        return status == EitherStatus.IsLeft;
    }

    public bool IfSuccess([NotNullWhen(true)] out T? v) {
        v = status == EitherStatus.IsRight ? Data : default;
        return status == EitherStatus.IsRight;
    }

    public bool UnlessFail([NotNullWhen(false)] out ErrorInfo? e) {
        e = status == EitherStatus.IsRight ? null : Error;
        return status == EitherStatus.IsRight;
    }

    public bool IfFail([NotNullWhen(true)] out ErrorInfo? e, [NotNullWhen(false)] out T? v) {
        (e, v) = status == EitherStatus.IsRight ? (null, Data) : (Error, default(T));
        return status == EitherStatus.IsLeft;
    }

    public bool IfSuccess([NotNullWhen(true)] out T? v, [NotNullWhen(false)] out ErrorInfo? e) {
        (e, v) = status == EitherStatus.IsRight ? (null, Data) : (Error, default(T));
        return status == EitherStatus.IsRight;
    }

    #endregion

    public V Match<V>(Func<T, V> success, Func<ErrorInfo, V> fail)
        => status == EitherStatus.IsRight ? success(Data!) : fail(Error!);

    #region Unwrap

    public T Unwrap()
        => status == EitherStatus.IsRight ? Data! : JustThrow(Error!);

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T? UnwrapOrDefault(T? defaultValue = default)
        => Data;

    [ExcludeFromCodeCoverage, MethodImpl(MethodImplOptions.AggressiveInlining)]
    static T JustThrow(ErrorInfo e) => e.Throw<T>();

    public ErrorInfo UnwrapError() =>
        status == EitherStatus.IsRight ? throw new InvalidOperationException("Outcome state is success") : Error!;

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ErrorInfo? UnwrapErrorOrDefault(ErrorInfo? defaultValue = null)
        => Error;

    #endregion

    #region NotFound related

    [Pure]
    public bool IsNotFound() => Error?.IsNotFound() ?? false;

    public Outcome<T> IfNotFound(T value)                => IsNotFound() ? value : this;
    public Outcome<T> IfNotFound(Outcome<T> @else)       => IsNotFound() ? @else : this;
    public Outcome<T> IfNotFound(Func<Outcome<T>> @else) => IsNotFound() ? @else() : this;

    public async ValueTask<Outcome<T>> IfNotFoundAsync(ValueTask<Outcome<T>> @else)       => IsNotFound() ? await @else : this;
    public async ValueTask<Outcome<T>> IfNotFoundAsync(Func<ValueTask<Outcome<T>>> @else) => IsNotFound() ? await @else() : this;

    #endregion

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