using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace RZ.Foundation.Helpers;

[PublicAPI]
public static class RzActivity
{
    extension(ActivitySource source)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Activity? Start(string? displayName = null,ActivityKind kind = ActivityKind.Server, [CallerMemberName] string name = "")
            => source.StartActivity(displayName is null ? name : $"{name}: {displayName}", kind);

        public Activity? Link(ActivityId? parentId, string? displayName = null, ActivityKind kind = ActivityKind.Internal, bool skipModule = false,
                              [CallerFilePath] string? module = null, [CallerMemberName] string name = "") {
            var activityName = displayName is null ? name : $"{name}: {displayName}";
            var activity = parentId is null
                               ? null
                               : source.StartActivity(activityName, kind, ActivityContext.Parse(parentId.Value.Value, traceState: null));
            if (!skipModule && module is not null)
                activity?.AddTag("module", module);
            return activity;
        }
    }
}