using Microsoft.AspNetCore.Components;

namespace RZ.Foundation.Blazor.Helpers;

public static class NavManagerExtensions
{
    public static string Path(this NavigationManager navManager)
    {
        var pathLength = navManager.Uri.Length - navManager.BaseUri.Length;
        return navManager.Uri.Substring(navManager.BaseUri.Length -1, pathLength +1);
    }
}