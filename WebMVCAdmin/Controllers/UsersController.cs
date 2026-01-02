using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DataAccessLayer.Accessors;
using WebMVC_Plans.Models;
using ModelLibrary.Models;
using AutoMapper;

namespace WebMVC_Plans.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class UsersController : Controller
    {
        private readonly UserAccessor _userAccessor;
        private readonly ILogger<UsersController> _logger;
        private readonly IMapper _mapper;

        public UsersController(UserAccessor userAccessor, ILogger<UsersController> logger, IMapper mapper)
        {
            _userAccessor = userAccessor ?? throw new ArgumentNullException(nameof(userAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Fetching all users");
                var users = await _userAccessor.GetAllAsync();
                var userViewModels = _mapper.Map<List<UserViewModel>>(users);
                return View(userViewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching users");
                TempData["ErrorMessage"] = "Error loading users.";
                return View(new List<UserViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            try
            {
                var user = await _userAccessor.GetWithSubscriptionsAsync(id.Value);
                if (user == null) return NotFound();

                var viewModel = _mapper.Map<UserViewModel>(user);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user details for ID {Id}", id);
                TempData["ErrorMessage"] = "Error loading user details.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            try
            {
                var user = await _userAccessor.GetByIdAsync(id.Value);
                if (user == null) return NotFound();

                var viewModel = _mapper.Map<EditUserViewModel>(user);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user for edit ID {Id}", id);
                TempData["ErrorMessage"] = "Error loading user for edit.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var user = await _userAccessor.GetByIdAsync(model.Id);
                if (user == null) return NotFound();

                _mapper.Map(model, user);
                user.UpdatedAt = DateTime.UtcNow;

                await _userAccessor.UpdateAsync(user);
                await _userAccessor.SaveChangesAsync();

                TempData["SuccessMessage"] = "User updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user ID {Id}", model.Id);
                ModelState.AddModelError("", "Error updating user.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id <= 0)
            {
                _logger.LogWarning("Invalid user ID provided for delete: {Id}", id);
                TempData["ErrorMessage"] = "Invalid user ID.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _logger.LogInformation("Attempting to soft delete user with ID: {Id}", id);

                var user = await _userAccessor.SoftDeleteAsync(id.Value);

                if (user == null)
                {
                    _logger.LogWarning("User with ID {Id} not found for deletion", id);
                    TempData["ErrorMessage"] = $"User with ID {id} was not found.";
                    return RedirectToAction(nameof(Index));
                }

                await _userAccessor.SaveChangesAsync();

                _logger.LogInformation("Successfully soft deleted user: {UserName}", user.UserName);
                TempData["SuccessMessage"] = $"User '{user.UserName}' was deleted successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting user with ID: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the user. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
