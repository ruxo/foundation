using Seq = LanguageExt.Seq;

namespace RZ.Foundation.Extensions;

[PublicAPI]
public static class AsyncEnumeratorExtension
{
    extension<T>(IAsyncEnumerator<T> itor)
    {
        [PublicAPI]
        public async ValueTask<IEnumerable<T>> TakeAtMost(int n) {
            var result = new T[n];
            var count = 0;
            while (count < n && await itor.MoveNextAsync().ConfigureAwait(false)) {
                result[count] = itor.Current;
                ++count;
            }
            return count == 0 ? Seq.empty<T>() : count == n ? result : result.AsSpan(0, count).ToArray();
        }

        [PublicAPI]
        public async ValueTask<Outcome<bool>> TryMoveNext() {
            try{
                return await itor.MoveNextAsync().ConfigureAwait(false);
            }
            catch (Exception e){
                return ErrorFrom.Exception(e);
            }
        }
    }
}