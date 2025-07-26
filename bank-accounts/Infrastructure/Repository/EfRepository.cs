using bank_accounts.Features;
using bank_accounts.Features.Abstract;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace bank_accounts.Infrastructure.Repository
{
    public class EfRepository<TEntity> : IRepository<TEntity> where TEntity : class, IEntity
    {
        private readonly AppDbContext _context;
        private readonly DbSet<TEntity> _dbSet;

        public EfRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        public async Task CreateAsync(TEntity entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<TEntity?> GetByIdAsync(Guid guid)
        {
            return await _dbSet.FindAsync(guid);
        }

        public async Task<(IEnumerable<TEntity> data, int totalCount)> GetFilteredAsync<TFilter>(TFilter filter) where TFilter : Filter<TEntity>
        {
            var query = _dbSet.AsQueryable();

            if (filter != null)
            {
                query = filter.ApplyFilters(query);
            }

            var totalCount = await query.CountAsync();
            var data = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return (data, totalCount);
        }

        public async Task UpdateAsync(TEntity entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePartialAsync<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> propertyExpression)
        {
            var existingEntity = _context.Find<TEntity>(entity.Id);
            if (existingEntity != null)
            {
                _context.Entry(existingEntity).State = EntityState.Detached;
            }

            _dbSet.Attach(entity);
            _context.Entry(entity).Property(propertyExpression).IsModified = true;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid guid)
        {
            var entity = await GetByIdAsync(guid);
            if (entity != null)
            {
                _dbSet.Remove(entity);
            }
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
