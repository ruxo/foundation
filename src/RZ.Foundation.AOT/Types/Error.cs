using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using LanguageExt.Common;
using Seq = LanguageExt.Seq;

namespace RZ.Foundation.Types;

public readonly record struct ErrorInfoLocation(string File, string Method, int Line, String comment);

/// <summary>
/// Generic Error Information
/// </summary>
public sealed record ErrorInfo
{
    public static readonly ErrorInfo NotFound = new(StandardErrorCodes.NotFound);

    [PublicAPI]
    public readonly record struct StackInfo(string? Value)
    {
        public static implicit operator StackInfo(string? stackInfo) => new(stackInfo);
        public static implicit operator StackInfo(Option<string> stackInfo) => new(stackInfo.ToNullable());
        public static implicit operator StackInfo(Exception e) => new(e.StackTrace);
        public static implicit operator StackInfo(StackTrace stack) => new(stack.ToString());
    }

    public ErrorInfo(string code, string? message = null, string? debugInfo = null, string? data = null,
              ErrorInfo? innerError = null, IEnumerable<ErrorInfo>? subErrors = null, string? stack = null, string? traceId = null,
              IEnumerable<ErrorInfoLocation>? locations = null) {
        (Code, Message, TraceId, DebugInfo, Data, InnerError, SubErrors, Stack) =
            (code, message ?? code, traceId ?? Activity.Current?.Id, debugInfo, data, innerError, subErrors, stack);
        Locations = ImmutableList<ErrorInfoLocation>.Empty.AddRange(locations ?? []);
    }

    /// <summary>
    /// Error code
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Relevant error message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Distributed trace ID
    /// </summary>
    public string? TraceId { get; init; } = Activity.Current?.Id;

    /// <summary>
    /// Information for debug. This should not be shown in Release mode.
    /// </summary>
    public string? DebugInfo { get; init;  }

    /// <summary>
    /// Serialized associated data
    /// </summary>
    public string? Data { get; }

    /// <summary>
    /// Parent error
    /// </summary>
    public ErrorInfo? InnerError { get; }

    /// <summary>
    /// Aggregation of sub-errors
    /// </summary>
    public IEnumerable<ErrorInfo>? SubErrors { get; }

    /// <summary>
    /// Stack trace, if needed. It shouldn't be used in Release mode.
    /// </summary>
    public string? Stack { get; init; }

    public ImmutableList<ErrorInfoLocation> Locations { get; }

    public ErrorInfo Trace(string description = ".", [CallerMemberName] string caller = "", [CallerLineNumber] int line = 0, [CallerFilePath] string file = "")
        => With(locations: Locations.Add(new(file, caller, line, description)));

    public bool Equals(ErrorInfo? other) {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return Code == other.Code
            && Message == other.Message
            && TraceId == other.TraceId
            && DebugInfo == other.DebugInfo
            && Data == other.Data
            && InnerError?.Equals(other.InnerError) != false
            && (SubErrors ?? []).SequenceEqual(other.SubErrors ?? [])
            && Stack == other.Stack
            && Locations.SequenceEqual(other.Locations);
    }

    public override int GetHashCode() {
        var hashCode = new HashCode();
        hashCode.Add(Code);
        hashCode.Add(Message);
        hashCode.Add(TraceId);
        hashCode.Add(DebugInfo);
        hashCode.Add(Data);
        hashCode.Add(InnerError);
        hashCode.Add(SubErrors?.Sum(e => e.GetHashCode()) ?? 0);
        hashCode.Add(Stack);
        hashCode.Add(Locations.Sum(e => e.GetHashCode()));
        return hashCode.ToHashCode();
    }

    [Pure]
    public bool Is(string code)
        => Code == code || InnerError?.Is(code) == true || SubErrors?.Any(e => e.Is(code)) == true;

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Is(ErrorInfo another) => Is(another.Code);

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsNotFound() => Is(StandardErrorCodes.NotFound);

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ErrorInfo Wrap(string code, string? message = null, string? debugInfo = null, string? data = null, string? traceId = null)
        => new(code, message ?? Code, debugInfo ?? DebugInfo, data ?? Data, this, subErrors: null, traceId ?? TraceId);

    [Pure]
    public string ToString(JsonTypeInfo<ErrorInfo>? options)
        => JsonSerializer.Serialize(this, options ?? ErrorInfoJsonContext.Default.ErrorInfo);

    [Pure]
    public override string ToString() => ToString(null);

    [Pure]
    public static Option<ErrorInfo> TryParse(string s, JsonTypeInfo<ErrorInfo>? options = null)
        => Try(s, json => JsonSerializer.Deserialize<ErrorInfo>(json, options ?? ErrorInfoJsonContext.Default.ErrorInfo)!).ToOption();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Throw<T>() => throw new ErrorInfoException(this);

    [PublicAPI, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ErrorInfo NoDebug()
        => With(debug: None, stack: None, inner: None, subErrors: None);

    public ErrorInfo With(string? code = null, Option<string>? debug = null, Option<StackInfo>? stack = null,
                          Option<ErrorInfo>? inner = null, Option<Seq<ErrorInfo>>? subErrors = null,
                          IEnumerable<ErrorInfoLocation>? locations = null)
        => new(code ?? Code, Message, (debug ?? Optional(DebugInfo)).ToNullable(), Data,
               (inner ?? InnerError).ToNullable(),
               subErrors is null ? SubErrors : subErrors.Value.ToNullable(),
               (stack?.Bind(s => Optional(s.Value)) ?? Optional(Stack)).ToNullable(), TraceId, locations ?? Locations);
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
                             DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(ErrorInfo))]
public partial class ErrorInfoJsonContext : JsonSerializerContext;

[PublicAPI]
public sealed class ErrorInfoException : ApplicationException
{
    public ErrorInfoException(string code, string? message = null, string? debugInfo = null, string? data = null, Exception? innerException = null,
                              IEnumerable<ErrorInfo>? subErrors = null) : base(message ?? code, innerException)
        => (Code, DebugInfo, AdditionalData, SubErrors) = (code, debugInfo, data, subErrors);

    public ErrorInfoException(Exception e) : this(ErrorFrom.Exception(e)) { }

    public ErrorInfoException(ErrorInfo ei) : base(ei.Message,
        Optional(ei.InnerError).Map(inner => new ErrorInfoException(inner)).ToNullable()) =>
        (Code, DebugInfo, AdditionalData, SubErrors) = (ei.Code, ei.DebugInfo, ei.Data, ei.SubErrors);

    public string Code { get; }

    /// <summary>
    /// object represents a debug information
    /// </summary>
    public string? DebugInfo { get; }

    /// <summary>
    /// object for additional data
    /// </summary>
    public string? AdditionalData { get; }

    public IEnumerable<ErrorInfo>? SubErrors { get; }

    public ErrorInfo ToErrorInfo() =>
        new(Code,
            Message,
            DebugInfo,
            AdditionalData,
            InnerException is ErrorInfoException ei
                ? ei.ToErrorInfo()
                : Optional(InnerException).Map(ErrorFrom.Exception).ToNullable(),
            SubErrors,
            StackTrace);
}

/// <summary>
/// Decorate any exception class to allow conversion to <see cref="ErrorInfo"/>
/// </summary>
[AttributeUsage(AttributeTargets.Class), PublicAPI]
public sealed class ErrorInfoAttribute(string code) : Attribute
{
    public string Code { get; } = code;
}

[PublicAPI]
public static class ErrorFrom
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ErrorInfo Program(string message, [CallerMemberName] string? caller = null) =>
        new(InvalidRequest, $"{caller}: {message}");

    public static ErrorInfo Exception(Exception e) {
        if (e is ErrorInfoException ei) return ei.ToErrorInfo();

        var errorInfoAttr = e.GetType().GetCustomAttribute<ErrorInfoAttribute>()?.Code;
        return new ErrorInfo(errorInfoAttr ?? Unhandled,
                             e.Message,
                             innerError: e.InnerException?.Apply(Exception),
                             subErrors: e.InnerException is AggregateException ae ? ae.InnerExceptions.Map(Exception) : null,
                             stack: e.StackTrace);
    }

    public static ErrorInfo Exception(Exception e, string message, string? data = null, string? debugInfo = null,
                                      [CallerMemberName] string? caller = null) {
        var error = Exception(e);
        return new ErrorInfo(error.Code, $"[{caller}] {message}", data: data, debugInfo: debugInfo, innerError: error);
    }

    public static ErrorInfo Exception(Error e) {
        var errorInfoAttr = from ex in e.Exception
                            from attr in Optional(ex.GetType().GetCustomAttribute<ErrorInfoAttribute>())
                            select attr.Code;
        var subErrors = e is ManyErrors me ? me.Errors.Map(Exception) : Seq.empty<ErrorInfo>();
        return new ErrorInfo(errorInfoAttr.IfNone(Unhandled),
                             e.Message,
                             innerError: e.Inner.Map(Exception).ToNullable(),
                             subErrors: subErrors.IsEmpty ? null : subErrors,
                             stack: e.Exception.ToNullable()?.StackTrace);
    }
}