namespace bank_accounts.Features.Common;

public abstract class Filter<TEntity>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    public abstract IQueryable<TEntity> ApplyFilters(IQueryable<TEntity> query);
}