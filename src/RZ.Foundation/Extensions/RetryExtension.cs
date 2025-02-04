using System;
using System.Threading.Tasks;
using LanguageExt.Common;

namespace RZ.Foundation.Extensions
{
    public static class RetryExtension
    {
        public static async Task<T> RetryIndefinitely<T>(Func<Task<T>> task, Action<Exception>? onFailure = null) {
            var result = await RetryUntil(task,
                                          onFailure == null
                                              ? null
                                              : new Func<Exception, bool>(e => {
                                                  onFailure(e);
                                                  return true;
                                              }));
            return result.GetSuccess();
        }

        public static T RetryIndefinitely<T>(Func<T> task, Action<Exception>? onFailure = null) =>
            RetryUntil(task,
                       onFailure == null
                           ? null
                           : new Func<Exception, bool>(e => {
                               onFailure(e);
                               return true;
                           })).GetSuccess();

        public static async Task<Result<T>> RetryUntil<T>(Func<Task<T>> task, Func<Exception,bool>? onFailure = null) {
            Result<T> result;
            var cont = true;
            do{
                result = await Try(task).ToResult();
                if (result.IsFaulted) cont = onFailure?.Invoke(result.GetFail()) ?? true;
            } while (result.IsFaulted && cont);

            return result;
        }

        public static Result<T> RetryUntil<T>(Func<T> task, Func<Exception,bool>? onFailure = null) {
            Result<T> result;
            var cont = true;
            do{
                result = Try(task).ToResult();
                if (result.IsFaulted) cont = onFailure?.Invoke(result.GetFail()) ?? true;
            } while (result.IsFaulted && cont);

            return result;
        }
    }
}