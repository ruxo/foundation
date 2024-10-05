using JetBrains.Annotations;

namespace RZ.Foundation.Blazor;

[PublicAPI]
public readonly record struct NotificationEvent(DateTimeOffset Timestamp, MessageSeverity Severity, string Message);

[PublicAPI]
public readonly record struct NotificationMessage(MessageSeverity Severity, string Message)
{
    public static implicit operator NotificationMessage(in (MessageSeverity Severity, string Message) tuple) =>
        new(tuple.Severity, tuple.Message);
}