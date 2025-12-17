using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelLibrary.Models;
using PersistenceLayer;
using LoggingLayer;

namespace DataAccessLayer.Accessors
{
    public class FolderAccessor : BaseDataAccessor<Folder>
    {
        public FolderAccessor(WebStorageContext context, ILogger<FolderAccessor> logger) : base(context, logger)
        {
        }

        public async Task<IEnumerable<Folder>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .Include(f => f.Files.Where(file => !file.IsDeleted))
                    .Include(f => f.SubFolders.Where(sub => !sub.IsDeleted))
                    .Where(f => f.UserId == userId && !f.IsDeleted)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(GetByUserIdAsync), ex, $"userId: {userId}");
                throw;
            }
        }

        public async Task<IEnumerable<Folder>> GetRootFoldersAsync(int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .Include(f => f.Files.Where(file => !file.IsDeleted))
                    .Include(f => f.SubFolders.Where(sub => !sub.IsDeleted))
                    .Where(f => f.UserId == userId && f.ParentFolderId == null && !f.IsDeleted)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(GetRootFoldersAsync), ex, $"userId: {userId}");
                throw;
            }
        }

        public async Task<IEnumerable<Folder>> GetSubFoldersAsync(int parentFolderId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .Include(f => f.Files.Where(file => !file.IsDeleted))
                    .Include(f => f.SubFolders.Where(sub => !sub.IsDeleted))
                    .Where(f => f.ParentFolderId == parentFolderId && !f.IsDeleted)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(GetSubFoldersAsync), ex, $"parentFolderId: {parentFolderId}");
                throw;
            }
        }

        public async Task<Folder?> GetWithFilesAsync(int folderId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .Include(f => f.Files.Where(file => !file.IsDeleted))
                    .FirstOrDefaultAsync(f => f.Id == folderId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(GetWithFilesAsync), ex, $"folderId: {folderId}");
                throw;
            }
        }

        public async Task<Folder?> GetWithSubFoldersAsync(int folderId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .Include(f => f.SubFolders.Where(sub => !sub.IsDeleted))
                    .FirstOrDefaultAsync(f => f.Id == folderId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(GetWithSubFoldersAsync), ex, $"folderId: {folderId}");
                throw;
            }
        }

        public override async Task<Folder?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .Include(f => f.Files.Where(file => !file.IsDeleted))
                    .Include(f => f.SubFolders.Where(sub => !sub.IsDeleted))
                    .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(GetByIdAsync), ex, $"id: {id}");
                throw;
            }
        }

        public async Task<Folder> GetOrCreateRootFolderAsync(int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var root = await _dbSet
                    .FirstOrDefaultAsync(f => f.UserId == userId && f.ParentFolderId == null && !f.IsDeleted, cancellationToken);

                if (root != null)
                {
                    return root;
                }

                var newRoot = new Folder
                {
                    UserId = userId,
                    Name = "root",
                    ParentFolderId = null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                await _dbSet.AddAsync(newRoot, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                return newRoot;
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(GetOrCreateRootFolderAsync), ex, $"userId: {userId}");
                throw;
            }
        }
    }
}
