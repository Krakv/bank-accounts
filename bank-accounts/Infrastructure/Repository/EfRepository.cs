using bank_accounts.Features.Common;
using bank_accounts.Features.Outbox.Dto;
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

    public async Task<(List<TEntity> data, int totalCount)> GetFilteredAsync<TFilter>(TFilter filter) where TFilter : Filter<TEntity>
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

    public async Task Update(TEntity entity)
    {
        _dbSet.Update(entity);
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

    public async Task<InterestAccruedDto?> AccrueInterestAsync(Guid accountId)
    {
        return await context.Database.SqlQueryRaw<InterestAccruedDto>(
                """
                SELECT 
                  account_id AS "AccountId",
                  period_from AS "PeriodFrom",
                  period_to AS "PeriodTo",
                  amount AS "Amount"
                FROM accrue_interest({0})
                """,
                accountId)
            .FirstOrDefaultAsync();
    }
}