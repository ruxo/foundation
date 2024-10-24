using RZ.Foundation.Blazor;
using RZ.Foundation.Blazor.MVVM;
using RZ.Foundation.Blazor.Shells;

namespace RZ.Blazor.Server.Example.Components.Home;

public sealed class WelcomeViewModel : ViewModel
{
    public WelcomeViewModel(ShellViewModel shell) {
        shell.NavBarMode = NavBarMode.New(NavBarType.Full);
    }
}