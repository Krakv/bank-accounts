using System.Text;
using System.Text.Json;
using bank_accounts.Features.Accounts.FrozeClientAccounts;
using bank_accounts.Features.Accounts.UnfrozeClientAccounts;
using bank_accounts.Features.Inbox.Entities;
using bank_accounts.Features.Inbox.Payloads;
using bank_accounts.Infrastructure.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace bank_accounts.Services.InboxDispatcherService;

public class InboxDispatcherService(ILogger<InboxDispatcherService> logger, IConnection rabbitMqConnection, IServiceScopeFactory scopeFactory) : IInboxDispatcherService
{
    public async Task ConsumeMessages()
    {
        var channel = await rabbitMqConnection.CreateChannelAsync();
        await channel.QueueDeclareAsync("account.antifraud", exclusive: false, durable: true, autoDelete: false, arguments: null);
        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            logger.LogInformation("GOTCHA!!!");
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                await HandleMessageSafeAsync(message, ea.RoutingKey);
                await channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Message processing failed");
            }
        };
        logger.LogInformation("STARTED_TO_CONSUME!!!");
        await channel.BasicConsumeAsync("account.antifraud", autoAck: false, consumer);
    }

    private async Task HandleMessageSafeAsync(string message, string routingKey)
    {
        using var scope = scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        try
        {
            var payload = JsonSerializer.Deserialize<ClientBlockingPayload>(message)
                ?? throw new JsonException("Payload is null");

            switch (routingKey)
            {
                case "client.blocked":
                    await mediator.Send(new FrozeClientAccountsCommand(payload.ClientId, payload));
                    logger.LogInformation("Client {ClientId} was frozen.", payload.ClientId);
                    break;
                case "client.unblocked":
                    await mediator.Send(new UnfrozeClientAccountsCommand(payload.ClientId, payload));
                    logger.LogInformation("Client {ClientId} was unfrozen.", payload.ClientId);
                    break;
                default:
                    throw new NotSupportedException($"Unknown routing key: {routingKey}");
            }
        }
        catch (JsonException ex)
        {
            await QuarantineMessageAsync(message, $"Invalid JSON: {ex.Message}");
            throw;
        }
        catch (DbUpdateException)
        {
            await QuarantineMessageAsync(message, "Message already processed");
            logger.LogWarning("Message with id={EventId} already processed ", JsonSerializer.Deserialize<ClientBlockingPayload>(message)!.EventId);
        }
    }

    private async Task QuarantineMessageAsync(string payload, string error)
    {
        using var scope = scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository<InboxDeadMessage>>();

        await repo.CreateAsync(new InboxDeadMessage
        {
            Id = Guid.NewGuid(),
            Payload = payload,
            Handler = nameof(QuarantineMessageAsync),
            Error = error,
            ReceivedAt = DateTime.UtcNow
        });
    }
}