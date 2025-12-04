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
    public class FilesController : ControllerBase
    {
        private readonly FileAccessor _fileAccessor;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FilesController> _logger;

        public FilesController(
            FileAccessor fileAccessor,
            IMapper mapper,
            IWebHostEnvironment environment,
            ILogger<FilesController> logger)
        {
            _fileAccessor = fileAccessor;
            _mapper = mapper;
            _environment = environment;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserFiles()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var files = await _fileAccessor.GetByUserIdAsync(userId);
            var response = _mapper.Map<IEnumerable<FileResponse>>(files);
            return Ok(response);
        }

        [HttpGet("folder/{folderId}")]
        public async Task<IActionResult> GetFilesByFolder(int folderId)
        {
            var files = await _fileAccessor.GetByFolderIdAsync(folderId);
            var response = _mapper.Map<IEnumerable<FileResponse>>(files);
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFileById(int id)
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

        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadFile(int id)
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

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] FileUploadRequest request)
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
            await _fileAccessor.SaveChangesAsync();

            var response = _mapper.Map<FileResponse>(file);
            return CreatedAtAction(nameof(GetFileById), new { id = file.Id }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFile(int id, [FromBody] FileUpdateRequest request)
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFile(int id)
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
            await _fileAccessor.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchFiles([FromQuery] string query)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var files = await _fileAccessor.SearchByNameAsync(userId, query);
            var response = _mapper.Map<IEnumerable<FileResponse>>(files);
            return Ok(response);
        }

        [HttpPatch("{id}/move")]
        public async Task<IActionResult> MoveFile(int id, [FromBody] MoveFileRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var file = await _fileAccessor.GetByIdAsync(id);

            if (file == null || file.UserId != userId)
            {
                return NotFound(new { Message = "File not found" });
            }

            file.FolderId = request.TargetFolderId;
            await _fileAccessor.UpdateAsync(file);
            await _fileAccessor.SaveChangesAsync();

            var response = _mapper.Map<FileResponse>(file);
            return Ok(response);
        }

        [HttpPost("bulk-move")]
        public async Task<IActionResult> BulkMoveFiles([FromBody] BulkMoveFilesRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
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

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentFiles([FromQuery] int limit = 10)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var files = await _fileAccessor.GetByUserIdAsync(userId);
            var recentFiles = files.OrderByDescending(f => f.UploadDate).Take(limit);
            var response = _mapper.Map<IEnumerable<FileResponse>>(recentFiles);
            return Ok(response);
        }
    }
}
