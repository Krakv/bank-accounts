using bank_accounts.Features.Accounts.Dto;
using bank_accounts.Features.Common;
using bank_accounts.Features.Inbox;
using bank_accounts.Features.Inbox.Payloads;
using bank_accounts.Features.Transactions.Dto;
using bank_accounts.Infrastructure.Repository;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System.Net;
using System.Net.Http.Json;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using static bank_accounts_integration_tests.ParallelTransferTests;

namespace bank_accounts_integration_tests;

public class ClientBlockedPreventsDebitTest : IAsyncLifetime
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase("bankaccounts")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithPortBinding(5435, 5432)
        .Build();

    private readonly RabbitMqContainer _rmqContainer = new RabbitMqBuilder()
        .WithImage("rabbitmq:4.1.3-management")
        .WithUsername("guest")
        .WithPassword("guest")
        .WithPortBinding(5675, 5672)
        .WithPortBinding(15673, 15672)
        .WithResourceMapping("RabbitMq/definitions.json", "/etc/rabbitmq/definitions.json")
        .WithEnvironment("RABBITMQ_SERVER_ADDITIONAL_ERL_ARGS", "-rabbitmq_management load_definitions \"/etc/rabbitmq/definitions.json\"")
        .Build();

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _rmqContainer.StartAsync();

        _factory = new TestApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var dbContextDescriptor = services.SingleOrDefault(d =>
                        d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (dbContextDescriptor != null) services.Remove(dbContextDescriptor);

                    services.AddDbContext<AppDbContext>(options =>
                        options.UseNpgsql(_dbContainer.GetConnectionString()));

                    var rmqDescriptor = services.SingleOrDefault(d =>
                        d.ServiceType == typeof(ConnectionFactory));
                    if (rmqDescriptor != null) services.Remove(rmqDescriptor);

                    var factory = new ConnectionFactory
                    {
                        Uri = new Uri(_rmqContainer.GetConnectionString()),
                        AutomaticRecoveryEnabled = true,
                        NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
                    };

                    services.AddSingleton(factory);

                    services.AddSingleton(factory.CreateConnectionAsync().Result);

                    services.AddHangfire(config =>
                        config.UsePostgreSqlStorage(options =>
                        {
                            options.UseNpgsqlConnection(_dbContainer.GetConnectionString());
                        }));

                    services.AddHangfireServer();

                    services.AddAuthentication("Bearer")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Bearer", _ => { });
                });
            });

        _client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _dbContainer.StopAsync();
        await _rmqContainer.StopAsync();
    }

    [Fact]
    public async Task ClientBlockedPreventsDebit()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var accountId = await CreateAccount(clientId);
        await BlockClient(clientId);
        await Task.Delay(5000);

        // Act
        var statusCode = await MakeDebitTransaction(accountId);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, statusCode);
    }

    private async Task<Guid> CreateAccount(Guid clientId)
    {
        var dto = new CreateAccountDto
        {
            OwnerId = clientId,
            Type = "Checking",
            Currency = "USD"
        };
        var resp = await _client.PostAsJsonAsync("/accounts", dto);
        resp.EnsureSuccessStatusCode();
        var result = (await resp.Content.ReadFromJsonAsync<MbResult<Guid>>())!;

        var transactionDto = new CreateTransactionDto
        {
            AccountId = result.Value,
            CounterpartyAccountId = null,
            Type = "Credit",
            Currency = "USD",
            Value = 100
        };
        var transResp = await _client.PostAsJsonAsync("/transactions", transactionDto);
        transResp.EnsureSuccessStatusCode();

        return result.Value;
    }

    private async Task BlockClient(Guid clientId)
    {
        var dto = new ClientBlockingPayload()
        {
            EventId = Guid.NewGuid(),
            ClientId = clientId,
            OccuredAt = DateTime.UtcNow,
            Meta = new Meta
            {
                CausationId = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid(),
                Source = "account.events",
                Version = "v1"
            }
        };
        var resp = await _client.PostAsJsonAsync("/event/block-client", dto);
        resp.EnsureSuccessStatusCode();
    }

    private async Task<HttpStatusCode> MakeDebitTransaction(Guid accountId)
    {
        var transactionDto = new CreateTransactionDto
        {
            AccountId = accountId,
            CounterpartyAccountId = null,
            Type = "Debit",
            Currency = "USD",
            Value = 50
        };
        var transResp = await _client.PostAsJsonAsync("/transactions", transactionDto);

        return transResp.StatusCode;
    }

    public class TestApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
        }
    }
}