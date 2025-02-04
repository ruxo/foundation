using System;
using JetBrains.Annotations;

namespace RZ.Foundation.Types;

[PublicAPI]
public readonly record struct CaseInsensitiveString(string Value) : IComparable<CaseInsensitiveString>
{
    public bool Equals(CaseInsensitiveString? other) =>
        string.Compare(Value, other?.Value, StringComparison.OrdinalIgnoreCase) == 0;

    public override int GetHashCode() => Value.ToUpperInvariant().GetHashCode();
    public override string ToString() => Value;

    public static implicit operator CaseInsensitiveString(string v) => new(v);

    public int CompareTo(CaseInsensitiveString other) => string.Compare(Value, other.Value, StringComparison.OrdinalIgnoreCase);
}