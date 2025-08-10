using System.Data;
using System.Linq.Expressions;
using bank_accounts.Features.Common;
using Microsoft.EntityFrameworkCore;

namespace bank_accounts.Infrastructure.Repository;

public class EfRepository<TEntity>(AppDbContext context) : IRepository<TEntity> where TEntity : class, IEntity
{
    private readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();

    public async Task CreateAsync(TEntity entity)
    {
        await _dbSet.AddAsync(entity);
        await context.SaveChangesAsync();
    }

    public async Task<TEntity?> GetByIdAsync(Guid guid)
    {
        return await _dbSet.FindAsync(guid);
    }

    public async Task<(IEnumerable<TEntity> data, int totalCount)> GetFilteredAsync<TFilter>(TFilter filter) where TFilter : Filter<TEntity>
    {
        var query = _dbSet.AsQueryable();

        query = filter.ApplyFilters(query);

        var totalCount = await query.CountAsync();
        var data = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return (data, totalCount);
    }

    public async Task UpdatePartialAsync<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> propertyExpression)
    {
        var existingEntity = await context.FindAsync<TEntity>(entity.Id);
        if (existingEntity != null) context.Entry(existingEntity).State = EntityState.Detached;

        _dbSet.Attach(entity);
        context.Entry(entity).Property(propertyExpression).IsModified = true;
        await context.SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(TEntity entity)
    {
        _dbSet.Remove(entity);
        await context.SaveChangesAsync();
    }

    public async Task AccrueInterestAsync(Guid accountId)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        await context.Database.ExecuteSqlRawAsync(
            "CALL accrue_interest({0})", accountId
        );

        await transaction.CommitAsync();
    }
}