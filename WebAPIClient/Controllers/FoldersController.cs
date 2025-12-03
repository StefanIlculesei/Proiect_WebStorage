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
    public class FoldersController : ControllerBase
    {
        private readonly FolderAccessor _folderAccessor;
        private readonly IMapper _mapper;

        public FoldersController(FolderAccessor folderAccessor, IMapper mapper)
        {
            _folderAccessor = folderAccessor;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserFolders()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var folders = await _folderAccessor.GetByUserIdAsync(userId);
            var response = _mapper.Map<IEnumerable<FolderResponse>>(folders);
            return Ok(response);
        }

        [HttpGet("root")]
        public async Task<IActionResult> GetRootFolders()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var folders = await _folderAccessor.GetRootFoldersAsync(userId);
            var response = _mapper.Map<IEnumerable<FolderResponse>>(folders);
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFolderById(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var folder = await _folderAccessor.GetByIdAsync(id);

            if (folder == null || folder.UserId != userId)
            {
                return NotFound(new { Message = "Folder not found" });
            }

            var response = _mapper.Map<FolderResponse>(folder);
            return Ok(response);
        }

        [HttpGet("{id}/tree")]
        public async Task<IActionResult> GetFolderTree(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var folder = await _folderAccessor.GetWithSubFoldersAsync(id);

            if (folder == null || folder.UserId != userId)
            {
                return NotFound(new { Message = "Folder not found" });
            }

            var response = _mapper.Map<FolderTreeResponse>(folder);
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateFolder([FromBody] FolderRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var folder = _mapper.Map<Folder>(request);
            folder.UserId = userId;

            await _folderAccessor.AddAsync(folder);
            await _folderAccessor.SaveChangesAsync();

            var response = _mapper.Map<FolderResponse>(folder);
            return CreatedAtAction(nameof(GetFolderById), new { id = folder.Id }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFolder(int id, [FromBody] FolderRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var folder = await _folderAccessor.GetByIdAsync(id);

            if (folder == null || folder.UserId != userId)
            {
                return NotFound(new { Message = "Folder not found" });
            }

            folder.Name = request.Name;
            folder.ParentFolderId = request.ParentFolderId;
            folder.UpdatedAt = DateTime.UtcNow;

            await _folderAccessor.UpdateAsync(folder);
            await _folderAccessor.SaveChangesAsync();

            var response = _mapper.Map<FolderResponse>(folder);
            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFolder(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var folder = await _folderAccessor.GetByIdAsync(id);

            if (folder == null || folder.UserId != userId)
            {
                return NotFound(new { Message = "Folder not found" });
            }

            folder.IsDeleted = true;
            folder.DeletedAt = DateTime.UtcNow;

            await _folderAccessor.UpdateAsync(folder);
            await _folderAccessor.SaveChangesAsync();

            return NoContent();
        }
    }
}
