using System;
using System.Threading.Tasks;

namespace RZ.Foundation.Extensions
{
    public static class RetryExtension
    {
        public static Task<T> RetryIndefinitely<T>(Func<Task<T>> task, Action<Exception> onFailure = null) =>
            RetryUntil(task,
                       onFailure == null
                           ? null
                           : new Func<Exception, bool>(e => {
                               onFailure(e);
                               return true;
                           }));

        public static T RetryIndefinitely<T>(Func<T> task, Action<Exception> onFailure = null) =>
            RetryUntil(task,
                       onFailure == null
                           ? null
                           : new Func<Exception, bool>(e => {
                               onFailure(e);
                               return true;
                           }));

        public static async Task<T> RetryUntil<T>(Func<Task<T>> task, Func<Exception,bool> onFailure = null) {
            ApiResult<T> result;
            var cont = true;
            do {
                result = await ApiResult<T>.SafeCallAsync(task);
                if (result.IsFail) cont = onFailure?.Invoke(result.GetFail()) ?? true;
            } while (result.IsFail && cont);

            return result.GetSuccess();
        }

        public static T RetryUntil<T>(Func<T> task, Func<Exception,bool> onFailure = null) {
            ApiResult<T> result;
            var cont = true;
            do {
                result = ApiResult<T>.SafeCall(task);
                if (result.IsFail) cont = onFailure?.Invoke(result.GetFail()) ?? true;
            } while (result.IsFail && cont);

            return result.GetSuccess();
        }
    }
}