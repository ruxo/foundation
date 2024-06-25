using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Seq = LanguageExt.Seq;

namespace RZ.Foundation.Extensions;

public static class AsyncEnumeratorExtension
{
    public static async ValueTask<Seq<T>> TakeAtMost<T>(this IAsyncEnumerator<T> itor, int n) {
        var result = new T[n];
        var count = 0;
        while (count < n && await itor.MoveNextAsync()) {
            result[count] = itor.Current;
            ++count;
        }
        return count == 0 ? Seq.empty<T>() : count == n ? Seq(result) : Seq(result.AsSpan(0, count).ToArray());
    }
}