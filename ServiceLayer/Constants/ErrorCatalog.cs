using System.Net;

namespace ServiceLayer.Constants;

public class ErrorInfo
{
    public string Title { get; set; }
    public string Message { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public string Category { get; set; }  // PlanLimit, Subscription, Validation, ServerError
    public bool IsActionable { get; set; }

    public ErrorInfo(string title, string message, HttpStatusCode statusCode,
        string category, bool isActionable)
    {
        Title = title;
        Message = message;
        StatusCode = statusCode;
        Category = category;
        IsActionable = isActionable;
    }
}

public static class ErrorCatalog
{
    // Error Code Constants
    public const string FILE_TOO_LARGE = "FILE_TOO_LARGE";
    public const string STORAGE_QUOTA_EXCEEDED = "STORAGE_QUOTA_EXCEEDED";
    public const string NO_ACTIVE_SUBSCRIPTION = "NO_ACTIVE_SUBSCRIPTION";
    public const string SUBSCRIPTION_EXPIRED = "SUBSCRIPTION_EXPIRED";
    public const string INVALID_FILE = "INVALID_FILE";
    public const string FILE_SAVE_FAILED = "FILE_SAVE_FAILED";
    public const string DATABASE_ERROR = "DATABASE_ERROR";
    public const string UNKNOWN_ERROR = "UNKNOWN_ERROR";

    // Error Catalog Entries
    public static readonly Dictionary<string, ErrorInfo> FileTooLarge = new()
    {
        {
            "en", new ErrorInfo(
                "File Size Exceeds Limit",
                "Your file is larger than the maximum allowed size for your plan.",
                HttpStatusCode.BadRequest,
                "PlanLimit",
                true
            )
        }
    };

    public static readonly Dictionary<string, ErrorInfo> StorageQuotaExceeded = new()
    {
        {
            "en", new ErrorInfo(
                "Storage Limit Reached",
                "You've reached your storage limit. Upgrade your plan to upload more files.",
                (HttpStatusCode)413,  // PayloadTooLarge
                "PlanLimit",
                true
            )
        }
    };

    public static readonly Dictionary<string, ErrorInfo> NoActiveSubscription = new()
    {
        {
            "en", new ErrorInfo(
                "No Active Plan",
                "You need an active plan to upload files. Choose a plan to get started.",
                HttpStatusCode.Forbidden,
                "Subscription",
                true
            )
        }
    };

    public static readonly Dictionary<string, ErrorInfo> SubscriptionExpired = new()
    {
        {
            "en", new ErrorInfo(
                "Plan Expired",
                "Your subscription has expired. Renew your plan to continue uploading.",
                HttpStatusCode.Forbidden,
                "Subscription",
                true
            )
        }
    };

    public static readonly Dictionary<string, ErrorInfo> InvalidFile = new()
    {
        {
            "en", new ErrorInfo(
                "Invalid File",
                "The file you're trying to upload is invalid or corrupted.",
                HttpStatusCode.BadRequest,
                "Validation",
                false
            )
        }
    };

    public static readonly Dictionary<string, ErrorInfo> FileSaveFailed = new()
    {
        {
            "en", new ErrorInfo(
                "Upload Failed",
                "An error occurred while saving your file. Please try again.",
                HttpStatusCode.InternalServerError,
                "ServerError",
                false
            )
        }
    };

    public static readonly Dictionary<string, ErrorInfo> DatabaseError = new()
    {
        {
            "en", new ErrorInfo(
                "Database Error",
                "An unexpected database error occurred. Our team has been notified.",
                HttpStatusCode.InternalServerError,
                "ServerError",
                false
            )
        }
    };

    public static readonly Dictionary<string, ErrorInfo> UnknownError = new()
    {
        {
            "en", new ErrorInfo(
                "Unknown Error",
                "An unexpected error occurred. Please try again later.",
                HttpStatusCode.InternalServerError,
                "ServerError",
                false
            )
        }
    };

    /// <summary>
    /// Gets error info by code and language (defaults to English)
    /// </summary>
    public static ErrorInfo GetError(string errorCode, string language = "en")
    {
        return errorCode switch
        {
            FILE_TOO_LARGE => FileTooLarge.GetValueOrDefault(language) ?? FileTooLarge["en"],
            STORAGE_QUOTA_EXCEEDED => StorageQuotaExceeded.GetValueOrDefault(language) ?? StorageQuotaExceeded["en"],
            NO_ACTIVE_SUBSCRIPTION => NoActiveSubscription.GetValueOrDefault(language) ?? NoActiveSubscription["en"],
            SUBSCRIPTION_EXPIRED => SubscriptionExpired.GetValueOrDefault(language) ?? SubscriptionExpired["en"],
            INVALID_FILE => InvalidFile.GetValueOrDefault(language) ?? InvalidFile["en"],
            FILE_SAVE_FAILED => FileSaveFailed.GetValueOrDefault(language) ?? FileSaveFailed["en"],
            DATABASE_ERROR => DatabaseError.GetValueOrDefault(language) ?? DatabaseError["en"],
            _ => UnknownError.GetValueOrDefault(language) ?? UnknownError["en"]
        };
    }
}

public static class ErrorCategories
{
    public const string PLAN_LIMIT = "PlanLimit";
    public const string SUBSCRIPTION = "Subscription";
    public const string VALIDATION = "Validation";
    public const string SERVER = "ServerError";
}
