using System;
using System.Runtime.InteropServices;

namespace RZ.Foundation
{
    public static class OptionHelper
    {
        public static Option<T> ToOption<T>(this T data) => data;

        public static Result<T, F> ToResult<T, F>(this Option<T> o, Func<F> none) => o.IsSome ? o.Get().AsSuccess<T,F>() : none();

        public static ApiResult<T> ToApiResult<T>(this Option<T> o, Func<Exception> none) => o.IsSome ? o.Get().AsApiSuccess() : none();
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

        public Option<TB> Chain<TB>(Func<T, Option<TB>> mapper) => isSome? mapper(value) : Option<TB>.None();
        public Option<T> IfNoneTry(Func<Option<T>> other) => isSome? this : other();

        public bool IsSome => isSome;
        public bool IsNone => !isSome;

        public void Apply(Action<T> handler)
        {
            if (isSome) handler(value);
        }

        public void Apply(Action noneHandler, Action<T> someHandler)
        {
            if (isSome) someHandler(value); else noneHandler();
        }

        public T Get() => isSome? value : throw new InvalidOperationException();
        public TResult Get<TResult>(Func<T, TResult> someHandler, Func<TResult> noneHandler) => isSome? someHandler(value) : noneHandler();
        [Obsolete("Use OrElse instead")]
        public T GetOrElse(Func<T> noneHandler) => isSome? value : noneHandler();
        public T OrElse(Func<T> noneHandler) => isSome? value : noneHandler();
        public T GetOrElse(T defaultValue) => isSome? value : defaultValue;
        public T GetOrDefault() => GetOrElse(default(T));
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
