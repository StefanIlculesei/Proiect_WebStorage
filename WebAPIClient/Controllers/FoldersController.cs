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
            var rootFolder = await _folderAccessor.GetOrCreateRootFolderAsync(userId);
            var folders = await _folderAccessor.GetSubFoldersAsync(rootFolder.Id);
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
            // Load the full node (includes Files and SubFolders for this level)
            var folder = await _folderAccessor.GetByIdAsync(id);

            if (folder == null || folder.UserId != userId)
            {
                return NotFound(new { Message = "Folder not found" });
            }

            var response = await BuildFolderTreeAsync(folder, userId);
            return Ok(response);
        }

        private async Task<FolderTreeResponse> BuildFolderTreeAsync(Folder folder, int userId)
        {
            // Build current node with counts
            var node = new FolderTreeResponse
            {
                Id = folder.Id,
                Name = folder.Name,
                ParentFolderId = folder.ParentFolderId,
                FileCount = folder.Files?.Count(f => !f.IsDeleted) ?? 0,
                SubFolderCount = folder.SubFolders?.Count(sf => !sf.IsDeleted) ?? 0,
                SubFolders = new List<FolderTreeResponse>()
            };

            // Iterate children and build recursively
            var children = (folder.SubFolders ?? new List<Folder>())
                .Where(sf => !sf.IsDeleted && sf.UserId == userId)
                .ToList();

            foreach (var child in children)
            {
                // Ensure child has its own Files and SubFolders loaded
                var fullChild = await _folderAccessor.GetByIdAsync(child.Id);
                if (fullChild != null && fullChild.UserId == userId)
                {
                    node.SubFolders.Add(await BuildFolderTreeAsync(fullChild, userId));
                }
            }

            return node;
        }

        [HttpPost]
        public async Task<IActionResult> CreateFolder([FromBody] FolderRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var parentFolderId = request.ParentFolderId;

            // If no parent provided, attach to user's root folder
            if (!parentFolderId.HasValue)
            {
                var rootFolder = await _folderAccessor.GetOrCreateRootFolderAsync(userId);
                parentFolderId = rootFolder.Id;
            }
            else
            {
                // Validate parent folder exists and belongs to user (if creating subfolder)
                var parentFolder = await _folderAccessor.GetByIdAsync(parentFolderId.Value);
                if (parentFolder == null || parentFolder.UserId != userId)
                {
                    return BadRequest(new { Message = "Parent folder not found or does not belong to user" });
                }
            }

            var folder = _mapper.Map<Folder>(request);
            folder.UserId = userId;
            folder.ParentFolderId = parentFolderId;
            folder.CreatedAt = DateTime.UtcNow;
            folder.UpdatedAt = DateTime.UtcNow;

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

        [HttpGet("{id}/contents")]
        public async Task<IActionResult> GetFolderContents(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var folder = await _folderAccessor.GetWithSubFoldersAsync(id);

            if (folder == null || folder.UserId != userId)
            {
                return NotFound(new { Message = "Folder not found" });
            }

            var folderWithFiles = await _folderAccessor.GetWithFilesAsync(id);
            var response = _mapper.Map<FolderContentsResponse>(folderWithFiles);
            response.SubFolders = _mapper.Map<List<FolderResponse>>(folder.SubFolders);

            return Ok(response);
        }

        [HttpPatch("{id}/move")]
        public async Task<IActionResult> MoveFolder(int id, [FromBody] MoveFolderRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var folder = await _folderAccessor.GetByIdAsync(id);

            if (folder == null || folder.UserId != userId)
            {
                return NotFound(new { Message = "Folder not found" });
            }

            // Prevent moving folder into itself or its descendants
            if (request.TargetParentFolderId == id)
            {
                return BadRequest(new { Message = "Cannot move folder into itself" });
            }

            // Validate target parent folder exists and belongs to user (if moving to a folder)
            if (request.TargetParentFolderId.HasValue)
            {
                var targetParentFolder = await _folderAccessor.GetByIdAsync(request.TargetParentFolderId.Value);
                if (targetParentFolder == null || targetParentFolder.UserId != userId)
                {
                    return BadRequest(new { Message = "Target parent folder not found or does not belong to user" });
                }
            }

            folder.ParentFolderId = request.TargetParentFolderId;
            folder.UpdatedAt = DateTime.UtcNow;

            await _folderAccessor.UpdateAsync(folder);
            await _folderAccessor.SaveChangesAsync();

            var response = _mapper.Map<FolderResponse>(folder);
            return Ok(response);
        }
    }
}
