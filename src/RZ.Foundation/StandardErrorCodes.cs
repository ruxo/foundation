using JetBrains.Annotations;

namespace RZ.Foundation;

/// <summary>
/// Standard Error Codes
/// </summary>
[PublicAPI]
public static class StandardErrorCodes
{
    public const string AuthenticationNeeded = "authenticaton-needed";

    /// <summary>
    /// For cancellation from <see cref="System.Threading.CancellationToken"/>
    /// </summary>
    public const string Cancelled = "cancelled";

    public const string DatabaseTransactionError = "database-transaction";
    public const string Duplication = "duplication";
    public const string HttpError = "http";
    public const string InvalidRequest = "invalid-request";
    public const string InvalidResponse = "invalid-response";
    public const string InvalidOrder = "invalid-order";
    public const string MissingConfiguration = "missing-configuration";
    public const string NetworkError = "network";
    public const string NotFound = "not-found";
    public const string PermissionNeeded = "permission-needed";
    public const string RaceCondition = "race-condition";

    /// <summary>
    /// Error from subsequent call to another service
    /// </summary>
    public const string ServiceError = "service-error";

    public const string Timeout = "timeout";
    public const string Unhandled = "unhandled";
    public const string ValidationFailed = "validation-failed";
}