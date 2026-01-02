using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DataAccessLayer.Accessors;
using WebMVC_Plans.Models;
using ModelLibrary.Models;
using AutoMapper;

namespace WebMVC_Plans.Controllers
{

    [Authorize(Policy = "AdminOnly")]
    public class PlansController : Controller
    {
        private readonly PlanAccessor _accesorPlan;
        private readonly SubscriptionAccessor _accesorSubscription;
        private readonly ILogger<PlansController> _logger;
        private readonly IMapper _mapper;

        public PlansController(PlanAccessor accesorPlan, SubscriptionAccessor accesorSubscription, ILogger<PlansController> logger, IMapper mapper)
        {
            _accesorPlan = accesorPlan ?? throw new ArgumentNullException(nameof(accesorPlan));
            _accesorSubscription = accesorSubscription ?? throw new ArgumentNullException(nameof(accesorSubscription));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Fetching all plans from database");

                // Get all plans from the database using AccesorPlan
                var plans = await _accesorPlan.GetAllAsync();

                // Compute subscriber counts per plan with a single grouped query to avoid DbContext concurrency
                var planIds = plans.Select(p => p.Id).ToList();
                var countMap = await _accesorSubscription.GetActiveCountsByPlanIdsAsync(planIds);

                var planViewModels = _mapper.Map<List<PlanViewModel>>(plans);

                foreach (var vm in planViewModels)
                {
                    vm.SubscriptionCount = countMap.TryGetValue(vm.Id, out var c) ? c : 0;
                }

                planViewModels = planViewModels.OrderBy(p => p.Price).ToList();

                _logger.LogInformation("Successfully retrieved {Count} plans", planViewModels.Count);

                return View(planViewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching plans");
                TempData["ErrorMessage"] = "An error occurred while loading the plans. Please try again later.";
                return View(new List<PlanViewModel>());
            }
        }


        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || id <= 0)
            {
                _logger.LogWarning("Invalid plan ID provided: {Id}", id);
                TempData["ErrorMessage"] = "Invalid plan ID.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _logger.LogInformation("Fetching plan with ID: {Id}", id);

                // Get the plan by ID using AccesorPlan
                var plan = await _accesorPlan.GetByIdAsync(id.Value);

                if (plan == null)
                {
                    _logger.LogWarning("Plan with ID {Id} not found", id);
                    TempData["ErrorMessage"] = $"Plan with ID {id} was not found.";
                    return RedirectToAction(nameof(Index));
                }

                var planViewModel = _mapper.Map<PlanViewModel>(plan);

                // Get active subscription count for consistency with Index page
                var countMap = await _accesorSubscription.GetActiveCountsByPlanIdsAsync(new List<int> { id.Value });
                planViewModel.SubscriptionCount = countMap.TryGetValue(id.Value, out var count) ? count : 0;

                _logger.LogInformation("Successfully retrieved plan: {PlanName}", plan.Name);

                return View(planViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching plan with ID: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the plan details. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            _logger.LogInformation("Displaying Create form");
            return View(new PlanViewModel { Currency = "USD" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PlanViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for Create");
                return View(model);
            }

            try
            {
                _logger.LogInformation("Creating new plan: {PlanName}", model.Name);

                // Convert ViewModel to Entity
                var plan = _mapper.Map<Plan>(model);
                plan.CreatedAt = DateTime.UtcNow;
                plan.UpdatedAt = DateTime.UtcNow;

                // Add entity using AccesorPlan
                await _accesorPlan.AddAsync(plan);
                await _accesorPlan.SaveChangesAsync();

                _logger.LogInformation("Successfully created plan with ID: {Id}", plan.Id);
                TempData["SuccessMessage"] = $"Plan '{plan.Name}' was created successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating plan");
                ModelState.AddModelError("", "An error occurred while creating the plan. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id <= 0)
            {
                _logger.LogWarning("Invalid plan ID provided for Edit: {Id}", id);
                TempData["ErrorMessage"] = "Invalid plan ID.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _logger.LogInformation("Fetching plan for edit with ID: {Id}", id);

                var plan = await _accesorPlan.GetByIdAsync(id.Value);

                if (plan == null)
                {
                    _logger.LogWarning("Plan with ID {Id} not found for edit", id);
                    TempData["ErrorMessage"] = $"Plan with ID {id} was not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Convert Entity to ViewModel
                var planViewModel = _mapper.Map<PlanViewModel>(plan);

                _logger.LogInformation("Displaying edit form for plan: {PlanName}", plan.Name);

                return View(planViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching plan for edit with ID: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the plan. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PlanViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for Edit");
                return View(model);
            }

            try
            {
                _logger.LogInformation("Updating plan with ID: {Id}", model.Id);

                // Get existing plan
                var existingPlan = await _accesorPlan.GetByIdAsync(model.Id);

                if (existingPlan == null)
                {
                    _logger.LogWarning("Plan with ID {Id} not found for update", model.Id);
                    TempData["ErrorMessage"] = $"Plan with ID {model.Id} was not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Update entity properties
                _mapper.Map(model, existingPlan);
                existingPlan.UpdatedAt = DateTime.UtcNow;

                // Update entity using AccesorPlan
                await _accesorPlan.UpdateAsync(existingPlan);
                await _accesorPlan.SaveChangesAsync();

                _logger.LogInformation("Successfully updated plan: {PlanName}", existingPlan.Name);
                TempData["SuccessMessage"] = $"Plan '{existingPlan.Name}' was updated successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating plan with ID: {Id}", model.Id);
                ModelState.AddModelError("", "An error occurred while updating the plan. Please try again.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id <= 0)
            {
                _logger.LogWarning("Invalid plan ID provided for delete: {Id}", id);
                TempData["ErrorMessage"] = "Invalid plan ID.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _logger.LogInformation("Attempting to soft delete plan with ID: {Id}", id);

                var plan = await _accesorPlan.SoftDeleteAsync(id.Value);

                if (plan == null)
                {
                    _logger.LogWarning("Plan with ID {Id} not found for deletion", id);
                    TempData["ErrorMessage"] = $"Plan with ID {id} was not found.";
                    return RedirectToAction(nameof(Index));
                }

                await _accesorPlan.SaveChangesAsync();

                _logger.LogInformation("Successfully soft deleted plan: {PlanName}", plan.Name);
                TempData["SuccessMessage"] = $"Plan '{plan.Name}' was deleted successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting plan with ID: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the plan. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
