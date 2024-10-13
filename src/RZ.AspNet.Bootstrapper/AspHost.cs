using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;

namespace RZ.AspNet;

[PublicAPI]
public static class AspHost
{
    public static async Task<Unit> RunPipeline(this WebApplicationBuilder builder, params AppModule[] modules) {
        foreach(var module in modules)
            await module.InstallServices(builder);

        var app = builder.Build();
        foreach(var module in modules)
            await module.InstallMiddleware(builder, app);

        await app.RunAsync();
        return unit;
    }
    const string EnvironmentKey = "ASPNETCORE_ENVIRONMENT";

    [ExcludeFromCodeCoverage]
    public static IConfiguration CreateDefaultConfigurationSettings() {
        var runningEnvironment = Optional(Environment.GetEnvironmentVariable(EnvironmentKey)).IfNone("Production");

        return new ConfigurationBuilder()
              .AddJsonFile("appsettings.json")
              .AddJsonFile($"appsettings.{runningEnvironment}.json", optional: true)
              .AddEnvironmentVariables()
              .Build();
    }
}