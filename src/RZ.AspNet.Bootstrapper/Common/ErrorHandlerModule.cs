namespace RZ.AspNet.Common;

[PublicAPI]
public class ErrorHandlerModule : AppModule
{
    public static readonly ErrorHandlerModule Default = new();

    public override ValueTask<Unit> InstallMiddleware(IHostApplicationBuilder configuration, WebApplication app) {
        if (!app.Environment.IsDevelopment()){
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            app.UseHsts();
        }
        return new(unit);
    }
}