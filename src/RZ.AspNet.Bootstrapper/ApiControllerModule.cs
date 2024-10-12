using RZ.Foundation;

namespace RZ.AspNet;

[PublicAPI]
public class ApiControllerModule : AppModule
{
    public static readonly ApiControllerModule Default = new();

    public override ValueTask<Unit> InstallServices(IHostApplicationBuilder builder) {
        builder.Services.AddControllers();
        return new(unit);
    }

    public override ValueTask<Unit> InstallMiddleware(IHostApplicationBuilder configuration, WebApplication app) {
        app.MapControllers();
        app.MapGet("/system/version", AppVersion.GetAppVersion);
        return new(unit);
    }
}