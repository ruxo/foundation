using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LanguageExt.Common;

namespace RZ.Foundation.Functional;

public static class YieldableExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T RunIO<T>(this HK<Synchronous, T> ma) => ma.As().Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<T> RunIO<T>(this HK<Asynchronous, T> ma) => ma.As().Value;
}

public sealed record ConstantYield<T>(T Value) : Yieldable<T>, HK<Synchronous, T>;

public sealed class FunctionYield<T>(Func<T> effect) : Yieldable<T>, HK<Synchronous, T>
{
    public T Value => effect();
}

public readonly record struct ConstantAsyncYield<T>(ValueTask<T> Value) : AsyncYieldable<T>, HK<Asynchronous, T>
{
    public ConstantAsyncYield(T value) : this(new ValueTask<T>(value)) {}
}

public readonly struct FunctionAsyncYield<T>(Func<ValueTask<T>> effect) : AsyncYieldable<T>, HK<Asynchronous, T>
{
    public ValueTask<T> Value => effect();
}

public class Asynchronous : IOT<Asynchronous>
{
    public static HK<Asynchronous, B> Map<A, B>(HK<Asynchronous, A> ma, Func<A, B> f) {
        return new ConstantAsyncYield<B>(ToMap());
        async ValueTask<B> ToMap() => f(await ma.As().Value);
    }

    public static HK<Asynchronous, T> Return<T>(T value) =>
        new ConstantAsyncYield<T>(value);

    public static HK<Asynchronous, B> Bind<A, B>(HK<Asynchronous, A> ma, Func<A, HK<Asynchronous, B>> f) {
        return new ConstantAsyncYield<B>(ToBind());
        async ValueTask<B> ToBind() => await f(await ma.As().Value).As().Value;
    }

    public static HK<Asynchronous, bool> EqualsTo<T>(HK<Asynchronous, T> a, HK<Asynchronous, T> b) {
        return new ConstantAsyncYield<bool>(eq());
        async ValueTask<bool> eq() {
            var va = await a.As().Value;
            var vb = await b.As().Value;
            var isNullEq = va is null && vb is null;
            return isNullEq || va?.Equals(vb) == true;
        }
    }

    public static HK<Asynchronous, bool> NotEqualsTo<T>(HK<Asynchronous, T> a, HK<Asynchronous, T> b) {
        return new ConstantAsyncYield<bool>(neq());
        async ValueTask<bool> neq() {
            var va = await a.As().Value;
            var vb = await b.As().Value;
            var isNullEq = va is null && vb is null;
            return !isNullEq && va?.Equals(vb) == false;
        }
    }

    public static HK<Asynchronous, Outcome<T>> Try<T>(Func<HK<Asynchronous, T>> f) where T : notnull {
        return new ConstantAsyncYield<Outcome<T>>(trap());
        async ValueTask<Outcome<T>> trap() {
            try {
                return await f().As().Value;
            }
            catch (Exception e) {
                return FailedOutcome<T>(Error.New(e));
            }
        }
    }
}

public class Synchronous : IOT<Synchronous>
{
    public static HK<Synchronous, B> Map<A, B>(HK<Synchronous, A> ma, Func<A, B> f) =>
        new ConstantYield<B>(f(ma.As().Value));

    public static HK<Synchronous, T> Return<T>(T value) =>
        new ConstantYield<T>(value);

    public static HK<Synchronous, B> Bind<A, B>(HK<Synchronous, A> ma, Func<A, HK<Synchronous, B>> f) =>
        f(ma.As().Value);

    #region Eq typeclass

    public static HK<Synchronous, bool> EqualsTo<T>(HK<Synchronous, T> a, HK<Synchronous, T> b) {
        var va = a.As().Value;
        var vb = b.As().Value;
        var isNullEq = va is null && vb is null;
        return new ConstantYield<bool>(isNullEq || va?.Equals(vb) == true);
    }

    public static HK<Synchronous, bool> NotEqualsTo<T>(HK<Synchronous, T> a, HK<Synchronous, T> b) {
        var va = a.As().Value;
        var vb = b.As().Value;
        var isNullEq = va is null && vb is null;
        return new ConstantYield<bool>(!isNullEq && va?.Equals(vb) == false);
    }

    #endregion

    public static HK<Synchronous, Outcome<T>> Try<T>(Func<HK<Synchronous, T>> f) where T : notnull {
        try {
            return Return(SuccessOutcome(f().As().Value));
        }
        catch (Exception e) {
            return Return(FailedOutcome<T>(Error.New(e)));
        }
    }
}

public static class SynchronousExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AsyncYieldable<T> As<T>(this HK<Asynchronous, T> @yield) => (AsyncYieldable<T>) @yield;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Yieldable<T> As<T>(this HK<Synchronous, T> @yield) => (Yieldable<T>) @yield;
}

public interface IOT<M> : Functor<M>, Monad<M>, Eq<M>, TryCatchType<M>
    where M : IOT<M>;


public interface TryCatchType<M> where M : TryCatchType<M>
{
    public static abstract HK<M, Outcome<T>> Try<T>(Func<HK<M, T>> f) where T : notnull;
}

public interface Yieldable<out T>
{
    public T Value { get; }
}

public interface AsyncYieldable<T>
{
    public ValueTask<T> Value { get; }
}