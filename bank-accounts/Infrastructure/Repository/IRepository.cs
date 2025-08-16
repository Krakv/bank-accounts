using bank_accounts.Features.Common;
using bank_accounts.Features.Outbox.Dto;

namespace bank_accounts.Infrastructure.Repository;

public interface IRepository<TEntity> where TEntity : class, IEntity
{
    Task CreateAsync(TEntity entity);
    Task<TEntity?> GetByIdAsync(Guid guid);
    Task<(List<TEntity> data, int totalCount)> GetFilteredAsync<TFilter>(TFilter filter) where TFilter : Filter<TEntity>;
    Task Update(TEntity entity);
    Task SaveChangesAsync();
    Task DeleteAsync(TEntity entity);
    Task<InterestAccruedDto?> AccrueInterestAsync(Guid accountId);
}