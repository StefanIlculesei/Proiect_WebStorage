using DataAccessLayer.Accessors;
using Microsoft.Extensions.Logging;
using ServiceLayer.Exceptions;

namespace ServiceLayer.Implementations;

/// <summary>
/// Service for managing and validating storage quotas
/// </summary>
public class StorageQuotaService : Interfaces.IStorageQuotaService
{
    private readonly UserAccessor _userAccessor;
    private readonly PlanAccessor _planAccessor;
    private readonly SubscriptionAccessor _subscriptionAccessor;
    private readonly ILogger<StorageQuotaService> _logger;

    public StorageQuotaService(
        UserAccessor userAccessor,
        PlanAccessor planAccessor,
        SubscriptionAccessor subscriptionAccessor,
        ILogger<StorageQuotaService> logger)
    {
        _userAccessor = userAccessor;
        _planAccessor = planAccessor;
        _subscriptionAccessor = subscriptionAccessor;
        _logger = logger;
    }

    /// <summary>
    /// Gets current storage quota information for a user
    /// </summary>
    public async Task<Interfaces.StorageQuotaInfo> GetQuotaInfoAsync(int userId)
    {
        try
        {
            var user = await _userAccessor.GetByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            var subscription = await _subscriptionAccessor.GetActiveSubscriptionByUserIdAsync(userId);
            if (subscription == null)
            {
                throw new NoActiveSubscriptionException();
            }

            // Check if subscription has expired
            if (subscription.Status == "expired" || (subscription.EndDate.HasValue && subscription.EndDate < DateTime.UtcNow))
            {
                throw new SubscriptionExpiredException(subscription.EndDate ?? DateTime.UtcNow);
            }

            var plan = await _planAccessor.GetByIdAsync(subscription.PlanId);
            if (plan == null)
            {
                throw new InvalidOperationException($"Plan with ID {subscription.PlanId} not found");
            }

            long storageRemaining = plan.LimitSize - user.StorageUsed;
            decimal usagePercentage = plan.LimitSize > 0
                ? (decimal)user.StorageUsed / plan.LimitSize * 100
                : 0;

            return new Interfaces.StorageQuotaInfo
            {
                UserId = userId,
                PlanId = plan.Id,
                PlanName = plan.Name,
                MaxFileSize = plan.MaxFileSize,
                TotalStorageLimit = plan.LimitSize,
                StorageUsed = user.StorageUsed,
                StorageRemaining = Math.Max(0, storageRemaining),
                UsagePercentage = Math.Min(100, usagePercentage),
                SubscriptionEndDate = subscription.EndDate
            };
        }
        catch (StorageException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting quota info for user {userId}");
            throw;
        }
    }

    /// <summary>
    /// Validates if a file upload is allowed based on plan limits
    /// </summary>
    public async Task ValidateUploadAsync(int userId, long fileSize)
    {
        try
        {
            var quotaInfo = await GetQuotaInfoAsync(userId);

            // Check if file exceeds max file size
            if (fileSize > quotaInfo.MaxFileSize)
            {
                throw new FileTooLargeException(quotaInfo.MaxFileSize, fileSize);
            }

            // Check if file exceeds remaining storage quota
            if (fileSize > quotaInfo.StorageRemaining)
            {
                throw new QuotaExceededException(
                    quotaInfo.StorageRemaining,
                    fileSize,
                    quotaInfo.TotalStorageLimit
                );
            }
        }
        catch (StorageException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error validating upload for user {userId}, fileSize {fileSize}");
            throw;
        }
    }
}
