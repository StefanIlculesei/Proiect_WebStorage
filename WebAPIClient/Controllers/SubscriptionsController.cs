using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DataAccessLayer.Accessors;
using System.Security.Claims;
using ModelLibrary.Models;
using WebAPIClient.DTOs;

namespace WebAPIClient.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SubscriptionsController : ControllerBase
    {
        private readonly SubscriptionAccessor _subscriptionAccessor;
        private readonly IMapper _mapper;

        public SubscriptionsController(SubscriptionAccessor subscriptionAccessor, IMapper mapper)
        {
            _subscriptionAccessor = subscriptionAccessor;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserSubscriptions()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var subscriptions = await _subscriptionAccessor.GetByUserIdAsync(userId);
            var response = _mapper.Map<IEnumerable<SubscriptionResponse>>(subscriptions);
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSubscriptionById(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var subscription = await _subscriptionAccessor.GetByIdAsync(id);

            if (subscription == null || subscription.UserId != userId)
            {
                return NotFound(new { Message = "Subscription not found" });
            }

            var response = _mapper.Map<SubscriptionResponse>(subscription);
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSubscription([FromBody] CreateSubscriptionRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var subscription = new Subscription
            {
                UserId = userId,
                PlanId = request.PlanId,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _subscriptionAccessor.AddAsync(subscription);
            await _subscriptionAccessor.SaveChangesAsync();

            var response = _mapper.Map<SubscriptionResponse>(subscription);
            return CreatedAtAction(nameof(GetSubscriptionById), new { id = subscription.Id }, response);
        }
    }
}
