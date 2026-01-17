using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
// ReSharper disable InconsistentNaming

namespace RZ.Foundation.AOT;

[PublicAPI]
public static class Prelude
{
    #region LanguageExt forward

    public static readonly Unit unit = Unit.Default;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Seq<T> Seq<T>(IEnumerable<T> items) => LanguageExt.Prelude.Seq(items);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Seq<T> Seq<T>(params T[] p) => LanguageExt.Prelude.Seq(p);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Some<T>(T v) => LanguageExt.Prelude.Some(v);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Some<T>(T? v) where T: struct => LanguageExt.Prelude.Some(v);

    public static readonly OptionNone None = LanguageExt.Prelude.None;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Optional<T>(T? value) => LanguageExt.Prelude.Optional(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Optional<T>(T? value) where T: struct => LanguageExt.Prelude.Optional(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T identity<T>(T x) => x;

    #endregion

    [ExcludeFromCodeCoverage, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Func<T> Constant<T>(T x) => () => x;

    [ExcludeFromCodeCoverage, MethodImpl(MethodImplOptions.NoOptimization)]
    public static void Noop() { }

    [ExcludeFromCodeCoverage, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Func<T, T> SideEffect<T>(Action<T> f) => x => {
                                                               f(x);
                                                               return x;
                                                           };

    [ExcludeFromCodeCoverage, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Func<T, ValueTask<T>> SideEffectAsync<T>([InstantHandle] Func<T, ValueTask> f)
        => async x => {
            await f(x);
            return x;
        };

    [ExcludeFromCodeCoverage, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T SideEffect<T>(this T x, Action<T> f) {
        f(x);
        return x;
    }

    [ExcludeFromCodeCoverage, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<T> SideEffectAsync<T>(this T x, Func<T,ValueTask> f) {
        await f(x);
        return x;
    }

    [ExcludeFromCodeCoverage, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<Unit> BooleanToOption(bool b) => b ? unit : None;

    [ExcludeFromCodeCoverage, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Func<Unit> ToUnit<T>(Func<T> effect) =>
        () => {
            effect();
            return unit;
        };

    [ExcludeFromCodeCoverage, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Func<T, Unit> ToUnit<T>(Action<T> action) =>
        v => {
            action(v);
            return unit;
        };

    [ExcludeFromCodeCoverage, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Unit ToUnit(Action action) {
        action();
        return unit;
    }

    #region ReadOnly

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IReadOnlyDictionary<K,V> ReadOnly<K,V>(Dictionary<K,V> dict) where K: notnull => dict;

    #endregion
}