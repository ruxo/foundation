using System.Reactive.Disposables;
using JetBrains.Annotations;
using ReactiveUI;

namespace RZ.Foundation.Blazor.MVVM;

[PublicAPI]
public abstract class ViewModel : ReactiveObject
{
    public virtual void ViewOnScreen() {}
    public virtual void ViewOffScreen() {}
}

[PublicAPI]
public abstract class ActivatableViewModel : ViewModel, IActivatableViewModel
{
    public ActivatableViewModel() {
        this.WhenActivated(OnActivated);
    }

    public ViewModelActivator Activator { get; } = new();

    protected abstract void OnActivated(CompositeDisposable disposables);
}