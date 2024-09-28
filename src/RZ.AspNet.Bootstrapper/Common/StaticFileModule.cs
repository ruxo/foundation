namespace RZ.AspNet.Common;

public class StaticFileModule : AppModule
{
    public static readonly StaticFileModule Default = new();

    public override ValueTask<Unit> InstallMiddleware(IHostApplicationBuilder configuration, WebApplication app) {
        app.UseStaticFiles();
        return new(unit);
    }
}