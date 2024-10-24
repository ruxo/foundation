using ReactiveUI;
using RZ.Foundation.Blazor.MVVM;
using RZ.Foundation.Blazor.Shells;

namespace RZ.Blazor.Server.Example.Components.ShellExample;

public sealed class ContentViewModel(ShellViewModel shell) : ViewModel
{
    public ReactiveCommand<RUnit, RUnit> OpenPopup { get; } = ReactiveCommand.Create(() => {
        shell.PushModal(new PopupViewModel());
    });
}