using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using RZ.Foundation.Blazor.Layout;

namespace RZ.Foundation.Blazor.MVVM;

public abstract record EventBubble
{
    [MethodImpl(MethodImplOptions.NoOptimization)]
    [PublicAPI]
    public static EventBubble? NoRaise(Unit _) => null;
}

public delegate ValueTask<EventBubble?> EventBubbleListener(EventBubble bubble);

public interface IEventBubbleSubscription
{
    /// <summary>
    /// The hook point to subscribe to listen to the event stream. Listeners will be called in the order of subscription,
    /// and the event will be passed to the next listener if the listener returns a non-null value.
    /// </summary>
    [PublicAPI]
    IDisposable Subscribe(EventBubbleListener listener);

    /// <summary>
    /// Send the event to the listeners.
    /// </summary>
    /// <param name="event"></param>
    /// <returns></returns>
    [PublicAPI]
    ValueTask<Unit> Raise(EventBubble @event);
}

public class EventBubbleSubscription : IEventBubbleSubscription
{
    readonly LinkedList<EventBubbleListener> listeners = new();

    public IDisposable Subscribe(EventBubbleListener listener) {
        var node = listeners.AddFirst(listener);
        return Disposable.Create(node, listeners.Remove);
    }

    public async ValueTask<Unit> Raise(EventBubble @event) {
        var node = listeners.First;
        var message = @event;
        while (node is not null && message is not null) {
            var listener = node.Value;
            message = await listener(message);
            node = node.Next;
        }
        return unit;
    }
}

[PublicAPI]
public static class BubbleExtensions
{
    public static async ValueTask<EventBubble?> HandleEvent<T>(this ValueTask<Outcome<T>> task,
                                                               ShellViewModel shell,
                                                                ILogger logger,
                                                               Func<T,string> successHandler) {
        if ((await task).IfSuccess(out var newVersion, out var error))
            shell.Notify((Severity.Success, successHandler(newVersion)));
        else{
            logger.LogError("Update Term & Condition failed with {@Error}!", error);
            shell.Notify((Severity.Error, $"Error {error.Message}!"));
        }
        return null;
    }

    public static ValueTask<EventBubble?> HandleEvent<T>(this ValueTask<T> task,
                                                         ShellViewModel shell,
                                                         ILogger logger,
                                                         Func<T, string> successHandler)
        => TryCatch(async () => await task).HandleEvent(shell, logger, successHandler);
}
