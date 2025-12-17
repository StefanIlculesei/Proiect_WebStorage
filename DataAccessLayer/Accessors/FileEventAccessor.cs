using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelLibrary.Models;
using PersistenceLayer;
using LoggingLayer;

namespace DataAccessLayer.Accessors
{
    public class FileEventAccessor : BaseDataAccessor<FileEvent>
    {
        public FileEventAccessor(WebStorageContext context, ILogger<FileEventAccessor> logger) : base(context, logger)
        {
        }

        public async Task<IEnumerable<FileEvent>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .Where(fe => fe.UserId == userId)
                    .OrderByDescending(fe => fe.EventDate)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(GetByUserIdAsync), ex, $"userId: {userId}");
                throw;
            }
        }

        public async Task<IEnumerable<FileEvent>> GetByFileIdAsync(int fileId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .Where(fe => fe.FileId == fileId)
                    .OrderByDescending(fe => fe.EventDate)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(GetByFileIdAsync), ex, $"fileId: {fileId}");
                throw;
            }
        }

        public async Task<IEnumerable<FileEvent>> GetRecentEventsAsync(int userId, int count, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .Where(fe => fe.UserId == userId)
                    .OrderByDescending(fe => fe.EventDate)
                    .Take(count)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(GetRecentEventsAsync), ex, $"userId: {userId}, count: {count}");
                throw;
            }
        }
    }
}
