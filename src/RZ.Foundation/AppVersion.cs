using System.Reflection;
using JetBrains.Annotations;

namespace RZ.Foundation;

[PublicAPI]
public static class AppVersion
{
    public static readonly string Current;
    public static readonly string Hash;

    static AppVersion() {
        var version = GetVersion(Assembly.GetEntryAssembly()!);
        if (version is null){
            Current = "(unversioned)";
            Hash = "";
        }
        else {
            var lastPlus = version.LastIndexOf('+');
            Current = lastPlus > 0 ? version[..lastPlus] : version;
            Hash = lastPlus > 0 ? version[(lastPlus + 1)..] : string.Empty;
        }
    }

    public static string? GetVersion(Assembly assembly)
        => (assembly.GetCustomAttribute(typeof(AssemblyInformationalVersionAttribute))
                as AssemblyInformationalVersionAttribute)?.InformationalVersion;

    public static string GetAppVersion(Assembly assembly)
        => GetVersion(assembly)?.Apply(TrimGitHashFromNet8) ?? "(unversioned)";

    static string TrimGitHashFromNet8(string s) {
        var lastPlus = s.LastIndexOf('+');
        return lastPlus > 0 ? s[..lastPlus] : s;
    }
}