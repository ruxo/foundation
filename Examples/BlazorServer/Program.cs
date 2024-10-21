using MudBlazor;
using MudBlazor.Services;
using RZ.Blazor.Server.Example.Components;
using RZ.Blazor.Server.Example.Components.Home;
using RZ.Blazor.Server.Example.Components.ShellExample;
using RZ.Foundation;
using RZ.Foundation.Blazor;
using RZ.Foundation.Blazor.Shells;
using RZ.Foundation.Blazor.Views;

var builder = WebApplication.CreateBuilder(args);

builder.Services
       .AddMudServices()
       .AddRzBlazorSettings(_ => new ShellOptions {
            Navigation = [
                new Navigation.Item("Home", View.Model<WelcomeViewModel>(), "/", Icon: Icons.Material.Filled.Home, NavBar: NavBarType.Full),
                new Navigation.Item("Shell", View.Model<ContentViewModel>(), "/shell", Icon: Icons.Material.Filled.ShieldMoon, NavBar: NavBarType.Mini),
                new Navigation.Item("Blank", View.Model<BlankContentViewModel>(), "/blank", Icon: Icons.Material.Filled.Foundation),
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
