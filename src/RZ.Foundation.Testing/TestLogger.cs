using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace RZ.Foundation.Testing;

public class TestLogger<T>(ITestOutputHelper output) : ILogger<T>
{
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
        output.WriteLine($"[{logLevel} {eventId}] {formatter(state, exception)}");
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public IDisposable BeginScope<TState>(TState state) where TState: notnull => throw new NotImplementedException();
}