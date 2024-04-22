using System.Runtime.CompilerServices;
using LanguageExt.Common;

namespace RZ.Foundation.Functional;

public static class StandardErrors
{
    public const int NotFoundCode = 1_000_000;
    public const int DuplicatedCode = 1_000_001;

    /// <summary>
    /// Code reaches a state that should not be possible by domain rules (indicate a bug in the code)
    /// </summary>
    public const int UnexpectedCode = 1_000_002;

    public const int StackTraceCode = 1_100_002;

    public static Error NotFoundFromKey(string key) => (NotFoundCode, $"Key [{key}] is not found");
    public static Error UnexpectedError(string message)  => (UnexpectedCode, message);

    public static readonly Error NotFound = (NotFoundCode, "Not Found");
    public static readonly Error Duplicated = (DuplicatedCode, "Item is duplicated");
    public static readonly Error Unexpected = (UnexpectedCode, "Unexpected error");

    public static Error StackTrace(string message) => (StackTraceCode, message);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Error WithMessage(this Error e, string message) => (e.Code, message);
}