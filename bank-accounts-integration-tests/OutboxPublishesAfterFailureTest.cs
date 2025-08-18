using bank_accounts.Features.Outbox.Entities;
using bank_accounts.Infrastructure.Repository;
using bank_accounts.Services.OutboxDispatcherService;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using static bank_accounts_integration_tests.ParallelTransferTests;

namespace bank_accounts_integration_tests;

public class OutboxPublishesAfterFailureTests : IAsyncLifetime
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase("bankaccounts")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithPortBinding(5434, 5432)
        .Build();

    private readonly RabbitMqContainer _rmqContainer = new RabbitMqBuilder()
        .WithImage("rabbitmq:4.1.3-management")
        .WithUsername("guest")
        .WithPassword("guest")
        .WithPortBinding(5674, 5672)
        .WithPortBinding(15672, 15672)
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
    public async Task OutboxPublishesAfterFailure()
    {
        Guid messageId;

        // Arrange

        await _rmqContainer.StopAsync();

        using (var scope = _factory.Services.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IRepository<OutboxMessage>>();
            var msg = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = "AccountOpened",
                Payload = "{ \"AccountId\": 123 }",
                CorrelationId = Guid.NewGuid(),
                CausationId = Guid.NewGuid(),
                OccurredAt = DateTime.UtcNow,
                Source = "account.events"
            };
            messageId = msg.Id;
            await repo.CreateAsync(msg);
            await repo.SaveChangesAsync();
        }

        // Act

        try
        {
            using var scope = _factory.Services.CreateScope();
            var service =(IOutboxDispatcherService)scope.ServiceProvider.GetService(typeof(IOutboxDispatcherService))!;
            await service.PublishPendingMessages();
        }
        catch (Exception)
        {
            // Assert
            using var scope = _factory.Services.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IRepository<OutboxMessage>>();
            var msg = await repo.GetByIdAsync(messageId);
            Assert.Null(msg!.ProcessedAt);
        }

        // Arrange

        await _rmqContainer.StartAsync();

        // Act

        using (var scope = _factory.Services.CreateScope())
        {
            var service = (IOutboxDispatcherService)scope.ServiceProvider.GetService(typeof(IOutboxDispatcherService))!;
            await service.PublishPendingMessages();
        }

        // Assert
        using (var scope = _factory.Services.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IRepository<OutboxMessage>>();
            var msg = await repo.GetByIdAsync(messageId);
            Assert.NotNull(msg!.ProcessedAt);
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
