using bank_accounts.Features.Accounts.Models;
using bank_accounts.Features.Transactions.Models;
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
