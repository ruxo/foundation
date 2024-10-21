using JetBrains.Annotations;
using ReactiveUI;

namespace RZ.Foundation.Blazor;

[PublicAPI]
public abstract record AppMode
{
    readonly Dictionary<string, object?> properties = new();

    public Option<T> GetProperty<T>(string key)
        => properties.TryGetValue(key, out var value) && value is T x ? Some(x) : None;

    public AppMode SetProperty<T>(string key, T value) {
        properties[key] = value;
        return this;
    }

    public sealed record Page : AppMode
    {
        public static readonly Page Default = new();
    }

    public sealed record Modal(ReactiveCommand<RUnit, RUnit> OnClose) : AppMode;
}