namespace RZ.AspNet;

public class BlazorServerModule<TApp> : AppModule
{
    public override ValueTask<Unit> InstallServices(IHostApplicationBuilder builder) {
        builder.Services.AddRazorComponents().AddInteractiveServerComponents();
        return new(unit);
    }

    public override ValueTask<Unit> InstallMiddleware(IHostApplicationBuilder configuration, WebApplication app) {
        app.MapRazorComponents<TApp>().AddInteractiveServerRenderMode();
        return new(unit);
    }
}