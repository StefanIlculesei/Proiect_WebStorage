using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DataAccessLayer.Accessors;
using System.Security.Claims;
using ModelLibrary.Models;
using WebAPIClient.DTOs;
using LoggingLayer;

namespace WebAPIClient.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FilesController : ControllerBase
    {
        private readonly FileAccessor _fileAccessor;
        private readonly FolderAccessor _folderAccessor;
        private readonly UserAccessor _userAccessor;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FilesController> _logger;

        public FilesController(
            FileAccessor fileAccessor,
            FolderAccessor folderAccessor,
            UserAccessor userAccessor,
            IMapper mapper,
            IWebHostEnvironment environment,
            ILogger<FilesController> logger)
        {
            _fileAccessor = fileAccessor;
            _folderAccessor = folderAccessor;
            _userAccessor = userAccessor;
            _mapper = mapper;
            _environment = environment;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserFiles()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var files = await _fileAccessor.GetByUserIdAsync(userId);
                var response = _mapper.Map<IEnumerable<FileResponse>>(files);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(GetUserFiles), ex);
                return StatusCode(500, new { Message = "An error occurred while retrieving files" });
            }
        }

        [HttpGet("folder/{folderId}")]
        public async Task<IActionResult> GetFilesByFolder(int folderId)
        {
            try
            {
                var files = await _fileAccessor.GetByFolderIdAsync(folderId);
                var response = _mapper.Map<IEnumerable<FileResponse>>(files);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(GetFilesByFolder), ex, $"folderId: {folderId}");
                return StatusCode(500, new { Message = "An error occurred while retrieving files" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFileById(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var file = await _fileAccessor.GetByIdAsync(id);

                if (file == null || file.UserId != userId)
                {
                    return NotFound(new { Message = "File not found" });
                }

                var response = _mapper.Map<FileResponse>(file);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(GetFileById), ex, $"fileId: {id}");
                return StatusCode(500, new { Message = "An error occurred while retrieving the file" });
            }
        }

        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadFile(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var file = await _fileAccessor.GetByIdAsync(id);

                if (file == null || file.UserId != userId)
                {
                    return NotFound(new { Message = "File not found" });
                }

                var filePath = Path.Combine(_environment.ContentRootPath, file.StoragePath);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound(new { Message = "Physical file not found" });
                }

                var memory = new MemoryStream();
                using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;

                return File(memory, file.MimeType ?? "application/octet-stream", file.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(DownloadFile), ex, $"fileId: {id}");
                return StatusCode(500, new { Message = "An error occurred while downloading the file" });
            }
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] FileUploadRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                if (request.File == null || request.File.Length == 0)
                {
                    return BadRequest(new { Message = "No file uploaded" });
                }

                // Create uploads directory if it doesn't exist
                var uploadsDir = Path.Combine(_environment.ContentRootPath, "uploads", userId.ToString());
                Directory.CreateDirectory(uploadsDir);

                // Generate unique filename
                var uniqueFileName = $"{Guid.NewGuid()}_{request.File.FileName}";
                var filePath = Path.Combine(uploadsDir, uniqueFileName);

                // Save file to disk
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.File.CopyToAsync(stream);
                }

                // Create database record
                var file = new ModelLibrary.Models.File
                {
                    UserId = userId,
                    FolderId = request.FolderId,
                    FileName = request.FileName,
                    FileSize = request.File.Length,
                    StoragePath = Path.Combine("uploads", userId.ToString(), uniqueFileName),
                    MimeType = request.File.ContentType,
                    Visibility = request.Visibility,
                    UploadDate = DateTime.UtcNow
                };

                await _fileAccessor.AddAsync(file);

                // Update user's storage usage
                var user = await _userAccessor.GetByIdAsync(userId);
                if (user != null)
                {
                    user.StorageUsed += request.File.Length;
                    await _userAccessor.UpdateAsync(user);
                }

                await _fileAccessor.SaveChangesAsync();

                var response = _mapper.Map<FileResponse>(file);
                return CreatedAtAction(nameof(GetFileById), new { id = file.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(UploadFile), ex, $"fileName: {request.File?.FileName}");
                return StatusCode(500, new { Message = "An error occurred while uploading the file" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFile(int id, [FromBody] FileUpdateRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var file = await _fileAccessor.GetByIdAsync(id);

                if (file == null || file.UserId != userId)
                {
                    return NotFound(new { Message = "File not found" });
                }

                if (!string.IsNullOrEmpty(request.FileName))
                    file.FileName = request.FileName;

                if (!string.IsNullOrEmpty(request.Visibility))
                    file.Visibility = request.Visibility;

                if (request.FolderId.HasValue)
                    file.FolderId = request.FolderId;

                await _fileAccessor.UpdateAsync(file);
                await _fileAccessor.SaveChangesAsync();

                var response = _mapper.Map<FileResponse>(file);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(UpdateFile), ex, $"fileId: {id}");
                return StatusCode(500, new { Message = "An error occurred while updating the file" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFile(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var file = await _fileAccessor.GetByIdAsync(id);

                if (file == null || file.UserId != userId)
                {
                    return NotFound(new { Message = "File not found" });
                }

                file.IsDeleted = true;
                file.DeletedAt = DateTime.UtcNow;

                await _fileAccessor.UpdateAsync(file);

                // Update user's storage usage
                var user = await _userAccessor.GetByIdAsync(userId);
                if (user != null)
                {
                    user.StorageUsed -= file.FileSize;
                    if (user.StorageUsed < 0) user.StorageUsed = 0; // Prevent negative values
                    await _userAccessor.UpdateAsync(user);
                }

                await _fileAccessor.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(DeleteFile), ex, $"fileId: {id}");
                return StatusCode(500, new { Message = "An error occurred while deleting the file" });
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchFiles([FromQuery] string query)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var files = await _fileAccessor.SearchByNameAsync(userId, query);
                var response = _mapper.Map<IEnumerable<FileResponse>>(files);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(SearchFiles), ex, $"query: {query}");
                return StatusCode(500, new { Message = "An error occurred while searching files" });
            }
        }

        [HttpPatch("{id}/move")]
        public async Task<IActionResult> MoveFile(int id, [FromBody] MoveFileRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var file = await _fileAccessor.GetByIdAsync(id);

                if (file == null || file.UserId != userId)
                {
                    return NotFound(new { Message = "File not found" });
                }

                // Validate target folder exists and belongs to user (if moving to a folder)
                if (request.TargetFolderId.HasValue)
                {
                    var targetFolder = await _folderAccessor.GetByIdAsync(request.TargetFolderId.Value);
                    if (targetFolder == null || targetFolder.UserId != userId)
                    {
                        return BadRequest(new { Message = "Target folder not found or does not belong to user" });
                    }
                }

                file.FolderId = request.TargetFolderId;
                await _fileAccessor.UpdateAsync(file);
                await _fileAccessor.SaveChangesAsync();

                var response = _mapper.Map<FileResponse>(file);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(MoveFile), ex, $"fileId: {id}");
                return StatusCode(500, new { Message = "An error occurred while moving the file" });
            }
        }

        [HttpPost("bulk-move")]
        public async Task<IActionResult> BulkMoveFiles([FromBody] BulkMoveFilesRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                // Validate target folder exists and belongs to user (if moving to a folder)
                if (request.TargetFolderId.HasValue)
                {
                    var targetFolder = await _folderAccessor.GetByIdAsync(request.TargetFolderId.Value);
                    if (targetFolder == null || targetFolder.UserId != userId)
                    {
                        return BadRequest(new { Message = "Target folder not found or does not belong to user" });
                    }
                }

                var files = new List<ModelLibrary.Models.File>();

                foreach (var fileId in request.FileIds)
                {
                    var file = await _fileAccessor.GetByIdAsync(fileId);
                    if (file != null && file.UserId == userId)
                    {
                        file.FolderId = request.TargetFolderId;
                        await _fileAccessor.UpdateAsync(file);
                        files.Add(file);
                    }
                }

                await _fileAccessor.SaveChangesAsync();
                return Ok(new { Message = $"{files.Count} files moved successfully", MovedCount = files.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(BulkMoveFiles), ex, $"fileCount: {request.FileIds.Count}");
                return StatusCode(500, new { Message = "An error occurred while moving files" });
            }
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentFiles([FromQuery] int limit = 10)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var files = await _fileAccessor.GetByUserIdAsync(userId);
                var recentFiles = files.OrderByDescending(f => f.UploadDate).Take(limit);
                var response = _mapper.Map<IEnumerable<FileResponse>>(recentFiles);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(GetRecentFiles), ex, $"limit: {limit}");
                return StatusCode(500, new { Message = "An error occurred while retrieving recent files" });
            }
        }
    }
}
