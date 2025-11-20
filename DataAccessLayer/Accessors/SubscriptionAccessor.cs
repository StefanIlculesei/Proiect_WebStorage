using Microsoft.EntityFrameworkCore;
using ModelLibrary.Models;
using PersistenceLayer;

namespace DataAccessLayer.Accessors
{
    public class SubscriptionAccessor : BaseDataAccessor<Subscription>
    {
        public SubscriptionAccessor(WebStorageContext context) : base(context)
        {
        }

        public async Task<Subscription?> GetActiveSubscriptionByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive, cancellationToken);
        }

        public async Task<IEnumerable<Subscription>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(s => s.Plan)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.StartDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Subscription>> GetExpiringSubscriptionsAsync(DateTime beforeDate, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(s => s.IsActive && s.EndDate.HasValue && s.EndDate.Value <= beforeDate)
                .ToListAsync(cancellationToken);
        }
    }
}
