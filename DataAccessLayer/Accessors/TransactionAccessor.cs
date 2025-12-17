using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelLibrary.Models;
using PersistenceLayer;
using LoggingLayer;

namespace DataAccessLayer.Accessors
{
    public class TransactionAccessor : BaseDataAccessor<Transaction>
    {
        public TransactionAccessor(WebStorageContext context, ILogger<TransactionAccessor> logger) : base(context, logger)
        {
        }

        public async Task<IEnumerable<Transaction>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .Where(t => t.UserId == userId)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(GetByUserIdAsync), ex, $"userId: {userId}");
                throw;
            }
        }

        public async Task<IEnumerable<Transaction>> GetBySubscriptionIdAsync(int subscriptionId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .Where(t => t.SubscriptionId == subscriptionId)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(GetBySubscriptionIdAsync), ex, $"subscriptionId: {subscriptionId}");
                throw;
            }
        }

        public async Task<Transaction?> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet.FirstOrDefaultAsync(t => t.InvoiceNumber == invoiceNumber, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(GetByInvoiceNumberAsync), ex, $"invoiceNumber: {invoiceNumber}");
                throw;
            }
        }
    }
}
