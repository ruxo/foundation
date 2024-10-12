namespace RZ.AspNet.Common;

[PublicAPI]
public class AuthorizationModuleBase : AppModule
{
    public override ValueTask<Unit> InstallMiddleware(IHostApplicationBuilder configuration, WebApplication app) {
        app.UseAuthentication();
        app.UseAuthorization();
        return new(unit);
    }
}