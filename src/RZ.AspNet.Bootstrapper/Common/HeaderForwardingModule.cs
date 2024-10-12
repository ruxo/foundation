using Microsoft.AspNetCore.HttpOverrides;

namespace RZ.AspNet.Common;

[PublicAPI]
public class HeaderForwardingModule(bool forwardAll) : AppModule
{
    public static readonly HeaderForwardingModule Default = new(true);
    public static readonly HeaderForwardingModule NoDefaultSettings = new(false);

    public override ValueTask<Unit> InstallServices(IHostApplicationBuilder builder) {
        if (forwardAll)
            builder.Services.Configure<ForwardedHeadersOptions>(opts => opts.ForwardedHeaders = ForwardedHeaders.All);
        return new(unit);
    }

    public override ValueTask<Unit> InstallMiddleware(IHostApplicationBuilder configuration, WebApplication app) {
        app.UseForwardedHeaders();
        return new(unit);
    }
}