namespace RZ.AspNet.Common;

public class LocalizationModule(string resourcesPath = "Resources", string defaultCulture = "en-US") : AppModule
{
    public override ValueTask<Unit> InstallServices(IHostApplicationBuilder builder) {
        builder.Services.AddLocalization(opts => opts.ResourcesPath = resourcesPath);
        return new(unit);
    }

    public override ValueTask<Unit> InstallMiddleware(IHostApplicationBuilder configuration, WebApplication app) {
        var opts = new RequestLocalizationOptions().SetDefaultCulture(defaultCulture);
        app.UseRequestLocalization(opts);
        return new(unit);
    }
}