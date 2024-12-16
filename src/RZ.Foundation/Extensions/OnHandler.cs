using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JetBrains.Annotations;
using LanguageExt.Common;
using RZ.Foundation.Types;

namespace RZ.Foundation.Extensions;

[PublicAPI]
public static class HandleCatchable
{
    [Pure]
    public static Option<T> ToOption<T>(in (Exception?, T) result)
        => result switch {
            (null, var value) => value,
            (_, _)            => None
        };

    [Pure]
    public static Outcome<T> ToOutcome<T>(in (Exception?, T) result)
        => result switch {
            (null, var value) => value,
            var (error, _)    => ErrorFrom.Exception(error)
        };

    [Pure]
    public static Result<T> ToResult<T>(in (Exception?, T) result)
        => result switch {
            (null, var value) => value,
            var (error, _)    => new Result<T>(error)
        };

    #region Option

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<Unit> CatchOption(this in OnHandlerSync handle)
        => ToOption(handle.Catch());

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> CatchOption<T>(this in OnHandlerSync<T> handle)
        => ToOption(handle.Catch());

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<Unit>> CatchOption(this OnHandler handle)
        => ToOption(await handle.Catch());

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<T>> CatchOption<T>(this OnHandler<T> handle)
        => ToOption(await handle.Catch());

    #endregion

    #region Outcome

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Outcome<Unit> CatchOutcome(this in OnHandlerSync handle)
        => ToOutcome(handle.Catch());

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Outcome<T> CatchOutcome<T>(this in OnHandlerSync<T> handle)
        => ToOutcome(handle.Catch());

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Outcome<Unit>> CatchOutcome(this OnHandler handle)
        => ToOutcome(await handle.Catch());

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Outcome<T>> CatchOutcome<T>(this OnHandler<T> handle)
        => ToOutcome(await handle.Catch());

    #endregion

    #region Result

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<Unit> CatchResult(this in OnHandlerSync handle)
        => ToResult(handle.Catch());

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T> CatchResult<T>(this in OnHandlerSync<T> handle)
        => ToResult(handle.Catch());

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<Unit>> CatchResult(this OnHandler handle)
        => ToResult(await handle.Catch());

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<T>> CatchResult<T>(this OnHandler<T> handle)
        => ToResult(await handle.Catch());

    #endregion
}

[PublicAPI]
public readonly struct OnHandlerSync(Action task)
{
    public void Catch(Action<Exception> handler) {
        try{
            task();
        }
        catch (Exception e) {
            handler(e);
        }
    }

    public Task Catch(Func<Exception, Task> handler) {
        try{
            task();
            return Task.CompletedTask;
        }
        catch (Exception e) {
            return handler(e);
        }
    }

    public (Exception?, Unit) Catch() {
        try{
            task();
            return (null, unit);
        }
        catch (Exception e) {
            return (e, unit);
        }
    }

    public void BeforeThrow(Action<Exception> effect) {
        try{
            task();
        }
        catch (Exception e) {
            effect(e);
            throw;
        }
    }
}

[PublicAPI]
public readonly struct OnHandlerSync<T>(Func<T> task)
{
    public T Catch(Func<Exception, T> handler) {
        try{
            return task();
        }
        catch (Exception e) {
            return handler(e);
        }
    }

    public Task<T> Catch(Func<Exception, Task<T>> handler) {
        try{
            return Task.FromResult(task());
        }
        catch (Exception e) {
            return handler(e);
        }
    }

    public (Exception?, T) Catch() {
        try{
            return (null, task());
        }
        catch (Exception e) {
            return (e, default!);
        }
    }

    public T BeforeThrow(Action<Exception> effect) {
        try{
            return task();
        }
        catch (Exception e){
            effect(e);
            throw;
        }
    }
}

[PublicAPI]
public readonly struct OnHandler(Task task)
{
    public async Task Catch(Action<Exception> handler) {
        try {
            await task;
        }
        catch (Exception e) {
            handler(e);
        }
    }

    public async Task Catch(Func<Exception, Task> handler) {
        try {
            await task;
        }
        catch (Exception e) {
            await handler(e);
        }
    }

    public async Task<(Exception? Error, Unit Value)> Catch() {
        try {
            await task;
            return (null, unit);
        }
        catch (Exception e) {
            return (e, unit);
        }
    }

    public async Task BeforeThrow(Action<Exception> effect) {
        try {
            await task;
        }
        catch (Exception e) {
            effect(e);
            throw;
        }
    }
}

[PublicAPI]
public readonly struct OnHandler<T>(Task<T> task)
{
    public async Task<T> Catch(Func<Exception, T> handler) {
        try {
            return await task;
        }
        catch (Exception e) {
            return handler(e);
        }
    }

    public async Task<T> Catch(Func<Exception, Task<T>> handler) {
        try {
            return await task;
        }
        catch (Exception e) {
            return await handler(e);
        }
    }

    public async Task<(Exception? Error, T Value)> Catch() {
        try {
            return (null, await task);
        }
        catch (Exception e) {
            return (e, default!);
        }
    }

    public async Task<T> BeforeThrow(Action<Exception> effect) {
        try{
            return await task;
        }
        catch (Exception e){
            effect(e);
            throw;
        }
    }
}
