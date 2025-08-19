using bank_accounts.Features.Accounts.Dto;
using bank_accounts.Features.Common;
using bank_accounts.Features.Transactions.Dto;
using bank_accounts.Infrastructure.Repository;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace bank_accounts_integration_tests;

public class ParallelTransferTests : IAsyncLifetime
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase("bankaccounts")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithPortBinding(5433, 5432)
        .Build();

    private readonly RabbitMqContainer _rmqContainer = new RabbitMqBuilder()
        .WithImage("rabbitmq:4.1.3-management")
        .WithUsername("guest")
        .WithPassword("guest")
        .WithPortBinding(5673, 5672)
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
    public async Task Fifty_Parallel_Transfers_Should_Keep_Total_Balance()
    {
        // Arrange
        var acc1 = await CreateAccount(10000);
        var acc2 = await CreateAccount(10000);
        var totalBefore = await GetTotalBalance();

        // Act
        var tasks = Enumerable.Range(0, 50).Select(_ => Transfer(acc1, acc2, 10));
        var enumerable = tasks.ToList();
        await Task.WhenAll(enumerable);

        // Assert
        var totalAfter = await GetTotalBalance();
        Assert.Equal(totalBefore, totalAfter);
        Assert.All(enumerable, task =>
        {
            Assert.True(
                task.Result.StatusCode is HttpStatusCode.Conflict or HttpStatusCode.Created,
                $"Unexpected status code: {task.Result.StatusCode}"
            );
        });
    }

    private async Task<Guid> CreateAccount(decimal balance)
    {
        var dto = new CreateAccountDto
        {
            OwnerId = Guid.NewGuid(),
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
            Value = balance
        };
        var transResp = await _client.PostAsJsonAsync("/transactions", transactionDto);
        transResp.EnsureSuccessStatusCode();

        return result.Value;
    }

    private async Task<HttpResponseMessage> Transfer(Guid from, Guid to, decimal amount)
    {
        var dto = new CreateTransactionDto
        {
            AccountId = from,
            CounterpartyAccountId = to,
            Type = "Debit",
            Value = amount,
            Currency = "USD"
        };

        return await _client.PostAsJsonAsync("/transactions", dto);
    }

    private async Task<decimal> GetTotalBalance()
    {
        var resp = await _client.GetAsync("/accounts");
        resp.EnsureSuccessStatusCode();
        var result = (await resp.Content.ReadFromJsonAsync<MbResult<AccountsDto>>())!;

        if (result.Value is { Accounts: not null })
        {
            return result.Value.Accounts.Sum(a => a.Balance);
        }

        throw new Exception("Result has no data.");
    }

    public class TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var principal = new ClaimsPrincipal(new ClaimsIdentity("Bearer"));
            var ticket = new AuthenticationTicket(principal, "Bearer");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    public class TestApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
        }
    }
}
