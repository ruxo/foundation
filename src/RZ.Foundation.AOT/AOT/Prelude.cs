using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

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
    public static Option<T> Some<T>(T? v) where T : struct => LanguageExt.Prelude.Some(v);

    public static readonly OptionNone None = LanguageExt.Prelude.None;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Optional<T>(T? value) => LanguageExt.Prelude.Optional(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Optional<T>(T? value) where T : struct => LanguageExt.Prelude.Optional(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T identity<T>(T x) => x;

    #endregion

    [ExcludeFromCodeCoverage, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Func<T> Constant<T>(T x) => () => x;

    [ExcludeFromCodeCoverage, MethodImpl(MethodImplOptions.NoOptimization)]
    public static void Noop() { }

    [ExcludeFromCodeCoverage, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Func<T, T> SideEffect<T>([InstantHandle] Action<T> f) => x => {
        f(x);
        return x;
    };

    [ExcludeFromCodeCoverage, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Func<T, ValueTask<T>> SideEffectAsync<T>([InstantHandle] Func<T, ValueTask> f)
        => async x => {
            await f(x);
            return x;
        };

    extension<T>(T x)
    {
        [PublicAPI, ExcludeFromCodeCoverage, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T SideEffect([InstantHandle] Action<T> f) {
            f(x);
            return x;
        }

        [PublicAPI, ExcludeFromCodeCoverage, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<T> SideEffectAsync([InstantHandle] Func<T, ValueTask> f) {
            await f(x);
            return x;
        }
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

    public static string ToJson(params (string Key, JsonNode? Value)[] pairs) {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        writer.WriteStartObject();
        foreach (var (key, value) in pairs){
            writer.WritePropertyName(key);
            if (value is null)
                writer.WriteNullValue();
            else
                value.WriteTo(writer);
        }
        writer.WriteEndObject();
        writer.Flush();
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    #region With

    public static Option<(A, B)> With<A, B>(Option<A> a, Option<B> b) => a.Bind(ax => b.Map(bx => (ax, bx)));

    public static Option<(A, B, C)> With<A, B, C>(Option<A> a, Option<B> b, Option<C> c)
        => a.Bind(ax => b.Bind(bx => c.Map(cx => (ax, bx, cx))));

    public static Option<IReadOnlyList<T>> With<T>(IReadOnlyList<Option<T>> a)
        => IfSome(a.Aggregate(Some(new List<T>(a.Count)),
                              (result, x) => from list in result
                                             from v in x
                                             select list.SideEffect(l => l.Add(v))), out var final)
               ? final
               : None;

    public static Outcome<(A, B)> With<A, B>(Outcome<A> a, Outcome<B> b) => a.Bind(ax => b.Map(bx => (ax, bx)));

    public static Outcome<(A, B, C)> With<A, B, C>(Outcome<A> a, Outcome<B> b, Outcome<C> c)
        => a.Bind(ax => b.Bind(bx => c.Map(cx => (ax, bx, cx))));

    public static Outcome<IReadOnlyList<T>> With<T>(IReadOnlyList<Outcome<T>> a)
        => Fail(a.Aggregate(SuccessOutcome(new List<T>(a.Count)),
                            (result, x) => from list in result
                                           from v in x
                                           select list.SideEffect(l => l.Add(v))), out var e, out var final)
               ? e
               : final;

    #endregion

    #region ReadOnly

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IReadOnlyDictionary<K, V> ReadOnly<K, V>(Dictionary<K, V> dict) where K : notnull => dict;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IReadOnlyDictionary<K, V> ReadOnly<K, V>(FrozenDictionary<K, V> dict) where K : notnull => dict;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IReadOnlyList<T> ReadOnly<T>(IEnumerable<T> seq) => seq.ToArray();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IReadOnlyList<T> ReadOnly<T>(params T[] list) => list;

    #endregion

    #region On & Try

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OnHandlerSync On(Action task)
        => new(task);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OnHandlerSyncX<TX> On<TX>(TX x, Action<TX> task)
        => new(x, task);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OnHandlerSync<T> On<T>(Func<T> task)
        => new(task);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OnHandlerSync<TX, T> On<TX, T>(TX x, Func<TX, T> task)
        => new(x, task);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OnHandler On(Task task)
        => new(task);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OnHandlerX<TX> On<TX>(TX x, Func<TX, Task> task)
        => new(x, task);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OnHandler<TX, T> On<TX, T>(TX x, Func<TX, Task<T>> task)
        => new(x, task);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OnHandler<T> On<T>(Task<T> task)
        => new(task);

    #region Try

    public static (Exception? Error, Unit Value) Try<S>(S state, Action<S> f) {
        try{
            f(state);
            return (null, unit);
        }
        catch (Exception e){
            return (e, unit);
        }
    }

    public static (Exception? Error, Unit Value) Try(Action f) {
        try{
            f();
            return (null, unit);
        }
        catch (Exception e){
            return (e, unit);
        }
    }

    public static async ValueTask<(Exception? Error, T Value)> Try<T>(ValueTask<T> task) {
        try{
            return (null, await task);
        }
        catch (Exception e){
            return (e, default!);
        }
    }

    public static async ValueTask<(Exception? Error, Unit Value)> Try(ValueTask task) {
        try{
            await task;
            return (null, unit);
        }
        catch (Exception e){
            return (e, unit);
        }
    }

    public static async ValueTask<(Exception? Error, Unit Value)> Try(Func<ValueTask> f) {
        try{
            await f();
            return (null, unit);
        }
        catch (Exception e){
            return (e, unit);
        }
    }

    public static async ValueTask<(Exception? Error, Unit Value)> Try<S>(S state, Func<S, ValueTask> f) {
        try{
            await f(state);
            return (null, unit);
        }
        catch (Exception e){
            return (e, unit);
        }
    }

    public static (Exception? Error, T Value) Try<S, T>(S state, Func<S, T> f) {
        try{
            return (null, f(state));
        }
        catch (Exception e){
            return (e, default!);
        }
    }

    public static (Exception? Error, T Value) Try<T>(Func<T> f) {
        try{
            return (null, f());
        }
        catch (Exception e){
            return (e, default!);
        }
    }

    public static async ValueTask<(Exception? Error, T Value)> Try<T>(Func<ValueTask<T>> f) {
        try{
            return (null, await f());
        }
        catch (Exception e){
            return (e, default!);
        }
    }

    public static async ValueTask<(Exception? Error, T Value)> Try<S, T>(S state, Func<S, ValueTask<T>> f) {
        try{
            return (null, await f(state));
        }
        catch (Exception e){
            return (e, default!);
        }
    }

    #endregion

    [Pure]
    public static Option<T> ToOption<T>(this in (Exception?, T) result)
        => result switch {
            (null, var value) => value,
            (_, _)            => None
        };

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<Option<T>> ToOption<T>(this ValueTask<(Exception?, T)> result)
        => (await result).ToOption();

    [Pure]
    public static Outcome<T> ToOutcome<T>(this in (Exception?, T) result)
        => result switch {
            (null, var value) => value,
            var (error, _)    => ErrorFrom.Exception(error)
        };

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<Outcome<T>> ToOutcome<T>(this ValueTask<(Exception?, T)> result)
        => (await result).ToOutcome();

    #endregion

    #region ToOutcome

    [Pure]
    [PublicAPI]
    public static Outcome<T> ToOutcome<T>(this Option<T> opt, ErrorInfo? error = null)
        => opt.Match(v => (Outcome<T>)v, () => error ?? new(StandardErrorCodes.NotFound));

    [Pure]
    [PublicAPI]
    public static Outcome<T> ToOutcome<T>(this Either<ErrorInfo, T> opt) => opt.Match(v => (Outcome<T>)v, e => e);

    [Pure]
    [PublicAPI]
    public static Outcome<T> ToOutcome<T>(this Try<T> self) => self.ToEither(ErrorFrom.Exception).ToOutcome();

    #endregion

    #region Try/Catch

    public static async ValueTask<Outcome<T>> TryCatch<T>(Task<T> task) {
        try{
            return await task;
        }
        catch (Exception e){
            return ErrorFrom.Exception(e);
        }
    }

    public static async ValueTask<Outcome<T>> TryCatch<T>(ValueTask<T> task) {
        try{
            return await task;
        }
        catch (Exception e){
            return ErrorFrom.Exception(e);
        }
    }

    public static async ValueTask<Outcome<Unit>> TryCatch(Task task) {
        try{
            await task;
            return unit;
        }
        catch (Exception e){
            return ErrorFrom.Exception(e);
        }
    }

    public static async ValueTask<Outcome<Unit>> TryCatch(ValueTask task) {
        try{
            await task;
            return unit;
        }
        catch (Exception e){
            return ErrorFrom.Exception(e);
        }
    }

    public static async IAsyncEnumerable<Outcome<T>> TryCatch<T>(IAsyncEnumerable<T> enumerable, [EnumeratorCancellation] CancellationToken cancelToken = default) {
       await using var itor = enumerable.GetAsyncEnumerator(cancelToken);
       ErrorInfo? e;
       while (Success(await TryCatch(itor.MoveNextAsync()), out var hasNext, out e) && hasNext)
           yield return itor.Current;

       if (e is not null)
           yield return e;
    }

    [PublicAPI]
    public static async ValueTask<Outcome<T>> TryCatch<T>([InstantHandle] Func<ValueTask<Outcome<T>>> handler) {
        try{
            return await handler();
        }
        catch (Exception e){
            return ErrorFrom.Exception(e);
        }
    }

    [PublicAPI]
    public static async ValueTask<Outcome<T>> TryCatch<T>([InstantHandle] Func<ValueTask<T>> handler) {
        try{
            return await handler();
        }
        catch (Exception e){
            return ErrorFrom.Exception(e);
        }
    }

    [PublicAPI]
    public static async ValueTask<Outcome<Unit>> TryCatch([InstantHandle] Func<ValueTask> handler) {
        try{
            await handler();
            return unit;
        }
        catch (Exception e){
            return ErrorFrom.Exception(e);
        }
    }

    [PublicAPI]
    public static Outcome<T> TryCatch<T>([InstantHandle] Func<Outcome<T>> handler) {
        try{
            return handler();
        }
        catch (Exception e){
            return ErrorFrom.Exception(e);
        }
    }

    public static Outcome<T> TryCatch<T>([InstantHandle] Func<T> handler) {
        try{
            return handler();
        }
        catch (Exception e){
            return ErrorFrom.Exception(e);
        }
    }

    public static Outcome<Unit> TryCatch([InstantHandle] Action handler) {
        try{
            handler();
            return unit;
        }
        catch (Exception e){
            return ErrorFrom.Exception(e);
        }
    }

    #endregion

    #region Option Helpers

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IfSome<T>(Option<T> opt, [NotNullWhen(true)] out T? data) => opt.IfSome(out data);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool UnlessSome<T>(Option<T> opt, [NotNullWhen(false)] out T? data) => opt.UnlessSome(out data);

    #endregion

    #region Outcome Helpers

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Success<T>(Outcome<T> outcome, [NotNullWhen(true)] out T? v, [NotNullWhen(false)] out ErrorInfo? e) {
        (v, e) = outcome.IsSuccess ? (outcome.Data, null) : (default(T), outcome.Error);
        return outcome.IsSuccess;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Success<T>(Outcome<T> outcome, [NotNullWhen(true)] out T? v) {
        v = outcome.IsSuccess ? outcome.Data : default;
        return outcome.IsSuccess;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool UnlessFail<T>(Outcome<T> outcome, [NotNullWhen(false)] out ErrorInfo? e) {
        e = outcome.IsSuccess ? null : outcome.Error;
        return outcome.IsSuccess;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Fail<T>(Outcome<T> outcome, [NotNullWhen(true)] out ErrorInfo? e, [NotNullWhen(false)] out T? v) {
        (v, e) = outcome.IsSuccess ? (outcome.Data!, null) : (default(T), outcome.Error);
        return outcome.IsFail;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Fail<T>(Outcome<T> outcome, [NotNullWhen(true)] out ErrorInfo? e) {
        e = outcome.IsSuccess ? null : outcome.Error;
        return outcome.IsFail;
    }

    public static bool FailButNotFound<T>(Outcome<T> outcome, [NotNullWhen(true)] out ErrorInfo? e, out T? v) {
        (v, e) = outcome.IsSuccess ? (outcome.Data!, null) : (default(T), outcome.Error);
        return outcome.IsFail && outcome.Error?.IsNotFound() != true;
    }

    public static bool FailButNotFound<T>(Outcome<T> outcome, [NotNullWhen(true)] out ErrorInfo? e) {
        e = outcome.IsSuccess ? null : outcome.Error;
        return outcome.IsFail && outcome.Error?.IsNotFound() != true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool UnlessSuccess<T>(Outcome<T> outcome, [NotNullWhen(false)] out T? v) {
        v = outcome.IsSuccess ? outcome.Data : default;
        return outcome.IsFail;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Outcome<T> UpCast<T, K>(Outcome<K> outcome) where K : T
        => outcome.IsSuccess ? outcome.Data! : outcome.Error!;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static OutcomeCatch<T> matchError<T>(Func<ErrorInfo, bool> predicate, Func<ErrorInfo, Outcome<T>> fail)
        => new(predicate, fail);

    #region catch

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeCatch<T> @catch<T>(Outcome<T> replacement)
        => new(static _ => true, _ => replacement);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeCatch<T> @catch<T>(Func<ErrorInfo, T> replacement)
        => new(static _ => true, e => replacement(e));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeCatch<T> @catch<T>(Func<ErrorInfo, ErrorInfo> replacement)
        => new(static _ => true, e => replacement(e));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeCatch<T> @catch<T>(Func<ErrorInfo, Outcome<T>> replacement)
        => new(static _ => true, replacement);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeCatch<T> @catch<T>(ErrorInfo error, Outcome<T> replacement)
        => matchError(e => e.Is(error), _ => replacement);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeCatch<T> @catch<T>(ErrorInfo error, Func<ErrorInfo, T> @catch)
        => matchError(e => e.Is(error), e => SuccessOutcome(@catch(e)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeCatch<T> @catch<T>(ErrorInfo error, Func<ErrorInfo, ErrorInfo> replacement)
        => matchError(e => e.Is(error), e => (Outcome<T>)replacement(e));

    #endregion

    #region failDo

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeSideEffect failDo(Action<ErrorInfo> fail) =>
        new(ToUnit(fail));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeSideEffect failDo(Func<ErrorInfo, Unit> fail) =>
        new(fail);

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeSideEffect<T> @do<T>(Action<T> sideEffect) =>
        new(ToUnit(sideEffect));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeSideEffect<T> @do<T>(Func<T, Unit> sideEffect) =>
        new(sideEffect);

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Outcome<T> FailedOutcome<T>(ErrorInfo error) => error;

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Outcome<T> SuccessOutcome<T>(T value) => value;

    public static readonly Outcome<Unit> UnitOutcome = unit;

    #endregion

    #region Throw If Error

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<T> ThrowIfError<T>(ValueTask<Outcome<T>> value)
        => (await value).Unwrap();

    public static async ValueTask<T?> ThrowUnlessNotFound<T>(ValueTask<Outcome<T>> value)
        => Success(await value, out var v, out var e) ? (T?) v : e.IsNotFound() ? default(T?) : throw new ErrorInfoException(e);

    public static T ThrowIfNotFound<T>(this Option<T> optionValue, string message)
        => optionValue.GetOrThrow(() => new ErrorInfoException("not-found", message));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ThrowIfNotFound<T>(Option<T> value)
        => value.ThrowIfNotFound("Not found");

    #endregion
}