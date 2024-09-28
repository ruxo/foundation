using System.Collections;
using JetBrains.Annotations;
using ReactiveUI;
using RZ.Foundation.Blazor.MVVM;
using RZ.Foundation.Blazor.Views;

namespace RZ.Foundation.Blazor.Layout;

[PublicAPI]
public class ShellViewModel : ViewModel, IEnumerable<ViewState>
{
    readonly Stack<ViewState> content = [];
    readonly MainLayoutViewModel mainVm;

    public ShellViewModel(MainLayoutViewModel mainVm) {
        this.mainVm = mainVm;

        content.Push(new ViewState(AppMode.Page.Instance, BlankContentViewModel.Instance, ViewMode.Single.Instance));
    }

    public AppMode AppMode => content.Peek().AppMode;
    public ViewModel Content => content.Peek().Content;
    public ViewMode ViewMode => content.Peek().ViewMode;

    public void InitView(ViewModel viewModel, bool isDualMode, Option<AppMode> initAppMode = default) {
        content.Clear();

        initAppMode.IfSome(m => mainVm.AppMode = m);
        var view = isDualMode ? new ViewMode.Dual() : ViewMode.Single.Instance;
        content.Push(new(mainVm.AppMode, viewModel, view));
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

    public Unit CloseCurrentView() {
        this.RaisePropertyChanging(nameof(Content));
        this.RaisePropertyChanging(nameof(ViewMode));
        content.Pop();

        this.RaisePropertyChanging(nameof(AppMode));
        mainVm.AppMode = content.Peek().AppMode;
        this.RaisePropertyChanged(nameof(AppMode));

        this.RaisePropertyChanged(nameof(ViewMode));
        this.RaisePropertyChanged(nameof(Content));
        return unit;
    }

    public NotificationMessage Notify(NotificationMessage message)
        => mainVm.Notify(message);

    public Unit CloneState(Func<ViewState, ViewState> stateBuilder) {
        this.RaisePropertyChanging(nameof(Content));
        this.RaisePropertyChanging(nameof(ViewMode));
        this.RaisePropertyChanging(nameof(AppMode));
        var newState = stateBuilder(content.Peek());
        content.Push(newState);
        mainVm.AppMode = newState.AppMode;
        this.RaisePropertyChanged(nameof(AppMode));
        this.RaisePropertyChanged(nameof(ViewMode));
        this.RaisePropertyChanged(nameof(Content));
        return unit;
    }

    public Unit PushModal(ViewModel? viewModel, Func<AppMode, AppMode> appModeGetter) {
        this.RaisePropertyChanging(nameof(Content));
        this.RaisePropertyChanging(nameof(ViewMode));
        var current = content.Peek();
        var appMode = appModeGetter(current.AppMode);

        this.RaisePropertyChanging(nameof(AppMode));
        content.Push(current with {
            AppMode = appMode,
            Content = viewModel ?? current.Content
        });
        mainVm.AppMode = appMode;

        this.RaisePropertyChanged(nameof(AppMode));
        this.RaisePropertyChanged(nameof(ViewMode));
        this.RaisePropertyChanged(nameof(Content));
        return unit;
    }

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
        var current = content.Pop();
        content.Push(current with {
            Content = replacement,
            AppMode = appMode ?? current.AppMode
        });
        if (appMode is not null){
            this.RaisePropertyChanging(nameof(AppMode));
            mainVm.AppMode = appMode;
            this.RaisePropertyChanged(nameof(AppMode));
        }
        this.RaisePropertyChanged(nameof(Content));
        return unit;
    }

    public IEnumerator<ViewState> GetEnumerator() => content.GetEnumerator();

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
