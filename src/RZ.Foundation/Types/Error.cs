using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using LanguageExt.Common;
using RZ.Foundation.Json;
using Seq = LanguageExt.Seq;

namespace RZ.Foundation.Types;

/// <summary>
/// Generic Error Information
/// </summary>
public sealed record ErrorInfo
{
    public readonly record struct StackInfo(string? Value)
    {
        public static implicit operator StackInfo(string? stackInfo) => new(stackInfo);
        public static implicit operator StackInfo(Option<string> stackInfo) => new(stackInfo.ToNullable());
        public static implicit operator StackInfo(Exception e) => new(e.StackTrace);
        public static implicit operator StackInfo(StackTrace stack) => new(stack.ToString());
    }

    public ErrorInfo(string code, string? message = default, object? debugInfo = null, object? data = null,
              ErrorInfo? innerError = default, IEnumerable<ErrorInfo>? subErrors = default, string? stack = default, string? traceId = default) =>
        (Code, Message, TraceId, DebugInfo, Data, InnerError, SubErrors, Stack) =
        (code, message ?? code, traceId ?? Activity.Current?.Id, debugInfo, data, innerError, subErrors, stack);

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
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TraceId { get; init; } = Activity.Current?.Id;

    /// <summary>
    /// **JSON SERIALIZABLE** Information for debug. This should not be shown in Release mode.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? DebugInfo { get; init;  }

    /// <summary>
    /// **JSON SERIALIZABLE** Serialized associated data
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Data { get; }

    /// <summary>
    /// Parent error
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ErrorInfo? InnerError { get; }

    /// <summary>
    /// Aggregation of sub-errors
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IEnumerable<ErrorInfo>? SubErrors { get; }

    /// <summary>
    /// Stack trace, if needed. It shouldn't be used in Release mode.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Stack { get; init; }

    [Pure]
    public bool Is(string code)
        => Code == code || InnerError?.Is(code) == true || SubErrors?.Any(e => e.Is(code)) == true;

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Is(ErrorInfo another) => Is(another.Code);

    [Pure]
    public string ToString(JsonSerializerOptions? options) => JsonSerializer.Serialize(this, options);

    [Pure]
    public override string ToString() => ToString(null);

    static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions().UseRzRecommendedSettings();

    [Pure]
    public static Option<ErrorInfo> TryParse(string s) =>
        Try(() => JsonSerializer.Deserialize<ErrorInfo>(s, SerializerOptions)!).ToOption();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Throw<T>() => throw new ErrorInfoException(this);
}

public sealed class ErrorInfoException : ApplicationException
{
    public ErrorInfoException(string code, string? message = null, object? debugInfo = null, object? data = null, Exception? innerException = null,
                              IEnumerable<ErrorInfo>? subErrors = null) : base(message ?? code, innerException)
        => (Code, DebugInfo, AdditionalData, SubErrors) = (code, debugInfo, data, subErrors);

    public ErrorInfoException(Exception e) : this(ErrorFrom.Exception(e)) {
    }

    public ErrorInfoException(ErrorInfo ei) : base(ei.Message,
        Optional(ei.InnerError).Map(inner => new ErrorInfoException(inner)).ToNullable()) =>
        (Code, DebugInfo, AdditionalData, SubErrors) = (ei.Code, ei.DebugInfo, ei.Data, ei.SubErrors);

    public string Code { get; }

    /// <summary>
    /// **JSON SERIALIZABLE** object represents a debug information
    /// </summary>
    public object? DebugInfo { get; }

    /// <summary>
    /// **JSON SERIALIZABLE** object for additional data
    /// </summary>
    public object? AdditionalData { get; }

    public IEnumerable<ErrorInfo>? SubErrors { get; }

    public ErrorInfo ToErrorInfo() =>
        new(Code,
            Message,
            DebugInfo,
            AdditionalData,
            InnerException is ErrorInfoException ei
                ? ei.ToErrorInfo()
                : Optional(InnerException).Map(e => ErrorFrom.Exception(e)).ToNullable(),
            SubErrors,
            StackTrace);
}

/// <summary>
/// Decorate any exception class to allow conversion to <see cref="ErrorInfo"/>
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ErrorInfoAttribute(string code) : Attribute
{
    public string Code { get; } = code;
}

public static class ErrorFrom
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ErrorInfo Program(string message, [CallerMemberName] string? caller = null) =>
        new(StandardErrorCodes.InvalidRequest, $"{caller}: {message}");

    public static ErrorInfo Exception(Error e) {
        var errorInfoAttr = from ex in e.Exception
                            from attr in Optional(ex.GetType().GetCustomAttribute<ErrorInfoAttribute>())
                            select attr.Code;
        var subErrors = e is ManyErrors me ? me.Errors.Map(Exception) : Seq.empty<ErrorInfo>();
        return new ErrorInfo(errorInfoAttr.IfNone(StandardErrorCodes.Unhandled),
            e.Message,
            e.ToString(),
            data: default,
            e.Inner.Map(Exception).ToNullable(),
            subErrors.IsEmpty ? null : subErrors,
            stack: e.Exception.ToNullable()?.StackTrace
            );
    }
}