namespace RZ.Foundation;

/// <summary>
/// Standard Error Codes
/// </summary>
[PublicAPI]
public static class StandardErrorCodes
{
    #region Backward commpatible

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

    #endregion

    public const string AUTHENTICATION_NEEDED = "authenticaton-needed";

    /// <summary>
    /// For cancellation from <see cref="System.Threading.CancellationToken"/>
    /// </summary>
    public const string CANCELLED = "cancelled";

    public const string DATABASE_TRANSACTION_ERROR = "database-transaction";
    public const string DUPLICATION = "duplication";
    public const string HTTP_ERROR = "http";
    public const string INVALID_REQUEST = "invalid-request";
    public const string INVALID_RESPONSE = "invalid-response";
    public const string INVALID_ORDER = "invalid-order";
    public const string MISSING_CONFIGURATION = "missing-configuration";
    public const string NETWORK_ERROR = "network";
    public const string NOT_FOUND = "not-found";
    public const string PERMISSION_NEEDED = "permission-needed";
    public const string RACE_CONDITION = "race-condition";

    /// <summary>
    /// Error from subsequent call to another service
    /// </summary>
    public const string SERVICE_ERROR = "service-error";

    public const string TIMEOUT = "timeout";
    public const string UNHANDLED = "unhandled";
    public const string VALIDATION_FAILED = "validation-failed";

    public const string NOT_SUPPORTED = "not-supported";
}