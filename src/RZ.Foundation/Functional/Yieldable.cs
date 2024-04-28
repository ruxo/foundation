using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

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
    readonly Lazy<T> value = new(effect);

    public T Value => value.Value;
}

public readonly record struct ConstantAsyncYield<T>(ValueTask<T> Value) : AsyncYieldable<T>, HK<Asynchronous, T>
{
    public ConstantAsyncYield(T value) : this(new ValueTask<T>(value)) {}
}

public class Asynchronous : Functor<Asynchronous>, Monad<Asynchronous>, Eq<Asynchronous>
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
}

public class Synchronous : Functor<Synchronous>, Monad<Synchronous>, Eq<Synchronous>
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
}

public static class SynchronousExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AsyncYieldable<T> As<T>(this HK<Asynchronous, T> @yield) => (AsyncYieldable<T>) @yield;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Yieldable<T> As<T>(this HK<Synchronous, T> @yield) => (Yieldable<T>) @yield;
}

public interface Yieldable<out T>
{
    public T Value { get; }
}

public interface AsyncYieldable<T>
{
    public ValueTask<T> Value { get; }
}