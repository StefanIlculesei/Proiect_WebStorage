namespace ServiceLayer.Interfaces;

/// <summary>
/// Service for managing user subscriptions and plan upgrades
/// </summary>
public interface ISubscriptionService
{
    /// <summary>
    /// Gets all available plans
    /// </summary>
    Task<IEnumerable<PlanInfo>> GetAvailablePlansAsync();

    /// <summary>
    /// Upgrades or changes user's subscription to a new plan
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="planId">New plan ID</param>
    /// <returns>Updated subscription information</returns>
    Task<SubscriptionInfo> UpgradePlanAsync(int userId, int planId);

    /// <summary>
    /// Downgrades user's subscription to a lower plan
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="planId">New plan ID</param>
    /// <returns>Updated subscription information</returns>
    Task<SubscriptionInfo> DowngradePlanAsync(int userId, int planId);

    /// <summary>
    /// Cancels user's subscription
    /// </summary>
    /// <param name="userId">User ID</param>
    Task CancelSubscriptionAsync(int userId);

    /// <summary>
    /// Renews an expired subscription for the same plan
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Updated subscription information</returns>
    Task<SubscriptionInfo> RenewSubscriptionAsync(int userId);

    /// <summary>
    /// Creates an initial subscription with the Free plan for a new user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Initial subscription information</returns>
    Task<SubscriptionInfo> CreateInitialSubscriptionAsync(int userId);
}

/// <summary>
/// Information about an available plan
/// </summary>
public class PlanInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public long MaxFileSize { get; set; }          // Max bytes per file
    public long LimitSize { get; set; }            // Total storage limit in bytes
    public decimal MonthlyPrice { get; set; }
    public decimal? YearlyPrice { get; set; }
    public int MaxFileCount { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Information about a user's subscription
/// </summary>
public class SubscriptionInfo
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int PlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;  // active, canceled, expired, trialing
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
