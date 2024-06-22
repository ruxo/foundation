using Microsoft.Extensions.DependencyInjection;

namespace RZ.Foundation.Blazor.MVVM;

public interface IViewModelFactory
{
    T Create<T>(params object[] args) where T : ViewModel;
}

public sealed class ViewModelFactory(IServiceProvider serviceProvider) : IViewModelFactory
{
    public T Create<T>(params object[] args) where T : ViewModel =>
        ActivatorUtilities.CreateInstance<T>(serviceProvider, args);
}