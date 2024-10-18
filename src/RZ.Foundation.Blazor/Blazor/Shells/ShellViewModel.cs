using System.Collections;
using System.Collections.ObjectModel;
using System.Reactive.Subjects;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using RZ.Foundation.Blazor.MVVM;
using RZ.Foundation.Blazor.Views;

namespace RZ.Foundation.Blazor.Shells;

[PublicAPI]
public class ShellViewModel : ViewModel, IEnumerable<ViewState>
{
    readonly ILogger<ShellViewModel> logger;
    readonly TimeProvider clock;
    readonly Stack<ViewState> content = [];
    readonly Subject<NotificationMessage> notifications = new();
    readonly ObservableAsPropertyHelper<int> messageCount;
    const int MaxNotifications = 20;

    bool isDarkMode;

    public ShellViewModel(ILogger<ShellViewModel> logger, TimeProvider clock, ShellOptions options) {
        this.logger = logger;
        this.clock = clock;

        InitView(options.InitialAppMode, options.InitialView, options.IsDualMode);

        messageCount = NotificationMessages.WhenAnyValue(x => x.Count).ToProperty(this, x => x.MessageCount);

        NavItems = new(options.Navigation);
    }

    public bool IsDarkMode
    {
        get => isDarkMode;
        set => this.RaiseAndSetIfChanged(ref isDarkMode, value);
    }

    public bool IsDrawerOpen
    {
        get => AppMode is AppMode.Page { IsDrawerOpen: true };
        set
        {
            if (AppMode is AppMode.Page p){
                this.RaisePropertyChanging();
                p.IsDrawerOpen = value;
                this.RaisePropertyChanged();
            }
            else
                logger.LogWarning("Cannot set drawer open state when not in page mode");
        }
    }

    public bool UseMiniDrawer{
        get => AppMode is AppMode.Page { UseMiniDrawer: true };
        set
        {
            if (AppMode is AppMode.Page p){
                this.RaisePropertyChanging();
                p.UseMiniDrawer = value;
                this.RaisePropertyChanged();
            }
            else
                logger.LogWarning("Cannot set mini drawer state when not in page mode");
        }
    }

    public AppMode AppMode => content.Peek().AppMode;
    public ViewModel Content => content.Peek().Content;
    public ViewMode ViewMode => content.Peek().ViewMode;
    public int StackCount => content.Count;

    public ObservableCollection<Navigation> NavItems { get; }

    public IObservable<NotificationMessage> Notifications => notifications;

    public ObservableCollection<NotificationEvent> NotificationMessages { get; } = new();

    public int MessageCount => messageCount.Value;

    public ReactiveCommand<RUnit, RUnit> ToggleDrawer => ReactiveCommand.Create(() => { IsDrawerOpen = !IsDrawerOpen; });

    public void InitView(AppMode? initialAppMode, ViewModel? viewModel, bool isDualMode, bool? isDrawerOpen = null) {
        content.Clear();
        var mode = initialAppMode ?? AppMode.Page.Default;
        if (mode is AppMode.Page p && isDrawerOpen.HasValue)
            p.IsDrawerOpen = isDrawerOpen.Value;
        content.Push(new(mode,
                         viewModel ?? BlankContentViewModel.Instance,
                         isDualMode ? new ViewMode.Dual() : ViewMode.Single.Instance));
    }

    #region Dual mode

    public bool TrySetRightPanel(ViewModel? viewModel) {
        var state = content.Peek();
        if (state.ViewMode is not ViewMode.Dual)
            return false;

        this.RaisePropertyChanging(nameof(Content));
        this.RaisePropertyChanging(nameof(ViewMode));
        content.Pop();
        content.Push(state with {
            ViewMode = new ViewMode.Dual { DetailPanel = viewModel }
        });
        this.RaisePropertyChanged(nameof(ViewMode));
        this.RaisePropertyChanged(nameof(Content));
        return true;
    }

    #endregion

    public Unit CloseCurrentView()
        => ChangingStack(() => content.Pop());

    public NotificationMessage Notify(NotificationMessage message) {
        NotificationMessages.Add(new(clock.GetLocalNow(), message.Severity, message.Message));
        return message;
    }

    public Unit CloneState(Func<ViewState, ViewState> stateBuilder) => ChangingStack(() => {
        var newState = stateBuilder(content.Peek());
        content.Push(newState);
    });

    public Unit PushModal(ViewModel? viewModel, Func<AppMode, AppMode> appModeGetter) => ChangingStack(() => {
        var current = content.Peek();
        var appMode = appModeGetter(current.AppMode);

        content.Push(current with {
            AppMode = appMode,
            Content = viewModel ?? current.Content
        });
    });

    public Unit Push(ViewModel viewModel)
        => CloneState(current => current with { Content = viewModel });

    public Unit PushModal(ViewModel? viewModel = null, ReactiveCommand<RUnit, RUnit>? onClose = default) {
        onClose ??= ReactiveCommand.Create(() => {
            CloseCurrentView();
        });
        return PushModal(viewModel, _ => new AppMode.Modal(onClose));
    }

    public Unit Replace(ViewModel replacement, AppMode? appMode = default) {
        this.RaisePropertyChanging(nameof(Content));
        if (appMode is not null)
            this.RaisePropertyChanging(nameof(AppMode));
        var current = content.Pop();
        content.Push(current with {
            Content = replacement,
            AppMode = appMode ?? current.AppMode
        });
        if (appMode is not null)
            this.RaisePropertyChanged(nameof(AppMode));
        this.RaisePropertyChanged(nameof(Content));
        return unit;
    }

    public IEnumerator<ViewState> GetEnumerator() => content.GetEnumerator();

    Unit ChangingStack(Action action) {
        this.RaisePropertyChanging(nameof(Content));
        this.RaisePropertyChanging(nameof(ViewMode));
        this.RaisePropertyChanging(nameof(AppMode));
        this.RaisePropertyChanging(nameof(StackCount));
        this.RaisePropertyChanging(nameof(IsDrawerOpen));
        this.RaisePropertyChanging(nameof(UseMiniDrawer));
        action();
        this.RaisePropertyChanged(nameof(UseMiniDrawer));
        this.RaisePropertyChanged(nameof(IsDrawerOpen));
        this.RaisePropertyChanged(nameof(StackCount));
        this.RaisePropertyChanged(nameof(AppMode));
        this.RaisePropertyChanged(nameof(ViewMode));
        this.RaisePropertyChanged(nameof(Content));
        return unit;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public sealed record ViewState(AppMode AppMode, ViewModel Content, ViewMode ViewMode);

public abstract record ViewMode
{
    public sealed record Single : ViewMode
    {
        public static readonly ViewMode Instance = new Single();
    }

    public sealed record Dual : ViewMode
    {
        public ViewModel? DetailPanel { get; init; }
    }
}
