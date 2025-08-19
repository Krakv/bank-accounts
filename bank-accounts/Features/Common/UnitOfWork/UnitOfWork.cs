using bank_accounts.Features.Accounts.Entities;
using bank_accounts.Features.Inbox.Entities;
using bank_accounts.Features.Outbox.Entities;
using bank_accounts.Features.Transactions.Entities;
using bank_accounts.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore.Storage;

namespace bank_accounts.Features.Common.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        OutboxMessages = new EfRepository<OutboxMessage>(_context);
        Accounts = new EfRepository<Account>(_context);
        Transactions = new EfRepository<Transaction>(_context);
        InboxConsumedMessages = new EfRepository<InboxConsumedMessage>(_context);
    }

    public IRepository<Account> Accounts { get; }
    public IRepository<Transaction> Transactions { get; }
    public IRepository<OutboxMessage> OutboxMessages { get; }
    public IRepository<InboxConsumedMessage> InboxConsumedMessages { get; }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        try
        {
            if (_transaction != null)
            {
                await _context.SaveChangesAsync();
                await _transaction.CommitAsync();
            }
        }
        finally
        {
            if (_transaction != null)
                await _transaction.DisposeAsync();
        }
    }

    public async Task RollbackAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _context.ChangeTracker.Clear();

        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}