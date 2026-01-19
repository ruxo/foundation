namespace RZ.Foundation.Modules;

[PublicAPI]
public static class Seq
{
    public static IEnumerable<T> InitInfinite<T>(T value) {
        while (true)
            yield return value;
        // ReSharper disable once IteratorNeverReturns
    }
}