using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelLibrary.Models;
using PersistenceLayer;
using LoggingLayer;

namespace DataAccessLayer.Accessors
{
    public class UserAccessor : BaseDataAccessor<User>
    {
        public UserAccessor(WebStorageContext context, ILogger<UserAccessor> logger) : base(context, logger)
        {
        }

        public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet.FirstOrDefaultAsync(u => u.UserName == username && !u.IsDeleted, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(GetByUsernameAsync), ex, $"username: {username}");
                throw;
            }
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet.FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(GetByEmailAsync), ex, $"email: {email}");
                throw;
            }
        }

        public async Task<User?> GetWithSubscriptionsAsync(int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .Include(u => u.Subscriptions)
                        .ThenInclude(s => s.Plan)
                    .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(GetWithSubscriptionsAsync), ex, $"userId: {userId}");
                throw;
            }
        }

        public async Task<long> GetStorageUsedAsync(int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _dbSet.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);
                return user?.StorageUsed ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(GetStorageUsedAsync), ex, $"userId: {userId}");
                throw;
            }
        }
    }
}
