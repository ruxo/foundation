using System.Reactive.Concurrency;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using RZ.Foundation.Blazor.MVVM;
using RZ.Foundation.Blazor.Shells;

namespace RZ.Foundation;

[PublicAPI]
public static class BlazorSettings
{
    public static IServiceCollection AddRzBlazorSettings(this IServiceCollection services)
        => services
          .AddSingleton<IViewFinder, ViewFinder>()
          .AddScoped<IScheduler>(_ => new SynchronizationContextScheduler(SynchronizationContext.Current!))
          .AddSingleton<IViewModelFactory, ViewModelFactory>()
          .AddScoped<IEventBubbleSubscription, EventBubbleSubscription>()
          .AddScoped<ShellViewModel>();
}