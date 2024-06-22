using System.Reactive.Concurrency;
using Microsoft.Extensions.DependencyInjection;
using RZ.Foundation.Blazor.Layout;
using RZ.Foundation.Blazor.Layout.Shell;
using RZ.Foundation.Blazor.MVVM;

namespace RZ.Foundation;

public static class BlazorSettings
{
    public static IServiceCollection AddRzBlazorSettings(this IServiceCollection services) =>
        services
           .AddSingleton<IViewLocator, ViewLocator>()
           .AddScoped<IScheduler>(_ => new SynchronizationContextScheduler(SynchronizationContext.Current!))
           .AddSingleton<IViewModelFactory, ViewModelFactory>()
           .AddScoped<MainLayoutViewModel>()
           .AddScoped<ShellViewModel>();
}