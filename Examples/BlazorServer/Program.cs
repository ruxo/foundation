global using RUnit = System.Reactive.Unit;
using MudBlazor;
using MudBlazor.Services;
using RZ.Blazor.Server.Example.Components;
using RZ.Blazor.Server.Example.Components.Home;
using RZ.Blazor.Server.Example.Components.ShellExample;
using RZ.Foundation;
using RZ.Foundation.Blazor.Shells;
using RZ.Foundation.Blazor.Views;

var builder = WebApplication.CreateBuilder(args);

builder.Services
       .AddMudServices()
       .AddRzBlazorSettings(_ => new ShellOptions {
            Navigation = [
                new Navigation.Item("Home", View.Model<WelcomeViewModel>(), "/", Icons.Material.Filled.Home),
                new Navigation.Item("Shell", View.Model<ContentViewModel>(), "/shell", Icons.Material.Filled.ShieldMoon),
                new Navigation.Item("Blank", View.Model<BlankContentViewModel>(), "/blank", Icons.Material.Filled.Foundation),
                new Navigation.DirectRoute("Direct", "/direct-route", Icons.Material.Filled.Directions)
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
