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

        public override async Task<IEnumerable<Plan>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet.Where(p => !p.IsDeleted).ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(GetAllAsync), ex);
                throw;
            }
        }

        public override async Task<Plan?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(GetByIdAsync), ex, $"id: {id}");
                throw;
            }
        }

        public async Task<Plan?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet.FirstOrDefaultAsync(p => p.Name == name && !p.IsDeleted, cancellationToken);
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
                    .FirstOrDefaultAsync(p => p.Id == planId && !p.IsDeleted, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(GetWithSubscriptionsAsync), ex, $"planId: {planId}");
                throw;
            }
        }

        public async Task<Plan?> SoftDeleteAsync(int planId, CancellationToken cancellationToken = default)
        {
            try
            {
                var plan = await _dbSet.FirstOrDefaultAsync(p => p.Id == planId && !p.IsDeleted, cancellationToken);
                if (plan == null) return null;

                plan.IsDeleted = true;
                plan.DeletedAt = DateTime.UtcNow;
                plan.UpdatedAt = DateTime.UtcNow;
                _dbSet.Update(plan);
                return plan;
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(SoftDeleteAsync), ex, $"planId: {planId}");
                throw;
            }
        }
    }
}
