using System.Reactive.Concurrency;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using RZ.Foundation.Blazor.Layout;
using RZ.Foundation.Blazor.MVVM;

namespace RZ.Foundation;

[PublicAPI]
public static class BlazorSettings
{
    public static IServiceCollection AddRzBlazorSettings<TMainVm>(this IServiceCollection services) where TMainVm : MainLayoutViewModel =>
        services
           .AddSingleton<IViewFinder, ViewFinder>()
           .AddScoped<IScheduler>(_ => new SynchronizationContextScheduler(SynchronizationContext.Current!))
           .AddSingleton<IViewModelFactory, ViewModelFactory>()
           .AddScoped<IEventBubbleSubscription, EventBubbleSubscription>()
           .AddScoped<MainLayoutViewModel, TMainVm>()
           .AddScoped<ShellViewModel>();
}