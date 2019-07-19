using System;
using System.Threading.Tasks;

namespace RZ.Foundation.Types
{
    /// <summary>
    /// Inspired by LanguageExt
    /// </summary>
    public class TryAsync<T>
    {
        readonly Func<Task<T>> runnable;

        public TryAsync(Func<Task<T>> runnable) {
            this.runnable = runnable;
        }

        public async Task<U> Try<U>(Func<T, U> success, Func<Exception, U> failed) {
            var result = await Try();
            return result.Get(success, failed);
        }

        public Task<ApiResult<T>> Try() => ApiResult<T>.SafeCallAsync(runnable);

        public TryAsync<U> Map<U>(Func<T, U> mapper) => new TryAsync<U>(async () => mapper(await runnable()));

        public TryAsync<U> ChainAsync<U>(Func<T,Task<U>> binder) => new TryAsync<U>(async () => await binder(await runnable()));

        public TryAsync<T> SideEffect(Action<T> action) => new TryAsync<T>(async () => {
            var result = await runnable();
            action(result);
            return result;
        });
    }
}