using Microsoft.EntityFrameworkCore;
using ModelLibrary.Models;
using PersistenceLayer;

namespace DataAccessLayer.Accessors
{
    public class TransactionAccessor : BaseDataAccessor<Transaction>
    {
        public TransactionAccessor(WebStorageContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Transaction>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Transaction>> GetBySubscriptionIdAsync(int subscriptionId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(t => t.SubscriptionId == subscriptionId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<Transaction?> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(t => t.InvoiceNumber == invoiceNumber, cancellationToken);
        }
    }
}
