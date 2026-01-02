using ServiceLayer.Constants;

namespace ServiceLayer.Exceptions;

/// <summary>
/// Base exception class for storage-related errors
/// </summary>
public abstract class StorageException : Exception
{
    public string ErrorCode { get; set; } = string.Empty;
    public string ErrorCategory { get; set; } = string.Empty;
    public int HttpStatusCode { get; set; }
    public bool IsActionable { get; set; }
    public string Title { get; set; } = string.Empty;
    public Dictionary<string, object> Details { get; set; }

    protected StorageException(string message) : base(message)
    {
        Details = new Dictionary<string, object>();
    }
}

/// <summary>
/// Exception thrown when file size exceeds the maximum allowed for the plan
/// </summary>
public class FileTooLargeException : StorageException
{
    public FileTooLargeException(long maxFileSize, long actualFileSize, string language = "en")
        : base($"File ({actualFileSize} bytes) exceeds maximum size ({maxFileSize} bytes)")
    {
        var errorInfo = ErrorCatalog.GetError(ErrorCatalog.FILE_TOO_LARGE, language);

        ErrorCode = ErrorCatalog.FILE_TOO_LARGE;
        ErrorCategory = errorInfo.Category;
        HttpStatusCode = (int)errorInfo.StatusCode;
        IsActionable = errorInfo.IsActionable;
        Title = errorInfo.Title;
        Details = new Dictionary<string, object>
        {
            { "maxFileSize", maxFileSize },
            { "actualFileSize", actualFileSize }
        };
    }
}

/// <summary>
/// Exception thrown when user has exceeded storage quota
/// </summary>
public class QuotaExceededException : StorageException
{
    public QuotaExceededException(long remaining, long needed, long limit, string language = "en")
        : base($"Insufficient storage. Need {needed} bytes but only {remaining} available.")
    {
        var errorInfo = ErrorCatalog.GetError(ErrorCatalog.STORAGE_QUOTA_EXCEEDED, language);

        ErrorCode = ErrorCatalog.STORAGE_QUOTA_EXCEEDED;
        ErrorCategory = errorInfo.Category;
        HttpStatusCode = (int)errorInfo.StatusCode;
        IsActionable = errorInfo.IsActionable;
        Title = errorInfo.Title;
        Details = new Dictionary<string, object>
        {
            { "storageRemaining", remaining },
            { "fileSize", needed },
            { "storageLimit", limit }
        };
    }
}

/// <summary>
/// Exception thrown when user has no active subscription
/// </summary>
public class NoActiveSubscriptionException : StorageException
{
    public NoActiveSubscriptionException(string language = "en")
        : base("No active subscription found.")
    {
        var errorInfo = ErrorCatalog.GetError(ErrorCatalog.NO_ACTIVE_SUBSCRIPTION, language);

        ErrorCode = ErrorCatalog.NO_ACTIVE_SUBSCRIPTION;
        ErrorCategory = errorInfo.Category;
        HttpStatusCode = (int)errorInfo.StatusCode;
        IsActionable = errorInfo.IsActionable;
        Title = errorInfo.Title;
    }
}

/// <summary>
/// Exception thrown when user's subscription has expired
/// </summary>
public class SubscriptionExpiredException : StorageException
{
    public SubscriptionExpiredException(DateTime expiredDate, string language = "en")
        : base($"Subscription expired on {expiredDate:yyyy-MM-dd}")
    {
        var errorInfo = ErrorCatalog.GetError(ErrorCatalog.SUBSCRIPTION_EXPIRED, language);

        ErrorCode = ErrorCatalog.SUBSCRIPTION_EXPIRED;
        ErrorCategory = errorInfo.Category;
        HttpStatusCode = (int)errorInfo.StatusCode;
        IsActionable = errorInfo.IsActionable;
        Title = errorInfo.Title;
        Details = new Dictionary<string, object>
        {
            { "expiredDate", expiredDate }
        };
    }
}

/// <summary>
/// Exception thrown when file save operation fails
/// </summary>
public class FileSaveException : StorageException
{
    public FileSaveException(string fileName, Exception? innerException = null, string language = "en")
        : base($"Failed to save file: {fileName}")
    {
        var errorInfo = ErrorCatalog.GetError(ErrorCatalog.FILE_SAVE_FAILED, language);

        ErrorCode = ErrorCatalog.FILE_SAVE_FAILED;
        ErrorCategory = errorInfo.Category;
        HttpStatusCode = (int)errorInfo.StatusCode;
        IsActionable = errorInfo.IsActionable;
        Title = errorInfo.Title;
        Details = new Dictionary<string, object>
        {
            { "fileName", fileName }
        };
    }
}
