using System.Diagnostics.Contracts;

namespace RZ.Foundation.Functional;

/// <summary>
/// Higher-Kinded Type helper, as `K` in LanguageExt v5
/// </summary>
/// <typeparam name="M">Monad itself</typeparam>
/// <typeparam name="T">Type</typeparam>
public interface HK<M, T>;

public interface Eq<M> where M : Eq<M>
{
    [Pure]
    public static abstract HK<M, bool> EqualsTo<T>(HK<M, T> a, HK<M, T> b);

    [Pure]
    public static abstract HK<M, bool> NotEqualsTo<T>(HK<M, T> a, HK<M, T> b);
}
