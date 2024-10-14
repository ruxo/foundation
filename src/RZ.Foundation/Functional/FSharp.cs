using System;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace RZ.Foundation.Functional;

[PublicAPI]
public static class FSharp
{
    /// <summary>
    /// Convert F# expression into C# Expression
    /// </summary>
    public static Expression<Func<A, B>> ToExpression<A, B>(this Expression<Func<A, B>> f) => f;
}