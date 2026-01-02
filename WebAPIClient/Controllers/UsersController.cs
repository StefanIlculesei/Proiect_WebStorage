using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ModelLibrary.Models;
using WebAPIClient.DTOs;
using DataAccessLayer.Accessors;
using ServiceLayer.Interfaces;

namespace WebAPIClient.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly FileAccessor _fileAccessor;
        private readonly FolderAccessor _folderAccessor;
        private readonly IStorageQuotaService _storageQuotaService;
        private readonly IMapper _mapper;
        private const long DEFAULT_STORAGE_LIMIT = 5368709120; // 5GB default

        public UsersController(
            UserManager<User> userManager,
            FileAccessor fileAccessor,
            FolderAccessor folderAccessor,
            IStorageQuotaService storageQuotaService,
            IMapper mapper)
        {
            _userManager = userManager;
            _fileAccessor = fileAccessor;
            _folderAccessor = folderAccessor;
            _storageQuotaService = storageQuotaService;
            _mapper = mapper;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            var rootFolder = await _folderAccessor.GetOrCreateRootFolderAsync(userId);

            var response = _mapper.Map<UserProfileResponse>(user);
            response.RootFolderId = rootFolder.Id;
            return Ok(response);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            _mapper.Map(request, user);
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
            }

            var rootFolder = await _folderAccessor.GetOrCreateRootFolderAsync(userId);

            var response = _mapper.Map<UserProfileResponse>(user);
            response.RootFolderId = rootFolder.Id;
            return Ok(response);
        }

        [HttpGet("storage")]
        public async Task<IActionResult> GetStorageUsage()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            // Get actual file and folder counts
            var files = await _fileAccessor.GetByUserIdAsync(userId);
            var folders = await _folderAccessor.GetByUserIdAsync(userId);

            var totalFiles = files.Count();
            var totalFolders = folders.Count();

            // Get storage limit from active subscription
            long storageLimit = DEFAULT_STORAGE_LIMIT;
            try
            {
                var quotaInfo = await _storageQuotaService.GetQuotaInfoAsync(userId);
                storageLimit = quotaInfo.TotalStorageLimit;
            }
            catch
            {
                // If no active subscription, use default 5GB
                storageLimit = DEFAULT_STORAGE_LIMIT;
            }

            var response = new StorageUsageResponse
            {
                StorageUsed = user.StorageUsed,
                StorageLimit = storageLimit,
                UsagePercentage = (user.StorageUsed / (double)storageLimit) * 100,
                TotalFiles = totalFiles,
                TotalFolders = totalFolders
            };

            return Ok(response);
        }

        [HttpGet("dashboard-stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            // Get actual file and folder counts
            var files = await _fileAccessor.GetByUserIdAsync(userId);
            var folders = await _folderAccessor.GetByUserIdAsync(userId);

            var totalFiles = files.Count();
            var totalFolders = folders.Count();

            // Get storage limit from active subscription
            long storageLimit = DEFAULT_STORAGE_LIMIT;
            try
            {
                var quotaInfo = await _storageQuotaService.GetQuotaInfoAsync(userId);
                storageLimit = quotaInfo.TotalStorageLimit;
            }
            catch
            {
                // If no active subscription, use default 5GB
                storageLimit = DEFAULT_STORAGE_LIMIT;
            }

            var response = new DashboardStatsResponse
            {
                StorageUsed = user.StorageUsed,
                StorageLimit = storageLimit,
                StoragePercentage = (int)((user.StorageUsed / (double)storageLimit) * 100),
                TotalFiles = totalFiles,
                TotalFolders = totalFolders
            };

            return Ok(response);
        }

        [HttpDelete("account")]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }


            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
            }

            return Ok(new { Message = "Account successfully deleted" });
        }
    }
}
