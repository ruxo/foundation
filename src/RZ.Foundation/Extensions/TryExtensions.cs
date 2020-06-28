using System;
using System.Threading.Tasks;
using LanguageExt;
using static RZ.Foundation.Prelude;
using static LanguageExt.Prelude;

namespace RZ.Foundation.Types
{
    public static class TryExtensions
    {

    }
    /*
    /// <summary>
    /// Inspired by LanguageExt
    /// </summary>
    public sealed class TryAsync<T>
    {
        readonly Func<Task<T>> runnable;

        public TryAsync(Func<Task<T>> runnable) {
            this.runnable = runnable;
        }

        public async Task<U> Try<U>(Func<T, U> success, Func<Exception, U> failed) =>
            (await Try()).Get(success, failed);

        public Task<ApiResult<T>> Try() => ApiResult<T>.SafeCallAsync(runnable);

        public async Task<ApiResult<T>> Try(Func<Exception, Exception> exceptionTransformer) {
            var result = await Try();
            return result.Map(Identity, exceptionTransformer);
        }
        public Task<Option<T>> TryOption() => Option<T>.SafeCallAsync(runnable);

        public TryAsync<U> Map<U>(Func<T, U> mapper) => new TryAsync<U>(async () => mapper(await runnable()));
        public TryAsync<U> MapAsync<U>(Func<T,Task<U>> mapper) => new TryAsync<U>(async () => await mapper(await runnable()));

        public TryAsync<U> Chain<U>(Func<T, TryAsync<U>> binder) => new TryAsync<U>(async () => await binder(await runnable()).runnable());
        public TryAsync<U> ChainAsync<U>(Func<T,Task<TryAsync<U>>> binder) => new TryAsync<U>(async () => await (await binder(await runnable())).runnable());

        public TryAsync<T> SideEffect(Action<T> action) => new TryAsync<T>(async () => {
            var result = await runnable();
            action(result);
            return result;
        });
        public TryAsync<T> SideEffectAsync(Func<T,Task> action) => new TryAsync<T>(async () => {
            var result = await runnable();
            await action(result);
            return result;
        });
    }

    public sealed class TryCall<T>
    {
        readonly Func<T> runnable;

        public TryCall(Func<T> runnable) {
            this.runnable = runnable;
        }

        public U Try<U>(Func<T, U> success, Func<Exception, U> failed) {
            var result = Try();
            return result.Get(success, failed);
        }

        public ApiResult<T> Try() => ApiResult<T>.SafeCall(runnable);
        public ApiResult<T> Try(Func<Exception, Exception> exceptionTransformer) => Try().Map(identity, exceptionTransformer);
        public Option<T> TryOption() => Try(runnable);

        public TryCall<U> Map<U>(Func<T, U> mapper) => new TryCall<U>(() => mapper(runnable()));

        public TryCall<U> Chain<U>(Func<T,TryCall<U>> binder) => new TryCall<U>(() => binder(runnable()).runnable());

        public TryCall<T> SideEffect(Action<T> action) => new TryCall<T>(() => {
            var result = runnable();
            action(result);
            return result;
        });

        public TryAsync<T> ToAsync() => new TryAsync<T>(() => Task.FromResult(runnable()));
    }
    */
}