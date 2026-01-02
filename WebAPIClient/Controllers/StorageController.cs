using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ServiceLayer.Interfaces;
using ServiceLayer.Exceptions;
using WebAPIClient.DTOs;
using LoggingLayer;

namespace WebAPIClient.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StorageController : ControllerBase
{
    private readonly IStorageQuotaService _storageQuotaService;
    private readonly ILogger<StorageController> _logger;

    public StorageController(
        IStorageQuotaService storageQuotaService,
        ILogger<StorageController> logger)
    {
        _storageQuotaService = storageQuotaService;
        _logger = logger;
    }

    /// <summary>
    /// Gets current storage quota information for the authenticated user
    /// </summary>
    /// <response code="200">Returns storage quota information</response>
    /// <response code="403">User has no active subscription or subscription expired</response>
    /// <response code="500">Server error occurred</response>
    [HttpGet("quota")]
    public async Task<IActionResult> GetQuota()
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var quotaInfo = await _storageQuotaService.GetQuotaInfoAsync(userId);

            var response = new StorageQuotaInfoDto
            {
                UserId = quotaInfo.UserId,
                PlanId = quotaInfo.PlanId,
                PlanName = quotaInfo.PlanName,
                MaxFileSize = quotaInfo.MaxFileSize,
                TotalStorageLimit = quotaInfo.TotalStorageLimit,
                StorageUsed = quotaInfo.StorageUsed,
                StorageRemaining = quotaInfo.StorageRemaining,
                UsagePercentage = quotaInfo.UsagePercentage,
                SubscriptionEndDate = quotaInfo.SubscriptionEndDate
            };

            return Ok(response);
        }
        catch (NoActiveSubscriptionException ex)
        {
            _logger.LogWarning($"No active subscription for quota request");
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
            _logger.LogWarning($"Subscription expired for quota request");
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
                    ExpiredDate = (DateTime?)ex.Details.GetValueOrDefault("expiredDate")
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting quota information");
            return StatusCode(500, new ApiErrorResponse
            {
                Message = "An error occurred while retrieving quota information",
                Title = "Server Error",
                ErrorCode = "DATABASE_ERROR",
                ErrorCategory = "ServerError",
                HttpStatusCode = 500,
                IsActionable = false
            });
        }
    }
}
