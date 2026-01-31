using System.Numerics;
using System.Runtime.CompilerServices;

namespace RZ.Foundation.Extensions;

public static class AsyncOutcomeEnumerableExtension
{
    [PublicAPI, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<Outcome<T>> Average<T>(this IAsyncEnumerable<Outcome<T>> seq, CancellationToken cancelToken = default) where T : INumber<T> {
        var average = T.Zero;
        var counter = 0;
        await foreach (var i in seq.ConfigureAwait(false).WithCancellation(cancelToken)){
            if (Fail(i, out var e, out var v)) return e;
            average += v;
            counter++;
        }
        return average / T.CreateChecked(counter);
    }

    extension<T>(IAsyncEnumerable<Outcome<T>> seq)
    {
        [PublicAPI]
        public async ValueTask<Outcome<TAverage>> AverageBy<TAverage>(Func<T, TAverage> getter, CancellationToken cancelToken = default)
            where TAverage : INumber<TAverage> {
            var average = TAverage.Zero;
            var counter = 0;
            await foreach (var i in seq.ConfigureAwait(false).WithCancellation(cancelToken)){
                if (Fail(i, out var e, out var v)) return e;
                average += getter(v);
                counter++;
            }
            return average / TAverage.CreateChecked(counter);
        }

        [PublicAPI]
        public async ValueTask<Outcome<T>> First(CancellationToken cancelToken = default) {
            if (seq.IsKnownEmpty()) return ErrorInfo.NotFound;
            try{
                await foreach (var i in seq.ConfigureAwait(false).WithCancellation(cancelToken)) return i;
            }
            catch (Exception e){
                return ErrorFrom.Exception(e);
            }
            return ErrorInfo.NotFound; // should never reach
        }

        [PublicAPI]
        public async ValueTask<Outcome<T>> Last(CancellationToken cancelToken = default) {
            if (seq.IsKnownEmpty()) return ErrorInfo.NotFound;
            try{
                await using var itor = seq.GetAsyncEnumerator(cancelToken);
                Outcome<T>? last = null;
                while (!await itor.MoveNextAsync().ConfigureAwait(false))
                    if (Fail(itor.Current, out var e)) return e;
                    else last = itor.Current;

                return last ?? ErrorInfo.NotFound;
            }
            catch (Exception e){
                return ErrorFrom.Exception(e);
            }
        }

        [PublicAPI]
        public async ValueTask<Outcome<List<A?>>> MakeMutableList<A>(Func<T,A?> selector, CancellationToken cancelToken = default) {
            if (seq.IsKnownEmpty()) return ErrorInfo.NotFound;

            var result = new List<A?>();
            await foreach (var i in seq.ConfigureAwait(false).WithCancellation(cancelToken))
                if (Success(i, out var v, out var e))
                    result.Add(selector(v));
                else
                    return e;
            return result;
        }

        [PublicAPI]
        public async ValueTask<Outcome<IReadOnlyList<A?>>> MakeList<A>(Func<T,A?> selector, CancellationToken cancelToken = default)
            => Success(await seq.MakeMutableList(selector, cancelToken).ConfigureAwait(false), out var v, out var e) ? v : e;

        [PublicAPI]
        public async ValueTask<Outcome<List<T>>> MakeMutableList(CancellationToken cancelToken = default) {
            if (seq.IsKnownEmpty()) return ErrorInfo.NotFound;

            var result = new List<T>();
            await foreach (var i in seq.ConfigureAwait(false).WithCancellation(cancelToken))
                if (Success(i, out var v, out var e))
                    result.Add(v);
                else
                    return e;
            return result;
        }

        [PublicAPI]
        public async ValueTask<Outcome<IReadOnlyList<T>>> MakeList(CancellationToken cancelToken = default)
            => Success(await seq.MakeMutableList(cancelToken).ConfigureAwait(false), out var v, out var e) ? v : e;
    }
}