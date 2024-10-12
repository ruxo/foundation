namespace RZ.AspNet.Common;

[PublicAPI]
public class LocalizationModule(string resourcesPath = "Resources", string defaultCulture = "en-US", params string[] supportedCultures)
    : AppModule
{
    public override ValueTask<Unit> InstallServices(IHostApplicationBuilder builder) {
        builder.Services.AddLocalization(opts => opts.ResourcesPath = resourcesPath);
        return new(unit);
    }

    public override ValueTask<Unit> InstallMiddleware(IHostApplicationBuilder configuration, WebApplication app) {
        var allCultures = supportedCultures.Append(defaultCulture).Distinct().ToArray();
        var opts = new RequestLocalizationOptions()
                  .SetDefaultCulture(defaultCulture)
                  .AddSupportedCultures(allCultures)
                  .AddSupportedUICultures(allCultures);
        app.UseRequestLocalization(opts);
        return new(unit);
    }
}