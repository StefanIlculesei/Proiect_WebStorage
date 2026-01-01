using DataAccessLayer.Accessors;
using Microsoft.Extensions.Logging;
using ServiceLayer.Exceptions;
using PersistenceLayer;

namespace ServiceLayer.Implementations;

/// <summary>
/// Service for managing user subscriptions and plan upgrades
/// </summary>
public class SubscriptionService : Interfaces.ISubscriptionService
{
    private readonly PlanAccessor _planAccessor;
    private readonly SubscriptionAccessor _subscriptionAccessor;
    private readonly UserAccessor _userAccessor;
    private readonly WebStorageContext _context;
    private readonly ILogger<SubscriptionService> _logger;

    // Subscription duration in days (30 days = 1 month)
    private const int SUBSCRIPTION_DURATION_DAYS = 30;

    // Plan names
    private const string FREE_PLAN_NAME = "Free";
    private const string PRO_PLAN_NAME = "Pro";
    private const string BUSINESS_PLAN_NAME = "Business";

    public SubscriptionService(
        PlanAccessor planAccessor,
        SubscriptionAccessor subscriptionAccessor,
        UserAccessor userAccessor,
        WebStorageContext context,
        ILogger<SubscriptionService> logger)
    {
        _planAccessor = planAccessor;
        _subscriptionAccessor = subscriptionAccessor;
        _userAccessor = userAccessor;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets all available plans
    /// </summary>
    public async Task<IEnumerable<Interfaces.PlanInfo>> GetAvailablePlansAsync()
    {
        try
        {
            var plans = await _planAccessor.GetAllAsync();
            return plans
                .OrderBy(p => p.LimitSize)
                .Select(p => new Interfaces.PlanInfo
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = string.Empty,  // Add description field if needed in Plan model
                    MaxFileSize = p.MaxFileSize,
                    LimitSize = p.LimitSize,
                    MonthlyPrice = p.Price,
                    YearlyPrice = p.Price * 10, // Approximate yearly price (10 months)
                    MaxFileCount = 0,  // Add max file count field if needed in Plan model
                    IsActive = true  // Assuming all retrieved plans are active
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available plans");
            throw;
        }
    }

    /// <summary>
    /// Upgrades or changes user's subscription to a new plan
    /// </summary>
    public async Task<Interfaces.SubscriptionInfo> UpgradePlanAsync(int userId, int planId)
    {
        try
        {
            // Verify user exists
            var user = await _userAccessor.GetByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException($"User with ID {userId} not found");
            }

            // Verify plan exists
            var newPlan = await _planAccessor.GetByIdAsync(planId);
            if (newPlan == null)
            {
                throw new InvalidOperationException($"Plan with ID {planId} not found");
            }

            // Get current subscription
            var currentSubscription = await _subscriptionAccessor.GetActiveSubscriptionByUserIdAsync(userId);

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Cancel current subscription if it exists
                    if (currentSubscription != null && currentSubscription.Status != "canceled")
                    {
                        currentSubscription.Status = "canceled";
                        currentSubscription.EndDate = DateTime.UtcNow;
                        currentSubscription.UpdatedAt = DateTime.UtcNow;
                        await _subscriptionAccessor.UpdateAsync(currentSubscription);
                    }

                    // Create new subscription
                    var now = DateTime.UtcNow;
                    var newSubscription = new ModelLibrary.Models.Subscription
                    {
                        UserId = userId,
                        PlanId = planId,
                        Status = "active",
                        IsActive = true,
                        AutoRenew = true,
                        StartDate = now,
                        EndDate = now.AddDays(SUBSCRIPTION_DURATION_DAYS),
                        CreatedAt = now,
                        UpdatedAt = now
                    };

                    await _subscriptionAccessor.AddAsync(newSubscription);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation($"User {userId} upgraded to plan {planId}");

                    return new Interfaces.SubscriptionInfo
                    {
                        Id = newSubscription.Id,
                        UserId = newSubscription.UserId,
                        PlanId = newSubscription.PlanId,
                        PlanName = newPlan.Name,
                        Status = newSubscription.Status ?? "active",
                        StartDate = newSubscription.StartDate ?? now,
                        EndDate = newSubscription.EndDate ?? now.AddDays(SUBSCRIPTION_DURATION_DAYS),
                        CreatedAt = newSubscription.CreatedAt ?? now,
                        UpdatedAt = newSubscription.UpdatedAt
                    };
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error upgrading plan for user {userId} to plan {planId}");
            throw;
        }
    }

    /// <summary>
    /// Downgrades user's subscription to a lower plan
    /// </summary>
    public async Task<Interfaces.SubscriptionInfo> DowngradePlanAsync(int userId, int planId)
    {
        try
        {
            // Verify user exists
            var user = await _userAccessor.GetByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException($"User with ID {userId} not found");
            }

            // Verify plan exists
            var newPlan = await _planAccessor.GetByIdAsync(planId);
            if (newPlan == null)
            {
                throw new InvalidOperationException($"Plan with ID {planId} not found");
            }

            // Get current subscription
            var currentSubscription = await _subscriptionAccessor.GetActiveSubscriptionByUserIdAsync(userId);
            if (currentSubscription == null)
            {
                throw new NoActiveSubscriptionException();
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Cancel current subscription
                    currentSubscription.Status = "canceled";
                    currentSubscription.EndDate = DateTime.UtcNow;
                    currentSubscription.UpdatedAt = DateTime.UtcNow;
                    await _subscriptionAccessor.UpdateAsync(currentSubscription);

                    // Create new subscription with downgraded plan
                    var now = DateTime.UtcNow;
                    var newSubscription = new ModelLibrary.Models.Subscription
                    {
                        UserId = userId,
                        PlanId = planId,
                        Status = "active",
                        IsActive = true,
                        AutoRenew = true,
                        StartDate = now,
                        EndDate = now.AddDays(SUBSCRIPTION_DURATION_DAYS),
                        CreatedAt = now,
                        UpdatedAt = now
                    };

                    await _subscriptionAccessor.AddAsync(newSubscription);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation($"User {userId} downgraded to plan {planId}");

                    return new Interfaces.SubscriptionInfo
                    {
                        Id = newSubscription.Id,
                        UserId = newSubscription.UserId,
                        PlanId = newSubscription.PlanId,
                        PlanName = newPlan.Name,
                        Status = newSubscription.Status ?? "active",
                        StartDate = newSubscription.StartDate ?? now,
                        EndDate = newSubscription.EndDate ?? now.AddDays(SUBSCRIPTION_DURATION_DAYS),
                        CreatedAt = newSubscription.CreatedAt ?? now,
                        UpdatedAt = newSubscription.UpdatedAt
                    };
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error downgrading plan for user {userId} to plan {planId}");
            throw;
        }
    }

    /// <summary>
    /// Cancels user's subscription
    /// </summary>
    public async Task CancelSubscriptionAsync(int userId)
    {
        try
        {
            var subscription = await _subscriptionAccessor.GetActiveSubscriptionByUserIdAsync(userId);
            if (subscription == null)
            {
                throw new NoActiveSubscriptionException();
            }

            subscription.Status = "canceled";
            subscription.IsActive = false;
            subscription.EndDate = DateTime.UtcNow;
            subscription.UpdatedAt = DateTime.UtcNow;

            await _subscriptionAccessor.UpdateAsync(subscription);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Subscription cancelled for user {userId}");
        }
        catch (StorageException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error canceling subscription for user {userId}");
            throw;
        }
    }

    /// <summary>
    /// Renews an expired subscription for the same plan
    /// </summary>
    public async Task<Interfaces.SubscriptionInfo> RenewSubscriptionAsync(int userId)
    {
        try
        {
            // Get the most recent subscription (even if expired)
            var allSubscriptions = await _subscriptionAccessor.GetByUserIdAsync(userId);
            var lastSubscription = allSubscriptions.OrderByDescending(s => s.CreatedAt).FirstOrDefault();

            if (lastSubscription == null)
            {
                throw new NoActiveSubscriptionException();
            }

            // Get the plan
            var plan = await _planAccessor.GetByIdAsync(lastSubscription.PlanId);
            if (plan == null)
            {
                throw new InvalidOperationException($"Associated plan is no longer available");
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Mark old subscription as canceled if not already
                    if (lastSubscription.Status != "canceled")
                    {
                        lastSubscription.Status = "canceled";
                        lastSubscription.IsActive = false;
                        lastSubscription.UpdatedAt = DateTime.UtcNow;
                        await _subscriptionAccessor.UpdateAsync(lastSubscription);
                    }

                    // Create renewed subscription
                    var now = DateTime.UtcNow;
                    var newSubscription = new ModelLibrary.Models.Subscription
                    {
                        UserId = userId,
                        PlanId = lastSubscription.PlanId,
                        Status = "active",
                        IsActive = true,
                        AutoRenew = true,
                        StartDate = now,
                        EndDate = now.AddDays(SUBSCRIPTION_DURATION_DAYS),
                        CreatedAt = now,
                        UpdatedAt = now
                    };

                    await _subscriptionAccessor.AddAsync(newSubscription);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation($"Subscription renewed for user {userId}, plan {lastSubscription.PlanId}");

                    return new Interfaces.SubscriptionInfo
                    {
                        Id = newSubscription.Id,
                        UserId = newSubscription.UserId,
                        PlanId = newSubscription.PlanId,
                        PlanName = plan.Name,
                        Status = newSubscription.Status ?? "active",
                        StartDate = newSubscription.StartDate ?? now,
                        EndDate = newSubscription.EndDate ?? now.AddDays(SUBSCRIPTION_DURATION_DAYS),
                        CreatedAt = newSubscription.CreatedAt ?? now,
                        UpdatedAt = newSubscription.UpdatedAt
                    };
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error renewing subscription for user {userId}");
            throw;
        }
    }

    /// <summary>
    /// Creates an initial subscription with the Free plan for a new user
    /// </summary>
    public async Task<Interfaces.SubscriptionInfo> CreateInitialSubscriptionAsync(int userId)
    {
        try
        {
            // Verify user exists
            var user = await _userAccessor.GetByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException($"User with ID {userId} not found");
            }

            // Check if user already has a subscription
            var existingSubscription = await _subscriptionAccessor.GetActiveSubscriptionByUserIdAsync(userId);
            if (existingSubscription != null)
            {
                _logger.LogWarning($"User {userId} already has an active subscription");
                return new Interfaces.SubscriptionInfo
                {
                    Id = existingSubscription.Id,
                    UserId = existingSubscription.UserId,
                    PlanId = existingSubscription.PlanId,
                    PlanName = "Free",
                    Status = existingSubscription.Status ?? "active",
                    StartDate = existingSubscription.StartDate ?? DateTime.UtcNow,
                    EndDate = existingSubscription.EndDate ?? DateTime.UtcNow.AddDays(SUBSCRIPTION_DURATION_DAYS),
                    CreatedAt = existingSubscription.CreatedAt ?? DateTime.UtcNow,
                    UpdatedAt = existingSubscription.UpdatedAt
                };
            }

            // Get the Free plan by name
            var allPlans = await _planAccessor.GetAllAsync();
            var freePlan = allPlans.FirstOrDefault(p => p.Name == FREE_PLAN_NAME);
            if (freePlan == null)
            {
                throw new InvalidOperationException($"Free plan '{FREE_PLAN_NAME}' not found in the system");
            }

            // Create new subscription with Free plan
            var now = DateTime.UtcNow;
            var newSubscription = new ModelLibrary.Models.Subscription
            {
                UserId = userId,
                PlanId = freePlan.Id,
                Status = "active",
                IsActive = true,
                AutoRenew = true,
                StartDate = now,
                EndDate = now.AddDays(SUBSCRIPTION_DURATION_DAYS),
                CreatedAt = now,
                UpdatedAt = now
            };

            await _subscriptionAccessor.AddAsync(newSubscription);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Initial subscription created for user {userId} with Free plan");

            return new Interfaces.SubscriptionInfo
            {
                Id = newSubscription.Id,
                UserId = newSubscription.UserId,
                PlanId = newSubscription.PlanId,
                PlanName = freePlan.Name,
                Status = newSubscription.Status ?? "active",
                StartDate = newSubscription.StartDate ?? now,
                EndDate = newSubscription.EndDate ?? now.AddDays(SUBSCRIPTION_DURATION_DAYS),
                CreatedAt = newSubscription.CreatedAt ?? now,
                UpdatedAt = newSubscription.UpdatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating initial subscription for user {userId}");
            throw;
        }
    }
}
