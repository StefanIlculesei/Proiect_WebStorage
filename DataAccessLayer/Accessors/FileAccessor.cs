using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PersistenceLayer;
using ModelLibrary.Models;
using LoggingLayer;

namespace DataAccessLayer.Accessors
{
    public class FileAccessor : BaseDataAccessor<ModelLibrary.Models.File>
    {
        public FileAccessor(WebStorageContext context, ILogger<FileAccessor> logger) : base(context, logger)
        {
        }

        public async Task<IEnumerable<ModelLibrary.Models.File>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .Where(f => f.UserId == userId && !f.IsDeleted)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(GetByUserIdAsync), ex, $"userId: {userId}");
                throw;
            }
        }

        public async Task<IEnumerable<ModelLibrary.Models.File>> GetByFolderIdAsync(int folderId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .Where(f => f.FolderId == folderId && !f.IsDeleted)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(GetByFolderIdAsync), ex, $"folderId: {folderId}");
                throw;
            }
        }

        public async Task<ModelLibrary.Models.File?> GetByStoragePathAsync(string storagePath, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet.FirstOrDefaultAsync(f => f.StoragePath == storagePath, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(GetByStoragePathAsync), ex, $"storagePath: {storagePath}");
                throw;
            }
        }

        public async Task<long> GetTotalSizeByUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .Where(f => f.UserId == userId && !f.IsDeleted)
                    .SumAsync(f => f.FileSize, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(GetTotalSizeByUserAsync), ex, $"userId: {userId}");
                throw;
            }
        }

        public async Task<IEnumerable<ModelLibrary.Models.File>> SearchByNameAsync(int userId, string searchTerm, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .Where(f => f.UserId == userId && !f.IsDeleted && f.FileName.Contains(searchTerm))
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(SearchByNameAsync), ex, $"userId: {userId}, searchTerm: {searchTerm}");
                throw;
            }
        }
    }
}
