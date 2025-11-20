using Microsoft.EntityFrameworkCore;
using ModelLibrary.Models;
using PersistenceLayer;

namespace DataAccessLayer.Accessors
{
    public class FolderAccessor : BaseDataAccessor<Folder>
    {
        public FolderAccessor(WebStorageContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Folder>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(f => f.UserId == userId && !f.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Folder>> GetRootFoldersAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(f => f.UserId == userId && f.ParentFolderId == null && !f.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Folder>> GetSubFoldersAsync(int parentFolderId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(f => f.ParentFolderId == parentFolderId && !f.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<Folder?> GetWithFilesAsync(int folderId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(f => f.Files.Where(file => !file.IsDeleted))
                .FirstOrDefaultAsync(f => f.Id == folderId, cancellationToken);
        }

        public async Task<Folder?> GetWithSubFoldersAsync(int folderId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(f => f.SubFolders.Where(sub => !sub.IsDeleted))
                .FirstOrDefaultAsync(f => f.Id == folderId, cancellationToken);
        }
    }
}
