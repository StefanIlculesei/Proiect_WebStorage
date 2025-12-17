using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelLibrary.Models;
using PersistenceLayer;
using LoggingLayer;

namespace DataAccessLayer.Accessors
{
    public class SubscriptionAccessor : BaseDataAccessor<Subscription>
    {
        public SubscriptionAccessor(WebStorageContext context, ILogger<SubscriptionAccessor> logger) : base(context, logger)
        {
        }

        public async Task<Subscription?> GetActiveSubscriptionByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .Include(s => s.Plan)
                    .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(GetActiveSubscriptionByUserIdAsync), ex, $"userId: {userId}");
                throw;
            }
        }

        public async Task<IEnumerable<Subscription>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .Include(s => s.Plan)
                    .Where(s => s.UserId == userId)
                    .OrderByDescending(s => s.StartDate)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(GetByUserIdAsync), ex, $"userId: {userId}");
                throw;
            }
        }

        public async Task<IEnumerable<Subscription>> GetExpiringSubscriptionsAsync(DateTime beforeDate, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .Where(s => s.IsActive && s.EndDate.HasValue && s.EndDate.Value <= beforeDate)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(GetExpiringSubscriptionsAsync), ex, $"beforeDate: {beforeDate}");
                throw;
            }
        }

        public async Task<Dictionary<int, int>> GetActiveCountsByPlanIdsAsync(List<int> planIds, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .Where(s => s.IsActive && planIds.Contains(s.PlanId))
                    .GroupBy(s => s.PlanId)
                    .Select(g => new { PlanId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.PlanId, x => x.Count, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(GetActiveCountsByPlanIdsAsync), ex, $"planIds count: {planIds.Count}");
                throw;
            }
        }
    }
}
