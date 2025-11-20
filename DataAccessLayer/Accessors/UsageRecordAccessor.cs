using Microsoft.EntityFrameworkCore;
using ModelLibrary.Models;
using PersistenceLayer;

namespace DataAccessLayer.Accessors
{
    public class UsageRecordAccessor : BaseDataAccessor<UsageRecord>
    {
        public UsageRecordAccessor(WebStorageContext context) : base(context)
        {
        }

        public async Task<IEnumerable<UsageRecord>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(ur => ur.UserId == userId)
                .OrderByDescending(ur => ur.EventDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<UsageRecord>> GetByPeriodAsync(int userId, DateTime periodStart, DateTime periodEnd, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(ur => ur.UserId == userId && ur.PeriodStart >= periodStart && ur.PeriodEnd <= periodEnd)
                .ToListAsync(cancellationToken);
        }

        public async Task<long> GetTotalUsageAsync(int userId, DateTime periodStart, DateTime periodEnd, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(ur => ur.UserId == userId && ur.PeriodStart >= periodStart && ur.PeriodEnd <= periodEnd)
                .SumAsync(ur => ur.FileSize, cancellationToken);
        }
    }
}
