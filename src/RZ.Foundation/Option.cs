using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using RZ.Foundation.Extensions;

namespace RZ.Foundation
{
    public static class OptionHelper
    {
        static readonly Exception DummyException = new Exception();

        public static Option<T> ToOption<T>(this T data) => data;
        public static Option<T> ToOption<T>(this T? data) where T : struct => data.HasValue? Option<T>.Some(data.Value) : None<T>();
        public static Option<T> None<T>() => Option<T>.None();

        public static Result<T, F> ToResult<T, F>(this Option<T> o, Func<F> none) => o.IsSome ? o.Get().AsSuccess<T,F>() : none();

        public static ApiResult<T> ToApiResult<T>(this Option<T> o) => o.IsSome ? o.Get().AsApiSuccess() : DummyException;
        public static ApiResult<T> ToApiResult<T>(this Option<T> o, Func<Exception> none) => o.IsSome ? o.Get().AsApiSuccess() : none();

        public static Option<T> Join<T>(this Option<Option<T>> doubleOption) => doubleOption.Chain(i => i);

        public static Option<T> Call<A, B, T>(this Option<(A, B)> x, Func<A, B, T> f) => x.Map(p => p.CallFrom(f));
        public static Option<T> Call<A, B, C, T>(this Option<(A, B, C)> x, Func<A, B, C, T> f) => x.Map(p => p.CallFrom(f));

        public static Option<Unit> Call<A, B>(this Option<(A, B)> x, Action<A, B> f) => x.Map(p => p.CallFrom(f));
        public static Option<Unit> Call<A, B, C>(this Option<(A, B, C)> x, Action<A, B, C> f) => x.Map(p => p.CallFrom(f));

        [return: MaybeNull]
        public static T ToNullable<T>(this Option<T> opt) where T : class => opt.GetOrDefault();
    }

    public static class OptionNullableHelper
    {
        public static T? ToNullable<T>(this Option<T> opt) where T : struct => opt.Get(v => v, () => (T?) null);
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
        public static implicit operator Option<T> ([AllowNull] T value) => From(value);

        public Option<T> Where(Func<T, bool> predicate) => isSome && predicate(value) ? this : None();
        public async Task<Option<T>> WhereAsync(Func<T, Task<bool>> predicate) => isSome && await predicate(value) ? this : None();

        public Option<TB> Map<TB>(Func<T,TB> mapper) => isSome? mapper(value) : Option<TB>.None();
        public async Task<Option<TB>> MapAsync<TB>(Func<T, Task<TB>> mapper) => isSome? await mapper(value) : Option<TB>.None();

        public Option<TB> Chain<TB>(Func<T, Option<TB>> mapper) => isSome? mapper(value) : Option<TB>.None();
        public async Task<Option<TB>> ChainAsync<TB>(Func<T, Task<Option<TB>>> mapper) => isSome ? await mapper(value) : Option<TB>.None();

        public Option<T> OrElse(T elseValue) => isSome ? this : elseValue;
        public Option<T> OrElse(Option<T> elseValue) => isSome ? this : elseValue;
        public Option<T> OrElse(Func<Option<T>> elseFunc) => isSome ? this : elseFunc();
        public Task<Option<T>> OrElseAsync(Func<Task<Option<T>>> elseFunc) => isSome ? Task.FromResult(this) : elseFunc();

        public bool IsSome => isSome;
        public bool IsNone => !isSome;

        public Option<T> IfNone(Action handler) {
            if (IsNone) handler();
            return this;
        }

        public async Task<Option<T>> IfNoneAsync(Func<Task> handler) {
            if (IsNone) await handler();
            return this;
        }
        public Option<T> Then(Action<T> handler)
        {
            if (isSome) handler(value);
            return this;
        }
        public Option<T> Then( Action<T> someHandler, Action noneHandler)
        {
            if (isSome) someHandler(value); else noneHandler();
            return this;
        }

        public async Task<Option<T>> ThenAsync(Func<T, Task> handler) {
            if (isSome) await handler(value);
            return this;
        }

        public async Task<Option<T>> ThenAsync(Func<T, Task> someHandler, Func<Task> noneHandler) {
            if (isSome) await someHandler(value);
            else await noneHandler();
            return this;
        }

        public T Get() => isSome? value : throw new InvalidOperationException();
        public T GetOrThrow(Func<Exception> noneHandler) => isSome ? value : throw noneHandler();
        public TResult Get<TResult>(Func<T, TResult> someHandler, Func<TResult> noneHandler) => isSome? someHandler(value) : noneHandler();
        public async Task<TR> GetAsync<TR>(Func<T, Task<TR>> someHandler, Func<Task<TR>> noneHandler) => isSome ? await someHandler(value) : await noneHandler();

        public T GetOrElse(T defaultValue) => isSome? value : defaultValue;

        public T GetOrElse(Func<T> noneHandler) => isSome? value : noneHandler();
        public async Task<T> GetOrElseAsync(Func<Task<T>> noneHandler) => isSome ? value : await noneHandler();

        public T GetOrDefault(T def = default) => isSome? value : def;

        public Option<U> TryCast<U>() => (U) (object) value!;

        #region Equality

        public override bool Equals(object obj) => obj is Option<T> other && isSome == other.isSome && Equals(value, other.value);
        public override int GetHashCode() => isSome? value!.GetHashCode() : 0;

        #endregion

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

        public static async Task<Option<T>> SafeCallAsync(Func<Task<T>> handler) {
            try {
                return await handler();
            }
            catch (Exception) {
                return None();
            }
        }
        public static Option<T> From([AllowNull] T value) => Equals(value, null) ? None() : Some(value);
        public static Option<T> None() => NoneSingleton;
        public static Option<T> Some(T value) => new Option<T>(value);
    }

    [StructLayout(LayoutKind.Auto)]
    public struct OptionSerializable<T>
    {
        public bool HasValue;
        [AllowNull]
        public T Value;
        public OptionSerializable(Option<T> opt)
        {
            HasValue = opt.IsSome;
            Value = opt.Get(x => x, () => default!);
        }
        public Option<T> ToOption() => HasValue ? Value : Option<T>.None();
    }
}
