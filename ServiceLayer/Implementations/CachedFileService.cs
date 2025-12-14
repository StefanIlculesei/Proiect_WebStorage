using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using ModelLibrary.Models;
using ServiceLayer.Interfaces;
using ServiceLayer.Options;
using FileModel = ModelLibrary.Models.File;

namespace ServiceLayer.Implementations;

public class CachedFileService : IFileService
{
    private readonly IFileService _inner;
    private readonly IMemoryCache _cache;
    private readonly CacheOptions _options;

    public CachedFileService(IFileService inner, IMemoryCache cache, IOptions<CacheOptions> options)
    {
        _inner = inner;
        _cache = cache;
        _options = options.Value;
    }

    public async Task<FileModel?> GetByIdAsync(int id)
    {
        if (!_options.Enabled) return await _inner.GetByIdAsync(id);
        var key = Key($"file:{id}");
        if (_cache.TryGetValue(key, out FileModel? cached)) return cached;
        var value = await _inner.GetByIdAsync(id);
        if (value != null)
            _cache.Set(key, value, TimeSpan.FromSeconds(_options.DefaultTtlSeconds));
        return value;
    }

    public async Task<IReadOnlyList<FileModel>> ListByFolderAsync(int folderId)
    {
        if (!_options.Enabled) return await _inner.ListByFolderAsync(folderId);
        var key = Key($"folder:{folderId}:files");
        if (_cache.TryGetValue(key, out IReadOnlyList<FileModel>? cached) && cached != null) return cached;
        var value = await _inner.ListByFolderAsync(folderId);
        _cache.Set(key, value, TimeSpan.FromSeconds(_options.DefaultTtlSeconds));
        return value;
    }

    public async Task<FileModel> CreateAsync(FileModel file)
    {
        var created = await _inner.CreateAsync(file);
        if (created.FolderId.HasValue)
            InvalidateFolder(created.FolderId.Value);
        Set(Key($"file:{created.Id}"), created);
        return created;
    }

    public async Task<FileModel> UpdateAsync(FileModel file)
    {
        var updated = await _inner.UpdateAsync(file);
        if (updated.FolderId.HasValue)
            InvalidateFolder(updated.FolderId.Value);
        Set(Key($"file:{updated.Id}"), updated);
        return updated;
    }

    public async Task<bool> DeleteAsync(int id)
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

    public async Task<IReadOnlyList<FileEvent>> GetEventsForFileAsync(int fileId)
    {
        if (!_options.Enabled) return await _inner.GetEventsForFileAsync(fileId);
        var key = Key($"file:{fileId}:events");
        if (_cache.TryGetValue(key, out IReadOnlyList<FileEvent>? cached) && cached != null) return cached;
        var value = await _inner.GetEventsForFileAsync(fileId);
        _cache.Set(key, value, TimeSpan.FromSeconds(_options.DefaultTtlSeconds));
        return value;
    }

    private string Key(string raw) => $"svc:{raw}";
    private void Set<T>(string key, T value) => _cache.Set(key, value, TimeSpan.FromSeconds(_options.DefaultTtlSeconds));
    private void Remove(string key) => _cache.Remove(key);
    private void InvalidateFolder(int folderId) => Remove(Key($"folder:{folderId}:files"));
}
