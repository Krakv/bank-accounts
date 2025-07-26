using bank_accounts.Features.Accounts.Entities;
using bank_accounts.Features.Transactions.Entities;
using bank_accounts.Infrastructure.Repository;

namespace bank_accounts.Features.Transactions
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Account> Accounts { get; }
        IRepository<Transaction> Transactions { get; }
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
