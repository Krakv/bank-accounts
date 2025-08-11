using bank_accounts.Features.Accounts.CreateAccount;
using bank_accounts.Features.Accounts.Dto;
using bank_accounts.Features.Accounts.GetAccount;
using bank_accounts.Features.Transactions;
using bank_accounts.Features.Transactions.CreateTransaction;
using bank_accounts.Features.Transactions.Dto;
using bank_accounts.Features.Transactions.GetTransaction;
using bank_accounts.Infrastructure.Repository;
using bank_accounts.PipelineBehaviors;
using bank_accounts.Services.CurrencyService;
using bank_accounts.Services.VerificationService;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace bank_accounts_xunit_test;

public class TransactionTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithDatabase("bankaccounts")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private IMediator _mediator = null!;
    private AppDbContext _dbContext = null!;

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        var connectionString = _dbContainer.GetConnectionString();
        var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        _dbContext = new AppDbContext(dbContextOptions);
        await _dbContext.Database.MigrateAsync();

        var services = new ServiceCollection();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateAccountCommand).Assembly));
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddValidatorsFromAssembly(typeof(Program).Assembly);
        services.AddSingleton(_dbContext);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddSingleton<IVerificationService, StubVerificationService>();
        services.AddSingleton<ICurrencyService, StubCurrencyService>();
        services.AddLogging();

        var serviceProvider = services.BuildServiceProvider();
        _mediator = serviceProvider.GetRequiredService<IMediator>();
    }

    public async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
    }

    [Fact]
    public async Task Single_Transaction_Should_Update_Balance_Correctly()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var account = await _mediator.Send(new CreateAccountCommand(
            new CreateAccountDto { OwnerId = ownerId, Type = "Deposit", Currency = "USD", InterestRate = 1 }
        ));
        var createTransactionDto = new CreateTransactionDto
        {
            AccountId = account,
            CounterpartyAccountId = null,
            Currency = "USD",
            Description = "Test transaction",
            Type = "Credit",
            Value = 100
        };

        // Act
        await _mediator.Send(new CreateTransactionCommand(createTransactionDto));

        // Assert
        var updatedAccount = await _mediator.Send(new GetAccountQuery(account));
        Assert.Equal(100, updatedAccount?.Balance);
    }

    [Fact]
    public async Task Transfer_Should_Update_Balances_Correctly()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var accountFrom = await _mediator.Send(new CreateAccountCommand(
            new CreateAccountDto { OwnerId = ownerId, Type = "Deposit", Currency = "USD", InterestRate = 1 }
        ));
        var accountTo = await _mediator.Send(new CreateAccountCommand(
            new CreateAccountDto { OwnerId = ownerId, Type = "Deposit", Currency = "USD", InterestRate = 1 }
        ));
        var createTransactionDto = new CreateTransactionDto
        {
            AccountId = accountFrom,
            CounterpartyAccountId = null,
            Currency = "USD",
            Description = "Test transaction",
            Type = "Credit",
            Value = 100
        };
        var createTransferDto = new CreateTransactionDto
        {
            AccountId = accountFrom,
            CounterpartyAccountId = accountTo,
            Currency = "USD",
            Description = "Test transfer",
            Type = "Debit",
            Value = 50
        };

        // Act
        await _mediator.Send(new CreateTransactionCommand(createTransactionDto));
        await _mediator.Send(new CreateTransactionCommand(createTransferDto));

        // Assert
        var updatedAccountFrom = await _mediator.Send(new GetAccountQuery(accountFrom));
        var updatedAccountTo = await _mediator.Send(new GetAccountQuery(accountTo));

        Assert.Equal(50, updatedAccountFrom?.Balance);
        Assert.Equal(50, updatedAccountTo?.Balance);
    }

    [Fact]
    public async Task Transfer_Should_Create_Transactions_Correctly()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var accountFrom = await _mediator.Send(new CreateAccountCommand(
            new CreateAccountDto { OwnerId = ownerId, Type = "Checking", Currency = "EUR", InterestRate = null }
        ));
        var accountTo = await _mediator.Send(new CreateAccountCommand(
            new CreateAccountDto { OwnerId = ownerId, Type = "Checking", Currency = "EUR", InterestRate = null }
        ));
        var createTransactionDto = new CreateTransactionDto
        {
            AccountId = accountFrom,
            CounterpartyAccountId = null,
            Currency = "EUR",
            Description = "Test transaction",
            Type = "Credit",
            Value = 100
        };
        var createTransferDto = new CreateTransactionDto
        {
            AccountId = accountFrom,
            CounterpartyAccountId = accountTo,
            Currency = "EUR",
            Description = "Test transfer",
            Type = "Debit",
            Value = 50
        };

        // Act
        var transactionGuids = await _mediator.Send(new CreateTransactionCommand(createTransactionDto));
        var transferGuids = await _mediator.Send(new CreateTransactionCommand(createTransferDto));

        // Assert
        Assert.NotNull(transactionGuids);
        Assert.NotNull(transferGuids);

        Assert.Single(transactionGuids);
        Assert.Equal(2, transferGuids.Length);

        var transaction = await _mediator.Send(new GetTransactionQuery(transactionGuids[0]));
        var transfer1 = await _mediator.Send(new GetTransactionQuery(transferGuids[0]));
        var transfer2 = await _mediator.Send(new GetTransactionQuery(transferGuids[1]));

        Assert.NotNull(transaction);
        Assert.NotNull(transfer1);
        Assert.NotNull(transfer2);

        Assert.Equal(100, transaction.Value);
        Assert.Equal(50, transfer1.Value);
        Assert.Equal(50, transfer2.Value);

        Assert.Equal("Credit", transaction.Type);
        Assert.Equal("Credit", transfer1.Type);
        Assert.Equal("Debit", transfer2.Type);

        Assert.Equal("EUR", transaction.Currency);
        Assert.Equal("EUR", transfer1.Currency);
        Assert.Equal("EUR", transfer2.Currency);

        Assert.Equal(transaction.AccountId, transfer2.AccountId);
        Assert.Equal(transfer2.AccountId, transfer1.CounterpartyAccountId);
        Assert.Equal(transfer2.CounterpartyAccountId, transfer1.AccountId);
    }
}