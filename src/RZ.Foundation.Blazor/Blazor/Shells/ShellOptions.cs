using JetBrains.Annotations;
using RZ.Foundation.Blazor.MVVM;

namespace RZ.Foundation.Blazor.Shells;

public delegate ViewModel ViewMaker(IViewModelFactory factory);

public sealed record ShellOptions
{
    public IEnumerable<Navigation> Navigation { get; init; } = [];
}

[PublicAPI]
public abstract record Navigation
{
    public sealed record Item(string Title, ViewMaker View, string NavPath, bool IsPartialMatch = false,
                              string? Icon = null, NavBarType? NavBar = null, ViewModeType? ViewMode = null,
                              string? Policy = null)
        : Navigation;

    public sealed record Divider : Navigation
    {
        public static readonly Divider Instance = new();
    }

    public sealed record Group(string Title, string? Icon = null) : Navigation
    {
        public IEnumerable<Navigation> Items { get; init; } = [];
    }
}