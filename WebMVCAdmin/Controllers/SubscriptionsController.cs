using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Globalization;
using DataAccessLayer.Accessors;
using WebMVC_Plans.Models;
using ModelLibrary.Models;
using AutoMapper;

namespace WebMVC_Plans.Controllers
{

    [Authorize(Policy = "AdminOnly")]
    [Route("[controller]")]
    public class SubscriptionsController : Controller
    {
        private readonly SubscriptionAccessor _accesorSubscription;
        private readonly PlanAccessor _accesorPlan;
        private readonly UserAccessor _accesorUser;
        private readonly ILogger<SubscriptionsController> _logger;
        private readonly IMapper _mapper;

        public SubscriptionsController(SubscriptionAccessor accesorSubscription, PlanAccessor accesorPlan, UserAccessor accesorUser, ILogger<SubscriptionsController> logger, IMapper mapper)
        {
            _accesorSubscription = accesorSubscription ?? throw new ArgumentNullException(nameof(accesorSubscription));
            _accesorPlan = accesorPlan ?? throw new ArgumentNullException(nameof(accesorPlan));
            _accesorUser = accesorUser ?? throw new ArgumentNullException(nameof(accesorUser));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // GET /Subscriptions
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Fetching all subscriptions from database");

                var subscriptions = await _accesorSubscription.GetAllAsync();

                // Fetch lookup data once
                var users = await _accesorUser.GetAllAsync();
                var plans = await _accesorPlan.GetAllAsync();
                var userMap = users.ToDictionary(u => u.Id, u => u.UserName);
                var planMap = plans.ToDictionary(p => p.Id, p => p.Name);

                var subscriptionViewModels = _mapper.Map<List<SubscriptionViewModel>>(subscriptions);

                foreach (var vm in subscriptionViewModels)
                {
                    if (userMap.TryGetValue(vm.UserId, out var uname))
                    {
                        vm.UserName = uname;
                    }
                    if (planMap.TryGetValue(vm.PlanId, out var pname))
                    {
                        vm.PlanName = pname;
                    }
                }

                _logger.LogInformation("Successfully retrieved {Count} subscriptions", subscriptionViewModels.Count);

                return View(subscriptionViewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching subscriptions");
                TempData["ErrorMessage"] = "An error occurred while loading the subscriptions. Please try again later.";
                return View(new List<SubscriptionViewModel>());
            }
        }
        // GET /Subscriptions/Details/{id}
        [HttpGet("Details/{id?}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || id <= 0)
            {
                _logger.LogWarning("Invalid subscription ID provided: {Id}", id);
                TempData["ErrorMessage"] = "Invalid subscription ID.";
                return RedirectToAction(nameof(Index));
            }
            try
            {
                _logger.LogInformation("Fetching subscription with ID: {Id}", id);

                var subscription = await _accesorSubscription.GetByIdAsync(id.Value);

                if (subscription == null)
                {
                    _logger.LogWarning("Subscription not found with ID: {Id}", id);
                    TempData["ErrorMessage"] = "Subscription not found.";
                    return RedirectToAction(nameof(Index));
                }

                var subscriptionViewModel = _mapper.Map<SubscriptionViewModel>(subscription);

                // Manually populate names if navigation properties are missing
                if (subscription.User == null)
                {
                    var user = await _accesorUser.GetByIdAsync(subscription.UserId);
                    subscriptionViewModel.UserName = user?.UserName;
                }
                if (subscription.Plan == null)
                {
                    var plan = await _accesorPlan.GetByIdAsync(subscription.PlanId);
                    subscriptionViewModel.PlanName = plan?.Name;
                }

                _logger.LogInformation("Successfully retrieved subscription with ID: {Id}", id);

                return View(subscriptionViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching subscription with ID: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the subscription details. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET /Subscriptions/Create
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var users = await _accesorUser.GetAllAsync();
            var plans = await _accesorPlan.GetAllAsync();

            var vm = new WebMVC_Plans.Models.CreateSubscriptionViewModel
            {
                Users = users
                    .OrderBy(u => u.UserName)
                    .Select(u => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Value = u.Id.ToString(),
                        Text = $"{u.UserName} ({u.Email})"
                    })
                    .ToList(),
                Plans = plans
                    .OrderBy(p => p.Price)
                    .Select(p => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = $"{p.Name} - {p.Price:N2} {p.Currency}"
                    })
                    .ToList()
            };

            return View(vm);
        }

        // POST /Subscriptions/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WebMVC_Plans.Models.CreateSubscriptionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var users = await _accesorUser.GetAllAsync();
                var plans = await _accesorPlan.GetAllAsync();
                model.Users = users.Select(u => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = u.Id.ToString(), Text = $"{u.UserName} ({u.Email})" });
                model.Plans = plans.Select(p => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = p.Id.ToString(), Text = $"{p.Name} - {p.Price:N2} {p.Currency}" });
                return View(model);
            }

            try
            {
                var nowUtc = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

                // Ensure dates are UTC
                if (model.StartDate.HasValue)
                {
                    model.StartDate = model.StartDate.Value.ToUniversalTime();
                }
                else
                {
                    model.StartDate = nowUtc;
                }

                if (model.EndDate.HasValue)
                {
                    model.EndDate = model.EndDate.Value.ToUniversalTime();
                }
                else
                {
                    // Auto-calculate EndDate
                    try
                    {
                        var plan = await _accesorPlan.GetByIdAsync(model.PlanId);
                        var baseStart = model.StartDate.Value;
                        var period = plan?.BillingPeriod?.Trim().ToLowerInvariant();

                        model.EndDate = period switch
                        {
                            "monthly" => baseStart.AddMonths(1),
                            "yearly" or "annual" => baseStart.AddYears(1),
                            "weekly" => baseStart.AddDays(7),
                            _ => baseStart.AddMonths(1)
                        };
                        _logger.LogInformation("Auto-calculated EndDate based on plan period '{Period}': {End:o}", period, model.EndDate);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to auto-calculate EndDate; defaulting to +1 month from now.");
                        model.EndDate = nowUtc.AddMonths(1);
                    }
                }

                var subscription = _mapper.Map<ModelLibrary.Models.Subscription>(model);
                subscription.CreatedAt = nowUtc;
                subscription.UpdatedAt = nowUtc;

                await _accesorSubscription.AddAsync(subscription);
                await _accesorSubscription.SaveChangesAsync();

                TempData["SuccessMessage"] = "Subscription created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating subscription");
                TempData["ErrorMessage"] = "An error occurred while creating the subscription.";

                var users = await _accesorUser.GetAllAsync();
                var plans = await _accesorPlan.GetAllAsync();
                model.Users = users.Select(u => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = u.Id.ToString(), Text = $"{u.UserName} ({u.Email})" });
                model.Plans = plans.Select(p => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = p.Id.ToString(), Text = $"{p.Name} - {p.Price:N2} {p.Currency}" });
                return View(model);
            }
        }

        // POST /Subscriptions/Delete/{id}
        [HttpPost("Delete/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id <= 0)
            {
                _logger.LogWarning("Invalid subscription ID provided for delete: {Id}", id);
                TempData["ErrorMessage"] = "Invalid subscription ID.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _logger.LogInformation("Attempting to hard delete subscription with ID: {Id}", id);

                var subscription = await _accesorSubscription.GetByIdAsync(id.Value);

                if (subscription == null)
                {
                    _logger.LogWarning("Subscription with ID {Id} not found for deletion", id);
                    TempData["ErrorMessage"] = $"Subscription with ID {id} was not found.";
                    return RedirectToAction(nameof(Index));
                }

                await _accesorSubscription.DeleteAsync(subscription);
                await _accesorSubscription.SaveChangesAsync();

                _logger.LogInformation("Successfully hard deleted subscription with ID: {Id}", id);
                TempData["SuccessMessage"] = "Subscription was deleted successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting subscription with ID: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the subscription. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }
    }

}