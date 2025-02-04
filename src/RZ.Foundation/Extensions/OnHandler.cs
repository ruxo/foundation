using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RZ.Foundation.Types;

namespace RZ.Foundation.Extensions;

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

    public void ErrorThrow(string code, string? message = null) {
        try{
            task();
        }
        catch (Exception e){
            throw new ErrorInfoException(code, message, innerException: e);
        }
    }
}

[PublicAPI]
public readonly struct OnHandlerSyncX<TX>(TX x, Action<TX> task)
{
    public void Catch(Action<Exception> handler) {
        try{
            task(x);
        }
        catch (Exception e) {
            handler(e);
        }
    }

    public Task Catch(Func<Exception, Task> handler) {
        try{
            task(x);
            return Task.CompletedTask;
        }
        catch (Exception e) {
            return handler(e);
        }
    }

    public (Exception?, Unit) Catch() {
        try{
            task(x);
            return (null, unit);
        }
        catch (Exception e) {
            return (e, unit);
        }
    }

    public void BeforeThrow(Action<Exception> effect) {
        try{
            task(x);
        }
        catch (Exception e) {
            effect(e);
            throw;
        }
    }

    public void ErrorThrow(string code, string? message = null) {
        try{
            task(x);
        }
        catch (Exception e){
            throw new ErrorInfoException(code, message, innerException: e);
        }
    }
}

[PublicAPI]
public readonly struct OnHandlerSync<TX,T>(TX x, Func<TX,T> task)
{
    public T Catch(Func<Exception, T> handler) {
        try{
            return task(x);
        }
        catch (Exception e) {
            return handler(e);
        }
    }

    public Task<T> Catch(Func<Exception, Task<T>> handler) {
        try{
            return Task.FromResult(task(x));
        }
        catch (Exception e) {
            return handler(e);
        }
    }

    public T BeforeThrow(Action<Exception> effect) {
        try{
            return task(x);
        }
        catch (Exception e){
            effect(e);
            throw;
        }
    }

    public T ErrorThrow(string code, string? message = null) {
        try{
            return task(x);
        }
        catch (Exception e){
            throw new ErrorInfoException(code, message, innerException: e);
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

    public T BeforeThrow(Action<Exception> effect) {
        try{
            return task();
        }
        catch (Exception e){
            effect(e);
            throw;
        }
    }

    public T ErrorThrow(string code, string? message = null) {
        try{
            return task();
        }
        catch (Exception e){
            throw new ErrorInfoException(code, message, innerException: e);
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

    public async Task BeforeThrow(Action<Exception> effect) {
        try {
            await task;
        }
        catch (Exception e) {
            effect(e);
            throw;
        }
    }

    public async Task ErrorThrow(string code, string? message = null) {
        try{
            await task;
        }
        catch (Exception e){
            throw new ErrorInfoException(code, message, innerException: e);
        }
    }
}

[PublicAPI]
public readonly struct OnHandlerX<TX>(TX x, Func<TX,Task> task)
{
    public async Task Catch(Action<Exception> handler) {
        try {
            await task(x);
        }
        catch (Exception e) {
            handler(e);
        }
    }

    public async Task Catch(Func<Exception, Task> handler) {
        try {
            await task(x);
        }
        catch (Exception e) {
            await handler(e);
        }
    }

    public async Task BeforeThrow(Action<Exception> effect) {
        try {
            await task(x);
        }
        catch (Exception e) {
            effect(e);
            throw;
        }
    }

    public async Task ErrorThrow(string code, string? message = null) {
        try{
            await task(x);
        }
        catch (Exception e){
            throw new ErrorInfoException(code, message, innerException: e);
        }
    }
}

[PublicAPI]
public readonly struct OnHandler<TX,T>(TX x, Func<TX, Task<T>> task)
{
    public async Task<T> Catch(Func<Exception, T> handler) {
        try {
            return await task(x);
        }
        catch (Exception e) {
            return handler(e);
        }
    }

    public async Task<T> Catch(Func<Exception, Task<T>> handler) {
        try {
            return await task(x);
        }
        catch (Exception e) {
            return await handler(e);
        }
    }

    public async Task<T> BeforeThrow(Action<Exception> effect) {
        try{
            return await task(x);
        }
        catch (Exception e){
            effect(e);
            throw;
        }
    }

    public async Task<T> ErrorThrow(string code, string? message = null) {
        try{
            return await task(x);
        }
        catch (Exception e){
            throw new ErrorInfoException(code, message, innerException: e);
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

    public async Task<T> BeforeThrow(Action<Exception> effect) {
        try{
            return await task;
        }
        catch (Exception e){
            effect(e);
            throw;
        }
    }

    public async Task<T> ErrorThrow(string code, string? message = null) {
        try{
            return await task;
        }
        catch (Exception e){
            throw new ErrorInfoException(code, message, innerException: e);
        }
    }
}
