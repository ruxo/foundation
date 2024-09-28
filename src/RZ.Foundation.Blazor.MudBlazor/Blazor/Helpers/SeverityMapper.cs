using JetBrains.Annotations;
using MudBlazor;

namespace RZ.Foundation.Blazor.Helpers;

[PublicAPI]
public static class SeverityMapper
{
    public static Severity ToMudSeverity(this Blazor.Layout.Severity severity) => severity switch {
        Layout.Severity.Info    => Severity.Info,
        Layout.Severity.Success => Severity.Success,
        Layout.Severity.Warning => Severity.Warning,
        Layout.Severity.Error   => Severity.Error,

        _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null)
    };
}