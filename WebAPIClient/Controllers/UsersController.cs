using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ModelLibrary.Models;
using WebAPIClient.DTOs;
using DataAccessLayer.Accessors;

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
        private readonly IMapper _mapper;

        public UsersController(
            UserManager<User> userManager,
            FileAccessor fileAccessor,
            FolderAccessor folderAccessor,
            IMapper mapper)
        {
            _userManager = userManager;
            _fileAccessor = fileAccessor;
            _folderAccessor = folderAccessor;
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

            var response = new StorageUsageResponse
            {
                StorageUsed = user.StorageUsed,
                StorageLimit = 5368709120, // Default 5GB, should be fetched from active subscription
                UsagePercentage = (user.StorageUsed / (double)5368709120) * 100,
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

            var response = new DashboardStatsResponse
            {
                StorageUsed = user.StorageUsed,
                StorageLimit = 5368709120,
                StoragePercentage = (int)((user.StorageUsed / (double)5368709120) * 100),
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
