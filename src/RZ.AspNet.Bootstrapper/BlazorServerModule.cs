namespace RZ.AspNet;

[PublicAPI]
public class BlazorServerModule<TApp> : AppModule
{
    public static readonly BlazorServerModule<TApp> Default = new();

    public override ValueTask<Unit> InstallServices(IHostApplicationBuilder builder) {
        builder.Services.AddRazorComponents().AddInteractiveServerComponents();
        return new(unit);
    }

    public override ValueTask<Unit> InstallMiddleware(IHostApplicationBuilder configuration, WebApplication app) {
        app.MapRazorComponents<TApp>().AddInteractiveServerRenderMode();
        return new(unit);
    }
}