using JetBrains.Annotations;
using ReactiveUI;

namespace RZ.Foundation.Blazor;

public abstract record AppMode
{
    readonly Dictionary<string, object?> properties = new();

    [PublicAPI]
    public Option<T> GetProperty<T>(string key)
        => properties.TryGetValue(key, out var value) && value is T x ? Some(x) : None;

    [PublicAPI]
    public AppMode SetProperty<T>(string key, T value) {
        properties[key] = value;
        return this;
    }

    public sealed record Page : AppMode
    {
        public static readonly Page Instance = new();

        public bool IsDrawerOpen { get; set; } = true;
    }

    [PublicAPI]
    public sealed record Modal(ReactiveCommand<RUnit, RUnit> OnClose) : AppMode;
}