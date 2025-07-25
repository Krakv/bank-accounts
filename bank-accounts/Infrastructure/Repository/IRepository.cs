using bank_accounts.Features.Abstract;

namespace bank_accounts.Infrastructure.Repository
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task CreateAsync(TEntity entity);
        Task<TEntity?> GetByIdAsync(Guid guid);
        Task<(IEnumerable<TEntity> data, int totalCount)> GetFilteredAsync<TFilter>(TFilter filter) where TFilter : Filter<TEntity>;
        Task UpdateAsync(TEntity entity);
        Task DeleteAsync(Guid guid);
        Task SaveChangesAsync();
    }
}
