namespace RZ.AspNet;

[PublicAPI]
public class AppModule
{
    public virtual ValueTask<Unit> InstallServices(IHostApplicationBuilder builder) => new(unit);
    public virtual ValueTask<Unit> InstallMiddleware(IHostApplicationBuilder configuration, WebApplication app) => new(unit);

    sealed class QuickAppModule(Func<IHostApplicationBuilder, ValueTask<Unit>>? serviceInstaller,
                                Func<IHostApplicationBuilder, WebApplication, ValueTask<Unit>>? middlewareInstaller) : AppModule
    {
        public override ValueTask<Unit> InstallServices(IHostApplicationBuilder builder)
            => serviceInstaller?.Invoke(builder) ?? new(unit);

        public override ValueTask<Unit> InstallMiddleware(IHostApplicationBuilder configuration, WebApplication app)
            => middlewareInstaller?.Invoke(configuration, app) ?? new(unit);
    }

    public static AppModule Of(Func<IHostApplicationBuilder, ValueTask<Unit>>? serviceInstaller,
                               Func<IHostApplicationBuilder, WebApplication, ValueTask<Unit>>? middlewareInstaller)
        => new QuickAppModule(serviceInstaller, middlewareInstaller);
}