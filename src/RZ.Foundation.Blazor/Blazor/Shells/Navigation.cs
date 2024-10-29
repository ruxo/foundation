using JetBrains.Annotations;

namespace RZ.Foundation.Blazor.Shells;

[PublicAPI]
public abstract record Navigation
{
    public sealed record Item(string Title, ViewMaker View, string NavPath, string? Icon = null, bool IsPartialMatch = false, string? Policy = null,
                              ViewModeType? ViewMode = null) : Navigation;

    public sealed record Divider : Navigation
    {
        public static readonly Divider Instance = new();
    }

    public sealed record Group(string Title, string? Icon = null) : Navigation
    {
        public IEnumerable<Navigation> Items { get; init; } = [];
    }

    public sealed record DirectRoute(string Title, string NavPath, string? Icon = null, bool IsPartialMatch = false, string? Policy = null) : Navigation;
}