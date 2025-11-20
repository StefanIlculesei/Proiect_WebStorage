using Microsoft.EntityFrameworkCore;
using PersistenceLayer;
using ModelLibrary.Models;

namespace DataAccessLayer.Accessors
{
    public class FileAccessor : BaseDataAccessor<ModelLibrary.Models.File>
    {
        public FileAccessor(WebStorageContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ModelLibrary.Models.File>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(f => f.UserId == userId && !f.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<ModelLibrary.Models.File>> GetByFolderIdAsync(int folderId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(f => f.FolderId == folderId && !f.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<ModelLibrary.Models.File?> GetByStoragePathAsync(string storagePath, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(f => f.StoragePath == storagePath, cancellationToken);
        }

        public async Task<long> GetTotalSizeByUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(f => f.UserId == userId && !f.IsDeleted)
                .SumAsync(f => f.FileSize, cancellationToken);
        }

        public async Task<IEnumerable<ModelLibrary.Models.File>> SearchByNameAsync(int userId, string searchTerm, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(f => f.UserId == userId && !f.IsDeleted && f.FileName.Contains(searchTerm))
                .ToListAsync(cancellationToken);
        }
    }
}
