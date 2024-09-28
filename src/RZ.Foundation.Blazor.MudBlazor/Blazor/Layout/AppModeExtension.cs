using MudBlazor;

namespace RZ.Foundation.Blazor.Layout;

public static class AppModeExtension
{
    public static MaxWidth? ContentWidth(this AppMode appMode)
        => appMode.GetProperty<MaxWidth>(nameof(ContentWidth)).ToNullable();

    public static AppMode ContentWidth(this AppMode appMode, MaxWidth value)
        => appMode.SetProperty(nameof(ContentWidth), value);
}