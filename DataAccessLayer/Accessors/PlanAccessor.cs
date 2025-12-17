using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelLibrary.Models;
using PersistenceLayer;
using LoggingLayer;

namespace DataAccessLayer.Accessors
{
    public class PlanAccessor : BaseDataAccessor<Plan>
    {
        public PlanAccessor(WebStorageContext context, ILogger<PlanAccessor> logger) : base(context, logger)
        {
        }

        public async Task<Plan?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet.FirstOrDefaultAsync(p => p.Name == name, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(GetByNameAsync), ex, $"name: {name}");
                throw;
            }
        }

        public async Task<Plan?> GetWithSubscriptionsAsync(int planId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .Include(p => p.Subscriptions)
                    .FirstOrDefaultAsync(p => p.Id == planId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(GetWithSubscriptionsAsync), ex, $"planId: {planId}");
                throw;
            }
        }
    }
}
