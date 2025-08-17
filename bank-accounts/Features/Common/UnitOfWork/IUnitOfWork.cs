using bank_accounts.Features.Accounts.Entities;
using bank_accounts.Features.Inbox.Entities;
using bank_accounts.Features.Outbox.Entities;
using bank_accounts.Features.Transactions.Entities;
using bank_accounts.Infrastructure.Repository;

namespace bank_accounts.Features.Common.UnitOfWork;

public interface IUnitOfWork : IDisposable
{
    IRepository<Account> Accounts { get; }
    IRepository<Transaction> Transactions { get; }
    IRepository<OutboxMessage> OutboxMessages { get; }
    IRepository<InboxConsumedMessage> InboxConsumedMessages { get; }
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}