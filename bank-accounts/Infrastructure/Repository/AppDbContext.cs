using bank_accounts.Features.Accounts.Entities;
using bank_accounts.Features.Transactions.Entities;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace bank_accounts.Infrastructure.Repository;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    [UsedImplicitly]
    public DbSet<Account> Accounts { get; set; }
    [UsedImplicitly]
    public DbSet<Transaction> Transactions { get; set; }

    [UsedImplicitly]
    public async Task<IDbContextTransaction> BeginTransactionAsync()
        => await Database.BeginTransactionAsync();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseLazyLoadingProxies();
    }
}