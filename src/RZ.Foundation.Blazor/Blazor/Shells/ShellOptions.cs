using JetBrains.Annotations;
using RZ.Foundation.Blazor.MVVM;

namespace RZ.Foundation.Blazor.Shells;

public sealed record ShellOptions
{
    public ViewModel? InitialView { get; init; }
    public AppMode? InitialAppMode { get; init; }
    public bool IsDualMode { get; init; }
    public IEnumerable<Navigation> Navigation { get; init; } = [];
}

[PublicAPI]
public abstract record Navigation
{
    public sealed record Item(string Title, string NavPath, string? Icon = null, bool IsPartialMatch = false, string? Policy = null)
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