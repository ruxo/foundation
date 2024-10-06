using MudBlazor;
using MudBlazor.Services;
using RZ.Blazor.Server.Example.Components;
using RZ.Foundation;
using RZ.Foundation.Blazor;
using RZ.Foundation.Blazor.Shells;

var builder = WebApplication.CreateBuilder(args);

builder.Services
       .AddMudServices()
       .AddRzBlazorSettings(_ => new ShellOptions {
            InitialAppMode = AppMode.Page.DefaultDrawerOff,
            Navigation = [
                new Navigation.Item("Home", "/", Icons.Material.Filled.Home),
                new Navigation.Item("Shell", "/shell", Icons.Material.Filled.ShieldMoon)
            ]
        })
       .AddRazorComponents()
       .AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
