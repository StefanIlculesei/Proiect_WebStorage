using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using ModelLibrary.Models;
using ServiceLayer.Interfaces;
using ServiceLayer.Options;
using LoggingLayer;
using FileModel = ModelLibrary.Models.File;

namespace ServiceLayer.Implementations;

public class CachedFileService : IFileService
{
    private readonly IFileService _inner;
    private readonly IMemoryCache _cache;
    private readonly CacheOptions _options;
    private readonly ILogger<CachedFileService> _logger;

    public CachedFileService(IFileService inner, IMemoryCache cache, IOptions<CacheOptions> options, ILogger<CachedFileService> logger)
    {
        _inner = inner;
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<FileModel?> GetByIdAsync(int id, int userId)
    {
        try
        {
            if (!_options.Enabled) return await _inner.GetByIdAsync(id, userId);
            var key = Key($"file:{id}:user:{userId}");
            if (_cache.TryGetValue(key, out FileModel? cached)) return cached;
            var value = await _inner.GetByIdAsync(id, userId);
            if (value != null)
                _cache.Set(key, value, TimeSpan.FromSeconds(_options.DefaultTtlSeconds));
            return value;
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
            if (!_options.Enabled) return await _inner.GetByUserIdAsync(userId);
            var key = Key($"user:{userId}:files");
            if (_cache.TryGetValue(key, out IReadOnlyList<FileModel>? cached) && cached != null) return cached;
            var value = await _inner.GetByUserIdAsync(userId);
            _cache.Set(key, value, TimeSpan.FromSeconds(_options.DefaultTtlSeconds));
            return value;
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
            if (!_options.Enabled) return await _inner.GetByFolderIdAsync(folderId);
            var key = Key($"folder:{folderId}:files");
            if (_cache.TryGetValue(key, out IReadOnlyList<FileModel>? cached) && cached != null) return cached;
            var value = await _inner.GetByFolderIdAsync(folderId);
            _cache.Set(key, value, TimeSpan.FromSeconds(_options.DefaultTtlSeconds));
            return value;
        }
        catch (Exception ex)
        {
            _logger.LogError(nameof(GetByFolderIdAsync), ex, $"folderId: {folderId}");
            throw;
        }
    }

    public async Task<IReadOnlyList<FileModel>> SearchByNameAsync(int userId, string searchTerm)
    {
        // Don't cache search results
        return await _inner.SearchByNameAsync(userId, searchTerm);
    }

    public async Task<IReadOnlyList<FileModel>> GetRecentFilesAsync(int userId, int limit = 10)
    {
        // Don't cache recent files as they change frequently
        return await _inner.GetRecentFilesAsync(userId, limit);
    }

    public async Task<FileModel> UploadFileAsync(int userId, int? folderId, string fileName, long fileSize, string storagePath, string? mimeType, string? visibility)
    {
        try
        {
            var file = await _inner.UploadFileAsync(userId, folderId, fileName, fileSize, storagePath, mimeType, visibility);
            if (folderId.HasValue)
                InvalidateFolder(folderId.Value);
            InvalidateUser(userId);
            Set(Key($"file:{file.Id}:user:{userId}"), file);
            return file;
        }
        catch (Exception ex)
        {
            _logger.LogError(nameof(UploadFileAsync), ex, $"fileName: {fileName}");
            throw;
        }
    }

    public async Task<FileModel> UpdateFileAsync(int id, int userId, string? fileName = null, string? visibility = null, int? folderId = null)
    {
        try
        {
            var file = await _inner.UpdateFileAsync(id, userId, fileName, visibility, folderId);
            if (folderId.HasValue)
                InvalidateFolder(folderId.Value);
            Invalidate(Key($"file:{id}:user:{userId}"));
            return file;
        }
        catch (Exception ex)
        {
            _logger.LogError(nameof(UpdateFileAsync), ex, $"fileId: {id}");
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(int id, int userId)
    {
        try
        {
            var result = await _inner.DeleteFileAsync(id, userId);
            if (result)
            {
                Invalidate(Key($"file:{id}:user:{userId}"));
                InvalidateUser(userId);
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(nameof(DeleteFileAsync), ex, $"fileId: {id}");
            throw;
        }
    }

    public async Task<FileModel?> MoveFileAsync(int id, int userId, int? targetFolderId)
    {
        try
        {
            var file = await _inner.MoveFileAsync(id, userId, targetFolderId);
            if (file != null)
            {
                if (targetFolderId.HasValue)
                    InvalidateFolder(targetFolderId.Value);
                Invalidate(Key($"file:{id}:user:{userId}"));
            }
            return file;
        }
        catch (Exception ex)
        {
            _logger.LogError(nameof(MoveFileAsync), ex, $"fileId: {id}");
            throw;
        }
    }

    public async Task<int> BulkMoveFilesAsync(List<int> fileIds, int userId, int? targetFolderId)
    {
        try
        {
            var count = await _inner.BulkMoveFilesAsync(fileIds, userId, targetFolderId);
            if (count > 0)
            {
                if (targetFolderId.HasValue)
                    InvalidateFolder(targetFolderId.Value);
                foreach (var fileId in fileIds)
                    Invalidate(Key($"file:{fileId}:user:{userId}"));
            }
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(nameof(BulkMoveFilesAsync), ex, $"fileCount: {fileIds.Count}");
            throw;
        }
    }

    public async Task<IReadOnlyList<FileEvent>> GetEventsForFileAsync(int fileId)
    {
        try
        {
            if (!_options.Enabled) return await _inner.GetEventsForFileAsync(fileId);
            var key = Key($"file:{fileId}:events");
            if (_cache.TryGetValue(key, out IReadOnlyList<FileEvent>? cached) && cached != null) return cached;
            var value = await _inner.GetEventsForFileAsync(fileId);
            _cache.Set(key, value, TimeSpan.FromSeconds(_options.DefaultTtlSeconds));
            return value;
        }
        catch (Exception ex)
        {
            _logger.LogError(nameof(GetEventsForFileAsync), ex, $"fileId: {fileId}");
            throw;
        }
    }

    private string Key(string raw) => $"svc:{raw}";
    private void Set<T>(string key, T value) => _cache.Set(key, value, TimeSpan.FromSeconds(_options.DefaultTtlSeconds));
    private void Invalidate(string key) => _cache.Remove(key);
    private void InvalidateFolder(int folderId) => Invalidate(Key($"folder:{folderId}:files"));
    private void InvalidateUser(int userId) => Invalidate(Key($"user:{userId}:files"));
}
