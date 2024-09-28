using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using RZ.Foundation.Blazor.MVVM;

namespace RZ.Foundation.Blazor.Layout;

public enum Severity
{
    Info,
    Success,
    Warning,
    Error
}

[PublicAPI]
public class MainLayoutViewModel : ViewModel
{
    readonly Subject<NotificationMessage> notifications = new();
    readonly ObservableCollection<NotificationEvent> notificationMessages = new();
    readonly ObservableAsPropertyHelper<int> messageCount;
    const int MaxNotifications = 20;

    AppMode appMode = AppMode.Page.Instance;
    bool isDarkMode;
    readonly ILogger<MainLayoutViewModel> logger;
    readonly TimeProvider clock;
    readonly IScheduler scheduler;

    public MainLayoutViewModel(ILogger<MainLayoutViewModel> logger, TimeProvider clock, IScheduler scheduler) {
        this.logger = logger;
        this.clock = clock;
        this.scheduler = scheduler;

        messageCount = notificationMessages.WhenAnyValue(x => x.Count).ToProperty(this, x => x.MessageCount);
    }

    public AppMode AppMode
    {
        get => appMode;
        set => this.RaiseAndSetIfChanged(ref appMode, value);
    }

    public bool IsDarkMode
    {
        get => isDarkMode;
        set => this.RaiseAndSetIfChanged(ref isDarkMode, value);
    }

    public bool IsDrawerOpen
    {
        get => appMode is AppMode.Page { IsDrawerOpen: true };
        set
        {
            if (appMode is AppMode.Page p){
                this.RaisePropertyChanging();
                p.IsDrawerOpen = value;
                this.RaisePropertyChanged();
            }
            else
                logger.LogWarning("Cannot set drawer open state when not in page mode");
        }
    }

    public IObservable<NotificationMessage> Notifications => notifications.ObserveOn(scheduler);

    public ObservableCollection<NotificationEvent> NotificationMessages => notificationMessages;

    public int MessageCount => messageCount.Value;

    public ReactiveCommand<RUnit, RUnit> ClearNotifications => ReactiveCommand.Create(() => NotificationMessages.Clear());

    public NotificationMessage Notify(NotificationMessage message) {
        var @event = new NotificationEvent(clock.GetUtcNow(), message.Severity, message.Message);
        notificationMessages.Insert(0, @event);
        while (notificationMessages.Count > MaxNotifications)
            notificationMessages.RemoveAt(notificationMessages.Count - 1);

        notifications.OnNext(message);
        return message;
    }

    public void ToggleDrawer() => IsDrawerOpen = !IsDrawerOpen;
}

public abstract record AppMode
{
    readonly Dictionary<string, object?> properties = new();

    [PublicAPI]
    public Option<T> GetProperty<T>(string key)
        => properties.TryGetValue(key, out var value) && value is T x ? Some(x) : None;

    [PublicAPI]
    public AppMode SetProperty<T>(string key, T value) {
        properties[key] = value;
        return this;
    }

    public sealed record Page : AppMode
    {
        public static readonly Page Instance = new();

        public bool IsDrawerOpen { get; set; } = true;
    }

    [PublicAPI]
    public sealed record Modal(ReactiveCommand<RUnit, RUnit> OnClose) : AppMode;
}

[PublicAPI]
public readonly record struct NotificationEvent(DateTimeOffset Timestamp, Severity Severity, string Message);

[PublicAPI]
public readonly record struct NotificationMessage(Severity Severity, string Message)
{
    public static implicit operator NotificationMessage(in (Severity Severity, string Message) tuple) =>
        new(tuple.Severity, tuple.Message);
}
