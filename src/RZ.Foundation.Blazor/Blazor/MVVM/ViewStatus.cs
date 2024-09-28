
using JetBrains.Annotations;
using RZ.Foundation.Types;

namespace RZ.Foundation.Blazor.MVVM;

[PublicAPI]
public abstract record ViewStatus
{
    public sealed record Idle : ViewStatus
    {
        [PublicAPI]
        public static readonly ViewStatus Instance = new Idle();
    }

    public sealed record Loading : ViewStatus
    {
        [PublicAPI]
        public static readonly ViewStatus Instance = new Loading();
    }

    public sealed record Failed : ViewStatus
    {
        public Failed(string message) {
            Error = new ErrorInfo(StandardErrorCodes.Unhandled, message);
            Message = message;
        }

        public Failed(ErrorInfo e) {
            Error = e;
            Message = e.Message;
        }

        public Failed(ErrorInfo e, string message) : this(e) => Message = message;

        public string Message { get; }
        public ErrorInfo Error { get; }
    }
    public sealed record Ready : ViewStatus
    {
        [PublicAPI]
        public static readonly ViewStatus Instance = new Ready();
    }
}
