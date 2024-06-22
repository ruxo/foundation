using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
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

public sealed class MainLayoutViewModel : ViewModel
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

    public ReactiveCommand<Unit, Unit> ClearNotifications => ReactiveCommand.Create<Unit, Unit>(_ => {
        NotificationMessages.Clear();
        return unit;
    });

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
    public sealed record Page : AppMode
    {
        public static readonly Page Instance = new();

        public bool IsDrawerOpen { get; set; } = true;
    }

    public sealed record Modal(ReactiveCommand<Unit, Unit> OnClose) : AppMode;
}

public readonly record struct NotificationEvent(DateTimeOffset Timestamp, Severity Severity, string Message);

public readonly record struct NotificationMessage(Severity Severity, string Message)
{
    public static implicit operator NotificationMessage(in (Severity Severity, string Message) tuple) =>
        new(tuple.Severity, tuple.Message);
}