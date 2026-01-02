using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using DataAccessLayer.Accessors;
using ModelLibrary.Models;
using ServiceLayer.Interfaces;
using ServiceLayer.Exceptions;
using PersistenceLayer;
using LoggingLayer;
using FileModel = ModelLibrary.Models.File;

namespace ServiceLayer.Implementations;

public class FileService : IFileService
{
    private readonly FileAccessor _fileAccessor;
    private readonly FileEventAccessor _fileEventAccessor;
    private readonly UserAccessor _userAccessor;
    private readonly FolderAccessor _folderAccessor;
    private readonly PlanAccessor _planAccessor;
    private readonly SubscriptionAccessor _subscriptionAccessor;
    private readonly IStorageQuotaService _storageQuotaService;
    private readonly WebStorageContext _context;
    private readonly ILogger<FileService> _logger;

    public FileService(
        FileAccessor fileAccessor,
        FileEventAccessor fileEventAccessor,
        UserAccessor userAccessor,
        FolderAccessor folderAccessor,
        PlanAccessor planAccessor,
        SubscriptionAccessor subscriptionAccessor,
        IStorageQuotaService storageQuotaService,
        WebStorageContext context,
        ILogger<FileService> logger)
    {
        _fileAccessor = fileAccessor;
        _fileEventAccessor = fileEventAccessor;
        _userAccessor = userAccessor;
        _folderAccessor = folderAccessor;
        _planAccessor = planAccessor;
        _subscriptionAccessor = subscriptionAccessor;
        _storageQuotaService = storageQuotaService;
        _context = context;
        _logger = logger;
    }

    public async Task<FileModel?> GetByIdAsync(int id, int userId)
    {
        try
        {
            var file = await _fileAccessor.GetByIdAsync(id);

            // Verify ownership
            if (file == null || file.UserId != userId)
                return null;

            return file;
        }
        catch (Exception ex)
        {
            _logger.LogError(nameof(GetByIdAsync), ex, $"fileId: {id}, userId: {userId}");
            throw;
        }
    }

    public async Task<IReadOnlyList<FileModel>> GetByUserIdAsync(int userId)
    {
        try
        {
            var files = await _fileAccessor.GetByUserIdAsync(userId);
            return files.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(nameof(GetByUserIdAsync), ex, $"userId: {userId}");
            throw;
        }
    }

    public async Task<IReadOnlyList<FileModel>> GetByFolderIdAsync(int folderId)
    {
        try
        {
            var files = await _fileAccessor.GetByFolderIdAsync(folderId);
            return files.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(nameof(GetByFolderIdAsync), ex, $"folderId: {folderId}");
            throw;
        }
    }

    public async Task<IReadOnlyList<FileModel>> SearchByNameAsync(int userId, string searchTerm)
    {
        try
        {
            var files = await _fileAccessor.SearchByNameAsync(userId, searchTerm);
            return files.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(nameof(SearchByNameAsync), ex, $"userId: {userId}, searchTerm: {searchTerm}");
            throw;
        }
    }

    public async Task<IReadOnlyList<FileModel>> GetRecentFilesAsync(int userId, int limit = 10)
    {
        try
        {
            var files = await _fileAccessor.GetByUserIdAsync(userId);
            return files.OrderByDescending(f => f.UploadDate).Take(limit).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(nameof(GetRecentFilesAsync), ex, $"userId: {userId}, limit: {limit}");
            throw;
        }
    }

    public async Task<FileModel> UploadFileAsync(int userId, int? folderId, string fileName, long fileSize, string storagePath, string? mimeType, string? visibility)
    {
        try
        {
            // Validate plan limits
            await ValidatePlanLimitsAsync(userId, fileSize);

            // Validate folder if provided
            if (folderId.HasValue)
            {
                var folder = await _folderAccessor.GetByIdAsync(folderId.Value);
                if (folder == null || folder.UserId != userId)
                    throw new InvalidOperationException("Folder not found or does not belong to user");
            }

            // Create file record
            var file = new FileModel
            {
                UserId = userId,
                FolderId = folderId,
                FileName = fileName,
                FileSize = fileSize,
                StoragePath = storagePath,
                MimeType = mimeType,
                Visibility = visibility,
                UploadDate = DateTime.UtcNow
            };

            await _fileAccessor.AddAsync(file);

            // Update user's storage usage
            var user = await _userAccessor.GetByIdAsync(userId);
            if (user != null)
            {
                user.StorageUsed += fileSize;
                await _userAccessor.UpdateAsync(user);
            }

            await _context.SaveChangesAsync();

            return file;
        }
        catch (Exception ex)
        {
            _logger.LogError(nameof(UploadFileAsync), ex, $"userId: {userId}, fileSize: {fileSize}");
            throw;
        }
    }

    public async Task<FileModel> UploadFileAsync(int userId, int? folderId, IFormFile file, string displayFileName, string? visibility, string uploadsBasePath)
    {
        try
        {
            if (file == null || file.Length == 0)
                throw new InvalidOperationException("No file provided or file is empty");

            // Create uploads directory structure
            var uploadsDir = Path.Combine(uploadsBasePath, "uploads", userId.ToString());
            Directory.CreateDirectory(uploadsDir);

            // Generate unique filename to avoid collisions
            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadsDir, uniqueFileName);

            // Save file to disk
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Calculate storage path for database
            var storagePath = Path.Combine("uploads", userId.ToString(), uniqueFileName);

            // Call the existing upload method with the calculated path
            var uploadedFile = await UploadFileAsync(
                userId,
                folderId,
                displayFileName ?? file.FileName,
                file.Length,
                storagePath,
                file.ContentType,
                visibility
            );

            return uploadedFile;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(nameof(UploadFileAsync), ex, $"userId: {userId}, fileName: {file?.FileName}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(nameof(UploadFileAsync), ex, $"userId: {userId}, fileName: {file?.FileName}");
            throw;
        }
    }

    public async Task<FileModel> UpdateFileAsync(int id, int userId, string? fileName = null, string? visibility = null, int? folderId = null)
    {
        try
        {
            var file = await _fileAccessor.GetByIdAsync(id);

            if (file == null || file.UserId != userId)
                throw new InvalidOperationException("File not found or does not belong to user");

            // Update fields if provided
            if (!string.IsNullOrEmpty(fileName))
                file.FileName = fileName;

            if (!string.IsNullOrEmpty(visibility))
                file.Visibility = visibility;

            if (folderId.HasValue)
            {
                // Validate folder ownership
                var folder = await _folderAccessor.GetByIdAsync(folderId.Value);
                if (folder == null || folder.UserId != userId)
                    throw new InvalidOperationException("Folder not found or does not belong to user");

                file.FolderId = folderId;
            }

            await _fileAccessor.UpdateAsync(file);
            await _context.SaveChangesAsync();

            return file;
        }
        catch (Exception ex)
        {
            _logger.LogError(nameof(UpdateFileAsync), ex, $"fileId: {id}, userId: {userId}");
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(int id, int userId)
    {
        try
        {
            var file = await _fileAccessor.GetByIdAsync(id);

            if (file == null || file.UserId != userId)
                return false;

            // Soft delete
            file.IsDeleted = true;
            file.DeletedAt = DateTime.UtcNow;

            await _fileAccessor.UpdateAsync(file);

            // Update user's storage usage
            var user = await _userAccessor.GetByIdAsync(userId);
            if (user != null)
            {
                user.StorageUsed -= file.FileSize;
                if (user.StorageUsed < 0) user.StorageUsed = 0;
                await _userAccessor.UpdateAsync(user);
            }

            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(nameof(DeleteFileAsync), ex, $"fileId: {id}, userId: {userId}");
            throw;
        }
    }

    public async Task<FileModel?> MoveFileAsync(int id, int userId, int? targetFolderId)
    {
        try
        {
            var file = await _fileAccessor.GetByIdAsync(id);

            if (file == null || file.UserId != userId)
                return null;

            // Validate target folder if provided
            if (targetFolderId.HasValue)
            {
                var targetFolder = await _folderAccessor.GetByIdAsync(targetFolderId.Value);
                if (targetFolder == null || targetFolder.UserId != userId)
                    throw new InvalidOperationException("Target folder not found or does not belong to user");
            }

            file.FolderId = targetFolderId;
            await _fileAccessor.UpdateAsync(file);
            await _context.SaveChangesAsync();

            return file;
        }
        catch (Exception ex)
        {
            _logger.LogError(nameof(MoveFileAsync), ex, $"fileId: {id}, userId: {userId}");
            throw;
        }
    }

    public async Task<int> BulkMoveFilesAsync(List<int> fileIds, int userId, int? targetFolderId)
    {
        try
        {
            // Validate target folder if provided
            if (targetFolderId.HasValue)
            {
                var targetFolder = await _folderAccessor.GetByIdAsync(targetFolderId.Value);
                if (targetFolder == null || targetFolder.UserId != userId)
                    throw new InvalidOperationException("Target folder not found or does not belong to user");
            }

            int movedCount = 0;

            foreach (var fileId in fileIds)
            {
                var file = await _fileAccessor.GetByIdAsync(fileId);
                if (file != null && file.UserId == userId)
                {
                    file.FolderId = targetFolderId;
                    await _fileAccessor.UpdateAsync(file);
                    movedCount++;
                }
            }

            await _context.SaveChangesAsync();

            return movedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(nameof(BulkMoveFilesAsync), ex, $"userId: {userId}, fileCount: {fileIds.Count}");
            throw;
        }
    }

    public async Task<IReadOnlyList<FileEvent>> GetEventsForFileAsync(int fileId)
    {
        try
        {
            var events = await _fileEventAccessor.GetByFileIdAsync(fileId);
            return events.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(nameof(GetEventsForFileAsync), ex, $"fileId: {fileId}");
            throw;
        }
    }

    private async Task ValidatePlanLimitsAsync(int userId, long addedBytes)
    {
        try
        {
            // Use StorageQuotaService to validate upload against plan limits
            // This will throw typed exceptions if limits are exceeded
            await _storageQuotaService.ValidateUploadAsync(userId, addedBytes);
        }
        catch (StorageException)
        {
            // Re-throw storage exceptions (FileTooLargeException, QuotaExceededException, etc.)
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error validating plan limits for userId: {userId}, addedBytes: {addedBytes}");
            throw;
        }
    }
}
