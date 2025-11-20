using Microsoft.EntityFrameworkCore;
using ModelLibrary.Models;
using PersistenceLayer;

namespace DataAccessLayer.Accessors
{
    public class PlanAccessor : BaseDataAccessor<Plan>
    {
        public PlanAccessor(WebStorageContext context) : base(context)
        {
        }

        public async Task<Plan?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.Name == name, cancellationToken);
        }

        public async Task<Plan?> GetWithSubscriptionsAsync(int planId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(p => p.Subscriptions)
                .FirstOrDefaultAsync(p => p.Id == planId, cancellationToken);
        }
    }
}
