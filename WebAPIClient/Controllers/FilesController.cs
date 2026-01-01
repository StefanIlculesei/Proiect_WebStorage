using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebAPIClient.DTOs;
using ServiceLayer.Exceptions;
using ServiceLayer.Constants;
using LoggingLayer;
using ServiceLayer.Interfaces;

namespace WebAPIClient.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FilesController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FilesController> _logger;

        public FilesController(
            IFileService fileService,
            IMapper mapper,
            IWebHostEnvironment environment,
            ILogger<FilesController> logger)
        {
            _fileService = fileService;
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
                var files = await _fileService.GetByUserIdAsync(userId);
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
                var files = await _fileService.GetByFolderIdAsync(folderId);
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
                var file = await _fileService.GetByIdAsync(id, userId);

                if (file == null)
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
                var file = await _fileService.GetByIdAsync(id, userId);

                if (file == null)
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
                    return BadRequest(new ApiErrorResponse
                    {
                        Message = "No file was provided or file is empty",
                        Title = "Invalid File",
                        ErrorCode = ErrorCatalog.INVALID_FILE,
                        ErrorCategory = ErrorCategories.VALIDATION,
                        HttpStatusCode = 400,
                        IsActionable = false
                    });
                }

                // Use service to handle file upload (including I/O operations)
                var file = await _fileService.UploadFileAsync(
                    userId,
                    request.FolderId,
                    request.File,
                    request.FileName,
                    request.Visibility,
                    _environment.ContentRootPath
                );

                var response = _mapper.Map<FileResponse>(file);
                return CreatedAtAction(nameof(GetFileById), new { id = file.Id }, response);
            }
            catch (FileTooLargeException ex)
            {
                _logger.LogWarning($"File too large: {request.File?.FileName}");
                return StatusCode(ex.HttpStatusCode, new ApiErrorResponse
                {
                    Message = ex.Message,
                    Title = ex.Title,
                    ErrorCode = ex.ErrorCode,
                    ErrorCategory = ex.ErrorCategory,
                    HttpStatusCode = ex.HttpStatusCode,
                    IsActionable = ex.IsActionable,
                    Details = new ErrorDetails
                    {
                        MaxFileSize = (long?)ex.Details.GetValueOrDefault("maxFileSize"),
                        FileSize = (long?)ex.Details.GetValueOrDefault("actualFileSize")
                    }
                });
            }
            catch (QuotaExceededException ex)
            {
                _logger.LogWarning($"Storage quota exceeded for user upload");
                return StatusCode(ex.HttpStatusCode, new ApiErrorResponse
                {
                    Message = ex.Message,
                    Title = ex.Title,
                    ErrorCode = ex.ErrorCode,
                    ErrorCategory = ex.ErrorCategory,
                    HttpStatusCode = ex.HttpStatusCode,
                    IsActionable = ex.IsActionable,
                    Details = new ErrorDetails
                    {
                        StorageRemaining = (long?)ex.Details.GetValueOrDefault("storageRemaining"),
                        FileSize = (long?)ex.Details.GetValueOrDefault("fileSize"),
                        StorageLimit = (long?)ex.Details.GetValueOrDefault("storageLimit")
                    }
                });
            }
            catch (NoActiveSubscriptionException ex)
            {
                _logger.LogWarning($"No active subscription for upload");
                return StatusCode(ex.HttpStatusCode, new ApiErrorResponse
                {
                    Message = ex.Message,
                    Title = ex.Title,
                    ErrorCode = ex.ErrorCode,
                    ErrorCategory = ex.ErrorCategory,
                    HttpStatusCode = ex.HttpStatusCode,
                    IsActionable = ex.IsActionable
                });
            }
            catch (SubscriptionExpiredException ex)
            {
                _logger.LogWarning($"Subscription expired for upload");
                return StatusCode(ex.HttpStatusCode, new ApiErrorResponse
                {
                    Message = ex.Message,
                    Title = ex.Title,
                    ErrorCode = ex.ErrorCode,
                    ErrorCategory = ex.ErrorCategory,
                    HttpStatusCode = ex.HttpStatusCode,
                    IsActionable = ex.IsActionable,
                    Details = new ErrorDetails
                    {
                        ExpiredDate = (DateTime?)ex.Details.GetValueOrDefault("expiredDate")
                    }
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, $"Invalid operation during upload: {request.File?.FileName}");
                return BadRequest(new ApiErrorResponse
                {
                    Message = ex.Message,
                    Title = "Invalid Request",
                    ErrorCode = ErrorCatalog.INVALID_FILE,
                    ErrorCategory = ErrorCategories.VALIDATION,
                    HttpStatusCode = 400,
                    IsActionable = false
                });
            }
            catch (FileSaveException ex)
            {
                _logger.LogError(ex, $"File save failed: {request.File?.FileName}");
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = ex.Message,
                    Title = ex.Title,
                    ErrorCode = ex.ErrorCode,
                    ErrorCategory = ex.ErrorCategory,
                    HttpStatusCode = ex.HttpStatusCode,
                    IsActionable = ex.IsActionable
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error during upload: {request.File?.FileName}");
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An unexpected error occurred while uploading the file",
                    Title = "Server Error",
                    ErrorCode = ErrorCatalog.UNKNOWN_ERROR,
                    ErrorCategory = ErrorCategories.SERVER,
                    HttpStatusCode = 500,
                    IsActionable = false
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFile(int id, [FromBody] FileUpdateRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var file = await _fileService.UpdateFileAsync(id, userId, request.FileName, request.Visibility, request.FolderId);

                var response = _mapper.Map<FileResponse>(file);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(nameof(UpdateFile), ex, $"fileId: {id}");
                return NotFound(new { Message = ex.Message });
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
                var result = await _fileService.DeleteFileAsync(id, userId);

                if (!result)
                {
                    return NotFound(new { Message = "File not found" });
                }

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
                var files = await _fileService.SearchByNameAsync(userId, query);
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
                var file = await _fileService.MoveFileAsync(id, userId, request.TargetFolderId);

                if (file == null)
                {
                    return NotFound(new { Message = "File not found" });
                }

                var response = _mapper.Map<FileResponse>(file);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(nameof(MoveFile), ex, $"fileId: {id}");
                return BadRequest(new { Message = ex.Message });
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
                var movedCount = await _fileService.BulkMoveFilesAsync(request.FileIds, userId, request.TargetFolderId);

                return Ok(new { Message = $"{movedCount} files moved successfully", MovedCount = movedCount });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(nameof(BulkMoveFiles), ex, $"fileCount: {request.FileIds.Count}");
                return BadRequest(new { Message = ex.Message });
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
                var files = await _fileService.GetRecentFilesAsync(userId, limit);
                var response = _mapper.Map<IEnumerable<FileResponse>>(files);
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
