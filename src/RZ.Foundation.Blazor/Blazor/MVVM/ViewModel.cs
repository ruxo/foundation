using System.Reactive.Disposables;
using ReactiveUI;

namespace RZ.Foundation.Blazor.MVVM;

public abstract class ViewModel : ReactiveObject;

public abstract class ViewModelDisposable : ViewModel, IDisposable
{
    protected CompositeDisposable Disposables { get; } = new();

    public void Dispose() {
        Disposables.Dispose();
        GC.SuppressFinalize(this);
    }
}