using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using RZ.Foundation.Extensions;

namespace RZ.Foundation
{
    public static class OptionHelper
    {
        static readonly Exception DummyException = new Exception();

        public static Option<T> ToOption<T>(this T data) => data;
        public static Option<T> None<T>() => Option<T>.None();

        public static Result<T, F> ToResult<T, F>(this Option<T> o, Func<F> none) => o.IsSome ? o.Get().AsSuccess<T,F>() : none();

        public static ApiResult<T> ToApiResult<T>(this Option<T> o) => o.IsSome ? o.Get().AsApiSuccess() : DummyException;
        public static ApiResult<T> ToApiResult<T>(this Option<T> o, Func<Exception> none) => o.IsSome ? o.Get().AsApiSuccess() : none();

        public static Option<(A, B)> With<A, B>(Option<A> a, Option<B> b) => a.Chain(ax => b.Map(bx => (ax, bx)));
        public static Option<(A, B, C)> With<A, B, C>(Option<A> a, Option<B> b, Option<C> c) =>
            a.Chain(ax => b.Chain(bx => c.Map(cx => (ax, bx,cx))));

        public static Option<T> Call<A, B, T>(this Option<(A, B)> x, Func<A, B, T> f) => x.Map(p => p.CallFrom(f));
        public static Option<T> Call<A, B, C, T>(this Option<(A, B, C)> x, Func<A, B, C, T> f) => x.Map(p => p.CallFrom(f));

        public static Option<Unit> Call<A, B>(this Option<(A, B)> x, Action<A, B> f) => x.Map(p => p.CallFrom(f));
        public static Option<Unit> Call<A, B, C>(this Option<(A, B, C)> x, Action<A, B, C> f) => x.Map(p => p.CallFrom(f));
    }
    public struct Option<T>
    {
        static readonly Option<T> NoneSingleton = new Option<T>();

        Option(T v)
        {
            isSome = true;
            value = v;
        }

        readonly bool isSome;
        readonly T value;
        public static implicit operator Option<T> (T value) => From(value);

        public Option<T> Where(Func<T, bool> predicate) => isSome && predicate(value) ? this : None();
        public Option<TB> Chain<TB>(Func<T, Option<TB>> mapper) => isSome? mapper(value) : Option<TB>.None();

        public Task<Option<TB>> ChainAsync<TB>(Func<T, Task<Option<TB>>> mapper) =>
            isSome ? mapper(value) : Task.FromResult(Option<TB>.None());
        public Option<T> OrElse(Func<Option<T>> elseFunc) => isSome ? this : elseFunc();

        public Task<Option<T>> OrElseAsync(Func<Task<Option<T>>> elseFunc) => isSome ? Task.FromResult(this) : elseFunc();

        [Obsolete("Use orElse")]
        public Option<T> IfNoneTry(Func<Option<T>> other) => isSome? this : other();

        public bool IsSome => isSome;
        public bool IsNone => !isSome;

        [Obsolete("Use Then instead")]
        public Option<T> Apply(Action<T> handler) => Then(handler);
        public Option<T> Then(Action<T> handler)
        {
            if (isSome) handler(value);
            return this;
        }

        [Obsolete("Use Then instead")]
        public void Apply(Action noneHandler, Action<T> someHandler) => Then(someHandler, noneHandler);
        public Option<T> Then( Action<T> someHandler, Action noneHandler)
        {
            if (isSome) someHandler(value); else noneHandler();
            return this;
        }

        public T Get() => isSome? value : throw new InvalidOperationException();
        public TResult Get<TResult>(Func<T, TResult> someHandler, Func<TResult> noneHandler) => isSome? someHandler(value) : noneHandler();
        public T GetOrElse(Func<T> noneHandler) => isSome? value : noneHandler();
        public T GetOrDefault(T def = default(T)) => isSome? value : def;
        public Option<TB> Map<TB>(Func<T, TB> mapper) => isSome? mapper(value) : Option<TB>.None();

        public Option<U> TryCast<U>()
        {
            if (!isSome) return Option<U>.None();
            if (Equals(value, null)) return Option<U>.Some(default(U));
            var converted = Convert.ChangeType(value, typeof(U));
            return Equals(converted, null)
                 ? Option<U>.None()
                 : Option<U>.Some((U)converted);
        }

        #region Equality
        public override bool Equals(object obj)
        {
            var other = obj as Option<T>?;
            return other != null && Equals(value, other.Value.value);
        }
        public override int GetHashCode() => isSome? value.GetHashCode() : 0;
        #endregion

        public T GetOrElse(T defaultValue) => isSome? value : defaultValue;

        public static Option<T> From(Func<T> initializer)
        {
            try
            {
                var result = initializer();
                return Equals(result, null) ? None() : Some(result);
            }
            catch (Exception)
            {
                return None();
            }
        }

        public static async Task<Option<T>> SafeCallAsync(Func<Task<Option<T>>> handler) {
            try {
                return await handler();
            }
            catch (Exception) {
                return None();
            }
        }
        public static Option<T> From(T value) => Equals(value, null) ? None() : Some(value);
        public static Option<T> None() => NoneSingleton;
        public static Option<T> Some(T value) => new Option<T>(value);
    }

    [StructLayout(LayoutKind.Auto)]
    public struct OptionSerializable<T>
    {
        public bool HasValue;
        public T Value;
        public OptionSerializable(Option<T> opt)
        {
            HasValue = opt.IsSome;
            Value = opt.Get(x => x, () => default(T));
        }
        public Option<T> ToOption() => HasValue ? Value : Option<T>.None();
    }
}
