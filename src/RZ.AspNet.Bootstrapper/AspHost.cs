namespace RZ.AspNet;

public static class AspHost
{
    public static async ValueTask<Unit> RunPipeline(this WebApplicationBuilder builder, params AppModule[] modules) {
        foreach(var module in modules)
            await module.InstallServices(builder);

        var app = builder.Build();
        foreach(var module in modules)
            await module.InstallMiddleware(builder, app);

        await app.RunAsync();
        return unit;
    }
}