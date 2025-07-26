using bank_accounts.Features.Abstract;
using System.Linq.Expressions;
using bank_accounts.Features;

namespace bank_accounts.Infrastructure.Repository
{
    public interface IRepository<TEntity> where TEntity : class, IEntity
    {
        Task CreateAsync(TEntity entity);
        Task<TEntity?> GetByIdAsync(Guid guid);
        Task<(IEnumerable<TEntity> data, int totalCount)> GetFilteredAsync<TFilter>(TFilter filter) where TFilter : Filter<TEntity>;
        Task UpdateAsync(TEntity entity);
        Task UpdatePartialAsync<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> propertyExpression);
        Task DeleteAsync(Guid guid);
        Task SaveChangesAsync();
    }
}
