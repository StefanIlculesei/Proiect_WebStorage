using Microsoft.EntityFrameworkCore;
using ModelLibrary.Models;
using PersistenceLayer;

namespace DataAccessLayer.Accessors
{
    public class UserAccessor : BaseDataAccessor<User>
    {
        public UserAccessor(WebStorageContext context) : base(context)
        {
        }

        public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.UserName == username && !u.IsDeleted, cancellationToken);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted, cancellationToken);
        }

        public async Task<User?> GetWithSubscriptionsAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(u => u.Subscriptions)
                    .ThenInclude(s => s.Plan)
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);
        }

        public async Task<long> GetStorageUsedAsync(int userId, CancellationToken cancellationToken = default)
        {
            var user = await _dbSet.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);
            return user?.StorageUsed ?? 0;
        }
    }
}
