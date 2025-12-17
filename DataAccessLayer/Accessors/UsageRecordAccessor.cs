using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelLibrary.Models;
using PersistenceLayer;
using LoggingLayer;

namespace DataAccessLayer.Accessors
{
    public class UsageRecordAccessor : BaseDataAccessor<UsageRecord>
    {
        public UsageRecordAccessor(WebStorageContext context, ILogger<UsageRecordAccessor> logger) : base(context, logger)
        {
        }

        public async Task<IEnumerable<UsageRecord>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .Where(ur => ur.UserId == userId)
                    .OrderByDescending(ur => ur.EventDate)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(GetByUserIdAsync), ex, $"userId: {userId}");
                throw;
            }
        }

        public async Task<IEnumerable<UsageRecord>> GetByPeriodAsync(int userId, DateTime periodStart, DateTime periodEnd, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .Where(ur => ur.UserId == userId && ur.PeriodStart >= periodStart && ur.PeriodEnd <= periodEnd)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(GetByPeriodAsync), ex, $"userId: {userId}, period: {periodStart} to {periodEnd}");
                throw;
            }
        }

        public async Task<long> GetTotalUsageAsync(int userId, DateTime periodStart, DateTime periodEnd, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .Where(ur => ur.UserId == userId && ur.PeriodStart >= periodStart && ur.PeriodEnd <= periodEnd)
                    .SumAsync(ur => ur.FileSize, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(GetTotalUsageAsync), ex, $"userId: {userId}, period: {periodStart} to {periodEnd}");
                throw;
            }
        }
    }
}
