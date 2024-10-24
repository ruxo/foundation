using System.Reactive.Disposables;
using System.Reactive.Linq;
using MudBlazor;
using ReactiveUI;
using RZ.Foundation.Blazor.MVVM;
using RZ.Foundation.Blazor.Shells;

namespace RZ.Foundation.BlazorViews.Shells;

public class ShellNavMenuViewModel(ShellViewModel shell) : ActivatableViewModel
{
    ObservableAsPropertyHelper<DrawerVariant> variant = default!;
    ObservableAsPropertyHelper<bool> showOnHover = default!;
    ObservableAsPropertyHelper<bool> iconOnly = default!;
    ObservableAsPropertyHelper<bool> isDrawerVisible = default!;

    protected override void OnActivated(CompositeDisposable disposables) {
        isDrawerVisible = shell.WhenAnyValue(x => x.IsDrawerVisible)
                              .ToProperty(this, x => x.IsDrawerVisible)
                              .DisposeWith(disposables);
        variant = shell.WhenAnyValue(x => x.UseMiniDrawer)
                       .Select(x => x ? DrawerVariant.Mini : DrawerVariant.Persistent)
                       .ToProperty(this, x => x.Variant)
                       .DisposeWith(disposables);
        showOnHover = shell.WhenAnyValue(x => x.UseMiniDrawer)
                           .ToProperty(this, x => x.ShowOnHover)
                           .DisposeWith(disposables);
        iconOnly = shell.WhenAnyValue(x => x.UseMiniDrawer,
                                      x => x.IsDrawerOpen,
                                      (useMiniDrawer, isDrawerOpen) => useMiniDrawer && !isDrawerOpen)
                        .ToProperty(this, x => x.IconOnly)
                        .DisposeWith(disposables);
    }

    public DrawerVariant Variant => variant.Value;
    public bool ShowOnHover => showOnHover.Value;
    public bool IconOnly => iconOnly.Value;

    public bool IsDrawerOpen
    {
        get => shell.IsDrawerOpen;
        set
        {
            this.RaisePropertyChanging();
            shell.IsDrawerOpen = value;
            this.RaisePropertyChanged();
        }
    }

    public bool IsDrawerVisible => isDrawerVisible.Value;

    public IEnumerable<Navigation> NavItems => shell.NavItems;
}