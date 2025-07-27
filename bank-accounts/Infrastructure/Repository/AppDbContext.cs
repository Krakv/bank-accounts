using bank_accounts.Features.Accounts.Entities;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace bank_accounts.Infrastructure.Repository;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts { get; set; }

    [UsedImplicitly]
    public async Task<IDbContextTransaction> BeginTransactionAsync()
        => await Database.BeginTransactionAsync();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseLazyLoadingProxies();
    }
}