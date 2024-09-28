using System.Reflection;
using JetBrains.Annotations;

namespace RZ.Foundation;

[PublicAPI]
public static class AppVersion
{
    public static readonly string Current = GetAppVersion(Assembly.GetEntryAssembly()!);

    public static string GetAppVersion(Assembly assembly) =>
        Optional(assembly.GetCustomAttribute(typeof(AssemblyInformationalVersionAttribute))
                     as AssemblyInformationalVersionAttribute)
           .Map(a => TrimGitHashFromNet8(a.InformationalVersion))
           .IfNone("(unversioned)");

    static string TrimGitHashFromNet8(string s) {
        var lastPlus = s.LastIndexOf('+');
        return lastPlus > 0 ? s[..lastPlus] : s;
    }
}