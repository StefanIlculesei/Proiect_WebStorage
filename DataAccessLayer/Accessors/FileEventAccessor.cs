using Microsoft.EntityFrameworkCore;
using ModelLibrary.Models;
using PersistenceLayer;

namespace DataAccessLayer.Accessors
{
    public class FileEventAccessor : BaseDataAccessor<FileEvent>
    {
        public FileEventAccessor(WebStorageContext context) : base(context)
        {
        }

        public async Task<IEnumerable<FileEvent>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(fe => fe.UserId == userId)
                .OrderByDescending(fe => fe.EventDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<FileEvent>> GetByFileIdAsync(int fileId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(fe => fe.FileId == fileId)
                .OrderByDescending(fe => fe.EventDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<FileEvent>> GetRecentEventsAsync(int userId, int count, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(fe => fe.UserId == userId)
                .OrderByDescending(fe => fe.EventDate)
                .Take(count)
                .ToListAsync(cancellationToken);
        }
    }
}
