using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Interfaces;
using LoggingLayer;

namespace WebAPIClient.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlansController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly ILogger<PlansController> _logger;

        public PlansController(
            ISubscriptionService subscriptionService,
            ILogger<PlansController> logger)
        {
            _subscriptionService = subscriptionService;
            _logger = logger;
        }

        /// <summary>
        /// Gets all available plans with pricing and features
        /// </summary>
        /// <response code="200">Returns list of available plans</response>
        /// <response code="500">Server error occurred</response>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetPlans()
        {
            try
            {
                var plans = await _subscriptionService.GetAvailablePlansAsync();
                return Ok(plans);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving plans");
                return StatusCode(500, new { Message = "An error occurred while retrieving plans" });
            }
        }
    }
}
