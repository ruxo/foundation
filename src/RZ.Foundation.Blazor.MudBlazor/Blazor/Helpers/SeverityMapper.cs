using JetBrains.Annotations;
using MudBlazor;

namespace RZ.Foundation.Blazor.Helpers;

[PublicAPI]
public static class SeverityMapper
{
    public static Severity ToMudSeverity(this MessageSeverity severity) => severity switch {
        MessageSeverity.Info    => Severity.Info,
        MessageSeverity.Success => Severity.Success,
        MessageSeverity.Warning => Severity.Warning,
        MessageSeverity.Error   => Severity.Error,

        _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null)
    };
}