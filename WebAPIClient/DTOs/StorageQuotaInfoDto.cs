namespace WebAPIClient.DTOs;

/// <summary>
/// Storage quota information for current user
/// </summary>
public class StorageQuotaInfoDto
{
    public int UserId { get; set; }
    public int? PlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;           // "Free", "Pro", "Enterprise"
    public long MaxFileSize { get; set; }          // Max bytes per file
    public long TotalStorageLimit { get; set; }    // Total account limit
    public long StorageUsed { get; set; }          // Current usage
    public long StorageRemaining { get; set; }     // Available quota
    public decimal UsagePercentage { get; set; }   // 0-100%
    public DateTime? SubscriptionEndDate { get; set; } // Null if no expiry
}
