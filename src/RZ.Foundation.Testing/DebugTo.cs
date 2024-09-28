using System.Diagnostics;
using System.Text;
using JetBrains.Annotations;
using Xunit.Abstractions;

namespace RZ.Foundation.Testing;

[PublicAPI]
public static class DebugTo
{
    /// <summary>
    /// A utility function to redirect <see cref="Trace"/> output to xUnit test output.
    /// </summary>
    /// <param name="output"></param>
    /// <returns></returns>
    public static IDisposable XUnit(ITestOutputHelper output) {
        var listener = new SimpleTrace(output.WriteLine);
        Trace.Listeners.Add(listener);
        return new Disposable<SimpleTrace>(listener, l => Trace.Listeners.Remove(l));
    }

    public class SimpleTrace(Action<string> writeLine) : TraceListener
    {
        StringBuilder messageComposer = new();

        public override void Write(string? message) {
            messageComposer.Append(message ?? string.Empty);
        }

        public override void WriteLine(string? message) {
            var sb = Interlocked.Exchange(ref messageComposer, new());
            if (sb.Length > 0)
                writeLine(sb.ToString());

            writeLine(message ?? string.Empty);
        }
    }

    sealed class Disposable<T>(T obj, Action<T> disposer) : IDisposable
    {
        public void Dispose() => disposer(obj);
    }
}