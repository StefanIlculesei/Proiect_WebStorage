using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ServiceLayer.Interfaces;
using ServiceLayer.Exceptions;
using WebAPIClient.DTOs;
using LoggingLayer;
using AutoMapper;
using DataAccessLayer.Accessors;

namespace WebAPIClient.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscriptionsController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly IStorageQuotaService _storageQuotaService;
    private readonly PlanAccessor _planAccessor;
    private readonly SubscriptionAccessor _subscriptionAccessor;
    private readonly IMapper _mapper;
    private readonly ILogger<SubscriptionsController> _logger;

    public SubscriptionsController(
        ISubscriptionService subscriptionService,
        IStorageQuotaService storageQuotaService,
        PlanAccessor planAccessor,
        SubscriptionAccessor subscriptionAccessor,
        IMapper mapper,
        ILogger<SubscriptionsController> logger)
    {
        _subscriptionService = subscriptionService;
        _storageQuotaService = storageQuotaService;
        _planAccessor = planAccessor;
        _subscriptionAccessor = subscriptionAccessor;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Gets the current active subscription for the authenticated user
    /// </summary>
    /// <response code="200">Returns current subscription info</response>
    /// <response code="403">User has no active subscription</response>
    /// <response code="500">Server error occurred</response>
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentSubscription()
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var quotaInfo = await _storageQuotaService.GetQuotaInfoAsync(userId);

            // Fetch the plan details
            var plan = await _planAccessor.GetByIdAsync(quotaInfo.PlanId ?? 0);

            PlanDetailResponse? planDetail = null;
            if (plan != null)
            {
                planDetail = new PlanDetailResponse
                {
                    Id = plan.Id,
                    Name = plan.Name,
                    Description = string.Empty,
                    MaxFileSize = plan.MaxFileSize,
                    LimitSize = plan.LimitSize,
                    MonthlyPrice = plan.Price,
                    YearlyPrice = plan.Price * 12,  // Approximate yearly price
                    MaxFileCount = 0,  // Not available in Plan model
                    IsActive = true
                };
            }

            var response = new SubscriptionResponse
            {
                UserId = quotaInfo.UserId,
                PlanId = quotaInfo.PlanId ?? 0,
                PlanName = quotaInfo.PlanName,
                Status = "active",
                StartDate = DateTime.UtcNow,
                EndDate = quotaInfo.SubscriptionEndDate ?? DateTime.UtcNow.AddDays(30),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null,
                Plan = planDetail
            };

            return Ok(response);
        }
        catch (NoActiveSubscriptionException ex)
        {
            _logger.LogWarning("No active subscription for current request");
            return StatusCode(403, new ApiErrorResponse
            {
                Message = ex.Message,
                Title = ex.Title,
                ErrorCode = ex.ErrorCode,
                ErrorCategory = ex.ErrorCategory,
                HttpStatusCode = ex.HttpStatusCode,
                IsActionable = ex.IsActionable
            });
        }
        catch (SubscriptionExpiredException ex)
        {
            _logger.LogWarning("Subscription expired for current request");

            // Fetch expired subscription to include in error response
            SubscriptionResponse? expiredSubscription = null;
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var allSubscriptions = await _subscriptionAccessor.GetByUserIdAsync(userId);
                var lastSubscription = allSubscriptions.OrderByDescending(s => s.CreatedAt).FirstOrDefault();

                if (lastSubscription != null)
                {
                    var plan = await _planAccessor.GetByIdAsync(lastSubscription.PlanId);

                    PlanDetailResponse? planDetail = null;
                    if (plan != null)
                    {
                        planDetail = new PlanDetailResponse
                        {
                            Id = plan.Id,
                            Name = plan.Name,
                            Description = string.Empty,
                            MaxFileSize = plan.MaxFileSize,
                            LimitSize = plan.LimitSize,
                            MonthlyPrice = plan.Price,
                            YearlyPrice = plan.Price * 12,
                            MaxFileCount = 0,
                            IsActive = true
                        };
                    }

                    expiredSubscription = new SubscriptionResponse
                    {
                        Id = lastSubscription.Id,
                        UserId = lastSubscription.UserId,
                        PlanId = lastSubscription.PlanId,
                        PlanName = plan?.Name ?? "Unknown",
                        Status = lastSubscription.Status ?? "expired",
                        StartDate = lastSubscription.StartDate ?? DateTime.UtcNow,
                        EndDate = lastSubscription.EndDate ?? DateTime.UtcNow,
                        CreatedAt = lastSubscription.CreatedAt ?? DateTime.UtcNow,
                        UpdatedAt = lastSubscription.UpdatedAt,
                        Plan = planDetail
                    };
                }
            }
            catch (Exception accessEx)
            {
                _logger.LogError(accessEx, "Error fetching expired subscription details");
            }

            return StatusCode(403, new ApiErrorResponse
            {
                Message = ex.Message,
                Title = ex.Title,
                ErrorCode = ex.ErrorCode,
                ErrorCategory = ex.ErrorCategory,
                HttpStatusCode = ex.HttpStatusCode,
                IsActionable = ex.IsActionable,
                Details = new ErrorDetails
                {
                    ExpiredDate = (DateTime?)ex.Details.GetValueOrDefault("expiredDate"),
                    Subscription = expiredSubscription
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current subscription");
            return StatusCode(500, new ApiErrorResponse
            {
                Message = "An error occurred while retrieving subscription information",
                Title = "Server Error",
                ErrorCode = "DATABASE_ERROR",
                ErrorCategory = "ServerError",
                HttpStatusCode = 500,
                IsActionable = false
            });
        }
    }

    /// <summary>
    /// Upgrades user's subscription to a new plan
    /// </summary>
    /// <response code="200">Plan upgrade successful</response>
    /// <response code="400">Invalid plan ID</response>
    /// <response code="500">Server error occurred</response>
    [HttpPost("upgrade")]
    public async Task<IActionResult> UpgradePlan([FromBody] UpgradePlanRequest request)
    {
        try
        {
            if (request == null || request.PlanId <= 0)
            {
                return BadRequest(new { Message = "Invalid plan ID" });
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var subscription = await _subscriptionService.UpgradePlanAsync(userId, request.PlanId);

            var response = new SubscriptionResponse
            {
                Id = subscription.Id,
                UserId = subscription.UserId,
                PlanId = subscription.PlanId,
                PlanName = subscription.PlanName,
                Status = subscription.Status,
                StartDate = subscription.StartDate,
                EndDate = subscription.EndDate,
                CreatedAt = subscription.CreatedAt,
                UpdatedAt = subscription.UpdatedAt
            };

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation during plan upgrade");
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upgrading plan");
            return StatusCode(500, new { Message = "An error occurred while upgrading your plan" });
        }
    }

    /// <summary>
    /// Downgrades user's subscription to a lower plan
    /// </summary>
    /// <response code="200">Plan downgrade successful</response>
    /// <response code="400">Invalid plan ID or no active subscription</response>
    /// <response code="500">Server error occurred</response>
    [HttpPost("downgrade")]
    public async Task<IActionResult> DowngradePlan([FromBody] UpgradePlanRequest request)
    {
        try
        {
            if (request == null || request.PlanId <= 0)
            {
                return BadRequest(new { Message = "Invalid plan ID" });
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var subscription = await _subscriptionService.DowngradePlanAsync(userId, request.PlanId);

            var response = new SubscriptionResponse
            {
                Id = subscription.Id,
                UserId = subscription.UserId,
                PlanId = subscription.PlanId,
                PlanName = subscription.PlanName,
                Status = subscription.Status,
                StartDate = subscription.StartDate,
                EndDate = subscription.EndDate,
                CreatedAt = subscription.CreatedAt,
                UpdatedAt = subscription.UpdatedAt
            };

            return Ok(response);
        }
        catch (NoActiveSubscriptionException ex)
        {
            _logger.LogWarning("No active subscription for downgrade");
            return BadRequest(new { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation during plan downgrade");
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downgrading plan");
            return StatusCode(500, new { Message = "An error occurred while downgrading your plan" });
        }
    }

    /// <summary>
    /// Cancels user's subscription
    /// </summary>
    /// <response code="204">Subscription cancelled successfully</response>
    /// <response code="400">No active subscription to cancel</response>
    /// <response code="500">Server error occurred</response>
    [HttpPost("cancel")]
    public async Task<IActionResult> CancelSubscription()
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _subscriptionService.CancelSubscriptionAsync(userId);
            return NoContent();
        }
        catch (NoActiveSubscriptionException ex)
        {
            _logger.LogWarning("No active subscription to cancel");
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error canceling subscription");
            return StatusCode(500, new { Message = "An error occurred while canceling your subscription" });
        }
    }

    /// <summary>
    /// Renews an expired subscription for the same plan
    /// </summary>
    /// <response code="200">Subscription renewed successfully</response>
    /// <response code="400">No subscription to renew</response>
    /// <response code="500">Server error occurred</response>
    [HttpPost("renew")]
    public async Task<IActionResult> RenewSubscription()
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var subscription = await _subscriptionService.RenewSubscriptionAsync(userId);

            var response = new SubscriptionResponse
            {
                Id = subscription.Id,
                UserId = subscription.UserId,
                PlanId = subscription.PlanId,
                PlanName = subscription.PlanName,
                Status = subscription.Status,
                StartDate = subscription.StartDate,
                EndDate = subscription.EndDate,
                CreatedAt = subscription.CreatedAt,
                UpdatedAt = subscription.UpdatedAt
            };

            return Ok(response);
        }
        catch (NoActiveSubscriptionException ex)
        {
            _logger.LogWarning("No subscription found to renew");
            return BadRequest(new { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation during subscription renewal");
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error renewing subscription");
            return StatusCode(500, new { Message = "An error occurred while renewing your subscription" });
        }
    }

    /// <summary>
    /// Adds a new subscription for a user (when all previous subscriptions are canceled)
    /// </summary>
    /// <response code="200">Subscription created successfully</response>
    /// <response code="400">Invalid plan ID or user already has active subscription</response>
    /// <response code="500">Server error occurred</response>
    [HttpPost]
    public async Task<IActionResult> AddSubscription([FromBody] UpgradePlanRequest request)
    {
        try
        {
            if (request == null || request.PlanId <= 0)
            {
                return BadRequest(new { Message = "Invalid plan ID" });
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Check if user already has an active subscription
            var activeSubscription = await _subscriptionAccessor.GetActiveSubscriptionByUserIdAsync(userId);
            if (activeSubscription != null)
            {
                return BadRequest(new { Message = "User already has an active subscription. Use upgrade/downgrade endpoints instead." });
            }

            // Use UpgradePlanAsync which handles the subscription creation logic
            var subscription = await _subscriptionService.UpgradePlanAsync(userId, request.PlanId);

            // Fetch the plan details for the response
            var plan = await _planAccessor.GetByIdAsync(subscription.PlanId);

            PlanDetailResponse? planDetail = null;
            if (plan != null)
            {
                planDetail = new PlanDetailResponse
                {
                    Id = plan.Id,
                    Name = plan.Name,
                    Description = string.Empty,
                    MaxFileSize = plan.MaxFileSize,
                    LimitSize = plan.LimitSize,
                    MonthlyPrice = plan.Price,
                    YearlyPrice = plan.Price * 12,
                    MaxFileCount = 0,
                    IsActive = true
                };
            }

            var response = new SubscriptionResponse
            {
                Id = subscription.Id,
                UserId = subscription.UserId,
                PlanId = subscription.PlanId,
                PlanName = subscription.PlanName,
                Status = subscription.Status,
                StartDate = subscription.StartDate,
                EndDate = subscription.EndDate,
                CreatedAt = subscription.CreatedAt,
                UpdatedAt = subscription.UpdatedAt,
                Plan = planDetail
            };

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation during subscription creation");
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating subscription");
            return StatusCode(500, new { Message = "An error occurred while creating your subscription" });
        }
    }
}
