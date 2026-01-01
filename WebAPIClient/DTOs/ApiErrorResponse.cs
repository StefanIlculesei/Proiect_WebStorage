namespace WebAPIClient.DTOs;

/// <summary>
/// Standard API error response
/// </summary>
public class ApiErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
    public string ErrorCategory { get; set; } = string.Empty;  // PlanLimit, Subscription, Validation, ServerError
    public int HttpStatusCode { get; set; }
    public ErrorDetails? Details { get; set; }
    public bool IsActionable { get; set; }
    public string Title { get; set; } = string.Empty;
}

/// <summary>
/// Additional error details for specific error types
/// </summary>
public class ErrorDetails
{
    // For storage limit errors
    public long? MaxFileSize { get; set; }
    public long? FileSize { get; set; }
    public long? StorageUsed { get; set; }
    public long? StorageLimit { get; set; }
    public long? StorageRemaining { get; set; }

    // For subscription errors
    public DateTime? ExpiredDate { get; set; }
    public SubscriptionResponse? Subscription { get; set; }
}
