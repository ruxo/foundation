using ReactiveUI;
using RZ.Foundation.Blazor.MVVM;
using RZ.Foundation.Blazor.Shared;

namespace RZ.Foundation.Blazor.Layout.Shell;

public sealed class ShellViewModel : ViewModel
{
    readonly Stack<ViewState> content = [];
    readonly MainLayoutViewModel mainVm;

    public ShellViewModel(MainLayoutViewModel mainVm) {
        this.mainVm = mainVm;

        content.Push(new ViewState(AppMode.Page.Instance, BlankContentViewModel.Instance, ViewMode.Single.Instance));
    }

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
        mainVm.AppMode = content.Peek().AppMode;
        this.RaisePropertyChanged(nameof(ViewMode));
        this.RaisePropertyChanged(nameof(Content));
        return unit;
    }

    public NotificationMessage Notify(NotificationMessage message) =>
        mainVm.Notify(message);

    public Unit PushModal(ViewModel? viewModel = null) {
        var onClose = ReactiveCommand.Create<Unit, Unit>(_ => CloseCurrentView());
        this.RaisePropertyChanging(nameof(Content));
        this.RaisePropertyChanging(nameof(ViewMode));
        var appMode = new AppMode.Modal(onClose);
        var current = content.Peek();
        content.Push(current with {
            AppMode = appMode,
            Content = viewModel ?? current.Content
        });
        mainVm.AppMode = appMode;
        this.RaisePropertyChanged(nameof(ViewMode));
        this.RaisePropertyChanged(nameof(Content));
        return unit;
    }

    public Unit Replace(ViewModel replacement) {
        this.RaisePropertyChanging(nameof(Content));
        var current = content.Pop();
        content.Push(current with {
            Content = replacement
        });
        this.RaisePropertyChanged(nameof(Content));
        return unit;
    }
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