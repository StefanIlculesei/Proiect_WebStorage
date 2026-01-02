using ModelLibrary.Models;
using ServiceLayer.Exceptions;

namespace ServiceLayer.Interfaces;

/// <summary>
/// Service for managing and validating storage quotas
/// </summary>
public interface IStorageQuotaService
{
    /// <summary>
    /// Gets current storage quota information for a user
    /// </summary>
    /// <returns>StorageQuotaInfo with plan limits and current usage</returns>
    /// <exception cref="NoActiveSubscriptionException">Thrown if user has no active subscription</exception>
    Task<StorageQuotaInfo> GetQuotaInfoAsync(int userId);

    /// <summary>
    /// Validates if a file upload is allowed based on plan limits
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="fileSize">Size of file to upload in bytes</param>
    /// <exception cref="FileTooLargeException">Thrown if file exceeds max file size</exception>
    /// <exception cref="QuotaExceededException">Thrown if file exceeds remaining quota</exception>
    /// <exception cref="NoActiveSubscriptionException">Thrown if user has no active subscription</exception>
    /// <exception cref="SubscriptionExpiredException">Thrown if subscription has expired</exception>
    Task ValidateUploadAsync(int userId, long fileSize);
}

/// <summary>
/// Storage quota information for a user
/// </summary>
public class StorageQuotaInfo
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
