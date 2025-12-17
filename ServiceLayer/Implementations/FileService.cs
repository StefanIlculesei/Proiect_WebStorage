using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DataAccessLayer.Accessors;
using ModelLibrary.Models;
using ServiceLayer.Interfaces;
using PersistenceLayer;
using LoggingLayer;
using FileModel = ModelLibrary.Models.File;

namespace ServiceLayer.Implementations;

public class FileService : IFileService
{
    private readonly FileAccessor _fileAccessor;
    private readonly FileEventAccessor _fileEventAccessor;
    private readonly PlanAccessor _planAccessor;
    private readonly SubscriptionAccessor _subscriptionAccessor;
    private readonly WebStorageContext _context;
    private readonly ILogger<FileService> _logger;

    public FileService(
        FileAccessor fileAccessor,
        FileEventAccessor fileEventAccessor,
        PlanAccessor planAccessor,
        SubscriptionAccessor subscriptionAccessor,
        WebStorageContext context,
        ILogger<FileService> logger)
    {
        _fileAccessor = fileAccessor;
        _fileEventAccessor = fileEventAccessor;
        _planAccessor = planAccessor;
        _subscriptionAccessor = subscriptionAccessor;
        _context = context;
        _logger = logger;
    }

    public async Task<FileModel?> GetByIdAsync(int id) => await _fileAccessor.GetByIdAsync(id);

    public async Task<IReadOnlyList<FileModel>> ListByFolderAsync(int folderId)
    {
        var files = await _fileAccessor.GetByFolderIdAsync(folderId);
        return files.ToList();
    }

    public async Task<FileModel> CreateAsync(FileModel file)
    {
        try
        {
            await ValidatePlanLimitsAsync(file.UserId, file.FileSize);
            await _fileAccessor.AddAsync(file);
            await _context.SaveChangesAsync();
            return file;
        }
        catch (Exception ex)
        {
            _logger.LogError(nameof(CreateAsync), ex, $"userId: {file.UserId}, fileSize: {file.FileSize}");
            throw;
        }
    }

    public async Task<FileModel> UpdateAsync(FileModel file)
    {
        try
        {
            var existing = await _fileAccessor.GetByIdAsync(file.Id);
            if (existing == null) throw new InvalidOperationException("File not found");

            var delta = Math.Max(0, file.FileSize - existing.FileSize);
            if (delta > 0)
                await ValidatePlanLimitsAsync(existing.UserId, delta);

            await _fileAccessor.UpdateAsync(file);
            await _context.SaveChangesAsync();
            return file;
        }
        catch (Exception ex)
        {
            _logger.LogError(nameof(UpdateAsync), ex, $"fileId: {file.Id}, newSize: {file.FileSize}");
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var existing = await _fileAccessor.GetByIdAsync(id);
            if (existing == null) return false;

            await _fileAccessor.DeleteAsync(existing);
            await _context.SaveChangesAsync();
            return true;
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
            // Get active subscription and plan limits
            var subscription = await _subscriptionAccessor.GetActiveSubscriptionByUserIdAsync(userId)
                ?? throw new InvalidOperationException("No active subscription for user");
            var plan = await _planAccessor.GetByIdAsync(subscription.PlanId)
                ?? throw new InvalidOperationException("Plan not found");

            if (addedBytes > plan.MaxFileSize)
                throw new InvalidOperationException($"File size ({addedBytes} bytes) exceeds plan's max file size ({plan.MaxFileSize} bytes)");

            var currentUsage = await _fileAccessor.GetTotalSizeByUserAsync(userId);
            if (currentUsage + addedBytes > plan.LimitSize)
                throw new InvalidOperationException($"Total storage ({currentUsage + addedBytes} bytes) exceeds plan storage capacity ({plan.LimitSize} bytes)");
        }
        catch (Exception ex)
        {
            _logger.LogError(nameof(ValidatePlanLimitsAsync), ex, $"userId: {userId}, addedBytes: {addedBytes}");
            throw;
        }
    }
}
