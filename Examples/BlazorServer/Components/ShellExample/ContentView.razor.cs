using ReactiveUI;
using RZ.Foundation.Blazor;
using RZ.Foundation.Blazor.MVVM;
using RZ.Foundation.Blazor.Shells;

namespace RZ.Blazor.Server.Example.Components.ShellExample;

public sealed class ContentViewModel : ViewModel
{
    public ContentViewModel(ShellViewModel shell) {
        shell.NavBarMode = NavBarMode.New(NavBarType.Mini);

        OpenModal = ReactiveCommand.Create(() => {
            shell.PushModal(new PopupViewModel());
        });

        ShowNewPage = ReactiveCommand.Create(() => {
            shell.Push(new PopupViewModel());
        });

        var n = 0;
        PopupSuccess = ReactiveCommand.Create(() => {
            shell.Notify(new(MessageSeverity.Info, $"Popup Success {++n}"));
        });

        PopupFailure = ReactiveCommand.Create(() => {
            shell.Notify(new(MessageSeverity.Error, $"Popup Failure {++n}"));
        });
    }

    public ReactiveCommand<RUnit, RUnit> OpenModal { get; }
    public ReactiveCommand<RUnit, RUnit> ShowNewPage { get; }
    public ReactiveCommand<RUnit, RUnit> PopupSuccess { get; }
    public ReactiveCommand<RUnit, RUnit> PopupFailure { get; }
}