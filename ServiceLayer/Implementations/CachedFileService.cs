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

    public async Task<FileModel?> GetByIdAsync(int id)
    {
        try
        {
            if (!_options.Enabled) return await _inner.GetByIdAsync(id);
            var key = Key($"file:{id}");
            if (_cache.TryGetValue(key, out FileModel? cached)) return cached;
            var value = await _inner.GetByIdAsync(id);
            if (value != null)
                _cache.Set(key, value, TimeSpan.FromSeconds(_options.DefaultTtlSeconds));
            return value;
        }
        catch (Exception ex)
        {
            _logger.LogError(nameof(GetByIdAsync), ex, $"fileId: {id}");
            throw;
        }
    }

    public async Task<IReadOnlyList<FileModel>> ListByFolderAsync(int folderId)
    {
        try
        {
            if (!_options.Enabled) return await _inner.ListByFolderAsync(folderId);
            var key = Key($"folder:{folderId}:files");
            if (_cache.TryGetValue(key, out IReadOnlyList<FileModel>? cached) && cached != null) return cached;
            var value = await _inner.ListByFolderAsync(folderId);
            _cache.Set(key, value, TimeSpan.FromSeconds(_options.DefaultTtlSeconds));
            return value;
        }
        catch (Exception ex)
        {
            _logger.LogError(nameof(ListByFolderAsync), ex, $"folderId: {folderId}");
            throw;
        }
    }

    public async Task<FileModel> CreateAsync(FileModel file)
    {
        try
        {
            var created = await _inner.CreateAsync(file);
            if (created.FolderId.HasValue)
                InvalidateFolder(created.FolderId.Value);
            Set(Key($"file:{created.Id}"), created);
            return created;
        }
        catch (Exception ex)
        {
            _logger.LogError(nameof(CreateAsync), ex, $"fileName: {file.FileName}");
            throw;
        }
    }

    public async Task<FileModel> UpdateAsync(FileModel file)
    {
        try
        {
            var updated = await _inner.UpdateAsync(file);
            if (updated.FolderId.HasValue)
                InvalidateFolder(updated.FolderId.Value);
            Set(Key($"file:{updated.Id}"), updated);
            return updated;
        }
        catch (Exception ex)
        {
            _logger.LogError(nameof(UpdateAsync), ex, $"fileId: {file.Id}");
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var existing = await _inner.GetByIdAsync(id);
            var ok = await _inner.DeleteAsync(id);
            if (ok)
            {
                Remove(Key($"file:{id}"));
                if (existing?.FolderId.HasValue == true)
                    InvalidateFolder(existing.FolderId.Value);
            }
            return ok;
        }
        catch (Exception ex)
        {
            _logger.LogError(nameof(DeleteAsync), ex, $"fileId: {id}");
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
    private void Remove(string key) => _cache.Remove(key);
    private void InvalidateFolder(int folderId) => Remove(Key($"folder:{folderId}:files"));
}
