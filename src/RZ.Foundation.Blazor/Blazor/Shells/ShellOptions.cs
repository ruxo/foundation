using RZ.Foundation.Blazor.MVVM;

namespace RZ.Foundation.Blazor.Shells;

public delegate ViewModel ViewMaker(IViewModelFactory factory);

public sealed record ShellOptions
{
    public NavBarMode InitialNavBar { get; set; } = NavBarMode.New(NavBarType.Full);
    public IEnumerable<Navigation> Navigation { get; init; } = [];
}