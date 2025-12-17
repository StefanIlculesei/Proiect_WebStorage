using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DataAccessLayer.Interfaces;
using PersistenceLayer;
using LoggingLayer;

namespace DataAccessLayer.Accessors
{
    /// <summary>
    /// Base data accessor implementation providing common data access operations
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public class BaseDataAccessor<T> : IDataAccessor<T> where T : class
    {
        protected readonly WebStorageContext _context;
        protected readonly DbSet<T> _dbSet;
        protected readonly ILogger<BaseDataAccessor<T>> _logger;

        public BaseDataAccessor(WebStorageContext context, ILogger<BaseDataAccessor<T>> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
            _logger = logger;
        }

        public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(GetByIdAsync), ex, $"id: {id}");
                throw;
            }
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet.ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(GetAllAsync), ex);
                throw;
            }
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(FindAsync), ex, "predicate search");
                throw;
            }
        }

        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(FirstOrDefaultAsync), ex, "predicate search");
                throw;
            }
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet.AnyAsync(predicate, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(AnyAsync), ex, "predicate check");
                throw;
            }
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
        {
            try
            {
                return predicate == null
                    ? await _dbSet.CountAsync(cancellationToken)
                    : await _dbSet.CountAsync(predicate, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(CountAsync), ex, "count operation");
                throw;
            }
        }

        public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            try
            {
                await _dbSet.AddAsync(entity, cancellationToken);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(AddAsync), ex, $"entity type: {typeof(T).Name}");
                throw;
            }
        }

        public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            try
            {
                await _dbSet.AddRangeAsync(entities, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(AddRangeAsync), ex, $"entity type: {typeof(T).Name}");
                throw;
            }
        }

        public virtual Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            try
            {
                _dbSet.Update(entity);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(UpdateAsync), ex, $"entity type: {typeof(T).Name}");
                throw;
            }
        }

        public virtual Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            try
            {
                _dbSet.Remove(entity);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(DeleteAsync), ex, $"entity type: {typeof(T).Name}");
                throw;
            }
        }

        public virtual Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            try
            {
                _dbSet.RemoveRange(entities);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(DeleteRangeAsync), ex, $"entity type: {typeof(T).Name}");
                throw;
            }
        }

        public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(SaveChangesAsync), ex);
                throw;
            }
        }
    }
}
