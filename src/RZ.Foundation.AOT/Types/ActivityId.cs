using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace RZ.Foundation.Types;

/// <summary>
/// Represents a traceable activity identifier (mostly for OpenTelemetry).
/// </summary>
[PublicAPI]
public readonly record struct ActivityId(string Value)
{
    /// <summary>
    /// Get current activity id.
    /// </summary>
    public static ActivityId? Current => Activity.Current?.Id is {} v? new(v) : null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ActivityId FromString(string value) => new(value);
}
