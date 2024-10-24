using ReactiveUI;
using RZ.Foundation.Blazor;
using RZ.Foundation.Blazor.MVVM;
using RZ.Foundation.Blazor.Shells;

namespace RZ.Blazor.Server.Example.Components.ShellExample;

public sealed class ContentViewModel : ViewModel
{
    public ContentViewModel(ShellViewModel shell) {
        shell.NavBarMode = NavBarMode.New(NavBarType.Mini);

        OpenPopup = ReactiveCommand.Create(() => {
            shell.PushModal(new PopupViewModel());
        });
    }

    public ReactiveCommand<RUnit, RUnit> OpenPopup { get; }
}